{% include 'service_preface.md' %}

## URL Format

```text
notifyrun://{user}:{password}@{host}:{port}/{channel}
notifyruns://{user}:{password}@{host}:{port}/{channel}
```

- `user` and `password`: Optional HTTP Basic Auth credentials.
- `host` and `port`: The address of the service.
- `channel`: The notify.run notification channel.

### Query Parameters

_None_

## Setup Guide

For the hosted service, open [https://notify.run](https://notify.run) and click **Create a Channel**.
A channel id will be generated, for example `pCEVD6IkQiwQvIKtOLVn`.

The URL would be:

```text
notifyruns://notify.run/pCEVD6IkQiwQvIKtOLVn
```
