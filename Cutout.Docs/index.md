# Cutout

![Cutout](images/scissors-icon.png)

**Templating that costs nothing at runtime.**

A source generator that renders Liquid style templates into a StringBuilder at compile time, so generated code keeps its indentation without a template engine dependency.

```shell
dotnet add package Cutout
```

## Why?

A source generator has to emit source code as a string, and that string has to
keep its indentation. The two usual answers each cost something:

* `IndentedTextWriter` is fast, but the template becomes a wall of `WriteLine`
  calls
* a template engine reads well, but it runs at runtime, and every generated file
  pays for it

Cutout renders the template at compile time. The template stays readable and the
generated code is a straight line of `builder.Append` calls.

This template,

```csharp
[Cutout.Template("{% if product == \"Awesome Shoes\" %} These shoes are awesome! {% else %} These shoes are not awesome! {% end %}")]
public static partial void Test(this StringBuilder builder, string product);
```

produces this method,

```csharp
internal static partial void Test(this StringBuilder builder, string product)
{
    if ( product == "Awesome Shoes" )
    {
        builder.Append(@" These shoes are awesome! ");
    }
    else
    {
        builder.Append(@" These shoes are not awesome! ");
    }
}
```

There is no template engine at runtime; the code above is what runs.

## What it does not do

* It is not a runtime template engine. Rendering happens at compile time, so a
  template cannot be read from a file or built at runtime
* Templates must be compile-time constants: a `const` string or a literal in the
  attribute
* It is not full Liquid. Every block closes with `{% end %}`, there are no
  Liquid filters, and the expressions are C# expressions
* Templates have to be in the compilation unit that uses them; the generator
  does not render templates from another project

## See also

* [Getting Started](articles/getting-started.md) - add the generator to a project
* [Template Definition](articles/template/syntax.md) - syntax and the deviations
  from Liquid
* [Analyser Rules](articles/rules/CUTOUT001.md) - what the analyser catches
