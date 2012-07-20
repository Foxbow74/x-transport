using System.Linq;
using System.Windows.Input;
using AlphaStudioCore;
using AlphaXTransport;
using ClientModuleUsers.VMs;
using ClientModuleUsers.Views;
using XTransport.WPF;

namespace ClientModuleUsers
{
	public class Module : AbstractModule
	{
		public Module(ModuleRegistrator _moduleRegistrator)
		{
			UiManager.RegisterDescriptor(new AlphaToolDescriptor("Users", EAlphaToolKind.PORTFOLIO_BROWSER, ModifierKeys.Alt, Key.F4, () => new ModuleView { DataContext = AlphaClient.Instance.GetRoot<ModuleVM>() }));
		}

		public override void AllModulesRegistered(ModuleRegistrator _moduleRegistrator)
		{
			//PortfolioLinkVM.DocumentDescriptors = UiManager.GetDocumentDescriptors(EAlphaKind.PORTFOLIO).ToArray();
		}
	}
}