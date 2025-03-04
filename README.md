<!-- markdownlint-disable MD033 MD041 -->
<div align="center">

<img src="droplets-icon.png" alt="Tuxedo" width="150px"/>

# Fluidic

[:running: **_Getting Started_
**](https://bmazzarol.github.io/Fluidic/getting-started.html)
|
[:books: **_Documentation_**](https://bmazzarol.github.io/Fluidic)

[![Nuget](https://img.shields.io/nuget/v/tuxedo.sourcegenerator)]
(https://www.nuget.org/packages/fluidic.sourcegenerator/)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=bmazzarol_Tuxedo&metric=coverage)](https://sonarcloud.io/summary/new_code?id=bmazzarol_Tuxedo)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=bmazzarol_Tuxedo&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=bmazzarol_Tuxedo)
[![CD Build](https://github.com/bmazzarol/tuxedo/actions/workflows/cd-build.yml/badge.svg)](https://github.com/bmazzarol/tuxedo/actions/workflows/cd-build.yml)
[![Check Markdown](https://github.com/bmazzarol/tuxedo/actions/workflows/check-markdown.yml/badge.svg)](https://github.com/bmazzarol/tuxedo/actions/workflows/check-markdown.yml)

Zero cost :muscle: source generated templating for .NET

</div>
<!-- markdownlint-enable MD033 MD041 -->

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
incurring a dependency on a template engine, and it lets you use a simple 
version of [liquid](https://shopify.github.io/liquid/) and any type that 
implements the basic StringBuilder API.