using System;

namespace TcpServiceCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class OperationContractAttribute : Attribute
    {
        public bool IsOneWay { get; set; }
    }
}
