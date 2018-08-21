[![NuGet version](https://badge.fury.io/nu/Leaf.xNet.svg)](https://badge.fury.io/nu/Leaf.xNet) [![Build status](https://ci.appveyor.com/api/projects/status/em4aje36etb63kdt/branch/master?svg=true)](https://ci.appveyor.com/project/grandsilence/leaf-xnet/branch/master)

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
[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=FZLZ5ED65HVCL)

**Yandex** Money / **Debert Card**: [410011037924983](https://money.yandex.com/to/410011037924983)  
Webmoney **WMR**: R246626749259  
Webmoney **WMZ**: Z349403749504  
Webmoney **WMU**: U313788999957  
Webmoney **WME**: E894184114651  
Webmoney **WMX**: X428336365219 / 138jva7sKAVat8s8XSyqkFBkbUc7dSyXj2  
  
Bitcoin **BTC**: 3AYA9xsGGZcbnX63i5bwfGkvXNDxC7r7Uo  
Bitcoin Gold **BTG**: AaXkAfose8ArsZHoWuC2bYZhMfkdutNG8S  
Bitcoin Cash **BCH**: 3N2wxyixecHtUrqJgRrdKs4pu7oPzMFtpm  
LiteCoin **LTC**: MK5Dsyo6ek9z1CN19oNRhTrK2VrvNWq654  


# Features
## Middle response headers (when redirected)
```csharp
var req = new HttpRequest();
// REQUIRED!
req.EnableMiddleHeaders = true;

// This requrest has a lot of redirects
var resp = req.Get("https://account.sonyentertainmentnetwork.com/");
var md = resp.MiddleHeaders;
```
