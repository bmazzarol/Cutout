# Break Statement

The `break` statement can be used inside loops to exit the loop early,
just like in C#.

## Syntax

```c#
{% break %}
```

## Example

```c#
{% for i = 0; i < 10; i++ %}
  {% if i == 5 %}
    {% break %}
  {% end %}
  Value: {{ i }}
{% end %}
```
