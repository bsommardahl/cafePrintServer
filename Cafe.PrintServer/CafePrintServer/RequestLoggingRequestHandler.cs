using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CafePrintServer
{
    public class RequestLoggingRequestHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken)
                .ContinueWith((task) =>
                                  {
                                      HttpResponseMessage response = task.Result;
                                      Console.WriteLine(request.RequestUri.ToString());
                                      return response;
                                  });
        }
    }
}