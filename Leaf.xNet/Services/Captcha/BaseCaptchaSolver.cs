using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Leaf.xNet.Services.Captcha
{
    public abstract class BaseCaptchaSolver : ICaptchaSolver, IDisposable
    {
        public string ApiKey { get; set; }
        public virtual bool IsApiKeyValid => !string.IsNullOrEmpty(ApiKey);

        public uint UploadRetries { get; set; } = 40;
        public uint StatusRetries { get; set; } = 80;

        public TimeSpan UploadDelayOnNoSlotAvailable { get; set; }
        public TimeSpan StatusDelayOnNotReady { get; set; }
        public TimeSpan BeforeStatusCheckingDelay { get; set; } = TimeSpan.FromSeconds(4);

        public CancellationToken CancelToken { get; set; } = CancellationToken.None;

        protected readonly WebClient Http = new WebClient();

        #region SolveImage : Generic

        /// <exception cref="NotImplementedException">Throws when method isn't implemented by your class.</exception>
        public virtual string SolveImage(string imageUrl)
        {
            throw NotImplemented(nameof(SolveImage), "string");
        }

        /// <exception cref="NotImplementedException">Throws when method isn't implemented by your class.</exception>
        public virtual string SolveImage(byte[] imageBytes)
        {
            throw NotImplemented(nameof(SolveImage), "byte[]");
        }

        /// <exception cref="NotImplementedException">Throws when method isn't implemented by your class.</exception>
        public virtual string SolveImage(Stream imageStream)
        {
            throw NotImplemented(nameof(SolveImage), nameof(Stream));
        }

        /// <exception cref="NotImplementedException">Throws when method isn't implemented by your class.</exception>
        public string SolveImageFromBase64(string imageBase64)
        {
            throw NotImplemented(nameof(SolveImageFromBase64), "string");
        }

        #endregion

        /// <exception cref="NotImplementedException">Throws when method isn't implemented by your class.</exception>
        public virtual string SolveRecaptcha(string pageUrl, string siteKey)
        {
            throw NotImplemented(nameof(SolveRecaptcha), "string, string");
        }

        protected void ApiKeyRequired()
        {
            if (!IsApiKeyValid)
                throw new CaptchaException(CaptchaError.InvalidApiKey);
        }

        protected void Delay(TimeSpan delay)
        {
            if (CancelToken != CancellationToken.None)
            {
                CancelToken.WaitHandle.WaitOne(UploadDelayOnNoSlotAvailable);
                CancelToken.ThrowIfCancellationRequested();
            }
            else
                Thread.Sleep(UploadDelayOnNoSlotAvailable);
        }

        protected void ThrowOnCancel()
        {
            if (CancelToken != CancellationToken.None)
                CancelToken.ThrowIfCancellationRequested();
        }

        private NotImplementedException NotImplemented(string method, string parameterType)
        {
            return new NotImplementedException($"Method \"{method}\"({parameterType}) of {GetType().Name} isn't implemented");
        }

        #region IDisposable

        public virtual void Dispose()
        {
            Http?.Dispose();
        }
        
        #endregion     
    }
}