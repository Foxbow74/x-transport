using XTransport;

namespace XTransportTest.Client
{
	internal class RefMirror : XObject
	{
		[X("RVAL")] private IXValue<A> m_ref;

		public override ETestKind Kind
		{
			get { return ETestKind.REF; }
		}

		public A Ref
		{
			get { return m_ref.Value; }
			set { m_ref.Value = value; }
		}
	}
}