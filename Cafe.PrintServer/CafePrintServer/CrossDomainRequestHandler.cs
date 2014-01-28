using System.Net.Http;
using System.Threading.Tasks;

namespace CafePrintServer
{
    public class CrossDomainRequestHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken)
                .ContinueWith((task) =>
                                  {
                                      HttpResponseMessage response = task.Result;
                                      response.Headers.Add("Access-Control-Allow-Origin", "*");
                                      return response;
                                  });
        }
    }
}