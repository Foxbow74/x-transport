using System;

namespace XTransport.Server.Storage
{
	internal class StorageObject : IStorageRecord
	{
		public DateTime ValidFrom { get; set; }
		public Guid Uid { get; set; }
		public int Kind { get; set; }

		public override string ToString()
		{
			return Kind + " \t" + Uid;
		}
	}
}