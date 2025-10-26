using System.Diagnostics.CodeAnalysis;
using System.Text;
using Cutout.Tests.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Cutout.Tests;

public static partial class Examples
{
    [Template("This is a very simple example")]
    public static partial void Test(this StringBuilder builder);

    [Template("This is a more complex example with a parameter: {{ param }}")]
    public static partial void TestWithParameter(this StringBuilder builder, int param);

    [Template(
        "This is a more complex example with a parameter {{ param }} and a second parameter {{ param2 }}"
    )]
    public static partial void TestWithTwoParameters(
        this StringBuilder builder,
        int param,
        string param2
    );

    public readonly record struct SomeModel(int Value, string Text);

    [Template(
        "This is a more complex example with a parameter {{ model.Value }} and a second parameter {{ model.Text }}"
    )]
    public static partial void TestWithModel(this StringBuilder builder, SomeModel model);

    private const string ExampleTemplate = """
        This is an example of a template that is defined in a constant string.

        It has a conditional block,

        {% if model.Value > 0 -%}
        The result is positive.
        {%- else -%}
        The result is negative.
        {%- end -%}

        And " is escaped.
        """;

    [Template(ExampleTemplate)]
    public static partial void TestWithConstantTemplate(
        this StringBuilder builder,
        SomeModel model
    );

    [FileTemplate]
    public static partial void TestWithFileTemplate(this StringBuilder builder, string name);
}

public class TemplateTests
{
    [Fact(DisplayName = "A simple example can be rendered into a string")]
    public void Case1()
    {
        var builder = new StringBuilder();
        builder.Test();
        Assert.Equal("This is a very simple example", builder.ToString());
    }

    [Fact(DisplayName = "Case1 produces the expected source")]
    public Task Case1a() =>
        """
            [Template("This is a very simple example")]
            public static partial void Test(this StringBuilder builder);
            """.VerifyTemplate();

    [Fact(DisplayName = "A example with a parameter can be rendered into a string")]
    public void Case2()
    {
        var builder = new StringBuilder();
        builder.TestWithParameter(42);
        Assert.Equal("This is a more complex example with a parameter: 42", builder.ToString());
    }

    [Fact(DisplayName = "Case2 produces the expected source")]
    public Task Case2a() =>
        """
            [Template("This is a more complex example with a parameter: {{ param }}")]
            public static partial void TestWithParameter(this StringBuilder builder, int param);
            """.VerifyTemplate();

    [Fact(DisplayName = "A example with two parameters can be rendered into a string")]
    public void Case3()
    {
        var builder = new StringBuilder();
        builder.TestWithTwoParameters(42, "Hello, World!");
        Assert.Equal(
            "This is a more complex example with a parameter 42 and a second parameter Hello, World!",
            builder.ToString()
        );
    }

    [Fact(DisplayName = "Case3 produces the expected source")]
    public Task Case3a() =>
        """
            [Template(
                "This is a more complex example with a parameter {{ param }} and a second parameter {{ param2 }}"
            )]
            public static partial void TestWithTwoParameters(
                this StringBuilder builder,
                int param,
                string param2
            );
            """.VerifyTemplate();

    [Fact(DisplayName = "A example with a model can be rendered into a string")]
    public void Case4()
    {
        var builder = new StringBuilder();
        builder.TestWithModel(new Examples.SomeModel(42, "Hello, World!"));
        Assert.Equal(
            "This is a more complex example with a parameter 42 and a second parameter Hello, World!",
            builder.ToString()
        );
    }

    [Fact(DisplayName = "Case4 produces the expected source")]
    public Task Case4a() =>
        """
            public readonly record struct SomeModel(int Value, string Text);

            [Template(
                "This is a more complex example with a parameter {{ model.Value }} and a second parameter {{ model.Text }}"
            )]
            public static partial void TestWithModel(this StringBuilder builder, SomeModel model);
            """.VerifyTemplate();

    [Fact(DisplayName = "A example with a constant template can be rendered into a string")]
    public void Case5()
    {
        var builder = new StringBuilder();
        builder.TestWithConstantTemplate(new Examples.SomeModel(42, "Hello, World!"));
        Assert.Equal(
            """
            This is an example of a template that is defined in a constant string.

            It has a conditional block,

            The result is positive.
            And " is escaped.
            """,
            builder.ToString()
        );
    }

    [Fact(DisplayName = "Case5 produces the expected source")]
    public Task Case5a() =>
        """
            private const string ExampleTemplate = @"This is an example of a template that is defined in a constant string.

            {% if model.Value > 0 -%}
            The result is positive.
            {%- else -%}
            The result is negative.
            {%- end -%}
            ";

            [Template(ExampleTemplate)]
            public static partial void TestWithConstantTemplate(
                this StringBuilder builder,
                SomeModel model
            );
            """.VerifyTemplate();

    [Fact(DisplayName = "A file template can be used")]
    public void Case6()
    {
        var builder = new StringBuilder();
        builder.TestWithFileTemplate("Ben");
        Assert.Equal(
            """
            # Hello Ben

            This is a test!

            """,
            builder.ToString()
        );
    }

    [Fact(DisplayName = "Case6 produces the expected source")]
    public Task Case6a() =>
        """
            [FileTemplate]
            public static partial void TestWithFileTemplate(this StringBuilder builder, string name);
            """.VerifyTemplate(
            [
                new TestAdditionalFile(
                    "test.txt",
                    SourceText.From("# Hello {{ name }}\n\nThis is a test!")
                ),
            ],
            generatorDriver =>
                generatorDriver.WithUpdatedAnalyzerConfigOptions(
                    new TestAnalyzerConfigOptionsProvider()
                )
        );

    private class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) =>
            throw new NotSupportedException();

        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) =>
            new TestAnalyzerConfigOptions();

        public override AnalyzerConfigOptions GlobalOptions => throw new NotSupportedException();

        private class TestAnalyzerConfigOptions : AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
            {
                if (string.Equals(key, "template_method", StringComparison.Ordinal))
                {
                    value = "<global namespace>.Test.TestWithFileTemplate";
                    return true;
                }

                value = null;
                return false;
            }
        }
    }
}
