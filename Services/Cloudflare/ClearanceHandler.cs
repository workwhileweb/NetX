using System;
using System.Threading;

namespace Leaf.Net.Services.Cloudflare
{
    /// <summary>
    /// A HTTP handler that transparently manages Cloudflare's Anti-DDoS measure.
    /// </summary>
    /// <remarks>
    /// Only the JavaScript challenge can be handled. CAPTCHA and IP address blocking cannot be bypassed.
    /// </remarks>
    public class ClearanceHandler
    {
        /// <summary>
        /// The default number of retries, if clearance fails.
        /// </summary>
        public static readonly int DefaultMaxRetries = 3;


        private readonly HttpRequest _request;


        /// <summary>
        /// Creates a new instance of the <see cref="ClearanceHandler"/> class with a specific inner handler.
        /// </summary>
        /// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
        public ClearanceHandler(HttpRequest request)
        {
            _request = request;
        }

        /// <summary>
        /// Gets or sets the number of clearance retries, if clearance fails.
        /// </summary>
        /// <remarks>A negative value causes an infinite amount of retries.</remarks>
        public int MaxRetries { get; set; } = DefaultMaxRetries;
        

        private HttpResponse Get(Uri uri)
        {
            // Save original options
            bool originalAllowAutoRedirect = _request.AllowAutoRedirect;
            bool originalIgnoreProtocolErrors = _request.IgnoreProtocolErrors;
            
            // Override options
            _request.AllowAutoRedirect = false;
            _request.IgnoreProtocolErrors = true;

            var response = _request.Get(uri);

            // Return original options
            _request.AllowAutoRedirect = originalAllowAutoRedirect;
            _request.IgnoreProtocolErrors = originalIgnoreProtocolErrors;

            return response;
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public HttpResponse Send(Uri uri, CancellationToken cancellationToken = default(CancellationToken)) // CancellationToken cancellationToken
        {
            // User-Agent is required
            if (string.IsNullOrEmpty(_request.UserAgent))
                _request.UserAgent = Http.ChromeUserAgent();

            var response = Get(uri);

            // (Re)try clearance if required.
            var retries = 0;
            while (IsClearanceRequired(response) && (MaxRetries < 0 || retries <= MaxRetries))
            {
                if (cancellationToken != default(CancellationToken))
                    cancellationToken.ThrowIfCancellationRequested();

                PassClearance(response); // cancellationToken
                response = Send(uri, cancellationToken);

                retries++;
            }

            // Clearance failed.

            if (IsClearanceRequired(response))
                throw new CloudFlareClearanceException(retries);

            return response;
            /*
            var idCookieAfter = ClientHandler.CookieContainer.GetCookiesByName(request.RequestUri, IdCookieName).FirstOrDefault();
            var clearanceCookieAfter = ClientHandler.CookieContainer.GetCookiesByName(request.RequestUri, ClearanceCookieName).FirstOrDefault();

            // inject set-cookie headers in case the cookies changed
            if (idCookieAfter != null && idCookieAfter != idCookieBefore)
            {
                response.Headers.Add(HttpHeader.SetCookie, idCookieAfter.ToHeaderValue());
            }
            if (clearanceCookieAfter != null && clearanceCookieAfter != clearanceCookieBefore)
            {
                response.Headers.Add(HttpHeader.SetCookie, clearanceCookieAfter.ToHeaderValue());
            }

            return response;*/
        }

        private static bool IsClearanceRequired(HttpResponse response)
        {
            bool isServiceUnavailable = response.StatusCode == HttpStatusCode.ServiceUnavailable;
            bool isCloudFlareServer = response[Net.HttpHeader.Server].IndexOf("cloudflare", StringComparison.OrdinalIgnoreCase) != -1;
                // .Any(i => i != null && CloudFlareServerNames.Any(s => string.Compare(s, i, StringComparison.OrdinalIgnoreCase) == 0));

            return isServiceUnavailable && isCloudFlareServer;
        }
        

        private void PassClearance(HttpResponse response) // , CancellationToken cancellationToken = default(CancellationToken))
        {
            var pageContent = response.ToString();
            var scheme = response.Address.Scheme;
            var host = response.Address.Host;
            var port = response.Address.Port;

            var solution = ChallengeSolver.Solve(pageContent, host);

            // TODO: set 4000 ms
            Thread.Sleep(4000);
            // await Task.Delay(5000, cancellationToken);

            // Using Uri for correct port resolving
            // Publish form with challenge solution
            Get(new Uri($"{scheme}://{host}:{port}{solution.ClearanceQuery}")).None();
        }        
    }
}