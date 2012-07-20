using AlphaStudioCore;
using XTransport;

namespace ClientModulePositionsEditor.VMs
{
	abstract class AbstractDerivativeVM : AlphaVM
	{
		[X("ASSET")]
		[XFactory(typeof(AssetFactory))]
		private IXValue<AbstractAssetVM> m_asset;

		public AbstractAssetVM Asset{get{return m_asset.Value;} }

		public abstract string Name { get; }


		private bool m_selected;
		public bool Selected
		{
			get { return m_selected; }
			set
			{
				m_selected = value;
				OnPropertyChanged(() => Selected);
			}
		}
	}
}