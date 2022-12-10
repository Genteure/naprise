{% include 'service_preface.md' %}

## URL Format

```text
discord://{webhookId}/{webhookToken}
```

### Query Parameters

| Parameter    | Description                                    |
| ------------ | ---------------------------------------------- |
| `username`   | The username to use for the message.           |
| `avatar_url` | The URL of the avatar to use for the message.  |
| `tts`        | Whether to use text-to-speech for the message. |

## Setup Guide

1. Open channel settings or server settings and go to **Integrations**.
2. Create a new webhook.
3. Copy the webhook URL.

The webhook URL will look like this:

```text
https://discord.com/api/webhooks/1234567890/abcdefghijklmnopqrstuvwxyz
```

`1234567890` is the webhook ID, and `abcdefghijklmnopqrstuvwxyz` is the webhook token, so the URL would be

```text
discord://1234567890/abcdefghijklmnopqrstuvwxyz
```

Or just replace the `https://discord.com/api/webhooks/` part with `discord://` and you're done.
