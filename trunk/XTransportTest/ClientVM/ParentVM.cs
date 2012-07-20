using System.Collections.Generic;
using System.Collections.ObjectModel;
using XTransport;
using XTransportTest.Client;

namespace XTransportTest.ClientVM
{
	internal class ParentVM : XObjectVM
	{
		[X("LIST")] private ICollection<ChildVM> m_value;

		public override ETestKind Kind
		{
			get { return ETestKind.PARENT; }
		}

		public ICollection<ChildVM> Children
		{
			get { return m_value; }
		}

		public ICollection<ChildVM> List
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