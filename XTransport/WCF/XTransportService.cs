using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Xml;
using XTransport.Server;

namespace XTransport.WCF
{
	[ServiceBehavior(MaxItemsInObjectGraph = Int32.MaxValue, InstanceContextMode = InstanceContextMode.PerCall,
		ConcurrencyMode = ConcurrencyMode.Multiple)]
	internal class XTransportService : IXTransportContract
	{
		private IXTransportCallback m_callback;

		internal XTransportService()
		{
			AbstractXServer.Instance.ServerObjectSaved += OnServerObjectSaved;
		}

		internal static IXTransportContract Contract
		{
			get
			{
				if (AbstractXServer.Instance == null)
					throw new NullReferenceException();
				if (!XTransportConfiguration.UseDirectAccess)
					throw new ApplicationException("Direct access to service isn't allowed");
				return new XTransportService();
			}
		}

		private AbstractXServer Implementation
		{
			get { return AbstractXServer.Instance; }
		}

		#region IXTransportContract Members

		public event ServerObjectSaved ServerObjectSaved;

		public void Subscribe()
		{
			m_callback = OperationContext.Current.GetCallbackChannel<IXTransportCallback>();
		}

		public void Save(Guid _uid, SessionId _sessionId)
		{
			Implementation.Save(_uid, _sessionId);
		}

		public IEnumerable<UndoXReport> Undo(Guid _uid, SessionId _sessionId)
		{
			return Implementation.Undo(_uid, _sessionId);
		}

		public IEnumerable<ServerXReport> Redo(Guid _uid, SessionId _sessionId)
		{
			return Implementation.Redo(_uid, _sessionId);
		}

		public void ClientObjectChanged(XReport _report, SessionId _sessionId)
		{
			Implementation.ClientObjectChanged(_report, _sessionId);
		}

		public void ClientObjectReverted(Guid _uid, SessionId _sessionId)
		{
			Implementation.ClientObjectReverted(_uid, _sessionId);
		}

		public ServerXReport GetReport(int _kind, Guid _uid, SessionId _sessionId)
		{
			return Implementation.GetReport(_kind, _uid, _sessionId);
		}

		public SessionId Login(Guid _userUid)
		{
			return Implementation.Login(_userUid);
		}

		public IEnumerable<int> AddNew(XReport _xReport, SessionId _sessionId, Guid _parentUid)
		{
			return Implementation.AddNew(_xReport, _sessionId, _parentUid);
		}

		public Guid GetRootUid()
		{
			return Implementation.GetRootUid();
		}

		#endregion

		public static void InitTransportHost()
		{
			if (XTransportConfiguration.UseDirectAccess)
				return;

			var host = new ServiceHost(typeof (XTransportService),
			                           new UriBuilder("net.tcp", "localhost", 9010, XTransportConfiguration.ServiceName).Uri);
			var binding = new NetTcpBinding {Security = new NetTcpSecurity {Mode = SecurityMode.None}};
			host.AddServiceEndpoint(typeof (IXTransportContract), binding, "Transport");
			var thServer = new Thread(host.Open) {IsBackground = true};
			thServer.Start();
			Thread.Sleep(1000);
		}

		private void OnServerObjectSaved(int _kind, Guid _uid, SessionId _sessionId)
		{
			if (ServerObjectSaved != null)
			{
				ServerObjectSaved(_kind, _uid, _sessionId);
			}
			if (m_callback != null)
			{
				try
				{
					m_callback.ObjectSaved(_kind, _uid, _sessionId);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					throw;
				}
			}
		}

		private T CheckSerialization<T>(T _dataContract)
		{
			var ser = new DataContractSerializer(typeof (T));
			var sb = new StringBuilder();
			using (var writer = XmlWriter.Create(sb, new XmlWriterSettings {OmitXmlDeclaration = true}))
			{
				ser.WriteObject(writer, _dataContract);
			}
			var xml = sb.ToString();
			var resultSerializer = new DataContractSerializer(typeof (T));
			T deserializedReport;
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				deserializedReport = (T) resultSerializer.ReadObject(stream);
			}
			return deserializedReport;
		}
	}
}