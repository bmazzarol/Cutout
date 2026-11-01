# Call Statement

`call` invokes another template method. The called method must also carry
`[Cutout.Template]`, and the arguments are ordinary C# expressions.

## Syntax

```c#
{% call MethodName(arg1, arg2) %}
```

## Example

```c#
[Cutout.Template("Some text before {% call Inner(title) %}")]
public static partial void Outer(this StringBuilder builder, string title);

[Cutout.Template("The product title is {{ title }}")]
public static partial void Inner(this StringBuilder builder, string title);
```

With `title` set to `Awesome Shoes` the builder contains,

```text
Some text before The product title is Awesome Shoes
```

## Nested calls

Calls can nest, and the indentation of the surrounding template is preserved,

```c#
[Cutout.Template("""
    This is an example with a call with leading whitespace,
    ```
        {% call Title(product) %}
    ```
    """)]
public static partial void Outer(this StringBuilder builder, Product product);

[Cutout.Template("""
    The title in two calls,
    {{ product.Title }}
    {{ product.Title.ToLowerInvariant() }}
    """)]
public static partial void Title(this StringBuilder builder, Product product);
```

renders,

````text
This is an example with a call with leading whitespace,
```
    The title in two calls,
    Awesome Shoes
    awesome shoes
```
````
