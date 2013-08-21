using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using XTransport.Server;

namespace XTransport
{
	[DataContract]
	[KnownType("GetKnownType")]
	internal class XReport
	{
		public XReport(Guid _uid, IEnumerable<AbstractXReportItem> _items, int _kind, EState _state)
		{
			State = _state;
			Uid = _uid;
			Items = _items.ToList();
			Kind = _kind;
		}

		public EState State { get; set; }

		[DataMember]
		public Guid Uid { get; private set; }

		[DataMember]
		internal List<AbstractXReportItem> Items { get; private set; }

		[DataMember]
		public uint ActualFrom { get; set; }

		[DataMember]
		public int Kind { get; private set; }

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

		public override string ToString()
		{
			return string.Format("xreport: {0} itms:{1} ({2})", ActualFrom, Items.Count, Uid);
		}

		public void UpdateAccordingNewValues(Dictionary<int, IServerXValue> _xValues, XReport _reverseReport)
		{
			var items = new List<AbstractXReportItem>();
			foreach (var reportItem in Items)
			{
				var newItem = reportItem.UpdateAccordingNewValue(_xValues[reportItem.FieldId], _reverseReport.Items.SingleOrDefault(_item => _item.FieldId == reportItem.FieldId));
				if (newItem != null)
				{
					items.Add(newItem);
				}
			}
			Items = items;
		}
	}
}