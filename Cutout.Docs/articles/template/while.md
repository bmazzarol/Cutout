# While Statement

The `while` statement allows you to write C#-style while loops in your template.

## Syntax

```c#
{% while condition %}
  ...
{% end %}
```

## Example

```c#
{% while i < 10 %}
Value: {{ i }}
{% end %}
```
