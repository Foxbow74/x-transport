using System;

namespace XTransport.Server.Storage
{
	public class StorageRootObject : IStorageRecord
	{
		public Guid Uid { get; set; }
		public int Kind { get; set; }
	}
}