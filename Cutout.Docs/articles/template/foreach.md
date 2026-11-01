# Foreach Statement

`foreach` iterates any enumerable in the template using C# syntax, including
destructuring.

## Syntax

```c#
{% foreach item in items %}
  ...
{% end %}
```

```c#
{% foreach (item, index) in items.Select((x, i) => (x, i + 1)) %}
  ...
{% end %}
```

## Example

```c#
private const string Template = """
    {%- foreach (tag, index) in product.Tags.Select((tag, index) => (tag, index + 1)) -%}
    {{ index }}. {{ tag }}
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
