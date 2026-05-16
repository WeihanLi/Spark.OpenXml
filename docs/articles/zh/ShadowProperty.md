# Shadow Property

Shadow Property 仍然可以通过 Fluent API 配置，用于导出没有 CLR 属性对应的列。

```csharp
using Spark.OpenXml;

FluentSettings.For<User>(settings =>
{
    settings.Property("ExportedAt")
        .HasColumnTitle("Exported At")
        .HasOutputFormatter(_ => DateTimeOffset.UtcNow.ToString("O"));
});
```
