using System;
using AlphaXTransport;
using XTransport;

namespace ClientModulePositionsEditor.VMs
{
	class CurrencyPairVM : AbstractAssetVM
	{
		[X("LEFT_CURRENCY")]
		private IXValue<CurrencyVM> m_left;

		[X("RIGHT_CURRENCY")]
		private IXValue<CurrencyVM> m_right;

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.CURRENCY_PAIR; }
		}

		public override string Name
		{
			get { return m_left.Value.Code + m_right.Value.Code; }
		}

		public override string AssetTypeName
		{
			get { throw new NotImplementedException(); }
		}
	}
}