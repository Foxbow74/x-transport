using XTransport.WPF;
using XTransportTest.Client;

namespace XTransportTest.ClientVM
{
	internal abstract class XObjectVM : ClientXObjectVM<ETestKind>
	{
	}

	internal abstract class XChildObjectVM<TParent> : ClientXChildObjectVM<ETestKind, TParent>
		where TParent : XObjectVM, new()
	{
	}
}