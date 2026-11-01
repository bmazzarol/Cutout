# If/Else Statement

`if` renders a block based on a C# boolean expression. `elseif` and `else` are
supported; Liquid's `unless` is not.

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
private const string Template = """
    {% if product == "Awesome Shoes" -%}
    These shoes are awesome!
    {%- else -%}
    These shoes are not awesome!
    {%- end -%}
    """;

[Cutout.Template(Template)]
public static partial void Test(this StringBuilder builder, string product);
```

With `product` set to `Awesome Shoes` the builder contains,

```text
These shoes are awesome!
```

With `product` set to `Cool Shoes` it contains,

```text
These shoes are not awesome!
```

The condition is compiled straight into an `if`, so there is no evaluation cost
at runtime.
