using System;
using XTransport.Client;

namespace XTransport
{
	public interface IXObject<TKind>
	{
		Guid Uid { get; }
		TKind Kind { get; }
	}

	public interface IXObjectFactory<TKind>
	{
		TKind Kind { get; }
		IClientXObject<TKind> CreateInstance(TKind _kind);
	}
}