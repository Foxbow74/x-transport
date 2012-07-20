using System.Collections.Generic;
using System.Collections.ObjectModel;
using AlphaStudioCore;
using AlphaXTransport;
using XTransport;

namespace ClientModulePositionsEditor.VMs
{
	class GeneralVM : AlphaVM
	{
		[X((int)EAlphaKind.DERIVATIVE)]
		[XFactory(typeof(DerivativeFactory))]
		private IList<AbstractDerivativeVM> m_derivatives;

		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
			DerivativeVMs = CreateObservableCollection(m_derivatives);
		}

		public ReadOnlyObservableCollection<AbstractDerivativeVM> DerivativeVMs { get; private set; }

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.NONE; }
		}
	}
}