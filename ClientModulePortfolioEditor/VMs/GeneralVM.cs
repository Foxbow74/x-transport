using System.Collections.Generic;
using System.Collections.ObjectModel;
using AlphaStudioCore;
using AlphaXTransport;
using XTransport;

namespace ClientModulePortfolioEditor.VMs
{
	class GeneralVM : AlphaVM
	{
		[X((int)EAlphaKind.CURRENCY)]
		private ICollection<CurrencyVM> m_currencyVms;

		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
			CurrencyVMs = CreateObservableCollection(m_currencyVms);
		}

		public ReadOnlyObservableCollection<CurrencyVM> CurrencyVMs { get; private set; }

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.NONE; }
		}
	}
}