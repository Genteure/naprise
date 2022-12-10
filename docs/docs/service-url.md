# Service URL

The Naprise service URL is the way to configure a service. They have the following format:

```
service://configuration/?query=values
```

The [Services](services/index.md) page have a list of all supported services and their URL format.

Some service also have a version of scheme with `s` added to the end, which is used for https/tls. For example, `apprise` is used for requests over http and `apprises` is used for requests over https.

## Compared to Apprise

Naprise's service URL are **not compatible** with [Apprise](https://github.com/caronc/apprise)'s URL.

There are no global parameters in Naprise.

Naprise always send the message as is to the service, equivalent to Apprise's `overflow=upstream`.

Instead of letting the user specify the message format, each service in Naprise decides what format to use.
You can provide message in multiple formats, each service will choose the format it supports, converting the message format as needed.

If you want to send to a selfhosted self-signed service, instead of adding `verify=no` to the URL, you can create a new `HttpClientHandlr` with the `ServerCertificateCustomValidationCallback` property set to always return `true`, create a new `HttpClient` with the handler then assign it to `Naprise.DefaultHttpClient`. See [Usage Examples](./usage-examples.md) for more details.

Instead of using `cto` and `rto` to specify request timeouts, you can set the `Timeout` property of `Naprise.DefaultHttpClient`, or pass a `CancelationToken` to the `NotifyAsync` method.
