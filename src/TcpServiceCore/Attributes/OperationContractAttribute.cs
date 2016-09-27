using System;

namespace TcpServiceCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class OperationContractAttribute : Attribute
    {
        public bool IsOneWay { get; set; }
    }
}
