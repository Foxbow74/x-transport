using AlphaXTransport;
using XTransport.WPF;

namespace AlphaStudioCore
{
	public abstract class AlphaVM : ClientXObjectVM<EAlphaKind>, IAlphaVM
	{
		public virtual void ViewCreated()
		{}
	}
}