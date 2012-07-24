using XTransport.Client;

namespace XTransport
{
	public interface IXObjectFactory<TKind>
	{
		TKind Kind { get; }
		IClientXObject<TKind> CreateInstance(TKind _kind);
	}
}