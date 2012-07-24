using System;

namespace XTransport.Client
{
	public interface IClientXObject<TKind>
	{
		Guid Uid { get; }
		TKind Kind { get; }
		bool IsDirty { get; }
		event Action<IClientXObject<TKind>> Changed;
	}
}