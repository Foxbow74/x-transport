using System;

namespace XTransport.Server
{
	internal class Session
	{
		private static int m_sessions;

		public Session(Guid _userUid)
		{
			UserUid = _userUid;
			SessionId = new SessionId(++m_sessions);
			IsAlive = true;
		}

		public Guid UserUid { get; private set; }
		public SessionId SessionId { get; private set; }
		public bool IsAlive { get; private set; }

		public void Resurrect()
		{
			IsAlive = true;
		}
	}
}