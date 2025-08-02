using System.Text;
using Cutout.Tests.Extensions;

namespace Cutout.Tests;

public static partial class IfTemplates
{
    public sealed record Product(string Title);

    private const string IfExample1 = """
        {% if product.Title == "Awesome Shoes" -%}
        These shoes are awesome!
        {%- end -%}
        """;

    [Template(IfExample1)]
    public static partial void Case1(this StringBuilder builder, Product product);

    private const string IfExample2 = """
        {% if product.Title == "Awesome Shoes" -%}
        These shoes are awesome!
        {%- else -%}
        These shoes are not awesome!
        {%- end -%}
        """;

    [Template(IfExample2)]
    public static partial void Case2(this StringBuilder builder, Product product);

    private const string IfExample3 = """
        {% if product.Title == "Awesome Shoes" -%}
        These shoes are awesome!
        {%- elseif product.Title == "Cool Shoes" -%}
        These shoes are cool!
        {%- else -%}
        These shoes are not awesome or cool!
        {%- end -%}
        """;

    [Template(IfExample3)]
    public static partial void Case3(this StringBuilder builder, Product product);
}

public class IfStatementTests
{
    [Fact(DisplayName = "A conditional if statement can used")]
    public void Case1()
    {
        var builder = new StringBuilder();
        builder.Case1(new IfTemplates.Product("Awesome Shoes"));
        Assert.Equal("These shoes are awesome!", builder.ToString());
    }

    [Fact(DisplayName = "Case1 produces the expected source")]
    public Task Case1a() =>
        """
            [Template("{% if product == \"Awesome Shoes\" %} These shoes are awesome! {% end %}")]
            public static partial void Test(this StringBuilder builder, string product);
            """.VerifyTemplate();

    [Fact(DisplayName = "A conditional if statement with else can used")]
    public void Case2()
    {
        var builder = new StringBuilder();
        builder.Case2(new IfTemplates.Product("Awesome Shoes"));
        Assert.Equal("These shoes are awesome!", builder.ToString());

        builder.Clear();
        builder.Case2(new IfTemplates.Product("Cool Shoes"));
        Assert.Equal("These shoes are not awesome!", builder.ToString());
    }

    [Fact(DisplayName = "Case2 produces the expected source")]
    public Task Case2a() =>
        """
            [Template("{% if product == \"Awesome Shoes\" %} These shoes are awesome! {% else %} These shoes are not awesome! {% end %}")]
            public static partial void Test(this StringBuilder builder, string product);
            """.VerifyTemplate();

    [Fact(DisplayName = "A conditional if statement with else if can used")]
    public void Case3()
    {
        var builder = new StringBuilder();
        builder.Case3(new IfTemplates.Product("Awesome Shoes"));
        Assert.Equal("These shoes are awesome!", builder.ToString());

        builder.Clear();
        builder.Case3(new IfTemplates.Product("Cool Shoes"));
        Assert.Equal("These shoes are cool!", builder.ToString());

        builder.Clear();
        builder.Case3(new IfTemplates.Product("Other Shoes"));
        Assert.Equal("These shoes are not awesome or cool!", builder.ToString());
    }

    [Fact(DisplayName = "Case3 produces the expected source")]
    public Task Case3a() =>
        """
            [Template("{% if product == \"Awesome Shoes\" %} These shoes are awesome! {% elseif product == \"Cool Shoes\" %} These shoes are cool! {% else %} These shoes are not awesome or cool! {% end %}")]
            public static partial void Test(this StringBuilder builder, string product);
            """.VerifyTemplate();
}
