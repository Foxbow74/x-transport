using System.Collections.Generic;
using System.Collections.ObjectModel;
using AlphaStudioCore;
using AlphaXTransport;
using ClientCommonWpf;
using XTransport;

namespace ClientModuleUsers.VMs
{
	internal class UsersBrowserVM : AlphaVM
	{
		private UserLinkVM m_selectedItem;

		[X((int) EAlphaKind.USER)]
		private ICollection<UserLinkVM> m_users;

		public override EAlphaKind Kind
		{
			get { return EAlphaKind.NONE; }
		}

		public UserLinkVM SelectedItem
		{
			get { return m_selectedItem; }
			set
			{
				if (m_selectedItem == value) return;
				m_selectedItem = value;
				if (value != null)
				{
					UiManager.CastUiMessage(EUiEvent.SELECTED, EAlphaKind.USER, value.Uid);
				}
				OnPropertyChanged(() => SelectedItem);
			}
		}

		public RelayCommand OpenCommand { get; private set; }

		public ReadOnlyObservableCollection<UserLinkVM> Users { get; private set; }

		public RelayCommand AddNewUserCommand { get; private set; }
		public RelayCommand DeleteUserCommand { get; private set; }

		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
			Users = CreateObservableCollection(m_users);

			AddNewUserCommand = new RelayCommand(AddNewUser);
			DeleteUserCommand = new RelayCommand(DeleteUser, _o => SelectedItem!=null);
			OpenCommand = new RelayCommand(ExecuteOpen);
		}

		private void DeleteUser(object _obj)
		{
			m_users.Remove(SelectedItem);
		}

		private void ExecuteOpen(object _obj)
		{
			SelectedItem.OpenCommand.Execute(null);
		}

		private void AddNewUser(object _obj)
		{
			var userLinkVM = new UserLinkVM {Name = "Aha..."};
			m_users.Add(userLinkVM);
		}
	}
}