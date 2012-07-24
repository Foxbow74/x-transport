using System;

namespace XTransport.Server.Storage
{
	public interface IStorageValue : IStorageRecord
	{
		Guid Owner { get; set; }
		int Field { get; set; }
		int OldId { get; set; }
		int Id { get; set; }
	}

	internal interface IStorageValueInternal : IStorageValue
	{
		IServerXValue CreateIServerXValue();
	}
}