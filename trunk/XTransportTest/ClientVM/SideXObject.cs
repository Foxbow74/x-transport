using XTransport.WPF;
using XTransportTest.Client;

namespace XTransportTest.ClientVM
{
	internal abstract class XObjectVM : ClientXObjectVM<ETestKind>, IClientSideXObject
	{
	}

	internal abstract class XChildObjectVM<TParent> : ClientXChildObjectVM<ETestKind, TParent>,
	                                                            IClientSideXObject
		where TParent : XObjectVM, new()
	{
	}
}