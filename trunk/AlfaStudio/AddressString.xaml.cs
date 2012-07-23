using System;
using System.Windows.Controls;
using AlphaStudioCore;
using AlphaXTransport;

namespace AlfaStudio
{
	/// <summary>
	/// Interaction logic for AddressString.xaml
	/// </summary>
	public partial class AddressString : UserControl
	{
		public AddressString()
		{
			InitializeComponent();
			UiManager.DocumentEvent += UiManagerDocumentEvent;
			UiManager.UIMessage += UiManagerUIMessage;
		}

		private void UiManagerUIMessage(EUiEvent _arg1, EAlphaKind _arg2, Guid _arg3)
		{
			
		}

		void UiManagerDocumentEvent(EDocumentCommand _command, EAlphaKind _kind, EAlphaDocumentKind _documentKind, Guid _uid)
		{
			switch (_command)
			{
				case EDocumentCommand.SELECTED:
					m_tb.Text = _documentKind.GetDocumentAddress(_uid);
					break;
			}
		}
	}
}
