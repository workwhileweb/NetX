using System;
using System.Net;
using System.Threading;
using Leaf.xNet.Services.Captcha;

namespace Leaf.xNet.Services.Cloudflare
{
    /// <summary>
    /// CloudFlare Anti-DDoS bypass extension for HttpRequest.
    /// </summary>
    /// <remarks>
    /// Only the JavaScript challenge can be handled. CAPTCHA and IP address blocking cannot be bypassed.
    /// </remarks>
    // ReSharper disable once UnusedMember.Global
    public static class CloudflareBypass
    {
        #region Public / Private: Data

        /// <summary>
        /// Delegate for Log message to UI.
        /// </summary>
        /// <param name="message">Message</param>
        public delegate void DLog(string message);

        /// <summary>
        /// Cookie key name used for identify CF clearance.
        /// </summary>
        public const string CfClearanceCookie = "cf_clearance";

        /// <summary>
        /// Default Accept-Language header added to Cloudflare server request.
        /// </summary>
        public static string DefaultAcceptLanguage { get; set; } = "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7";

        /// <summary>
        /// Gets or sets the number of clearance retries, if clearance fails.
        /// </summary>
        /// <remarks>A negative value causes an infinite amount of retries.</remarks>
        public static int MaxRetries { get; set; } = 4;

        /// <summary>
        /// Delay before post form with solution in milliseconds.
        /// </summary>
        /// <remarks>Recommended value is 4000 ms. You can look extract value at challenge HTML. Second argument of setTimeout().</remarks>
        public static int Delay { get; set; } = 5000;

        private const string LogPrefix = "[Cloudflare] ";

        #endregion


        #region Public: Http Extensions

        /// <summary>
        /// Check response for Cloudflare protection.
        /// </summary>
        /// <returns>Returns <keyword>true</keyword> if response has Cloudflare protection challenge.</returns>
        public static bool IsCloudflared(this HttpResponse response)
        {
            bool serviceUnavailable = response.StatusCode == HttpStatusCode.ServiceUnavailable || response.StatusCode == HttpStatusCode.Forbidden;
            bool cloudflareServer = response[HttpHeader.Server].IndexOf("cloudflare", StringComparison.OrdinalIgnoreCase) != -1;

            return serviceUnavailable && cloudflareServer;
        }

        /// <summary>
        /// GET request with bypassing Cloudflare JavaScript challenge.
        /// </summary>
        /// <param name="request">Http request</param>
        /// <param name="url">Address</param>
        /// <param name="log">Log action</param>
        /// <param name="cancellationToken">Cancel protection</param>
        /// <param name="captchaSolver">Captcha solving provider when Recaptcha required for pass</param>
        /// <returns>Returns original HttpResponse</returns>
        public static HttpResponse GetThroughCloudflare(this HttpRequest request, string url,
            DLog log = null,
            CancellationToken cancellationToken = default(CancellationToken),
            ICaptchaSolver captchaSolver = null)
        {
            if (!request.UseCookies)
                throw new CloudflareException($"{LogPrefix}Cookies must be enabled. Please set ${nameof(HttpRequest.UseCookies)} to true.");

            // User-Agent is required
            if (string.IsNullOrEmpty(request.UserAgent))
                request.UserAgent = Http.ChromeUserAgent();

            log?.Invoke($"{LogPrefix}Checking availability at: {url} ...");

            for (int i = 0; i < MaxRetries; i++)
            {
                string retry = $". Retry {i + 1} / {MaxRetries}.";
                log?.Invoke($"{LogPrefix}Trying to bypass{retry}");

                var response = ManualGet(request, url);
                if (!response.IsCloudflared())
                {
                    log?.Invoke($"{LogPrefix} OK. Not found at: {url}");
                    return response;
                }

                // Remove expired clearance if present
                var cookies = request.Cookies.GetCookies(url);
                foreach (Cookie cookie in cookies)
                {
                    if (cookie.Name != CfClearanceCookie) 
                        continue;

                    cookie.Expired = true;
                    break;
                }

                if (cancellationToken != default(CancellationToken))
                    cancellationToken.ThrowIfCancellationRequested();

                // Bypass depend on challenge type: JS / Recaptcha
                //
                if (IsJsChallenge(response))
                {
                    if (SolveJsChallenge(ref response, request, url, retry, log, cancellationToken))
                        return response;
                }
                else if (IsRecaptchaChallenge(response))
                {
                    if (SolveRecaptchaChallenge(ref response, request, url, retry, log, cancellationToken))
                        return response;
                }
                else
                    throw new CloudflareException("Unknown challenge type");
            }

            throw new CloudflareException(MaxRetries, $"{LogPrefix}ERROR. Rate limit reached.");
        }

        /// <inheritdoc cref="GetThroughCloudflare(HttpRequest, string, DLog, CancellationToken, ICaptchaSolver)"/>
        /// <param name="request">Http request</param>
        /// <param name="uri">Uri Address</param>
        /// <param name="log">Log delegate</param>
        /// <param name="cancellationToken"></param>
        // ReSharper disable once UnusedMember.Global
        public static HttpResponse GetThroughCloudflare(this HttpRequest request, Uri uri, 
            DLog log = null,
            CancellationToken cancellationToken = default(CancellationToken),
            ICaptchaSolver captchaSolver = null)
        {
            return GetThroughCloudflare(request, uri.AbsoluteUri, log, cancellationToken, captchaSolver);
        }

        #endregion


        #region Private: General (GetSolutionUri & PassClearance)

        private static Uri GetSolutionUri(HttpResponse response)
        {
            string pageContent = response.ToString();
            string scheme = response.Address.Scheme;
            string host = response.Address.Host;
            int port = response.Address.Port;
            var solution = ChallengeSolver.Solve(pageContent, host, port);

            return new Uri($"{scheme}://{host}:{port}{solution.ClearanceQuery}");
        }

        private static HttpResponse PassClearance(HttpRequest request, HttpResponse response, string refererUrl,
            DLog log, CancellationToken cancellationToken)
        {
            // Using Uri for correct port resolving
            var uri = GetSolutionUri(response);

            log?.Invoke($"{LogPrefix}: delay {Delay} ms...");
            if (cancellationToken == default(CancellationToken))
                Thread.Sleep(Delay);
            else
            {
                cancellationToken.WaitHandle.WaitOne(Delay);
                cancellationToken.ThrowIfCancellationRequested();
            }

            return request.ManualGet( uri.AbsoluteUri, refererUrl);
        }

        #endregion


        #region Private: Challenge (JS)

        private static bool IsJsChallenge(HttpResponse response)
        {
            // Cross-platform string.Contains
            return response.ToString().IndexOf("jschl-answer", StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static bool SolveJsChallenge(ref HttpResponse response, HttpRequest request, string url, string retry, 
            DLog log, CancellationToken cancellationToken)
        {
            response = PassClearance(request, response, url, log, cancellationToken);

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (response.StatusCode) {
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.Forbidden:
                    return false;
                case HttpStatusCode.Found:
                    // Т.к. ранее использовался ручной режим - нужно обработать редирект, если он есть, чтобы вернуть отфильтрованное тело запроса    
                    if (response.HasRedirect)
                    {
                        if (!response.ContainsCookie(url, CfClearanceCookie))
                            return false;

                        log?.Invoke($"{LogPrefix}Passed. Trying to get the original response at: {url} ...");

                        // Не используем manual т.к. могут быть переадресации
                        bool ignoreProtocolErrors = request.IgnoreProtocolErrors;
                        // Отключаем обработку HTTP ошибок
                        request.IgnoreProtocolErrors = true;

                        request.AddCloudflareHeaders(url); // заголовки важны для прохождения cloudflare
                        response = request.Get(response.RedirectAddress.AbsoluteUri);
                        request.IgnoreProtocolErrors = ignoreProtocolErrors;

                        if (IsCloudflared(response))
                        {
                            log?.Invoke($"{LogPrefix}ERROR. Unable to get he original response at: {url}");
                            return false;
                        }
                    }

                    log?.Invoke($"{LogPrefix}OK. Done: {url}");
                    return true;
            }

            log?.Invoke($"{LogPrefix}ERROR. Status code: {response.StatusCode}{retry}.");
            return false;
        }

        #endregion


        #region Private: Challenge (Recaptcha)

        private static bool IsRecaptchaChallenge(HttpResponse response)
        {
            // Cross-platform string.Contains
            return response.ToString().IndexOf("<div class=\"g-recaptcha\">", StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static bool SolveRecaptchaChallenge(ref HttpResponse response, HttpRequest request, string url, string retry, 
            DLog log, CancellationToken cancellationToken)
        {
            if (request.CaptchaSolver == null)
                throw new CaptchaException(CaptchaError.CaptchaResolverRequired);

            throw new NotImplementedException();
        }

        #endregion


        #region Private: HttpRequest Extensions

        private static HttpResponse ManualGet(this HttpRequest request, string url, string refererUrl = null)
        {
            request.ManualMode = true;

            request.AddCloudflareHeaders(refererUrl ?? url);
            var response = request.Get(url);

            request.ManualMode = false;

            return response;
        }

        private static void AddCloudflareHeaders(this HttpRequest request, string refererUrl)
        {
            request.AddHeader(HttpHeader.Referer, refererUrl);
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            request.AddHeader("Upgrade-Insecure-Requests", "1");
            
            if (!request.ContainsHeader(HttpHeader.AcceptLanguage))
                request.AddHeader(HttpHeader.AcceptLanguage, DefaultAcceptLanguage);
        }

        #endregion
    }
}
