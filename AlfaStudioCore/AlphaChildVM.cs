using AlphaXTransport;
using XTransport.WPF;

namespace AlphaStudioCore
{
	public abstract class AlphaChildVM<TParent> : ClientXChildObjectVM<EAlphaKind, TParent>, IAlphaVM
		where TParent : ClientXObjectVM<EAlphaKind>
	{
		public virtual void ViewCreated(){}
	}
}