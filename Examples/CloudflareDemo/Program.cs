#define CHARLES
using System;
using System.IO;
using Leaf.xNet;
using Leaf.xNet.Services.Captcha;
using Leaf.xNet.Services.Cloudflare;

namespace CloudflareDemo
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
                http.Proxy = HttpProxyClient.DebugProxy;
                http.Proxy.AbsoluteUriInStartingLine = false;
                #endif

                // JS Challenge (+ custom port)
                var respJsChallenge = http.GetThroughCloudflare("http://lionroyalcpk.club:2082/");
                string respJsChallengeStr = respJsChallenge.ToString();

                bool jsChallengePassed = respJsChallengeStr.Contains("Lion Royal Casino");
                Console.WriteLine($"{nameof(jsChallengePassed)} = {jsChallengePassed}");

                // Recaptcha Challenge
                // You can use: RucaptchaSolver | TwoCaptchaSolver | CapmonsterSolver
                http.CaptchaSolver = new RucaptchaSolver {
                    ApiKey = File.ReadAllText("api_key.txt")
                };

                var respRecaptcha = http.GetThroughCloudflare("http://pablojing.club/");
                string respRecaptchaStr = respRecaptcha.ToString();

                bool recaptchaChallengePassed = respRecaptchaStr.Contains("Pablo Poker | Sign in");
                Console.WriteLine($"{nameof(recaptchaChallengePassed)} = {recaptchaChallengePassed}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
            finally
            {
                http?.Dispose();
            }

            Console.ReadKey();
        }
    }
}
