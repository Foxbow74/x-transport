using XTransport;
using XTransportTest.Client;

namespace XTransportTest.ClientVM
{
	internal class AMirrorVM : XObjectVM
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

		protected override void InstantiationFinished()
		{
			LinkProperty(m_value, () => Value);
		}
	}
}