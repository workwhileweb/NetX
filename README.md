# SharpNet
### beta

[![NuGet version](https://badge.fury.io/nu/SharpNet.svg)](https://badge.fury.io/nu/SharpNet)

**SharpNet** beta - provides Http(s), Socks4a, Socks4, Socks5, Chain proxy for .Net framework's HttpClient class

**SharpNet** it's a fork of [xNet](https://github.com/X-rus/xNet)


# Installation
 
Install via NuGet
 
```
PM > Install-Package SharpNet
```
 
# Examples
 
```csharp
    var socksProxy = new Socks5ProxyClient("127.0.0.1", 3128);

    var handler = new ProxyHandler(socksProxy);
    var client = new HttpClient(handler);

    var request = new HttpRequestMessage();
	request.Method = HttpMethod.Post;

    var parameters = new Dictionary<string, string> { { "param1", "1" }, { "param2", "2" } };
    var encodedContent = new FormUrlEncodedContent(parameters);

    var response = await client.PostAsync("http://httpbin.org/post", encodedContent);
    var content  = await response.Content.ReadAsStringAsync();
``` 