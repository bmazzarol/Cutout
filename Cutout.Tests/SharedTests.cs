using Cutout.Tests.Extensions;

namespace Cutout.Tests;

public sealed class SharedTests
{
    [Fact(DisplayName = "All shared code renders correctly")]
    public Task Case1()
    {
        var driver = """
            using Cutout;

            internal static partial class Test
            {
                [Template("test {{value}}")]
                public static partial void Test(this StringBuilder builder, string value);
            }
            """.BuildDriver([]);

        return Verify(driver).IgnoreGeneratedResult(result => result.HintName.StartsWith("Test"));
    }
}
