using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XTransport.Server;

namespace XTransport.Client
{
	internal class ClientXObjectDescriptor<TKind>
	{
		private readonly AbstractXClient<TKind> m_client;

		private readonly Dictionary<Type, ClientXObject<TKind>> m_instances = new Dictionary<Type, ClientXObject<TKind>>();

		private readonly Dictionary<IClientXObject<TKind>, int> m_instancesCounter = new Dictionary<IClientXObject<TKind>, int>();

		private State m_currentState;

		private int m_kind;
		private bool m_notInitialized = true;
		private Guid m_collectionOwnerUid;
		private ServerXReport m_report;

		public ClientXObjectDescriptor(Guid _uid, AbstractXClient<TKind> _client, Guid _collectionOwnerUid)
		{
#if DEBUG
			if (_uid == Guid.Empty)
			{
				throw new NotImplementedException("??? why");
			}
#endif
			m_collectionOwnerUid = _collectionOwnerUid;
			Uid = _uid;
			m_client = _client;
		}

		public ClientXObjectDescriptor(ClientXObject<TKind> _newBorn, AbstractXClient<TKind> _client, int _kindId, Guid _parentUid)
			: this(_newBorn.Uid, _client, _parentUid)
		{
			var child = _newBorn as IClientXChildObject<TKind>;
			if (child != null)
			{
				child.SetParent(_parentUid);
			}
			m_instances.Add(_newBorn.GetType(), _newBorn);
			m_instancesCounter.Add(_newBorn, 1);
			Kind = _kindId;
			var now = DateTime.Now;
			_newBorn.Changed += XObjectChanged;
			var changes = _newBorn.GetChanges();
			if (changes.Any())
			{
				Report = new ServerXReport(_newBorn.Uid, _newBorn.GetChanges(), now, now, DateTime.MinValue,
										   m_client.KindToIntInternal(_newBorn.Kind));
			}
		}

		public DateTime ActualFrom { get; set; }
		public DateTime Stored { get; set; }
		public DateTime Loaded { get; set; }
		public DateTime LastModified { get; set; }

		private int Kind
		{
			get
			{
				if (m_notInitialized)
				{
					throw new ApplicationException("Kind is not initialized");
				}
				return m_kind;
			}
			set
			{
				m_kind = value;
				m_notInitialized = false;
			}
		}

		public ServerXReport Report
		{
			get
			{
				if (m_report == null)
				{
					Report = m_client.GetReport(Kind, Uid);
				}
				return m_report;
			}
			set
			{
				m_report = value;
				if (m_report != null)
				{
					LastModified = m_report.LastModification;
					Stored = m_report.StoredActualFrom;
					ActualFrom = Loaded = m_report.ActualFrom;
				}
			}
		}

		public bool IsEmpty
		{
			get { return m_instances.Count == 0; }
		}

		private State CurrentCurrentState
		{
			get
			{
				if (m_currentState == null)
				{
					Debug.WriteLine("CREATE STATE");
					m_currentState = new State(this);
				}
				return m_currentState;
			}
		}

		public bool IsUndoEnabled
		{
			get { return CurrentCurrentState.IsUndoEnabled; }
		}

		public bool IsRedoEnabled
		{
			get { return CurrentCurrentState.IsRedoEnabled; }
		}

		public bool IsRevertEnabled
		{
			get { return CurrentCurrentState.IsRevertEnabled; }
		}

		public Guid Uid { get; private set; }

		public bool ContainsInstanceOf<TO>(out TO _result) where TO : ClientXObject<TKind>
		{
			ClientXObject<TKind> value;
			if(m_instances.TryGetValue(typeof (TO), out value))
			{
				_result = (TO) value;
				return true;
			}
			_result = null;
			return false;
		}


		public TO Get<TO>(IXObjectFactory<TKind> _factory) where TO : ClientXObject<TKind>
		{
			ClientXObject<TKind> result;
			var type = typeof (TO);
			if (!m_instances.TryGetValue(type, out result))
			{
				if (_factory == null)
				{
					if (type.IsAbstract)
					{
						throw new ApplicationException("Can't instantiate abstract type, please init _factory arg");
					}

					result = (ClientXObject<TKind>) Activator.CreateInstance(typeof (TO));
					if (m_notInitialized)
					{
						Kind = m_client.KindToIntInternal(result.Kind);
					}
				}
				else
				{
					if (m_notInitialized)
					{
						Kind = m_client.KindToIntInternal(_factory.Kind);
					}
					result = (ClientXObject<TKind>) _factory.CreateInstance(m_client.IntToKindInternal(Report.Kind));
					var newType = result.GetType();
					ClientXObject<TKind> existed;
					if (m_instances.TryGetValue(newType, out existed))
					{
						m_instancesCounter[existed]++;
						return (TO) existed;
					}
					m_instances.Add(type, result);
					type = newType;
				}
				var child = result as IClientXChildObject<TKind>;
				if(child!=null)
				{
					child.SetParent(m_collectionOwnerUid);
				}
				result.SetUid(Report.Uid);
				result.OnInstantiationFinished(m_client);
				result.ApplyChanges(Report, true);
				result.Changed += XObjectChanged;
				m_instances.Add(type, result);
				m_instancesCounter.Add(result, 1);
			}
			else
			{
				m_instancesCounter[result]++;
			}
			return (TO) result;
		}

		public void Release(IClientXObject<TKind> _object)
		{
			var type = _object.GetType();
			if (!m_instancesCounter.ContainsKey(_object)) return;
			var rest = --m_instancesCounter[_object];
			if (rest == 0)
			{
				m_instances.Remove(type);
				m_instancesCounter.Remove(_object);
				if (_object is IDisposable)
				{
					((IDisposable) _object).Dispose();
				}
			}
			if (m_instances.Count == 0)
			{
				Report = null;
			}
		}

		private void XObjectChanged(IClientXObject<TKind> _xObject)
		{
			m_currentState = null;

			var xObject = ((ClientXObject<TKind>) _xObject);
			var changes = xObject.GetChanges().ToArray();

			var xReport = new XReport(_xObject.Uid, changes, DateTime.Now, Kind);

			ActualFrom = LastModified = xReport.ActualFrom;
			Report.MergeChanges(xReport);
			foreach (var obj in m_instances.Values)
			{
				if (!ReferenceEquals(obj, _xObject))
				{
					obj.Changed -= XObjectChanged;
					obj.ApplyChanges(xReport, false);
					obj.Changed += XObjectChanged;
				}
			}
			m_client.ClientObjectChanged(xReport);
			ClearState();
		}

		public void ServerObjectSaved(bool _local)
		{
			//get new report with ORIGINAL values, instead CHANGES
			Report = m_client.GetReport(Kind, Uid);

			if (_local)
			{
				foreach (var obj in m_instances.Values)
				{
					obj.SaveInternal();
				}
			}
			else
			{
				foreach (var obj in m_instances.Values)
				{
					obj.Changed -= XObjectChanged;
					obj.ApplyChanges(Report, false);
					obj.Changed += XObjectChanged;
				}
			}
			ClearState();
		}

		public void Revert()
		{
			ActualFrom = LastModified = Loaded = Stored;
			foreach (var obj in m_instances.Values)
			{
				obj.Changed -= XObjectChanged;
				obj.Revert();
				obj.Changed += XObjectChanged;
			}
			ClearState();
		}

		public void Undo(UndoXReport _xReport)
		{
			Debug.WriteLine("UNDO\t" + _xReport.ActualFrom + "\tUID\t" + _xReport.Uid + "\t[" + _xReport.Kind + "]");
			if (_xReport.ActualFrom == ActualFrom) return;
			if (_xReport.NeedRevert)
			{
				Debug.WriteLine("REVERT\tActualFrom=\t" + Stored);
				// Have no undo info, just revert existing changes
				ActualFrom = Loaded = Stored;
				foreach (var obj in m_instances.Values)
				{
					obj.Changed -= XObjectChanged;
					obj.Revert();
					obj.Changed += XObjectChanged;
				}
			}
			else
			{
				Debug.WriteLine("APPLY\tActualFrom=\t" + _xReport.ActualFrom);
				ActualFrom = _xReport.ActualFrom;
				foreach (var obj in m_instances.Values)
				{
					obj.Changed -= XObjectChanged;
					obj.ApplyChanges(_xReport, false);
					obj.Changed += XObjectChanged;
				}
			}
			ClearState();
		}

		public void Redo(ServerXReport _report)
		{
			ActualFrom = _report.ActualFrom;
			foreach (var obj in m_instances.Values)
			{
				obj.Changed -= XObjectChanged;
				obj.ApplyChanges(_report, false);
				obj.Changed += XObjectChanged;
			}
			ClearState();
		}

		public void ClearState()
		{
			m_currentState = null;
			if (!m_collectionOwnerUid.Equals(Guid.Empty))
			{
				m_client.ClearState(m_collectionOwnerUid);
			}
		}

		public void AddedToCollection(ClientXObject<TKind> _child, IEnumerable<int> _addAs)
		{
			foreach (var instance in m_instances.Values)
			{
				foreach (var addAs in _addAs)
				{
					instance.AddedToCollection(_child, addAs);
				}
			}
		}

		public void RemovedFromCollection(ClientXObject<TKind> _child)
		{
			foreach (var instance in m_instances.Values)
			{
				instance.RemovedFromCollection(_child);
			}
		}

		#region Nested type: State

		private class State
		{
			public State(ClientXObjectDescriptor<TKind> _descriptor)
			{
				IsUndoEnabled = _descriptor.ActualFrom > _descriptor.Loaded;
				IsRedoEnabled = _descriptor.ActualFrom < _descriptor.LastModified;
				IsRevertEnabled = _descriptor.Loaded < _descriptor.Stored || _descriptor.ActualFrom > _descriptor.Loaded;
				foreach (var clientXObjectInternal in _descriptor.m_instances.Values)
				{
					foreach (var uid in clientXObjectInternal.GetChildUids())
					{
						if (!IsUndoEnabled) IsUndoEnabled = _descriptor.m_client.GetIsUndoEnabled(uid);
						if (!IsRedoEnabled) IsRedoEnabled = _descriptor.m_client.GetIsRedoEnabled(uid);
						if (!IsRevertEnabled) IsRevertEnabled = _descriptor.m_client.GetIsRevertEnabled(uid);
					}
				}
			}

			public bool IsUndoEnabled { get; private set; }
			public bool IsRedoEnabled { get; private set; }
			public bool IsRevertEnabled { get; private set; }
		}

		#endregion
	}
}