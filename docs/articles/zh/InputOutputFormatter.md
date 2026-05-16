# 输入输出格式化

输入和输出格式化仍然可以通过 Attribute 和 Fluent API 配置。

```csharp
using Spark.OpenXml;

FluentSettings.For<User>(settings =>
{
    settings.Property(x => x.Name)
        .HasColumnTitle("User Name")
        .HasOutputFormatter(value => value?.ToString()?.Trim())
        .HasInputFormatter(value => value?.ToString()?.Trim());
});
```

格式化器处理的是值，不再接收 OpenXML 或 NPOI 单元格对象。
