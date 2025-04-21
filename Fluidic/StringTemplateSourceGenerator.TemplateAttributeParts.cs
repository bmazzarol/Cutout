using Fluidic.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Scriban.Parsing;

namespace Fluidic;

public sealed partial class StringTemplateSourceGenerator
{
    private sealed record TemplateAttributeParts
    {
        public string? Template { get; init; }

        public string? TemplatePath { get; private set; }

        internal IReadOnlyList<Token> Tokens { get; init; } = [];

        public TemplateAttributeParts(MethodDetails details, SemanticModel ctxSemanticModel)
        {
            var attributes = details
                .MethodDeclaration.AttributeLists.SelectMany(list => list.Attributes)
                .ToArray();

            if (Array.Exists(attributes, x => x.IsNamedAttribute("Fluidic.FileTemplate")))
            {
                ExtractPath(ctxSemanticModel, attributes, details);
                return;
            }

            var arguments = attributes
                .Single(x => x.IsNamedAttribute("Fluidic.StringTemplate"))
                .ArgumentList!.Arguments;

            var template = ctxSemanticModel.GetConstantValue(arguments[0].Expression);

            Template = template.HasValue ? template.Value?.ToString() : string.Empty;

            var lexer = new Lexer(Template!);
            Tokens = lexer.ToArray();
        }

        private void ExtractPath(
            SemanticModel ctxSemanticModel,
            AttributeSyntax[] attributes,
            MethodDetails details
        )
        {
            var arguments = attributes
                .Single(x => x.IsNamedAttribute("Fluidic.FileTemplate"))
                .ArgumentList?.Arguments;

            if (arguments is null)
            {
                TemplatePath = BuildFileNameFromMethodName(details);
                return;
            }

            var path = ctxSemanticModel.GetConstantValue(arguments.Value[0].Expression);
            TemplatePath = path.HasValue
                ? path.Value?.ToString()
                : BuildFileNameFromMethodName(details);
        }

        // The name of the file is the name of the method inclusive of the class name and namespace with .liquid extension
        private static string? BuildFileNameFromMethodName(MethodDetails details)
        {
            return $"{details.MethodSymbol.ContainingType}.{details.MethodSymbol.Name}.liquid";
        }
    }
}
