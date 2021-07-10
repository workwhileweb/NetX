namespace Tuan.Net.X.Services.Captcha
{
    /// <inheritdoc />
    // ReSharper disable once UnusedType.Global
    public class TwoCaptchaSolver : RucaptchaSolver
    {
        public TwoCaptchaSolver()
        {
            Host = "2captcha.com";
        }
    }
}