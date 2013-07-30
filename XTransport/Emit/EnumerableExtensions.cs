using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XTransport.Emit
{
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Execute the provided <paramref name="action"/> on every item in <paramref name="sequence"/>.
		/// </summary>
		/// <typeparam name="TItem">Type of the items stored in <paramref name="sequence"/></typeparam>
		/// <param name="sequence">Sequence of items to process.</param>
		/// <param name="action">Code to run over each item.</param>
		public static void ForEach<TItem>(this IEnumerable<TItem> sequence, Action<TItem> action)
		{
			if (sequence == null)
				return;

			foreach (var item in sequence)
			{
				action(item);
			}
		}

		/// <summary>
		/// Any() extension with null check
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <returns></returns>
		public static bool SafeAny<T>(this IEnumerable<T> enumerable)
		{
			return enumerable != null && enumerable.Any();
		}

		/// <summary>
		/// Any() extension with null check
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static bool SafeAny<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
		{
			return enumerable != null && enumerable.Any(predicate);
		}

		/// <summary>
		/// FirstOrDefault() extension with null check
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <returns></returns>
		public static T SafeFirstOrDefault<T>(this IEnumerable<T> enumerable)
		{
			return enumerable == null ? default(T) : enumerable.FirstOrDefault();
		}

		/// <summary>
		/// Check if Enumerable Is Null Or Empty
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="seq"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> seq)
		{
			return seq == null || !seq.Any();
		}

		/// <summary>
		/// Check if Enumerable Is Null Or Empty
		/// </summary>
		/// <param name="seq"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(this IEnumerable seq)
		{
			return seq == null || !(seq.Cast<object>().Any());
		}

		/// <summary>
		/// Compare element-by element using predicate
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <param name="seq1"></param>
		/// <param name="seq2"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static bool AllMatch<T1, T2>(this IEnumerable<T1> seq1, IEnumerable<T2> seq2, Func<T1, T2, bool> predicate)
		{
			return AllMatch(seq1, seq2, (t1, t2, i) => predicate(t1, t2));
		}

		/// <summary>
		/// Compare element-by element using predicate
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <param name="seq1"></param>
		/// <param name="seq2"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static bool AllMatch<T1, T2>(this IEnumerable<T1> seq1, IEnumerable<T2> seq2, Func<T1, T2, int, bool> predicate)
		{
			var seq1E = (seq1 ?? Enumerable.Empty<T1>()).GetEnumerator();
			var seq2E = (seq2 ?? Enumerable.Empty<T2>()).GetEnumerator();

			var i = 0;
			while (true)
			{
				bool next1 = seq1E.MoveNext(), next2 = seq2E.MoveNext();
				if (next1 ^ next2)
				{
					return false;
				}
				if (!next1)
				{
					return true;
				}
				if (!predicate(seq1E.Current, seq2E.Current, i++))
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Return Max or, if not any, default value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="seq"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static T MaxOrDefault<T>(this IEnumerable<T> seq, T defaultValue)
		{
			// ReSharper disable PossibleMultipleEnumeration
			return seq.SafeAny() ? seq.Max() : defaultValue;
			// ReSharper restore PossibleMultipleEnumeration
		}

		/// <summary>
		/// Return Max or, if not any, default value
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <typeparam name="TElement"></typeparam>
		/// <param name="seq"></param>
		/// <param name="selector"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static TValue MaxOrDefault<TValue, TElement>(this IEnumerable<TElement> seq, Func<TElement, TValue> selector, TValue defaultValue)
		{
			// ReSharper disable PossibleMultipleEnumeration
			return !seq.SafeAny() ? defaultValue : seq.Max(selector);
			// ReSharper restore PossibleMultipleEnumeration
		}

		/// <summary>
		/// Return Min or, if not any, default value
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="seq"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static T MinOrDefault<T>(this IEnumerable<T> seq, T defaultValue)
		{
			// ReSharper disable PossibleMultipleEnumeration
			return seq.SafeAny() ? seq.Min() : defaultValue;
			// ReSharper restore PossibleMultipleEnumeration
		}

		/// <summary>
		/// Return Min or, if not any, default value
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <typeparam name="TElement"></typeparam>
		/// <param name="seq"></param>
		/// <param name="selector"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public static TValue MinOrDefault<TValue, TElement>(this IEnumerable<TElement> seq, Func<TElement, TValue> selector, TValue defaultValue)
		{
			// ReSharper disable PossibleMultipleEnumeration
			return !seq.SafeAny() ? defaultValue : seq.Min(selector);
			// ReSharper restore PossibleMultipleEnumeration
		}
	}
}