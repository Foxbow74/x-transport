using System.Collections.Generic;
using XTransport;

namespace XTransportTest.Client
{
	internal class RootMirror : XObject
	{
		[X((int) ETestKind.A)] private ICollection<AMirror> m_aItems;

		[X((int) ETestKind.REF)] private ICollection<RefMirror> m_refItems;

		public ICollection<AMirror> AItems
		{
			get { return m_aItems; }
		}

		public ICollection<RefMirror> RefItems
		{
			get { return m_refItems; }
		}

		public override ETestKind Kind
		{
			get { return ETestKind.ROOT; }
		}
	}
}