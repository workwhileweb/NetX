using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;

namespace Leaf.Net
{
    internal static class HttpExtensions
    {
        private static readonly Dictionary<string, string> HeaderSeparators = new Dictionary<string, string> {
            {"User-Agent", " "}
        };

        public static HttpResponseMessage ToHttpResponseMessage(this HttpResponse httpResponse)
        {
            var code = (System.Net.HttpStatusCode)((int)httpResponse.StatusCode);

            var response = new HttpResponseMessage(code); 
            var headers  = httpResponse.EnumerateHeaders();

            while (headers.MoveNext())
            {
                var pair = headers.Current;
                response.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
            }

            return response;
        }

        public static HttpRequest ToHttpRequest(this HttpRequestMessage request, CookieContainer cookieContainer)
        {
            var httpRequest = new HttpRequest();

            var headers = request.Headers.Union(request.Content != null
                ? request.Content.Headers 
                : Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>());

            httpRequest.Cookies = new CookieStorage(false, cookieContainer);

            foreach (var keyValue in headers)
                httpRequest.AddHeader(keyValue.Key, string.Join(GetHeaderSeparator(keyValue.Key), keyValue.Value));

            return httpRequest;
        }

        public static void FillCookieContainer(CookieContainer cookieContainer, HttpResponse httpResponse)
        {
            //simplified, all cookies are set to the root path
            var rootUri = new Uri($"{httpResponse.Address.Scheme}://{httpResponse.Address.Authority}");
            var cookies = httpResponse.Cookies.GetCookies(rootUri);

            foreach (Cookie cookie in cookies)
                cookieContainer.Add(rootUri, cookie);
        }

        private static string GetHeaderSeparator(string name)
        {
            return HeaderSeparators.ContainsKey(name) ? HeaderSeparators[name] : ",";
        }
    }
}
