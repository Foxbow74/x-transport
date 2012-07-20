using System;
using System.Runtime.Serialization;

namespace XTransport
{
	[DataContract]
	internal class XReportListItem
	{
		public XReportListItem(Guid _uid, EReportListItemState _state)
		{
			Uid = _uid;
			State = _state;
		}

		[DataMember]
		public EReportListItemState State { get; private set; }

		[DataMember]
		public Guid Uid { get; private set; }
	}
}