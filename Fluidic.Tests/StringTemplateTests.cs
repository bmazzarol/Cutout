using System.Text;

namespace Fluidic.Tests;

public static partial class Examples
{
    [StringTemplate("This is a very simple example")]
    public static partial void Test(this StringBuilder builder);
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
}
