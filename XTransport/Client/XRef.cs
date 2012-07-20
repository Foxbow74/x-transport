using System;

namespace XTransport.Client
{
	internal sealed class XRef<T, TKind> : AbstractXValue, IXValue<T>, IXClientUserInternal<TKind>
		where T : ClientXObject<TKind>
	{
		private readonly IXObjectFactory<TKind> m_factory;
		private AbstractXClient<TKind> m_client;
		private Guid m_current;

		private T m_object;
		private Guid m_original;

		internal XRef(IXObjectFactory<TKind> _factory)
		{
			m_factory = _factory;
		}

		internal XRef()
		{
		}

		public override bool IsDirty
		{
			get { return CheckValue(m_current, m_original); }
		}

		#region IXClientUserInternal<TKind> Members

		void IXClientUserInternal<TKind>.SetClient(AbstractXClient<TKind> _client)
		{
			m_client = _client;
		}

		#endregion

		#region IXValue<T> Members

		public T Value
		{
			get { return m_object; }
			set { SetValue(value.Uid); }
		}

		#endregion

		public override AbstractXReportItem GetXReportItem(int _xname)
		{
#if DEBUG
			if (!IsDirtyAndHaveReportItems)
			{
				throw new ApplicationException("Not dirty value");
			}
#endif
			return new XReportItem<Guid>(_xname, m_current, XReportItemState.CHANGE);
		}

		public override void Save()
		{
			m_original = m_current;
		}

		public override void ApplyChanges(AbstractXReportItem _reportItem, bool _firstTime)
		{
			var item = (XReportItem<Guid>) _reportItem;
			switch (_reportItem.State)
			{
				case XReportItemState.ORIGINAL:
					m_original = item.Value;
					if (_firstTime)
					{
						SetValue(m_original);
					}
					break;
				case XReportItemState.CHANGE:
					SetValue(item.Value);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override void Revert()
		{
			SetValue(m_original);
		}

		private void SetValue(Guid _uid)
		{
			if (!CheckValue(m_current, _uid)) return;
			m_current = _uid;
			if (m_factory == null)
			{
				m_object = m_client.GetDescriptor(_uid).Get<T>(null);
			}
			else
			{
				m_object = m_client.GetDescriptor(_uid).Get<T>(m_factory);
			}

			OnChanged();
		}

		private static bool CheckValue(Guid _current, Guid _value)
		{
			return !_current.Equals(_value);
		}
	}
}