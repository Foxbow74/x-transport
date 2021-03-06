﻿using System;
using System.Collections.Generic;

namespace XTransport.Server.Storage
{
public interface IStorage : IDisposable
{
	IEnumerable<StorageRootObject> LoadRoot();
	IEnumerable<IStorageRecord> LoadObject(Guid _uid, DateTime _now = default(DateTime));
	AbstractXServer.ObjectDescriptor LoadObjectCharacteristics(Guid _uid, DateTime _now = default(DateTime));

	int InsertMain(Guid _uid, int _kind, DateTime _now, Guid _parent = default(Guid), int? _field = null);
	int InsertValue<T>(Guid _uid, int _field, T _value, int? _lastId, DateTime _now);
	void Delete(Guid _uid, int _field, DateTime _now);

	IDisposable CreateTransaction();
	Guid GetCollectionOwnerUid(Guid _uid);

	void DeleteAll();
	void Shrink();
}
}