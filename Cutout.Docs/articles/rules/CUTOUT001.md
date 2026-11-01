# CUTOUT001 - Invalid Template

The template could not be parsed. The diagnostic is reported on the template
method and reads,

```text
CUTOUT001: Failed to parse template for Templates.Test: <parser message>
```

Common causes are a block without an `end`, an expression inside an interpolation
hole that does not compile, or a Liquid feature that Cutout does not implement,
such as filters.

```c#
[Cutout.Template("""
    {% if product.Title %}
    {{ product.Title }}
    """)]
public static partial void Test(this StringBuilder builder, Product product);
```

The `if` block above is never closed, so the generator reports CUTOUT001 instead
of generating code. Fix the template; see [Template Syntax](../template/syntax.md)
for the supported statements.
