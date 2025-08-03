# For Statement

The `for` statement allows you to write C#-style for loops in your template.

## Syntax

```c#
{% for i = 0; i < items.Count; i++ %}
  ...
{% end %}
```

## Example

```c#
{% for i = 0; i < items.Count; i++ %}
Item: {{ items[i] }}
{% end %}
```
