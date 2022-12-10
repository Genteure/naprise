{% include 'service_preface.md' %}

## URL Format

```text
apprise://{user}:{password}@{host}:{port}/{token}
apprises://{user}:{password}@{host}:{port}/{token}
```

- `user` and `password`: Optional HTTP Basic Auth credentials.
- `host` and `port`: The address of the service.
- `token`: The `KEY` of the Apprise API server.

### Query Parameters

| Parameter | Description                                                 |
| --------- | ----------------------------------------------------------- |
| `tag`     | A tag to for Apprise API to filter target URL.              |
| `format`  | Force the format of the notification. Default is `markdown` |

## Setup Guide
