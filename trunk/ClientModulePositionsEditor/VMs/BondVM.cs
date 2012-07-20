using AlphaXTransport;
using XTransport;

namespace ClientModulePositionsEditor.VMs
{
	class BondVM : AbstractAssetVM
	{
		[X("NAME")]
		private IXValue<string> m_name;

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.BOND; }
		}

		public override string Name
		{
			get { return m_name.Value; }
		}

		public override string AssetTypeName
		{
			get { return "Bond"; }
		}
	}
}