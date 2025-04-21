using System.Text;
using Fluidic.Tests.Extensions;

namespace Fluidic.Tests;

public static partial class IfTemplates
{
    public sealed record Product(string Title);

    [FileTemplate]
    public static partial void Case1(this StringBuilder builder, Product product);
}

public class IfControlTests
{
    [Fact(DisplayName = "A conditional if statement can used in a file template")]
    public void Case1()
    {
        var builder = new StringBuilder();
        builder.Case1(new IfTemplates.Product("Awesome Shoes"));
        Assert.Equal(
            @"
    These shoes are awesome!
",
            builder.ToString()
        );
    }

    [Fact(DisplayName = "Case1 produces the expected source")]
    public Task Case1a() =>
        """
            [StringTemplate("{{ if product == \"Awesome Shoes\" }} These shoes are awesome! {{ end }}")]
            public static partial void Test(this StringBuilder builder, string product);
            """.VerifyStringTemplate();
}
