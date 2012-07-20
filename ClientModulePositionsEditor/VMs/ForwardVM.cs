using AlphaXTransport;

namespace ClientModulePositionsEditor.VMs
{
	class ForwardVM : AbstractDerivativeVM
	{
		public override EAlphaKind Kind
		{
			get { return EAlphaKind.FORWARD; }
		}

		public override string Name
		{
			get { return "Forward " + Asset.Name; }
		}
	}
}