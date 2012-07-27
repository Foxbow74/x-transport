using System;
using System.Collections.Generic;

namespace XTransport.Server.Storage
{
	public interface IStorage : IDisposable
	{
		int InsertValue<T>(Guid _uid, int _field, T _value, int? _lastId, DateTime _now);
		void Delete(Guid _uid, int _field, DateTime _now);

		IEnumerable<StorageRootObject> LoadRoot();
		DateTime LoadObjectParameters(Guid _uid, out int _kind);
		IEnumerable<IStorageRecord> LoadObject(Guid _uid, DateTime _now);

		int InsertMain(Guid _uid, int _kind, DateTime _now, Guid _parent = default(Guid), int? _field = null);

		IDisposable CreateTransaction();
		Guid GetCollectionOwnerUid(Guid _uid);
	}
}