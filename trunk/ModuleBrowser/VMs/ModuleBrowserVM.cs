using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AlphaStudioCore;
using AlphaXTransport;
using ClientCommonWpf;
using XTransport;

namespace ClientModuleBrowser.VMs
{
	internal class ModuleBrowserVM : AlphaVM
	{
		[X((int)EAlphaKind.PORTFOLIO)]
		private IList<PortfolioLinkVM> m_portfolios;

		public RelayCommand OpenCommand { get; private set; }

		public ModuleBrowserVM()
		{
			UiManager.UIMessage += UiMangerUIMessage;
			OpenCommand = new RelayCommand(ExecuteOpen);
		}

		private void ExecuteOpen(object _o)
		{
			SelectedItem.OpenCommand.Execute(null);
		}

		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
			Items = CreateObservableCollection(m_portfolios);
		}

		void UiMangerUIMessage(EUiEvent _message, EAlphaKind _kind, Guid _arg)
		{
			if(_kind!=EAlphaKind.PORTFOLIO) return;

			switch (_message)
			{
				case EUiEvent.SELECTED:
					SelectedItem = Items.First(_vm => _vm.Uid == _arg);
					break;
				default:
					throw new ArgumentOutOfRangeException("_message");
			}
		}

		public ReadOnlyObservableCollection<PortfolioLinkVM> Items { get; private set; }

		private PortfolioLinkVM m_selectedItem;

		public PortfolioLinkVM SelectedItem
		{
			get { return m_selectedItem; }
			set
			{
				if(m_selectedItem==value) return;
				m_selectedItem = value;
				UiManager.CastUiMessage(EUiEvent.SELECTED, EAlphaKind.PORTFOLIO, value.Uid);
				OnPropertyChanged(()=>SelectedItem);
			}
		}

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.NONE; }
		}
	}
}