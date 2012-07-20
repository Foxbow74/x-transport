using System;
using System.Linq.Expressions;
using System.Windows.Data;
using AlfaStudioCore;
using AvalonDock;
using ClientCommonWpf;

namespace AlfaStudio
{
	public class AlphaDocument : DocumentContent
	{
		private readonly IAbstractNamedPersistedViewModel m_namedPersistedViewModel;
		public RelayCommand BackCommand { get; private set; }
		public RelayCommand ForwardCommand { get; private set; }
		public RelayCommand CloseCommand { get; private set; }

		public AlphaDocument()
		{
			DataContext = this;
			BackCommand = new RelayCommand(ExecuteBack, _o => true);
			ForwardCommand = new RelayCommand(ExecuteForward, _o => true);
			CloseCommand = new RelayCommand(ExecuteClose, _o => true);
		}

		public AlphaDocument(IAlphaDocumentDescriptor _descriptor, Guid _id):this()
		{
			Content = _descriptor.Generate(_id, out m_namedPersistedViewModel);
			var binding = new Binding(((Expression<Func<IAbstractNamedPersistedViewModel>>) (() => ViewModel)).GetName() + "." + ((Expression<Func<string>>) (() => ViewModel.DocumentTitle)).GetName()) {Mode = BindingMode.OneWay};
			SetBinding(TitleProperty, binding);
			Id = _id;
			Kind = _descriptor.Kind;
			UiManager.UIMessage += UiManagerOnUIMessage;
		}

		public EAlphaDocumentKind Kind { get; private set; }

		private void UiManagerOnUIMessage(UIEvent _uiEvent, Guid _id)
		{
			if(_id!=Id) return;
			switch (_uiEvent)
			{
			}
		}

		
		public Guid Id { get; private set; }

		public IAbstractNamedPersistedViewModel ViewModel
		{
			get { return m_namedPersistedViewModel; }
		}

		private void ExecuteClose(object _obj)
		{
			throw new NotImplementedException();
		}

		private void ExecuteForward(object _obj)
		{
			throw new NotImplementedException();
		}

		private void ExecuteBack(object _o)
		{
			throw new NotImplementedException();
		}
	}
}
