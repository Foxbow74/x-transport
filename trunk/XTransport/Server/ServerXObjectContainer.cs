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

		public ServerXObjectContainer(int _kind, Guid _uid, DateTime _validFrom)
		{
			m_kind = _kind;
			Uid = _uid;
			Stored = 1;
			ValidFrom = _validFrom;
		}

		public ServerXObjectContainer(int _kind, Guid _uid)
		{
			m_kind = _kind;
			Uid = _uid;
			Stored = 0;
			ValidFrom = default(DateTime);
		}

		public DateTime ValidFrom { get; set; }
		public uint Stored { get; set; }

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
				//Stored = changes.ActualFrom;
				ValidFrom = _now;

				foreach (var pair in m_changes)
				{
					if (pair.Key == _sessionId)
					{
						continue;
					}
					var reverseReport = new XReport(changes.Uid, reverseItems, changes.Kind, EState.UNDO_ABLE | EState.REVERT_ABLE | (pair.Value.Any()?EState.REDO_ABLE : EState.SINGLE)) { ActualFrom = changes.ActualFrom };
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
					return new UndoXReport(Uid, actual.Items, actual.ActualFrom, Kind, EState.UNDO_ABLE|EState.REVERT_ABLE|(last!=actual?EState.REDO_ABLE : EState.SINGLE));
				}
			}
			return new UndoXReport(Uid, Stored, Kind);
		}

		internal void GetAvailableUndoDate(SessionId _sessionId, AbstractXServer _server,
		                                   ref uint _dateTime, ref List<Guid> _candidates)
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
				uids.AddRange(
					changes.Items.OfType<XReportList>().SelectMany(_list => _list.Items).Where(
						_item => _item.State == EReportListItemState.ADDED).Select(_item => _item.Uid));
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
			return new ServerXReport(Uid, actual.Items, actual.ActualFrom, Kind, EState.UNDO_ABLE | EState.REVERT_ABLE | (last!=actual ? EState.REDO_ABLE : EState.SINGLE));
		}

		internal void GetAvailableRedoDate(SessionId _sessionId, AbstractXServer _server,
		                                   ref uint _generation, ref List<Guid> _candidates)
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
						if (result < _generation)
						{
							_candidates.Clear();
							_generation = result;
						}
						if (result == _generation)
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
				uids.AddRange(
					changes.Items.OfType<XReportList>().SelectMany(_list => _list.Items).Where(
						_item => _item.State == EReportListItemState.ADDED).Select(_item => _item.Uid));
			}
			foreach (var pair in m_xValues)
			{
				var list = pair.Value as ServerXList;
				if (list == null) continue;
				uids.AddRange(list.GetGuids());
			}
			foreach (var uid in uids)
			{
				_server.GetAvailableRedoDate(uid, _sessionId, _server, ref _generation, ref _candidates);
			}
		}

		internal void FillFromClient(XReport _xReport, SessionId _sessionId)
		{
			Uid = _xReport.Uid;
			AddChanges(_sessionId, _xReport);
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
			var state = EState.SINGLE;

			var items = new List<AbstractXReportItem>();
			foreach (var pair in m_xValues)
			{
				items.Add(pair.Value.GetOriginalValueReportItem(pair.Key, _sessionId));
			}

			if (m_changes.TryGetValue(_sessionId, out reports))
			{
				if (m_currentVersion[_sessionId] >= 0)
				{
					var last = reports[reports.Count - 1];
					var actual = reports[m_currentVersion[_sessionId]];

					state |= EState.UNDO_ABLE|EState.REVERT_ABLE;
					if (last != actual) state |= EState.REDO_ABLE;


					lastModification = last.ActualFrom;
					var changes = actual;
					items.AddRange(changes.Items);
					actualFrom = changes.ActualFrom;
				}
			}
			else
			{
				m_changes.Add(_sessionId, new List<XReport>());
				m_currentVersion.Add(_sessionId, -1);
			}

			var result = new ServerXReport(Uid, items, actualFrom, Kind, state);
			return result;
		}

		internal ServerXReport GetReport(SessionId _sessionId, uint _actualFor)
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

			var result = new ServerXReport(Uid, items, actualFrom, Kind, EState.SINGLE);
			return result;
		}

		public uint GetCurrentGeneration(SessionId _sessionId)
		{
			int current;
			if(m_currentVersion.TryGetValue(_sessionId, out current))
			{
				return m_changes[_sessionId][current].ActualFrom;
			}
			return Stored;
		}
	}
}