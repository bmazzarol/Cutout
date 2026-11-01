# Var Statement

`var` declares a local variable in the generated method. It is ordinary C#, so
any expression that compiles is allowed.

## Syntax

```c#
{% var name = expression %}
```

## Example

```c#
private const string Template = """
    {% var i = 0 -%}
    {% while i < 3 -%}
    {{ i++ }}
    {% end -%}
    """;

[Cutout.Template(Template)]
public static partial void Test(this StringBuilder builder);
```

The builder contains,

```text
0
1
2
```

Variables declared with `var` are local to the template they are declared in. A
template method cannot read a variable from another template method; pass values
as parameters instead.
