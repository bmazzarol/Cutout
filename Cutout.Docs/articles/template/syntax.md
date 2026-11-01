# Template Definition

Cutout templates use a syntax inspired
by [Liquid](https://shopify.github.io/liquid/), but use C# expressions instead
of the custom language that Liquid uses. The template reads like Liquid and the
expressions behave like C#.

## Syntax Overview

* **Literals:** Any text outside of `{{ ... }}` or `{% ... %}` is treated as a
  string literal and written directly to the output.
* **Expressions:** Use `{{ ... }}` to insert the value of any valid C#
  expression.
* **Blocks:** Use `{% ... %}` for control flow (e.g., if, for, foreach, while)
  and function calls.
* **Whitespace Control:** Whitespace can be managed using the `-` character,
  similar to Liquid. For example, `{%- ... -%}` trims whitespace around the
  block.

### Example

```csharp
private const string Template = """
    {% if name == "Bob" %}
    Hello Bob
    {% else %}
    Hello {{ name }}
    {% end %}
    """;

[Cutout.Template(Template)]
public static partial void MyTemplateMethod(StringBuilder sb, string name);
```

## Whitespace Handling

Cutout supports whitespace control similar to
[Liquid's whitespace basics](https://shopify.github.io/liquid/basics/whitespace/)
:

* `{% ... %}`: Preserves whitespace around the block.
* `{%- ... %}`: Trims whitespace before the block.
* `{% ... -%}`: Trims whitespace after the block.
* `{%- ... -%}`: Trims whitespace both before and after the block.
* The same applies to output tags: `{{ ... }}`, `{{- ... }}`, `{{ ... -}}`,
  `{{- ... -}}`.

This controls the formatting of the generated code, which is what keeps the
indentation of the output correct.

## Deviations from Liquid Standard

| Feature            | Cutout                                   | Liquid                          |
|--------------------|------------------------------------------|---------------------------------|
| End block          | one `{% end %}` closes every block       | `{% endif %}`, `{% endfor %}`   |
| Expressions        | any valid C# expression                  | Liquid's own language           |
| Function calls     | `{% call Function(args) %}`              | not supported                   |
| Loop syntax        | `for`, `foreach`, `while`                | `for ... in ...`                |
| Whitespace control | `-` modifier, as in Liquid               | `-` modifier                    |
| Conditionals       | `if`, `elseif`, `else`                   | also has `unless` and others    |
