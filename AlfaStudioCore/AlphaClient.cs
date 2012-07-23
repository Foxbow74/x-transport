using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using XTransport.Client;
using AlphaXTransport;

namespace AlphaStudioCore
{
	public class AlphaClient : AbstractXClient<EAlphaKind>
	{
		private static AlphaClient m_instance;
		private static readonly object m_syncRoot = new object();

		public static AlphaClient Instance
		{
			get
			{
				if (m_instance != null)
				{
					return m_instance;
				}
				if (Monitor.TryEnter(m_syncRoot, 5000))
				{
					if (m_instance == null)
					{
						m_instance = new AlphaClient();
					}
					Monitor.Exit(m_syncRoot);
					return m_instance;
				}
				throw new ApplicationException("Can't instaniate AlphaClient");
			}
		}

		protected override IEnumerable<KeyValuePair<EAlphaKind, EAlphaKind>> GetAbstractRootKindMap()
		{
			yield return new KeyValuePair<EAlphaKind, EAlphaKind>(EAlphaKind.CURRENCY_PAIR, EAlphaKind.ASSET);
			yield return new KeyValuePair<EAlphaKind, EAlphaKind>(EAlphaKind.BOND, EAlphaKind.ASSET);
			yield return new KeyValuePair<EAlphaKind, EAlphaKind>(EAlphaKind.INDEX, EAlphaKind.ASSET);
			yield return new KeyValuePair<EAlphaKind, EAlphaKind>(EAlphaKind.FORWARD, EAlphaKind.DERIVATIVE);
			yield return new KeyValuePair<EAlphaKind, EAlphaKind>(EAlphaKind.FUTURES, EAlphaKind.DERIVATIVE);
		}

		public override Guid UserUid
		{
			get { return new Guid("967FE308-3FA3-45E4-9C38-1E234B673AF9"); }
		}

		protected override int KindToInt(EAlphaKind _kind)
		{
			return (int)_kind;
		}

		protected override EAlphaKind IntToKind(int _kind)
		{
			return (EAlphaKind) _kind;
		}

		protected override Dispatcher GetUiDispatcher()
		{
			return Dispatcher.CurrentDispatcher;
		}

		protected override void ObjectReleased(Guid _uid, EAlphaKind _kind)
		{
			UiManager.CastUiMessage(EUiEvent.REF_DELETED, _kind, _uid);
		}
	}
}
