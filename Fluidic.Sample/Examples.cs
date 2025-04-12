using System.Text;

namespace Fluidic.Sample;

public static partial class Examples
{
    [StringTemplate("This is a very simple example")]
    public static partial void Test(this StringBuilder builder);

    [FileTemplate]
    public static partial void FileTest(this StringBuilder builder);
}
