using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using XTransport.Server;
using XTransport.WCF;

namespace XTransport.Client
{
	public abstract class AbstractXClient<TKind>
	{
		private readonly Dictionary<Guid, ClientXObjectDescriptor<TKind>> m_descriptors =
			new Dictionary<Guid, ClientXObjectDescriptor<TKind>>();

		private readonly SessionId m_sessionId;

		private readonly IXTransportContract m_transport;
		private ClientXObjectDescriptor<TKind> m_root;

		protected AbstractXClient()
		{
			m_transport = new XTransportClient();
			m_sessionId = m_transport.Login(UserUid);
			m_transport.ServerObjectSaved += OnServerObjectSaved;
		}

		public abstract Guid UserUid { get; }

		internal void ClientObjectChanged(XReport _xReport)
		{
			m_transport.ClientObjectChanged(_xReport, m_sessionId);
		}

		internal ClientXObjectDescriptor<TKind> GetDescriptor(Guid _uid)
		{
			ClientXObjectDescriptor<TKind> descriptor;
			if (!m_descriptors.TryGetValue(_uid, out descriptor))
			{
				descriptor = new ClientXObjectDescriptor<TKind>(_uid, this);
				m_descriptors.Add(_uid, descriptor);
			}
			return descriptor;
		}

		internal ClientXObjectDescriptor<TKind> GetRootDescriptor()
		{
			if (m_root == null)
			{
				var uid = m_transport.GetRootUid();
				var descriptor = new ClientXObjectDescriptor<TKind>(uid, this);
				m_descriptors.Add(uid, descriptor);
				m_root = descriptor;
			}
			return m_root;
		}

		internal IEnumerable<int> AddIfNotExists(IClientXObjectInternal<TKind> _child, IClientXObjectInternal<TKind> _collectionOwner)
		{
			if (m_descriptors.ContainsKey(_child.Uid))
			{
				return Enumerable.Empty<int>();
			}
			var parentUid = _collectionOwner.Uid;
			if (_child is IClientXChildObject<TKind>)
			{
				((IClientXChildObject<TKind>) _child).SetParent(_collectionOwner);
			}
			_child.OnInstantiationFinished(this);
			var report = new XReport(_child.Uid, _child.GetChanges(), DateTime.Now, KindToInt(_child.Kind));
			var alsoKnownAs = m_transport.AddNew(report, m_sessionId, _collectionOwner.Uid);
			var descriptor = new ClientXObjectDescriptor<TKind>(_child.Uid, this);
			descriptor.SetParentUid(parentUid);
			m_descriptors.Add(_child.Uid, descriptor);
			descriptor.AddNew(_child, KindToInt(_child.Kind));
			return alsoKnownAs;
		}

		internal int KindToIntInternal(TKind _kind)
		{
			return KindToInt(_kind);
		}

		internal TKind IntToKindInternal(int _kind)
		{
			return IntToKind(_kind);
		}

		protected abstract int KindToInt(TKind _kind);
		protected abstract TKind IntToKind(int _kind);

		internal Dispatcher GetUiDispatcherInternal()
		{
			return GetUiDispatcher();
		}

		internal ServerXReport GetReport(int _kind, Guid _uid)
		{
			return m_transport.GetReport(_kind, _uid, m_sessionId);
		}

		protected virtual Dispatcher GetUiDispatcher()
		{
			throw new ApplicationException("Method GetUiDispatcher should be overrided in MVVM client");
		}

		public void Release(IClientXObject<TKind> _object)
		{
			var descriptor = GetDescriptor(_object.Uid);
			descriptor.Release(_object);
			if (descriptor.IsEmpty)
			{
				RefDeleted(_object);
			}
		}

		internal void RefDeleted(IClientXObject<TKind> _object)
		{
			ObjectReleased(_object.Uid, _object.Kind);
		}

		protected abstract void ObjectReleased(Guid _uid, TKind _kind);

		public TO Get<TO>(Guid _uid) where TO : IClientXObject<TKind>
		{
			var descriptor = GetDescriptor(_uid);
			var result = descriptor.Get<TO>(null);
			return result;
		}

		internal TO Get<TO>(Guid _uid, IXObjectFactory<TKind> _factory) where TO : IClientXObject<TKind>
		{
			var descriptor = GetDescriptor(_uid);
			var result = descriptor.Get<TO>(_factory);
			return result;
		}

		public TO GetRoot<TO>() where TO : IClientXObject<TKind>
		{
			var descriptor = GetRootDescriptor();
			var result = descriptor.Get<TO>(null);
			return result;
		}

		public TO Join<TO>(TO _xObject) where TO : IClientXObject<TKind>
		{
			((IClientXObjectInternal<TKind>) _xObject).OnInstantiationFinished(this);
			return _xObject;
		}

		private void OnServerObjectSaved(int _kind, Guid _uid, SessionId _sessionId)
		{
			ClientXObjectDescriptor<TKind> descriptor;
			if (m_descriptors.TryGetValue(_uid, out descriptor))
			{
				descriptor.ServerObjectSaved(_sessionId == m_sessionId);
			}
		}

		public void Save(Guid _uid)
		{
			m_transport.Save(_uid, m_sessionId);
		}

		public void Undo(Guid _uid)
		{
			foreach (var undoReport in m_transport.Undo(_uid, m_sessionId))
			{
				m_descriptors[undoReport.Uid].Undo(undoReport);
			}
		}

		public void Redo(Guid _uid)
		{
			foreach (var serverReport in m_transport.Redo(_uid, m_sessionId))
			{
				if (serverReport == null) continue;
				m_descriptors[serverReport.Uid].Redo(serverReport);
			}
		}

		public void Revert(Guid _uid)
		{
			m_descriptors[_uid].Revert();
			m_transport.ClientObjectReverted(_uid, m_sessionId);
		}

		public bool GetIsRevertEnabled(Guid _uid)
		{
			return m_descriptors[_uid].IsRevertEnabled;
		}

		public bool GetIsRedoEnabled(Guid _uid)
		{
			return m_descriptors[_uid].IsRedoEnabled;
		}

		public bool GetIsUndoEnabled(Guid _uid)
		{
			return m_descriptors[_uid].IsUndoEnabled;
		}

		public void ChildChanged(Guid _parentUid)
		{
			GetDescriptor(_parentUid).ChildChanged();
		}
	}
}