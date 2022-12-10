{% include 'service_preface.md' %}

## URL Format

```text
telegram://{token}@{chat_id}
```

- `token`: The telegram bot token.
- `chat_id`: The chat id to send the message to.

If `chat_id` is a number (group id or user id) it will be sent to telegram api as is, otherwise it will be prefixed with `@` before sent to telegram api.

### Query Parameters

| Parameter                  | Description                                                                                       |
| -------------------------- | ------------------------------------------------------------------------------------------------- |
| `api_host`                 | The telegram api host, default is `https://api.telegram.org`.                                     |
| `parse_mode`               | Mode for parsing entities in the message text, default is none.                                   |
| `message_thread_id`        | Unique identifier for the target message thread (topic) of the forum; for forum supergroups only. |
| `disable_web_page_preview` | Disables link previews for links in this message.                                                 |
| `disable_notification`     | Sends the message silently. Users will receive a notification with no sound.                      |
| `protect_content`          | Protects the contents of the sent message from forwarding and saving.                             |

## Setup Guide

### Create a Telegram Bot

1. Open [BotFather](https://t.me/botfather) in Telegram.
2. Send `/newbot` to create a new bot.
3. Enter the bot name and username.
4. Copy the bot token.

### Get the Chat ID

For private chat, send a message to the bot. For groups, add the bot to the group.

Open the following link in your browser:

```
https://api.telegram.org/bot{token}/getUpdates
```

Replace `{token}` with your bot token.

The `id` field under `chat` is the chat id.

### Example URLs

Direct message (private chat):

```
telegram://123456789:ABC-DEF1234ghIkl-zyx57W2v1u123ew11@123456789
```

Group:

```
telegram://123456789:ABC-DEF1234ghIkl-zyx57W2v1u123ew11@-123456789
```

Channel:

```
telegram://123456789:ABC-DEF1234ghIkl-zyx57W2v1u123ew11@channel_name
```
