using TcpServiceCore.Attributes;
using TcpServiceCore.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TcpServiceCore.Dispatching
{
    class MethodOperation
    {
        public readonly string Name;
        public readonly string TypeQualifiedName;
        public readonly bool IsReturnTypeGeneric;
        public readonly bool IsVoidTask;
        public readonly bool IsOperation;
        public readonly bool IsOneWay;
        public readonly bool IsAwaitable;

        public readonly MethodInfo MethodInfo;
        public readonly Type ReturnType;
        public readonly Type[] ParameterTypes;

        static readonly Type ByteArrayType;

        static MethodOperation()
        {
            ByteArrayType = typeof(byte[]);
        }

        public MethodOperation(MethodInfo methodInfo)
        {
            this.MethodInfo = methodInfo;
            this.ReturnType = this.MethodInfo.ReturnType;
            this.Name = methodInfo.Name;
            this.TypeQualifiedName = $"{methodInfo.DeclaringType.Name}.{methodInfo.Name}";

            this.IsReturnTypeGeneric = this.ReturnType.GetTypeInfo().IsGenericType;
            this.IsVoidTask = this.MethodInfo.ReturnType == typeof(Task);
            this.IsAwaitable = IsVoidTask || 
                (this.IsReturnTypeGeneric && this.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
            this.ParameterTypes = this.MethodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
            var attr = this.MethodInfo.GetCustomAttribute<OperationContractAttribute>();
            if (attr != null)
            {
                this.IsOperation = true;
                this.IsOneWay = attr.IsOneWay;
            }
        }

        public void ValidateOperationContract()
        {
            if (!this.IsOperation)
                throw new Exception($"{this.Name} is not attributed with {nameof(OperationContractAttribute)}");
            if (!this.IsAwaitable)
                throw new Exception($"{this.Name} is Operation Contract, it must return Task or Task<T>");
            if (this.IsOneWay && !this.IsVoidTask)
                throw new Exception($"{this.Name} is One Way Operation Contract, it must return Task");
        }

        public async Task<object> Execute(object instance, Message request)
        {
            object[] parameters = null;
            if (ParameterTypes != null)
            {
                var length = ParameterTypes.Length;
                parameters = new object[length];
                for (int i = 0; i < length; i++)
                {
                    var pt = ParameterTypes[i];
                    if (pt == ByteArrayType)
                        parameters[i] = request.Parameters[i];
                    else
                        parameters[i] = Global.Serializer.Deserialize(pt, request.Parameters[i]);
                }
            }
            object result = null;
            if (this.IsVoidTask)
            {
                await (dynamic)this.MethodInfo.Invoke(instance, parameters);
            }
            else
            {
                result = await (dynamic)this.MethodInfo.Invoke(instance, parameters);
            }
            return result;
        }
    }
}
