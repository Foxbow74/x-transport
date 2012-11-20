using XTransport;

namespace XTransportTest.Client
{
	internal class A : XObject
	{
		public A()
		{ }

		[X("IVAL")] private IXValue<int> m_value;

		[X("BVAL")]
		private IXValue<byte[]> m_bytes;

		public override ETestKind Kind
		{
			get { return ETestKind.A; }
		}

		public int Value
		{
			get { return m_value.Value; }
			set { m_value.Value = value; }
		}

		public byte[] Bytes
		{
			get { return m_bytes.Value; }
			set { m_bytes.Value = value; }
		}
	}
}