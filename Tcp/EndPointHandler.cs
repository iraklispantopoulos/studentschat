using System;
using System.Threading.Tasks;

namespace Tcp
{
    public class EndPointHandler<Response> where Response : class, new()
    {
        public string EndPoint { get; private set; }
        public Func<string,Task<Response>> Handler { get; private set; }
        public EndPointHandler(string endPoint, Func<string, Task<Response>> handler)
        {
            EndPoint = endPoint;
            Handler = handler;
        }
    }
}
