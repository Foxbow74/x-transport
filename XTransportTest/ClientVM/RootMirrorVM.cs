using System.Collections.Generic;
using XTransport;
using XTransportTest.Client;

namespace XTransportTest.ClientVM
{
	internal class RootMirrorVM : XObjectVM
	{
		[X((int) ETestKind.A)] private IList<AMirrorVM> m_aItems;

		[X((int) ETestKind.PARENT)] private IList<ParentMirrorVM> m_parentItems;

		public IList<AMirrorVM> AItems
		{
			get { return m_aItems; }
		}

		public IList<ParentMirrorVM> ParentItems
		{
			get { return m_parentItems; }
		}

		public override ETestKind Kind
		{
			get { return ETestKind.ROOT; }
		}
	}
}