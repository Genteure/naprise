{% include 'service_preface.md' %}

## URL Format

```text
ntfy://{user}:{password}@{host}:{port}/{topic}
ntfys://{user}:{password}@{host}:{port}/{topic}
```

- `user` and `password`: Optional HTTP Basic Auth credentials.
- `host` and `port`: The address of the service.
- `topic`: The ntfy notification topic.

### Query Parameters

| Parameter  | Description                                     |
| ---------- | ----------------------------------------------- |
| `tags`     | A comma-separated list of tags.                 |
| `priority` | The priority of the notification as a number.   |
| `click`    | A URL to open when the notification is clicked. |
| `delay`    | The delay before the notification is sent.      |
| `email`    | The email address to send the notification to.  |


## Setup Guide

Just use any string as the `topic` and you're good to go, no setup required.

Example:

```text
ntfys://ntfy.sh/test
```
