using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XTransport.Emit
{
	public static class TypeExtension
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="getericTypeDefinition"></param>
		/// <returns></returns>
		public static bool HasGenericTypeDefinition(this Type source, Type getericTypeDefinition)
		{
			if (!source.IsGenericType) return false;
			return source.GetGenericTypeDefinition() == getericTypeDefinition;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="method"></param>
		/// <param name="attributes"></param>
		/// <param name="returnType"></param>
		/// <param name="name"></param>
		/// <param name="parameterTypes"></param>
		/// <returns></returns>
		public static bool HasMethod(this Type source, out MethodInfo method, MethodAttributes attributes, Type returnType, String name, params Type[] parameterTypes)
		{
			method = null;

			var methods =
				from m in source.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
				where
					m.Name == name &&
					(m.Attributes & attributes) == attributes &&
					m.ReturnType == returnType &&
					parameterTypes.AreEqual(m.GetParameters())
				select m;

			if (methods.Count() == 1) method = methods.First();
			return method != null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		public static bool AreEqual(this Type[] source, params ParameterInfo[] destination)
		{
			if (source.Length != destination.Length) return false;
			return !source.Where((t, i) => t != destination[i].ParameterType).Any();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static IEnumerable<Type> GetAllInterfaces(this Type source)
		{
			return source.IsInterface ? source.GetInterfaces().Union(new[] { source }) : source.GetInterfaces();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static String ToShortString(this Type source)
		{
			var buff = new StringBuilder(256);

			if (source.IsGenericType)
			{
				buff.Append(source.Name.Substring(0, source.Name.IndexOf('`')))
				    .Append('<')
				    .Append(String.Join(",", source.XGetGenericArguments().Select(x => x.ToShortString()).ToArray()))
				    .Append('>');
			}
			else if (source.HasElementType)
			{
				buff.Append(source.GetElementType().ToShortString())
				    .Append("[]");
			}
			else
			{
				buff.Append(source.Name);
			}

			return buff.ToString();
		}

		/// <summary>
		/// Возвращает массив аргументов для типа, унаследованного от RAW-generic
		/// </summary>
		/// <param name="toCheck"></param>
		/// <param name="baseRawGenericType">Базовый RAW-generic класс или интерфейс</param>
		/// <returns>Массив параметров Generic, либо null, если toCheck - не наследник baseRawGenericType</returns>
		public static Type[] TryGetGenericArguments(this Type toCheck, Type baseRawGenericType)
		{
			Guard.Argument.IsNotNull(toCheck, "toCheck");
			Guard.Argument.IsNotNull(baseRawGenericType, "baseRawGenericType");

			if (baseRawGenericType.IsInterface)
			{
				var interfaces = new List<Type>();
				if (toCheck.IsInterface)
					interfaces.Add(toCheck);
				interfaces.AddRange(toCheck.GetInterfaces());
				return (from interfaceType in interfaces
				        where interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == baseRawGenericType
				        select interfaceType.GetGenericArguments()).FirstOrDefault();
			}

			var baseType = toCheck;
			while (baseType != null)
			{
				if (baseType.IsGenericType &&
				    baseType.GetGenericTypeDefinition() == baseRawGenericType)
				{
					return baseType.GetGenericArguments();
				}
				baseType = baseType.BaseType;
			}
			return null;
		}

		/// <summary>
		/// Проверка, что тип унаследован или имплементит заданный
		/// </summary>
		/// <param name="toCheck"></param>
		/// <param name="baseType"></param>
		/// <returns></returns>
		public static bool IsSubclassOrImplements(this Type toCheck, Type baseType)
		{
			Guard.Argument.IsNotNull(toCheck, "toCheck");
			Guard.Argument.IsNotNull(baseType, "baseType");

			if (baseType.IsGenericTypeDefinition)
			{
				if (baseType.IsInterface)
				{
					return toCheck.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == baseType);
				}

				var type = toCheck;
				while (type != null)
				{
					if (type.IsGenericType &&
					    type.GetGenericTypeDefinition() == baseType)
					{
						return true;
					}
					type = type.BaseType;
				}
				return false;
			}
			return baseType.IsAssignableFrom(toCheck);
		}

	}
}