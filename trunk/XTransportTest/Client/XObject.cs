using XTransport.Client;

namespace XTransportTest.Client
{
	internal abstract class XObject : ClientXObject<ETestKind>
	{
	}

	internal abstract class XChildObject<TParent> : ClientXChildObject<ETestKind, TParent>
		where TParent : XObject
	{
	}
}