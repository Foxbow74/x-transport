using System.Collections.ObjectModel;
using AlphaStudioCore;
using AlphaXTransport;
using XTransport;

namespace ClientModulePortfolioEditor.VMs
{
	class PortfolioEditorVM : AlphaNamedVM
	{
		[X("NOTIONAL_AMOUNT")]
		private IXValue<decimal> m_notionalAmount;

		[X("BASE_CURRENCY")]
		private IXValue<CurrencyVM> m_baseCurrency;

		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
			LinkProperty(m_notionalAmount, () => NotionalAmount);
			LinkProperty(m_baseCurrency, () => BaseCurrency);

			CurrencyVMs = AlphaClient.Instance.GetRoot<GeneralVM>().CurrencyVMs;
		}

		public ReadOnlyObservableCollection<CurrencyVM> CurrencyVMs { get; private set; }

		public CurrencyVM BaseCurrency
		{
			get { return m_baseCurrency.Value; } 
			set { m_baseCurrency.Value = value; }
		}

		public decimal NotionalAmount
		{
			get { return m_notionalAmount.Value; }
			set { m_notionalAmount.Value = value; }
		}

		public override string DocumentTitle
		{
			get
			{
				return Name + " editor";
			}
		}

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.PORTFOLIO; }
		}
	}
}
