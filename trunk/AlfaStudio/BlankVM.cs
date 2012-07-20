using AlphaStudioCore;
using AlphaXTransport;

namespace AlfaStudio
{
	class BlankVM:AlphaNamedVM
	{
		public override string DocumentTitle
		{
			get
			{
				return "New page";
			}
		}

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.NONE; }
		}
	}
}