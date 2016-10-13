﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using TcpServiceCore.Attributes;
using TcpServiceCore.Communication;
using TcpServiceCore.Buffering;

namespace TcpServiceCore.Dispatching
{
    public class ContractDescription<T> : ContractDescription
    {
        static readonly object _lock = new object();
        public static ContractDescription<T> Instance;

        public static ContractDescription<T> Create()
        {
            if (Instance == null)
            {
                lock (_lock)
                {
                    if (Instance == null)
                        Instance = new ContractDescription<T>();
                }
            }
            return Instance;
        }

        protected ContractDescription()
            : base(typeof(T))
        {

        }
    }

    public class ContractDescription
    {
        public readonly string ContractName;
        public readonly Type ContractType;
        public readonly Type CallbackType;
        public readonly bool HasCallback;
        internal readonly IEnumerable<OperationDescription> Operations;

        readonly Type type;
        readonly TypeInfo typeInfo;
        protected ContractDescription(Type type)
        {
            this.type = type;
            this.typeInfo = type.GetTypeInfo();
            Type callback;
            this.Operations = ValidateContract(this.typeInfo, out callback);
            this.ContractType = this.type;
            this.ContractName = this.type.FullName;
            this.CallbackType = callback;
            this.HasCallback = this.CallbackType != null;
        }

        public void ValidateImplementation(TypeInfo serviceType)
        {
            ValidateContract(serviceType, this.typeInfo);
        }

        internal IBufferManager CreateBufferManager(ChannelConfig config)
        {
            return Global.BufferManagerFactory
                    .CreateBufferManager(this.ContractName, config.MaxBufferSize, config.MaxBufferPoolSize);
        }

        internal static bool IsContract(TypeInfo contractType)
        {
            return contractType.IsInterface && contractType.GetCustomAttributes(false)
                                    .OfType<ServiceContractAttribute>()
                                    .FirstOrDefault() != null;
        }

        internal static IEnumerable<OperationDescription> ValidateContract(TypeInfo contractType)
        {
            Type callback;
            return ValidateContract(contractType, out callback);
        }

        internal static IEnumerable<OperationDescription> ValidateContract(TypeInfo contractType, out Type callbackType)
        {
            //validate contract
            if(!IsContract(contractType))
                throw new Exception($"{contractType} must be Interface and attributed with {nameof(ServiceContractAttribute)}");

            //validate callback
            var contractAttr = contractType.GetCustomAttribute<ServiceContractAttribute>();
            callbackType = contractAttr.CallbackContract;
            if (callbackType != null)
            {
                ValidateContract(callbackType.GetTypeInfo());
            }

            //validate operations
            var operations = contractType.GetMethods()
                                .Where(x => x.GetCustomAttribute<OperationContractAttribute>() != null)
                                .Select(x => new OperationDescription(x));
            
            foreach (var op in operations)
            {
                op.ValidateOperationContract();
            }

            return operations;
        }

        internal static IEnumerable<OperationDescription> ValidateContract(TypeInfo serviceType, TypeInfo contractType)
        {
            if (contractType.IsAssignableFrom(serviceType) == false)
                throw new Exception($"Type {serviceType} does not implement interface {contractType}");

            return ValidateContract(contractType);
        }
    }
}
