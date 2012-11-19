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
		private readonly Dictionary<TKind, List<TKind>> m_abstractRootKindMap = new Dictionary<TKind, List<TKind>>();

		private readonly Dictionary<Guid, ClientXObjectDescriptor<TKind>> m_descriptors =
			new Dictionary<Guid, ClientXObjectDescriptor<TKind>>();

		private readonly SessionId m_sessionId;

		private readonly IXTransportContract m_transport;
		private int[] m_abstractKinds = new int[0];
		private ClientXObjectDescriptor<TKind> m_root;

		protected AbstractXClient()
		{
			SetAbstractRootKindMap(GetAbstractRootKindMap());
			m_transport = new XTransportClient();
			m_sessionId = m_transport.Login(UserUid);
			m_transport.ServerObjectSaved += OnServerObjectSaved;
		}

		public abstract Guid UserUid { get; }

		internal ClientXObjectDescriptor<TKind> RootDescriptor
		{
			get
			{
				if (m_root == null)
				{
					var uid = m_transport.GetRootUid();
					var descriptor = new ClientXObjectDescriptor<TKind>(uid, this, default(Guid));
					m_descriptors.Add(uid, descriptor);
					m_root = descriptor;
				}
				return m_root;
			}
		}

		protected abstract IEnumerable<KeyValuePair<TKind, TKind>> GetAbstractRootKindMap();

		internal void ClientObjectChanged(XReport _xReport)
		{
			if (_xReport.Uid == RootDescriptor.Uid)
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
			if (m_descriptors.TryGetValue(_uid, out descriptor))
			{
				return descriptor;
			}
			throw new ApplicationException();
		}

		internal void DeleteObject(ClientXObject<TKind> _child, Guid _collectionOwnerUid)
		{
			if (_collectionOwnerUid.Equals(m_root.Uid))
			{
				m_root.RemovedFromCollection(_child);
			}
			Release(_child);
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
			if (report.Uid == RootDescriptor.Uid)
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
				ObjectReleased(_object.Uid, _object.Kind);
			}
		}

		protected abstract void ObjectReleased(Guid _uid, TKind _kind);

		internal TO GetInternal<TO>(Guid _uid, IXObjectFactory<TKind> _factory) where TO : ClientXObject<TKind>
		{
			if (_factory == null && typeof (TO).IsAbstract)
			{
				var xFactoryAttribute = typeof(TO).GetCustomAttributes(typeof(XFactoryAttribute), true).Cast<XFactoryAttribute>().SingleOrDefault();
				if (xFactoryAttribute == null)
				{
					throw new ApplicationException("Can't instantiate abstract type");
				}
				_factory = (IXObjectFactory<TKind>)Activator.CreateInstance(xFactoryAttribute.FactoryType);
			}

			ClientXObjectDescriptor<TKind> descriptor;
			if (m_descriptors.TryGetValue(_uid, out descriptor))
			{
				return descriptor.Get<TO>(_factory);
			}
			var collectionOwnerUid = m_transport.GetСollectionOwnerUid(_uid);
			descriptor = new ClientXObjectDescriptor<TKind>(_uid, this, collectionOwnerUid);
			m_descriptors.Add(_uid, descriptor);
			var result = descriptor.Get<TO>(_factory);
			return result;
		}

		public TO Get<TO>(Guid _uid) where TO : ClientXObject<TKind>
		{
			return GetInternal<TO>(_uid, null);
		}

		internal TO GetInternal<TO>(Guid _uid, IXObjectFactory<TKind> _factory, Guid _collectionOwnerUid)
			where TO : ClientXObject<TKind>
		{
			ClientXObjectDescriptor<TKind> descriptor;
			TO result;
			if (!m_descriptors.TryGetValue(_uid, out descriptor))
			{
				descriptor = new ClientXObjectDescriptor<TKind>(_uid, this, _collectionOwnerUid);
				m_descriptors.Add(_uid, descriptor);
				result = descriptor.Get<TO>(_factory);
			}
			else
			{
				if (!descriptor.ContainsInstanceOf(out result))
				{
					result = descriptor.Get<TO>(_factory);
				}
			}
			return result;
		}

		internal void RegisterNewItem(ClientXObject<TKind> _child, Guid _collectionOwnerUid, int _fieldId)
		{
			var ownerDescriptor = GetDescriptor(_collectionOwnerUid);
			ClientXObjectDescriptor<TKind> childDescriptor;
			if (m_descriptors.TryGetValue(_child.Uid, out childDescriptor))
			{
				if(!childDescriptor.CollectionOwnerUid.Equals(_collectionOwnerUid))
				{
					throw new ApplicationException("Object owned by another parent");
				}
				ProcessAddedToCollection(_child, ownerDescriptor, _fieldId);
				return;
				//throw new ApplicationException("Object is not unique");
			}

			var kindId = KindToInt(_child.Kind);
			var report = new XReport(_child.Uid, _child.GetChanges(), kindId, EState.SINGLE);
			m_transport.AddNew(report, m_sessionId, _collectionOwnerUid);

			var descriptor = new ClientXObjectDescriptor<TKind>(_child, this, kindId, _collectionOwnerUid);
			m_descriptors.Add(_child.Uid, descriptor);

			ProcessAddedToCollection(_child, ownerDescriptor, _fieldId);
			_child.SetClientInternal(this);
			_child.OnInstantiationFinished();
		}

		private void ProcessAddedToCollection(ClientXObject<TKind> _child, ClientXObjectDescriptor<TKind> ownerDescriptor, int _fieldId)
		{
			List<TKind> alsoKnownAsList;
			if (ownerDescriptor == RootDescriptor && m_abstractRootKindMap.TryGetValue(_child.Kind, out alsoKnownAsList))
			{
				var alsoKnownAs = new List<TKind>(alsoKnownAsList) {_child.Kind};
				ownerDescriptor.AddedToCollection(_child, alsoKnownAs.Select(KindToInt));
			}
			else
			{
				ownerDescriptor.AddedToCollection(_child, new[] {_fieldId});
			}
		}

		public TO GetRoot<TO>() where TO : ClientXObject<TKind>
		{
			var descriptor = RootDescriptor;
			var result = descriptor.Get<TO>(null);
			return result;
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

		public void Save(ClientXObject<TKind> _obj)
		{
			Save(_obj.Uid);
		}

		public void Undo(ClientXObject<TKind> _obj)
		{
			Undo(_obj.Uid);
		}

		public void Redo(ClientXObject<TKind> _obj)
		{
			Redo(_obj.Uid);
		}

		public void Revert(ClientXObject<TKind> _obj)
		{
			Revert(_obj.Uid);
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

		public bool GetIsRevertEnabled(ClientXObject<TKind> _obj)
		{
			return GetIsRevertEnabled(_obj.Uid);
		}

		public bool GetIsRevertEnabled(Guid _uid)
		{
			ClientXObjectDescriptor<TKind> descriptor;
			if (m_descriptors.TryGetValue(_uid, out descriptor))
			{
				return descriptor.IsRevertEnabled;
			}
			return false;
		}

		public bool GetIsRedoEnabled(ClientXObject<TKind> _obj)
		{
			return GetIsRedoEnabled(_obj.Uid);
		}

		public bool GetIsRedoEnabled(Guid _uid)
		{
			ClientXObjectDescriptor<TKind> descriptor;
			if (m_descriptors.TryGetValue(_uid, out descriptor))
			{
				return descriptor.IsRedoEnabled;
			}
			return false;
		}

		public bool GetIsUndoEnabled(ClientXObject<TKind> _obj)
		{
			return GetIsUndoEnabled(_obj.Uid);
		}

		public bool GetIsUndoEnabled(Guid _uid)
		{
			ClientXObjectDescriptor<TKind> descriptor;
			if (m_descriptors.TryGetValue(_uid, out descriptor))
			{
				return descriptor.IsUndoEnabled;
			}
			return false;
		}

		internal void ClearParentState(Guid _parentUid, bool _isDirty)
		{
			ClientXObjectDescriptor<TKind> descriptor;
			if (m_descriptors.TryGetValue(_parentUid, out descriptor))
			{
				descriptor.ResetState();
				descriptor.UpdateDirty(_isDirty);	
			}
		}

		private void SetAbstractRootKindMap(IEnumerable<KeyValuePair<TKind, TKind>> _map)
		{
			m_abstractRootKindMap.Clear();
			m_abstractKinds = new int[0];
			var keys = _map.Select(_pair => _pair.Key).Distinct().ToList();
			var values = _map.Select(_pair => _pair.Value).Distinct().ToArray();
			foreach (var kind in keys)
			{
				if (values.Contains(kind))
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