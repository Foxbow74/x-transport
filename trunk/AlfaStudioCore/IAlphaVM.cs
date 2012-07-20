using AlphaXTransport;
using XTransport.Client;

namespace AlphaStudioCore
{
	public interface IAlphaVM : IClientXObjectVM<EAlphaKind>
	{
		void ViewCreated();
	}
}