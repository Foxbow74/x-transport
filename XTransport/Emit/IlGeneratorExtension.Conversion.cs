using System;
using System.Reflection;
using System.Reflection.Emit;

namespace XTransport.Emit
{
	partial class IlGeneratorExtension
	{
		private static bool CanGenerateConverter(Type source, Type destination, out Func<ILGenerator, ILGenerator> emitter)
		{
			emitter = LookUpForString2Nullable(source, destination) ??
			          LookUpForNullable2String(source, destination) ??
			          LookUpForEnum2String(source, destination) ??
			          LookUpForString2Enum(source, destination) ??
			          LookUpForStruct2String(source, destination) ??
			          LookUpForClass2String(source, destination)
				;
			return emitter != null;
		}

		private static MethodBase LookUpConverter(Type source, Type destination)
		{
			return
				LookUpForOperator(source, destination) ??
				LookUpForConvertMethod(source, destination) ??
				LookUpForWrapper(source, destination)
				;
		}

		private static MethodInfo LookUpForOperator(Type source, Type destination)
		{
			MethodInfo result;

			const MethodAttributes ps = MethodAttributes.Public | MethodAttributes.Static;
			if (destination.HasMethod(out result, ps, destination, "op_Explicit", source)) return result;
			if (destination.HasMethod(out result, ps, destination, "op_Implicit", source)) return result;
			if (source.HasMethod(out result, ps, destination, "op_Explicit", source)) return result;
			if (source.HasMethod(out result, ps, destination, "op_Implicit", source)) return result;

			return result;
		}

		private static MethodBase LookUpForWrapper(Type source, Type destination)
		{
			if (destination.IsInterface || destination.IsAbstract) return null;

			// assume wrapper
			var constructor = destination.GetConstructor(new[] { source });
			if (constructor != null) return constructor;

			// assume parameterized wrapper
			if (destination.IsGenericType && destination.GetGenericArguments().Length == 1)
			{
				try
				{
					var def = destination.XGetGenericDefinition();
					var type = def.XMakeGenericType(source);
					constructor = type.GetConstructor(new[] { source });
				}
				catch (ArgumentException)
				{
					// will be thrown if def.XMakeGenericType(source)
					// violates generic constraints for the type def
				}
			}

			return constructor;
		}

		private static MethodBase LookUpForConvertMethod(Type source, Type destination)
		{
			MethodInfo converter = null;
			// Convert.Toxxx(yyy);
			if (
				(source == typeof(String) && destination.IsPrimitive) ||
				(source.IsPrimitive && destination == typeof(String)) ||
				(source == typeof(String) && destination == typeof(DateTime)) ||
				(source == typeof(DateTime) && destination == typeof(String)) ||
				(source.IsPrimitive && destination == typeof(DateTime)) ||
				(source == typeof(DateTime) && destination.IsPrimitive) ||
				(source.IsPrimitive && destination.IsPrimitive)
				)
			{
				converter = typeof(Convert).GetMethod("To" + destination.Name, new[] { source });
				if (converter == null) throw new ArgumentException(String.Format("There is no converter from {0} to {1}", source.Name, destination.Name), "destination");
			}
			return converter;
		}

		private static Func<ILGenerator, ILGenerator> LookUpForNullable2String(Type source, Type destination)
		{
			if (!(source.IsGenericType && source.GetGenericTypeDefinition() == typeof(Nullable<>))) return null;
			if (typeof(String) != destination) return null;

			LocalBuilder primitive;
			Label @else;
			Label endIf;

			// nullable<?> is on top of the stack
			return il => il
				             .DefineLocal(source, out primitive)
				             .DefineLabel(out @else)
				             .DefineLabel(out endIf)
				             .Stloc(primitive)
				             .Ldloca(primitive)
				             .Call(source.GetProperty("HasValue").GetGetMethod())
				             .Brfalse_S(@else) // @if true
				             .Ldloca(primitive)
				             .Constrained(source)
				             .Callvirt(typeof(Object).GetMethod("ToString"))
				             .Br_S(endIf)
				             .Label(@else) // @else
				             .Ldnull()
				             .Label(endIf) // @end
				;
		}

		private static Func<ILGenerator, ILGenerator> LookUpForString2Nullable(Type source, Type destination)
		{
			if (typeof(String) != source) return null;
			if (!(destination.IsGenericType && destination.GetGenericTypeDefinition() == typeof(Nullable<>))) return null;

			var valueType = destination.XGetGenericArguments()[0];
			//if (!valueType.IsPrimitive) return null; // maybe replace this with TryParse 4 all structs?

			MethodInfo tryParse;
			if (valueType.HasMethod(out tryParse, MethodAttributes.Static | MethodAttributes.Public,
			                        typeof(bool), "TryParse", typeof(String), valueType.MakeByRefType()))
			{
				LocalBuilder result;
				LocalBuilder value;
				Label @else;
				Label endIf;

				// String is on top of the stack
				return il => il
					             .DefineLocal(destination, out result)
					             .DefineLocal(valueType, out value)
					             .DefineLabel(out @else)
					             .DefineLabel(out endIf)
					             .Ldloca(value)
					             .Call(tryParse)
					             .Brfalse_S(@else) // @if true
					             .Ldloc(value)
					             .Newobj(destination, valueType)
					             .Stloc(result)
					             .Br_S(endIf)
					             .Label(@else) // @else
					             .Ldloca(result)
					             .Initobj(destination)
					             .Label(endIf) // @end
					             .Ldloc(result)
					;
			}

			// finally try .ctor(String)
			var ctor = valueType.GetConstructor(new[] { typeof(String) });
			if (ctor != null)
			{
				LocalBuilder result;
				Label @else;
				Label endIf;
				// String is on top of the stack
				return il => il
					             .DefineLocal(destination, out result)
					             .DefineLabel(out @else)
					             .DefineLabel(out endIf)
					             .Dup()
					             // BR: empty strings considered bad initializer argument
					             .Call(typeof(String).GetMethod("IsNullOrEmpty"))
					             .Brtrue_S(@else) // @if !IsNullOrEmpty
					             .Newobj(ctor)
					             .Newobj(destination, valueType)
					             .Stloc(result)
					             .Br_S(endIf)
					             .Label(@else) // @else
					             .Pop()
					             .Ldloca(result)
					             .Initobj(destination)
					             .Label(endIf) // @end
					             .Ldloc(result)
					;
			}

			return null;
		}

		private static Func<ILGenerator, ILGenerator> LookUpForEnum2String(Type source, Type destination)
		{
			if (!source.IsEnum) return null;
			if (typeof(String) != destination) return null;

			// unboxed enum is on top of the stack
			return il => il
				             .Box(source)
				             .Ldstr("d")
				             .Call(FromLambda.Method<Enum, String, String>((e, f) => e.ToString(f)))
				;
		}

		private static Func<ILGenerator, ILGenerator> LookUpForString2Enum(Type source, Type destination)
		{
			if (typeof(String) != source) return null;
			if (!destination.IsEnum) return null;

			LocalBuilder value;

			// String is on top of the stack
			return il => il
				             .DefineLocal(source, out value)
				             .Stloc(value)
				             .LdTypeInfo(destination)
				             .Ldloc(value)
				             .Convert(source, Enum.GetUnderlyingType(destination))
				             .Call(typeof(Enum).GetMethod("ToObject", new[] { typeof(Type), Enum.GetUnderlyingType(destination) }))
				             .Unbox_Any(destination)
				;
		}

		private static Func<ILGenerator, ILGenerator> LookUpForStruct2String(Type source, Type destination)
		{
			if (!source.IsValueType) return null;
			if (typeof(String) != destination) return null;

			LocalBuilder value;

			// struct is on top of the stack
			return il => il
				             .DefineLocal(source, out value)
				             .Stloc(value)
				             .Ldloca(value)
				             .Constrained(source)
				             .Callvirt(FromLambda.Method<Object, String>(o => o.ToString()))
				;
		}

		private static Func<ILGenerator, ILGenerator> LookUpForClass2String(Type source, Type destination)
		{
			if (source.IsValueType) return null;
			if (typeof(String) != destination) return null;

			// instance or null is on top of the stack
			return il => il
				             .Dup()
				             .If(true, x => x
					                            .Callvirt(FromLambda.Method<Object, String>(o => o.ToString()))
				             );
		}

	}
}