using XTransport;
using XTransportTest.Client;

namespace XTransportTest.ClientVM
{
	internal class ChildVM : XChildObjectVM<ParentVM>
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

		protected override void InstantiationFinished()
		{
			LinkProperty(m_value, () => Value);
		}
	}
}