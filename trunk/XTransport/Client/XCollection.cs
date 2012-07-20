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
		private readonly IXObjectFactory<TKind> m_factory;
		private readonly Dictionary<Guid, T> m_list = new Dictionary<Guid, T>();
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
			return m_list.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T _item)
		{
			m_list.Add(_item.Uid, _item);
			AddedToCollection(_item);
			UpdateVM(() => m_observableCollection.Add(_item));
		}

		public void Clear()
		{
			if (m_list.Count <= 0) return;
			var items = m_list.ToArray();
			m_list.Clear();
			foreach (var item in items)
			{
				RemovedFromCollection(item.Value);
			}
			OnChanged();
			UpdateVM(() => m_observableCollection.Clear());
		}

		public bool Contains(T _item)
		{
			return m_list.ContainsKey(_item.Uid);
		}

		public void CopyTo(T[] _array, int _arrayIndex)
		{
			m_list.Values.CopyTo(_array, _arrayIndex);
		}

		public bool Remove(T _item)
		{
			if (m_list.Remove(_item.Uid))
			{
				RemovedFromCollection(_item);
				UpdateVM(() => m_observableCollection.Remove(_item));
				return true;
			}
			return false;
		}

		public int Count
		{
			get { return m_list.Count; }
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
			return m_list.Keys;
		}

		public void AddSilently(IXObject<TKind> _item)
		{
			m_list.Add(_item.Uid, (T)_item);
			UpdateVM(() => m_observableCollection.Add((T)_item));
		}

		public void RemoveSilently(ClientXObject<TKind> _item)
		{
			if (_item is T)
			{
				m_list.Remove(_item.Uid);
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
			items.AddRange(m_list.Values.Except(m_original).Select(_arg => new XReportListItem(_arg.Uid, EReportListItemState.ADDED)));
			items.AddRange(m_original.Except(m_list.Values).Select(_arg => new XReportListItem(_arg.Uid, EReportListItemState.REMOVED)));
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
						m_list.Clear();
						foreach (var item in m_original)
						{
							AddSilently(item);
							//Add(item);
						}
					}
					break;
				case XReportItemState.CHANGE:
					//Clear();
					//foreach (var item in m_original)
					//{
					//    Add(item);
					//}
					Revert();

					foreach (var item in rl.Items)
					{
						switch (item.State)
						{
							case EReportListItemState.ADDED:
								var add = GetChild(item);
								Add(add);
								break;
							case EReportListItemState.REMOVED:
								Remove(m_list.Values.Single(_arg => _arg.Uid == item.Uid));
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
			var objects = m_list.Values.ToArray();
			foreach (var obj in objects)
			{
				if (!m_original.Contains(obj))
				{
					Remove(obj);
				}
			}
			foreach (var obj in m_original)
			{
				if (!objects.Contains(obj))
				{
					Add(obj);
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
				m_original.AddRange(m_list.Values);
			}
			m_isDirty = false;
			m_isDirtyAndHaveReportItems = false;
		}

		#endregion

		private void AddedToCollection(T _item)
		{
			var kindId = m_client.KindToIntInternal(_item.Kind);
			if (kindId != m_fieldId)
			{
				m_owner.AddedToCollection(_item, kindId);
			}
			foreach(var kind in m_client.AddIfNotExists(_item, m_owner))
			{
				if(kind!=m_fieldId) m_owner.AddedToCollection(_item, kind);
			}
			_item.Changed += ItemChanged;
			OnChanged();
		}

		private void RemovedFromCollection(T _item)
		{
			m_owner.RemovedFromCollection(_item);
			_item.Changed -= ItemChanged;
			m_client.Release(_item);
			m_client.RefDeleted(_item);
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
				m_observableCollection = new ObservableCollection<T>(m_list.Values);
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
			return m_list.Values.Any(_arg => _arg.IsDirty);
		}

		private bool RecalcIsDirtyAndHaveReportItems()
		{
			if (m_list.Count != m_original.Count) return true;
			if (m_list.Values.Except(m_original).Any()) return true;
			if (m_original.Except(m_list.Values).Any()) return true;
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