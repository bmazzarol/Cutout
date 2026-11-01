# For Statement

`for` is a C# style `for` loop inside a template. The loop counter is a normal
C# variable and can be used in the body.

## Syntax

```c#
{% for i = 0; i < items.Count; i++ %}
  ...
{% end %}
```

## Example

```c#
private const string Template = """
    {%- for i = 0; i < product.Tags.Length; i++ -%}
    {{ i + 1 }}. {{ product.Tags[i] }}
    {% end -%}
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

The newline that ends each body line is kept, so every tag lands on its own line.
Use `{%- ... -%}` when the whitespace around a tag should be removed.
