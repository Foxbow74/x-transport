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

		void UiManagerUIMessage(EUiEvent _uiEvent, EAlphaKind _kind, Guid _uid)
		{
			switch (_kind)
			{
				case EAlphaKind.NONE:
					break;
				case EAlphaKind.PORTFOLIO:
					break;
				case EAlphaKind.CURRENCY:
					break;
				case EAlphaKind.USER:
					break;
				default:
					throw new ArgumentOutOfRangeException("_kind");
			}
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
