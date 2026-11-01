# While Statement

`while` repeats a block while a C# boolean expression is true. The condition is
evaluated in the generated code, so any expression that compiles is allowed.

## Syntax

```c#
{% while condition %}
  ...
{% end %}
```

## Example

```c#
private const string Template = """
    {% var i = 0 -%}
    {% while i < product.Tags.Length -%}
    {{ i + 1 }}. {{ product.Tags[i++] }}
    {% end %}
    """;

[Cutout.Template(Template)]
public static partial void Test(this StringBuilder builder, Product product);
```

With `product.Tags` set to `["awesome", "shoes", "cool"]` the builder contains,

```text
1. awesome
2. shoes
3. cool
```

The loop variable has to be declared with `var` inside the template; nothing
outside the template is visible to the condition.
