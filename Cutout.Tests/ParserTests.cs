using Cutout.Exceptions;

namespace Cutout.Tests;

public class ParserTests
{
    [Fact(DisplayName = "A raw string can be parsed")]
    public void Case1()
    {
        const string template = "raw string";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var single = Assert.Single(result);
        var raw = Assert.IsType<Syntax.RawText>(single);
        Assert.Collection(
            raw.Value,
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(token.ToSpan(template), "raw");
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(token.ToSpan(template), "string");
            }
        );
    }

    [Fact(DisplayName = "A renderable code block can be parsed")]
    public void Case2()
    {
        const string template = "{{ code }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var item = Assert.Single(result);
        var renderableExpression = Assert.IsType<Syntax.RenderableExpression>(item);
        Assert.Collection(
            renderableExpression.Value,
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(token.ToSpan(template), "code");
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            }
        );
    }

    [Fact(DisplayName = "Raw and renderable code blocks can be parsed")]
    public void Case3()
    {
        const string template = "raw {{ code }} string";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        Assert.Collection(
            result,
            item =>
            {
                var rawText = Assert.IsType<Syntax.RawText>(item);
                Assert.Equal("raw ", rawText.Value.ToSpan(template));
            },
            item =>
            {
                var renderableExpression = Assert.IsType<Syntax.RenderableExpression>(item);
                Assert.Equal(" code ", renderableExpression.Value.ToSpan(template));
            },
            item =>
            {
                var rawText = Assert.IsType<Syntax.RawText>(item);
                Assert.Equal(" string", rawText.Value.ToSpan(template));
            }
        );
    }

    [Fact(DisplayName = "An empty template can be parsed")]
    public void Case4()
    {
        const string template = "";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        Assert.Empty(result);
    }

    [Fact(DisplayName = "Code blocks must have balanced braces")]
    public void Case5()
    {
        const string template = "{{ code";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): Code exit token not found (value: 'code')",
            exception.Message
        );
        Assert.Equal("code", exception.Token.ToSpan(template).ToString());
    }

    [Fact(DisplayName = "Code blocks cannot be empty")]
    public void Case6()
    {
        const string template = "{{ }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:3 (Whitespace): Code block is empty (value: ' ')",
            exception.Message
        );
        Assert.Equal(TokenType.Whitespace, exception.Token.Type);
        Assert.Equal(" ", exception.Value);
    }

    [Fact(DisplayName = "Nested code blocks are not allowed")]
    public void Case7()
    {
        const string template = "{{ {{ nested }} }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (CodeEnter): Nested code blocks are not allowed (value: '{{')",
            exception.Message
        );
        Assert.Equal(TokenType.CodeEnter, exception.Token.Type);
        Assert.Equal("{{", exception.Value);
    }

    [Fact(DisplayName = "A break statement can be parsed")]
    public void Case8()
    {
        const string template = "{{ break }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var item = Assert.Single(result);
        Assert.IsType<Syntax.BreakStatement>(item);
    }

    [Fact(DisplayName = "A continue statement can be parsed")]
    public void Case9()
    {
        const string template = "{{ continue }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var item = Assert.Single(result);
        Assert.IsType<Syntax.ContinueStatement>(item);
    }

    [Fact(DisplayName = "A return statement can be parsed")]
    public void Case10()
    {
        const string template = "{{ return }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var item = Assert.Single(result);
        Assert.IsType<Syntax.ReturnStatement>(item);
    }

    [Fact(DisplayName = "A var statement can be parsed")]
    public void Case11()
    {
        const string template = "{{ var x = 42 }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var item = Assert.Single(result);
        var varDeclaration = Assert.IsType<Syntax.VarStatement>(item);
        Assert.Collection(
            varDeclaration.Assignment,
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(token.ToSpan(template), "x");
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(token.ToSpan(template), "=");
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(token.ToSpan(template), "42");
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            }
        );
    }

    [Fact(DisplayName = "A var statement without an expression throws an error")]
    public void Case12()
    {
        const string template = "{{ var }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ var }} declaration requires an assignment expression (value: 'var')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("var", exception.Value);
    }

    [Fact(DisplayName = "A call statement can be parsed")]
    public void Case13()
    {
        const string template = "{{ call function(arg1, arg2) }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var item = Assert.Single(result);
        var callStatement = Assert.IsType<Syntax.CallStatement>(item);
        Assert.Equal("function", callStatement.Name);
        Assert.Collection(
            callStatement.Parameters,
            param =>
            {
                Assert.Equal("arg1", param);
            },
            param =>
            {
                Assert.Equal("arg2", param);
            }
        );
    }

    [Fact(DisplayName = "A call statement can be parsed (no arguments)")]
    public void Case13b()
    {
        const string template = "{{ call function() }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var item = Assert.Single(result);
        var callStatement = Assert.IsType<Syntax.CallStatement>(item);
        Assert.Equal("function", callStatement.Name);
        Assert.Empty(callStatement.Parameters);
    }

    [Fact(DisplayName = "A call statement function part throws an error")]
    public void Case14()
    {
        const string template = "{{ call }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ call }} statement requires parameters (value: 'call')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("call", exception.Value);
    }

    [Fact(DisplayName = "A call statement without parentheses throws an error")]
    public void Case15()
    {
        const string template = "{{ call function }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ call }} statement requires a function name and () with optional parameters (value: 'call')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("call", exception.Value);
    }

    [Fact(DisplayName = "A call statement without parentheses throws an error (first only)")]
    public void Case15b()
    {
        const string template = "{{ call function( }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ call }} statement requires a function name and () with optional parameters (value: 'call')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("call", exception.Value);
    }

    [Fact(DisplayName = "A call statement without parentheses throws an error (last only)")]
    public void Case15c()
    {
        const string template = "{{ call function) }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ call }} statement requires a function name and () with optional parameters (value: 'call')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("call", exception.Value);
    }

    [Fact(DisplayName = "A call statement with only parentheses throws an error")]
    public void Case16()
    {
        const string template = "{{ call () }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ call }} statement requires a function name and () with optional parameters (value: 'call')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("call", exception.Value);
    }

    [Fact(DisplayName = "A while statement can be parsed")]
    public void Case17()
    {
        const string template = "{{ while condition }} some code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var item = Assert.Single(result);
        var whileStatement = Assert.IsType<Syntax.WhileStatement>(item);
        Assert.Collection(
            whileStatement.Condition,
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(token.ToSpan(template), "condition");
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            }
        );

        var expression = Assert.Single(whileStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Collection(
            rawText.Value,
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(token.ToSpan(template), "some");
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            },
            token =>
            {
                Assert.Equal(TokenType.Raw, token.Type);
                Assert.Equal(token.ToSpan(template), "code");
            },
            token =>
            {
                Assert.Equal(TokenType.Whitespace, token.Type);
                Assert.Equal(token.ToSpan(template), " ");
            }
        );
    }

    [Fact(DisplayName = "A while statement without a condition throws an error")]
    public void Case18()
    {
        const string template = "{{ while }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ while }} statement requires a condition (value: 'while')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("while", exception.Value);
    }

    [Fact(DisplayName = "A while statement without an end token throws an error")]
    public void Case19()
    {
        const string template = "{{ while condition }} some code";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at end of file: Unexpected end of file, expected a {{ end }} block",
            exception.Message
        );
        Assert.Equal(TokenType.Eof, exception.Token.Type);
        Assert.Equal(string.Empty, exception.Value);
    }

    [Fact(DisplayName = "A for statement can be parsed")]
    public void Case20()
    {
        const string template = "{{ for i = 0; i < items.Length; i++ }} some code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var item = Assert.Single(result);
        var forStatement = Assert.IsType<Syntax.ForStatement>(item);
        Assert.Equal(" i = 0; i < items.Length; i++ ", forStatement.Condition.ToSpan(template));

        var expression = Assert.Single(forStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" some code ", rawText.Value.ToSpan(template));
    }

    [Fact(DisplayName = "A foreach statement can be parsed")]
    public void Case21()
    {
        const string template = "{{ foreach item in items }} some code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var item = Assert.Single(result);
        var foreachStatement = Assert.IsType<Syntax.ForeachStatement>(item);
        Assert.Equal(" item in items ", foreachStatement.Condition.ToSpan(template));

        var expression = Assert.Single(foreachStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" some code ", rawText.Value.ToSpan(template));
    }

    [Fact(DisplayName = "An if statement can be parsed")]
    public void Case22()
    {
        const string template = "{{ if condition }} some code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var item = Assert.Single(result);
        var ifStatement = Assert.IsType<Syntax.IfStatement>(item);
        Assert.Equal(" condition ", ifStatement.Condition.ToSpan(template));

        var expression = Assert.Single(ifStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" some code ", rawText.Value.ToSpan(template));
    }

    [Fact(DisplayName = "An if statement without a condition throws an error")]
    public void Case23()
    {
        const string template = "{{ if }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ if }} statement requires a condition (value: 'if')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("if", exception.Value);
    }

    [Fact(DisplayName = "An if statement without an end token throws an error")]
    public void Case24()
    {
        const string template = "{{ if condition }} some code";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at end of file: Unexpected end of file, expected a {{ end }} block",
            exception.Message
        );
        Assert.Equal(TokenType.Eof, exception.Token.Type);
        Assert.Equal(string.Empty, exception.Value);
    }

    [Fact(DisplayName = "An else without an if statement throws an error")]
    public void Case25()
    {
        const string template = "{{ else }} some code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ else }} found but not expected (value: 'else')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("else", exception.Value);
    }

    [Fact(DisplayName = "An else if without an if statement throws an error")]
    public void Case26()
    {
        const string template = "{{ elseif condition }} some code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ elseif }} found but not expected (value: 'elseif')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("elseif", exception.Value);
    }

    [Fact(DisplayName = "An else statement can be parsed")]
    public void Case27()
    {
        const string template = "{{ if condition }} some code {{ else }} other code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var single = Assert.Single(result);
        var ifStatement = Assert.IsType<Syntax.IfStatement>(single);
        Assert.Equal(" condition ", ifStatement.Condition.ToSpan(template));

        var expression = Assert.Single(ifStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" some code ", rawText.Value.ToSpan(template));

        Assert.NotNull(ifStatement.Else);
        expression = Assert.Single(ifStatement.Else.Expressions);
        rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" other code ", rawText.Value.ToSpan(template));
    }

    [Fact(DisplayName = "An else after an else statement throws an error")]
    public void Case28()
    {
        const string template =
            "{{ if condition }} some code {{ else }} other code {{ else }} more code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:55 (Raw): Only one {{ else }} is allowed within an {{ if }} statement (value: 'else')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("else", exception.Value);
    }

    [Fact(DisplayName = "An else if can be parsed")]
    public void Case29()
    {
        const string template =
            "{{ if condition }} some code {{ elseif otherCondition }} other code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var single = Assert.Single(result);
        var ifStatement = Assert.IsType<Syntax.IfStatement>(single);
        Assert.Equal(" condition ", ifStatement.Condition.ToSpan(template));
        var expression = Assert.Single(ifStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" some code ", rawText.Value.ToSpan(template));
        Assert.NotNull(ifStatement.ElseIfs);
        single = Assert.Single(ifStatement.ElseIfs);
        var elseIfStatement = Assert.IsType<Syntax.ElseIfStatement>(single);
        Assert.Equal(" otherCondition ", elseIfStatement.Condition.ToSpan(template));
        expression = Assert.Single(elseIfStatement.Expressions);
        rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" other code ", rawText.Value.ToSpan(template));
    }

    [Fact(DisplayName = "An else if after an else statement throws an error")]
    public void Case30()
    {
        const string template =
            "{{ if condition }} some code {{ else }} other code {{ elseif anotherCondition }} more code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:55 (Raw): Cannot have {{ elseif }} after {{ else }} in an {{ if }} statement (value: 'elseif')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("elseif", exception.Value);
    }

    [Fact(DisplayName = "A if/else if/else statement can be parsed")]
    public void Case32()
    {
        const string template =
            "{{ if condition }} some code {{ elseif otherCondition }} other code {{ else }} final code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var single = Assert.Single(result);
        var ifStatement = Assert.IsType<Syntax.IfStatement>(single);
        Assert.Equal(" condition ", ifStatement.Condition.ToSpan(template));
        var expression = Assert.Single(ifStatement.Expressions);
        var rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" some code ", rawText.Value.ToSpan(template));
        Assert.NotNull(ifStatement.ElseIfs);
        single = Assert.Single(ifStatement.ElseIfs);
        var elseIfStatement = Assert.IsType<Syntax.ElseIfStatement>(single);
        Assert.Equal(" otherCondition ", elseIfStatement.Condition.ToSpan(template));
        expression = Assert.Single(elseIfStatement.Expressions);
        rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" other code ", rawText.Value.ToSpan(template));
        Assert.NotNull(ifStatement.Else);
        expression = Assert.Single(ifStatement.Else.Expressions);
        rawText = Assert.IsType<Syntax.RawText>(expression);
        Assert.Equal(" final code ", rawText.Value.ToSpan(template));
    }

    [Fact(DisplayName = "A end statement without a block throws an error")]
    public void Case33()
    {
        const string template = "{{ end }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ end }} found but not expected (value: 'end')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("end", exception.Value);
    }

    [Fact(DisplayName = "A end statement with extra tokens throws an error")]
    public void Case34()
    {
        const string template = "{{ end extra }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:4 (Raw): {{ end }} statement should only contain the identifier (value: 'end')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("end", exception.Value);
    }

    [Fact(DisplayName = "A else statement with a condition throws an error")]
    public void Case36()
    {
        const string template = "{{ if test }} something {{ else condition }} some code {{ end }}";
        var tokens = Lexer.Tokenize(template);
        var exception = Assert.Throws<ParseException>(() => Parser.Parse(tokens, template));
        Assert.Equal(
            "Parse error at 1:28 (Raw): {{ else }} statement should only contain the identifier (value: 'else')",
            exception.Message
        );
        Assert.Equal(TokenType.Raw, exception.Token.Type);
        Assert.Equal("else", exception.Value);
    }

    [Fact(DisplayName = "A if statement can be nested inside an if statement")]
    public void Case37()
    {
        const string template =
            "{{ if condition1 }} some code {{ if condition2 }} nested code {{ end }}{{ end }} some more code";
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        Assert.Equal(2, result.Count);
        var ifStatement = Assert.IsType<Syntax.IfStatement>(result[0]);
        Assert.Equal(" condition1 ", ifStatement.Condition.ToSpan(template));

        Assert.Equal(2, ifStatement.Expressions.Count);
        Assert.Collection(
            ifStatement.Expressions,
            expression =>
            {
                var rawText = Assert.IsType<Syntax.RawText>(expression);
                Assert.Equal(" some code ", rawText.Value.ToSpan(template));
            },
            expression =>
            {
                var nestedIfStatement = Assert.IsType<Syntax.IfStatement>(expression);
                Assert.Equal(" condition2 ", nestedIfStatement.Condition.ToSpan(template));
                Assert.Single(nestedIfStatement.Expressions);
                var nestedRawText = Assert.IsType<Syntax.RawText>(nestedIfStatement.Expressions[0]);
                Assert.Equal(" nested code ", nestedRawText.Value.ToSpan(template));
            }
        );

        var rawText = Assert.IsType<Syntax.RawText>(result[1]);
        Assert.Equal(" some more code", rawText.Value.ToSpan(template));
    }

    [Fact(DisplayName = "A if else statement can be nested inside an if else statement")]
    public void Case39()
    {
        const string template = """
            {{ if condition1 }} 
                nested
                {{ if condition2 }} 
                    test 
                {{ else }}  
                    other 
                {{ end }} 
                code 
            {{ else }} 
                final 
                {{ if test }} 
                    test 
                {{ else }} 
                    other 
                {{ end }} 
                code 
            {{ end }}
            """;
        var tokens = Lexer.Tokenize(template);
        var result = Parser.Parse(tokens, template);

        var single = Assert.Single(result);
        var ifStatement = Assert.IsType<Syntax.IfStatement>(single);
        Assert.Equal(" condition1 ", ifStatement.Condition.ToSpan(template));
        Assert.Equal(3, ifStatement.Expressions.Count);
        var nestedIf = Assert.IsType<Syntax.IfStatement>(ifStatement.Expressions[1]);
        Assert.Equal(" condition2 ", nestedIf.Condition.ToSpan(template));
        Assert.Single(nestedIf.Expressions);
        Assert.NotNull(nestedIf.Else);
        Assert.Single(nestedIf.Else.Expressions);
        Assert.NotNull(ifStatement.Else);
        Assert.Equal(3, ifStatement.Else.Expressions.Count);
        var elseNestedIf = Assert.IsType<Syntax.IfStatement>(ifStatement.Else.Expressions[1]);
        Assert.Equal(" test ", elseNestedIf.Condition.ToSpan(template));
        Assert.Single(elseNestedIf.Expressions);
        Assert.NotNull(elseNestedIf.Else);
        Assert.Single(elseNestedIf.Else.Expressions);
    }
}
