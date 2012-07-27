using XTransport;

namespace XTransportTest.Client
{
	internal class RefObject : XObject
	{
		[X("RVAL")]
		private IXValue<A> m_ref;

		[X("CHILD_REF")]
		private IXValue<Child> m_cref;

		public override ETestKind Kind
		{
			get { return ETestKind.REF; }
		}

		public A Ref
		{
			get { return m_ref.Value; }
			set { m_ref.Value = value; }
		}

		public Child ChildRef
		{
			get { return m_cref.Value; }
			set {m_cref.Value = value;}
		}
	}
}