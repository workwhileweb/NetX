# Extreme.Net
## beta

[![NuGet version](https://badge.fury.io/nu/Extreme.Net.svg)](https://badge.fury.io/nu/Extreme.Net)
[![Build status](https://ci.appveyor.com/api/projects/status/7mwsovabbtwq6i65?svg=true)](https://ci.appveyor.com/project/extremecodetv/extreme-net)

**Extreme.Net** beta - provides Http(s), Socks4a, Socks4, Socks5, Chain proxy to HttpClient

**Extreme.Net** it's a fork of [xNet](https://github.com/X-rus/xNet)


# Installation
 
Install via NuGet
 
```
PM > Install-Package Extreme.Net
```
 
# Examples
 
```csharp
    var socksProxy = new Socks5ProxyClient("77.109.184.55", 62810);

    var handler = new ProxyHandler(socksProxy);
    var client = new HttpClient(handler);

    var request = new HttpRequestMessage();
	request.Method = HttpMethod.Post;

    var parameters = new Dictionary<string, string> { { "param1", "1" }, { "param2", "2" } };
    var encodedContent = new FormUrlEncodedContent(parameters);

    var response = await client.PostAsync("http://httpbin.org/post", encodedContent);
    var content  = await response.Content.ReadAsStringAsync();

``` 


#Developer

Artem Dontsov

![VK](https://vk.com/images/faviconnew.ico?3) [VK](https://vk.com/extremecodetv)

![YouTube](https://s.ytimg.com/yts/img/favicon-vflz7uhzw.ico) [YouTube](https://www.youtube.com/channel/UCBNlINWfd08qgDkUTaUY4_w)