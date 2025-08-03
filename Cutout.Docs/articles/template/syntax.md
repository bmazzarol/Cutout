# Template Definition

Cutout templates use a syntax inspired
by [Liquid](https://shopify.github.io/liquid/), but support C# expressions
instead of the custom language that Liquid uses. This allows you to write
templates that are both powerful and easy to read, while leveraging the full
capabilities of C#.

## Syntax Overview

* **Literals:** Any text outside of `{{ ... }}` or `{% ... %}` is treated as a
  string literal and written directly to the output.
* **Expressions:** Use `{{ ... }}` to insert the value of any valid C#
  expression.
***Blocks:** Use `{% ... %}` for control flow (e.g., if, for, foreach, while)
  and function calls.
***Whitespace Control:** Whitespace can be managed using the `-` character,
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

This allows for fine-grained control over the formatting of generated code,
making it easier to maintain correct indentation and spacing.

## Deviations from Liquid Standard
<!-- markdownlint-disable MD013 -->
| Feature            | Cutout Implementation                           | Liquid Standard                                   |
|--------------------|-------------------------------------------------|---------------------------------------------------|
| End Block          | Single `{% end %}` for all blocks               | Specific `{% endif %}`, `{% endfor %}` etc.       |
| Expressions        | Must be valid C# expressions                    | Liquid expressions                                |
| Function Calls     | `{% call Function(args) %}`                     | Not supported in Liquid                           |
| Loop Syntax        | C# style (`for`, `foreach`, `while`)            | Liquid style (`for ... in ...`)                   |
| Whitespace Control | Same as Liquid (`-` modifier)                   | Liquid whitespace control                         |
| Conditionals       | Only one conditional statement (`if`) supported | Multiple conditional types (`if`, `unless`, etc.) |
