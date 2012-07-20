using System;
using System.Collections.Generic;
using System.ServiceModel;
using XTransport.Server;

namespace XTransport.WCF
{
	[ServiceContract(Name = "IXTransportContract", Namespace = "XTransport",
		CallbackContract = typeof (IXTransportCallback))]
	internal interface IXTransportContract
	{
		event ServerObjectSaved ServerObjectSaved;

		[OperationContract]
		void Subscribe();

		[OperationContract]
		void Save(Guid _uid, SessionId _sessionId);

		[OperationContract]
		IEnumerable<UndoXReport> Undo(Guid _uid, SessionId _sessionId);

		[OperationContract]
		IEnumerable<ServerXReport> Redo(Guid _uid, SessionId _sessionId);

		[OperationContract]
		void ClientObjectChanged(XReport _report, SessionId _sessionId);

		[OperationContract]
		void ClientObjectReverted(Guid _uid, SessionId _sessionId);

		[OperationContract]
		ServerXReport GetReport(int _kind, Guid _uid, SessionId _sessionId);

		[OperationContract]
		SessionId Login(Guid _userUid);

		[OperationContract]
		IEnumerable<int> AddNew(XReport _xReport, SessionId _sessionId, Guid _parentUid);

		[OperationContract]
		Guid GetRootUid();
	}
}