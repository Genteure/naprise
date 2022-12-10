{% include 'service_preface.md' %}

## URL Format

```text
gotify://{host}:{port}/{apptoken}
gotifys://{host}:{port}/{apptoken}
```

- `host` and `port`: The address of the service.
- `apptoken`: The Gotify application token.

### Query Parameters

| Parameter   | Description                                  |
| ----------- | -------------------------------------------- |
| `priority`  | The priority of the message as a number.     |
| `click_url` | The URL to open when the message is clicked. |
