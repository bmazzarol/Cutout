using System.Text;
using Xunit;

namespace Cutout.Tests;

public static partial class DocExamples
{
    public sealed record Product(string Title, string[] Tags);

    [Template(
        """
            {% if product == "Awesome Shoes" -%}
            These shoes are awesome!
            {%- else -%}
            These shoes are not awesome!
            {%- end -%}
            """
    )]
    public static partial void IfExample(this StringBuilder builder, string product);

    [Template(
        """
            {%- for i = 0; i < product.Tags.Length; i++ -%}
            {{ i + 1 }}. {{ product.Tags[i] }}
            {% end -%}
            """
    )]
    public static partial void ForExample(this StringBuilder builder, Product product);

    [Template(
        """
            {%- foreach (tag, index) in product.Tags.Select((tag, index) => (tag, index + 1)) -%}
            {{ index }}. {{ tag }}
            {% end -%}
            """
    )]
    public static partial void ForeachExample(this StringBuilder builder, Product product);

    [Template(
        """
            {% var i = 0 -%}
            {% while i < product.Tags.Length -%}
            {{ i + 1 }}. {{ product.Tags[i++] }}
            {% end -%}
            """
    )]
    public static partial void WhileExample(this StringBuilder builder, Product product);

    [Template(
        """
            {% var i = 0 -%}
            {% while i < 3 -%}
            {{ i++ }}
            {% end -%}
            """
    )]
    public static partial void VarExample(this StringBuilder builder);

    [Template(
        """
            Continue and break can also be used
            {%- for i = 0; i < product.Tags.Length; i++ -%}
            {%- if product.Tags[i] == "awesome" -%}
            {%- continue -%}
            {%- end -%}
            {%- if product.Tags[i] == "shoes" -%}
            {%- break -%}
            {%- end -%}
            {{i + 1}}. {{ product.Tags[i] }}
            {%- end -%}
            """
    )]
    public static partial void BreakContinueExample(this StringBuilder builder, Product product);

    [Template("This is a very simple example with a {{parameter}} parameter")]
    public static partial void SimpleExample(this StringBuilder builder, string parameter);

    [Template(
        """
            A multi-line template example
            with a {{parameter}} parameter.

            It also has a conditional section,
            {%- if parameter == "INVALID" %}
            show this text
            {%- else %}
            show this text instead
            {%- end %}
            """
    )]
    public static partial void MultiLineExample(this StringBuilder builder, string parameter);

    [Template("Some text before {% call Inner(title) %}")]
    public static partial void CallExample(this StringBuilder builder, string title);

    [Template("The product title is {{ title }}")]
    public static partial void Inner(this StringBuilder builder, string title);

    [Template(
        """
            {% if product.Tags.Length == 0 -%}
            {%- return -%}
            {%- end -%}
            {{ product.Title }} has {{ product.Tags.Length }} tags
            """
    )]
    public static partial void ReturnExample(this StringBuilder builder, Product product);
}

public class DocExampleTests
{
    private static string Show(StringBuilder sb) => sb.ToString().Replace("\n", "\\n");

    [Fact]
    public void PrintDocExamples()
    {
        var tags = new[] { "awesome", "shoes", "cool" };
        var product = new DocExamples.Product("Awesome Shoes", tags);
        var sb = new StringBuilder();
        var lines = new System.Collections.Generic.List<string>();

        DocExamples.IfExample(sb, "Awesome Shoes");
        lines.Add($"if(true)        = {Show(sb)}");
        sb.Clear();
        DocExamples.IfExample(sb, "Cool Shoes");
        lines.Add($"if(false)       = {Show(sb)}");

        sb.Clear();
        DocExamples.ForExample(sb, product);
        lines.Add($"for             = {Show(sb)}");

        sb.Clear();
        DocExamples.ForeachExample(sb, product);
        lines.Add($"foreach         = {Show(sb)}");

        sb.Clear();
        DocExamples.WhileExample(sb, product);
        lines.Add($"while           = {Show(sb)}");

        sb.Clear();
        DocExamples.VarExample(sb);
        lines.Add($"var             = {Show(sb)}");

        sb.Clear();
        DocExamples.BreakContinueExample(sb, product);
        lines.Add($"break/continue  = {Show(sb)}");

        sb.Clear();
        DocExamples.SimpleExample(sb, "value");
        lines.Add($"simple          = {Show(sb)}");
        sb.Clear();
        DocExamples.MultiLineExample(sb, "value");
        lines.Add($"multiline       = {Show(sb)}");
        sb.Clear();
        DocExamples.CallExample(sb, "Awesome Shoes");
        lines.Add($"call            = {Show(sb)}");

        sb.Clear();
        DocExamples.ReturnExample(sb, product);
        lines.Add($"return          = {Show(sb)}");
        sb.Clear();
        DocExamples.ReturnExample(sb, new DocExamples.Product("Awesome Shoes", []));
        lines.Add($"return(empty)   = {Show(sb)}");

        File.WriteAllText("/tmp/cutout-doc-examples.txt", string.Join("\n", lines));
    }
}
