# Naprise

**Naprise** is a .NET library that allows you to easily send notifications to popular messaging services like Telegram, Discord, and more.

Naprise is heavily inspired by [Apprise](https://github.com/caronc/apprise).

![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-brightgreen)
![License: MIT](https://img.shields.io/github/license/genteure/naprise)
[![Nuget Version](https://img.shields.io/nuget/v/naprise)](https://www.nuget.org/packages/Naprise)
[![Nuget Downloads](https://img.shields.io/nuget/dt/naprise)](https://www.nuget.org/packages/Naprise)

## Quick Start

Link to NuGet: [https://www.nuget.org/packages/Naprise](https://www.nuget.org/packages/Naprise)

```powershell
dotnet add package Naprise
# or
Install-Package Naprise
```

```csharp
var notifier = Naprise.Create("discord://106943697/YGCTVYbXQ7_pTEv-f3jX3e");

await notifier.NotifyAsync(new Message
{
    Type = MessageType.Success,
    Title = "Hello from Naprise!",
    Markdown = "**This** is a _test_ message. :heart:",
});
```
