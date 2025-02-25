﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace BizHawk.Common
{
	// fields are serialized/deserialized in their memory order as reported by Marshal.OffsetOf
	// to do anything useful, passed targets should be [StructLayout.Sequential] or [StructLayout.Explicit]
	public static class BinaryQuickSerializer
	{
		private static MethodInfo FromExpression(Expression e)
			=> e is MethodCallExpression caller
				? caller.Method
				: throw new ArgumentException(message: "Expression must be a method call", paramName: nameof(e));

		private static MethodInfo Method<T>(Expression<Action<T>> f)
		{
			return FromExpression(f.Body);
		}

		private static readonly Dictionary<Type, MethodInfo> Readhandlers = new Dictionary<Type, MethodInfo>();
		private static readonly Dictionary<Type, MethodInfo> Writehandlers = new Dictionary<Type, MethodInfo>();

		private static void AddR<T>(Expression<Action<BinaryReader>> f)
		{
			var method = Method(f);
			if (!typeof(T).IsAssignableFrom(method.ReturnType))
			{
				throw new InvalidOperationException("Type mismatch");
			}

			Readhandlers.Add(typeof(T), method);
		}

		private static void AddW<T>(Expression<Action<BinaryWriter>> f)
		{
			var method = Method(f);
			if (!method.GetParameters()[0].ParameterType.IsAssignableFrom(typeof(T)))
			{
				throw new InvalidOperationException("Type mismatch");
			}

			Writehandlers.Add(typeof(T), Method(f));
		}

		static BinaryQuickSerializer()
		{
			AddR<bool>(r => r.ReadBoolean());
			AddW<bool>(r => r.Write(false));

			AddR<sbyte>(r => r.ReadSByte());
			AddW<sbyte>(r => r.Write((sbyte)0));

			AddR<byte>(r => r.ReadByte());
			AddW<byte>(r => r.Write((byte)0U));

			AddR<short>(r => r.ReadInt16());
			AddW<short>(r => r.Write((short)0));

			AddR<ushort>(r => r.ReadUInt16());
			AddW<ushort>(r => r.Write((ushort)0U));

			AddR<int>(r => r.ReadInt32());
			AddW<int>(r => r.Write(0));

			AddR<uint>(r => r.ReadUInt32());
			AddW<uint>(r => r.Write(0U));

			AddR<long>(r => r.ReadInt64());
			AddW<long>(r => r.Write(0L));

			AddR<ulong>(r => r.ReadUInt64());
			AddW<ulong>(r => r.Write(0UL));
		}

		private delegate void Reader(object target, BinaryReader r);
		private delegate void Writer(object target, BinaryWriter w);

		private class SerializationFactory
		{
			public Type Type;
			public Reader Read;
			public Writer Write;

			public SerializationFactory(Type type, Reader read, Writer write)
			{
				Type = type;
				Read = read;
				Write = write;
			}
		}

		private static SerializationFactory CreateFactory(Type t)
		{
			var fields = DeepEquality.GetAllFields(t)
				////.OrderBy(fi => (int)fi.GetManagedOffset()) // [StructLayout.Sequential] doesn't work with this
				.OrderBy(fi => (int)Marshal.OffsetOf(t, fi.Name))
				.ToList();

			var rmeth = new DynamicMethod($"{t.Name}_r", null, new[] { typeof(object), typeof(BinaryReader) }, true);
			var wmeth = new DynamicMethod($"{t.Name}_w", null, new[] { typeof(object), typeof(BinaryWriter) }, true);

			{
				var il = rmeth.GetILGenerator();
				var target = il.DeclareLocal(t);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Castclass, t);
				il.Emit(OpCodes.Stloc, target);

				foreach (var field in fields)
				{
					il.Emit(OpCodes.Ldloc, target);
					il.Emit(OpCodes.Ldarg_1);
					if (!Readhandlers.TryGetValue(field.FieldType, out var m))
					{
						throw new InvalidOperationException($"(R) Can't handle nested type {field.FieldType}");
					}

					il.Emit(OpCodes.Callvirt, m);
					il.Emit(OpCodes.Stfld, field);
				}

				il.Emit(OpCodes.Ret);
			}

			{
				var il = wmeth.GetILGenerator();
				var target = il.DeclareLocal(t);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Castclass, t);
				il.Emit(OpCodes.Stloc, target);

				foreach (var field in fields)
				{
					il.Emit(OpCodes.Ldarg_1);
					il.Emit(OpCodes.Ldloc, target);
					il.Emit(OpCodes.Ldfld, field);
					if (!Writehandlers.TryGetValue(field.FieldType, out var m))
					{
						throw new InvalidOperationException($"(W) Can't handle nested type {field.FieldType}");
					}

					il.Emit(OpCodes.Callvirt, m);
				}

				il.Emit(OpCodes.Ret);
			}

			return new SerializationFactory(
				t,
				(Reader) rmeth.CreateDelegate(typeof(Reader)),
				(Writer) wmeth.CreateDelegate(typeof(Writer)));
		}

		private static readonly IDictionary<Type, SerializationFactory> Serializers =
			new ConcurrentDictionary<Type, SerializationFactory>();

		private static SerializationFactory GetFactory(Type t)
		{
			if (!Serializers.TryGetValue(t, out var f))
			{
				f = CreateFactory(t);
				Serializers[t] = f;
			}

			return f;
		}

		public static T Create<T>(BinaryReader r)
			where T : new()
		{
			T target = new T();
			Read(target, r);
			return target;
		}

		public static object Create(Type t, BinaryReader r)
		{
			object target = Activator.CreateInstance(t);
			Read(target, r);
			return target;
		}

		public static void Read(object target, BinaryReader r)
		{
			GetFactory(target.GetType()).Read(target, r);
		}

		public static void Write(object target, BinaryWriter w)
		{
			GetFactory(target.GetType()).Write(target, w);
		}
	}
}
