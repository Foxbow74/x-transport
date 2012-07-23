using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using XTransport.Client;

namespace XTransportTest.Client
{
	internal class TstClient : AbstractXClient<ETestKind>
	{
		private static Dispatcher m_dsp;
		private static readonly Thread m_thread;
		private readonly Guid m_userUid;

		static TstClient()
		{
			m_thread = new Thread(_o =>
			                      	{
			                      		m_dsp = Dispatcher.CurrentDispatcher;
			                      		Dispatcher.Run();
			                      	}) {IsBackground = true};

			m_thread.SetApartmentState(ApartmentState.STA);
		}


		protected override IEnumerable<KeyValuePair<ETestKind, ETestKind>> GetAbstractRootKindMap()
		{
			yield return new KeyValuePair<ETestKind, ETestKind>(ETestKind.A, ETestKind.AB);
			yield return new KeyValuePair<ETestKind, ETestKind>(ETestKind.B, ETestKind.AB);
			yield return new KeyValuePair<ETestKind, ETestKind>(ETestKind.A, ETestKind.ALL);
			yield return new KeyValuePair<ETestKind, ETestKind>(ETestKind.B, ETestKind.ALL);
			yield return new KeyValuePair<ETestKind, ETestKind>(ETestKind.PARENT, ETestKind.ALL);
			yield return new KeyValuePair<ETestKind, ETestKind>(ETestKind.REF, ETestKind.ALL);
		}

		public TstClient()
		{
			m_userUid = Guid.NewGuid();
		}

		public override Guid UserUid
		{
			get { return m_userUid; }
		}

		protected override int KindToInt(ETestKind _kind)
		{
			return (int) _kind;
		}

		protected override ETestKind IntToKind(int _kind)
		{
			return (ETestKind) _kind;
		}

		protected override Dispatcher GetUiDispatcher()
		{
			if (!m_thread.IsAlive)
			{
				m_thread.Start();
			}
			while (m_dsp == null)
			{
				Thread.Sleep(10);
			}
			return m_dsp;
		}

		protected override void ObjectReleased(Guid _uid, ETestKind _kind)
		{
		}
	}
}