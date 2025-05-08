using System.Text;
using Cutout.Tests.Extensions;

namespace Cutout.Tests;

public static partial class CallTemplates
{
    public sealed record Product(string Title);

    private const string CallExample1 = "Some text before {{ call Case2(product.Title) }}";

    [Template(CallExample1)]
    public static partial void Case1(this StringBuilder builder, Product product);

    private const string CallExample2 = "The product title is {{ title }}";

    [Template(CallExample2)]
    public static partial void Case2(this StringBuilder builder, string title);
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
            [Template("Some text before {{ call Case2(product) }}")]
            public static partial void Test(this StringBuilder builder, string product);
            """.VerifyTemplate();
}
