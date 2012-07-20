using XTransport.Client;

namespace XTransportTest.Client
{
	internal abstract class XObject : ClientXObject<ETestKind>, IClientSideXObject
	{
	}

	internal abstract class XChildObject<TParent> : ClientXChildObject<ETestKind, TParent>, IClientSideXObject
		where TParent : XObject
	{
	}
}