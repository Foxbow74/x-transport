using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AlphaStudioCore;
using AlphaXTransport;
using AvalonDock;
using ClientCommonWpf;

namespace AlfaStudio
{
	public interface IAlphaVMContainer
	{
		RelayCommand CloseCommand { get; }
		RelayCommand RevertCommand { get; }
		RelayCommand UndoCommand { get; }
		RelayCommand RedoCommand { get; }
		RelayCommand SaveCommand { get; }
	}

	/// <summary>
	/// Interaction logic for AlphaPage.xaml
	/// </summary>
	public partial class AlphaPage : DocumentContent, IAlphaVMContainer
	{
		private int m_current;
		private readonly List<Tuple<Guid, IAlphaDocumentDescriptor>> m_history = new List<Tuple<Guid, IAlphaDocumentDescriptor>>();

		private IAlphaNamedVM m_namedViewModel;

		public RelayCommand BackCommand { get; private set; }
		public RelayCommand ForwardCommand { get; private set; }
		public RelayCommand CloseCommand { get; private set; }

		public RelayCommand RevertCommand { get; private set; }
		public RelayCommand UndoCommand { get; private set; }
		public RelayCommand RedoCommand { get; private set; }
		
		public RelayCommand SaveCommand { get; private set; }

		public AlphaPage()
		{
			BlankDescriptor = new AlphaDocumentDescriptor<BlankVM>("New page", EAlphaKind.NONE, EAlphaDocumentKind.BLANK, ModifierKeys.None, Key.None, _blank => null);
			InitializeComponent();

			IsCloseable = true;
			Background = Brushes.White;

			SeeAlso = new ObservableCollection<AlphaSpecificDocumentVM>();

			DataContext = this;
			BackCommand = new RelayCommand(ExecuteBack, _o => m_history.Count > 1 && m_current > 0);
			ForwardCommand = new RelayCommand(ExecuteForward, _o => m_current < (m_history.Count - 1));
			CloseCommand = new RelayCommand(_o => Close());

			RevertCommand = new RelayCommand(ExecuteRevert, _o => IsNotBlank() && AlphaClient.Instance.GetIsRevertEnabled(ViewModel.Uid));
			UndoCommand = new RelayCommand(ExecuteUndo, _o => IsNotBlank() && AlphaClient.Instance.GetIsUndoEnabled(ViewModel.Uid));
			RedoCommand = new RelayCommand(ExecuteRedo, _o => IsNotBlank() && AlphaClient.Instance.GetIsRedoEnabled(ViewModel.Uid));

			SaveCommand = new RelayCommand(ExecuteSave, _o => IsNotBlank() && ViewModel.IsDirty);

			UiManager.UIMessage += UiManagerOnUiMessage;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			ReleaseVM();
		}

		private void ReleaseVM()
		{
			if (m_namedViewModel != null)
			{
				if (!(m_namedViewModel is BlankVM))
				{
					AlphaClient.Instance.Release(m_namedViewModel);
					m_namedViewModel.PropertyChanged -= NamedViewModelOnPropertyChanged;
				}
			}
			SeeAlso.Clear();
		}

		private bool IsNotBlank()
		{
			return ViewModel!=null && !(ViewModel is BlankVM);
		}

		public void Open(IAlphaDocumentDescriptor _descriptor, Guid _id)
		{
			if(m_current<(m_history.Count-1))
			{
				m_history.RemoveRange(m_current + 1, m_history.Count - m_current - 1);
			}
			m_history.Add(new Tuple<Guid, IAlphaDocumentDescriptor>(_id, _descriptor));
			m_current = m_history.Count - 1;
			UpdateContent();
		}

		private void UpdateContent()
		{
			ReleaseVM();
			if (CurrentUid.Equals(Guid.Empty))
			{
				m_body.Content = null;
				m_namedViewModel = new BlankVM();
				m_seeAlso.Visibility = Visibility.Collapsed;
				UiManager.CastDocumentCommand(EDocumentCommand.SELECTED, EAlphaKind.NONE, EAlphaDocumentKind.BLANK, Guid.Empty);
			}
			else
			{
				var descriptor = m_history[m_current].Item2;
				m_body.Content = descriptor.Generate(CurrentUid, out m_namedViewModel);
				UiManager.CastUiMessage(EUiEvent.SELECTED, m_namedViewModel.Kind, CurrentUid);
				foreach (var dscr in UiManager.GetDocumentDescriptors(m_namedViewModel.Kind))
				{
					if (dscr.DocKind != descriptor.DocKind) SeeAlso.Add(new AlphaSpecificDocumentVM(dscr, CurrentUid));
				}
				m_seeAlso.Visibility = Visibility.Visible;
				UiManager.CastDocumentCommand(EDocumentCommand.SELECTED, m_namedViewModel.Kind, descriptor.DocKind, CurrentUid);
				m_namedViewModel.PropertyChanged += NamedViewModelOnPropertyChanged;
			}

			UpdateTitle();
		}

		private void NamedViewModelOnPropertyChanged(object _sender, PropertyChangedEventArgs _propertyChangedEventArgs)
		{
			if(_propertyChangedEventArgs.PropertyName==((Expression<Func<string>>)(() => ViewModel.DocumentTitle)).GetName())
			{
				UpdateTitle();
			}
		}

		private void UpdateTitle()
		{
			Title = m_namedViewModel.DocumentTitle;
		}

		private void UiManagerOnUiMessage(EUiEvent _eUiEvent, EAlphaKind _kind,Guid _uid)
		{
			if (_eUiEvent==EUiEvent.REF_DELETED)
			{
				var toDel = m_history.Where(_tuple => _tuple.Item1 == _uid).ToArray();
				if (toDel.Length > 0)
				{
					foreach (var tuple in toDel)
					{
						var index = m_history.IndexOf(tuple);
						m_history.Remove(tuple);
						if (index <= m_current)
						{
							m_current--;
						}
					}
					if (m_current < 0)
					{
						m_current = 0;
					}
					if (m_history.Count == 0)
					{
						Close();
						return;
					}
					UpdateContent();
				}
			}

			if(_uid!=CurrentUid) return;
			switch (_eUiEvent)
			{
			}
		}

		public override bool Close()
		{
			UiManager.UIMessage -= UiManagerOnUiMessage;
			return base.Close();
		}

		public Guid CurrentUid { get { return m_history[m_current].Item1; } }

		public EAlphaDocumentKind Kind { get { return m_history[m_current].Item2.DocKind; } }

		public IAlphaNamedVM ViewModel
		{
			get { return m_namedViewModel; }
		}

		private void ExecuteForward(object _obj)
		{
			m_current++;
			UpdateContent();
		}

		private void ExecuteBack(object _o)
		{
			m_current--;
			UpdateContent();
		}

		private void ExecuteRevert(object _o)
		{
			AlphaClient.Instance.Revert(ViewModel.Uid);
		}

		private void ExecuteUndo(object _o)
		{
			AlphaClient.Instance.Undo(ViewModel.Uid);
		}

		private void ExecuteRedo(object _o)
		{
			AlphaClient.Instance.Redo(ViewModel.Uid);
		}

		private void ExecuteSave(object _obj)
		{
			AlphaClient.Instance.Save(ViewModel.Uid);
		}

		public ObservableCollection<AlphaSpecificDocumentVM> SeeAlso { get; private set; }

		public static IAlphaDocumentDescriptor BlankDescriptor { get; private set; }

		public void Selected()
		{
			if (IsNotBlank())UiManager.CastDocumentCommand(EDocumentCommand.SELECTED, ViewModel.Kind, Kind, CurrentUid);
		}
	}
}
