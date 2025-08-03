# Foreach Statement

The `foreach` statement allows you to iterate over collections in your template
using C# syntax.

## Syntax

```c#
{% foreach item in items %}
  ...
{% end %}
```

## Example

```c#
{% foreach item in items %}
Item: {{ item }}
{% end %}
```
