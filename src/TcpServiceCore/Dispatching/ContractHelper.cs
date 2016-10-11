using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using TcpServiceCore.Attributes;

namespace TcpServiceCore.Dispatching
{
    static class ContractHelper
    {
        public static bool IsContract(TypeInfo contractType)
        {
            return contractType.IsInterface && contractType.GetCustomAttributes(false)
                                    .OfType<ServiceContractAttribute>()
                                    .FirstOrDefault() != null;
        }

        public static IEnumerable<OperationDescription> ValidateContract(TypeInfo contractType)
        {
            if(!IsContract(contractType))
                throw new Exception($"{contractType} must be Interface and attributed with {nameof(ServiceContractAttribute)}");

            var operations = contractType.GetMethods()
                                .Where(x => x.GetCustomAttribute<OperationContractAttribute>() != null)
                                .Select(x => new OperationDescription(x));
            
            foreach (var op in operations)
            {
                op.ValidateOperationContract();
            }

            return operations;
        }

        public static IEnumerable<OperationDescription> ValidateContract(TypeInfo serviceType, TypeInfo contractType)
        {
            if (contractType.IsAssignableFrom(serviceType) == false)
                throw new Exception($"Type {serviceType} does not implement interface {contractType}");

            return ValidateContract(contractType);
        }
    }
}
