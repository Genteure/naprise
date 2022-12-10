{% include 'service_preface.md' %}

## URL Format

```text
notica://{user}:{password}@{host}:{port}/{token}
noticas://{user}:{password}@{host}:{port}/{token}
```

- `user` and `password`: Optional HTTP Basic Auth credentials.
- `host` and `port`: The address of the service.
- `token`: The unique ID displayed on Notica.

### Query Parameters

_None_

## Setup Guide

For the hosted service, open [https://notica.us](https://notica.us) and allow notifications.
The URL of the page will change to something like `https://notica.us/?7WUU7N`.

The query parameter is the token, in this case `7WUU7N`.

So the URL would be:

```text
noticas://notica.us/7WUU7N
```

Note there is no `?` in the URL, the token is part of the path, not the query.
