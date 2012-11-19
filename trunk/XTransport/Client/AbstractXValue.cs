using System;

namespace XTransport.Client
{
	internal abstract class AbstractXValue : IXValueInternal
	{
		#region IXValueInternal Members

		public event Action<IXValueInternal> Changed;
		public event Action<bool> DirtyChanged;


		public abstract AbstractXReportItem GetXReportItem(int _xname);

		public abstract void ApplyChanges(AbstractXReportItem _reportItem, bool _firstTime);
		public abstract void Revert();
		public abstract void Save();
		public abstract bool IsDirty { get; }

		public virtual bool IsDirtyAndHaveReportItems
		{
			get { return IsDirty; }
		}

		#endregion

		protected virtual void OnChanged()
		{
			if (Changed != null)
			{
				Changed(this);
			}
		}

		protected virtual void OnDirtyChanged()
		{
			if (DirtyChanged != null)
			{
				DirtyChanged(IsDirty);
			}
		}
	}
}