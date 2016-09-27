using TcpServiceCore.Attributes;
using TcpServiceCore.Protocol;
using System;
using System.Linq;
using System.Reflection;
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
        public readonly Type[] Parameters;

        public MethodOperation(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            ReturnType = MethodInfo.ReturnType;
            Name = methodInfo.Name;
            TypeQualifiedName = $"{methodInfo.DeclaringType.Name}.{methodInfo.Name}";

            IsReturnTypeGeneric = ReturnType.GetTypeInfo().IsGenericType;
            IsVoidTask = MethodInfo.ReturnType == typeof(Task);
            IsAwaitable = IsVoidTask || 
                (IsReturnTypeGeneric && ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
            Parameters = MethodInfo.GetParameters().Select(x => x.ParameterType).ToArray();
            var attr = MethodInfo.GetCustomAttribute<OperationContractAttribute>();
            if (attr != null)
            {
                IsOperation = true;
                IsOneWay = attr.IsOneWay;
            }
        }

        public void ValidateOperationContract()
        {
            if (!IsOperation)
                throw new Exception($"{Name} is not attributed with {nameof(OperationContractAttribute)}");
            if (!IsAwaitable)
                throw new Exception($"{Name} is Operation Contract, it must return Task or Task<T>");
            if (IsOneWay && !IsVoidTask)
                throw new Exception($"{Name} is One Way Operation Contract, it must return Task");
        }

        public async Task<object> Execute(object instance, Request request)
        {
            var paramBytes = request.Parameter;
            
            //TODO fix this when supporting multiple parameters
            var param = Parameters == null || Parameters.Length == 0 ? null :
                new[] {
                    Global.Serializer.Deserialize(Parameters[0], paramBytes)
                };

            object result = null;
            if (IsVoidTask)
            {
                await (dynamic)MethodInfo.Invoke(instance, param);
            }
            else
            {
                result = await (dynamic)MethodInfo.Invoke(instance, param);
            }
            return result;
        }
    }
}
