{% include 'service_preface.md' %}

## URL Format

```text
serverchan://{token}@serverchan
```

- `token`: The serverchan token.
- The host is always `serverchan`.

### Query Parameters

| Parameter | Description                                    |
| --------- | ---------------------------------------------- |
| `channel` | The messaging channel.                         |
| `openid`  | The openid of the user to send the message to. |
