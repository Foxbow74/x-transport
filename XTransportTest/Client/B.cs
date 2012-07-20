using XTransport;

namespace XTransportTest.Client
{
	internal class B : XObject
	{
		[X("SVAL")] private IXValue<string> m_value;

		public override ETestKind Kind
		{
			get { return ETestKind.B; }
		}

		public string Value
		{
			get { return m_value.Value; }
			set { m_value.Value = value; }
		}
	}
}