# File Templates

The templates can also be stored in external files.

To get started, create a '.editorconfig' file in your project.

In this, connect static files to valid methods in your project.

For example, if you have a template called 'my-template.txt',
you can connect it to the 'MyTemplate' method.

First, add the template to the project.

```xml
<Project>
    <ItemGroup>
        <AdditionalFiles Include="my-template.txt" />
    </ItemGroup>
</Project>
```

Create the template method.

```c#
using My.Project;

public static partial class Templates
{
    [Cutout.FileTemplate] // required
    public static partial void MyTemplate(this StringBuilder sb, string name);
}
```

Then link the 2 together in the '.editorconfig' file.

```editorconfig
[my-template.txt]
template_method = My.Project.Templates.MyTemplate
```

Now you can externalize your templates and set your editor of choice up to
enable syntax highlighting and other features.
