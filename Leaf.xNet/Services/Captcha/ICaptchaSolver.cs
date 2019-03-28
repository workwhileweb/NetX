using System;
using System.IO;
using System.Threading;

namespace Leaf.xNet.Services.Captcha
{
    public interface ICaptchaSolver
    {
        uint UploadRetries { get; set; }
        uint StatusRetries { get; set; }

        TimeSpan UploadDelayOnNoSlotAvailable { get; set; }
        TimeSpan StatusDelayOnNotReady { get; set; }
        TimeSpan BeforeStatusCheckingDelay { get; set; }
        
        CancellationToken CancelToken { get; set; }

        string SolveImage(string imageUrl);
        string SolveImage(byte[] imageBytes);
        string SolveImage(Stream imageStream);
        string SolveImageFromBase64(string imageBase64);

        string SolveRecaptcha(string pageUrl, string siteKey);
    }
}