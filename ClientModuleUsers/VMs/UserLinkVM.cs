using System.Linq;
using AlphaStudioCore;
using AlphaXTransport;
using ClientCommonWpf;

namespace ClientModuleUsers.VMs
{
	class UserLinkVM : AlphaNamedVM
	{
		public static IAlphaDocumentDescriptor[] DocumentDescriptors { get; set; }
		public AlphaSpecificDocumentVM[] SpecificDocumentVMs { get; private set; }

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.USER; }
		}
		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
			SpecificDocumentVMs = DocumentDescriptors.Select(_descriptor => new AlphaSpecificDocumentVM(_descriptor, Uid)).ToArray();
			OpenCommand = new RelayCommand(OpenExecute, _o => true);
		}
		private void OpenExecute(object _o)
		{
			var byDefault = DocumentDescriptors.Single(_descriptor => _descriptor.IsDefault);
			UiManager.CastDocumentCommand(EDocumentCommand.OPEN, Kind, byDefault.DocKind, Uid);
		}

		public RelayCommand OpenCommand { get; private set; }
	}
}