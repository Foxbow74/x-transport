using System.Collections.Generic;
using XTransport;
using XTransportTest.Client;

namespace XTransportTest.ClientVM
{
	internal class RootMirrorVM : XObjectVM
	{
		[X((int) ETestKind.A)] private ICollection<AMirrorVM> m_aItems;

		[X((int) ETestKind.PARENT)] private ICollection<ParentMirrorVM> m_parentItems;

		public ICollection<AMirrorVM> AItems
		{
			get { return m_aItems; }
		}

		public ICollection<ParentMirrorVM> ParentItems
		{
			get { return m_parentItems; }
		}

		public override ETestKind Kind
		{
			get { return ETestKind.ROOT; }
		}
	}
}