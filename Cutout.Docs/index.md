# Cutout

![Cutout](images/scissors-icon.png)

**Templating that costs nothing at runtime.**

A source generator that renders Liquid style templates into a StringBuilder at compile time, so generated code keeps its indentation without a template engine dependency.

```shell
dotnet add package Cutout
```

## Why?

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
