# Leaf.Net
### Beta Release 4.0.0

[![NuGet version](https://badge.fury.io/nu/Leaf.Net.svg)](https://badge.fury.io/nu/Leaf.Net)

**Leaf.Net** beta - provides Http(s), Socks4a, Socks4, Socks5, Chain proxy for .Net framework's HttpClient class

**Leaf.Net** it's a based on [Extreme.Net](https://github.com/Fedorus/Extreme.Net). And original library [xNet](https://github.com/X-rus/xNet).


# Installation
Install via NuGet will be in the future.

```
// PM > Install-Package Leaf.Net
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
