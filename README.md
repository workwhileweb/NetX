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
  
Bitcoin **BTC**: `3AYA9xsGGZcbnX63i5bwfGkvXNDxC7r7Uo`  
Bitcoin Gold **BTG**: `AaXkAfose8ArsZHoWuC2bYZhMfkdutNG8S`  
Bitcoin Cash **BCH**: `3N2wxyixecHtUrqJgRrdKs4pu7oPzMFtpm`  
LiteCoin **LTC**: `MK5Dsyo6ek9z1CN19oNRhTrK2VrvNWq654`  


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
HttpRequest.DontTrackCookies = true;
```

## Modern User-Agent Randomization
```csharp
httpRequest.UserAgentRandomize();
// Call it again if you want change it again
```
