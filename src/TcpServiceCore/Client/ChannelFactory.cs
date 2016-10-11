using TcpServiceCore.Attributes;
using TcpServiceCore.Communication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TcpServiceCore.Dispatching;

namespace TcpServiceCore.Client
{
    public static class ChannelFactory<T>
    {
        static Type ImplementingType;
        static Type ProxyType;

        static ChannelFactory()
        {
            ImplementingType = typeof(InnerProxy<T>);
        }

        public static async Task<T> CreateProxy(string server, int port, ChannelConfig config, bool open = true)
        {
            if (ProxyType == null)
                ProxyType = CreateProxyType();

            var channel = new InnerProxy<T>(server, port, config);
            var proxy = Activator.CreateInstance(ProxyType, channel);
            if (open)
                await ((IClientChannel)proxy).Open();
            return (T)proxy;
        }

        static Type CreateProxyType()
        {
            var _interfaceType = typeof(T);

            if (!_interfaceType.GetTypeInfo().IsInterface)
            {
                throw new InvalidOperationException($"{_interfaceType.FullName} is not an interface");
            }

            var an = new AssemblyName("TcpServiceCore_" + _interfaceType.Name);
            var asm = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);

            var moduleName = Path.ChangeExtension(an.Name, "dll");
            var module = asm.DefineDynamicModule(moduleName);

            var ns = _interfaceType.Namespace;
            if (!string.IsNullOrEmpty(ns))
                ns += ".";
            var builder = module.DefineType(ns + _interfaceType.Name + "_TcpServiceCoreProxy",
                TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit
                | TypeAttributes.AutoClass | TypeAttributes.NotPublic | TypeAttributes.Sealed);

            var channel = builder.DefineField("channel", ImplementingType, FieldAttributes.Private);

            var ctor = builder.DefineConstructor(MethodAttributes.Public,
                CallingConventions.HasThis,
                new Type[] { channel.FieldType });

            var il = ctor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, channel);
            il.Emit(OpCodes.Ret);

            ImplementInterface(typeof(IClientChannel), builder, channel);
            ImplementInterface(_interfaceType, builder, channel);
            
            return builder.CreateTypeInfo().AsType();
        }

        static void ImplementInterface(Type interfaceType, TypeBuilder builder, FieldBuilder channel)
        {
            if (builder.FindInterfaces((t, o) => t == interfaceType, null).Length == 0)
            {
                builder.AddInterfaceImplementation(interfaceType);

                var parentInterfaces = interfaceType.GetInterfaces();
                foreach (var _interface in parentInterfaces)
                {
                    ImplementInterface(_interface, builder, channel);
                }

                IEnumerable<MethodOperation> operations = null;
                var intInfo = interfaceType.GetTypeInfo();

                if (ContractHelper.IsContract(intInfo))
                {
                    operations = ContractHelper.ValidateContract(intInfo);
                }
                else
                {
                    operations = interfaceType.GetMethods().Select(x => new MethodOperation(x));
                }

                foreach (var operation in operations)
                {
                    var mb = builder.DefineMethod(
                        $"{interfaceType.Name}.{operation.Name}",
                        MethodAttributes.Public | MethodAttributes.Virtual,
                        operation.ReturnType,
                        operation.ParameterTypes);

                    var il = mb.GetILGenerator();

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, channel);

                    MethodInfo invoke = null;
                    //implement the user interface
                    if (operation.IsOperation)
                    {
                        if (operation.IsOneWay)
                        {
                            invoke = ImplementingType.GetMethod("SendOneWay");
                        }
                        else if (operation.IsVoidTask)
                        {
                            invoke = ImplementingType.GetMethod("SendVoid");
                        }
                        else
                        {
                            invoke = ImplementingType.GetMethod("SendReturn")
                                    .MakeGenericMethod(operation.ReturnType.GetGenericArguments()
                                    .First());
                        }

                        il.Emit(OpCodes.Ldstr, mb.Name);
                        il.Emit(OpCodes.Ldc_I4_S, operation.ParameterTypes.Length);
                        il.Emit(OpCodes.Newarr, typeof(object));
                        for (byte x = 0; x < operation.ParameterTypes.Length; x++)
                        {
                            var xType = operation.ParameterTypes[x];
                            il.Emit(OpCodes.Dup);
                            il.Emit(OpCodes.Ldc_I4_S, x);
                            switch (x)
                            {
                                case 0: il.Emit(OpCodes.Ldarg_1); break;
                                case 1: il.Emit(OpCodes.Ldarg_2); break;
                                case 2: il.Emit(OpCodes.Ldarg_3); break;
                                default: il.Emit(OpCodes.Ldarg_S, x + 1); break;
                            }
                            if (xType.GetTypeInfo().IsValueType)
                                il.Emit(OpCodes.Box, xType);
                            il.Emit(OpCodes.Stelem_Ref);
                        }
                    }
                    else
                    {
                        invoke = ImplementingType.GetMethod(operation.Name, operation.ParameterTypes);
                        for (byte x = 0; x < operation.ParameterTypes.Length; x++)
                        {
                            switch (x)
                            {
                                case 0: il.Emit(OpCodes.Ldarg_1); break;
                                case 1: il.Emit(OpCodes.Ldarg_2); break;
                                case 2: il.Emit(OpCodes.Ldarg_3); break;
                                default: il.Emit(OpCodes.Ldarg_S, x + 1); break;
                            }
                        }
                    }
                    il.Emit(OpCodes.Call, invoke);
                    il.Emit(OpCodes.Ret);

                    builder.DefineMethodOverride(mb, operation.MethodInfo);
                }
            }
        }
    }
}
