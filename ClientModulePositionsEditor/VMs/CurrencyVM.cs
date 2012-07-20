using AlphaStudioCore;
using AlphaXTransport;
using XTransport;

namespace ClientModulePositionsEditor.VMs
{
	class CurrencyVM : AlphaVM
	{
		[X("CODE")]
		private IXValue<string> m_code;

		public string Code { get { return m_code.Value; } }

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.CURRENCY; }
		}
	}
}