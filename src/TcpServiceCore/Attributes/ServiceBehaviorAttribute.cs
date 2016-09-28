using TcpServiceCore.Dispatching;
using System;

namespace TcpServiceCore.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ServiceBehaviorAttribute : Attribute
    {
        public InstanceContextMode InstanceContextMode { get; set; }
    }
}
