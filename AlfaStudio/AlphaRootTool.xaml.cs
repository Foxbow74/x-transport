using System;
using System.ComponentModel;
using AlphaStudioCore;
using AlphaXTransport;
using AvalonDock;
using ClientCommonWpf;

namespace AlfaStudio
{
	/// <summary>
	/// Interaction logic for AlphaRootTool.xaml
	/// </summary>
	public partial class AlphaRootTool : DockableContent, IAlphaVMContainer
	{
		private readonly IAlphaVM m_vm;

		public RelayCommand CloseCommand { get; private set; }
		public RelayCommand RevertCommand { get; private set; }
		public RelayCommand UndoCommand { get; private set; }
		public RelayCommand RedoCommand { get; private set; }

		public RelayCommand SaveCommand { get; private set; }

		public AlphaRootTool(IAlphaRootToolDescriptor _descriptor)
		{
			InitializeComponent();

			Name = _descriptor.RootToolIdentifier;
			var element = _descriptor.Generate(out m_vm);
			m_body.DataContext = m_vm;
			m_body.Content = element;
			Title = _descriptor.Name;
			Descriptor = _descriptor;
			DataContext = this;
			RevertCommand = new RelayCommand(ExecuteRevert, _o => AlphaClient.Instance.GetIsRevertEnabled(m_vm.Uid));
			UndoCommand = new RelayCommand(ExecuteUndo, _o => AlphaClient.Instance.GetIsUndoEnabled(m_vm.Uid));
			RedoCommand = new RelayCommand(ExecuteRedo, _o => AlphaClient.Instance.GetIsRedoEnabled(m_vm.Uid));
			SaveCommand = new RelayCommand(ExecuteSave, _o => m_vm.IsDirty);
			CloseCommand = new RelayCommand(_o => Close());

			UiManager.UIMessage += UiManagerOnUiMessage;
		}

		public IAlphaRootToolDescriptor Descriptor { get; private set; }

		private void ExecuteRevert(object _o)
		{
			AlphaClient.Instance.Revert(m_vm.Uid);
		}

		private void ExecuteUndo(object _o)
		{
			AlphaClient.Instance.Undo(m_vm.Uid);
		}

		private void ExecuteRedo(object _o)
		{
			AlphaClient.Instance.Redo(m_vm.Uid);
		}

		private void ExecuteSave(object _obj)
		{
			AlphaClient.Instance.Save(m_vm.Uid);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (m_vm != null)
			{
				AlphaClient.Instance.Release(m_vm);
			}
		}

		private void UiManagerOnUiMessage(EUiEvent _eUiEvent, EAlphaKind _kind, Guid _id)
		{
			if (_id != m_vm.Uid) return;
			switch (_eUiEvent)
			{
			}
		}
	}
}
