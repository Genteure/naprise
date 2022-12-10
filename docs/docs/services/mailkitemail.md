{% include 'service_preface.md' %}

This service is in a separate nuget package: [Naprise.Service.MailKit](https://www.nuget.org/packages/Naprise.Service.MailKit/).  
It uses [MailKit](https://www.nuget.org/packages/MailKit/) to send emails.

```powershell
dotnet add package Naprise.Service.MailKit
# or
Install-Package Naprise.Service.MailKit
```

You will need to register the service in ServiceRegistry before using it.

```csharp
Naprise.DefaultRegistry.AddMailKit();
```

## `email` URL Format

As a quick way to send emails from your personal email account to yourself, you can use the `email` URL format.

```text
email://{user}:{pass}@{domain}
```

- `user`: The name of your email account.
- `pass`: The password or application-secret of your email account.
- `domain`: The domain of your email account.

Examples:

```text
email://john:myS3cr5t@gmail.com
email://aoi:somePassword@yahoo.co.jp
email://123456:furageoupjhjygto@qq.com
```

See [the table below](#supported-domains) for a list of supported domains.


## `smtp` and `smtps` URL Format

If your email service is not supported by `email` URL, or you want to send emails from a custom email server, you can use the `smtp` and `smtps` URL formats.

```text
smtp://{smtp_host}:{smtp_port}/{from}/{to}
smtps://{smtp_host}:{smtp_port}/{from}/{to}
smtp://{smtp_host}:{smtp_port}/{username}/{password}/{from}/{to}
smtps://{smtp_host}:{smtp_port}/{username}/{password}/{from}/{to}
```

* `smtp://`: Connect to the SMTP server using unencrypted plain text or `STARTTLS`.
* `smtps://`: Connect to the SMTP server using encrypted `SSL/TLS`.

??? info "**TLDR**: Use `smtp://` for port `25` and `587`, and `smtps://` for port `465`."
    Some email providers' documentation refer `STARTTLS` as just `TLS`, which technically is not the same thing.  

    `SSL` now days in the context of SMTP often means `TLS 1.2` is being used, even if it's being called `SSL` and not `TLS`. The client will establish a SSL/TLS connection first just like HTTPS, and then start the SMTP protocol. The standard port for SMTP over SSL/TLS is `465`.  
    
    `STARTTLS` on the other hand, is a SMTP command that tells the SMTP server to switch to TLS encryption after the connection is established. The client will establish a plain text connection first, and then send the `STARTTLS` command to the SMTP server, if both parties support it, the SMTP server will switch to TLS encryption. The most common port for SMTP with `STARTTLS` support is `587`.

- `smtp_host`: The SMTP host of your email server.
- `smtp_port`: The SMTP port of your email server, **always required**.
- `username`: The username of your email account.
- `password`: The password or application-secret of your email account.
- `from`: The email address of the sender.
- `to`: The email address of the recipient.

If the `username` and `password` are not provided, it's assumed that the SMTP server does not require authentication, which might be the case for self-hosted local SMTP servers.

Examples:

```text
smtps://smtp.gmail.com:465/jone@gmail.com/JoneDoe1234/jone@gmail.com/friend@outlook.com
smtp://smtp.local:25/homeserver/password/homeserver@local/admin@local
```

## Supported Domains

The following table lists the supported domains for `email` URLs and their corresponding SMTP settings.
If your domain is not listed here, you can still use the `smtp` or `smtps` URL formats to send emails.

| Domain        | Name  | SMTP Server          | Encryption |
| ------------- | ----- | -------------------- | ---------- |
{% for platform in email_platforms -%}
{% for domain in platform.domains -%}
| `{{ domain }}` | {{ platform.name }} | `{{ platform.host }}:{{ platform.port }}` | `{{ 'SSL/TLS' if platform.useSsl else 'NONE/STARTTLS' }}` |
{% endfor -%}{% endfor %}
