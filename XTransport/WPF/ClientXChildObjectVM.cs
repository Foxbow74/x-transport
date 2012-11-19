using System;
using XTransport.Client;

namespace XTransport.WPF
{
	public abstract class ClientXChildObjectVM<TKind, TParent> : ClientXObjectVM<TKind>, IClientXChildObject<TKind>
		where TParent : ClientXObjectVM<TKind>
	{
		private Guid m_parentUid;
		public TParent Parent
		{
			get { return Client.Get<TParent>(m_parentUid); }
		}


		#region IClientXChildObject<TKind> Members

		ClientXObject<TKind> IClientXChildObject<TKind>.Parent { get { return Client.Get<TParent>(m_parentUid); } }

		void IClientXChildObject<TKind>.SetParent(Guid _collectionOwner)
		{
			m_parentUid = _collectionOwner;
		}

		#endregion
	}
}