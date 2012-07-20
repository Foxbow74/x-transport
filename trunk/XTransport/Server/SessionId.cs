using System;
using System.Runtime.Serialization;

namespace XTransport.Server
{
	[DataContract]
	internal class SessionId
	{
		[DataMember] private readonly Int32 m_value;

		public SessionId(int _val)
		{
			m_value = _val;
		}

		public override bool Equals(object _obj)
		{
			return m_value == ((SessionId) _obj).m_value;
		}

		public bool Equals(SessionId _other)
		{
			return _other.m_value == m_value;
		}

		public static bool operator ==(SessionId _first, SessionId _second)
		{
			if (ReferenceEquals(_first, _second))
				return true;

			if (((object) _first == null) || ((object) _second == null))
				return false;

			return _first.m_value == _second.m_value;
		}

		public static bool operator !=(SessionId _first, SessionId _second)
		{
			return !(_first == _second);
		}

		public override int GetHashCode()
		{
			return m_value;
		}

		public override String ToString()
		{
			return m_value.ToString();
		}
	}
}