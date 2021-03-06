﻿using System;
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
		private readonly Dictionary<Guid, T> m_dict = new Dictionary<Guid, T>();
		private readonly IXObjectFactory<TKind> m_factory;
		private readonly List<T> m_original = new List<T>();
		private AbstractXClient<TKind> m_client;
		private int m_fieldId;
		private bool m_isDirty;
		private bool m_isDirtyAndHaveReportItems;

		private ObservableCollection<T> m_observableCollection;

		private Guid m_ownerUid;
		private Dispatcher m_uiDispatcher;

		internal XCollection(IXObjectFactory<TKind> _factory)
		{
			m_factory = _factory;
		}

		internal XCollection()
		{
		}

		#region ICollection<T> Members

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
			AddedToCollection(_item);
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

		void IXCollection<TKind>.SetOwnerInfo(Guid _ownerUid, int _fieldId)
		{
			m_ownerUid = _ownerUid;
			m_fieldId = _fieldId;
		}

		public IEnumerable<Guid> GetUids()
		{
			return m_dict.Keys;
		}

		void IXCollection<TKind>.AddSilently(ClientXObject<TKind> _item)
		{
			if (!m_dict.ContainsKey(_item.Uid))
			{
				m_dict.Add(_item.Uid, (T) _item);
				_item.SetClientInternal(m_client);
				UpdateVM(() => m_observableCollection.Add((T) _item));
			}
		}

		void IXCollection<TKind>.RemoveSilently(Guid _uid)
		{
			T item;
			if (m_dict.TryGetValue(_uid, out item))
			{
				if (m_dict.Remove(_uid))
				{
					UpdateVM(() => m_observableCollection.Remove(item));
				}
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
			items.AddRange(
				m_dict.Values.Except(m_original).Select(_arg => new XReportListItem(_arg.Uid, EReportListItemState.ADDED)));
			items.AddRange(
				m_original.Except(m_dict.Values).Select(_arg => new XReportListItem(_arg.Uid, EReportListItemState.REMOVED)));
			var rl = new XReportList(_xname, XReportItemState.CHANGE, items);
			return rl;
		}

		public override void ApplyChanges(AbstractXReportItem _reportItem, bool _firstTime)
		{
			var rl = (XReportList) _reportItem;
			var toAdd = new List<Guid>();
			var toDel = new List<Guid>();
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
						toAdd.AddRange(rl.Items.Select(_item => _item.Uid));
					}
					break;
				case XReportItemState.CHANGE:
					var current = m_dict.Values.ToArray();
					foreach (var item in current)
					{
						if (!m_original.Contains(item))
						{
							toDel.Add(item.Uid);
							//((IXCollection<TKind>) this).RemoveSilently(item.Uid);
							//item.Changed -= ItemChanged;
							//m_client.Release(item);
						}
					}
					foreach (var item in m_original)
					{
						if (!current.Contains(item))
						{
							toAdd.Add(item.Uid);
							//((IXCollection<TKind>) this).AddSilently(item);
						}
					}

					foreach (var item in rl.Items)
					{
						switch (item.State)
						{
							case EReportListItemState.ADDED:
								toAdd.Add(item.Uid);
								toDel.Remove(item.Uid);
								//var add = GetChild(item);
								//((IXCollection<TKind>) this).AddSilently(add);)
								break;
							case EReportListItemState.REMOVED:
								toDel.Add(item.Uid);
								toAdd.Remove(item.Uid);
								//((IXCollection<TKind>) this).RemoveSilently(item.Uid);
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			var changed = false;
			foreach (var uid in toAdd)
			{
				changed = true;
				var add = m_client.GetInternal<T>(uid, m_factory, m_ownerUid);
				add.Changed += ItemChanged;
				((IXCollection<TKind>) this).AddSilently(add);
			}
			foreach (var uid in toDel)
			{
				T del;
				if(m_dict.TryGetValue(uid, out del))
				{
					changed = true;
					del.Changed -= ItemChanged;
					m_client.Release(del);
					((IXCollection<TKind>)this).RemoveSilently(uid);
				}
			}
			if(changed)
			{
				OnChanged();
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
					((IXCollection<TKind>) this).RemoveSilently(item.Uid);
				}
			}
			foreach (var item in m_original)
			{
				if (!objects.Contains(item))
				{
					((IXCollection<TKind>) this).AddSilently(item);
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
			m_client.RegisterNewItem(_item, m_ownerUid, m_fieldId);
			_item.Changed += ItemChanged;
			OnChanged();
		}

		private void RemovedFromCollection(T _item)
		{
			m_client.DeleteObject(_item, m_ownerUid);
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
			return m_client.GetInternal<T>(_item.Uid, m_factory, m_ownerUid);
		}
	}
}