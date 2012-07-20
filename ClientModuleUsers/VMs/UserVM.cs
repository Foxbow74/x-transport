using AlphaStudioCore;
using AlphaXTransport;

namespace ClientModuleUsers.VMs
{
	class UserVM:AlphaNamedVM
	{
		public override EAlphaKind Kind
		{
			get { return EAlphaKind.USER; }
		}

		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
		}
	}
}
