using System;
using System.ServiceModel;
using XTransport.Server;

namespace XTransport.WCF
{
	[ServiceContract(Name = "IXTransportCallback", Namespace = "XTransport")]
	internal interface IXTransportCallback
	{
		[OperationContract]
		void ObjectSaved(Int32 _kind, Guid _uid, SessionId _sessionId);
	}
}