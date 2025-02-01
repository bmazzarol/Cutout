using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Fluidic;

public sealed partial class StringTemplateSourceGenerator
{
    private sealed record StringTemplateAttributeParts
    {
        public string? Template { get; }

        public StringTemplateAttributeParts(MethodDeclarationSyntax methodDeclaration)
        {
            var arguments = methodDeclaration
                .AttributeLists.SelectMany(list => list.Attributes)
                .Single(attribute =>
                    string.Equals(
                        attribute.Name.ToString(),
                        "StringTemplate",
                        StringComparison.Ordinal
                    )
                    || string.Equals(
                        attribute.Name.ToString(),
                        "Fluidic.StringTemplate",
                        StringComparison.Ordinal
                    )
                )
                .ArgumentList!.Arguments;

            var nameToArgs = arguments
                .Where(x => x.NameEquals is not null)
                .ToDictionary(
                    x => x.NameEquals?.Name.ToString(),
                    syntax => syntax.Expression.ToString(),
                    StringComparer.OrdinalIgnoreCase
                );
            Template = nameToArgs.TryGetValue(nameof(Template), out var failureMessage)
                ? failureMessage
                : arguments
                    .Where(x => x.NameEquals is null)
                    .Select(x => x.Expression.ToString())
                    .FirstOrDefault();
        }

        private static bool IsEnabled(string value, Dictionary<string?, string> nameToArgs) =>
            nameToArgs.TryGetValue(value, out var v)
            && string.Equals(v, "true", StringComparison.Ordinal);
    }
}
