using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using XTransport.Server;

namespace XTransport
{
	[DataContract]
	internal class XReportList : AbstractXReportItem
	{
		public XReportList(int _fieldId, XReportItemState _state, IEnumerable<XReportListItem> _items)
			: base(_fieldId, _state)
		{
			Items = _items.ToList();
		}

		[DataMember]
		public List<XReportListItem> Items { get; private set; }

		public override string ToString()
		{
			return base.ToString() + "[" + Items.Count + " items]";
		}

		public override AbstractXReportItem CreateDefaultReportItem()
		{
			return new XReportList(FieldId, XReportItemState.CHANGE, Enumerable.Empty<XReportListItem>());
		}

		public override IServerXValue CreateXValue()
		{
			return new ServerXList();
		}

		public override AbstractXReportItem UpdateAccordingNewValue(IServerXValue _xValue, AbstractXReportItem _reverseReport)
		{
			var original = ((XReportList) _xValue.GetOriginalValueReportItem(-1, null)).Items.Select(_item => _item.Uid).ToArray();
			var target = original.ToList();
			if (_reverseReport != null)
			{
				foreach (var item in ((XReportList) _reverseReport).Items)
				{
					switch (item.State)
					{
						case EReportListItemState.ADDED:
							target.Add(item.Uid);
							break;
						case EReportListItemState.REMOVED:
							if (!target.Remove(item.Uid))
							{
								throw new ApplicationException();
							}
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
			}

			foreach (var item in Items)
			{
				switch (item.State)
				{
					case EReportListItemState.ADDED:
						target.Add(item.Uid);
						break;
					case EReportListItemState.REMOVED:
						if (!target.Remove(item.Uid))
						{
							throw new ApplicationException();
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			Items.Clear();
			foreach (var uid in target)
			{
				if (!original.Contains(uid)) Items.Add(new XReportListItem(uid, EReportListItemState.ADDED));
			}
			foreach (var uid in original)
			{
				if (!target.Contains(uid)) Items.Add(new XReportListItem(uid, EReportListItemState.REMOVED));
			}
			return this;
		}
	}
}