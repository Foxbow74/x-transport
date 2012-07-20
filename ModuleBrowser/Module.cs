using System.Linq;
using System.Windows.Input;
using AlphaStudioCore;
using AlphaXTransport;
using ClientModuleBrowser.VMs;
using ClientModuleBrowser.Views;
using XTransport.WPF;

namespace ClientModuleBrowser
{
	public class Module : AbstractModule
	{
		public Module(ModuleRegistrator _moduleRegistrator)
		{
			UiManager.RegisterDescriptor(new AlphaToolDescriptor("Browser", EAlphaToolKind.PORTFOLIO_BROWSER, ModifierKeys.Alt, Key.F3, () => new ModuleBrowserView { DataContext = AlphaClient.Instance.GetRoot<ModuleBrowserVM>() }));
		}

		public override void AllModulesRegistered(ModuleRegistrator _moduleRegistrator)
		{
			PortfolioLinkVM.DocumentDescriptors = UiManager.GetDocumentDescriptors(EAlphaKind.PORTFOLIO).ToArray();
		}
	}
}