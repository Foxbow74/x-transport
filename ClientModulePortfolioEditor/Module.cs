using System.Windows.Input;
using AlphaStudioCore;
using AlphaXTransport;
using ClientModulePortfolioEditor.VMs;
using ClientModulePortfolioEditor.Views;
using XTransport.WPF;

namespace ClientModulePortfolioEditor
{
	class Module: AbstractModule
	{
		public Module(ModuleRegistrator _moduleRegistrator)
		{
			UiManager.RegisterDescriptor(new AlphaDocumentDescriptor<PortfolioEditorVM>("Edit", EAlphaKind.PORTFOLIO, EAlphaDocumentKind.EDIT, ModifierKeys.None, Key.F2, _pvm => new PortfolioEditorView() { DataContext = _pvm }) { IsDefault = true });
		}
	}
}
