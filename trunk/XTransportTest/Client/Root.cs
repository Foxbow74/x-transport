using System.Collections.Generic;
using XTransport;

namespace XTransportTest.Client
{
	internal class Root : XObject
	{
		[X((int) ETestKind.A)] private ICollection<A> m_aItems;
		[XFactory(typeof (TestFactory))] [X((int) ETestKind.AB)] private ICollection<XObject> m_ab;
		[XFactory(typeof (TestFactory))] [X((int) ETestKind.ALL)] private ICollection<XObject> m_all;

		[X((int) ETestKind.B)] private ICollection<B> m_bItems;

		[X((int) ETestKind.PARENT)] private ICollection<Parent> m_parentItems;

		[X((int) ETestKind.REF)] private ICollection<RefObject> m_refItems;

		public ICollection<XObject> All
		{
			get { return m_all; }
		}

		public ICollection<XObject> AB
		{
			get { return m_ab; }
		}

		public ICollection<A> AItems
		{
			get { return m_aItems; }
		}

		public ICollection<B> BItems
		{
			get { return m_bItems; }
		}

		public ICollection<Parent> ParentItems
		{
			get { return m_parentItems; }
		}

		public ICollection<RefObject> RefItems
		{
			get { return m_refItems; }
		}

		public override ETestKind Kind
		{
			get { return ETestKind.ROOT; }
		}
	}
}