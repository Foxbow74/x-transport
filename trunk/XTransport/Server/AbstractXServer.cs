﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using XTransport.Server.Storage;
using XTransport.WCF;

namespace XTransport.Server
{
	internal delegate void ServerObjectSaved(int _kind, Guid _uid, SessionId _sessionId);

	public abstract class AbstractXServer
	{
		internal static AbstractXServer Instance;

		private readonly AutoResetEvent m_are = new AutoResetEvent(true);

		private readonly Dictionary<Guid, ServerXObjectContainer> m_objects = new Dictionary<Guid, ServerXObjectContainer>();

		private readonly Dictionary<SessionId, Session> m_sessions = new Dictionary<SessionId, Session>();

		private ServerXObjectRoot m_root;

		private ServerObjectSaved m_serverObjectSaved;

		protected AbstractXServer()
		{
			if (Instance != null)
				throw new Exception("Instance of Server Allready Exist");

			Instance = this;
			XTransportService.InitTransportHost();
		}

		protected virtual bool IsAsync
		{
			get { return true; }
		}

		internal ServerXObjectRoot Root
		{
			get { return m_are.GetSafely(ref m_root, CreateAndLoadRoot); }
		}

		protected virtual bool LoadAllOnStart
		{
			get { return false; }
		}

		internal SessionId Login(Guid _userUid)
		{
			var dead = m_sessions.Values.SingleOrDefault(_session => !_session.IsAlive && _session.UserUid == _userUid);
			if (dead != null)
			{
				dead.Resurrect();
				return dead.SessionId;
			}
			var ss = new Session(_userUid);
			m_sessions.Add(ss.SessionId, ss);
			return ss.SessionId;
		}

		internal void AddNew(XReport _xReport, SessionId _sessionId, Guid _parentUid)
		{
			var obj = new ServerXObjectContainer(_xReport.Kind, _xReport.Uid, default(DateTime));
			obj.FillFromClient(_xReport, _sessionId);
			m_objects.Add(_xReport.Uid, obj);
		}

		internal ServerXReport GetReport(int _kind, Guid _uid, SessionId _sessionId)
		{
			if (GetRootUid() == _uid) return Root.GetReport(_sessionId);

			ServerXObjectContainer xObject;
			if (!m_objects.TryGetValue(_uid, out xObject))
			{
				m_are.GetSafely(ref xObject, () => GetReportInner(_uid, out xObject));
			}
			return xObject.GetReport(_sessionId);
		}

		private ServerXObjectContainer GetReportInner(Guid _uid, out ServerXObjectContainer _xObject)
		{
			if (!m_objects.TryGetValue(_uid, out _xObject))
			{
				using (var st = CreateStorage())
				{
					int kind;
					var vfrom = st.LoadObjectParameters(_uid, out kind);
					_xObject = new ServerXObjectContainer(kind, _uid, vfrom);
					foreach (var record in st.LoadObject(_uid, DateTime.Now))
					{
						if (record is StorageChild)
						{
							var rt = (StorageChild) record;
							_xObject.AddChildren(rt.Field, rt.Uid);
						}
						else if (record is IStorageValueInternal)
						{
							var val = (IStorageValueInternal) record;
							_xObject.SetValue(val.Field, val);
						}
						else
						{
							throw new NotImplementedException();
						}
					}
				}
				m_objects.Add(_uid, _xObject);
			}
			return _xObject;
		}

		internal void ClientObjectChanged(XReport _report, SessionId _sessionId)
		{
			m_objects[_report.Uid].AddChanges(_sessionId, _report);
		}

		internal IEnumerable<UndoXReport> Undo(Guid _uid, SessionId _sessionId)
		{
			var dateTime = DateTime.MinValue;
			var candidates = new List<Guid>();
			GetAvailableUndoDate(_uid, _sessionId, this, ref dateTime, ref candidates);

			var reports = new List<UndoXReport>();
			foreach (var candidateUid in candidates.Distinct())
			{
				var serverXObjectContainer = m_objects[candidateUid];
				reports.Add(serverXObjectContainer.Undo(_sessionId));
			}
			return reports;
		}

		internal void GetAvailableUndoDate(Guid _uid, SessionId _sessionId, AbstractXServer _server, ref DateTime _dateTime, ref List<Guid> _candidates)
		{
			ServerXObjectContainer xObjectContainer;
			if (m_objects.TryGetValue(_uid, out xObjectContainer))
			{
				xObjectContainer.GetAvailableUndoDate(_sessionId, this, ref _dateTime, ref _candidates);
			}
		}

		internal IEnumerable<ServerXReport> Redo(Guid _uid, SessionId _sessionId)
		{
			var dateTime = DateTime.MaxValue;
			var candidates = new List<Guid>();
			GetAvailableRedoDate(_uid, _sessionId, this, ref dateTime, ref candidates);

			var reports = new List<ServerXReport>();
			foreach (var candidateUid in candidates.Distinct())
			{
				reports.Add(m_objects[candidateUid].Redo(_sessionId));
			}
			return reports;
		}

		internal void GetAvailableRedoDate(Guid _uid, SessionId _sessionId, AbstractXServer _server, ref DateTime _dateTime, ref List<Guid> _candidates)
		{
			ServerXObjectContainer xObjectContainer;
			if (m_objects.TryGetValue(_uid, out xObjectContainer))
			{
				xObjectContainer.GetAvailableRedoDate(_sessionId, this, ref _dateTime, ref _candidates);
			}
		}

		internal Guid GetRootUid()
		{
			return Root.Uid;
		}

		private ServerXObjectRoot CreateAndLoadRoot()
		{
#if DEBUG
			var root = new ServerXObjectRoot(new Guid("00000000-0000-0000-0000-0123456789AB"));
#else
			var root = new ServerXObjectRoot(Guid.NewGuid());
#endif

			using (var st = CreateStorage())
			{
				foreach (var record in st.LoadRoot())
				{
					var rt = record;
					root.Add(rt.Kind, rt.Uid, this);
				}
			}
			m_objects.Add(root.Uid, root);
			return root;
		}

		protected abstract IStorage CreateStorage();

		internal void ClientObjectReverted(Guid _uid, SessionId _sessionId)
		{
			m_objects[_uid].Revert(_sessionId);
		}

		internal bool Save(Guid _uid, SessionId _sessionId)
		{
			using (var st = CreateStorage())
			{
				using (st.CreateTransaction())
				{
					return SaveInternal(_uid, st, DateTime.Now, _sessionId);
				}
			}
		}

		internal bool SaveInternal(Guid _uid, IStorage _st, DateTime _now, SessionId _sessionId)
		{
			ServerXObjectContainer obj;
			if (!m_objects.TryGetValue(_uid, out obj))
			{
				return false;
			}

			if (obj.Stored == default(DateTime) && _uid != GetRootUid())
			{
				obj.Stored = _now;
				obj.StoredId = _st.InsertMain(obj.Uid, obj.Kind, _now);
			}
			var saved = obj.Save(_sessionId, _st, _now, this);
			saved = obj.SaveChildren(this, _sessionId, _st, _now) | saved;
			if (saved)
			{
				OnServerObjectSaved(_uid, _sessionId);
			}
			return saved;
		}

		internal bool SaveChild(Guid _childUid, Guid _ownerUid, SessionId _sessionId, IStorage _storage, int _field,
		                        DateTime _now)
		{
			if (_ownerUid == GetRootUid())
			{
				return SaveInternal(_childUid, _storage, _now, _sessionId);
			}
			ServerXObjectContainer child;
			if (!m_objects.TryGetValue(_childUid, out child))
			{
				//not loaded yet
				return false;
			}

			var saved = false;
			if (child.Stored == default(DateTime))
			{
				saved = true;
				child.Stored = _now;
				child.StoredId = _storage.InsertMain(child.Uid, child.Kind, _now, _ownerUid, _field);
			}
			saved = child.Save(_sessionId, _storage, _now, this) | saved;
			saved = child.SaveChildren(this, _sessionId, _storage, _now) | saved;
			if (saved)
			{
				OnServerObjectSaved(_childUid, _sessionId);
			}
			return saved;
		}

		internal event ServerObjectSaved ServerObjectSaved
		{
			add { m_serverObjectSaved += value; }
			remove { m_serverObjectSaved -= value; }
		}

		private void OnServerObjectSaved(Guid _uid, SessionId _sessionId)
		{
			Console.WriteLine("SAVED:" + _uid);
			if (m_serverObjectSaved != null)
			{
				if (IsAsync)
				{
					foreach (ServerObjectSaved d in m_serverObjectSaved.GetInvocationList())
					{
						d.BeginInvoke(m_objects[_uid].Kind, _uid, _sessionId, null, null);
					}
				}
				else
				{
					m_serverObjectSaved(m_objects[_uid].Kind, _uid, _sessionId);
				}
			}
		}

		public virtual void Reset()
		{
			m_serverObjectSaved = null;
			m_root = null;
			m_objects.Clear();
			if (LoadAllOnStart)
			{
				LoadAll();
			}
		}

		private void LoadAll()
		{
			using (var st = CreateStorage())
			{
				foreach (var record in st.LoadAll())
				{
					if (record is StorageObject)
					{
						var rt = (StorageObject) record;
						var obj = new ServerXObjectContainer(rt.Kind, rt.Uid, rt.ValidFrom);
						m_objects.Add(rt.Uid, obj);
						if (record is StorageChild)
						{
							var chd = (StorageChild) record;
							m_objects[chd.Parent].AddChildren(chd.Field, chd.Uid);
						}
					}
					else if (record is IStorageValueInternal)
					{
						var val = (IStorageValueInternal)record;
						m_objects[val.Owner].SetValue(val.Field, val);
					}
					else
					{
						throw new NotImplementedException();
					}
				}
			}
		}

		internal void Delete(IStorage _storage, Guid _uid, int _field, DateTime _now)
		{
			_storage.Delete(_uid, _field, _now);
		}
	}
}