using AlphaStudioCore;
using AlphaXTransport;

namespace ClientModulePortfolioEditor.VMs
{
	class CurrencyVM : AlphaNamedVM
	{
		public override EAlphaKind Kind
		{
			get { return EAlphaKind.CURRENCY; }
		}
	}
}