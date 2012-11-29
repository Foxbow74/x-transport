using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace XTransport.Server
{
	[DataContract]
	internal class UndoXReport : ServerXReport
	{
		public UndoXReport(Guid _uid, IEnumerable<AbstractXReportItem> _items, uint _actualFrom, int _kind, EState _state) 
			: base(_uid, _items, _actualFrom, _kind, _state)
		{
			NeedRevert = false;
		}

		public UndoXReport(Guid _uid, uint _stored, int _kind)
			: base(_uid, new AbstractXReportItem[0], _stored, _kind, EState.REDO_ABLE)
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
		public ServerXReport(Guid _uid, IEnumerable<AbstractXReportItem> _items, uint _actualFrom, int _kind, EState _state)
			: base(_uid, _items, _kind, _state)
		{
			ActualFrom = _actualFrom;
		}

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
			State = EState.UNDO_ABLE | EState.REVERT_ABLE;
			
			foreach (var reportItem in _report.Items)
			{
				switch (reportItem.State)
				{
					case XReportItemState.CHANGE:
						var changeExists = Items.SingleOrDefault(_item => _item.FieldId == reportItem.FieldId && _item.State == XReportItemState.CHANGE);
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

			if (ActualFrom < _report.ActualFrom)
			{
				foreach (var reportItem in Items)
				{
					switch (reportItem.State)
					{
						case XReportItemState.CHANGE:
							var changeExists = _report.Items.SingleOrDefault(_item => _item.FieldId == reportItem.FieldId); // && _item.State == XReportItemState.CHANGE);
							if (changeExists == null)
							{
								_report.Items.Add(reportItem);
							}
							break;
					}
				}
			}
		}
	}
}