using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace Leaf.xNet.Services.Captcha.Tests
{
    [TestClass]
    public class CaptchaProxyTests
    {
        [TestMethod]
        public void CaptchaProxy__Default()
        {
            var cp = new CaptchaProxy();
            Assert.AreEqual(default(CaptchaProxy), cp);
            Assert.IsNull(cp.Address);
            Assert.IsFalse(cp.IsValid);
        }

        [TestMethod]
        public void CaptchaProxy__Valid_CaptchaProxyType()
        {
            const string address = "127.0.0.1:8080";
            var proxyTypes = Enum.GetValues(typeof(CaptchaProxyType)).Cast<CaptchaProxyType>().ToArray();

            // Using ProxyType via CaptchaProxyType 
            foreach (var proxyType in proxyTypes)
            {
                var cp = new CaptchaProxy(proxyType, address);
                Assert.AreNotEqual(default(CaptchaProxy), cp);
                Assert.AreEqual(proxyType, cp.Type);
                Assert.AreEqual(address, cp.Address);
                Assert.IsTrue(cp.IsValid);
            }
            
            // Using ProxyType via string
            foreach (var proxyType in proxyTypes)
            {
                string proxyTypeStr = proxyType.ToString();

                var cp = new CaptchaProxy(proxyTypeStr, address);
                Assert.AreNotEqual(default(CaptchaProxy), cp);
                Assert.AreEqual(proxyTypeStr, cp.Type.ToString());
                Assert.AreEqual(address, cp.Address);
                Assert.IsTrue(cp.IsValid);
            }

            // Check ProxyType via string when it's invalid
            Assert.ThrowsException<ArgumentException>(() => {
                var cp = new CaptchaProxy("UnknownProxyType", address);
            });
        }

        [TestMethod]
        public void CaptchaProxy__Invalid_Address()
        {
            const CaptchaProxyType type = CaptchaProxyType.HTTPS;

            Assert.ThrowsException<ArgumentException>(() => {
                var cp = new CaptchaProxy(type, null);
            });

            Assert.ThrowsException<ArgumentException>(() => {
                var cp = new CaptchaProxy(type, string.Empty);
            });

            Assert.ThrowsException<ArgumentException>(() => {
                var cp = new CaptchaProxy(type, "127.0.0.1");
            });

            Assert.ThrowsException<ArgumentException>(() => {
                var cp = new CaptchaProxy(type, "127.0.0.1:");
            });

            Assert.ThrowsException<ArgumentException>(() => {
                var cp = new CaptchaProxy(type, "127.0.0.1:8");
            });
        }

    }
}