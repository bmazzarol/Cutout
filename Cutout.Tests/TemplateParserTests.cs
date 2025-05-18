using Cutout.Exceptions;
using Cutout.Parser;
using Scriban.Parsing;

namespace Cutout.Tests;

public sealed class TemplateParserTests
{
    [Fact(DisplayName = "A raw string can be parsed")]
    public void Case1()
    {
        const string template = "raw string";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var item = Assert.Single(result);
        var rawText = Assert.IsType<Syntax.RawText>(item);
        Assert.Equal("raw string", rawText.Value);
    }

    [Fact(DisplayName = "A renderable code block can be parsed")]
    public void Case2()
    {
        const string template = "{{ code }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var item = Assert.Single(result);
        var renderableExpression = Assert.IsType<Syntax.RenderableExpression>(item);
        Assert.Equal("code", renderableExpression.Value);
    }

    [Fact(DisplayName = "Raw and renderable code blocks can be parsed")]
    public void Case3()
    {
        const string template = "raw {{ code }} string";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        Assert.Collection(
            result,
            item =>
            {
                var rawText = Assert.IsType<Syntax.RawText>(item);
                Assert.Equal("raw ", rawText.Value);
            },
            item =>
            {
                var renderableExpression = Assert.IsType<Syntax.RenderableExpression>(item);
                Assert.Equal("code", renderableExpression.Value);
            },
            item =>
            {
                var rawText = Assert.IsType<Syntax.RawText>(item);
                Assert.Equal(" string", rawText.Value);
            }
        );
    }

    [Fact(DisplayName = "An if statement without else can be parsed")]
    public void Case4()
    {
        const string template = "{{ if condition }} test {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var item = Assert.Single(result);
        var ifStatement = Assert.IsType<Syntax.IfStatement>(item);
        Assert.Equal("condition", ifStatement.Condition);
        var expression = Assert.Single(ifStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" test ", rawText.Value);
    }

    [Fact(DisplayName = "An if statement with else can be parsed")]
    public void Case5()
    {
        const string template = "{{ if condition }} test {{ else }} test2 {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        Assert.Collection(
            result,
            item =>
            {
                var ifStatement = Assert.IsType<Syntax.IfStatement>(item);
                Assert.Equal("condition", ifStatement.Condition);
                var expression = Assert.Single(ifStatement.Expressions);
                var rawText = Assert.IsType<Syntax.RawText>(expression);
                Assert.Equal(" test ", rawText.Value);
            },
            item =>
            {
                var elseStatement = Assert.IsType<Syntax.ElseStatement>(item);
                var expression = Assert.Single(elseStatement.Expressions);
                var rawText = Assert.IsType<Syntax.RawText>(expression);
                Assert.Equal(" test2 ", rawText.Value);
            }
        );
    }

    [Fact(DisplayName = "An if statement with multiple conditions can be parsed")]
    public void Case6()
    {
        const string template =
            "{{ if condition1 }} test {{ else if condition2 }} test2 {{ else }} test3 {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        Assert.Collection(
            result,
            item =>
            {
                var ifStatement = Assert.IsType<Syntax.IfStatement>(item);
                Assert.Equal("condition1", ifStatement.Condition);
                var expression = Assert.Single(ifStatement.Expressions);
                var rawText = Assert.IsType<Syntax.RawText>(expression);
                Assert.Equal(" test ", rawText.Value);
            },
            item =>
            {
                var elseIfStatement = Assert.IsType<Syntax.ElseIfStatement>(item);
                Assert.Equal("condition2", elseIfStatement.Condition);
                var expression = Assert.Single(elseIfStatement.Expressions);
                var rawText = Assert.IsType<Syntax.RawText>(expression);
                Assert.Equal(" test2 ", rawText.Value);
            },
            item =>
            {
                var elseStatement = Assert.IsType<Syntax.ElseStatement>(item);
                var expression = Assert.Single(elseStatement.Expressions);
                var rawText = Assert.IsType<Syntax.RawText>(expression);
                Assert.Equal(" test3 ", rawText.Value);
            }
        );
    }

    [Fact(DisplayName = "An if statement with multiple conditions and no else can be parsed")]
    public void Case7()
    {
        const string template = "{{ if condition1 }} test {{ else if condition2 }} test2 {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        Assert.Collection(
            result,
            item =>
            {
                var ifStatement = Assert.IsType<Syntax.IfStatement>(item);
                Assert.Equal("condition1", ifStatement.Condition);
                var expression = Assert.Single(ifStatement.Expressions);
                var rawText = Assert.IsType<Syntax.RawText>(expression);
                Assert.Equal(" test ", rawText.Value);
            },
            item =>
            {
                var elseIfStatement = Assert.IsType<Syntax.ElseIfStatement>(item);
                Assert.Equal("condition2", elseIfStatement.Condition);
                var expression = Assert.Single(elseIfStatement.Expressions);
                var rawText = Assert.IsType<Syntax.RawText>(expression);
                Assert.Equal(" test2 ", rawText.Value);
            }
        );
    }

    [Fact(DisplayName = "An invalid if statement will throw an exception (else too short)")]
    public void Case8()
    {
        const string template = "{{ if condition1 }} test {{ else if }} test2 {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var exception = Assert.Throws<ParseException>(() => TemplateParser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 0:36 (CodeExit): else if statement condition not found (value: '}}')",
            exception.Message
        );
    }

    [Fact(DisplayName = "An invalid if statement will throw an exception (else missing if)")]
    public void Case9()
    {
        const string template =
            "{{ if condition1 }} test {{ else invalid condition }} test2 {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var exception = Assert.Throws<ParseException>(() => TemplateParser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 0:33 (Identifier): else statement must be followed by if statement (value: 'invalid')",
            exception.Message
        );
    }

    [Fact(DisplayName = "An invalid if statement will throw an exception (if too short)")]
    public void Case10()
    {
        const string template = "{{ if }} test {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var exception = Assert.Throws<ParseException>(() => TemplateParser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 0:6 (CodeExit): if statement condition not found (value: '}}')",
            exception.Message
        );
    }

    [Fact(DisplayName = "An invalid if statement will throw an exception (end missing)")]
    public void Case11()
    {
        const string template = "{{ if condition1 }} test {{ else if condition2 }} test2 ";
        var tokens = new Lexer(template).ToArray();
        var exception = Assert.Throws<ParseException>(() => TemplateParser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 0:49 (Raw): else or end not found (value: ' test2 ')",
            exception.Message
        );
    }

    [Fact(DisplayName = "An if statement can be nested")]
    public void Case12()
    {
        const string template = "{{ if condition1 }}{{ if condition2 }} test {{ end }}{{ end }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var item = Assert.Single(result);
        var ifStatement = Assert.IsType<Syntax.IfStatement>(item);
        Assert.Equal("condition1", ifStatement.Condition);
        var innerIfStatement = Assert.IsType<Syntax.IfStatement>(ifStatement.Expressions[0]);
        Assert.Equal("condition2", innerIfStatement.Condition);
        var expression = Assert.Single(innerIfStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" test ", rawText.Value);
    }

    [Fact(DisplayName = "A for statement can be parsed")]
    public void Case13()
    {
        const string template = "{{ for condition }} test {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var item = Assert.Single(result);
        var forStatement = Assert.IsType<Syntax.ForStatement>(item);
        Assert.Equal("condition", forStatement.Condition);
        var expression = Assert.Single(forStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" test ", rawText.Value);
    }

    [Fact(DisplayName = "A invalid for statement will throw an exception (for missing condition)")]
    public void Case14()
    {
        const string template = "{{ for }} test {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var exception = Assert.Throws<ParseException>(() => TemplateParser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 0:7 (CodeExit): for statement condition not found (value: '}}')",
            exception.Message
        );
    }

    [Fact(DisplayName = "A foreach statement can be parsed")]
    public void Case15()
    {
        const string template = "{{ foreach condition }} test {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var item = Assert.Single(result);
        var foreachStatement = Assert.IsType<Syntax.ForeachStatement>(item);
        Assert.Equal("condition", foreachStatement.Condition);
        var expression = Assert.Single(foreachStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" test ", rawText.Value);
    }

    [Fact(
        DisplayName = "A invalid foreach statement will throw an exception (foreach missing condition)"
    )]
    public void Case16()
    {
        const string template = "{{ foreach }} test {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var exception = Assert.Throws<ParseException>(() => TemplateParser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 0:11 (CodeExit): foreach statement condition not found (value: '}}')",
            exception.Message
        );
    }

    [Fact(DisplayName = "A while statement can be parsed")]
    public void Case17()
    {
        const string template = "{{ while condition }} test {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var item = Assert.Single(result);
        var whileStatement = Assert.IsType<Syntax.WhileStatement>(item);
        Assert.Equal("condition", whileStatement.Condition);
        var expression = Assert.Single(whileStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" test ", rawText.Value);
    }

    [Fact(
        DisplayName = "A invalid while statement will throw an exception (while missing condition)"
    )]
    public void Case18()
    {
        const string template = "{{ while }} test {{ end }}";
        var tokens = new Lexer(template).ToArray();
        var exception = Assert.Throws<ParseException>(() => TemplateParser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 0:9 (CodeExit): while statement condition not found (value: '}}')",
            exception.Message
        );
    }

    [Fact(DisplayName = "A continue statement can be parsed")]
    public void Case19()
    {
        const string template = "{{ continue }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var item = Assert.Single(result);
        Assert.IsType<Syntax.ContinueStatement>(item);
    }

    [Fact(DisplayName = "A break statement can be parsed")]
    public void Case20()
    {
        const string template = "{{ break }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var item = Assert.Single(result);
        Assert.IsType<Syntax.BreakStatement>(item);
    }

    [Fact(DisplayName = "A invalid break will throw an exception (invalid token)")]
    public void Case21()
    {
        const string template = "{{ break invalid }}";
        var tokens = new Lexer(template).ToArray();
        var exception = Assert.Throws<ParseException>(() => TemplateParser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 0:9 (Identifier): Expected only keyword 'break' (value: 'invalid')",
            exception.Message
        );
    }

    [Fact(DisplayName = "A while with continue and break can be parsed")]
    public void Case22()
    {
        const string template = """
            {{ while condition }} 
            {{ if i == 0 }}
            {{ continue }}
            {{ end }}

            The value is {{i}}
            {{ end }}
            """;
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var whileStatement = Assert.IsType<Syntax.WhileStatement>(Assert.Single(result));
        Assert.Equal("condition", whileStatement.Condition);
        Assert.Collection(
            whileStatement.Expressions,
            item =>
            {
                var ifStatement = Assert.IsType<Syntax.IfStatement>(item);
                Assert.Equal("i == 0", ifStatement.Condition);
                var expression = Assert.Single(ifStatement.Expressions);
                Assert.IsType<Syntax.ContinueStatement>(expression);
            },
            item =>
            {
                var rawText = Assert.IsType<Syntax.RawText>(item);
                Assert.Equal("\r\n\r\nThe value is ", rawText.Value);
            },
            item =>
            {
                var renderableExpression = Assert.IsType<Syntax.RenderableExpression>(item);
                Assert.Equal("i", renderableExpression.Value);
            },
            item =>
            {
                var rawText = Assert.IsType<Syntax.RawText>(item);
                Assert.Equal("\r\n", rawText.Value);
            }
        );
    }

    [Fact(DisplayName = "A return statement can be parsed")]
    public void Case23()
    {
        const string template = "{{ return }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var item = Assert.Single(result);
        Assert.IsType<Syntax.ReturnStatement>(item);
    }

    [Fact(DisplayName = "A invalid return will throw an exception (invalid token)")]
    public void Case24()
    {
        const string template = "{{ return invalid }}";
        var tokens = new Lexer(template).ToArray();
        var exception = Assert.Throws<ParseException>(() => TemplateParser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 0:10 (Identifier): Expected only keyword 'return' (value: 'invalid')",
            exception.Message
        );
    }

    [Fact(DisplayName = "Extra newline raw text is removed after code blocks")]
    public void Case25()
    {
        const string template = """
            {{var a = 1}}
            {{break}}
            {{continue}}
            {{return}}
            """;
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        Assert.Collection(
            result,
            item =>
            {
                var renderableExpression = Assert.IsType<Syntax.VarStatement>(item);
                Assert.Equal("a = 1", renderableExpression.Assignment);
            },
            item => Assert.IsType<Syntax.BreakStatement>(item),
            item => Assert.IsType<Syntax.ContinueStatement>(item),
            item => Assert.IsType<Syntax.ReturnStatement>(item)
        );
    }

    [Fact(DisplayName = "A call statement can be parsed")]
    public void Case26()
    {
        const string template = "{{ call method(param1, param2) }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        var item = Assert.Single(result);
        var callStatement = Assert.IsType<Syntax.CallStatement>(item);
        Assert.Equal("method", callStatement.Name);
        Assert.Equal("param1, param2", callStatement.Parameters);
        Assert.Equal(string.Empty, callStatement.LeadingWhitespace);
    }

    [Fact(DisplayName = "A invalid call statement will throw an exception (invalid token)")]
    public void Case27()
    {
        const string template = "{{ call method(param1, param2) invalid }}";
        var tokens = new Lexer(template).ToArray();
        var exception = Assert.Throws<ParseException>(() => TemplateParser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 0:31 (Identifier): Invalid call statement. Expected format: 'MethodName(...)'. (value: 'invalid')",
            exception.Message
        );
    }

    [Fact(DisplayName = "A call statement with leading whitespace can be parsed")]
    public void Case28()
    {
        const string template =
            "some text \n some other text \n\n       {{ call method(param1, param2) }}";
        var tokens = new Lexer(template).ToArray();
        var result = TemplateParser.Parse(tokens, template);

        Assert.Collection(
            result,
            item =>
            {
                var rawText = Assert.IsType<Syntax.RawText>(item);
                Assert.Equal("some text \n some other text \n\n       ", rawText.Value);
            },
            item =>
            {
                var callStatement = Assert.IsType<Syntax.CallStatement>(item);
                Assert.Equal("method", callStatement.Name);
                Assert.Equal("param1, param2", callStatement.Parameters);
                Assert.Equal("       ", callStatement.LeadingWhitespace);
            }
        );
    }
}
