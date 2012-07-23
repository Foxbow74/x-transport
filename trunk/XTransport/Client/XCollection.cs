using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace XTransport.Client
{
	internal sealed class XCollection<T, TKind> : AbstractXValue, ICollection<T>, IXCollection<TKind>
		where T : ClientXObject<TKind>
	{
		private readonly IXObjectFactory<TKind> m_factory;
		private readonly Dictionary<Guid, T> m_dict = new Dictionary<Guid, T>();
		private readonly List<T> m_original = new List<T>();
		private AbstractXClient<TKind> m_client;
		private bool m_isDirty;
		private bool m_isDirtyAndHaveReportItems;

		private ObservableCollection<T> m_observableCollection;

		private IClientXObjectInternal<TKind> m_owner;
		private Dispatcher m_uiDispatcher;
		private int m_fieldId;

		internal XCollection(IXObjectFactory<TKind> _factory)
		{
			m_factory = _factory;
		}

	 	internal XCollection()
		{
		}

		#region IList<T> Members

		public IEnumerator<T> GetEnumerator()
		{
			return m_dict.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T _item)
		{
			//m_dict.Add(_item.Uid, _item);
			AddedToCollection(_item);
			UpdateVM(() => m_observableCollection.Add(_item));
		}

		public void Clear()
		{
			if (m_dict.Count <= 0) return;
			var items = m_dict.ToArray();
			m_dict.Clear();
			foreach (var item in items)
			{
				RemovedFromCollection(item.Value);
			}
			OnChanged();
			UpdateVM(() => m_observableCollection.Clear());
		}

		public bool Contains(T _item)
		{
			return m_dict.ContainsKey(_item.Uid);
		}

		public void CopyTo(T[] _array, int _arrayIndex)
		{
			m_dict.Values.CopyTo(_array, _arrayIndex);
		}

		public bool Remove(T _item)
		{
			if (m_dict.Remove(_item.Uid))
			{
				RemovedFromCollection(_item);
				UpdateVM(() => m_observableCollection.Remove(_item));
				return true;
			}
			return false;
		}

		public int Count
		{
			get { return m_dict.Count; }
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IXCollection<TKind> Members

		void IXClientUserInternal<TKind>.SetClient(AbstractXClient<TKind> _client)
		{
			m_client = _client;
		}

		void IXCollection<TKind>.SetOwnerInfo(IClientXObjectInternal<TKind> _xObject, int _fieldId)
		{
			m_owner = _xObject;
			m_fieldId = _fieldId;
		}

		public IEnumerable<Guid> GetUids()
		{
			return m_dict.Keys;
		}

		void IXCollection<TKind>.AddSilently(IXObject<TKind> _item)
		{
			if (!m_dict.ContainsKey(_item.Uid))
			{
				m_dict.Add(_item.Uid, (T) _item);
				UpdateVM(() => m_observableCollection.Add((T) _item));
			}
		}

		void IXCollection<TKind>.RemoveSilently(Guid _uid)
		{
			T item;
			if (m_dict.TryGetValue(_uid, out item))
			{
				m_dict.Remove(_uid);
				UpdateVM(() => m_observableCollection.Remove((T) item));
			}
		}

		public override bool IsDirty
		{
			get { return m_isDirty; }
		}

		public override bool IsDirtyAndHaveReportItems
		{
			get { return m_isDirtyAndHaveReportItems; }
		}

		public override AbstractXReportItem GetXReportItem(int _xname)
		{
#if DEBUG
			if (!IsDirtyAndHaveReportItems)
			{
				throw new ApplicationException("Not dirty value");
			}
#endif
			var items = new List<XReportListItem>();
			items.AddRange(m_dict.Values.Except(m_original).Select(_arg => new XReportListItem(_arg.Uid, EReportListItemState.ADDED)));
			items.AddRange(m_original.Except(m_dict.Values).Select(_arg => new XReportListItem(_arg.Uid, EReportListItemState.REMOVED)));
			var rl = new XReportList(_xname, XReportItemState.CHANGE, items);
			return rl;
		}

		public override void ApplyChanges(AbstractXReportItem _reportItem, bool _firstTime)
		{
			var rl = (XReportList) _reportItem;
			switch (rl.State)
			{
				case XReportItemState.ORIGINAL:
					m_original.Clear();
					foreach (var item in rl.Items.Select(GetChild))
					{
						m_original.Add(item);
					}
					if (_firstTime)
					{
						m_dict.Clear();
						foreach (var item in m_original)
						{
							((IXCollection<TKind>)this).AddSilently(item);
						}
					}
					break;
				case XReportItemState.CHANGE:
					var items = m_dict.Values.ToArray();
					foreach (var item in items)
					{
						if (!m_original.Contains(item))
						{
							((IXCollection<TKind>)this).RemoveSilently(item.Uid);
							item.Changed -= ItemChanged;
							m_client.Release(item);
							m_client.RefDeleted(item);
						}
					}
					foreach (var item in m_original)
					{
						if (!items.Contains(item))
						{
							((IXCollection<TKind>)this).AddSilently(item);
						}
					}

					foreach (var item in rl.Items)
					{
						switch (item.State)
						{
							case EReportListItemState.ADDED:
								var add = GetChild(item);
								((IXCollection<TKind>)this).AddSilently(add);
								break;
							case EReportListItemState.REMOVED:
								((IXCollection<TKind>)this).RemoveSilently(item.Uid);
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			UpdateIsDirty();
		}

		public override void Revert()
		{
			var objects = m_dict.Values.ToArray();
			foreach (var item in objects)
			{
				if (!m_original.Contains(item))
				{
					((IXCollection<TKind>)this).RemoveSilently(item.Uid);
				}
			}
			foreach (var item in m_original)
			{
				if (!objects.Contains(item))
				{
					((IXCollection<TKind>)this).AddSilently(item);
				}
			}
			foreach (var uid in GetUids())
			{
				m_client.Revert(uid);
			}
		}

		public override void Save()
		{
			if (IsDirtyAndHaveReportItems)
			{
				m_original.Clear();
				m_original.AddRange(m_dict.Values);
			}
			m_isDirty = false;
			m_isDirtyAndHaveReportItems = false;
		}

		#endregion

		private void AddedToCollection(T _item)
		{
			m_client.AddIfNotExists(_item, m_owner, m_fieldId);
			_item.Changed += ItemChanged;
			OnChanged();
		}

		private void RemovedFromCollection(T _item)
		{
			m_client.RemovedFromCollection(_item, m_owner);
			_item.Changed -= ItemChanged;
			OnChanged();
		}

		private void UpdateVM(Action _func)
		{
			if (m_observableCollection != null)
			{
				if (m_uiDispatcher == Dispatcher.CurrentDispatcher)
				{
					_func();
				}
				else
				{
					m_uiDispatcher.BeginInvoke(DispatcherPriority.Background, new ThreadStart(_func));
				}
			}
		}

		public ReadOnlyObservableCollection<T> CreateObservableCollection()
		{
			if (m_observableCollection == null)
			{
				m_uiDispatcher = Dispatcher.CurrentDispatcher;
				m_observableCollection = new ObservableCollection<T>(m_dict.Values);
			}
			return new ReadOnlyObservableCollection<T>(m_observableCollection);
		}

		private void ItemChanged(IClientXObject<TKind> _obj)
		{
			UpdateIsDirty();
		}

		protected override void OnChanged()
		{
			UpdateIsDirty();
			base.OnChanged();
		}

		private void UpdateIsDirty()
		{
			m_isDirtyAndHaveReportItems = RecalcIsDirtyAndHaveReportItems();
			var newDirty = RecalcIsDirty() || m_isDirtyAndHaveReportItems;
			if (newDirty != m_isDirty)
			{
				m_isDirty = newDirty;
				OnDirtyChanged();
			}
		}

		private bool RecalcIsDirty()
		{
			return m_dict.Values.Any(_arg => _arg.IsDirty);
		}

		private bool RecalcIsDirtyAndHaveReportItems()
		{
			if (m_dict.Count != m_original.Count) return true;
			if (m_dict.Values.Except(m_original).Any()) return true;
			if (m_original.Except(m_dict.Values).Any()) return true;
			return false;
		}

		private T GetChild(XReportListItem _item)
		{
			var descriptor = m_client.GetDescriptor(_item.Uid);
			var child = descriptor.Get<T>(m_factory);
			;
			if (child is IClientXChildObject<TKind>)
			{
				((IClientXChildObject<TKind>) child).SetParent(m_owner);
				descriptor.SetParentUid(m_owner.Uid);
			}
			else
			{
				descriptor.SetParentUid(m_client.GetRootDescriptor().Uid);
			}
			return child;
		}
	}
}