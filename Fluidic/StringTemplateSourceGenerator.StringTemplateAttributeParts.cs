using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Scriban.Parsing;

namespace Fluidic;

public sealed partial class StringTemplateSourceGenerator
{
    private sealed record StringTemplateAttributeParts
    {
        public string? Template { get; }

        internal IReadOnlyList<Token> Tokens { get; }

        public StringTemplateAttributeParts(MethodDetails details, SemanticModel ctxSemanticModel)
        {
            var arguments = details
                .MethodDeclaration.AttributeLists.SelectMany(list => list.Attributes)
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

            var template = ctxSemanticModel.GetConstantValue(arguments[0].Expression);

            Template = template.HasValue ? template.Value?.ToString() : string.Empty;

            var lexer = new Lexer(Template!);
            Tokens = lexer.ToArray();
        }
    }
}
