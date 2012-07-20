using System;
using System.Collections.Generic;
using System.Linq;
using XTransport.Server.Storage;

namespace XTransport.Server
{
	internal class ServerXObjectContainer
	{
		private readonly Dictionary<SessionId, List<XReport>> m_changes = new Dictionary<SessionId, List<XReport>>();

		private readonly Dictionary<SessionId, int> m_currentVersion = new Dictionary<SessionId, int>();
		private readonly int m_kind;
		private readonly Dictionary<int, int> m_xValueOldIds = new Dictionary<int, int>();
		private readonly Dictionary<int, IServerXValue> m_xValues = new Dictionary<int, IServerXValue>();

		public ServerXObjectContainer(int _kind, Guid _uid, DateTime _stored)
		{
			m_kind = _kind;
			Uid = _uid;
			Stored = _stored;
		}

		public DateTime Stored { get; internal set; }
		public int StoredId { get; set; }

		public Guid Uid { get; internal set; }

		internal Dictionary<int, IServerXValue> XValues
		{
			get { return m_xValues; }
		}

		public int Kind
		{
			get { return m_kind; }
		}

		internal void AddChildren(int _field, Guid _uid)
		{
			IServerXValue value;
			ServerXList list;
			if (!m_xValues.TryGetValue(_field, out value))
			{
				list = new ServerXList();
				m_xValues.Add(_field, list);
			}
			else
			{
				list = (ServerXList) value;
			}
			list.AddChildUid(_uid);
		}

		internal void SetValue(int _field, IStorageValueInternal _val)
		{
			IServerXValue value;
			if (!m_xValues.TryGetValue(_field, out value))
			{
				value = _val.CreateIServerXValue();
				m_xValues.Add(_field, value);
			}
			else
			{
				value.SetValue(_val);
			}
			m_xValueOldIds.Add(_field, _val.OldId);
		}

		internal void SetUid(Guid _uid)
		{
			Uid = _uid;
		}

		internal bool Save(SessionId _sessionId, IStorage _storage, DateTime _now, AbstractXServer _abstractXServer)
		{
			List<XReport> reports;
			if (m_changes.TryGetValue(_sessionId, out reports) && m_currentVersion[_sessionId] >= 0)
			{
				var reverseItems = new List<AbstractXReportItem>();

				var changes = reports[m_currentVersion[_sessionId]];
				foreach (var item in changes.Items)
				{
					IServerXValue xValue;
					if (!m_xValues.TryGetValue(item.FieldId, out xValue))
					{
						xValue = item.CreateXValue();
						m_xValues.Add(item.FieldId, xValue);
						reverseItems.Add(item.CreateDefaultReportItem());
					}
					else
					{
						reverseItems.Add(xValue.CreateReverseReportItem(item));
					}

					int oldId;
					if (m_xValueOldIds.TryGetValue(item.FieldId, out oldId))
					{
						m_xValueOldIds[item.FieldId] = xValue.SaveOriginalValue(Uid, item, _storage, oldId, _now, _abstractXServer);
					}
					else
					{
						m_xValueOldIds[item.FieldId] = xValue.SaveOriginalValue(Uid, item, _storage, null, _now, _abstractXServer);
					}
				}
				reports.Clear();
				m_currentVersion[_sessionId] = -1;

				var reverseReport = new XReport(changes.Uid, reverseItems, _now, changes.Kind);
				foreach (var pair in m_changes)
				{
					if(pair.Key==_sessionId)
					{
						continue;
					}
					foreach (var xReport in pair.Value)
					{
						xReport.UpdateAccordingNewValues(m_xValues, reverseReport);
					}
					pair.Value.Insert(0, reverseReport);
					m_currentVersion[pair.Key]++;
				}
				return true;
			}
			return false;
		}

		internal virtual bool SaveChildren(AbstractXServer _server, SessionId _sessionId, IStorage _storage, DateTime _now)
		{
			var saved = false;
			foreach (var pair in m_xValues)
			{
				var list = pair.Value as ServerXList;
				if (list != null)
				{
					foreach (var childUid in list.GetGuids())
					{
						saved = _server.SaveChild(childUid, Uid, _sessionId, _storage, pair.Key, _now) | saved;
					}
				}
			}
			return saved;
		}

		internal void Revert(SessionId _sessionId)
		{
			List<XReport> reports;
			if (!m_changes.TryGetValue(_sessionId, out reports) || !reports.Any()) return;
			reports.Clear();
			m_currentVersion[_sessionId] = -1;
		}

		internal void FillFromClient(XReport _xReport, SessionId _sessionId)
		{
			Uid = _xReport.Uid;
			AddChanges(_sessionId, _xReport);
		}

		internal UndoXReport Undo(SessionId _sessionId)
		{
			if (m_currentVersion.ContainsKey(_sessionId))
			{
				var version = --m_currentVersion[_sessionId];
				if (version > -1)
				{
					var reports = m_changes[_sessionId];
					var actual = reports[version];
					var last = reports[reports.Count - 1];
					return new UndoXReport(Uid, actual.Items, actual.ActualFrom, last.ActualFrom, Stored, Kind);
				}
			}
			return new UndoXReport(Uid, Stored, Kind);
		}

		internal void GetAvailableUndoDate(SessionId _sessionId, AbstractXServer _server,
		                                   ref DateTime _dateTime, ref List<Guid> _candidates)
		{
			XReport changes = null;
			var result = Stored;
			int version;
			if (m_currentVersion.TryGetValue(_sessionId, out version))
			{
				if (version >= 0)
				{
					changes = m_changes[_sessionId][version];
					result = changes.ActualFrom;
				}
				if (result > _dateTime)
				{
					_candidates.Clear();
					_dateTime = result;
				}
				if (result == _dateTime)
				{
					_candidates.Add(Uid);
				}
			}

			var uids = new List<Guid>();
			if (changes != null)
			{
				uids.AddRange(changes.Items.OfType<XReportList>().SelectMany(_list => _list.Items).Where(_item => _item.State == EReportListItemState.ADDED).Select(_item => _item.Uid));
			}

			foreach (var pair in m_xValues)
			{
				var list = pair.Value as ServerXList;
				if (list == null) continue;
				uids.AddRange(list.GetGuids());
				if (changes != null)
				{
					var pairCopy = pair;
					var update = (XReportList) changes.Items.FirstOrDefault(_item => _item.FieldId == pairCopy.Key);
					if (update != null)
					{
						foreach (var item in update.Items)
						{
							switch (item.State)
							{
								case EReportListItemState.REMOVED:
									uids.Remove(item.Uid);
									break;
							}
						}
					}
				}
			}
			foreach (var uid in uids)
			{
				_server.GetAvailableUndoDate(uid, _sessionId, _server, ref _dateTime, ref _candidates);
			}
		}

		internal ServerXReport Redo(SessionId _sessionId)
		{
			List<XReport> reports;
			if (!m_changes.TryGetValue(_sessionId, out reports)) return null;
			var version = ++m_currentVersion[_sessionId];
			var actual = reports[version];
			var last = reports[reports.Count - 1];
			return new ServerXReport(Uid, actual.Items, actual.ActualFrom, last.ActualFrom, Stored, Kind);
		}

		internal void GetAvailableRedoDate(SessionId _sessionId, AbstractXServer _server,
		                                   ref DateTime _dateTime, ref List<Guid> _candidates)
		{
			XReport changes = null;
			int version;
			if (m_currentVersion.TryGetValue(_sessionId, out version))
			{
				if (version < m_changes[_sessionId].Count)
				{
					if (version < m_changes[_sessionId].Count - 1)
					{
						changes = m_changes[_sessionId][version + 1];
						var result = changes.ActualFrom;
						if (result < _dateTime)
						{
							_candidates.Clear();
							_dateTime = result;
						}
						if (result == _dateTime)
						{
							_candidates.Add(Uid);
						}
					}
					else if (version >= 0)
					{
						changes = m_changes[_sessionId][version];
					}
				}
			}
			var uids = new List<Guid>();
			if (changes != null)
			{
				uids.AddRange(changes.Items.OfType<XReportList>().SelectMany(_list => _list.Items).Where(_item => _item.State == EReportListItemState.ADDED).Select(_item => _item.Uid));
			}
			foreach (var pair in m_xValues)
			{
				var list = pair.Value as ServerXList;
				if (list == null) continue;
				uids.AddRange(list.GetGuids());
			}
			foreach (var uid in uids)
			{
				_server.GetAvailableRedoDate(uid, _sessionId, _server, ref _dateTime, ref _candidates);
			}
		}

		internal void AddChanges(SessionId _sessionId, XReport _report)
		{
			List<XReport> list;
			if (!m_changes.TryGetValue(_sessionId, out list))
			{
				list = new List<XReport>();
				m_changes.Add(_sessionId, list);
				m_currentVersion.Add(_sessionId, 0);
			}
			else
			{
				var version = ++m_currentVersion[_sessionId];
				if (version < list.Count)
				{
					list.RemoveRange(version, list.Count - version);
				}
			}
			list.Add(_report);
		}

		internal ServerXReport GetReport(SessionId _sessionId)
		{
			var actualFrom = Stored;
			var lastModification = Stored;
			List<XReport> reports;

			var items = new List<AbstractXReportItem>();
			foreach (var pair in m_xValues)
			{
				items.Add(pair.Value.GetOriginalValueReportItem(pair.Key, _sessionId));
			}

			if (m_changes.TryGetValue(_sessionId, out reports))
			{
				if (m_currentVersion[_sessionId] >= 0)
				{
					lastModification = reports[reports.Count - 1].ActualFrom;
					var changes = reports[m_currentVersion[_sessionId]];
					items.AddRange(changes.Items);
					actualFrom = changes.ActualFrom;
				}
			}
			else
			{
				m_changes.Add(_sessionId, new List<XReport>());
				m_currentVersion.Add(_sessionId, -1);
			}

			var result = new ServerXReport(Uid, items, actualFrom, lastModification, Stored, Kind);
			return result;
		}

		internal ServerXReport GetReport(SessionId _sessionId, DateTime _actualFor)
		{
			var actualFrom = Stored;
			var lastModification = Stored;
			var items = new List<AbstractXReportItem>();
			List<XReport> reports;
			XReport last = null;
			if (m_changes.TryGetValue(_sessionId, out reports))
			{
				last = reports.LastOrDefault(_report => _report.ActualFrom <= _actualFor);
				if (last != null)
				{
					actualFrom = last.ActualFrom;
				}
			}
			else
			{
				m_changes.Add(_sessionId, new List<XReport>());
				m_currentVersion.Add(_sessionId, -1);
			}

			foreach (var pair in m_xValues)
			{
				var xName = pair.Key;
				items.Add(pair.Value.GetOriginalValueReportItem(xName, _sessionId));

				if (last != null)
				{
					items.AddRange(last.Items.Where(_item => _item.FieldId == xName));
				}
			}

			var result = new ServerXReport(Uid, items, actualFrom, lastModification, Stored, Kind);
			return result;
		}
	}
}