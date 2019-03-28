namespace Leaf.xNet.Services.Captcha
{
    public class CapmonsterSolver : RucaptchaSolver
    {
        public override string Host { get; }

        public CapmonsterSolver(string host = "127.0.0.3:80")
        {
            Host = host;
        }
    }
}