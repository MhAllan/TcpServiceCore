using TcpServiceCore.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TcpServiceCore.Dispatching
{
    class TypeDispatcher<T> where T: new()
    {
        static readonly object _lock = new object();
        static TypeDispatcher<T> instance;
        
        public static TypeDispatcher<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                            instance = new TypeDispatcher<T>();
                    }
                }
                return instance;
            }
        }

        public readonly InstanceContextMode InstanceContextMode;
        readonly List<MethodOperation> OperationDispatchers = new List<MethodOperation>();
        private TypeDispatcher()
        {
            var type = typeof(T);
            InstanceContextMode = InstanceContextMode.PerCall;

            var serviceBehavior = type.GetTypeInfo().GetCustomAttribute<ServiceBehaviorAttribute>(false);
            if (serviceBehavior != null)
            {
                InstanceContextMode = serviceBehavior.InstanceContextMode;
            }

            GetOperations(type);
            
            if (OperationDispatchers.Count == 0)
                throw new Exception("No OperationContract found");
        }

        void GetOperations(Type type)
        {
            var interfaces = type.GetInterfaces();
            foreach (var intfc in interfaces)
            {
                var intInfo = intfc.GetTypeInfo();
                if (ContractHelper.IsContract(intInfo))
                {
                    var operations = ContractHelper.ValidateContract(intInfo);
                    if (operations != null)
                    {
                        OperationDispatchers.AddRange(operations);
                    }
                    GetOperations(intfc);
                }
            }
        }

        public T CreateInstance()
        {
            return new T();
        }

        public MethodOperation GetOperation(string name)
        {
            return OperationDispatchers.FirstOrDefault(x => x.TypeQualifiedName == name);
        }
    }
}
