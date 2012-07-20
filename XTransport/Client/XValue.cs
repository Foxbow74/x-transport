using System;

namespace XTransport.Client
{
	internal class XValue<T> : AbstractXValue, IXValue<T>
	{
		private T m_current;
		private T m_original;

		internal XValue()
		{
		}

		public override bool IsDirty
		{
			get { return CheckValue(m_current, m_original); }
		}

		#region IXValue<T> Members

		public T Value
		{
			get { return m_current; }
			set
			{
				if (CheckValue(m_current, value))
				{
					m_current = value;
					OnChanged();
				}
			}
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
			return new XReportItem<T>(_xname, m_current, XReportItemState.CHANGE);
		}

		public override void Save()
		{
			m_original = m_current;
		}

		public override void ApplyChanges(AbstractXReportItem _reportItem, bool _firstTime)
		{
			var item = (XReportItem<T>) _reportItem;
			switch (_reportItem.State)
			{
				case XReportItemState.ORIGINAL:
					m_original = item.Value;
					if (_firstTime)
					{
						m_current = m_original;
					}
					break;
				case XReportItemState.CHANGE:
					Value = item.Value;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override void Revert()
		{
			Value = m_original;
		}

		private static bool CheckValue(T _current, T _value)
		{
			if (ReferenceEquals(_current, _value)) return false;
			return (_current == null) || (_value == null) || !_current.Equals(_value);
		}
	}
}