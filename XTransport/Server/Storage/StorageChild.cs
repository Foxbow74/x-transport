using System;

namespace XTransport.Server.Storage
{
	internal class StorageChild : StorageObject
	{
		public Guid Parent { get; set; }
		public int Field { get; set; }
		public DateTime? ValidTill { get; set; }

		public override string ToString()
		{
			return " \t" + Kind + " \t" + Uid + " \t" + Parent + " \t" + Field;
		}
	}
}