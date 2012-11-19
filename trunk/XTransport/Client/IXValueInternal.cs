using System;

namespace XTransport.Client
{
	internal interface IXValueInternal
	{
		bool IsDirty { get; }
		bool IsDirtyAndHaveReportItems { get; }

		AbstractXReportItem GetXReportItem(int _xname);
		void ApplyChanges(AbstractXReportItem _reportItem, bool _firstTime);
		void Revert();
		void Save();

		event Action<IXValueInternal> Changed;
		event Action<bool> DirtyChanged;
	}
}