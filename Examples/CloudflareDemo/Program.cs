//#define CHARLES

using System;
using System.IO;
using Tuan.Net.X.Services.Captcha;
using Tuan.Net.X.Services.Cloudflare;

namespace Tuan.Net.X.Examples.CloudflareDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            HttpRequest http = null;

            try
            {
                http = new HttpRequest();
                http.UserAgentRandomize();

                #if CHARLES
                http.Proxy = ProxyClient.DebugHttpProxy;
                #endif

                // JS Challenge (404)
                /*
                var respJsChallenge404 = http.GetThroughCloudflare("https://uam.hitmehard.fun/");
                //string respJsChallengeStr404 = respJsChallenge404.ToString();

                bool jsChallengePassed404 = respJsChallenge404.StatusCode == HttpStatusCode.NotFound;
                Console.WriteLine($@"{nameof(jsChallengePassed404)} = {jsChallengePassed404}");
                */


                // JS Challenge (+ custom port)
                var respJsChallenge = http.GetThroughCloudflare("http://lionroyalcpk.club:2082/");
                string respJsChallengeStr = respJsChallenge.ToString();

                bool jsChallengePassed = respJsChallengeStr.Contains("Lion Royal Casino");
                Console.WriteLine($@"{nameof(jsChallengePassed)} = {jsChallengePassed}");

                // Recaptcha Challenge
                // You can use: RucaptchaSolver | TwoCaptchaSolver | CapmonsterSolver
                http.CaptchaSolver = new RucaptchaSolver {
                    ApiKey = File.ReadAllText("api_key.txt")
                };

                var respRecaptcha = http.GetThroughCloudflare("http://pablojing.club/");
                string respRecaptchaStr = respRecaptcha.ToString();

                bool recaptchaChallengePassed = respRecaptchaStr.Contains("Pablo Poker | Sign in");
                Console.WriteLine($@"{nameof(recaptchaChallengePassed)} = {recaptchaChallengePassed}");

                // Recaptcha Challenge (Access Denied)
                try
                {
                    http.GetThroughCloudflare("https://lobbyl.wiw818.pw:8443/");
                    Console.WriteLine(@"CloudFlare Access Denied PASSED! So your IP is allowed!");
                }
                catch (CloudflareException cfException)
                {
                    Console.WriteLine($@"CloudFlare, EXCEPTION raised when accessed to forbidden host by IP: {cfException.Message}");
                    Console.WriteLine(@"It should have ""access denied..."" message");
                }
                
                Console.WriteLine($@"{nameof(recaptchaChallengePassed)} = {recaptchaChallengePassed}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"ERROR: {ex.Message}");
            }
            finally
            {
                http?.Dispose();
            }

            Console.ReadKey();
        }
    }
}
