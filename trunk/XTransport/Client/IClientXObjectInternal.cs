using System;
using System.Collections.Generic;

namespace XTransport.Client
{
	internal interface IClientXObjectInternal<TKind> : IClientXObject<TKind>
	{
		IEnumerable<AbstractXReportItem> GetChanges();
		void ApplyChanges(XReport _report, bool _firstTime);
		void Revert();
		void SaveInternal();
		void OnInstantiationFinished(AbstractXClient<TKind> _client);
		void SetUid(Guid _uid);
		void OnDeserialized();
		IEnumerable<Guid> GetChildUids();
		void AddedToCollection<T>(T _item, int _kind) where T : ClientXObject<TKind>;
		void RemovedFromCollection<T>(T _item) where T : ClientXObject<TKind>;
	}
}