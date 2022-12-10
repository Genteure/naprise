{% include 'service_preface.md' %}

## URL Format

```text
bark://{host}/{token}
bark://{host}:{port}/{token}
barks://{host}/{token}
barks://{host}:{port}/{token}
```

### Query Parameters

| Parameter | Description                                      |
| --------- | ------------------------------------------------ |
| `url`     | The URL to open when the notification is tapped. |
| `group`   | The group of the notification.                   |
| `icon`    | The icon of the notification.                    |
| `level`   | The level of the notification.                   |
| `sound`   | The sound of the notification.                   |

## Setup Guide

After installing Bark, you can find your push URL in the app. It should look like:

```text
https://api.day.app/kgZxA3pkswQpZ67J
```

`api.day.app` is the host, and `kgZxA3pkswQpZ67J` is the token, so the URL would be

```
barks://api.day.app/kgZxA3pkswQpZ67J
```
