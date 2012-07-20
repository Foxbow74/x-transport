using AlphaStudioCore;
using AlphaXTransport;
using XTransport;

namespace ClientModulePositionsEditor.VMs
{
	class PortfolioInstrumentVM : AlphaChildVM<ModuleVM>
	{
		[X("DERIVATIVE")]
		[XFactory(typeof(DerivativeFactory))]
		private IXValue<AbstractDerivativeVM> m_derivative;

		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
			LinkProperty(m_derivative,()=>Name);
		}

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.PORTFOLIO_INSTRUMENT; }
		}

		public string Name
		{
			get { return Derivative.Name; }
		}

		public AbstractDerivativeVM Derivative
		{
			get { return m_derivative.Value; }
			set { m_derivative.Value = value; }
		}

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