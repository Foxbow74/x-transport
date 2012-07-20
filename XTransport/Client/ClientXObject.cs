using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XTransport.Client
{
	public abstract class ClientXObject<TKind> : AbstractXObject<TKind>, IClientXObjectInternal<TKind>
	{
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
				if (typeof (IList<>).MakeGenericType(genericArgumentType).IsAssignableFrom(fieldInfo.FieldType))
				{
					if (typeof (IClientXObjectInternal<TKind>).IsAssignableFrom(genericArgumentType))
					{
						var tp = typeof (XList<,>).MakeGenericType(new[] {genericArgumentType, typeof (TKind)});
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
					if (typeof (IClientXObjectInternal<TKind>).IsAssignableFrom(genericArgumentType))
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
				var xlist = xfield as IXList<TKind>;
				if (xlist!=null)
				{
					xlist.SetOwnerInfo(this, info.FieldId);
				}
				info.Field.SetValue(this, xfield);
				m_xValues.Add(info.FieldId, xfield);
			}
			SubscribePersistedValuesChanges();
		}

		#region IClientXObjectInternal<TKind> Members

		void IClientXObjectInternal<TKind>.OnInstantiationFinished(AbstractXClient<TKind> _client)
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

		public event Action<IClientXObject<TKind>> Changed;

		IEnumerable<AbstractXReportItem> IClientXObjectInternal<TKind>.GetChanges()
		{
			return
				m_xValues.Where(_pair => _pair.Value.IsDirtyAndHaveReportItems).Select(
					_pair => _pair.Value.GetXReportItem(_pair.Key));
		}

		void IClientXObjectInternal<TKind>.SetUid(Guid _uid)
		{
			Uid = _uid;
		}

		public virtual void OnDeserialized()
		{
		}

		public IEnumerable<Guid> GetChildUids()
		{
			foreach (var list in m_xValues.Values.OfType<IXList<TKind>>())
			{
				foreach (var uid in list.GetUids())
				{
					yield return uid;
				}
			}
		}

		public void AddedToCollection<T>(T _item, int _kind) where T : ClientXObject<TKind>
		{
			IXValueInternal xValueInternal;
			if (m_xValues.TryGetValue(_kind, out xValueInternal))
			{
				var list = xValueInternal as IXList<TKind>;
				if(list!=null)
				{
					list.AddSilently(_item);
				}
			}
		}

		public void RemovedFromCollection<T>(T _item) where T : ClientXObject<TKind>
		{
			foreach (var list in m_xValues.Values.OfType<IXList<TKind>>())
			{
				list.RemoveSilently(_item);
			}	
		}

		void IClientXObjectInternal<TKind>.ApplyChanges(XReport _report, bool _firstTime)
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

		public bool IsDirty { get; private set; }

		void IClientXObjectInternal<TKind>.Revert()
		{
			foreach (var value in m_xValues.Values)
			{
				value.Revert();
			}
			IsDirty = false;
		}

		void IClientXObjectInternal<TKind>.SaveInternal()
		{
			foreach (var value in m_xValues.Values)
			{
				value.Save();
			}
			IsDirty = false;
			OnChanged();
		}

		#endregion

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
			if (_value.IsDirty)
			{
				IsDirty = true;
			}
			else
			{
				IsDirty = m_xValues.Values.Any(_av => _av.IsDirty);
			}
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