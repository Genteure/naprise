# Adding new service

A service is any class that:

- Implements `INotifier`
- Have a public constructor that takes a `ServiceConfig`
- Have a `NapriseNotificationService` attribute

Services can then be added to the `ServiceRegistry`

```csharp
Naprise.DefaultRegistry.Add<MyService>();
// or
Naprise.DefaultRegistry.Add(typeof(MyService));
```

```csharp
var registry = new ServiceRegistry().AddDefaultServices();

registry.Add<MyService>();
// or
registry.Add(typeof(MyService));
```

In addition, services provided by Naprise should:

- Be sealed
- Inherit from `NotificationService`
- Have a `NotificationServiceWebsite` attribute
- Have a `NotificationServiceApiDoc` attribute

If a service requires another library, consider creating a new project for it so that the dependency is not required for users don't use the service.

## Designing the service URL

The service URL is the way to configure a service.

Here are some guidelines for designing the URL:

- **IMPORTANT**: Host MUST NOT be used for passing tokens or api keys, as it is case insensitive, all uppercase characters will be converted to lowercase.
- Prefer using the service's full name as the URL scheme. For example, use `discord` instead of `dc`.
- For self-hostable services, add `s` to the scheme for requests over https/tls. For example, use `apprise` for calling API over http and `apprises` for calling API over https.
- Prefer using host and path for required arguments.
- Prefer using query parameters for optional arguments.

## Implementing the service

Steps to implement a service:

- Create a new file in `src/Naprise/Service`.
- Copy the content of [`src/Naprise/Service/Template.cs`](#template-for-implementing-new-service) to the new file.
- Rename the class name to the service name.
- Fill in all the attributes.
- Parse the URL in the constructor.
- Implement the `SendAsync` method.

Some considerations when implementing the constructor:

- Use `Flurl.Url` instead of `System.Uri` for parsing and building URLs.
- Throw `NapriseInvalidUrlException` if the URL is invalid, e.g. missing required arguments, or contains invalid arguments.
- Check the token format if applicable, but do not send network requests in the constructor.
- Store the parsed arguments in readonly fields.

Some considerations when implementing the SendAsync method:

- Use `Flurl.Url` for parsing and building URLs.
- If the service supports setting color, convert the message type to a color using `this.Asset.GetColor(type)`.
- If the service does not support setting color, prepend the message with the string returned by `this.Asset.GetAscii(type)`.
- If the service only support one message format (e.g. markdown), convert the message to the supported format using `message.Prefer<Format>Body()`.
- If the service supports multiple message formats, it's up to you to decide which format to use.
- Check the response and throw `NapriseNotifyFailedException` if the request failed.

## Adding tests

If you're adding a new service for Naprise:

Please also add tests for the new service in `src/Naprise.Tests/Service/`.
Please add test cases for all valid URLs.

You can optionally add tests for invalid URLs and tests for sending messages, see `DiscordTests.cs` for an example.

## Adding documentation

Please add documentation for the new service in `docs/docs/services/`.

Add link to the new page in `nav` section of `docs/mkdocs.yml`.

Generate README.md and the json file for documentation website by running

```
dotnet run --project src/Naprise.DocGenerator
```

## Template for implementing new service

- Check [GitHub](https://github.com/Genteure/naprise/blob/main/src/Naprise/Service/Template.cs) for latest version of this template.
- If you are not adding the service to Naprise, the namespace should also be changed.

```csharp
--8<-- "../src/Naprise/Service/Template.cs"
```
