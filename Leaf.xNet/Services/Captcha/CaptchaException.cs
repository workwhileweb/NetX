using System;

namespace Leaf.xNet.Services.Captcha
{
    public enum CaptchaError
    {
        Unknown,
        CustomMessage,
        InvalidApiKey,
        EmptyResponse
    }

    //[Serializable]
    public class CaptchaException : Exception
    {
        public readonly CaptchaError Error;

        public CaptchaException(string message) : base(message)
        {
            Error = CaptchaError.CustomMessage;
        }

        public CaptchaException(CaptchaError error)
        {
            Error = error;
        }
    }
}