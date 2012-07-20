using System.Collections.Generic;
using System.Collections.ObjectModel;
using XTransport;
using XTransportTest.Client;

namespace XTransportTest.ClientVM
{
	internal class ParentMirrorVM : XObjectVM
	{
		[X("LIST")] private IList<ChildMirrorVM> m_value;

		public override ETestKind Kind
		{
			get { return ETestKind.PARENT; }
		}

		public IList<ChildMirrorVM> Children
		{
			get { return m_value; }
		}

		public IList<ChildMirrorVM> List
		{
			get { return m_value; }
		}

		public ReadOnlyObservableCollection<ChildMirrorVM> ObsCol { get; private set; }

		protected override void InstantiationFinished()
		{
			ObsCol = CreateObservableCollection(m_value);
		}
	}
}