# Usage Examples

## Send to multiple services

```csharp
// You can also pass in any enumerable of string like List<string> or string[]
var notifier = Naprise.Create("discord://106943697/YGCTVYbXQ7_pTEv-f3jX3e", "telegram://123456789:ABC-DEF1234ghIkl-zyx57W2v1u123ew11");

await notifier.NotifyAsync(new Message
{
    Type = MessageType.Success,
    Title = "Hello from Naprise!",
    Markdown = "**This** is a _test_ message. :heart:",
});
```

## Adding a custom service

```csharp
Naprise.DefaultRegistry.Add<MyService>();
```

See [Adding new service](./new-service.md) for more details.

If you are implementing a new service, please consider submitting a pull request so everyone can benefit from your work, thanks! :slight_smile:

## Using `ServiceRegistry`

```csharp
var registry = new ServiceRegistry().AddDefaultServices();
var notifier = registry.Create("discord://106943697/YGCTVYbXQ7_pTEv-f3jX3e");
```

## Ignore invalid schemes and URLs

```csharp
Naprise.DefaultRegistry.IgnoreUnknownScheme = true;

// "invalid" is not a valid scheme
var notifier = Naprise.Create("invalid://anything");
Assert.Equal(Naprise.NoopNotifier, notifier);
```

```csharp
Naprise.DefaultRegistry.IgnoreInvalidUrl = true;

// "discord" expects a id and a token
var notifier = Naprise.Create("discord://missing-token");
Assert.Equal(Naprise.NoopNotifier, notifier);
```

## Customizing the `HttpClient`

Changing the `HttpClient` used by one registry:

```csharp
var registry = new ServiceRegistry().AddDefaultServices();
registry.HttpClient = new HttpClient();
// registry.HttpClient is null by default
// when it is null, Naprise.DefaultHttpClient is used
```

or globally:

```csharp
Naprise.DefaultHttpClient = new HttpClient();
```

New `HttpClient` instances are used even if it's set after the notifier is created.

```csharp
// Create a notifier first
var notifier = Naprise.Create("discord://106943697/YGCTVYbXQ7_pTEv-f3jX3e");

// Then change the default HttpClient
Naprise.DefaultHttpClient = new HttpClient();

// The notifier uses the new HttpClient
await notifier.NotifyAsync(new Message
{
    Type = MessageType.Success,
    Title = "Hello from Naprise!",
    Markdown = "**This** is a _test_ message. :heart:",
});
```

## Setting a custom user agent

```csharp
Naprise.DefaultHttpClient.DefaultRequestHeaders.Add("User-Agent", "MyGreatApp/0.1.0");
```

## Using proxies

```csharp
var handler = new HttpClientHandler
{
    Proxy = new WebProxy("http://127.0.0.1:8080"),
};

Naprise.DefaultHttpClient = new HttpClient(handler);
```

## Send to a selfhosted self-signed service

a.k.a how to disable SSL certificate validation.

```csharp
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
};

Naprise.DefaultHttpClient = new HttpClient(handler);

// Apprise API at https://selfhosted.example.com with a self-signed certificate
var notifier = Naprise.Create("apprises://selfhosted.example.com/apprise");
await notifier.NotifyAsync(new Message
{
    Type = MessageType.Success,
    Title = "Hello from Naprise!",
    Markdown = "**This** is a _test_ message. :heart:",
});
```
