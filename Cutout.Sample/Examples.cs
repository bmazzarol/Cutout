using System.Text;

namespace Cutout.Sample;

public static partial class Examples
{
    [Template("This is a very simple example")]
    public static partial void Test(this StringBuilder builder);
}
