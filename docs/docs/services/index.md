# Services

| Service | Schemes | Service's homepage |
| ------- | ------- | ------- |
{% for service in services -%}
| [{{ service.name }}](./{{ service.id }}.md) | {{ service.schemes | map('format_scheme') | join('<br>') }} | [{{ service.website }}]({{ service.website }}) |
{% endfor -%}
