using System;
using XTransport.Server.Storage;

namespace XTransport.Server
{
	public interface IServerXRef
	{
		Guid Value { get; set; }
	}

	internal sealed class ServerXRef<TKind> : IServerXValue, IServerXRef
	{
		private readonly IXObjectFactory<TKind> m_factory;

		internal ServerXRef(IXObjectFactory<TKind> _factory)
		{
			m_factory = _factory;
		}

		internal ServerXRef()
		{
		}

		#region IServerXRef Members

		public Guid Value { get; set; }

		#endregion

		#region IServerXValue Members

		public int SaveOriginalValue(Guid _uid, AbstractXReportItem _reportItem, IStorage _storage,
		                                       int? _lastId, DateTime _now, AbstractXServer _abstractXServer)
		{
			var item = (XReportItem<Guid>) _reportItem;
			Value = item.Value;
			return _storage.InsertValue(_uid, _reportItem.FieldId.GetHashCode(), item.Value, _lastId, _now);
		}

		public AbstractXReportItem CreateReverseReportItem(AbstractXReportItem _item)
		{
			return new XReportItem<Guid>(_item.FieldId, Value, XReportItemState.CHANGE);
		}

		public void SaveValue(Guid _uid, int _fieldId, IStorage _storage, DateTime _now)
		{
			_storage.InsertValue(_uid, _fieldId, Value, null, _now);
		}

		public AbstractXReportItem GetOriginalValueReportItem(int _fieldId, SessionId _sessionId)
		{
			return new XReportItem<Guid>(_fieldId, Value, XReportItemState.ORIGINAL);
		}

		public void SetValue(IStorageValue _val)
		{
			Value = ((StorageValue<Guid>) _val).Val;
		}

		#endregion
	}
}