{% include 'service_preface.md' %}

## URL Format

```text
onebot12://{access_token}@{host}:{port}/private/{user_id}
onebot12://{access_token}@{host}:{port}/group/{group_id}
onebot12://{access_token}@{host}:{port}/channel/{guild_id}/{channel_id}
onebot12s://{access_token}@{host}:{port}/private/{user_id}
onebot12s://{access_token}@{host}:{port}/group/{group_id}
onebot12s://{access_token}@{host}:{port}/channel/{guild_id}/{channel_id}
```

- `access_token`: Optional. The access token of your bot.
- `host` and `port`: The address of the service.
- `user_id`: The id of the user.
- `group_id`: The group number.
- `guild_id` and `channel_id`: The ids for the guild and channel.

### Query Parameters

_None_
