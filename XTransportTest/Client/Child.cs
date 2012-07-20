using XTransport;

namespace XTransportTest.Client
{
	internal class Child : XChildObject<Parent>
	{
		[X("CVAL")] private IXValue<double> m_value;

		public double Value
		{
			get { return m_value.Value; }
			set { m_value.Value = value; }
		}

		public override ETestKind Kind
		{
			get { return ETestKind.CHILD; }
		}
	}
}