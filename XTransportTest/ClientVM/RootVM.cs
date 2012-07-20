using System.Collections.Generic;
using XTransport;
using XTransportTest.Client;

namespace XTransportTest.ClientVM
{
	internal class RootVM : XObjectVM
	{
		[X((int) ETestKind.A)] private IList<Avm> m_aItems;

		[X((int) ETestKind.PARENT)] private IList<ParentVM> m_parentItems;

		public IList<Avm> AItems
		{
			get { return m_aItems; }
		}

		public IList<ParentVM> ParentItems
		{
			get { return m_parentItems; }
		}

		public override ETestKind Kind
		{
			get { return ETestKind.ROOT; }
		}
	}
}