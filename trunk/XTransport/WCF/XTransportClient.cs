using System;
using System.Collections.Generic;
using System.ServiceModel;
using XTransport.Server;

namespace XTransport.WCF
{
	internal class XTransportClient : IXTransportContract
	{
		private readonly XTransportCallback m_callback;
		private IXTransportContract m_duplexChannel;
		private ChannelFactory<IXTransportContract> m_duplexChannelFactory;

		internal XTransportClient()
		{
			if (XTransportConfiguration.UseDirectAccess)
			{
				XTransportService.Contract.ServerObjectSaved += OnServerObjectSaved;
			}
			else
			{
				m_callback = new XTransportCallback();
				m_callback.OnServerObjectSaved += OnServerObjectSaved;
			}
		}

		internal IXTransportContract ServiceContract
		{
			get
			{
				if (XTransportConfiguration.UseDirectAccess)
					return XTransportService.Contract;
				if (m_duplexChannel == null)
				{
					if (m_duplexChannelFactory == null)
					{
						var uri =
							new UriBuilder("net.tcp", "localhost", 9010,
							               string.Format("{0}\\Transport", XTransportConfiguration.ServiceName)).Uri;
						var address = new EndpointAddress(uri);
						var binding = new NetTcpBinding {Security = new NetTcpSecurity {Mode = SecurityMode.None}};
						m_duplexChannelFactory = new DuplexChannelFactory<IXTransportContract>(m_callback, binding, address);
					}
					m_duplexChannel = m_duplexChannelFactory.CreateChannel();
					m_duplexChannel.Subscribe();
				}
				return m_duplexChannel;
			}
		}

		#region IXTransportContract Members

		public event ServerObjectSaved ServerObjectSaved;

		public void Subscribe()
		{
			ServiceContract.Subscribe();
		}

		public void Save(Guid _uid, SessionId _sessionId)
		{
			ServiceContract.Save(_uid, _sessionId);
		}

		public IEnumerable<UndoXReport> Undo(Guid _uid, SessionId _sessionId)
		{
			return ServiceContract.Undo(_uid, _sessionId);
		}

		public IEnumerable<ServerXReport> Redo(Guid _uid, SessionId _sessionId)
		{
			return ServiceContract.Redo(_uid, _sessionId);
		}

		public uint ClientObjectChanged(XReport _report, SessionId _sessionId)
		{
			return ServiceContract.ClientObjectChanged(_report, _sessionId);
		}

		public void ClientObjectReverted(Guid _uid, SessionId _sessionId)
		{
			ServiceContract.ClientObjectReverted(_uid, _sessionId);
		}

		public ServerXReport GetReport(int _kind, Guid _uid, SessionId _sessionId)
		{
			return ServiceContract.GetReport(_kind, _uid, _sessionId);
		}

		public SessionId Login(Guid _userUid)
		{
			return ServiceContract.Login(_userUid);
		}

		public uint AddNew(XReport _xReport, SessionId _sessionId, Guid _parentUid)
		{
			return ServiceContract.AddNew(_xReport, _sessionId, _parentUid);
		}

		public Guid GetRootUid()
		{
			return ServiceContract.GetRootUid();
		}

		public Guid GetСollectionOwnerUid(Guid _uid)
		{
			return ServiceContract.GetСollectionOwnerUid(_uid);
		}

		#endregion

		private void OnServerObjectSaved(int _kind, Guid _uid, SessionId _sessionId)
		{
			if (ServerObjectSaved != null)
			{
				ServerObjectSaved(_kind, _uid, _sessionId);
			}
		}

		#region Nested type: XTransportCallback

		[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
		internal class XTransportCallback : IXTransportCallback
		{
			#region IXTransportCallback Members

			public void ObjectSaved(Int32 _kind, Guid _uid, SessionId _sessionId)
			{
				if (OnServerObjectSaved != null)
				{
					OnServerObjectSaved(_kind, _uid, _sessionId);
				}
			}

			#endregion

			public event ServerObjectSaved OnServerObjectSaved;
		}

		#endregion
	}
}