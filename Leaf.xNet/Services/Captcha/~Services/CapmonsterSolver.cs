namespace Leaf.xNet.Services.Captcha
{
    /// <inheritdoc />
    // ReSharper disable once UnusedType.Global
    public class CapmonsterSolver : RucaptchaSolver
    {
        public CapmonsterSolver(string host = "127.0.0.3:80")
        {
            Host = host;
            IsApiKeyRequired = false;
        }
    }
}