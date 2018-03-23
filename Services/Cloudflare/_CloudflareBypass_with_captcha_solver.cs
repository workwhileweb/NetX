//#define USE_CAPTCHA_SOLVER

using System;
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

        /// <summary>
        /// GET request with bypassing Cloudflare JavaScript challenge.
        /// </summary>
        /// <param name="uri">Address</param>
        /// <param name="log">Log action</param>
        /// <param name="cancellationToken">Cancel protection</param>
        /// <returns>Returns original HttpResponse</returns>
        public static HttpResponse GetThroughCloudflare(this HttpRequest request, string url,
            DLog log = null,
            #if USE_CAPTCHA_SOLVER
            DSolveRecaptcha reCaptchaSolver = null,
            #endif
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // User-Agent is required
            if (string.IsNullOrEmpty(request.UserAgent))
                request.UserAgent = Http.ChromeUserAgent();

            log?.Invoke("Проверяем наличие CloudFlare по адресу: " + url);

            HttpResponse response = null;

            for (int i = 0; i < MaxRetries; i++)
            {
                string retry = $". Попытка {i + 1} из {MaxRetries}.";

                #region Try catch disabled
                /*try
                {*/
                #endregion

                response = ManualGet(request, url);
                if (!response.IsCloudflared())
                {
                    log?.Invoke("УСПЕХ: Cloudflare не обнаружен, работаем дальше: " + url);
                    return response;
                }

                log?.Invoke("Обхожу CloudFlare" + retry);

                if (cancellationToken != default(CancellationToken))
                    cancellationToken.ThrowIfCancellationRequested();

                response = PassClearance(request, response, url, log, cancellationToken);

                // JS Challange passed
                if (response.StatusCode == HttpStatusCode.Found)
                {
                    // Т.к. ранее использовался ручной режим - нужно обработать редирект, если он есть, чтобы вернуть отфильтрованное тело запроса

                    // TODO: иногда бывает 403 forbidden из-за cloudflare, в чем проблема?
                    if (response.HasRedirect)
                        response = request.Get(response.RedirectAddress);

                    break;
                }

                #if USE_CAPTCHA_SOLVER
                if (response.StatusCode == HttpStatusCode.Forbidden && reCaptchaSolver == null)
                    continue;
                #else
                if (response.StatusCode == HttpStatusCode.Forbidden) // && reCaptchaSolver == null)
                    continue;
                #endif

                // Not implemented status code
                if (response.StatusCode != HttpStatusCode.Forbidden)
                {
                    log?.Invoke(
                        $"CloudFlare не смог пройти JS Challange, причина не ясна. Статус код: {response.StatusCode}" +
                        retry);

                    #if USE_CAPTCHA_SOLVER
                    continue;
                    #endif
                }

                #region Captcha solver
                #if USE_CAPTCHA_SOLVER
                // IF Forbidden and has recaptcha
                //

                // ReCaptcha solve required
                string strResponse = response.ToString();
                // TODO: while for captcha solving
                const string siteKeyPattern = "data-sitekey=\"";
                if (!strResponse.Contains(siteKeyPattern))
                {
                    string error =
                        "CloudFlare не смог пройти т.к. возращен код Forbidden, но ключ сайта для рекаптчи не найден" +
                        retry;

                    log?.Invoke(error);
                    throw new CloudflareException(error);
                }

                if (reCaptchaSolver == null)
                    throw new CloudflareException("Cloudflare требует решение ReCaptcha, но делегат для её решения не был предоставлен");
    
                string siteKey = strResponse.Substring(siteKeyPattern, "\"");
                if (siteKey == string.Empty)
                    throw new CloudflareException("Cloudflare требует решение ReCaptcha, но ключ сайта не был найден в HTML коде");
    
                string submitRelativeUrl = strResponse.Substring("id=\"challenge-form\" action=\"", "\"");
                if (submitRelativeUrl == string.Empty)
                    throw new CloudflareException("Cloudflare требует решение ReCaptcha, но адрес публикации формы с каптчей не найден в HTML коде");
    
                // Build uri Form GET action
                var address = response.Address;
                var submitUri = new Uri($"{address.Scheme}://{address.Host}:{address.Port}{submitRelativeUrl}");
    
                log?.Invoke("CloudFlare выдал рекаптчу, решаю...");
    
                // Решаем рекаптчу и отправляем форму
                var rp = new RequestParams { ["g-recaptcha-response"] = reCaptchaSolver(siteKey) };
                response = request.Get(submitUri, rp);
    
                // После отправки формы кидает на главную, поэтому идем по адресу какой требуется согласно функции
                if (response.Address.AbsoluteUri != url)
                    response = request.Get(url);

                #endif
                
                #endregion
            }

            if (response == null)
                throw new Exception("Ответ null");

            // Clearance failed.
            if (response.IsCloudflared())
                throw new CloudflareException(MaxRetries, "Превышен лимит попыток обхода Cloudflare");

            log?.Invoke("CloudFlare успешно пройден");
            return response;
        }

        /// <inheritdoc />
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

        private static HttpResponse ManualGet(HttpRequest request, string url)
        {
            request.ManualMode = true;

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

            // Publish form with challenge solution
            request.AddHeader(HttpHeader.Referer, refererUrl);
            return ManualGet(request, uri.AbsoluteUri);
        }

#region Disabled code
        /*
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

    var response = await ManualGetAsync(request, uri);

    // (Re)try clearance if required.
    int retries = 0;
    while (response.IsCloudflared() && (MaxRetries < 0 || retries <= MaxRetries))
    {
        if (cancellationToken != default(CancellationToken))
            cancellationToken.ThrowIfCancellationRequested();

        await PassClearanceAsync(request, response, cancellationToken);
        response = await ManualGetAsync(request, uri); // await GetAsyncThroughCloudflare(request, uri, cancellationToken);

        retries++;
    }

    // Clearance failed.
    if (response.IsCloudflared())
        throw new CloudflareException(retries);

    // Т.к. ранее использовался ручной режим - нужно обработать редирект, чтобы вернуть нужное тело запроса
    if (response.HasRedirect)
        response = request.Get(response.RedirectAddress);

    return response;
}

/// <inheritdoc />
/// <param name="url">Url</param>
public static async Task<HttpResponse> GetAsyncThroughCloudflare(this HttpRequest request, string url,
    CancellationToken cancellationToken = default(CancellationToken))
{
    return await GetAsyncThroughCloudflare(request, new Uri(url), cancellationToken);
}
*/

        /*
private static async Task<HttpResponse> ManualGetAsync(HttpRequest request, string url)
{
    request.ManualMode = true;
    var response = await request.GetAsync(url);
    request.ManualMode = false;

    return response;
}*/

        /*
        private static async Task PassClearanceAsync(HttpRequest request, HttpResponse response, CancellationToken cancellationToken)
        {
            // Using Uri for correct port resolving
            var uri = GetSolutionUri(response);

            await Task.Delay(Delay, cancellationToken);

            // Publish form with challenge solution
            (await ManualGetAsync(request, uri.AbsoluteUri)).None();
        }*/
#endregion
    }
}