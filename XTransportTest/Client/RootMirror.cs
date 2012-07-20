using System.Collections.Generic;
using XTransport;

namespace XTransportTest.Client
{
	internal class RootMirror : XObject
	{
		[X((int) ETestKind.A)] private IList<AMirror> m_aItems;

		[X((int) ETestKind.REF)] private IList<RefMirror> m_refItems;

		public IList<AMirror> AItems
		{
			get { return m_aItems; }
		}

		public IList<RefMirror> RefItems
		{
			get { return m_refItems; }
		}

		public override ETestKind Kind
		{
			get { return ETestKind.ROOT; }
		}
	}
}