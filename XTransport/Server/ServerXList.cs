using System;
using System.Collections.Generic;
using System.Linq;
using XTransport.Server.Storage;

namespace XTransport.Server
{
	internal sealed class ServerXList : IServerXValue
	{
		private readonly List<Guid> m_list = new List<Guid>();

		internal ServerXList()
		{
		}

		public int Field { get; set; }

		#region IServerXValue Members

		public int SaveOriginalValue(Guid _uid, AbstractXReportItem _reportItem, IStorage _storage, int? _lastId, DateTime _now, AbstractXServer _abstractXServer)
		{
			var items = ((XReportList) _reportItem).Items;
			foreach (var reportListItem in items)
			{
				switch (reportListItem.State)
				{
					case EReportListItemState.ADDED:
						m_list.Add(reportListItem.Uid);
						break;
					case EReportListItemState.REMOVED:
						m_list.Remove(reportListItem.Uid);
						_abstractXServer.Delete(_storage, reportListItem.Uid, _reportItem.FieldId.GetHashCode(), _now);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			return -1;
		}

		public AbstractXReportItem CreateReverseReportItem(AbstractXReportItem _reportItem)
		{
			var items = ((XReportList) _reportItem).Items;
			var resultItems = new List<XReportListItem>();
			foreach (var item in items)
			{
				resultItems.Add(new XReportListItem(item.Uid, item.State==EReportListItemState.ADDED?EReportListItemState.REMOVED : EReportListItemState.ADDED));
			}
			var result = new XReportList(_reportItem.FieldId, XReportItemState.CHANGE, resultItems);
			return result;
		}

		public void SaveValue(Guid _uid, int _fieldId, IStorage _storage, DateTime _now)
		{
		}

		public AbstractXReportItem GetOriginalValueReportItem(int _fieldId, SessionId _sessionId)
		{
			return new XReportList(_fieldId, XReportItemState.ORIGINAL,
			                       m_list.Select(_item => new XReportListItem(_item, EReportListItemState.ORIGINAL)));
		}

		public void SetValue(IStorageValue _val)
		{
			throw new NotImplementedException();
		}

		#endregion

		//public void SaveChildren(ServerXObjectContainer _owner, IXServerInternal _server, SessionId _sessionId,
		//                         Storage _storage, int _field, DateTime _now)
		//{
		//    foreach (var guid in m_list)
		//    {
		//        _server.SaveChild(guid, _owner, _sessionId, _storage, _field, _now);
		//    }
		//}

		public IEnumerable<Guid> GetGuids()
		{
			return m_list;
		}

		public void AddChildUid(Guid _uid)
		{
			m_list.Add(_uid);
		}

		public void AddRange(IEnumerable<Guid> _uids)
		{
			m_list.AddRange(_uids);
		}

		public void Add(Guid _uid)
		{
			m_list.Add(_uid);
		}
	}
}