using AlphaStudioCore;
using AlphaXTransport;
using ClientCommonWpf;
using System.Linq;

namespace ClientModuleBrowser.VMs
{
	class PortfolioLinkVM : AlphaNamedVM
	{
		public static IAlphaDocumentDescriptor[] DocumentDescriptors { get; set; }
		public AlphaSpecificDocumentVM[] SpecificDocumentVMs { get; private set; }

		public PortfolioLinkVM()
		{
			OpenCommand = new RelayCommand(OpenExecute, _o => true);
		}

		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
			SpecificDocumentVMs = DocumentDescriptors.Select(_descriptor => new AlphaSpecificDocumentVM(_descriptor, Uid)).ToArray();
		}

		private void OpenExecute(object _o)
		{
			var byDefault = DocumentDescriptors.Single(_descriptor => _descriptor.IsDefault);
			UiManager.CastDocumentCommand(EDocumentCommand.OPEN, Kind, byDefault.DocKind, Uid);
		}

		public RelayCommand OpenCommand { get; private set; }

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.PORTFOLIO; }
		}
	}
}