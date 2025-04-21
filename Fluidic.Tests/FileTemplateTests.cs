using System.Text;
using Fluidic.Tests.Extensions;
using Microsoft.CodeAnalysis.Text;

namespace Fluidic.Tests;

public static partial class Examples
{
    [FileTemplate]
    public static partial void FileTest(this StringBuilder builder, string name);
}

public class FileTemplateTests
{
    [Fact(DisplayName = "A simple example can be rendered into a string")]
    public void Case1()
    {
        var builder = new StringBuilder();
        builder.FileTest("Ben");
        Assert.Equal(
            """
            Hello Ben!

            This is a test file.

            And it has some content in it.

            """,
            builder.ToString()
        );
    }

    [Fact(DisplayName = "Case1 produces the expected source")]
    public Task Case1a() =>
        """
            [FileTemplate]
            public static partial void FileTest(this StringBuilder builder);
            """.VerifyFileTemplate(
            [
                new TestAdditionalFile(
                    "Test.FileTest.liquid",
                    SourceText.From(
                        """
                        This is a very simple example

                        This is a new line
                        And so is this

                        Let's add some more lines
                        """
                    )
                ),
            ]
        );
}
