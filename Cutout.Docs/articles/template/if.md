# If/Else Statement

The `if` statement allows conditional rendering in templates.
The condition must be a valid C# boolean expression.

## Syntax

```c#
{% if condition %}
  ...
{% elseif otherCondition %}
  ...
{% else %}
  ...
{% end %}
```

## Example

```c#
{% if name == "Bob" %}
Hello Bob
{% else %}
Hello {{ name }}
{% end %}
```
