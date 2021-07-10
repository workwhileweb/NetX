using System.Collections.Generic;
using Xunit;

namespace Tuan.Net.X.Tests
{
    public class HttpRequestTests
    {
        private const string BaseUrl = "https://nghttp2.org";

        [Fact]
        public void UserAgentRandomizeTest()
        {
            const int generateRetries = 10;
            const int minScore = 3;
            int scores = 0;

            using (var req = new HttpRequest(BaseUrl))
            {                
                string lastUserAgent = null;
                for (int i = 0; i < generateRetries; i++)
                {
                    req.UserAgentRandomize();
                    Assert.False(string.IsNullOrEmpty(req.UserAgent), "Unable to generate random UserAgent");
                    
                    if (lastUserAgent != req.UserAgent)
                        ++scores;

                    lastUserAgent = req.UserAgent;
                }
                
                Assert.False(scores < minScore,
                    $"For {generateRetries} retries unable to generate minimal count of user agents (${minScore})");

                string useragentJson = req.Get("/httpbin/user-agent").ToString();
                Assert.Contains(req.UserAgent, useragentJson);
            }
        }

        [Fact]
        public void GetTest()
        {
            const string getArgument = "getArgument";
            const string getValue = "getValue";

            using (var req = new HttpRequest(BaseUrl))
            {
                var response = req.Get("/httpbin/get?{getArgument}={getValue}");
                string source = response.ToString();

                Assert.Contains(getArgument, source);
                Assert.Contains(getValue, source);
            }
        }

        [Fact]
        public void PostTestFormEncoded()
        {
            const string postArgument = "postArgument";
            const string postValue = "postValue";

            using (var req = new HttpRequest(BaseUrl))
            {
                var rp = new RequestParams {
                    [postArgument] = postValue
                };

                var response = req.Post("/httpbin/post", rp);
                string source = response.ToString();

                Assert.Contains(postArgument, source);
                Assert.Contains(postValue, source);
            }
        }

        [Fact]
        public void PostTestMultipart()
        {
            const string postArgument = "postArgument";
            const string postValue = "postValue";

            using (var req = new HttpRequest(BaseUrl))
            {
                var rp = new MultipartContent {
                    { new StringContent(postValue), postArgument}
                };

                var response = req.Post("/httpbin/post", rp);
                var source = response.ToString();

                Assert.Contains(postArgument, source);
                Assert.Contains(postValue, source);
            }
        }

        [Fact]
        public void FilterCookies()
        {
            var list = new Dictionary<string, string> {
                {
                    "MS_LOGIN_COOKIE_10151=-1,R,L,null,; Expires=Sun, 30-Dec-18 16:57:13 GMT; Path=/RemovedPath",
                    "MS_LOGIN_COOKIE_10151=-1,R,L,null%2C; Expires=Sun, 30-Dec-18 16:57:13 GMT; Path=/"
                },
                {
                    "MS_LOGIN_COOKIE_10151=-1,R,L,null,; Expires=Sun, 30-Dec-18 16:57:13 GMT; Path=/",
                    "MS_LOGIN_COOKIE_10151=-1,R,L,null%2C; Expires=Sun, 30-Dec-18 16:57:13 GMT; Path=/"
                }
            };

            foreach (var item in list)
            {
                Assert.Equal(item.Value, CookieFilters.Filter(item.Key));
            }
        }

        [Fact]
        public void FilterCookieDomains()
        {
            var validDomains = new[] {
                // 1st level
                "localhost",
                // 2nd level
                "google.com",
                // 3rd level wildcard subdomains
                ".google.com",
                // 3rd level
                "maps.google.com",
                ".maps.google.com",

            };

            foreach (string validDomain in validDomains)
                Assert.NotNull(CookieFilters.FilterDomain(validDomain));

            // Normalized Wildcard Domains
            //
            // 1st level with wildcard will be normalized to
            var normalizedDomains = new Dictionary<string, string> {
                {"  .localhost", "localhost"}
            };

            foreach (var normalizedDomain in normalizedDomains)
            {
                string filteredNormalizedDomain = CookieFilters.FilterDomain(normalizedDomain.Key);
                Assert.Equal(normalizedDomain.Value, filteredNormalizedDomain);
            }

            // Invalid Domains
            //
            var invalidDomains = new[] {
                "    ",
                null,
                "\r\n\t\t",
                "\n\n "
            };
            foreach (string invalidDomain in invalidDomains)
            {
                Assert.Null(CookieFilters.FilterDomain(invalidDomain));
            }
        }
    }
}
