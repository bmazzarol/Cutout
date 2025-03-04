<!-- markdownlint-disable MD013 -->

# ![Fluidic](https://raw.githubusercontent.com/bmazzarol/Fluidic/main/droplets-small-icon.png) Fluidic

<!-- markdownlint-enable MD013 -->

[![Nuget](https://img.shields.io/nuget/v/fluidic.sourcegenerator)](https://www.nuget.org/packages/tuxedo.sourcegenerator/)

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
incurring a dependency on a template engine, and it lets you use a simple
version of [liquid](https://shopify.github.io/liquid/) and any type that
implements the basic StringBuilder API.
