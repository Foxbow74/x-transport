using XTransport;

namespace XTransportTest.Client
{
	internal class AMirror : XObject
	{
		[X("IVAL")] private IXValue<int> m_value;

		public override ETestKind Kind
		{
			get { return ETestKind.A; }
		}

		public int Value
		{
			get { return m_value.Value; }
			set { m_value.Value = value; }
		}
	}
}