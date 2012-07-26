using System;

namespace XTransport.Server
{
	internal delegate void ServerObjectSaved(int _kind, Guid _uid, SessionId _sessionId);
}