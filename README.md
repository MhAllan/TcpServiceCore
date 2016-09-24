### TcpServiceCore
#### Introduction
* Service Framework for .Net Standard over TCP
* WCF style (No WCF code is used)
* No SOAP
* Enforces Asynchronous IO
* Fast and Lightweight

##### Defining Contract
* Start by defining contract using **ServiceContract** attribute on an Interface, unlike WCF you can't target classes
* Contracts can be inherit if all interfaces has **ServiceContract** attribute
* Define operations using **OperationContract** attribute
* Operations **must** return Task or Task&lt;T&gt;
* One way operation (Where the client doesn't block while the service operation is executing) **must return Task**
* One way operation returns when the proxy writes to the network stream, no delivery is guaranteed.
* Operations' ***out parameters*** are ignored so don't define out parameters

          using System;
          using System.Collections.Generic;
          using System.Linq;
          using System.Threading.Tasks;
          using TcpServiceCore.Attributes;

          [ServiceContract]
          public interface IService
          {

              [OperationContract]
              Task<string> Echo(string msg);

              [OperationContract(IsOneWay = true)]
              Task FireMsg(string msg);
          }

#### Create the service

* Implement your contract and attribute with **ServiceBehavoir** attribute, like WCF you can set the InstanceContextMode
* When the InstanceContextMode is Single one service instance will be created per host
* When the InstanceContextMode is PerCall, a new isntance is created for every method call
* When the InstanceContextMode is PerSession, a new instance is created per Proxy
* Instances are not created when the proxy opens, but when the proxy issues an operation request.
    
        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
        public class Service : IService
        {
          public Task<string> Echo(string msg)
          {
              return Task.FromResult(msg.ToUpper());
          }

          public Task FireMsg(string msg)
          {
              Console.WriteLine($"received {msg}");
              return Task.CompletedTask;
          }
        }

#### Serialization
* For contract parameters and return to be marshalled they need to be compatible with the used serializer, the framework has two built-in serializers **ProtoSerializer** which is the default, and **JsonSerializer**.
* **ProtoSerializer** uses **Protobuf-net** nuget library, so if you use this serializer (the default) your type need to be attributed with **ProtoContract** and its properties need to be attributed with **ProtoMember** attribute
* You can implement your own serializer by implementing **ISerializer** interface then setting Global.Serializer global static property

           Global.Serializer = new TcpServiceCore.Serialization.Json.JsonSerializer();

#### Start the host
* Create instance of ServiceHost&lt;T&gt; where T is your service type
* Add contracts: for every contract you want to expose you need to call AddContract&lt;T&gt; on the host, where T is your contract type
* Initializing Service: As long as services are parameterless you may want to initialize them before invoking their operations, to initialize a service you can ServiceInstanciated event fired by ServiceHost&lt;T&gt;
* Open the host

#### Creating Proxy
* The proxy creation is also WCF style, you create proxy using ChannelFactory&lt;T&gt;.CreateProxy. where T is your contract type
* ChannelFactory&lt;T&gt;.CreateProxy won't open the connection unless you pass 'true' as the last parameter
* The generated proxy implements your contract and **IClientChannel** interface to allow you to (Open, Close, Dispose) the proxy

        public static void Main(string[] args)
        {
            RunExample().Wait();
        }

        static async Task RunExample()
        {
            var config = new ChannelConfig
            {
                ReceiveTimeout = TimeSpan.FromSeconds(10),
                SendTimeout = TimeSpan.FromSeconds(10)
            };

            var host = new ServiceHost<Service>(9091);

            host.AddContract<IService>(config);

            host.ServiceInstanciated += s =>
            {
                //construct created instances
            };

            await host.Open();

            var client = await ChannelFactory<IService>.CreateProxy("localhost", 9091, config, true);

            using ((IClientChannel)client)
            {
                var result = await client.Echo("message");
                Console.WriteLine(result);
            }
            
            Console.ReadLine();
        }
#### Limitation
This framework is a core for more complex framework for distributed systems, the protocol is not yet done, so there are some limitations, for now, the operations can take only one parameter, if you want to pass multiple parameters you need to wrap them in a containing type. 
