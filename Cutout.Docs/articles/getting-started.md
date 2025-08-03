# Getting Started

To use this library, simply include `Cutout.dll` in your project or grab
it from [NuGet](https://www.nuget.org/packages/Cutout/), and add a reference to it.

```xml
<ItemGroup>
    <PackageReference Include="Cutout" Version="x.x.x">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
</ItemGroup>
```

Then use the `Cutout.Template` attribute to define a template method.

[!code-csharp[](../../Cutout.Sample/Examples.cs#ParameterExample)]

The first parameter is the `StringBuilder`-like type to write to. Everything
else passed can be used in the template.

The template must be a compile-time constant string, so it can be defined
as a `const` field or inline in the attribute.

[!code-csharp[](../../Cutout.Sample/Examples.cs#ExampleWithConditionAndConstTemplate)]
