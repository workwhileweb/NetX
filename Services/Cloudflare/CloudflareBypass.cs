using System;
using System.Net;
using System.Threading;

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
        public delegate void DLog(string message);

        public delegate string DSolveRecaptcha(string siteKey);


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

        private static void AddCloudflareHeaders(this HttpRequest request, string refererUrl)
        {
            request.AddHeader(HttpHeader.Referer, refererUrl);
            request.AddHeader(HttpHeader.Accept,
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            request.AddHeader("Upgrade-Insecure-Requests", "1");
            request.AddHeader(HttpHeader.AcceptLanguage, "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
        }

        /// <summary>
        /// GET request with bypassing Cloudflare JavaScript challenge.
        /// </summary>
        /// <param name="uri">Address</param>
        /// <param name="log">Log action</param>
        /// <param name="cancellationToken">Cancel protection</param>
        /// <returns>Returns original HttpResponse</returns>
        public static HttpResponse GetThroughCloudflare(this HttpRequest request, string url,
            DLog log = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // User-Agent is required
            if (string.IsNullOrEmpty(request.UserAgent))
                request.UserAgent = Http.ChromeUserAgent();

            log?.Invoke("Проверяем наличие CloudFlare по адресу: " + url);

            for (int i = 0; i < MaxRetries; i++)
            {
                string retry = $". Попытка {i + 1} из {MaxRetries}.";
                log?.Invoke("Обхожу CloudFlare" + retry);

                
                var response = ManualGet(request, url);
                if (!response.IsCloudflared())
                {
                    log?.Invoke("УСПЕХ: Cloudflare не обнаружен, работаем дальше: " + url);
                    return response;
                }

                // Remove expired clearance if present
                var cookies = request.Cookies.GetCookies(url);
                foreach (Cookie cookie in cookies)
                {
                    if (cookie.Name != "cf_clearance") 
                        continue;

                    cookie.Expired = true;
                    break;
                }

                if (cancellationToken != default(CancellationToken))
                    cancellationToken.ThrowIfCancellationRequested();

                response = PassClearance(request, response, url, log, cancellationToken);

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (response.StatusCode) {
                    case HttpStatusCode.ServiceUnavailable:
                    case HttpStatusCode.Forbidden:
                        continue;
                    case HttpStatusCode.Found:
                        // Т.к. ранее использовался ручной режим - нужно обработать редирект, если он есть, чтобы вернуть отфильтрованное тело запроса    
                        if (response.HasRedirect)
                        {
                            if (!response.ContainsCookie(url, "cf_clearance"))
                                continue;

                            log?.Invoke($"CloudFlare этап пройден, получаю оригинальную страницу: {url}...");

                            // Не используем manual т.к. могут быть переадресации
                            bool ignoreProtocolErrors = request.IgnoreProtocolErrors;
                            // Отключаем обработку HTTP ошибок
                            request.IgnoreProtocolErrors = true;

                            request.AddCloudflareHeaders(url); // заголовки важны для прохождения cloudflare
                            response = request.Get(response.RedirectAddress.AbsoluteUri);
                            request.IgnoreProtocolErrors = ignoreProtocolErrors;

                            if (IsCloudflared(response))
                            {
                                log?.Invoke($"Ошибка CloudFlare: этап пройден, но оригинальная страница не получена: {url}");
                                continue;
                            }
                        }

                        log?.Invoke($"CloudFlare: успех, оригинальная страница получена: {url}");
                        return response;
                }

                log?.Invoke($"CloudFlare не смог пройти JS Challange, причина не ясна. Статус код: {response.StatusCode}" + retry);
            }

            throw new CloudflareException(MaxRetries, "Превышен лимит попыток обхода Cloudflare");
        }

        /// <inheritdoc cref="GetThroughCloudflare()"/>
        /// <param name="uri">Uri Address</param>
        public static HttpResponse GetThroughCloudflare(this HttpRequest request, Uri uri,
            DLog log = null,
            #if USE_CAPTCHA_SOLVER
            DSolveRecaptcha reCaptchaSolver = null,
            #endif
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetThroughCloudflare(request, uri.AbsoluteUri, log,
                #if USE_CAPTCHA_SOLVER
                reCaptchaSolver,
                #endif
                cancellationToken);
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

        private static HttpResponse ManualGet(HttpRequest request, string url, string refererUrl = null)
        {
            request.ManualMode = true;

            request.AddCloudflareHeaders(refererUrl ?? url);
            var response = request.Get(url);

            request.ManualMode = false;

            return response;
        }

        private static HttpResponse PassClearance(HttpRequest request, HttpResponse response,
            string refererUrl,
            DLog log,
            CancellationToken cancellationToken)
        {
            // Using Uri for correct port resolving
            var uri = GetSolutionUri(response);

            log?.Invoke($"CloudFlare: ждем {Delay} мс...");
            if (cancellationToken == default(CancellationToken))
                Thread.Sleep(Delay);
            else
            {
                cancellationToken.WaitHandle.WaitOne(Delay);
                cancellationToken.ThrowIfCancellationRequested();
            }

            return ManualGet(request, uri.AbsoluteUri, refererUrl);
        }
    }
}