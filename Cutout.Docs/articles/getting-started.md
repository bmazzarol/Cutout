# Getting Started

Add the generator to the project, then mark the template methods `partial`.

```xml
<ItemGroup>
    <PackageReference Include="Cutout" Version="x.x.x">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
</ItemGroup>
```

Use the `Cutout.Template` attribute to define a template method. The first
parameter is the `StringBuilder`-like type to write to; everything else is
available in the template.

```c#
using System.Text;

[Cutout.Template("This is a very simple example with a {{parameter}} parameter")]
public static partial void Test(this StringBuilder builder, string parameter);
```

With `parameter` set to `value` the builder contains,

```text
This is a very simple example with a value parameter
```

The template has to be a compile-time constant, so define it as a `const` field
or inline in the attribute.

```c#
private const string TemplateExample = """
    A multi-line template example
    with a {{parameter}} parameter.

    It also has a conditional section,
    {%- if parameter == "INVALID" %}
    show this text
    {%- else %}
    show this text instead
    {%- end %}
    """;

[Cutout.Template(TemplateExample)]
public static partial void Test(this StringBuilder builder, string parameter);
```

With `parameter` set to `value` that renders,

```text
A multi-line template example
with a value parameter.

It also has a conditional section,
show this text instead
```

## What the generator does not accept

* The template method must be `partial`; the generator fills in the body
* The template must be a constant string, not a string built at runtime
* The first parameter must be a `StringBuilder`-like type; the generator writes
  the rendered output into it
* Templates come from the attribute. The generator does not read template files
  from other projects
