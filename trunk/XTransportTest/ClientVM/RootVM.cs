using System.Collections.Generic;
using XTransport;
using XTransportTest.Client;

namespace XTransportTest.ClientVM
{
	internal class RootVM : XObjectVM
	{
		[X((int) ETestKind.A)] private ICollection<Avm> m_aItems;

		[X((int) ETestKind.PARENT)] private ICollection<ParentVM> m_parentItems;

		public ICollection<Avm> AItems
		{
			get { return m_aItems; }
		}

		public ICollection<ParentVM> ParentItems
		{
			get { return m_parentItems; }
		}

		public override ETestKind Kind
		{
			get { return ETestKind.ROOT; }
		}
	}
}