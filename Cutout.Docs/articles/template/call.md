# Call Statement

The `call` statement allows you to invoke other template methods decorated
with `[Cutout.Template]`.

## Syntax

```c#
{% call MethodName(arg1, arg2) %}
```

## Example

```c#
{% call RenderHeader("Title") %}
```
