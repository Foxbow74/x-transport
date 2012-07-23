using System.Runtime.Serialization;
using XTransport.Server;

namespace XTransport
{
	/// <summary>
	/// Contains way to change ORIGINAL state to CURRENT, but not a DELTA between CURRENT and PREVIOUS
	/// </summary>
	[DataContract]
	internal abstract class AbstractXReportItem
	{
		protected AbstractXReportItem(int _fieldId, XReportItemState _state)
		{
			FieldId = _fieldId;
			State = _state;
		}

		[DataMember]
		public int FieldId { get; private set; }

		[DataMember]
		public XReportItemState State { get; private set; }

		public override string ToString()
		{
			return State + " FID:" + FieldId;
		}

		public abstract AbstractXReportItem CreateDefaultReportItem();

		public abstract IServerXValue CreateXValue();

		public abstract AbstractXReportItem UpdateAccordingNewValue(IServerXValue _xValue, AbstractXReportItem _reverseReport);
	}
}