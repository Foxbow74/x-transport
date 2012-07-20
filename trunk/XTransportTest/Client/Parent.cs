using System.Collections.Generic;
using XTransport;

namespace XTransportTest.Client
{
	class Parent : XObject
	{
		public Parent()
		{}

		[X("LIST")] private IList<Child> m_value;

		public override ETestKind Kind
		{
			get { return ETestKind.PARENT; }
		}

		public IList<Child> Children
		{
			get { return m_value; }
		}
	}
}