![Cutout](https://raw.githubusercontent.com/bmazzarol/Cutout/main/scissors-small-icon.png)

# Cutout

**Templating that costs nothing at runtime.**

A source generator that renders Liquid style templates into a StringBuilder at compile time, so generated code keeps its indentation without a template engine dependency.

[![Nuget](https://img.shields.io/nuget/v/Cutout)](https://www.nuget.org/packages/Cutout/)

```shell
dotnet add package Cutout
```

[Getting started](https://bmazzarol.github.io/Cutout/articles/getting-started.html) - [Full documentation](https://bmazzarol.github.io/Cutout/)

## Why?

A source generator has to build the generated code as a string, and keeping the
indentation right by hand is tedious. A template engine reads better, but a
runtime engine costs performance that generated code cannot afford.

Cutout renders the template at compile time. The generated code is plain
`StringBuilder` appends, so nothing is left in the binary except string
concatenation.

## First example

```csharp
using Cutout;

public static partial class Templates
{
    private const string Template = """
        {% if name == "Bob" %}
        Hello Bob
        {% else %}
        Hello {{ name }}
        {% end %}
        """;

    [Cutout.Template(Template)]
    public static partial void Greet(StringBuilder sb, string name);
}
```

The first parameter is the `StringBuilder` like type to write to. Every other
parameter is available in the template.

## Template language

Everything outside `{{ ... }}` and `{% ... %}` is a literal. Inside them any
valid C# expression works, because the template is compiled to C#. If something
is wrong, the compiler tells you.

Supported blocks:

- `if` / `elseif` / `else`
- `for`, `foreach`, `while`
- `call`, which invokes another template marked with `[Cutout.Template]`
- `var`, `continue`, `break` and `return`

Liquid whitespace control with `-` is supported, for example `{{- name }}`.

Three deviations from Liquid:

- every block closes with `{% end %}`, there is no `{% endif %}`
- there are no Liquid filters
- expressions are C#, not Liquid

## What it does not do

- It is not a runtime template engine. A template has to be a compile time
  constant in the same compilation unit that uses it.
- It is not full Liquid.

## See also

- [Getting started](https://bmazzarol.github.io/Cutout/getting-started.html)
- [Template syntax](https://bmazzarol.github.io/Cutout/articles/template/syntax.html)
- [Analyser rules](https://bmazzarol.github.io/Cutout/articles/rules/CUTOUT001.html)

## Attributions

[Icons created by juicy_fish](https://www.flaticon.com/free-icons)
