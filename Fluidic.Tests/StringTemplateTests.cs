using System.Text;
using Fluidic.Tests.Extensions;

namespace Fluidic.Tests;

public static partial class Examples
{
    [StringTemplate("This is a very simple example")]
    public static partial void Test(this StringBuilder builder);

    [StringTemplate("This is a more complex example with a parameter: {{ param }}")]
    public static partial void TestWithParameter(this StringBuilder builder, int param);

    [StringTemplate(
        "This is a more complex example with a parameter {{ param }} and a second parameter {{ param2 }}"
    )]
    public static partial void TestWithTwoParameters(
        this StringBuilder builder,
        int param,
        string param2
    );

    public readonly record struct SomeModel(int Value, string Text);

    [StringTemplate(
        "This is a more complex example with a parameter {{ model.Value }} and a second parameter {{ model.Text }}"
    )]
    public static partial void TestWithModel(this StringBuilder builder, SomeModel model);
}

public class StringTemplateTests
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
            [StringTemplate("This is a very simple example")]
            public static partial void Test(this StringBuilder builder);
            """.VerifyStringTemplate();

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
            [StringTemplate("This is a more complex example with a parameter: {{ param }}")]
            public static partial void TestWithParameter(this StringBuilder builder, int param);
            """.VerifyStringTemplate();

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
            [StringTemplate(
                "This is a more complex example with a parameter {{ param }} and a second parameter {{ param2 }}"
            )]
            public static partial void TestWithTwoParameters(
                this StringBuilder builder,
                int param,
                string param2
            );
            """.VerifyStringTemplate();

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

            [StringTemplate(
                "This is a more complex example with a parameter {{ model.Value }} and a second parameter {{ model.Text }}"
            )]
            public static partial void TestWithModel(this StringBuilder builder, SomeModel model);
            """.VerifyStringTemplate();
}
