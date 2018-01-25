using System;
using System.Threading;
using System.Threading.Tasks;

namespace Leaf.Net.Services.Cloudflare
{
    /// <summary>
    /// Cloudflare Anti-DDoS bypass extension for HttpRequest.
    /// </summary>
    /// <remarks>
    /// Only the JavaScript challenge can be handled. CAPTCHA and IP address blocking cannot be bypassed.
    /// </remarks>
    public static class CloudflareBypass
    {
        /// <summary>
        /// Gets or sets the number of clearance retries, if clearance fails.
        /// </summary>
        /// <remarks>A negative value causes an infinite amount of retries.</remarks>
        public static int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Delay before post form with solution in milliseconds.
        /// </summary>
        /// <remarks>Recommended value is 4000 ms. You can look extract value at challenge HTML. Second argument of setTimeout().</remarks>
        public static int Delay { get; set; } = 4000;

        /// <summary>
        /// Check response for Cloudflare protection.
        /// </summary>
        /// <returns>Returns <see cref="true"/> if response has Cloudflare protection challenge.</returns>
        public static bool IsCloudflared(this HttpResponse response)
        {
            bool serviceUnavailable = response.StatusCode == HttpStatusCode.ServiceUnavailable;
            bool cloudflareServer = response[HttpHeader.Server].IndexOf("cloudflare", StringComparison.OrdinalIgnoreCase) != -1;

            return serviceUnavailable && cloudflareServer;
        }

        /// <summary>
        /// GET request with bypassing Cloudflare JavaScript challenge.
        /// </summary>
        /// <param name="uri">Address</param>
        /// <param name="cancellationToken">Cancel protection</param>
        /// <returns>Returns original HttpResponse</returns>
        public static HttpResponse GetThroughCloudflare(this HttpRequest request, Uri uri,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // User-Agent is required
            if (string.IsNullOrEmpty(request.UserAgent))
                request.UserAgent = Http.ChromeUserAgent();

            var response = Get(request, uri);

            // (Re)try clearance if required.
            int retries = 0;
            while (response.IsCloudflared() && (MaxRetries < 0 || retries <= MaxRetries))
            {
                if (cancellationToken != default(CancellationToken))
                    cancellationToken.ThrowIfCancellationRequested();

                PassClearance(request, response, cancellationToken);
                response = GetThroughCloudflare(request, uri, cancellationToken);

                retries++;
            }

            // Clearance failed.
            if (response.IsCloudflared())
                throw new CloudflareException(retries);

            return response;
        }

        /// <inheritdoc />
        /// <param name="url">Url</param>
        public static HttpResponse GetThroughCloudflare(this HttpRequest request, string url,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetThroughCloudflare(request, new Uri(url), cancellationToken);
        }

        /// <summary>
        /// Async GET request with bypassing Cloudflare JavaScript challenge.
        /// </summary>
        /// <param name="uri">Address</param>
        /// <param name="cancellationToken">Cancel protection</param>
        /// <returns>Returns original HttpResponse</returns>
        public static async Task<HttpResponse> GetAsyncThroughCloudflare(this HttpRequest request, Uri uri,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // User-Agent is required
            if (string.IsNullOrEmpty(request.UserAgent))
                request.UserAgent = Http.ChromeUserAgent();

            var response = await GetAsync(request, uri);

            // (Re)try clearance if required.
            int retries = 0;
            while (response.IsCloudflared() && (MaxRetries < 0 || retries <= MaxRetries))
            {
                if (cancellationToken != default(CancellationToken))
                    cancellationToken.ThrowIfCancellationRequested();

                await PassClearanceAsync(request, response, cancellationToken);
                response = await GetAsyncThroughCloudflare(request, uri, cancellationToken);

                retries++;
            }

            // Clearance failed.
            if (response.IsCloudflared())
                throw new CloudflareException(retries);

            return response;
        }

        /// <inheritdoc />
        /// <param name="url">Url</param>
        public static async Task<HttpResponse> GetAsyncThroughCloudflare(this HttpRequest request, string url,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetAsyncThroughCloudflare(request, new Uri(url), cancellationToken);
        }

        private static Uri GetSolutionUri(HttpResponse response)
        {
            var pageContent = response.ToString();
            var scheme = response.Address.Scheme;
            var host = response.Address.Host;
            var port = response.Address.Port;
            var solution = ChallengeSolver.Solve(pageContent, host);

            return new Uri($"{scheme}://{host}:{port}{solution.ClearanceQuery}");
        }

        private static HttpResponse Get(HttpRequest request, Uri uri)
        {
            request.ManualMode = true;
            var response = request.Get(uri);
            request.ManualMode = false;

            return response;
        }

        private static async Task<HttpResponse> GetAsync(HttpRequest request, Uri uri)
        {
            request.ManualMode = true;
            var response = await request.GetAsync(uri);
            request.ManualMode = false;

            return response;
        }

        private static void PassClearance(HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
        {
            // Using Uri for correct port resolving
            var uri = GetSolutionUri(response);

            if (cancellationToken == default(CancellationToken))
                Thread.Sleep(Delay);
            else
                cancellationToken.WaitHandle.WaitOne(Delay);

            // Publish form with challenge solution
            Get(request, uri).None();
        }

        private static async Task PassClearanceAsync(HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
        {
            // Using Uri for correct port resolving
            var uri = GetSolutionUri(response);

            await Task.Delay(Delay, cancellationToken);

            // Publish form with challenge solution
            (await GetAsync(request, uri)).None();
        }
    }
}