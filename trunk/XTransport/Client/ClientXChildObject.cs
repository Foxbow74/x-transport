using System;

namespace XTransport.Client
{
	public abstract class ClientXChildObject<TKind, TParent> : ClientXObject<TKind>, IClientXChildObject<TKind>, IXClientUserInternal<TKind>
		where TParent : ClientXObject<TKind>
	{
		private AbstractXClient<TKind> m_client;

		private Guid m_parentUid;
		public TParent Parent
		{
			get { return m_client.Get<TParent>(m_parentUid); }
		}

		#region IClientXChildObject<TKind> Members

		void IClientXChildObject<TKind>.SetParent(Guid _collectionOwner)
		{
			m_parentUid = _collectionOwner;
		}

		#endregion

		public void SetClient(AbstractXClient<TKind> _client)
		{
			m_client = _client;
		}
	}
}