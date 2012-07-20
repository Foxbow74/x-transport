using System.Collections.Generic;
using System.Collections.ObjectModel;
using XTransport;
using XTransportTest.Client;

namespace XTransportTest.ClientVM
{
	internal class ParentVM : XObjectVM
	{
		[X("LIST")] private IList<ChildVM> m_value;

		public override ETestKind Kind
		{
			get { return ETestKind.PARENT; }
		}

		public IList<ChildVM> Children
		{
			get { return m_value; }
		}

		public IList<ChildVM> List
		{
			get { return m_value; }
		}

		public ReadOnlyObservableCollection<ChildVM> ObsCol { get; private set; }

		protected override void InstantiationFinished()
		{
			ObsCol = CreateObservableCollection(m_value);
		}
	}
}