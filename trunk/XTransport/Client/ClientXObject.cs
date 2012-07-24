using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XTransport.Client
{
	public abstract class ClientXObject<TKind> : IClientXObject<TKind> //, IClientXObjectInternal<TKind>
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

			Uid = Guid.NewGuid();

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
				var xlist = xfield as IXCollection<TKind>;
				if (xlist != null)
				{
					xlist.SetOwnerInfo(this, info.FieldId);
				}
				info.Field.SetValue(this, xfield);
				m_xValues.Add(info.FieldId, xfield);
			}
			SubscribePersistedValuesChanges();
		}

		public DateTime Stored { get; internal set; }

		#region IClientXObject<TKind> Members

		public Guid Uid { get; internal set; }
		public abstract TKind Kind { get; }

		public event Action<IClientXObject<TKind>> Changed;
		public bool IsDirty { get; private set; }

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

		internal void OnInstantiationFinished(AbstractXClient<TKind> _client)
		{
			if (this is IXClientUserInternal<TKind>)
			{
				((IXClientUserInternal<TKind>) this).SetClient(_client);
			}

			foreach (var clientUser in m_xValues.Values.OfType<IXClientUserInternal<TKind>>())
			{
				clientUser.SetClient(_client);
			}

			InstantiationFinished();
		}

		internal IEnumerable<AbstractXReportItem> GetChanges()
		{
			return
				m_xValues.Where(_pair => _pair.Value.IsDirtyAndHaveReportItems).Select(
					_pair => _pair.Value.GetXReportItem(_pair.Key));
		}

		internal void SetUid(Guid _uid)
		{
			Uid = _uid;
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
				Stored = _report.ActualFrom;
				UnsubscribePersistedValuesChanges();
			}
			var done = new List<IXValueInternal>();
			foreach (var item in _report.Items.OrderBy(_item => _item.State))
			{
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
				IsDirty = m_xValues.Values.Any(_internal => _internal.IsDirty);
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
			IsDirty = false;
		}

		internal void SaveInternal()
		{
			foreach (var value in m_xValues.Values)
			{
				value.Save();
			}
			IsDirty = false;
		}

		protected virtual void InstantiationFinished()
		{
		}

		private void SubscribePersistedValuesChanges()
		{
			foreach (var value in m_xValues.Values)
			{
				value.Changed += XValueOnChanged;
				value.DirtyChanged += XValueOnDirtyChanged;
			}
		}

		private void UnsubscribePersistedValuesChanges()
		{
			foreach (var value in m_xValues.Values)
			{
				value.Changed -= XValueOnChanged;
				value.DirtyChanged -= XValueOnDirtyChanged;
			}
		}

		internal virtual void XValueOnChanged(IXValueInternal _value)
		{
			XValueOnDirtyChanged(_value);
			OnChanged();
		}

		internal virtual void XValueOnDirtyChanged(IXValueInternal _value)
		{
			if (_value.IsDirty)
			{
				IsDirty = true;
			}
			else
			{
				IsDirty = m_xValues.Values.Any(_av => _av.IsDirty);
			}
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