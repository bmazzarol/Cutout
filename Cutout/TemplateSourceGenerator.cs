using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Cutout.Exceptions;
using Microsoft.CodeAnalysis.Text;
using Pasted;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Cutout;

/// <summary>
/// Source generator for Cutout templates
/// </summary>
#pragma warning disable RS1038
[Generator]
#pragma warning restore RS1038
public sealed partial class TemplateSourceGenerator : IIncrementalGenerator
{
    private readonly FirebreakPocCounters _pocCounters = new();

    /// <summary>POC instrumentation for the firebreak-poc tests.</summary>
    public FirebreakPocCounters PocCounters => _pocCounters;

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("TemplateAttribute.g", EmbeddedFiles.TemplateAttribute_Source);
            ctx.AddSource("FileTemplateAttribute.g", EmbeddedFiles.FileTemplateAttribute_Source);
            ctx.AddSource("RenderUtilities.g", EmbeddedFiles.RenderUtilities_Source);
        });

        // attribute-based template methods
        // Firebreak POC: entry stage captures failures as Err (e.g. attributes.Single
        // throwing inside BuildTemplateDetails) instead of letting exceptions reach
        // Roslyn as AD0001, which would drop the generator's whole output.
        var attributeTemplateProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Cutout.TemplateAttribute",
            IsTemplateMethod,
            (ctx, token) =>
            {
                _pocCounters.EntryInvocations++;
                Result<TemplateMethodDetails> entry;
                try
                {
                    entry = new Ok<TemplateMethodDetails>(BuildTemplateDetails(ctx, token));
                }
                catch (Exception ex)
                {
                    entry = new Err<TemplateMethodDetails>(
                        EquatableArray<GenerationError>.Create(
                            GenerationError.FromException(ex, ctx.TargetNode)
                        )
                    );
                }
                return entry;
            }
        );

        // Parse stage: exceptions (ParseException) become located Err values.
        // Err items pass through untouched — they are cached as value-equal data.
        var parsedAttributeTemplates = attributeTemplateProvider.Select(
            (details, token) => details.Map(
                d =>
                {
                    _pocCounters.ParseInvocations++;
                    try
                    {
                        _ = d.AttributeDetails.Syntaxes;
                    }
                    catch (ParseException ex)
                    {
                        throw new LocatedFailureException(GenerationError.At(
                            FirebreakDescriptors.TemplateParseError,
                            $"{d.ClassDetails.Name}.{d.MethodDetails.Name}: {ex.Message}",
                            d.MethodDetails.MethodDeclaration
                        ));
                    }
                    return d;
                },
                d => d.MethodDetails.MethodDeclaration
            )
        );

        context.RegisterSourceOutput(parsedAttributeTemplates, GenerateTemplate);

        // external config-based template methods
        var embeddedFiles = context.AdditionalTextsProvider.Select((file, _) => file).Collect();
        var configProvider = context.AnalyzerConfigOptionsProvider.Select((options, _) => options);
        var fileTemplateAttributeProvider = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "Cutout.FileTemplateAttribute",
                IsTemplateMethod,
                (x, _) => x
            )
            .Collect();

        var fileTemplateProvider = embeddedFiles
            .Combine(configProvider)
            .Combine(fileTemplateAttributeProvider)
            .SelectMany(BuildTemplateDetailsFromFile)
            .Select(
                (details, token) => details.Map(
                    d =>
                    {
                        _pocCounters.ParseInvocations++;
                        try
                        {
                            _ = d.AttributeDetails.Syntaxes;
                        }
                        catch (ParseException ex)
                        {
                            throw new LocatedFailureException(GenerationError.At(
                                FirebreakDescriptors.TemplateParseError,
                                $"{d.ClassDetails.Name}.{d.MethodDetails.Name}: {ex.Message}",
                                d.MethodDetails.MethodDeclaration
                            ));
                        }
                        return d;
                    },
                    d => d.MethodDetails.MethodDeclaration
                )
            );

        context.RegisterSourceOutput(fileTemplateProvider, GenerateTemplate);
    }

    private static bool IsTemplateMethod(SyntaxNode syntax, CancellationToken token)
    {
        return syntax is MethodDeclarationSyntax mds
            // the method is either static
            && mds.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
            // and returns a void
            && mds.ReturnType is PredefinedTypeSyntax pts
            && pts.Keyword.IsKind(SyntaxKind.VoidKeyword)
            // and has at least one this parameter
            && mds.ParameterList.Parameters.Any(p =>
                p.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword))
            );
    }

    private static IEnumerable<Result<TemplateMethodDetails>> BuildTemplateDetailsFromFile(
        (
            (ImmutableArray<AdditionalText> Files, AnalyzerConfigOptionsProvider Options) Static,
            ImmutableArray<GeneratorAttributeSyntaxContext> Methods
        ) ctx,
        CancellationToken token
    )
    {
        Dictionary<
            string,
            (
                SemanticModel semanticModel,
                MethodDeclarationSyntax methodDeclarationSyntax,
                IMethodSymbol methodSymbol
            )
        > methodLookup = new(StringComparer.Ordinal);
        foreach (var context in ctx.Methods)
        {
            if (context.TargetNode is not MethodDeclarationSyntax mds)
                continue;

            var methodSymbol = context.SemanticModel.GetDeclaredSymbol(mds, token);

            if (methodSymbol is null)
                continue;

            var ns = methodSymbol.ContainingNamespace?.ToDisplayString();
            var @class = methodSymbol.ContainingType.Name;
            var fullName = $"{ns}.{@class}.{methodSymbol.Name}";
            methodLookup.Add(fullName, (context.SemanticModel, mds, methodSymbol));
        }

        foreach (var file in ctx.Static.Files)
        {
            if (file.GetText(token)?.ToString() is not { } templateText)
            {
                continue;
            }

            var options = ctx.Static.Options.GetOptions(file);

            if (
                !options.TryGetValue("template_method", out var method)
                || string.IsNullOrWhiteSpace(method)
            )
            {
                continue;
            }

            if (!methodLookup.TryGetValue(method, out var methodContext))
            {
                continue;
            }

            // Firebreak POC: per-file failures become Err values, located at the
            // method declaration — a broken file template never takes down the lane.
            Result<TemplateMethodDetails> entry;
            try
            {
                entry = new Ok<TemplateMethodDetails>(
                    BuildTemplateDetailsFromSyntax(
                        methodContext.semanticModel,
                        methodContext.methodDeclarationSyntax,
                        methodContext.methodSymbol,
                        templateText
                    )
                );
            }
            catch (Exception ex)
            {
                entry = new Err<TemplateMethodDetails>(
                    EquatableArray<GenerationError>.Create(
                        GenerationError.FromException(ex, methodContext.methodDeclarationSyntax)
                    )
                );
            }

            yield return entry;
        }
    }

    private static TemplateMethodDetails BuildTemplateDetails(
        GeneratorAttributeSyntaxContext ctx,
        CancellationToken token
    )
    {
        var methodDeclarationSyntax = (MethodDeclarationSyntax)ctx.TargetNode;

        var methodSymbol = ctx.SemanticModel.GetDeclaredSymbol(
            methodDeclarationSyntax,
            cancellationToken: token
        )!;

        return BuildTemplateDetailsFromSyntax(
            ctx.SemanticModel,
            methodDeclarationSyntax,
            methodSymbol,
            template: null
        );
    }

    private static TemplateMethodDetails BuildTemplateDetailsFromSyntax(
        SemanticModel semanticModel,
        MethodDeclarationSyntax methodDeclarationSyntax,
        IMethodSymbol methodSymbol,
        string? template
    )
    {
        var ns = methodSymbol.ContainingNamespace?.ToDisplayString();

        // get all usings
        var usings = methodDeclarationSyntax
            .TryGetUsings()
            .Add(SF.UsingDirective(SF.ParseName(" System")))
            .Add(SF.UsingDirective(SF.ParseName(" System.Diagnostics.CodeAnalysis")))
            .Add(SF.UsingDirective(SF.ParseName(" Cutout")));

        // get the method details
        var containingType = methodSymbol.ContainingType;
        var @class = containingType.Name;
        var classDetails = new ClassDetails(Name: @class, ClassSymbol: containingType);

        var name = methodDeclarationSyntax.Identifier.Text;
        var methodDetails = new MethodDetails(
            Name: name,
            MethodDeclaration: methodDeclarationSyntax,
            MethodSymbol: methodSymbol
        );

        // get the attribute details
        var attributeParts =
            template is null || string.IsNullOrWhiteSpace(template)
                ? new TemplateAttributeParts(methodDetails, semanticModel)
                : new TemplateAttributeParts(template);

        return new TemplateMethodDetails(
            Namespace: ns,
            Usings: usings,
            ClassDetails: classDetails,
            MethodDetails: methodDetails,
            AttributeDetails: attributeParts
        );
    }

    private void GenerateTemplate(
        SourceProductionContext context,
        Result<TemplateMethodDetails> result
    )
    {
        // Firebreak POC: Err items report precise, static-descriptor diagnostics.
        // Nothing here can throw, so no item can take the whole generator down.
        if (result is Err<TemplateMethodDetails> error)
        {
            _pocCounters.ReportedDiagnostics += error.Errors.Count;
            foreach (var generationError in error.Errors)
            {
                context.ReportDiagnostic(generationError.ToDiagnostic());
            }
            return;
        }

        var details = ((Ok<TemplateMethodDetails>)result).Value;
        _pocCounters.OutputInvocations++;

        try
        {
            using (var writer = new StringWriter())
            {
                var indentedWriter = new IndentedTextWriter(writer, "    ");

                WriteTemplateMethod(indentedWriter, details, includeWhitespaceReceiver: false);
                context.AddSource(
                    $"{details.ClassDetails.Name}.{details.MethodDetails.Name}.g.cs",
                    SourceText.From(writer.ToString(), Encoding.UTF8)
                );
            }

            using (var writer = new StringWriter())
            {
                var indentedWriter = new IndentedTextWriter(writer, "    ");

                WriteTemplateMethod(indentedWriter, details, includeWhitespaceReceiver: true);

                context.AddSource(
                    $"{details.ClassDetails.Name}.{details.MethodDetails.Name}.wsr.g.cs",
                    SourceText.From(writer.ToString(), Encoding.UTF8)
                );
            }
        }
        catch (Exception ex)
        {
            // Simulated-renderer-bug path: even a rendering failure is a located
            // diagnostic, not a generator crash.
            _pocCounters.ReportedDiagnostics++;
            context.ReportDiagnostic(
                GenerationError.FromException(ex, details.MethodDetails.MethodDeclaration).ToDiagnostic()
            );
        }
    }
}
