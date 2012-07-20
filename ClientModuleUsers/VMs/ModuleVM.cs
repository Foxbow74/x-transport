using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AlphaStudioCore;
using AlphaXTransport;
using ClientCommonWpf;
using XTransport;

namespace ClientModuleUsers.VMs
{
	class ModuleVM : AlphaVM, IRoot
	{
		public override EAlphaKind Kind
		{
			get { return EAlphaKind.NONE; }
		}

		[X((int)EAlphaKind.USER)]
		private IList<UserVM> m_users;

		protected override void InstantiationFinished()
		{
			base.InstantiationFinished();
			Users = CreateObservableCollection(m_users);

			AddNewUserCommand  = new RelayCommand(AddNewUser);
		}

		private void AddNewUser(object _obj)
		{
			m_users.Add(new UserVM(){Name = "Haha"});
		}

		public ReadOnlyObservableCollection<UserVM> Users { get; private set; }

		public RelayCommand AddNewUserCommand { get; private set; }
	}
}
