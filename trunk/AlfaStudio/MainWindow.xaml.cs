using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AlphaStudioCore;
using AlphaXTransport;
using AvalonDock;
using ClientCommonWpf;
using XTransport.WPF;

namespace AlfaStudio
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private readonly ModuleRegistrator m_moduleRegistrator;

		public ObservableCollection<AlphaPage> Documents { get; private set; }

		public ObservableCollection<AlphaToolDescriptorVM> Tools { get; private set; }

		public ObservableCollection<AlphaRootToolDescriptorVM> RootTools { get; private set; }

		public MainWindow()
		{
			m_moduleRegistrator = new ModuleRegistrator();
			m_moduleRegistrator.LoadAssembliesNamesContains("ClientModule");
			m_moduleRegistrator.RegisterModules();

			Documents = new ObservableCollection<AlphaPage>();

			Tools = new ObservableCollection<AlphaToolDescriptorVM>();

			foreach (var alphaToolDescriptor in UiManager.ToolDescriptors)
			{
				var item = new AlphaToolDescriptorVM(alphaToolDescriptor);
				InputBindings.Add(new InputBinding(item.OpenCommand, new KeyGesture(item.Descriptor.Key, item.Descriptor.ModifierKeys)));
				Tools.Add(item);
			}

			RootTools = new ObservableCollection<AlphaRootToolDescriptorVM>();
			foreach (var alphaToolDescriptor in UiManager.GetRootToolDescriptors())
			{
				var item = new AlphaRootToolDescriptorVM(alphaToolDescriptor);
				InputBindings.Add(new InputBinding(item.OpenCommand, new KeyGesture(item.Descriptor.Key, item.Descriptor.ModifierKeys)));
				RootTools.Add(item);
			}

			UiManager.ShowToolWindow += UiManagerOnShowToolWindow;
			UiManager.ShowRootToolWindow += UiManagerOnShowRootToolWindow;
			UiManager.DocumentEvent += UiManagerOnDocumentEvent;

			NewPageCommand = new RelayCommand(NewPageExecute, NewPageCanExecute);

			InitializeComponent();
			DataContext = this;
			m_manager.Loaded += ManagerOnLoaded;
			m_manager.ActiveContentChanged += MManagerOnActiveContentChanged;
			m_manager.DeserializationCallback = DeserializationCallback;
			Closing += MainWindowClosing;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void MManagerOnActiveContentChanged(object _sender, EventArgs _eventArgs)
		{
			PropertyChanged.Cast(this, () => ActivePage);
			PropertyChanged.Cast(this, () => ForwardCommand);
			PropertyChanged.Cast(this, () => BackCommand);
			PropertyChanged.Cast(this, () => CloseCommand);
			PropertyChanged.Cast(this, () => RevertCommand);
			PropertyChanged.Cast(this, () => UndoCommand);
			PropertyChanged.Cast(this, () => RedoCommand);
			PropertyChanged.Cast(this, () => SaveCommand);

			if (ActivePage != null)
			{
				ActivePage.Selected();
			}
		}

		private void UiManagerOnDocumentEvent(EDocumentCommand _command, EAlphaKind _model, EAlphaDocumentKind _kind, Guid _uid)
		{
			switch (_command)
			{
				case EDocumentCommand.OPEN:
				case EDocumentCommand.OPEN_IN_NEW_TAB:
					foreach (var alphaPage in m_manager.Documents.OfType<AlphaPage>())
					{
						if(alphaPage.Kind==_kind && alphaPage.CurrentUid==_uid)
						{
							m_manager.ActiveContent = alphaPage;
							alphaPage.Show(m_manager);
							return;
						}
					}
					var descriptor = UiManager.GetDescriptor(_model, _kind);
					
					if (_command==EDocumentCommand.OPEN_IN_NEW_TAB || ActivePage==null)
					{
						NewPageExecute(descriptor);
					}
					var page = ((AlphaPage)m_manager.ActiveDocument);
					page.Open(descriptor, _uid);
					m_manager.ActiveContent = page;
					page.Show(m_manager);
					break;
				case EDocumentCommand.ADD_TO_FAVORITS:
				case EDocumentCommand.SELECTED:
					break;
				default:
					throw new ArgumentOutOfRangeException("_command");
			}
		}

		private void UiManagerOnShowToolWindow(EAlphaToolKind _eAlphaToolKind)
		{
			foreach (var alphaTool in m_manager.DockableContents.OfType<AlphaTool>())
			{
				if (alphaTool.Descriptor.Kind == _eAlphaToolKind)
				{
					m_manager.ActiveContent = alphaTool;
					alphaTool.Show(m_manager);
					return;
				}
			}
			var descriptor = UiManager.GetDescriptor(_eAlphaToolKind);
			var tool = new AlphaTool(descriptor);
			m_manager.MainDocumentPane.Items.Add(tool);
		}

		private void UiManagerOnShowRootToolWindow(IAlphaRootToolDescriptor _alphaRootToolDescriptor)
		{
			foreach (var alphaTool in m_manager.DockableContents.OfType<AlphaRootTool>())
			{
				if (alphaTool.Descriptor.Name == _alphaRootToolDescriptor.Name)
				{
					m_manager.ActiveContent = alphaTool;
					alphaTool.Show(m_manager);
					return;
				}
			}
			var alphaRootTool = new AlphaRootTool(_alphaRootToolDescriptor);
			m_manager.MainDocumentPane.Items.Add(alphaRootTool);
		}

		private void DeserializationCallback(object _sender, DeserializationCallbackEventArgs _deserializationCallbackEventArgs)
		{

			var rootToolDescriptor = UiManager.GetRootToolDescriptor(_deserializationCallbackEventArgs.Name);
			if (rootToolDescriptor != null)
			{
				_deserializationCallbackEventArgs.Content = new AlphaRootTool(rootToolDescriptor);
			}
			else
			{
				EAlphaToolKind kind;
				if (Enum.TryParse(_deserializationCallbackEventArgs.Name, out kind))
				{
					var descriptor = UiManager.GetDescriptor(kind);
					_deserializationCallbackEventArgs.Content = new AlphaTool(descriptor);
				}
		
			}
		}

		void MainWindowClosing(object _sender, CancelEventArgs _e)
		{
			m_manager.SaveLayout("jj.txt");
		}

		private void ManagerOnLoaded(object _sender, RoutedEventArgs _routedEventArgs)
		{
			if (File.Exists("jj.txt"))
			{
				m_manager.RestoreLayout("jj.txt");
			}
		}

		#region new page

		public RelayCommand NewPageCommand { get; private set; }

		public RelayCommand BackCommand
		{
			get
			{
				return ActivePage == null ? m_emptyCommand : ActivePage.BackCommand;
			}
		}

		public RelayCommand ForwardCommand
		{
			get
			{
				return ActivePage == null ? m_emptyCommand : ActivePage.ForwardCommand;
			}
		}

		public RelayCommand CloseCommand
		{
			get
			{
				return ActiveVMContainer == null ? m_emptyCommand : ActiveVMContainer.CloseCommand;
			}
		}

		public RelayCommand RevertCommand
		{
			get
			{
				return ActiveVMContainer == null ? m_emptyCommand : ActiveVMContainer.RevertCommand;
			}
		}

		public RelayCommand UndoCommand
		{
			get
			{
				return ActiveVMContainer == null ? m_emptyCommand : ActiveVMContainer.UndoCommand;
			}
		}

		public RelayCommand RedoCommand
		{
			get
			{
				return ActiveVMContainer == null ? m_emptyCommand : ActiveVMContainer.RedoCommand;
			}
		}

		public RelayCommand SaveCommand
		{
			get
			{
				return ActiveVMContainer == null ? m_emptyCommand : ActiveVMContainer.SaveCommand;
			}
		}

		private readonly RelayCommand m_emptyCommand = new RelayCommand(delegate {  }, _o => false);

		private AlphaPage ActivePage
		{
			get
			{
				return (m_manager.ActiveDocument != null && m_manager.ActiveDocument is AlphaPage) ? ((AlphaPage)m_manager.ActiveDocument) : null;
			}

		}

		private IAlphaVMContainer ActiveVMContainer
		{
			get
			{
				return (m_manager.ActiveContent != null && m_manager.ActiveContent is IAlphaVMContainer) ? ((IAlphaVMContainer)m_manager.ActiveContent) : null;
			}

		}

		

		private bool NewPageCanExecute(object _o)
		{
			return true;
		}

		private void NewPageExecute(object _o)
		{
			var alphaDocument = new AlphaPage();

			if(_o==null)
			{
				alphaDocument.Open(AlphaPage.BlankDescriptor, Guid.Empty);
			}

			Documents.Add(alphaDocument);
			m_manager.ActiveContent = alphaDocument;
			alphaDocument.Show(m_manager);
		}

		#endregion
	}

}