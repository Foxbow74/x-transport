using System;
using System.Collections;
using System.Collections.Generic;

namespace XTransport.Server.Storage
{
	public interface IUValue
	{
		string Field { get; }
	}

	internal interface IUValueInternal : IUValue
	{
		IServerXValue XValue { get; }
	}

	public sealed class UValue<T> : IUValueInternal
	{
		private readonly ServerXValue<T> m_xValue;

		public UValue(string _field, T _value)
		{
			m_xValue = new ServerXValue<T> {Value = _value};
			Field = _field;
		}

		#region IUValueInternal Members

		public string Field { get; private set; }

		IServerXValue IUValueInternal.XValue
		{
			get { return m_xValue; }
		}

		#endregion
	}

	public class UplodableObject : IEnumerable
	{
		private readonly Dictionary<int, IServerXValue> m_xValues = new Dictionary<int, IServerXValue>();

		public UplodableObject(int _kind) : this(Guid.NewGuid(), _kind)
		{
		}

		public UplodableObject(Guid _uid, int _kind)
		{
			Uid = _uid;
			Kind = _kind;
		}

		public Guid Uid { get; private set; }
		public int Kind { get; private set; }

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		#endregion

		public void SaveFields(SQLiteStorage _storage, DateTime _now)
		{
			foreach (var xValue in m_xValues)
			{
				xValue.Value.SaveValue(Uid, xValue.Key.GetHashCode(), _storage, _now);
			}
		}

		public void AddField<T>(int _field, T _value)
		{
			m_xValues.Add(_field, new ServerXValue<T>() {Value = _value});
		}

		public void AddField<T>(string _field, T _value)
		{
			m_xValues.Add(_field.GetHashCode(), new ServerXValue<T>() {Value = _value});
		}

		public void Add(IUValue _uValue)
		{
			m_xValues.Add(_uValue.Field.GetHashCode(), ((IUValueInternal) _uValue).XValue);
		}
	}
}