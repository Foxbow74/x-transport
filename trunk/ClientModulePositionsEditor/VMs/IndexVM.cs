using AlphaXTransport;
using XTransport;

namespace ClientModulePositionsEditor.VMs
{
	class IndexVM : AbstractAssetVM
	{
		[X("NAME")]
		private IXValue<string> m_name;

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.INDEX; }
		}

		public override string Name
		{
			get { return m_name.Value; }
		}

		public override string AssetTypeName
		{
			get { return "Index"; }
		}
	}
}