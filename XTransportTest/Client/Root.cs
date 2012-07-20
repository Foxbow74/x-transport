using System.Collections.Generic;
using XTransport;

namespace XTransportTest.Client
{
	internal class Root : XObject
	{
		[X((int) ETestKind.A)] private IList<A> m_aItems;

		[X((int) ETestKind.B)] private IList<B> m_bItems;

		[X((int) ETestKind.PARENT)] private IList<Parent> m_parentItems;

		[X((int)ETestKind.REF)] private IList<RefObject> m_refItems;

		[XFactory(typeof(TestFactory))]
		[X((int)ETestKind.ALL)]private IList<XObject> m_all;

		[XFactory(typeof(TestFactory))]
		[X((int)ETestKind.AB)]private IList<XObject> m_ab;

		public IList<XObject> All
		{
			get { return m_all; }
		}

		public IList<XObject> AB
		{
			get { return m_ab; }
		}

		public IList<A> AItems
		{
			get { return m_aItems; }
		}

		public IList<B> BItems
		{
			get { return m_bItems; }
		}

		public IList<Parent> ParentItems
		{
			get { return m_parentItems; }
		}

		public IList<RefObject> RefItems
		{
			get { return m_refItems; }
		}

		public override ETestKind Kind
		{
			get { return ETestKind.ROOT; }
		}
	}
}