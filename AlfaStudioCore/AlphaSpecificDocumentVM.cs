using System;
using ClientCommonWpf;

namespace AlphaStudioCore
{
	public class AlphaSpecificDocumentVM
	{
		private readonly Guid m_uid;

		public IAlphaDocumentDescriptor Descriptor { get; set; }

		public string InputGestureText { get; private set; }
		public RelayCommand OpenCommand { get; private set; }
		public RelayCommand OpenInNewTabCommand { get; private set; }
		public RelayCommand AddToFavoritsCommand { get; private set; }

		public AlphaSpecificDocumentVM(IAlphaDocumentDescriptor _descriptor, Guid _uid)
		{
			m_uid = _uid;
			Descriptor = _descriptor;

			InputGestureText = Utils.InputGestureText(_descriptor.ModifierKeys, _descriptor.Key);
			OpenCommand = new RelayCommand(OpenExecute);
			OpenInNewTabCommand = new RelayCommand(OpenInNewTabExecute);
			AddToFavoritsCommand = new RelayCommand(AddToFavoritsExecute);
		}

		private void OpenInNewTabExecute(object _obj)
		{
			UiManager.CastDocumentCommand(EDocumentCommand.OPEN_IN_NEW_TAB, Descriptor.Kind, Descriptor.DocKind, m_uid);
		}

		private void AddToFavoritsExecute(object _obj)
		{
			UiManager.CastDocumentCommand(EDocumentCommand.ADD_TO_FAVORITS, Descriptor.Kind, Descriptor.DocKind, m_uid);
		}

		private void OpenExecute(object _o)
		{
			UiManager.CastDocumentCommand(EDocumentCommand.OPEN, Descriptor.Kind, Descriptor.DocKind, m_uid);
		}
	}
}