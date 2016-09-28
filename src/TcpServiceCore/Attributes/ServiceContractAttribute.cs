using System;

namespace TcpServiceCore.Attributes
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class ServiceContractAttribute : Attribute
    {
    }
}
