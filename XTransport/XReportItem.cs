using System;
using System.Runtime.Serialization;
using XTransport.Server;

namespace XTransport
{
	[DataContract]
	internal class XReportItem<T> : AbstractXReportItem
	{
		public XReportItem(int _fieldId, T _value, XReportItemState _state)
			: base(_fieldId, _state)
		{
			Value = _value;
		}

		[DataMember]
		public T Value { get; private set; }

		public override AbstractXReportItem CreateDefaultReportItem()
		{
			return new XReportItem<T>(FieldId, default(T), XReportItemState.CHANGE);
		}

		public override IServerXValue CreateXValue()
		{
			return new ServerXValue<T>();
		}

		public override AbstractXReportItem UpdateAccordingNewValue(IServerXValue _xValue, AbstractXReportItem _reverseReport)
		{
			var item = (XReportItem<T>)_xValue.GetOriginalValueReportItem(-1, null);
			return item.Value.Equals(Value) ? null : this;
		}

		public override string ToString()
		{
			return base.ToString() + " = " + Value;
		}
	}
}