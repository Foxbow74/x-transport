using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XTransport.Emit
{
	public static class ReflectionHelper
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="t"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Type GetFieldOrPropertyType(this Type t, String name)
		{
			return t.GetFieldOrProperty(name).GetFieldOrPropertyType();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mi"></param>
		/// <returns></returns>
		public static Type GetFieldOrPropertyType(this MemberInfo mi)
		{
			if (mi == null)
			{
				return null;
			}
			return mi is FieldInfo ? ((FieldInfo)mi).FieldType : ((PropertyInfo)mi).PropertyType;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="t"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static MemberInfo GetFieldOrProperty(this Type t, String name)
		{
			if (t == null)
			{
				return null;
			}

			// only public for now
			var fi = t.GetField(name, BF.PublicInstance);
			if (fi != null)
			{
				return fi;
			}

			var pi = t.GetProperty(name, BF.PublicInstance);
			if (pi != null)
			{
				return pi;
			}

			return null;
		}

		// Has a multitude of uses, but primary of those is  finding out
		// whether some type is a closed generic for some other type
		// e.g. SameMetadataToken(List<int>, List<>) will return true
		// for comparison, typeof(List<int>) == typeof(List<>) will return false
		/// <summary>
		/// 
		/// </summary>
		/// <param name="t1"></param>
		/// <param name="t2"></param>
		/// <returns></returns>
		public static bool SameMetadataToken(this MemberInfo t1, MemberInfo t2)
		{
			return t1.Module.Assembly == t2.Module.Assembly &&
			       t1.Module == t2.Module &&
			       t1.MetadataToken == t2.MetadataToken;
		}

		/// <summary>
		/// Check if t1 is same as t2
		/// </summary>
		/// <param name="t1"></param>
		/// <param name="t2"></param>
		/// <returns></returns>
		public static bool SameType(this Type t1, Type t2)
		{
			if (!t1.SameBasisType(t2))
			{
				return false;
			}
			var t1Args = t1.XGetGenericArguments();
			var t2Args = t2.XGetGenericArguments();

			if (t1Args.Length != t2Args.Length)
			{
				throw new NotSupportedException(String.Format("Something was overlooked: " +
				                                              "The type '{0}' and '{1}' claimed that they share basis type", t1, t2));
			}
			return t1Args.AllMatch(t2Args,
			                       (t1Argsi, t2Argsi) => t1Argsi.SameType(t2Argsi));
		}

		/// <summary>
		/// Get Basis Generic definition or this type
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Type GetBasisType(this Type t)
		{
			return t.XGetGenericDefinition();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="t1"></param>
		/// <param name="t2"></param>
		/// <returns></returns>
		public static bool SameBasisType(this Type t1, Type t2)
		{
			if (t1 == null || t2 == null)
			{
				return t1 == t2;
			}
			return t1.SameMetadataToken(t2);
		}

		/// <summary>
		/// Get array of basic types
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static Type[] GetBaseTypes(this Type t)
		{
			if (t.IsInterface)
			{
				return t.GetInterfaces().Concat(new[] { typeof(object) }).ToArray();
			}
			var baseTypes = new List<Type>();
			for (var current = t.BaseType; current != null; current = current.BaseType)
				baseTypes.Add(current);
			return baseTypes.ToArray();
		}

		/// <summary>
		/// Get type, basic types and interfaces
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static IEnumerable<Type> Hierarchy(this Type t)
		{
			if (t == null)
			{
				yield break;
			}
			for (var current = t; current != null; current = current.BaseType)
				yield return current;

			foreach (var baseIface in t.GetInterfaces())
				yield return baseIface;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mb"></param>
		/// <returns></returns>
		public static IEnumerable<MethodBase> Hierarchy(this MethodBase mb)
		{
			if (mb == null)
			{
				return Enumerable.Empty<MethodBase>();
			}
			if (mb is MethodInfo)
			{
				return Hierarchy((MethodBase)((MethodInfo)mb)).Cast<MethodBase>();
			}
			else if (mb is ConstructorInfo)
			{
				return Hierarchy((MethodBase)((ConstructorInfo)mb)).Cast<MethodBase>();
			}
			else
			{
				throw new ArgumentException(@"mb must be a MethodInfo or ConstructorInfo.", "mb");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="mi"></param>
		/// <returns></returns>
		public static IEnumerable<MethodInfo> Hierarchy(this MethodInfo mi)
		{
			if (mi == null)
			{
				yield break;
			}
			for (var current = mi; current != null; current =
			                                        current != current.GetBaseDefinition() ? current.GetBaseDefinition() : null)
			{
				yield return current;

				foreach (var declaration in current.Declarations())
					yield return declaration;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pi"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyInfo> Hierarchy(this PropertyInfo pi)
		{
			var acc = pi == null ? null :
				          pi.CanRead ? pi.GetGetMethod(true) :
					          pi.CanWrite ? pi.GetSetMethod(true) :
						          ((Func<MethodInfo>)(() => { throw new InvalidOperationException("Property Has No Get and Set Methods"); }))();
			return acc.Hierarchy().Select(m =>
				                              {
					                              var p = m.EnclosingProperty();
					                              if (p == null)
						                              throw new InvalidOperationException("Property is null");
					                              return p;
				                              });
		}

		/*
				public static IEnumerable<ConstructorInfo> Hierarchy(this ConstructorInfo ci)
				{
					// todo. implement with IL analysis
					throw new NotImplementedException();
				}
		*/

		/// <summary>
		/// Get property for 
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static PropertyInfo EnclosingProperty(this MethodBase source)
		{
			if (source == null) return null;
			return (
				       from prop in source.DeclaringType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
				       where prop.GetAccessors(true).Contains(source)
				       select prop
			       ).FirstOrDefault();
		}
	}
}