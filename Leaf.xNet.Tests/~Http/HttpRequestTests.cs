using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Leaf.xNet.Tests
{
    [TestClass]
    public class HttpRequestTests
    {
        private const string BaseUrl = "http://httpbin.org";


        [TestMethod]
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
                    if (string.IsNullOrEmpty(req.UserAgent))
                        Assert.Fail("Не удалось сгенерировать случайный UserAgent");

                    if (lastUserAgent != req.UserAgent)
                        ++scores;

                    lastUserAgent = req.UserAgent;
                }
                        
                if (scores < minScore)
                    Assert.Fail($"За {generateRetries} попыток не было сгенерировано минимальное число агентов (${minScore})");

                string useragentJson = req.Get("/user-agent").ToString();
                StringAssert.Contains(useragentJson, req.UserAgent);
            }
        }

        [TestMethod]
        public void GetTest()
        {
            const string getArgument = "getArgument";
            const string getValue = "getValue";

            using (var req = new HttpRequest(BaseUrl))
            {
                var response = req.Get("/get?{getArgument}={getValue}");
                var source = response.ToString();

                StringAssert.Contains(source, getArgument);
                StringAssert.Contains(source, getValue);
            }
        }

        [TestMethod]
        public void PostTestFormEncoded()
        {
            const string postArgument = "postArgument";
            const string postValue = "postValue";

            using (var req = new HttpRequest(BaseUrl))
            {
                var rp = new RequestParams {
                    [postArgument] = postValue
                };

                var response = req.Post("/post", rp);
                var source = response.ToString();

                StringAssert.Contains(source, postArgument);
                StringAssert.Contains(source, postValue);
            }
        }

        [TestMethod]
        public void PostTestMultipart()
        {
            const string postArgument = "postArgument";
            const string postValue = "postValue";

            using (var req = new HttpRequest(BaseUrl))
            {
                var rp = new MultipartContent {
                    { new StringContent(postValue), postArgument}
                };

                var response = req.Post("/post", rp);
                var source = response.ToString();

                StringAssert.Contains(source, postArgument);
                StringAssert.Contains(source, postValue);
            }
        }
        /*
        [TestMethod]
        public void PostTest2()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest3()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest4()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest5()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest6()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest7()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest8()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest9()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest10()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest11()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest12()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void PostTest13()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void RawTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void RawTest1()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void AddHeaderTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void AddXmlHttpRequestHeaderTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void AddHeaderTest1()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void CloseTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void DisposeTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void ContainsCookieTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void ContainsHeaderTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void ContainsHeaderTest1()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void EnumerateHeadersTest()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void ClearAllHeadersTest()
        {
            Assert.Fail();
        }*/
    }
}
