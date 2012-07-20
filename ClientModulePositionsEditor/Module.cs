using System.Windows.Input;
using AlphaStudioCore;
using AlphaXTransport;
using ClientModulePositionsEditor.VMs;
using ClientModulePositionsEditor.Views;
using XTransport.WPF;

namespace ClientModulePositionsEditor
{
	class Module : AbstractModule
	{
		public Module(ModuleRegistrator _moduleRegistrator)
		{
			UiManager.RegisterDescriptor(new AlphaDocumentDescriptor<ModuleVM>("Positions", EAlphaKind.PORTFOLIO, EAlphaDocumentKind.PORTFOLIO_POSITIONS, ModifierKeys.Alt, Key.F2, _pvm => new ModuleView { DataContext = _pvm }));
		}
	}
}
