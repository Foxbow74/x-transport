using System;
using XTransport.Server.Storage;

namespace XTransport.Server
{
	internal interface IServerXValue
	{
		void SaveValue(Guid _uid, int _fieldId, IStorage _storage, DateTime _now);
		AbstractXReportItem GetOriginalValueReportItem(int _fieldId, SessionId _sessionId);
		void SetValue(IStorageValue _val);

		int SaveOriginalValue(Guid _uid, AbstractXReportItem _reportItem, IStorage _storage, int? _lastId, DateTime _now,
		                      AbstractXServer _abstractXServer);

		AbstractXReportItem CreateReverseReportItem(AbstractXReportItem _item);
	}
}