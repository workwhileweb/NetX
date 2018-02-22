using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace Leaf.Net
{
    public class ProxyHandler : HttpClientHandler
    {
        private readonly ProxyClient _proxyClient;

        public ProxyHandler(ProxyClient proxyClient)
        {
            _proxyClient = proxyClient;
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponse httpResponse;

            var httpRequest   = request.ToHttpRequest(CookieContainer);
            httpRequest.Proxy = _proxyClient;

            cancellationToken.ThrowIfCancellationRequested();

            if (request.Method == System.Net.Http.HttpMethod.Get)
            {
                httpResponse = await GetAsync(httpRequest, request);
            }
            else if(request.Method == System.Net.Http.HttpMethod.Post)
            {
                httpResponse = await PostAsync(httpRequest, request);
            }
            else
            {
                httpResponse = await RawAsync(httpRequest, request);
            }

            cancellationToken.ThrowIfCancellationRequested();

            HttpExtensions.FillCookieContainer(CookieContainer, httpResponse);

            var response = httpResponse.ToHttpResponseMessage();
            response.RequestMessage = request;

            var stream = httpResponse.ToMemoryStream();
            if (stream != null)
                response.Content = new ProgressStreamContent(stream, CancellationToken.None);

            return response;
        }

        private static async Task<HttpResponse> GetAsync(HttpRequest request, HttpRequestMessage message)
        {
            return await request.GetAsync(message.RequestUri.ToString());
        }

        private static async Task<HttpResponse> PostAsync(HttpRequest request, HttpRequestMessage message)
        {
            var content = await message.Content.ReadAsByteArrayAsync();
            return await request.PostAsync(message.RequestUri.ToString(), content);
        }

        private static async Task<HttpResponse> RawAsync(HttpRequest request, HttpRequestMessage message)
        {
            var method = ConvertMethod(message.Method);
            return await request.RawAsync(method, message.RequestUri.ToString());
        }

        private static HttpMethod ConvertMethod(System.Net.Http.HttpMethod netHttpMethod)
        {
            if (netHttpMethod == System.Net.Http.HttpMethod.Head)
                return HttpMethod.HEAD;

            if (netHttpMethod == System.Net.Http.HttpMethod.Delete)
                return HttpMethod.DELETE;
            
            if (netHttpMethod == System.Net.Http.HttpMethod.Put)
                return HttpMethod.PUT;

            throw new HttpException($"Method {netHttpMethod} not supported");
        }
    }
}
