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
			SetAbstractRootKindMap(GetAbstractRootKindMap());
			m_transport = new XTransportClient();
			m_sessionId = m_transport.Login(UserUid);
			m_transport.ServerObjectSaved += OnServerObjectSaved;
		}

		protected abstract IEnumerable<KeyValuePair<TKind, TKind>> GetAbstractRootKindMap();

		public abstract Guid UserUid { get; }

		internal void ClientObjectChanged(XReport _xReport)
		{
			if(_xReport.Uid==GetRootDescriptor().Uid)
			{
				var toDel = _xReport.Items.Where(_item => m_abstractKinds.Contains(_item.FieldId)).ToArray();
				foreach (var item in toDel)
				{
					_xReport.Items.Remove(item);
				}
			}
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

		internal void AddIfNotExists(IClientXObjectInternal<TKind> _child, IClientXObjectInternal<TKind> _collectionOwner, int _fieldId)
		{
			if (m_descriptors.ContainsKey(_child.Uid))
			{
				return;
			}
			var parentUid = _collectionOwner.Uid;
			if (_child is IClientXChildObject<TKind>)
			{
				((IClientXChildObject<TKind>) _child).SetParent(_collectionOwner);
			}
			_child.OnInstantiationFinished(this);
			
			var kindId = KindToInt(_child.Kind);
			var report = new XReport(_child.Uid, _child.GetChanges(), DateTime.Now, kindId);

			m_transport.AddNew(report, m_sessionId, _collectionOwner.Uid);

			if (_collectionOwner.Uid.Equals(m_root.Uid))
			{
				var alsoKnownAs = new List<TKind>(m_abstractRootKindMap[_child.Kind]) { _child.Kind };
				m_root.AddedToCollection(_child, alsoKnownAs);
			}
			else
			{
				_collectionOwner.AddedToCollection(_child, _fieldId);
			}
			var descriptor = new ClientXObjectDescriptor<TKind>(_child, this, kindId, parentUid);
			m_descriptors.Add(_child.Uid, descriptor);
		}

		internal void RemovedFromCollection(IClientXObjectInternal<TKind> _child, IClientXObjectInternal<TKind> _collectionOwner)
		{
			if (_collectionOwner.Uid.Equals(m_root.Uid))
			{
				m_root.RemovedFromCollection(_child);
			}
			Release(_child);
			RefDeleted(_child);
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
			var report = m_transport.GetReport(_kind, _uid, m_sessionId);
			if(report.Uid==GetRootDescriptor().Uid)
			{
				#region add aggregated lists

				var lists = report.Items.OfType<XReportList>().ToList();
				var dict = new Dictionary<TKind, Dictionary<XReportItemState, List<XReportListItem>>>();
				foreach (var list in lists)
				{
					List<TKind> kinds;
					if (m_abstractRootKindMap.TryGetValue(IntToKind(list.FieldId), out kinds))
					{
						foreach (var kind in kinds)
						{
							Dictionary<XReportItemState, List<XReportListItem>> dictionary;
							if (!dict.TryGetValue(kind, out dictionary))
							{
								dictionary = new Dictionary<XReportItemState, List<XReportListItem>>();
								dict.Add(kind, dictionary);
							}
							List<XReportListItem> items;
							if (!dictionary.TryGetValue(list.State, out items))
							{
								items = new List<XReportListItem>(list.Items);
								dictionary.Add(list.State, items);
							}
							else
							{
								items.AddRange(list.Items);
							}
						}
					}
				}
				foreach (var kindPair in dict)
				{
					foreach (var stateAndList in kindPair.Value)
					{
						report.Items.Add(new XReportList(KindToInt(kindPair.Key), stateAndList.Key, stateAndList.Value));
					}
				}

				#endregion

			}
			return report;
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

		private int[] m_abstractKinds = new int[0];
		private readonly Dictionary<TKind, List<TKind>> m_abstractRootKindMap = new Dictionary<TKind, List<TKind>>();

		void SetAbstractRootKindMap(IEnumerable<KeyValuePair<TKind, TKind>> _map)
		{
			m_abstractRootKindMap.Clear();
			m_abstractKinds = new int[0];
			var keys = _map.Select(_pair => _pair.Key).Distinct().ToList();
			var values = _map.Select(_pair => _pair.Value).Distinct().ToArray();
			foreach (var kind in keys)
			{
				if(values.Contains(kind))
				{
					throw new ApplicationException("Map key " + kind + " can't value");
				}
			}
			m_abstractKinds = values.Select(KindToInt).ToArray();
			foreach (var kind in keys)
			{
				var kindCopy = kind;
				var list = _map.Where(_pair => _pair.Key.Equals(kindCopy)).Select(_pair => _pair.Value).Distinct().ToList();
				m_abstractRootKindMap.Add(kind, list);
			}
		}
	}
}