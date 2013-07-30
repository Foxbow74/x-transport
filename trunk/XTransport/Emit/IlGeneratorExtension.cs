using System;
using System.Reflection;
using System.Reflection.Emit;

namespace XTransport.Emit
{
	public static partial class IlGeneratorExtension
	{
		// ReSharper disable InconsistentNaming
		// ReSharper disable LocalizableElement

		/// <summary>
		/// Define local variable
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		/// <param name="variableInfo"></param>
		/// <returns></returns>
		public static ILGenerator DefineLocal(this ILGenerator il, Type type, out LocalBuilder variableInfo)
		{
			variableInfo = il.DeclareLocal(type);
			return il;
		}

		/// <summary>
		/// Define Label
		/// </summary>
		/// <param name="il"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static ILGenerator DefineLabel(this ILGenerator il, out Label label)
		{
			label = il.DefineLabel();
			return il;
		}

		/// <summary>
		/// Apply Coder Function
		/// </summary>
		/// <param name="il"></param>
		/// <param name="coder"></param>
		/// <returns></returns>
		public static ILGenerator Apply(this ILGenerator il, Func<ILGenerator, ILGenerator> coder)
		{
			if (coder != null) coder(il);
			return il;
		}

		/// <summary>
		/// Emit Add
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Add(this ILGenerator il)
		{
			il.Emit(OpCodes.Add);
			return il;
		}

		/// <summary>
		/// Emit Add
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator And(this ILGenerator il)
		{
			il.Emit(OpCodes.And);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static ILGenerator Beq(this ILGenerator il, Label label)
		{
			il.Emit(OpCodes.Beq, label);
			return il;
		}

		/// <summary>
		/// Emit Box
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ILGenerator Box(this ILGenerator il, Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			// the line below forbids usage of .box in expressions creation
			// if (!type.IsValueType) throw new ArgumentException("Value type expected", "type");

			il.Emit(OpCodes.Box, type);
			return il;
		}

		/// <summary>
		/// Jump
		/// </summary>
		/// <param name="il"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static ILGenerator Br(this ILGenerator il, Label label)
		{
			il.Emit(OpCodes.Br, label);
			return il;
		}

		/// <summary>
		/// Jump (short)
		/// </summary>
		/// <param name="il"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static ILGenerator Br_S(this ILGenerator il, Label label)
		{
			il.Emit(OpCodes.Br_S, label);
			return il;
		}

		/// <summary>
		/// Jump if False
		/// </summary>
		/// <param name="il"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static ILGenerator Brfalse(this ILGenerator il, Label label)
		{
			il.Emit(OpCodes.Brfalse, label);
			return il;
		}

		/// <summary>
		/// Jump if False (short)
		/// </summary>
		/// <param name="il"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static ILGenerator Brfalse_S(this ILGenerator il, Label label)
		{
			il.Emit(OpCodes.Brfalse_S, label);
			return il;
		}

		/// <summary>
		/// Jump if True
		/// </summary>
		/// <param name="il"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static ILGenerator Brtrue(this ILGenerator il, Label label)
		{
			il.Emit(OpCodes.Brtrue, label);
			return il;
		}

		/// <summary>
		/// Jump if True (short)
		/// </summary>
		/// <param name="il"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static ILGenerator Brtrue_S(this ILGenerator il, Label label)
		{
			il.Emit(OpCodes.Brtrue_S, label);
			return il;
		}

		/// <summary>
		/// Emit Call
		/// </summary>
		/// <param name="il"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		public static ILGenerator Call(this ILGenerator il, MethodBase method)
		{
			if (method is ConstructorInfo)
			{
				il.Emit(OpCodes.Call, (ConstructorInfo)method);
			}
			else
			{
				il.EmitCall(OpCodes.Call, (MethodInfo)method, null);
			}
			return il;
		}

		/// <summary>
		/// Emit Call Virt
		/// </summary>
		/// <param name="il"></param>
		/// <param name="method"></param>
		/// <returns></returns>
		public static ILGenerator Callvirt(this ILGenerator il, MethodBase method)
		{
			// WHL-382 - it isn't convenitent to check out whether a method is virtual, thus we're doing it here
			il.EmitCall(method.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, (MethodInfo)method, null);
			return il;
		}

		/// <summary>
		/// Check If Equals
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Ceq(this ILGenerator il)
		{
			il.Emit(OpCodes.Ceq);
			return il;
		}

		/// <summary>
		/// Check If greater
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Cgt(this ILGenerator il)
		{
			il.Emit(OpCodes.Cgt);
			return il;
		}

		/// <summary>
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Clt(this ILGenerator il)
		{
			il.Emit(OpCodes.Clt);
			return il;
		}

		/// <summary>
		/// Not Equals
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Cne(this ILGenerator il)
		{
			return il.Ceq().Cfalse();
		}

		/// <summary>
		/// Greater or equals
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Cge(this ILGenerator il)
		{
			return il.Clt().Cfalse();
		}

		/// <summary>
		/// Lowest or equals
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Cle(this ILGenerator il)
		{
			return il.Cgt().Cfalse();
		}

		/// <summary>
		/// If False
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Cfalse(this ILGenerator il)
		{
			return il.Ldc_I4(0).Ceq();
		}

		/// <summary>
		/// If True
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Ctrue(this ILGenerator il)
		{
			return il.Ldc_I4(1).Ceq();
		}

		/// <summary>
		/// Compare
		/// </summary>
		/// <param name="il"></param>
		/// <param name="pred"></param>
		/// <returns></returns>
		public static ILGenerator Cmp(this ILGenerator il, PredicateType pred)
		{
			switch (pred)
			{
				case PredicateType.Equal:
					return il.Ceq();
				case PredicateType.GreaterThan:
					return il.Cgt();
				case PredicateType.GreaterThanOrEqual:
					return il.Cge();
				case PredicateType.LessThan:
					return il.Clt();
				case PredicateType.LessThanOrEqual:
					return il.Cle();
				case PredicateType.NotEqual:
					return il.Cne();
				case PredicateType.IsFalse:
					return il.Cfalse();
				case PredicateType.IsTrue:
					return il.Ctrue();
				default:
					throw new ArgumentException(@"Invalid Predicate Type", "pred");
			}
		}

		/// <summary>
		/// Cast to class
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ILGenerator Castclass(this ILGenerator il, Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			il.Emit(OpCodes.Castclass, type);
			return il;
		}

		/// <summary>
		/// Constrains the type on which a virtual method call is made
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ILGenerator Constrained(this ILGenerator il, Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			il.Emit(OpCodes.Constrained, type);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		public static ILGenerator Convert(this ILGenerator il, Type source, Type destination)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (destination == null) throw new ArgumentNullException("destination");

			if (source == destination) return il;

			if (source == typeof(Object) && destination.IsValueType) return il.Unbox_Any(destination);
			if (source.IsValueType && destination == typeof(Object)) return il.Box(destination);

			// if (source.IsAssignableFrom(destination)) return this;
			// --> it doesn't work for int? -> int, cause int is assignable from int?

			var converter = LookUpConverter(source, destination);
			if (converter != null) // not so beauty, but it's enough for internal code
			{
				// todo. implement invariant culture here
				if (converter is ConstructorInfo) return il.Newobj((ConstructorInfo)converter);
				// note the ClassCastException expected below in near future :)
				return converter.IsVirtual ? il.Callvirt((MethodInfo)converter) : il.Call((MethodInfo)converter);
			}

			Func<ILGenerator, ILGenerator> emitter;
			if (CanGenerateConverter(source, destination, out emitter)) return emitter(il);

			return il.Castclass(destination);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Debugger(this ILGenerator il)
		{
			il.Emit(OpCodes.Break);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Dup(this ILGenerator il)
		{
			il.Emit(OpCodes.Dup);
			return il;
		}

		/// <summary>
		/// Emit Equality Compare for 2 arguments in top of stack
		/// </summary>
		/// <param name="il"></param>
		/// <param name="argType"></param>
		/// <returns></returns>
		public static ILGenerator IsEquals(this ILGenerator il, Type argType)
		{
			if (argType.IsPrimitive)
			{
				return il.Ceq();
			}

			var nulType = Nullable.GetUnderlyingType(argType);
			if (nulType != null)
			{
				var miGetValue = argType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).GetGetMethod();
				var miHasValue = argType.GetProperty("HasValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).GetGetMethod();
				var locVar1 = il.DeclareLocal(argType);
				var locVar2 = il.DeclareLocal(argType);
				var lblHasValues = il.DefineLabel();
				var lblExit = il.DefineLabel();

				il
					.Stloc(locVar2)
					.Stloc(locVar1)
					.Ldloca(locVar1)
					.Call(miHasValue)
					.Ldloca(locVar2)
					.Call(miHasValue)
					.Add()
					.Dup()
					.Ldc_I4(2)
					.Beq(lblHasValues)
					.Ldtrue()
					.Xor()
					.Br(lblExit)
					.Label(lblHasValues)
					.Pop()
					.Ldloca(locVar1)
					.Call(miGetValue)
					.Ldloca(locVar2)
					.Call(miGetValue)
					.IsEquals(nulType)
					.Label(lblExit);
				return il;
			}

			var eqMethod = argType.GetMethod("op_Equality",
				BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
				null, new Type[] { argType, argType }, null);
			if (eqMethod != null)
			{
				return il.Call(eqMethod);
			}
			var eqType = typeof(IEquatable<>).MakeGenericType(argType);
			var lblEnd = il.DefineLabel();
			if (!argType.IsValueType)
			{
				if (eqType.IsAssignableFrom(argType))
				{
					var lblHasValue1 = il.DefineLabel();
					var locVar2 = il.DeclareLocal(argType);

					il
						.Stloc(locVar2)
						.Dup()
						.Brtrue(lblHasValue1)
						.Ldloc(locVar2) //                  if arg1 == 0
						.Ceq() //                           Сравниваем refernces
						.Br(lblEnd)
						.Label(lblHasValue1)
						.Ldloc(locVar2)
						.Callvirt(eqType.GetMethod("Equals"));
				}
				else
				{
					il.Ceq();
				}
			}
			else
			{
				if (eqType.IsAssignableFrom(argType))
				{
					var var1 = il.DeclareLocal(argType);
					il.Stloc(var1).Ldloca(var1);
					il.Callvirt(eqType.GetMethod("Equals"));
				}
			}

			il.MarkLabel(lblEnd);
			return il;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="valueType"></param>
		/// <returns></returns>
		public static ILGenerator Initobj(this ILGenerator il, Type valueType)
		{
			il.Emit(OpCodes.Initobj, valueType);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="valueType"></param>
		/// <returns></returns>
		public static ILGenerator Isinst(this ILGenerator il, Type valueType)
		{
			il.Emit(OpCodes.Isinst, valueType);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="label"></param>
		/// <returns></returns>
		public static ILGenerator Label(this ILGenerator il, Label label)
		{
			il.MarkLabel(label);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static ILGenerator Ldarg(this ILGenerator il, int index)
		{
			if (index < 4)
				switch (index)
				{
					case 0:
						il.Emit(OpCodes.Ldarg_0);
						return il;
					case 1:
						il.Emit(OpCodes.Ldarg_1);
						return il;
					case 2:
						il.Emit(OpCodes.Ldarg_2);
						return il;
					case 3:
						il.Emit(OpCodes.Ldarg_3);
						return il;
					default:
						throw new ArgumentOutOfRangeException("index", "Index should not be negative");
				}

			if (index > byte.MaxValue) il.Emit(OpCodes.Ldarg, index);
			else il.Emit(OpCodes.Ldarg_S, (byte)index);

			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="mi"></param>
		/// <returns></returns>
		public static ILGenerator Ldftn(this ILGenerator il, MethodInfo mi)
		{
			il.Emit(OpCodes.Ldftn, mi);
			return il;
		}

		public static ILGenerator Ldvirtftn(this ILGenerator il, MethodInfo mi)
		{
			il.Emit(OpCodes.Ldvirtftn, mi);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Ldtrue(this ILGenerator il)
		{
			return il.Ldc_I4(1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Ldfalse(this ILGenerator il)
		{
			return il.Ldc_I4(0);
		}

		/// <summary>
		/// Load Int Constant
		/// </summary>
		/// <param name="il"></param>
		/// <param name="constant"></param>
		/// <returns></returns>
		public static ILGenerator Ldc_I4(this ILGenerator il, int constant)
		{
			if (constant < 9)
				if (constant > -2)
					switch (constant)
					{
						case 0:
							il.Emit(OpCodes.Ldc_I4_0);
							return il;
						case 1:
							il.Emit(OpCodes.Ldc_I4_1);
							return il;
						case 2:
							il.Emit(OpCodes.Ldc_I4_2);
							return il;
						case 3:
							il.Emit(OpCodes.Ldc_I4_3);
							return il;
						case 4:
							il.Emit(OpCodes.Ldc_I4_4);
							return il;
						case 5:
							il.Emit(OpCodes.Ldc_I4_5);
							return il;
						case 6:
							il.Emit(OpCodes.Ldc_I4_6);
							return il;
						case 7:
							il.Emit(OpCodes.Ldc_I4_7);
							return il;
						case 8:
							il.Emit(OpCodes.Ldc_I4_8);
							return il;
						case -1:
							il.Emit(OpCodes.Ldc_I4_M1);
							return il;
					}
				else
				{
					il.Emit(OpCodes.Ldc_I4, constant);
					return il;
				}

			if (constant > sbyte.MaxValue || constant < sbyte.MinValue) il.Emit(OpCodes.Ldc_I4, constant);
			else il.Emit(OpCodes.Ldc_I4_S, (sbyte)constant);

			return il;
		}

		/// <summary>
		/// Load long constant
		/// </summary>
		/// <param name="il"></param>
		/// <param name="constant"></param>
		/// <returns></returns>
		public static ILGenerator Ldc_I8(this ILGenerator il, long constant)
		{
			il.Emit(OpCodes.Ldc_I8, constant);
			return il;
		}

		/// <summary>
		/// Load Floa Constant
		/// </summary>
		/// <param name="il"></param>
		/// <param name="constant"></param>
		/// <returns></returns>
		public static ILGenerator Ldc_R4(this ILGenerator il, float constant)
		{
			il.Emit(OpCodes.Ldc_R4, constant);
			return il;
		}

		/// <summary>
		/// Load double constant
		/// </summary>
		/// <param name="il"></param>
		/// <param name="constant"></param>
		/// <returns></returns>
		public static ILGenerator Ldc_R8(this ILGenerator il, double constant)
		{
			il.Emit(OpCodes.Ldc_R8, constant);
			return il;
		}

		///<summary>
		/// Emit Load Default Value
		///</summary>
		///<param name="il"></param>
		///<param name="type"></param>
		///<returns></returns>
		///<exception cref="ArgumentOutOfRangeException"></exception>
		public static ILGenerator LdDefault(this ILGenerator il, Type type)
		{
			if (type == typeof(void)) return il; // usualy used in props autogen

			if (type.IsPrimitive)
			{
				if (typeof(bool) == type || typeof(byte) == type || typeof(sbyte) == type || typeof(short) == type ||
					typeof(ushort) == type || typeof(int) == type || typeof(uint) == type || typeof(char) == type)
					return il.Ldc_I4(0);

				if (typeof(float) == type) return il.Ldc_R4(0);
				if (typeof(double) == type) return il.Ldc_R8(0.0);

				if (typeof(long) == type || typeof(ulong) == type) return il.Ldc_I8(0);

				throw new ArgumentOutOfRangeException("type", "Unexpected primitive type: " + type);
			}

			if (type.IsValueType)
			{
				var variable = il.DeclareLocal(type);
				return il.Ldloca(variable).Initobj(type).Ldloc(variable);
			}

			return il.Ldnull();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static ILGenerator Ldelem(this ILGenerator il, Type t)
		{
			il.Emit(OpCodes.Ldelem, t);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Ldelem_Ref(this ILGenerator il)
		{
			il.Emit(OpCodes.Ldelem_Ref);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="field"></param>
		/// <returns></returns>
		public static ILGenerator Ldfld(this ILGenerator il, FieldInfo field)
		{
			il.Emit(field.IsStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, field);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="field"></param>
		/// <returns></returns>
		public static ILGenerator Ldflda(this ILGenerator il, FieldInfo field)
		{
			il.Emit(field.IsStatic ? OpCodes.Ldsflda : OpCodes.Ldflda, field);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static ILGenerator Ldloc(this ILGenerator il, LocalVariableInfo variable)
		{
			if (variable.LocalIndex < 4)
				switch (variable.LocalIndex)
				{
					case (0):
						il.Emit(OpCodes.Ldloc_0);
						return il;
					case (1):
						il.Emit(OpCodes.Ldloc_1);
						return il;
					case (2):
						il.Emit(OpCodes.Ldloc_2);
						return il;
					case (3):
						il.Emit(OpCodes.Ldloc_3);
						return il;
					default:
						throw new ArgumentOutOfRangeException("variable", "Variable index should be positive");
				}

			if (variable.LocalIndex > byte.MaxValue)
				il.Emit(OpCodes.Ldloc, variable.LocalIndex);
			else
				il.Emit(OpCodes.Ldloc_S, (byte)variable.LocalIndex);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static ILGenerator Ldloca(this ILGenerator il, LocalVariableInfo variable)
		{
			if (variable.LocalIndex > byte.MaxValue)
				il.Emit(OpCodes.Ldloca, variable.LocalIndex);
			else
				il.Emit(OpCodes.Ldloca_S, (byte)variable.LocalIndex);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Ldnull(this ILGenerator il)
		{
			il.Emit(OpCodes.Ldnull);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ILGenerator Ldtoken(this ILGenerator il, Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			il.Emit(OpCodes.Ldtoken, type);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="methodInfo"></param>
		/// <returns></returns>
		public static ILGenerator Ldtoken(this ILGenerator il, MethodInfo methodInfo)
		{
			if (methodInfo == null) throw new ArgumentNullException("methodInfo");

			il.Emit(OpCodes.Ldtoken, methodInfo);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="fieldInfo"></param>
		/// <returns></returns>
		public static ILGenerator Ldtoken(this ILGenerator il, FieldInfo fieldInfo)
		{
			if (fieldInfo == null) throw new ArgumentNullException("fieldInfo");

			il.Emit(OpCodes.Ldtoken, fieldInfo);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="constant"></param>
		/// <returns></returns>
		public static ILGenerator Ldstr(this ILGenerator il, String constant)
		{
			if (constant == null) return il.Ldnull();

			il.Emit(OpCodes.Ldstr, constant);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ILGenerator Newarr(this ILGenerator il, Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			il.Emit(OpCodes.Newarr, type);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="ctor"></param>
		/// <returns></returns>
		public static ILGenerator Newobj(this ILGenerator il, ConstructorInfo ctor)
		{
			if (ctor == null) throw new ArgumentNullException("ctor");

			il.Emit(OpCodes.Newobj, ctor);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		/// <param name="ctorParams"></param>
		/// <returns></returns>
		public static ILGenerator Newobj(this ILGenerator il, Type type, params Type[] ctorParams)
		{
			if (type == null) throw new ArgumentNullException("type");

			var ctor = type.GetConstructor(ctorParams);
			if (ctor != null) return il.Newobj(ctor);

			ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, CallingConventions.Standard, ctorParams, null);
			if (ctor != null) return il.Newobj(ctor);

			throw new ArgumentException(String.Format("No such .ctor({1}) for type {0}.", type, ctorParams));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Nop(this ILGenerator il)
		{
			il.Emit(OpCodes.Nop);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Pop(this ILGenerator il)
		{
			il.Emit(OpCodes.Pop);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Ret(this ILGenerator il)
		{
			il.Emit(OpCodes.Ret);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static ILGenerator @Sizeof(this ILGenerator il, Type t)
		{
			il.Emit(OpCodes.Sizeof, t);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="t"></param>
		/// <returns></returns>
		public static ILGenerator Stelem(this ILGenerator il, Type t)
		{
			il.Emit(OpCodes.Stelem, t);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Stelem_Ref(this ILGenerator il)
		{
			il.Emit(OpCodes.Stelem_Ref);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="field"></param>
		/// <returns></returns>
		public static ILGenerator Stfld(this ILGenerator il, FieldInfo field)
		{
			il.Emit(field.IsStatic ? OpCodes.Stsfld : OpCodes.Stfld, field);
			return il;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="variable"></param>
		/// <returns></returns>
		public static ILGenerator Stloc(this ILGenerator il, LocalBuilder variable)
		{
			if (variable == null) return il; // do nothing for void

			if (variable.LocalIndex < 4)
			{
				switch (variable.LocalIndex)
				{
					case 0:
						il.Emit(OpCodes.Stloc_0);
						return il;
					case 1:
						il.Emit(OpCodes.Stloc_1);
						return il;
					case 2:
						il.Emit(OpCodes.Stloc_2);
						return il;
					case 3:
						il.Emit(OpCodes.Stloc_3);
						return il;
				}
			}

			il.Emit(variable.LocalIndex > byte.MaxValue ? OpCodes.Stloc : OpCodes.Stloc_S, variable);

			return il;
		}

		/// <summary>
		/// Switch
		/// </summary>
		/// <param name="il"></param>
		/// <param name="labels"></param>
		/// <returns></returns>
		public static ILGenerator Switch(this ILGenerator il, Label[] labels)
		{
			il.Emit(OpCodes.Switch, labels);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator @Throw(this ILGenerator il)
		{
			il.Emit(OpCodes.Throw);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="exceptionType"></param>
		/// <param name="ctorParamTypes"></param>
		/// <returns></returns>
		public static ILGenerator @Throw(this ILGenerator il, Type exceptionType, params Type[] ctorParamTypes)
		{
			if (exceptionType == null) throw new ArgumentNullException("exceptionType");
			if (!typeof(Exception).IsAssignableFrom(exceptionType)) throw new ArgumentException("Exception type expected.", "exceptionType");

			var constructor = exceptionType.GetConstructor(ctorParamTypes);
			if (constructor == null) throw new ArgumentNullException("exceptionType", "No ctor found");

			return il.Newobj(constructor).Throw();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ILGenerator Unbox(this ILGenerator il, Type type)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (!type.IsValueType) throw new ArgumentException("Value type expected", "type");

			il.Emit(OpCodes.Unbox, type);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ILGenerator Unbox_Any(this ILGenerator il, Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			il.Emit(OpCodes.Unbox_Any, type);
			return il;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="il"></param>
		/// <returns></returns>
		public static ILGenerator Xor(this ILGenerator il)
		{
			il.Emit(OpCodes.Xor);
			return il;
		}
	}
}
