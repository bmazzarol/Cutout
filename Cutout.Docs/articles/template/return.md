# Return Statement

The `return` statement can be used to exit from a template method early,
similar to C#.

## Syntax

```c#
{% return %}
```

## Example

```c#
{% if shouldExit %}
  {% return %}
{% end %}
Continue rendering...
```
