using System;
using System.Linq;
using Tuan.Net.X.Services.Captcha;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Leaf.xNet.Services.Captcha.Tests
{
    public class CaptchaProxyTests
    {
        [Fact]
        public void CaptchaProxy__Default()
        {
            var cp = new CaptchaProxy();
            Assert.Equal(default, cp);
            Assert.Null(cp.Address);
            Assert.False(cp.IsValid);
        }

        [Fact]
        public void CaptchaProxy__Valid_CaptchaProxyType()
        {
            const string address = "127.0.0.1:8080";
            var proxyTypes = Enum.GetValues(typeof(CaptchaProxyType)).Cast<CaptchaProxyType>().ToArray();

            // Using ProxyType via CaptchaProxyType 
            foreach (var proxyType in proxyTypes)
            {
                var cp = new CaptchaProxy(proxyType, address);
                Assert.NotEqual(default, cp);
                Assert.Equal(proxyType, cp.Type);
                Assert.Equal(address, cp.Address);
                Assert.True(cp.IsValid);
            }
            
            // Using ProxyType via string
            foreach (var proxyType in proxyTypes)
            {
                string proxyTypeStr = proxyType.ToString();

                var cp = new CaptchaProxy(proxyTypeStr, address);
                Assert.NotEqual(default, cp);
                Assert.Equal(proxyTypeStr, cp.Type.ToString());
                Assert.Equal(address, cp.Address);
                Assert.True(cp.IsValid);
            }

            // Check ProxyType via string when it's invalid
            Assert.Throws<ArgumentException>(() => {
                var _ = new CaptchaProxy("UnknownProxyType", address);
            });
        }

        [Fact]
        public void CaptchaProxy__Invalid_Address()
        {
            const CaptchaProxyType type = CaptchaProxyType.HTTPS;

            Assert.Throws<ArgumentException>(() => {
                var _ = new CaptchaProxy(type, null);
            });

            Assert.Throws<ArgumentException>(() => {
                var _ = new CaptchaProxy(type, string.Empty);
            });

            Assert.Throws<ArgumentException>(() => {
                var _ = new CaptchaProxy(type, "127.0.0.1");
            });

            Assert.Throws<ArgumentException>(() => {
                var _ = new CaptchaProxy(type, "127.0.0.1:");
            });

            Assert.Throws<ArgumentException>(() => {
                var _ = new CaptchaProxy(type, "127.0.0.1:8");
            });
        }

    }
}