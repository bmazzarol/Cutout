using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Cutout.Tests;

public static class TemplateMethodDetailsTests
{
    private static TemplateMethodDetails BuildDetails(string source, string template)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            "identity",
            new[] { tree },
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );
        var model = compilation.GetSemanticModel(tree);

        var method = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().First();
        var symbol = model.GetDeclaredSymbol(method)!;

        return new TemplateMethodDetails(
            Namespace: symbol.ContainingNamespace?.ToDisplayString(),
            Usings: SF.List<UsingDirectiveSyntax>(),
            ClassDetails: new ClassDetails(
                symbol.ContainingType.Name,
                (INamedTypeSymbol)symbol.ContainingType
            ),
            MethodDetails: new MethodDetails(method.Identifier.Text, method, symbol),
            AttributeDetails: new TemplateAttributeParts(template)
        );
    }

    [Fact(DisplayName = "Two independently parsed identical templates compare equal")]
    public static void IdenticalContentIsEqualDespiteFreshSyntaxAndSymbols()
    {
        const string source = """
            using System.Text;
            public static partial class Test
            {
                public static partial void Hello(this StringBuilder builder, string name);
            }
            """;

        var a = BuildDetails(source, "Hello {{ name }}");
        var b = BuildDetails(source, "Hello {{ name }}");

        Assert.True(a.Equals(b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact(DisplayName = "A template text change is detected")]
    public static void TemplateChangeIsDetected()
    {
        const string source = """
            using System.Text;
            public static partial class Test
            {
                public static partial void Hello(this StringBuilder builder, string name);
            }
            """;

        var a = BuildDetails(source, "Hello {{ name }}");
        var b = BuildDetails(source, "Hi {{ name }}");

        Assert.False(a.Equals(b));
    }

    [Fact(DisplayName = "A parameter type change is detected")]
    public static void ParameterTypeChangeIsDetected()
    {
        var a = BuildDetails(
            """
            using System.Text;
            public static partial class Test
            {
                public static partial void Hello(this StringBuilder builder, string name);
            }
            """,
            "Hello {{ name }}"
        );
        var b = BuildDetails(
            """
            using System.Text;
            public static partial class Test
            {
                public static partial void Hello(this StringBuilder builder, int name);
            }
            """,
            "Hello {{ name }}"
        );

        Assert.False(a.Equals(b));
    }

    [Fact(DisplayName = "A class accessibility change is detected")]
    public static void AccessibilityChangeIsDetected()
    {
        var a = BuildDetails(
            """
            using System.Text;
            public static partial class Test
            {
                public static partial void Hello(this StringBuilder builder, string name);
            }
            """,
            "Hello {{ name }}"
        );
        var b = BuildDetails(
            """
            using System.Text;
            internal static partial class Test
            {
                public static partial void Hello(this StringBuilder builder, string name);
            }
            """,
            "Hello {{ name }}"
        );

        Assert.False(a.Equals(b));
    }
}
