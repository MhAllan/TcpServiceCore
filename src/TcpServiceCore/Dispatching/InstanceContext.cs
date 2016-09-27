using TcpServiceCore.Protocol;
using System.Threading.Tasks;

namespace TcpServiceCore.Dispatching
{
    class InstanceContext<T> where T: new()
    {
        public static volatile InstanceContext<T> Current;

        public readonly T Service;
        readonly TypeDispatcher<T> Dispatcher;

        public InstanceContext(TypeDispatcher<T> dispatcher)
        {
            Dispatcher = dispatcher;
            Service = Dispatcher.CreateInstance();
        }

        public async Task HandleRequest(Request request, IRequestHandler requestHandler)
        {           
            var operation = Dispatcher.GetOperation(request.Operation);

            var result = await operation.Execute(Service, request);

            if (operation.IsOneWay == false)
            {
              var resp = operation.IsVoidTask ? new Response(request.Id, false, (byte)1) : new Response(request.Id, false, result);
              await requestHandler.WriteResponse(resp);
            }
        }
    }
}
