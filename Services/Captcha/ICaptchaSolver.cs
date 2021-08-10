using System;
using System.IO;
using System.Threading;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global

namespace Tuan.Net.X.Services.Captcha
{
    public interface ICaptchaSolver
    {
        uint UploadRetries { get; set; }
        uint StatusRetries { get; set; }

        TimeSpan UploadDelayOnNoSlotAvailable { get; set; }
        TimeSpan StatusDelayOnNotReady { get; set; }
        TimeSpan BeforeStatusCheckingDelay { get; set; }
        
        string SolveImage(string imageUrl, CancellationToken cancelToken = default);
        string SolveImage(byte[] imageBytes, CancellationToken cancelToken = default);
        string SolveImage(Stream imageStream, CancellationToken cancelToken = default);
        string SolveImageFromBase64(string imageBase64, CancellationToken cancelToken = default);

        string SolveRecaptcha(string pageUrl, string siteKey, CancellationToken cancelToken = default);
    }
}