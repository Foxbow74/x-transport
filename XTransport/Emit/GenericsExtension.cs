using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XTransport.Emit
{
	public static class GenericsExtension
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Type[] XGetGenericArguments(this Type t)
		{
			if (t == null) return null;
			return t.IsGenericType ? t.GetGenericArguments() : new Type[0];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mb"></param>
		/// <returns></returns>
		public static Type[] XGetGenericArguments(this MethodBase mb)
		{
			if (mb == null) return null;
			return mb.IsGenericMethod ? mb.GetGenericArguments() : new Type[0];
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Type XGetGenericDefinition(this Type t)
		{
			if (t == null) return null;
			return t.IsGenericType ? t.GetGenericTypeDefinition() : t;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mb"></param>
		/// <returns></returns>
		public static MethodBase XGetGenericDefinition(this MethodBase mb)
		{
			if (mb == null) return null;
			var mi = mb as MethodInfo;
			return mi == null ? mb : mi.XGetGenericDefinition();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mi"></param>
		/// <returns></returns>
		public static MethodInfo XGetGenericDefinition(this MethodInfo mi)
		{
			if (mi == null) return null;
			return mi.IsGenericMethod ? mi.GetGenericMethodDefinition() : mi;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="t"></param>
		/// <param name="targs"></param>
		/// <returns></returns>
		public static Type XMakeGenericType(this Type t, IEnumerable<Type> targs)
		{
			return t.XMakeGenericType(targs.ToArray());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="t"></param>
		/// <param name="targs"></param>
		/// <returns></returns>
		public static Type XMakeGenericType(this Type t, params Type[] targs)
		{
			if (t == null) return null;
			if (!t.IsGenericType || t.IsGenericParameter)
			{
				if (targs.Length != 0)
				{
					throw new NotSupportedException(targs.Length.ToString());
				}
				return t;
			}
			return t.GetGenericTypeDefinition().MakeGenericType(targs);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mb"></param>
		/// <param name="margs"></param>
		/// <returns></returns>
		public static MethodInfo XMakeGenericMethod(this MethodBase mb, IEnumerable<Type> margs)
		{
			return mb.XMakeGenericMethod(margs.ToArray());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mb"></param>
		/// <param name="margs"></param>
		/// <returns></returns>
		public static MethodInfo XMakeGenericMethod(this MethodBase mb, params Type[] margs)
		{
			return mb.XMakeGenericMethod(mb.DeclaringType.XGetGenericArguments(), margs);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mb"></param>
		/// <param name="targs"></param>
		/// <param name="margs"></param>
		/// <returns></returns>
		public static MethodInfo XMakeGenericMethod(this MethodBase mb, IEnumerable<Type> targs, Type[] margs)
		{
			return mb.XMakeGenericMethod(targs.ToArray(), margs.ToArray());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mb"></param>
		/// <param name="targs"></param>
		/// <param name="margs"></param>
		/// <returns></returns>
		public static MethodInfo XMakeGenericMethod(this MethodBase mb, Type[] targs, IEnumerable<Type> margs)
		{
			return mb.XMakeGenericMethod(targs.ToArray(), margs.ToArray());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mb"></param>
		/// <param name="targs"></param>
		/// <param name="margs"></param>
		/// <returns></returns>
		public static MethodInfo XMakeGenericMethod(this MethodBase mb, IEnumerable<Type> targs, IEnumerable<Type> margs)
		{
			return mb.XMakeGenericMethod(targs.ToArray(), margs.ToArray());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mb"></param>
		/// <param name="targs"></param>
		/// <param name="margs"></param>
		/// <returns></returns>
		public static MethodInfo XMakeGenericMethod(this MethodBase mb, Type[] targs, Type[] margs)
		{
			if (mb == null) return null;
			var pattern = (MethodInfo)mb.Module.ResolveMethod(mb.MetadataToken);

			var typeImpl = pattern.DeclaringType;
			if (!targs.IsNullOrEmpty()) typeImpl = typeImpl.MakeGenericType(targs);

			var methodImpl = typeImpl.GetMethods(BF.All).Single(mi2 => mi2.SameMetadataToken(mb));
			if (!margs.IsNullOrEmpty()) methodImpl = methodImpl.MakeGenericMethod(margs);

			return methodImpl;
		}
	}
}