{% include 'service_preface.md' %}

## URL Format

```text
pushdeer://{user}:{pass}@{host}:{port}/{pushkey}
pushdeers://{user}:{pass}@{host}:{port}/{pushkey}
```

- `user` and `password`: Optional HTTP Basic Auth credentials.
- `host` and `port`: The address of the service.
- `pushkey`: The PushKey.

### Query Parameters

_None_

## Setup Guide

Create a Key in the PushDeer app, it should look similar to `PDUBj4fkoihKi93dLfC7PXDzuHUVN4NSq`.

The hosted service's API domain is `api2.pushdeer.com`, so the URL would be:

```text
pushdeers://api2.pushdeer.com/PDUBj4fkoihKi93dLfC7PXDzuHUVN4NSq
```
