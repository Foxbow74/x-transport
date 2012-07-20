using System.Linq;
using System.Windows.Input;
using AlphaStudioCore;
using AlphaXTransport;
using ClientModuleUsers.VMs;
using ClientModuleUsers.Views;
using XTransport.WPF;

namespace ClientModuleUsers
{
	public class ModuleUsers : AbstractModule
	{
		public ModuleUsers(ModuleRegistrator _moduleRegistrator)
		{
			UiManager.RegisterDescriptor(new AlphaRootToolDescriptor<UsersBrowserVM>("Пользователи", ModifierKeys.None, Key.F2, _vm => new UsersBrowserView { DataContext = _vm }));
			UiManager.RegisterDescriptor(new AlphaDocumentDescriptor<UserVM>("Edit", EAlphaKind.USER, EAlphaDocumentKind.EDIT, ModifierKeys.Alt, Key.F4, _vm => new UserView { DataContext = _vm }) { IsDefault = true });
		}

		public override void AllModulesRegistered(ModuleRegistrator _moduleRegistrator)
		{
			UserLinkVM.DocumentDescriptors = UiManager.GetDocumentDescriptors(EAlphaKind.USER).ToArray();
		}
	}
}