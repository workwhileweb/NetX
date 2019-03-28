namespace Leaf.xNet.Services.Captcha
{
    // ReSharper disable once UnusedMember.Global
    /// <inheritdoc />
    public class TwoCaptchaSolver : RucaptchaSolver
    {
        public override string Host { get; } = "2captcha.com";
    }
}