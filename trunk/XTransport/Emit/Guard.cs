using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace XTransport.Emit
{
	public class Guard
	{
		internal Guard()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public class Argument
		{
			internal Argument()
			{
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotEmpty(Guid argument, string argumentName)
			{
				if (argument == Guid.Empty)
					throw new ArgumentException("\"{0}\" cannot be empty guid.".FormatWith(argumentName), argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotEmpty(string argument, string argumentName)
			{
				if (string.IsNullOrEmpty((argument ?? string.Empty).Trim()))
					throw new ArgumentException("\"{0}\" cannot be blank.".FormatWith(argumentName), argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="length"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotOutOfLength(string argument, int length, string argumentName)
			{
				if (argument.Trim().Length > length)
					throw new ArgumentException("\"{0}\" cannot be more than {1} character.".FormatWith(argumentName, length), argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotNull(object argument, string argumentName)
			{
				if (argument == null)
					throw new ArgumentNullException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotNegative(int argument, string argumentName)
			{
				if (argument < 0)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotNegativeOrZero(int argument, string argumentName)
			{
				if (argument <= 0)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotNegative(long argument, string argumentName)
			{
				if (argument < 0)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotNegativeOrZero(long argument, string argumentName)
			{
				if (argument <= 0)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotNegative(float argument, string argumentName)
			{
				if (argument < 0)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotNegativeOrZero(float argument, string argumentName)
			{
				if (argument <= 0)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotNegative(decimal argument, string argumentName)
			{
				if (argument < 0)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotNegativeOrZero(decimal argument, string argumentName)
			{
				if (argument <= 0)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotInPast(DateTime argument, string argumentName)
			{
				if (argument < DateTime.Now)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotInFuture(DateTime argument, string argumentName)
			{
				if (argument > DateTime.Now)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotNegative(TimeSpan argument, string argumentName)
			{
				if (argument < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotNegativeOrZero(TimeSpan argument, string argumentName)
			{
				if (argument <= TimeSpan.Zero)
					throw new ArgumentOutOfRangeException(argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotEmpty<T>(ICollection<T> argument, string argumentName)
			{
				IsNotNull(argument, argumentName);

				if (argument.Count == 0)
					throw new ArgumentException("GuardCollectionCannotBeEmpty", argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			public static void IsNotEmpty<T>(IEnumerable<T> argument, string argumentName)
			{
				IsNotNull(argument, argumentName);

				if (!argument.Any())
					throw new ArgumentException("GuardCollectionCannotBeEmpty", argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="min"></param>
			/// <param name="max"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotOutOfRange(int argument, int min, int max, string argumentName)
			{
				if (argument < min || argument > max)
					throw new ArgumentOutOfRangeException(argumentName, "{0} must be between \"{1}\"-\"{2}\".".FormatWith(argumentName, min, max));
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotInvalidEmail(string argument, string argumentName)
			{
				IsNotEmpty(argument, argumentName);

				if (!argument.IsEmail())
					throw new ArgumentException("\"{0}\" is not a valid email address.".FormatWith(argumentName), argumentName);
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="argument"></param>
			/// <param name="argumentName"></param>
			[DebuggerStepThrough]
			public static void IsNotInvalidWebUrl(string argument, string argumentName)
			{
				IsNotEmpty(argument, argumentName);

				if (!argument.IsWebUrl())
					throw new ArgumentException("\"{0}\" is not a valid web url.".FormatWith(argumentName), argumentName);
			}

			/// <summary>
			/// Throws an exception of type <typeparamref name="TException"/> with the specified message
			/// when the assertion statement is true.
			/// </summary>
			/// <typeparam name="TException">The type of exception to throw.</typeparam>
			/// <param name="assertion">The assertion to evaluate. If true then the <typeparamref name="TException"/> exception is thrown.</param>
			/// <param name="message">string. The exception message to throw.</param>
			[DebuggerStepThrough]
			public static void Against<TException>(bool assertion, string message) where TException : Exception
			{
				if (assertion)
					throw (TException)Activator.CreateInstance(typeof(TException), message);
			}

			/// <summary>
			/// Throws an exception of type <typeparamref name="TException"/> with the specified message
			/// when the assertion
			/// </summary>
			/// <typeparam name="TException"></typeparam>
			/// <param name="assertion"></param>
			/// <param name="message"></param>
			[DebuggerStepThrough]
			public static void Against<TException>(Func<bool> assertion, string message) where TException : Exception
			{
				if (assertion())
					throw (TException)Activator.CreateInstance(typeof(TException), message);
			}

			/// <summary>
			/// Throws a <see cref="ArgumentException"/> when the specified object
			/// instance does not inherit from <typeparamref name="TBase"/> type.
			/// </summary>
			/// <typeparam name="TBase">The base type to check for.</typeparam>
			/// <param name="instance">The object to check if it inherits from <typeparamref name="TBase"/> type.</param>
			/// <param name="message">string. The exception message to throw.</param>
			[DebuggerStepThrough]
			public static void IsNotInheritsFrom<TBase>(object instance, string message) where TBase : Type
			{
				// IsNotNull(instance, "instance");
				IsNotInheritsFrom<TBase>(instance.GetType(), message);
			}

			/// <summary>
			/// Throws a <see cref="ArgumentException"/> when the specified type does not
			/// inherit from the <typeparamref name="TBase"/> type.
			/// </summary>
			/// <typeparam name="TBase">The base type to check for.</typeparam>
			/// <param name="type">The <see cref="Type"/> to check if it inherits from <typeparamref name="TBase"/> type.</param>
			/// <param name="message">string. The exception message to throw.</param>
			[DebuggerStepThrough]
			public static void IsNotInheritsFrom<TBase>(Type type, string message)
			{
				// IsNotNull(type, "type");
				if (!typeof(TBase).IsAssignableFrom(type))
					throw new ArgumentException(message);
			}

			/// <summary>
			/// Throws a <see cref="ArgumentException"/> when the specified object
			/// instance does not inherit from <typeparamref name="TBase"/> type.
			/// </summary>
			/// <typeparam name="TBase">The base type to check for.</typeparam>
			/// <param name="instance">The object to check if it inherits from <typeparamref name="TBase"/> type.</param>
			/// <param name="message">string. The exception message to throw.</param>
			[DebuggerStepThrough]
			public static void IsNotDerivedFrom<TBase>(object instance, string message) where TBase : Type
			{
				// IsNotNull(instance, "instance");
				IsNotDerivedFrom<TBase>(instance.GetType(), message);
			}

			/// <summary>
			/// Throws a <see cref="ArgumentException"/> when the specified type does not
			/// inherit from the <typeparamref name="TBase"/> type.
			/// </summary>
			/// <typeparam name="TBase">The base type to check for.</typeparam>
			/// <param name="type">The <see cref="Type"/> to check if it inherits from <typeparamref name="TBase"/> type.</param>
			/// <param name="message">string. The exception message to throw.</param>
			[DebuggerStepThrough]
			public static void IsNotDerivedFrom<TBase>(Type type, string message)
			{
				// IsNotNull(type, "type");
				if (type.BaseType != typeof(TBase))
					throw new ArgumentException(message);
			}

			/// <summary>
			/// Throws a <see cref="ArgumentException"/> when the specified object
			/// instance does not implement the <typeparamref name="TInterface"/> interface.
			/// </summary>
			/// <typeparam name="TInterface">The interface type the object instance should implement.</typeparam>
			/// <param name="instance">The object insance to check if it implements the <typeparamref name="TInterface"/> interface</param>
			/// <param name="message">string. The exception message to throw.</param>
			[DebuggerStepThrough]
			public static void IsNotImplements<TInterface>(object instance, string message)
			{
				// IsNotNull(instance, "instance");
				IsNotImplements<TInterface>(instance.GetType(), message);
			}

			/// <summary>
			/// Throws an <see cref="ArgumentException"/> when the specified type does not
			/// implement the <typeparamref name="TInterface"/> interface.
			/// </summary>
			/// <typeparam name="TInterface">The interface type that the <paramref name="type"/> should implement.</typeparam>
			/// <param name="type">The <see cref="Type"/> to check if it implements from <typeparamref name="TInterface"/> interface.</param>
			/// <param name="message">string. The exception message to throw.</param>
			[DebuggerStepThrough]
			public static void IsNotImplements<TInterface>(Type type, string message)
			{
				// IsNotNull(type, "type");

				// if (!typeof(TInterface).IsAssignableFrom(type))
				//     throw new ArgumentException(message);

				if (type.GetInterface(typeof(TInterface).Name) == null)
					throw new ArgumentException(message);
			}

			/// <summary>
			/// Throws an <see cref="ArgumentException"/> when the specified object instance is
			/// not of the specified type.
			/// </summary>
			/// <typeparam name="TType">The Type that the <paramref name="instance"/> is expected to be.</typeparam>
			/// <param name="instance">The object instance whose type is checked.</param>
			/// <param name="message">The message of the <see cref="ArgumentException"/> exception.</param>
			/// <param name="args"></param>
			[DebuggerStepThrough]
			public static void IsNotOfType<TType>(object instance, string message, params object[] args)
			{
				if (!(instance is TType))
					throw new ArgumentException(message.FormatWith(args));
			}
		}
	}
}