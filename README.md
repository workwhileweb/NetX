[![NuGet version](https://badge.fury.io/nu/Leaf.xNet.svg)](https://badge.fury.io/nu/Leaf.xNet) [![Build status](https://ci.appveyor.com/api/projects/status/em4aje36etb63kdt/branch/master?svg=true)](https://ci.appveyor.com/project/grandsilence/leaf-xnet/branch/master) [![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=FZLZ5ED65HVCL)


# Leaf.xNet
**Leaf.xNet** - provides HTTP/HTTPS, Socks 4A, Socks 4, Socks 5

**Leaf.xNet** it's a based on [Extreme.Net](https://github.com/Fedorus/Extreme.Net). And original library [xNet](https://github.com/X-rus/xNet).

Usage same like original xNet.

# Installation

### NuGet
```
PM > Install-Package Leaf.xNet
```

### .NET CLI
```
dotnet add package Leaf.xNet
```

### Paket CLI
```
paket add Leaf.xNet
```

# Donate
If this project help you reduce time to develop, you can give me a cup of coffee :)


[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=FZLZ5ED65HVCL)

**Yandex** Money / **Debert Card**: [410011037924983](https://money.yandex.com/to/410011037924983)  
Webmoney **WMR**: `R246626749259`  
Webmoney **WMZ**: `Z349403749504`  
Webmoney **WMU**: `U313788999957`  
Webmoney **WME**: `E894184114651`  
Webmoney **WMX**: `X428336365219`

Bitcoin **BTC**: `36uHKL713c1FmmpWB89MkbLEeCgbsfsGc5`  
Bitcoin Gold **BTG**: `Abf3jmLwiYw6ewuwMgu4AeHw4a8WVZUySH`  
LiteCoin **LTC**: `M8rkfHAB62NyvAPkaZUG4GeQB5DPvts4xD`  
LiteCoin **LTC** (alternate): `32ecMPkD8uXZ7f7rUgUvEdPzrNcx21J5po`  


# Features
## CloudFlare bypass inside
```csharp
using Leaf.xNet.Services.Cloudflare;

// Check and pass CloudFlare JS Challange if it's present
// Attention: It's working when Re-Captcha enabled
var httpRequest = new HttpRequest();
var clearResp = httpRequest.GetThroughCloudflare("https://...");

// Check only (without solution)
var resp = httpRequest.Get("https://...");
bool isCloudFlared = resp.isCloudFlared();
```

## Middle response headers (when redirected)
```csharp
httpRequest.EnableMiddleHeaders = true;

// This requrest has a lot of redirects
var resp = httpRequest.Get("https://account.sonyentertainmentnetwork.com/");
var md = resp.MiddleHeaders;
```

## Cookies by default
Cookies enabled by default. If you wait to disable parsing it use:
```csharp
HttpRequest.UseCookies = false;
```

## Modern User-Agent Randomization
```csharp
httpRequest.UserAgentRandomize();
// Call it again if you want change it again
```

# How to
## Send multipart requests with fields and files
Methods `AddField()` and `AddFile()` has been removed (unstable).
Use this code:
```csharp
using (var request = new HttpRequest())
{
    var multipartContent = new MultipartContent()
    {
        {new StringContent("Harry Potter"), "login"},
        {new StringContent("Crucio"), "password"},
        {new FileContent(@"C:\hp.rar"), "file1", "hp.rar"}
    };

    // When response isn't required
    request.Post("https://google.com", multipartContent).None();

    // Or
    // var resp = request.Post("https://google.com", multipartContent);
    // And then read as string
    // string respStr = resp.ToString();
}
```

## Get page source (response body) and find a value between strings
```csharp
// Add in the beginning 
using Leaf.xNet.Extensions;

// Add in your method
// Don't forget about Dispose HttpRequest (use using statement or call r.Dispose())
var r = new HttpRequest();
string html = r.Get("https://google.com").ToString();
string title = html.Substring("<title>", "</title>");
```

## Download a file
```csharp
var request = new HttpRequest();
var resp = request.Get("http://google.com/file.zip");
// Do you checks here
request.ToFile("C:\\myDownloadedFile.zip");
```

## Get Cookies
```csharp
var req = new HttpRequest();
string response = req.Get("https://twitter.com/login").ToString();
var cookies = req.Cookies.GetCookies("https://twitter.com");
foreach (Cookie cookie in cookies) {
    // concat your string or do what you want
    Console.WriteLine($"{cookie.Name}: {cookie.Value}");
}
```

## Add a Cookie to HttpRequest.Cookies storage
```csharp
var req = new HttpRequest();
req.Cookies.Set(string name, string value, string domain, string path = "/");
// or
var cookie = new Cookie(string name, string value, string domain, string path);
req.Cookies.Set(cookie);
```