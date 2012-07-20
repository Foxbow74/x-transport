using System;

namespace XTransport
{
	public abstract class AbstractXObject<TKind> : IXObject<TKind>
	{
		public DateTime Stored { get; internal set; }

		#region IXObject<TKind> Members

		public Guid Uid { get; internal set; }
		public abstract TKind Kind { get; }

		#endregion
	}
}