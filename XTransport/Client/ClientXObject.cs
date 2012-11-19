using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XTransport.Client
{
	public abstract class ClientXObject<TKind> : IClientXObject<TKind>
	{
		private readonly Dictionary<int, IXValueInternal> m_xValues = new Dictionary<int, IXValueInternal>();
		private int m_batchChanges;
		private bool m_batchUpdating;

		protected ClientXObject()
		{
			List<XFieldInfo<TKind>> list;
			if (!m_xValueInfos.TryGetValue(GetType(), out list))
			{
				list = InitializeType(GetType());
			}

			foreach (var info in list)
			{
				IXValueInternal xfield;
				if (info.Factory == null)
				{
					xfield = (IXValueInternal) info.Constructor.Invoke(new object[] {});
				}
				else
				{
					xfield = (IXValueInternal) info.Constructor.Invoke(new object[] {info.Factory,});
				}
				info.Field.SetValue(this, xfield);
				m_xValues.Add(info.FieldId, xfield);
			}
			SetUid(Guid.NewGuid());
			SubscribePersistedValuesChanges();
		}

		internal void SetClientInternal(AbstractXClient<TKind> _client)
		{
			if (this is IXClientUserInternal<TKind>)
			{
				((IXClientUserInternal<TKind>)this).SetClient(_client);
			}
			foreach (var pair in m_xValues)
			{
				var clientUser = pair.Value as IXClientUserInternal<TKind>;
				if (clientUser != null)
				{
					clientUser.SetClient(_client);
				}
			}
		}

		internal void SetUid(Guid _uid)
		{
			Uid = _uid;
			foreach (var pair in m_xValues)
			{
				var collection = pair.Value as IXCollection<TKind>;
				if (collection != null)
				{
					collection.SetOwnerInfo(Uid, pair.Key);
				}
			}
		}

		internal void OnInstantiationFinished()
		{
			InstantiationFinished();
		}

		protected virtual void InstantiationFinished()
		{
		}

		#region IClientXObject<TKind> Members

		public Guid Uid { get; internal set; }
		public abstract TKind Kind { get; }

		public event Action<IClientXObject<TKind>> Changed;
		private bool? m_isDirty;
		public bool IsDirty
		{
			get
			{
				return (m_isDirty??(m_isDirty=RecalcIsDirty())).Value;
			}
		}

		private bool RecalcIsDirty()
		{
			return m_xValues.Values.Any(_internal => _internal.IsDirty);
		}

		#endregion

		#region static part

		private static readonly Dictionary<Type, List<XFieldInfo<TKind>>> m_xValueInfos =
			new Dictionary<Type, List<XFieldInfo<TKind>>>();

		private static List<XFieldInfo<TKind>> InitializeType(Type _type)
		{
			var list = new List<XFieldInfo<TKind>>();
			m_xValueInfos.Add(_type, list);

			var fields = new List<FieldInfo>();
			var bt = _type;
			do
			{
				fields.AddRange(bt.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
				bt = bt.BaseType;
			} while (bt != null);
			foreach (var fieldInfo in fields)
			{
				if (!fieldInfo.FieldType.IsGenericType) continue;
				var genericArgumentType = fieldInfo.FieldType.GetGenericArguments().First();

				var xAttribute = fieldInfo.GetCustomAttributes(typeof (XAttribute), true).Cast<XAttribute>().SingleOrDefault();
				if (xAttribute == null)
				{
					continue;
				}
				var xFieldInfo = new XFieldInfo<TKind>(xAttribute.HashCode, fieldInfo);

				var xFactoryAttribute =
					fieldInfo.GetCustomAttributes(typeof (XFactoryAttribute), true).Cast<XFactoryAttribute>().SingleOrDefault();
				if (genericArgumentType.IsAbstract)
				{
					if (xFactoryAttribute == null)
					{
						throw new ApplicationException(string.Format("Not specified XFactoryAttribute for {0} of type {1}",
						                                             fieldInfo.FieldType.Name, fieldInfo.DeclaringType.Name));
					}
					xFieldInfo.Factory = (IXObjectFactory<TKind>) Activator.CreateInstance(xFactoryAttribute.FactoryType);
				}

				var constructor = fieldInfo.FieldType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
				                                                     Type.EmptyTypes, null);
				if (typeof (ICollection<>).MakeGenericType(genericArgumentType).IsAssignableFrom(fieldInfo.FieldType))
				{
					if (typeof (ClientXObject<TKind>).IsAssignableFrom(genericArgumentType))
					{
						var tp = typeof (XCollection<,>).MakeGenericType(new[] {genericArgumentType, typeof (TKind)});
						if (genericArgumentType.IsAbstract)
						{
							constructor = tp.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
							                                new[] {typeof (IXObjectFactory<TKind>)}, null);
						}
						else
						{
							constructor = tp.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
						}
					}
				}
				else if (typeof (IXValue<>).MakeGenericType(genericArgumentType).IsAssignableFrom(fieldInfo.FieldType))
				{
					if (typeof (ClientXObject<TKind>).IsAssignableFrom(genericArgumentType))
					{
						var tp = typeof (XRef<,>).MakeGenericType(new[] {genericArgumentType, typeof (TKind)});
						if (genericArgumentType.IsAbstract)
						{
							constructor = tp.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
							                                new[] {typeof (IXObjectFactory<TKind>)}, null);
						}
						else
						{
							constructor = tp.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
						}
					}
					else
					{
						var tp = typeof (XValue<>).MakeGenericType(new[] {genericArgumentType});
						constructor = tp.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
					}
				}
				else
				{
					continue;
				}

				if (constructor == null)
				{
					throw new ApplicationException(string.Format("Can't get default constructor or factory for {0} of type {1}",
					                                             fieldInfo.FieldType.Name,
					                                             fieldInfo.DeclaringType.Name));
				}

				xFieldInfo.Constructor = constructor;
				list.Add(xFieldInfo);
			}
			return list;
		}

		#endregion

		internal IEnumerable<AbstractXReportItem> GetChanges()
		{
			return
				m_xValues.Where(_pair => _pair.Value.IsDirtyAndHaveReportItems).Select(
					_pair => _pair.Value.GetXReportItem(_pair.Key));
		}

		internal IEnumerable<Guid> GetChildUids()
		{
			foreach (var list in m_xValues.Values.OfType<IXCollection<TKind>>())
			{
				foreach (var uid in list.GetUids())
				{
					yield return uid;
				}
			}
		}

		internal void AddedToCollection<T>(T _item, int _fieldId) where T : ClientXObject<TKind>
		{
			IXValueInternal value;
			if (!m_xValues.TryGetValue(_fieldId, out value)) return;

			var list = value as IXCollection<TKind>;
			if (list == null)
			{
				throw new ApplicationException();
			}
			list.AddSilently(_item);
		}

		internal void RemovedFromCollection<T>(T _item) where T : ClientXObject<TKind>
		{
			foreach (var list in m_xValues.Values.OfType<IXCollection<TKind>>())
			{
				list.RemoveSilently(_item.Uid);
			}
		}

		internal void ApplyChanges(XReport _report, bool _firstTime)
		{
			if (_firstTime)
			{
				UnsubscribePersistedValuesChanges();
			}
			var done = new List<IXValueInternal>();
			foreach (var item in _report.Items.OrderBy(_item => _item.State))
			{
                if(Kind.ToString()=="ALL")
                {
                    
                }

				IXValueInternal value;
				if (m_xValues.TryGetValue(item.FieldId, out value))
				{
					value.ApplyChanges(item, _firstTime);
					done.Add(value);
				}
			}
			if (_firstTime)
			{
				SubscribePersistedValuesChanges();
				m_isDirty = null;
			}
			foreach (var xValue in m_xValues.Values.Except(done))
			{
				xValue.Revert();
			}
		}

		internal void Revert()
		{
			foreach (var value in m_xValues.Values)
			{
				value.Revert();
			}
			m_isDirty = false;
		}

		internal void SaveInternal()
		{
			foreach (var value in m_xValues.Values)
			{
				value.Save();
			}
			m_isDirty = false;
		}

		private void SubscribePersistedValuesChanges()
		{
			foreach (var value in m_xValues.Values)
			{
				value.Changed += XValueOnChanged;
				value.DirtyChanged += UpdateDirty;
			}
		}

		private void UnsubscribePersistedValuesChanges()
		{
			foreach (var value in m_xValues.Values)
			{
				value.Changed -= XValueOnChanged;
				value.DirtyChanged -= UpdateDirty;
			}
		}

		internal virtual void XValueOnChanged(IXValueInternal _value)
		{
			UpdateDirty(_value.IsDirty);
			OnChanged();
		}

		internal void UpdateDirty(bool _isDirty)
		{
			m_isDirty = _isDirty ? (bool?)true : null;
		}

		internal void SetIsDirty()
		{
			
		}

		private void OnChanged()
		{
			if (Changed != null)
			{
				if (m_batchUpdating)
				{
					m_batchChanges++;
				}
				else
				{
					Changed(this);
				}
			}
		}

		private void BeginBatchUpdating()
		{
			m_batchUpdating = true;
			m_batchChanges = 0;
		}

		private void EndBatchUpdating()
		{
			if (m_batchChanges > 0 && m_batchUpdating)
			{
				if (Changed != null)
				{
					Changed(this);
				}
				m_batchUpdating = false;
			}
		}

		#region Nested type: BatchChanges

		protected class BatchChanges : IDisposable
		{
			private readonly ClientXObject<TKind> m_xObject;

			public BatchChanges(ClientXObject<TKind> _xObject)
			{
				m_xObject = _xObject;
				_xObject.BeginBatchUpdating();
			}

			#region IDisposable Members

			public void Dispose()
			{
				m_xObject.EndBatchUpdating();
			}

			#endregion
		}

		#endregion
	}
}