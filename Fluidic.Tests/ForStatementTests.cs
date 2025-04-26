using System.Text;
using Fluidic.Tests.Extensions;

namespace Fluidic.Tests;

public static partial class ForTemplates
{
    public sealed record Product(string Title, string[] Tags);

    private const string ForExample1 = """
        # {{ product.Title }}

        This is an example template for a product with tags.

        These are the tags,

        {{ foreach tag in product.Tags }}
        * {{ tag }}
        {{ end }}
        These can also be numbered,

        {{ foreach (tag, index) in product.Tags.Select((tag,index) => (tag, index + 1)) }}
        {{index}}. {{ tag }}
        {{ end }}
        And used with traditional for loop,

        {{ for i = 0; i < product.Tags.Length; i++ }}
        {{i + 1}}. {{ product.Tags[i] }}
        {{ end }}
        """;

    [Template(ForExample1)]
    public static partial void Case1(this StringBuilder builder, Product product);

    private const string ForExample2 = """
        A while loop can also be used

        {{ var i = 0 }}
        {{ while i < product.Tags.Length }}
        {{i + 1}}. {{ product.Tags[i++] }}
        {{ end }}
        """;

    [Template(ForExample2)]
    public static partial void Case2(this StringBuilder builder, Product product);

    private const string ForExample3 = """
        Continue and break can also be used
        {{ for i = 0; i < product.Tags.Length; i++ }}
        {{ if product.Tags[i] == "awesome" }}
        {{ continue }}
        {{ end }}
        {{ if product.Tags[i] == "shoes" }}
        {{ break }}
        {{ end }}
        {{i + 1}}. {{ product.Tags[i] }}
        {{ end }}
        """;

    [Template(ForExample3)]
    public static partial void Case3(this StringBuilder builder, Product product);
}

public class ForStatementTests
{
    [Fact(DisplayName = "A for statement can used")]
    public void Case1()
    {
        var builder = new StringBuilder();
        builder.Case1(new ForTemplates.Product("Awesome Shoes", ["awesome", "shoes", "cool"]));
        Assert.Equal(
            """
            # Awesome Shoes

            This is an example template for a product with tags.

            These are the tags,

            * awesome
            * shoes
            * cool

            These can also be numbered,

            1. awesome
            2. shoes
            3. cool

            And used with traditional for loop,

            1. awesome
            2. shoes
            3. cool

            """,
            builder.ToString()
        );
    }

    [Fact(DisplayName = "Case1 produces the expected source")]
    public Task Case1a() =>
        """
            [Template("This is a test for tags [{{ foreach tag in product.Tags }}{{tag}}; {{ end }}] which is cool.")]
            public static partial void Test(this StringBuilder builder, string product);
            """.VerifyTemplate();

    [Fact(DisplayName = "A while statement can used")]
    public void Case2()
    {
        var builder = new StringBuilder();
        builder.Case2(new ForTemplates.Product("Awesome Shoes", ["awesome", "shoes", "cool"]));
        Assert.Equal(
            """
            A while loop can also be used

            1. awesome
            2. shoes
            3. cool

            """,
            builder.ToString()
        );
    }

    [Fact(DisplayName = "Case2 produces the expected source")]
    public Task Case2a() =>
        """
            [Template("This is a test for tags [{{ while i < product.Tags.Length }}{{i + 1}}. {{ product.Tags[i++] }}{{ end }}] which is cool.")]
            public static partial void Test(this StringBuilder builder, string product);
            """.VerifyTemplate();

    [Fact(DisplayName = "A for statement with continue and break can used")]
    public void Case3()
    {
        var builder = new StringBuilder();
        builder.Case3(new ForTemplates.Product("Awesome Shoes", ["awesome", "shoes", "cool"]));
        Assert.Equal(
            """
            Continue and break can also be used

            """,
            builder.ToString()
        );
    }

    [Fact(DisplayName = "Case3 produces the expected source")]
    public Task Case3a() =>
        """
            [Template("This is a test for tags [{{ for i = 0; i < product.Tags.Length; i++ }}{{ if product.Tags[i] == \"awesome\" }}{{ continue }}{{ else if product.Tags[i] == \"shoes\" }}{{ break }}{{ end }}{{i + 1}}. {{ product.Tags[i] }}{{ end }}] which is cool.")]
            public static partial void Test(this StringBuilder builder, string product);
            """.VerifyTemplate();
}
