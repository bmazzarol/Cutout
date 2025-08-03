# Continue Statement

The `continue` statement can be used inside loops to skip to the next iteration
, just like in C#.

## Syntax

```c#
{% continue %}
```

## Example

```c#
{% for i = 0; i < 10; i++ %}
  {% if i % 2 == 0 %}
    {% continue %}
  {% end %}
  Odd: {{ i }}
{% end %}
```
