using System.Collections.Generic;
using XTransport;

namespace XTransportTest.Client
{
	internal class Parent : XObject
	{
		[X("LIST")] private ICollection<Child> m_value;

		public override ETestKind Kind
		{
			get { return ETestKind.PARENT; }
		}

		public ICollection<Child> Children
		{
			get { return m_value; }
		}
	}
}