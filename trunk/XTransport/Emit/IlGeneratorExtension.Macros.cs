using System;
using System.Reflection;
using System.Reflection.Emit;

namespace XTransport.Emit
{
	partial class IlGeneratorExtension
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="condition"></param>
		/// <param name="true"></param>
		/// <returns></returns>
		public static ILGenerator @If(this ILGenerator il, bool condition, Func<ILGenerator, ILGenerator> @true)
		{
			var endBlock = il.DefineLabel();

			// it's possible to calculate length of the "@true(il)" block by visiting delegate body
			// but i'm too lazy to do that :) thus short jumps aren't used
			il.Emit(condition ? OpCodes.Brfalse : OpCodes.Brtrue, endBlock);
			@true(il);
			il.MarkLabel(endBlock);

			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="condition"></param>
		/// <param name="true"></param>
		/// <param name="false"></param>
		/// <returns></returns>
		public static ILGenerator @If(this ILGenerator il, bool condition, Func<ILGenerator, ILGenerator> @true, Func<ILGenerator, ILGenerator> @false)
		{
			var elseMarker = il.DefineLabel();
			var endifMarker = il.DefineLabel();

			il.Emit(condition ? OpCodes.Brfalse : OpCodes.Brtrue, elseMarker);
			@true(il);
			il.Emit(OpCodes.Br, endifMarker);
			il.MarkLabel(elseMarker);
			@false(il);
			il.MarkLabel(endifMarker);

			return il;
		}

		///<summary>
		///</summary>
		///<param name="il"></param>
		///<param name="start"></param>
		///<param name="end"></param>
		///<returns></returns>
		public static ILGenerator LdArgs(this ILGenerator il, int start, int end)
		{
			for (var i = start; i <= end; i++)
				Ldarg(il, i);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="fieldInfo"></param>
		/// <returns></returns>
		public static ILGenerator LdFieldInfo(this ILGenerator il, FieldInfo fieldInfo)
		{
			if (fieldInfo == null) throw new ArgumentNullException("fieldInfo");

			il.Emit(OpCodes.Ldtoken, fieldInfo);
			il.Emit(OpCodes.Ldtoken, fieldInfo.DeclaringType);
			il.EmitCall(OpCodes.Call, typeof(FieldInfo).GetMethod("GetFieldFromHandle", new[] { typeof(RuntimeFieldHandle), typeof(RuntimeTypeHandle) }), null);
			il.Emit(OpCodes.Castclass, typeof(FieldInfo));

			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="methodBase"></param>
		/// <returns></returns>
		public static ILGenerator LdMethodInfo(this ILGenerator il, MethodBase methodBase)
		{
			if (methodBase == null) throw new ArgumentNullException("methodBase");

			il.Emit(OpCodes.Ldtoken, (MethodInfo)methodBase);
			il.Emit(OpCodes.Ldtoken, methodBase.DeclaringType);
			il.EmitCall(OpCodes.Call, typeof(MethodBase).GetMethod("GetMethodFromHandle", new[] { typeof(RuntimeMethodHandle), typeof(RuntimeTypeHandle) }), null);

			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="typeInfo"></param>
		/// <returns></returns>
		public static ILGenerator LdTypeInfo(this ILGenerator il, Type typeInfo)
		{
			if (typeInfo == null) throw new ArgumentNullException("typeInfo");

			il.Emit(OpCodes.Ldtoken, typeInfo);
			il.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) }), null);

			return il;
		}
	}
}