using ClientCommonWpf;

namespace AlphaStudioCore
{
	public class AlphaToolDescriptorVM
	{
		public AlphaToolDescriptor Descriptor { get; private set; }
		public AlphaToolDescriptorVM(AlphaToolDescriptor _descriptor)
		{
			Descriptor = _descriptor;
			InputGestureText = Utils.InputGestureText(_descriptor.ModifierKeys, _descriptor.Key);
			OpenCommand = new RelayCommand(ExecuteOpen);
		}

		private void ExecuteOpen(object _o)
		{
			UiManager.CastShowToolWindow(Descriptor.Kind);
		}

		public string InputGestureText { get; private set; }
		public RelayCommand OpenCommand { get; private set; }
	}

	public class AlphaRootToolDescriptorVM
	{
		public IAlphaRootToolDescriptor Descriptor { get; private set; }
		public AlphaRootToolDescriptorVM(IAlphaRootToolDescriptor _descriptor)
		{
			Descriptor = _descriptor;
			InputGestureText = Utils.InputGestureText(_descriptor.ModifierKeys, _descriptor.Key);
			OpenCommand = new RelayCommand(ExecuteOpen);
		}

		private void ExecuteOpen(object _o)
		{
			UiManager.CastShowRootToolWindow(Descriptor);
		}

		public string InputGestureText { get; private set; }
		public RelayCommand OpenCommand { get; private set; }
	}
}
