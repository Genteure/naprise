{%- set service = services | selectattr("id", "equalto", page.file.name) | first -%}
# {{ service.name }}

- Schemes: {{ service.schemes | map('format_scheme') | join(', ') }}
- Format: {{ service.format }}
- Homepage: [{{ service.website }}]({{ service.website }})
- API Documentation: [{{ service.doc }}]({{ service.doc }})

_List of all supported services: [Services](./index.md)_
