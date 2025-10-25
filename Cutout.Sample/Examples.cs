using System.Text;

namespace Cutout.Sample;

public static partial class Examples
{
    [Template("This is a very simple example")]
    public static partial void Test(this StringBuilder builder);

    #region ParameterExample

    [Template("This is a very simple example with a {{parameter}} parameter")]
    public static partial void Test2(this StringBuilder builder, string parameter);

    #endregion

    #region ExampleWithConditionAndConstTemplate

    private const string TemplateExample = """
        A multi-line template example
        with a {{parameter}} parameter.

        It also has a conditional section,
        {%- if parameter  == "INVALID" -%}
        show this text
        {%- else -%}
        show this text instead
        {%- end -%}
        """;

    [Template(TemplateExample)]
    public static partial void Test3(this StringBuilder builder, string parameter);

    #endregion

    #region ExampleWithExternalFileTemplate

    [FileTemplate]
    public static partial void Test4(this StringBuilder builder, string parameter);

    #endregion
}
