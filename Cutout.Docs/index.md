<!-- markdownlint-disable MD033 MD041 -->
<div align="center">

<img src="images/scissors-icon.png" alt="Cutout" width="150px"/>

# Cutout

---

[![Nuget](https://img.shields.io/nuget/v/Cutout)](https://www.nuget.org/packages/Cutout/)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=bmazzarol_Cutout&metric=coverage)](https://sonarcloud.io/summary/new_code?id=bmazzarol_Cutout)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=bmazzarol_Cutout&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=bmazzarol_Cutout)
[![CD Build](https://github.com/bmazzarol/Cutout/actions/workflows/cd-build.yml/badge.svg)](https://github.com/bmazzarol/Cutout/actions/workflows/cd-build.yml)
[![Check Markdown](https://github.com/bmazzarol/Cutout/actions/workflows/check-markdown.yml/badge.svg)](https://github.com/bmazzarol/Cutout/actions/workflows/check-markdown.yml)

Zero cost :muscle: source generated templating for .NET

---

</div>

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
