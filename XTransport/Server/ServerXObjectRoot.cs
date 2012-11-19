using System;
using System.Collections.Generic;
using System.Linq;
using XTransport.Server.Storage;

namespace XTransport.Server
{
	internal class ServerXObjectRoot : ServerXObjectContainer
	{
		public ServerXObjectRoot(Guid _uid)
			: base(-1, _uid)
		{
		}

		public void Add(int _kind, Guid _xObjectUid)
		{
			RegisterObject(_kind, _xObjectUid);
		}

		private void RegisterObject(int _kind, Guid _xObjectUid)
		{
			ServerXList list;
			IServerXValue value;
			if (!XValues.TryGetValue(_kind, out value))
			{
				list = new ServerXList();
				XValues.Add(_kind, list);
			}
			else
			{
				list = (ServerXList) value;
			}
			list.AddChildUid(_xObjectUid);
		}

		internal override bool SaveChildren(AbstractXServer _server, SessionId _sessionId, IStorage _storage, DateTime _now)
		{
			var toSave = new List<Guid>();
			foreach (var pair in XValues)
			{
				var list = pair.Value as ServerXList;
				if (list != null)
				{
					toSave.AddRange(list.GetGuids());
				}
			}
			var saved = false;
			foreach (var uid in toSave.Distinct())
			{
                saved |= _server.SaveInternal(uid, _storage, _now, _sessionId);
			}
			return saved;
		}
	}
}