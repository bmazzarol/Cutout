# Break And Continue

`break` exits a loop and `continue` skips to the next iteration, exactly as in
C#. Both are only valid inside `for`, `foreach` and `while` blocks.

## Syntax

```c#
{% break %}
{% continue %}
```

## Example

```c#
private const string Template = """
    Continue and break can also be used
    {%- for i = 0; i < product.Tags.Length; i++ -%}
    {%- if product.Tags[i] == "awesome" -%}
    {%- continue -%}
    {%- end -%}
    {%- if product.Tags[i] == "shoes" -%}
    {%- break -%}
    {%- end -%}
    {{i + 1}}. {{ product.Tags[i] }}
    {%- end -%}
    """;

[Cutout.Template(Template)]
public static partial void Test(this StringBuilder builder, Product product);
```

With `product.Tags` set to `["awesome", "shoes", "cool"]` the builder contains,

```text
Continue and break can also be used
```

`awesome` is skipped by `continue`, `shoes` breaks the loop, and `cool` is never
reached.
