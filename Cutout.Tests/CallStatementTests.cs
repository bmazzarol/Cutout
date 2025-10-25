using System.Text;
using Cutout.Tests.Extensions;

namespace Cutout.Tests;

public static partial class CallTemplates
{
    public sealed record Product(string Title);

    private const string CallExample1 = "Some text before {% call Case2(product.Title) %}";

    [Template(CallExample1)]
    public static partial void Case1(this StringBuilder builder, Product product);

    private const string CallExample2 = "The product title is {{ title }}";

    [Template(CallExample2)]
    public static partial void Case2(this StringBuilder builder, string title);

    private const string CallExample3 = """
        This is an example with a call with leading whitespace,
        ```
            {% call Case4(product) %}
        ```
        """;

    [Template(CallExample3)]
    public static partial void Case3(this StringBuilder builder, Product product);

    private const string CallExample4 = """
        The title in two calls,
        {{ product.Title }}
        {{ product.Title.ToLowerInvariant() }}
        """;

    [Template(CallExample4)]
    public static partial void Case4(this StringBuilder builder, Product product);

    [Template(
        """
            This is an example more than one level of nesting,

                {% call Case3(product) %}
            """
    )]
    public static partial void Case5(this StringBuilder builder, Product product);
}

public sealed class CallStatementTests
{
    [Fact(DisplayName = "A call statement can used")]
    public void Case1()
    {
        var builder = new StringBuilder();
        builder.Case1(new CallTemplates.Product("Awesome Shoes"));
        Assert.Equal("Some text before The product title is Awesome Shoes", builder.ToString());
    }

    [Fact(DisplayName = "Case1 produces the expected source")]
    public Task Case1a() =>
        """
            [Template("Some text before {% call Case2(product) %}")]
            public static partial void Test(this StringBuilder builder, string product);
            """.VerifyTemplate();

    [Fact(DisplayName = "A call statement with leading whitespace can used")]
    public void Case2()
    {
        var builder = new StringBuilder();
        builder.Case3(new CallTemplates.Product("Awesome Shoes"));
        Assert.Equal(
            """
            This is an example with a call with leading whitespace,
            ```
                The title in two calls,
                Awesome Shoes
                awesome shoes
            ```
            """,
            builder.ToString()
        );
    }

    [Fact(DisplayName = "A nested call statement with leading whitespace can used")]
    public void Case3()
    {
        var builder = new StringBuilder();
        builder.Case5(new CallTemplates.Product("Awesome Shoes"));
        Assert.Equal(
            """
            This is an example more than one level of nesting,

                This is an example with a call with leading whitespace,
                ```
                    The title in two calls,
                    Awesome Shoes
                    awesome shoes
                ```
            """,
            builder.ToString()
        );
    }
}
