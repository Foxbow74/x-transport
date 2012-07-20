using AlphaStudioCore;
using AvalonDock;

namespace AlfaStudio
{
	public class AlphaTool : DockableContent
	{
		public AlphaToolDescriptor Descriptor { get; private set; }

		public AlphaTool(AlphaToolDescriptor _descriptor)
		{
			Descriptor = _descriptor;
			Content = _descriptor.GenerateFunc();
			Title = _descriptor.Name;
			Name = _descriptor.Kind.ToString();
		}
	}
}