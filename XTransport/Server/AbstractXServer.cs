using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using XTransport.Server.Storage;
using XTransport.WCF;

namespace XTransport.Server
{
	public abstract class AbstractXServer
	{
		internal static AbstractXServer Instance;

		private readonly AutoResetEvent m_are = new AutoResetEvent(true);

		private readonly Dictionary<Guid, ServerXObjectContainer> m_objects = new Dictionary<Guid, ServerXObjectContainer>();
		private readonly Dictionary<Guid, Guid> m_parents = new Dictionary<Guid, Guid>();

		private readonly Dictionary<SessionId, Session> m_sessions = new Dictionary<SessionId, Session>();
		private uint m_generation = 2;

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

		internal uint AddNew(XReport _report, SessionId _sessionId, Guid _parentUid)
		{
			_report.ActualFrom = NextGeneration();
			var obj = new ServerXObjectContainer(_report.Kind, _report.Uid);
			obj.FillFromClient(_report, _sessionId);
			m_objects.Add(_report.Uid, obj);
			return _report.ActualFrom;
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
					var descriptor = st.LoadObjectCharacteristics(_uid);
					_xObject = new ServerXObjectContainer(_uid, descriptor);
					var records = st.LoadObject(_uid, DateTime.Now).ToList();
					foreach (var record in records)
					{
						if (record is StorageChild)
						{
							var rt = (StorageChild) record;
							_xObject.AddChildren(rt.Field, rt.Uid);
							m_parents[rt.Uid] = _xObject.Uid;
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

		public uint NextGeneration()
		{
			return m_generation++;
		}

		internal void ClientObjectChanged(XReport _report, SessionId _sessionId)
		{
			_report.ActualFrom = NextGeneration();
			m_objects[_report.Uid].AddChanges(_sessionId, _report);
		}

		internal IEnumerable<UndoXReport> Undo(Guid _uid, SessionId _sessionId)
		{
			var dateTime = uint.MinValue;
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

		internal void GetAvailableUndoDate(Guid _uid, SessionId _sessionId, AbstractXServer _server, ref uint _dateTime,
		                                   ref List<Guid> _candidates)
		{
			ServerXObjectContainer xObjectContainer;
			if (m_objects.TryGetValue(_uid, out xObjectContainer))
			{
				xObjectContainer.GetAvailableUndoDate(_sessionId, this, ref _dateTime, ref _candidates);
			}
		}

		internal IEnumerable<ServerXReport> Redo(Guid _uid, SessionId _sessionId)
		{
			var dateTime = uint.MaxValue;
			var candidates = new List<Guid>();
			GetAvailableRedoDate(_uid, _sessionId, this, ref dateTime, ref candidates);

			var reports = new List<ServerXReport>();
			foreach (var candidateUid in candidates.Distinct())
			{
				reports.Add(m_objects[candidateUid].Redo(_sessionId));
			}
			return reports;
		}

		internal void GetAvailableRedoDate(Guid _uid, SessionId _sessionId, AbstractXServer _server, ref uint _generation,
		                                   ref List<Guid> _candidates)
		{
			ServerXObjectContainer xObjectContainer;
			if (m_objects.TryGetValue(_uid, out xObjectContainer))
			{
				xObjectContainer.GetAvailableRedoDate(_sessionId, this, ref _generation, ref _candidates);
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
					root.Add(rt.Kind, rt.Uid);
					m_parents[rt.Uid] = root.Uid;
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

			if (obj.Stored == 0 && _uid != GetRootUid())
			{
				obj.Stored = obj.GetCurrentGeneration(_sessionId);
				obj.StoredId = _st.InsertMain(obj.Uid, obj.Kind, _now);
			}
			var saved = obj.Save(_sessionId, _st, _now, this);
			saved = obj.SaveChildren(this, _sessionId, _st, _now) | saved;
			OnServerObjectSaved(_uid, _sessionId);
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
			if (child.Stored == 0)
			{
				saved = true;
				child.ValidFrom = _now;
				child.Stored = 1;
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
		}

		internal void Delete(IStorage _storage, Guid _uid, int _field, DateTime _now)
		{
			ServerXObjectContainer objectContainer;
			if (m_objects.TryGetValue(_uid, out objectContainer))
			{
				objectContainer.Deleted(_now);
			}
			_storage.Delete(_uid, _field, _now);
		}

		public Guid GetСollectionOwnerUid(Guid _uid)
		{
			Guid result;
			if (!m_parents.TryGetValue(_uid, out result))
			{
				using (var st = CreateStorage())
				{
					result = st.GetCollectionOwnerUid(_uid);
				}
				m_parents[_uid] = result;
			}
			return result;
		}

		#region Nested type: ObjectDescriptor

		public struct ObjectDescriptor
		{
			private readonly int m_kind;

			private readonly DateTime m_validFrom;

			private readonly DateTime? m_validTill;

			public ObjectDescriptor(int _kind, DateTime _validFrom, DateTime? _validTill)
			{
				m_kind = _kind;
				m_validFrom = _validFrom;
				m_validTill = _validTill;
			}

			public int Kind
			{
				get { return m_kind; }
			}

			public DateTime ValidFrom
			{
				get { return m_validFrom; }
			}

			public DateTime? ValidTill
			{
				get { return m_validTill; }
			}
		}

		#endregion
	}
}