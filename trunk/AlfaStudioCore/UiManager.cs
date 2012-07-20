using System;
using System.Collections.Generic;
using System.Linq;
using AlphaXTransport;

namespace AlphaStudioCore
{
	public enum EDocumentCommand
	{
		OPEN,
		OPEN_IN_NEW_TAB,
		SELECTED,
		ADD_TO_FAVORITS
	}

	public static class UiManager
	{
		public static event Action<EUiEvent, EAlphaKind, Guid> UIMessage;

		public static void CastUiMessage(EUiEvent _event, EAlphaKind _kind, Guid _id)
		{
			if (UIMessage != null)
			{
				UIMessage(_event, _kind, _id);
			}
		}

		public static event Action<EAlphaToolKind> ShowToolWindow;

		public static event Action<IAlphaRootToolDescriptor> ShowRootToolWindow;

		public static void CastShowToolWindow(EAlphaToolKind _kind)
		{
			if (ShowToolWindow != null)
			{
				ShowToolWindow(_kind);
			}
		}

		public static void CastShowRootToolWindow(IAlphaRootToolDescriptor _descriptor)
		{
			if (ShowRootToolWindow != null)
			{
				ShowRootToolWindow(_descriptor);
			}
		}

		public static event Action<EDocumentCommand, EAlphaKind, EAlphaDocumentKind, Guid> DocumentEvent;

		public static void CastDocumentCommand(EDocumentCommand _command, EAlphaKind _kind, EAlphaDocumentKind _doc, Guid _uid)
		{
			if (DocumentEvent != null)
			{
				DocumentEvent(_command, _kind, _doc, _uid);
			}
		}

		#region tools

		private static readonly Dictionary<EAlphaToolKind, AlphaToolDescriptor> m_toolDescriptors =
			new Dictionary<EAlphaToolKind, AlphaToolDescriptor>();

		public static void RegisterDescriptor(AlphaToolDescriptor _descriptor)
		{
			m_toolDescriptors.Add(_descriptor.Kind, _descriptor);
		}

		public static AlphaToolDescriptor GetDescriptor(EAlphaToolKind _eAlphaToolKind)
		{
			return m_toolDescriptors[_eAlphaToolKind];
		}

		public static IEnumerable<AlphaToolDescriptor> ToolDescriptors
		{
			get { return m_toolDescriptors.Values; }
		}

		#endregion

		#region root tools

		private static readonly List<IAlphaRootToolDescriptor> m_rootToolDescriptors = new List<IAlphaRootToolDescriptor>();

		public static void RegisterDescriptor(IAlphaRootToolDescriptor _descriptor)
		{
			m_rootToolDescriptors.Add(_descriptor);
		}

		public static IEnumerable<IAlphaRootToolDescriptor> GetRootToolDescriptors()
		{
			return m_rootToolDescriptors;
		}

		public static IAlphaRootToolDescriptor GetRootToolDescriptor(string _rootToolIdentifier)
		{
			return m_rootToolDescriptors.SingleOrDefault(_descriptor => _descriptor.RootToolIdentifier == _rootToolIdentifier);
		}

		#endregion


		#region documents

		private static readonly Dictionary<EAlphaKind, Dictionary<EAlphaDocumentKind, IAlphaDocumentDescriptor>> m_documentDescriptors = new Dictionary<EAlphaKind, Dictionary<EAlphaDocumentKind, IAlphaDocumentDescriptor>>();

		public static void RegisterDescriptor(IAlphaDocumentDescriptor _descriptor)
		{
			Dictionary<EAlphaDocumentKind, IAlphaDocumentDescriptor> byModel;
			if (!m_documentDescriptors.TryGetValue(_descriptor.Kind, out byModel))
			{
				byModel = new Dictionary<EAlphaDocumentKind, IAlphaDocumentDescriptor>();
				m_documentDescriptors.Add(_descriptor.Kind, byModel);
			}
			byModel.Add(_descriptor.DocKind, _descriptor);
		}

		public static IEnumerable<IAlphaDocumentDescriptor> GetDocumentDescriptors(EAlphaKind _model)
		{
			return m_documentDescriptors[_model].Values;
		}

		public static IAlphaDocumentDescriptor GetDescriptor(EAlphaKind _model, EAlphaDocumentKind _kind)
		{
			return m_documentDescriptors[_model][_kind];
		}

		#endregion

		public static string GetShortName(this EAlphaDocumentKind _documentKind)
		{
			switch (_documentKind)
			{
				case EAlphaDocumentKind.BLANK:
					return "blank";
				case EAlphaDocumentKind.VIEW:
					return "vw";
				case EAlphaDocumentKind.EDIT:
					return "ed";
				case EAlphaDocumentKind.PORTFOLIO_POSITIONS:
					return "pos";
				default:
					throw new ArgumentOutOfRangeException("_documentKind");
			}
		}

		public static string GetDocumentAddress(this EAlphaDocumentKind _documentKind, Guid _uid)
		{
			return _documentKind.GetShortName() + ":" + _uid;
		}

	}
}
