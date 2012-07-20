using System.Windows.Input;
using AlphaStudioCore;
using ClientModuleOutput.VMs;
using ClientModuleOutput.Views;
using XTransport.WPF;

namespace ClientModuleOutput
{
	class Module : AbstractModule
	{
		public Module(ModuleRegistrator _moduleRegistrator)
		{
			UiManager.RegisterDescriptor(new AlphaToolDescriptor("Output", EAlphaToolKind.OUTPUT, ModifierKeys.Alt, Key.F12, () => new ModuleOutputView { DataContext = new ModuleOutputVM() }));
		}
	}
}
