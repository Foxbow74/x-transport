using System;

namespace XTransport.Server.Storage
{
	public class StorageValue<T> : IStorageValueInternal
	{
		public T Val { get; set; }

		#region IStorageValueInternal Members

		public Guid Owner { get; set; }
		public int Field { get; set; }

		public int OldId { get; set; }

		public int Id { get; set; }

		IServerXValue IStorageValueInternal.CreateIServerXValue()
		{
			return new ServerXValue<T> {Value = Val};
		}

		#endregion

		public override string ToString()
		{
			return "\t\t" + Owner + "\t" + Field + " = " + Val + " id=" + Id;
		}
	}
}