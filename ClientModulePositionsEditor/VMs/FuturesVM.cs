using AlphaXTransport;

namespace ClientModulePositionsEditor.VMs
{
	class FuturesVM : AbstractDerivativeVM
	{
		public override EAlphaKind Kind
		{
			get { return EAlphaKind.FUTURES; }
		}
	
		public override string Name
		{
			get { return Asset.AssetTypeName + " futures " + Asset.Name; }
		}
	}
}