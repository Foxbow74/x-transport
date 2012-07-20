using System;
using XTransport.Server.Storage;

namespace XTransport.Server
{
	internal sealed class ServerXValue<T> : IServerXValue, IXValue<T>
	{
		internal ServerXValue()
		{
		}

		#region IServerXValue Members

		public int SaveOriginalValue(Guid _uid, AbstractXReportItem _reportItem, IStorage _storage,
		                                       int? _lastId, DateTime _now, AbstractXServer _abstractXServer)
		{
			var item = (XReportItem<T>) _reportItem;
			Value = item.Value;
			return _storage.InsertValue(_uid, _reportItem.FieldId.GetHashCode(), item.Value, _lastId, _now);
		}

		public AbstractXReportItem CreateReverseReportItem(AbstractXReportItem _item)
		{
			return new XReportItem<T>(_item.FieldId, Value, XReportItemState.CHANGE);
		}

		public void SaveValue(Guid _uid, int _fieldId, IStorage _storage, DateTime _now)
		{
			_storage.InsertValue(_uid, _fieldId, Value, null, _now);
		}

		public AbstractXReportItem GetOriginalValueReportItem(int _fieldId, SessionId _sessionId)
		{
			return new XReportItem<T>(_fieldId, Value, XReportItemState.ORIGINAL);
		}

		public void SetValue(IStorageValue _val)
		{
			Value = ((StorageValue<T>) _val).Val;
		}

		#endregion

		#region IXValue<T> Members

		public T Value { get; set; }

		#endregion
	}
}