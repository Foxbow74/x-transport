using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace XTransport.Server
{
	[DataContract]
	internal class UndoXReport : ServerXReport
	{
		public UndoXReport(Guid _uid, IEnumerable<AbstractXReportItem> _items, uint _actualFrom, uint _lastModification, uint _storedActualFrom, int _kind) 
			: base(_uid, _items, _actualFrom, _lastModification, _storedActualFrom, _kind)
		{
			NeedRevert = false;
		}

		public UndoXReport(Guid _uid, uint _stored, int _kind)
			: base(_uid, new AbstractXReportItem[0], _stored, _stored, _stored, _kind)
		{
			NeedRevert = true;
		}

		[DataMember]
		public bool NeedRevert { get; private set; }
	}

	[DataContract]
	[KnownType("GetKnownType")]
	internal class ServerXReport : XReport
	{
		public ServerXReport(Guid _uid, IEnumerable<AbstractXReportItem> _items, uint _actualFrom, uint _lastModification, uint _storedActualFrom, int _kind)
			: base(_uid, _items, _kind)
		{
			ActualFrom = _actualFrom;
			LastModification = _lastModification;
			StoredActualFrom = _storedActualFrom;
		}


		[DataMember]
		public uint StoredActualFrom { get; private set; }

		[DataMember]
		public uint LastModification { get; private set; }

		private static Type[] GetKnownType()
		{
			return new[]
			       	{
			       		typeof (UndoXReport),
			       		typeof (ServerXReport),
			       		typeof (XReportList),
			       		typeof (XReportListItem),
			       		typeof (XReportItem<Guid>),
			       		typeof (XReportItem<String>),
			       		typeof (XReportItem<Int32>),
			       		typeof (XReportItem<Int64>),
			       		typeof (XReportItem<Double>),
			       		typeof (XReportItem<Decimal>)
			       	};
		}

		public void MergeChanges(XReport _report)
		{
			foreach (var reportItem in _report.Items)
			{
				switch (reportItem.State)
				{
					case XReportItemState.CHANGE:
						var changeExists =
							Items.SingleOrDefault(_item => _item.FieldId == reportItem.FieldId && _item.State == XReportItemState.CHANGE);
						if (changeExists != null)
						{
							Items.Remove(changeExists);
						}
						Items.Add(reportItem);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}