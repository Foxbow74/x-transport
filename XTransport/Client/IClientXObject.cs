using System;

namespace XTransport.Client
{
	public interface IClientXObject<TKind> : IXObject<TKind>
	{
		bool IsDirty { get; }
		event Action<IClientXObject<TKind>> Changed;
	}
}