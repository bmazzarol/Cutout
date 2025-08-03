<!-- markdownlint-disable MD013 -->

# ![Cutout](https://raw.githubusercontent.com/bmazzarol/Cutout/main/scissors-small-icon.png) Cutout

<!-- markdownlint-enable MD013 -->

[![Nuget](https://img.shields.io/nuget/v/cutout)](https://www.nuget.org/packages/cutout/)

When building source generators there is a requirement to generate source code
as a string that has indentation managed correctly. The code also needs to be
as fast as possible to not impact users in large repositories.

The recommended approach is to use
the [IndentedTextWriter](https://learn.microsoft.com/en-us/dotnet/api/system.codedom.compiler.indentedtextwriter?view=net-9.0)
. This class is simple to use, but is low level.

A standard template engine is a better approach from a code maintenance
perspective, but the performance is not as good.

This aims to provide the best of both worlds.

It's a source generator, so can be used in other source generators without
incurring a dependency on a template engine, and it lets you use a simplified
version of [liquid](https://shopify.github.io/liquid/) and any type that
implements the basic StringBuilder API.

## How to use

Create a static method with the `[Cutout.Template]` attribute.

The first parameter is the `StringBuilder` like type to write to.
Everything else passed can be used in the template.

```csharp
using Cutout;
    
public static partial class MyTemplate
{
    private const string Template = """
        {% if name == "Bob" %}
        Hello Bob
        {% else %}
        Hello {{ name }}
        {% end %}
        """;
    
    [Cutout.Template(Template)] 
    public static partial void MyTemplateMethod(StringBuilder sb, string name);
}
```

## Template Language

Everything that is not between `{{` or `{@` and `}}` or `%}` is treated as a
string literal.

Any valid C# expression can be used in the template as it is compiled to C#
code. So if something is not working, the compiler will tell you.

It supports the whitespace control characters `-` the same as liquid.

One deviation from liquid is that there is only one end keyword, `{% end %}`.

The following keywords are supported,

### If/Else

The `if` keyword is used to conditionally render a block of code.
Everything that is a valid C# boolean expression can be used.

For example,

```liquid
{% if true %}
Some that is true
{% elseif false %}
Some other thing
{% else %}
The default
{% end %}
```

### For/Each/While

The standard looping keywords are supported.
They align to the same keywords in C#.

```liquid
{% for i = 0; i < items.Count; i++ %}
{{ i }}
{% end %}

{% foreach item in items %}
{{ item }}
{% end %}

{% while true %}
{% end %}
```

`var`, `continue`, `break` and `return` are also supported.

### Functions

Any other methods using the `[Cutout.Template]` attribute can be called.

This allows for building up complex templates from smaller ones.

The syntax is like so,

```liquid
{% call MyFunction(1, 2, 3) %}
```
