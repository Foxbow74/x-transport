using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace XTransport.Emit
{
	public static class StringExtensions
	{
		private static readonly Regex _guidExpression = new Regex(@"^(\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\}{0,1})$", RegexOptions.Compiled);
		private static readonly Regex _emailExpression = new Regex(@"^([0-9a-zA-Z]+[-._+&])*[0-9a-zA-Z]+@([-0-9a-zA-Z]+[.])+[a-zA-Z]{2,6}$", RegexOptions.Singleline | RegexOptions.Compiled);
		private static readonly Regex _webUrlExpression = new Regex(@"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.Singleline | RegexOptions.Compiled);
		private static readonly Lazy<IFormatProvider> _formatProvider = new Lazy<IFormatProvider>(() => new FormatProvider());

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static bool IsGuid(this string value)
		{
			return !string.IsNullOrEmpty(value)
			       && value.Contains("-")
			       && _guidExpression.IsMatch(value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static bool IsEmail(this string value)
		{
			return !string.IsNullOrEmpty(value)
			       && value.Contains("@")
			       && _emailExpression.IsMatch(value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static bool IsWebUrl(this string value)
		{
			return !string.IsNullOrEmpty(value)
			       && value.Contains("://")
			       && _webUrlExpression.IsMatch(value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static string FromMd5(this string value)
		{
			var provider = new MD5CryptoServiceProvider();
			var bytes = Encoding.UTF8.GetBytes(value);
			var builder = new StringBuilder();

			bytes = provider.ComputeHash(bytes);

			foreach (var @byte in bytes)
				builder.Append(@byte.ToString("x2").ToLower());

			return builder.ToString();
		}

		/// <summary>
		/// Apply String.Format Using This String As Format String
		/// </summary>
		/// <param name="value">This Format String</param>
		/// <param name="args">An Object array containing zero or more objects to format.</param>
		/// <returns>A copy of format in which the format items have been replaced by the 
		/// String equivalent of the corresponding instances of Object in args.</returns>
		/// <remarks>New format specifications:
		/// $1:(OneForm):(MultiForm) - для случая один или много
		/// $n:(1):(2):(5) - для склонения числа вещей   "{0} {0:$n:арбуз:арбуза:арбузов}".FormatWith(n)
		/// $b             - для количества байт/килобайт мегабайт "{0:$b}".FormatWith(bytesCount)
		/// </remarks>
		[DebuggerStepThrough]
		public static string FormatWith(this string value, params object[] args)
		{
			if (string.IsNullOrEmpty((value ?? string.Empty).Trim()))
				throw new ArgumentException("value");
			// ReSharper disable AssignNullToNotNullAttribute
			return string.Format(_formatProvider.Value, value, args);
			// ReSharper restore AssignNullToNotNullAttribute
		}

		/// <summary>
		/// Extension shortcut to String.IsNullOrEmpty
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

		/// <summary>
		/// Extension shortcut to String.IsNullOrEmpty
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="compareString"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static int LevenshteinDistance(this string value, string compareString)
		{
			return LevenshteinDistance(value, compareString, false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string Left(this string value, int length)
		{
			if (length < 0)
				throw new ArgumentException(@"Length Must Be > 0.", "length");
			if (value == null || value.Length < length)
				return value;
			return value.Substring(0, length);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string Right(this string value, int length)
		{
			if (length < 0)
				throw new ArgumentException(@"Length Must Be > 0.", "length");
			if (value == null || value.Length < length)
				return value;
			return value.Substring(value.Length - length, length);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="startIndex"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string Mid(this string value, int startIndex, int length)
		{
			if (startIndex < 0)
				throw new ArgumentException(@"StartIndex Must Be > 0.", "startIndex");

			if (length < 0)
				throw new ArgumentException(@"Length Must Be > 0.", "length");

			return value.Substring(startIndex, length);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="startIndex"></param>
		/// <returns></returns>
		public static string Mid(this string value, int startIndex)
		{
			if (startIndex < 0)
				throw new ArgumentException(@"StartIndex Must Be > 0.", "startIndex");

			return value.Substring(startIndex);
		}

		/// <summary>
		/// Усекает строку до указанной длины. При усечении дополняет многоточиями
		/// </summary>
		/// <param name="s"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public static string TrimWithPeriods(this string s, int length)
		{
			if (length < 3)
				throw new ArgumentException(@"Length Must Be > 3.", "length");
			if (s == null || s.Length < length)
				return s;

			return s.Left(length - 3) + "...";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="delimiter"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public static string JoinNotEmpty(this string delimiter, params string[] values)
		{
			return delimiter.JoinNotEmpty((IEnumerable<string>)values);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="delimiter"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		public static string JoinNotEmpty(this string delimiter, IEnumerable<string> values)
		{
			var sb = new StringBuilder();
			foreach (var s in values.Where(s => !IsNullOrEmpty(s)))
			{
				(sb.Length == 0 ? sb : sb.Append(delimiter)).Append(s);
			}
			return sb.ToString();
		}

		/// <summary>
		/// Возвращает null, если строка пустая или пробельная
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string NullIfWhiteSpace(this string s)
		{
			return (String.IsNullOrWhiteSpace(s)) ? null : s;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="compareString"></param>
		/// <param name="removeSpecialChars"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public static int LevenshteinDistance(this string value, string compareString, bool removeSpecialChars)
		{
			Guard.Argument.IsNotNull(value, "value");
			Guard.Argument.IsNotNull(compareString, "compareString");

			if (removeSpecialChars)
			{
				value = Regex.Replace(value.ToLower(), "\\W", string.Empty);
				compareString = Regex.Replace(compareString.ToLower(), "\\W", string.Empty);
			}

			var rowLen = value.Length;          // length of sRow
			var colLen = compareString.Length;  // length of sCol

			// Test string length
			if (Math.Max(value.Length, compareString.Length) > Math.Pow(2, 31))
				throw (new Exception(string.Format("Maximum string length in Levenshtein.iLD is {0} {1} {2}.", Math.Pow(2, 31), ".\nYours is ", Math.Max(value.Length, compareString.Length))));

			// Step 1
			if (rowLen == 0)
				return colLen;

			if (colLen == 0)
				return rowLen;

			// Create the two vectors
			var v0 = new int[rowLen + 1];
			var v1 = new int[rowLen + 1];

			int rowIndex;                       // iterates through sRow
			int colIndex;                       // iterates through sCol

			// ReSharper disable TooWideLocalVariableScope
			char rowI;                         // ith character of sRow
			char colJ;                         // jth character of sCol
			int cost;                          // cost
			int[] vTmp;
			// ReSharper restore TooWideLocalVariableScope

			// Step 2
			// Initialize the first vector
			for (rowIndex = 1; rowIndex <= rowLen; rowIndex++)
			{
				v0[rowIndex] = rowIndex;
			}

			// Step 3 For each column
			for (colIndex = 1; colIndex <= colLen; colIndex++)
			{
				// Set the 0'th element to the column number
				v1[0] = colIndex;
				colJ = compareString[colIndex - 1];

				// Step 4 Fore each row
				for (rowIndex = 1; rowIndex <= rowLen; rowIndex++)
				{
					rowI = value[rowIndex - 1];

					// Step 5
					cost = (rowI == colJ ? 0 : 1);

					// Step 6 Find minimum
					var min = v0[rowIndex] + 1;
					var b = v1[rowIndex - 1] + 1;
					var c = v0[rowIndex - 1] + cost;

					if (b < min)
						min = b;

					if (c < min)
						min = c;

					v1[rowIndex] = min;
				}

				// Swap the vectors
				vTmp = v0;
				v0 = v1;
				v1 = vTmp;
			}


			// Step 7

			// Value between 0 - 100
			// 0 == perfect match, 100 == totaly different

			// The vectors where swaped one last time at the end of the last loop,
			// that is why the result is now in v0 rather than in v1
			// System.Console.WriteLine("iDist=" + v0[rowLen]);
			var max = Math.Max(rowLen, colLen);
			return ((100 * v0[rowLen]) / max);
		}

		#region FormatProvider
		private class FormatProvider : IFormatProvider, ICustomFormatter
		{
			#region IFormatProvider Members
			public object GetFormat(Type formatType)
			{
				return typeof(ICustomFormatter).Equals(formatType) ? this
					       : System.Globalization.CultureInfo.CurrentCulture.GetFormat(formatType);
			}
			#endregion

			#region ICustomFormatter Members
			public string Format(string format, object arg, IFormatProvider formatProvider)
			{
				int formatType = 0;
				if (format.IsNullOrEmpty()) formatType = 0;
				else if (format.StartsWith("$1:")) formatType = 1;
				else if (format.StartsWith("$n:")) formatType = 2;
				else if (format.StartsWith("$b")) formatType = 3;

				if ((arg == null) || (!IsNumericType(arg.GetType()))) formatType = 0;

				switch (formatType)
				{
					case 1:
						return FormatOneOrMany(format, arg);
					case 2:
						return FormatPlural(format, arg);
					case 3:
						return FormatBytes(arg);
				}
				return (arg == null) ? string.Empty :
					       (arg is IFormattable) ?
						       ((IFormattable)arg).ToString(format, formatProvider) :
						       arg.ToString();
			}
			#endregion

			private static string FormatBytes(object arg)
			{
				var num = Convert.ToInt64(arg);
				double value = num;
				string measure;
				var fmt = "G3";
				if (num < 1024)
				{
					return String.Format("{0} б", num);
				}
				if (num < 1024 * 1024)
				{
					measure = "кб";
					value /= 1024;
				}
				else if (num < 1024 * 1024 * 1024)
				{
					measure = "Мб";
					value /= (1024 * 1024);
				}
				else
				{
					measure = "Гб";
					value /= (1024 * 1024 * 1024);
				}
				if (value > 999) fmt = "N0";
				return value.ToString(fmt) + " " + measure;
			}

			private static string FormatOneOrMany(string format, object arg)
			{
				Int64 num;
				if (!IsIntegerType(arg.GetType()) && !IsRounded(Convert.ToDouble(arg))) num = 10;
				else num = Convert.ToInt64(arg);

				var parts = format.Split(':');
				if (parts.Length != 3)
					throw new FormatException("Формат для $1 должен содержать 2 элемента: для \"1\" и \"много\"");
				return num == 1 ? parts[1] : parts[2];
			}

			private static string FormatPlural(string format, object arg)
			{
				var parts = format.Split(':');
				if (parts.Length != 4)
					throw new FormatException("Формат для $n должен содержать 3 элемента: для 1, 234 и 567890 ");
				var nForm = 2;
				if (IsIntegerType(arg.GetType()) || IsRounded(Convert.ToDouble(arg)))
				{
					nForm = 3;
					var num = Convert.ToInt64(arg);
					var n100 = (int)(num % 100);
					var n1 = n100 % 10;
					if (n100 >= 11 && n100 <= 19) nForm = 3;
					else
						switch (n1)
						{
							case 4:
							case 3:
							case 2:
								nForm = 2;
								break;
							case 1:
								nForm = 1;
								break;
						}
				}
				return parts[nForm];

			}

			private static bool IsRounded(double dblVal)
			{
				var remainder = Math.Abs(dblVal - Math.Truncate(dblVal));
				return remainder < 0.0000001;
			}

			private static bool IsIntegerType(Type t)
			{
				if (!IsNumericType(t)) return false;

				switch (Type.GetTypeCode(t))
				{
					case TypeCode.Decimal:
					case TypeCode.Double:
					case TypeCode.Single:
						return false;
				}
				return true;
			}

			private static bool IsNumericType(Type t)
			{
				switch (Type.GetTypeCode(t))
				{
					case TypeCode.Byte:
					case TypeCode.Decimal:
					case TypeCode.Double:
					case TypeCode.Int16:
					case TypeCode.Int32:
					case TypeCode.Int64:
					case TypeCode.SByte:
					case TypeCode.Single:
					case TypeCode.UInt16:
					case TypeCode.UInt32:
					case TypeCode.UInt64:
						return true;
				}
				return false;
			}

		}
		#endregion
	}
}