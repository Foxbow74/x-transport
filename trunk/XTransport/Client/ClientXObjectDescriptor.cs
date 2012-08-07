using System;
using System.Collections.Generic;
using System.Linq;
using XTransport.Server;

namespace XTransport.Client
{
	internal class ClientXObjectDescriptor<TKind>
	{
		private readonly AbstractXClient<TKind> m_client;

		private readonly Dictionary<Type, ClientXObject<TKind>> m_instances = new Dictionary<Type, ClientXObject<TKind>>();

		private readonly Dictionary<IClientXObject<TKind>, int> m_instancesCounter = new Dictionary<IClientXObject<TKind>, int>();

		private int m_kind;
		private bool m_notInitialized = true;
		private ServerXReport m_report;

		public ClientXObjectDescriptor(Guid _uid, AbstractXClient<TKind> _client, Guid _collectionOwnerUid)
		{
#if DEBUG
			if (_uid == Guid.Empty)
			{
				throw new NotImplementedException("??? why");
			}
#endif
			CollectionOwnerUid = _collectionOwnerUid;
			Uid = _uid;
			m_client = _client;
		}

		/// <summary>
		/// Вызывается для создания дескриптора для нового объекта
		/// </summary>
		/// <param name="_newBorn"></param>
		/// <param name="_client"></param>
		/// <param name="_kindId"></param>
		/// <param name="_parentUid"></param>
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
			ResetState();
			_newBorn.Changed += XObjectChanged;
			var changes = _newBorn.GetChanges();
			if (changes.Any())
			{
				Report = new ServerXReport(_newBorn.Uid, _newBorn.GetChanges(), 0, m_client.KindToIntInternal(_newBorn.Kind), EState.SINGLE);
			}
		}

		private EState m_state;
		public EState State
		{
			get
			{
				if(m_state==EState.UNKNOWN)
				{
					m_state = m_instances.Values.SelectMany(_clientXObject => _clientXObject.GetChildUids()).Aggregate(m_report == null ? EState.SINGLE : Report.State, (_current, _uid) => _current | m_client.GetDescriptor(_uid).State);
				}
				return m_state;
			}
		}

		public void ResetState()
		{
			m_state = EState.UNKNOWN;
			if (!CollectionOwnerUid.Equals(Guid.Empty))
			{
				m_client.ClearState(CollectionOwnerUid);
			}
		}

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
				ResetState();
			}
		}

		public bool IsEmpty
		{
			get { return m_instances.Count == 0; }
		}

		public bool IsUndoEnabled
		{
			get { return State.HasFlag(EState.UNDO_ABLE); }
		}

		public bool IsRedoEnabled
		{
			get { return State.HasFlag(EState.REDO_ABLE); }
		}

		public bool IsRevertEnabled
		{
			get { return State.HasFlag(EState.REVERT_ABLE); }
		}

		public Guid Uid { get; private set; }

		public Guid CollectionOwnerUid { get; private set; }

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
					child.SetParent(CollectionOwnerUid);
				}
				result.SetUid(Report.Uid);
				result.SetClientInternal(m_client);
				result.ApplyChanges(Report, true);
				result.Changed += XObjectChanged;
				m_instances.Add(type, result);
				m_instancesCounter.Add(result, 1);
				result.OnInstantiationFinished();
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
			var xObject = ((ClientXObject<TKind>) _xObject);
			var changes = xObject.GetChanges().ToArray();

			var xReport = new XReport(_xObject.Uid, changes, Kind, EState.UNDO_ABLE|EState.REVERT_ABLE);

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
		}

		public void ServerObjectSaved(bool _local)
		{
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
		}

		public void Revert()
		{
			foreach (var obj in m_instances.Values)
			{
				obj.Changed -= XObjectChanged;
				obj.Revert();
				obj.Changed += XObjectChanged;
			}
			Report = null;
		}

		public void Undo(UndoXReport _report)
		{
			if (_report.NeedRevert)
			{
				foreach (var obj in m_instances.Values)
				{
					obj.Changed -= XObjectChanged;
					obj.Revert();
					obj.Changed += XObjectChanged;
				}
			}
			else
			{
				foreach (var obj in m_instances.Values)
				{
					obj.Changed -= XObjectChanged;
					obj.ApplyChanges(_report, false);
					obj.Changed += XObjectChanged;
				}
			}
			Report = _report;
		}

		public void Redo(ServerXReport _report)
		{
			foreach (var obj in m_instances.Values)
			{
				obj.Changed -= XObjectChanged;
				obj.ApplyChanges(_report, false);
				obj.Changed += XObjectChanged;
			}
			Report = _report;
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
	}
}