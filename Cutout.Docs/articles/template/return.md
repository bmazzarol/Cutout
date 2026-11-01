# Return Statement

`return` stops the template method at that point. Nothing written after it is
emitted, so it is the way to bail out of a template when the input is not
usable.

## Syntax

```c#
{% return %}
```

## Example

```c#
private const string Template = """
    {% if product.Tags.Length == 0 -%}
    {%- return -%}
    {%- end -%}
    {{ product.Title }} has {{ product.Tags.Length }} tags
    """;

[Cutout.Template(Template)]
public static partial void Test(this StringBuilder builder, Product product);
```

With `product.Tags` empty the builder is empty. With `product.Tags` set to
`["awesome", "shoes"]` and `product.Title` set to `Awesome Shoes` the builder
contains,

```text
Awesome Shoes has 2 tags
```

The generated code returns from the method, so the guard is checked by the
compiler generated code rather than at runtime by a template engine.
