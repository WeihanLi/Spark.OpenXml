# Shadow Properties

Shadow properties are still available through Fluent API configuration when exported data needs columns that are not backed by CLR properties.

```csharp
using Spark.OpenXml;

FluentSettings.For<User>(settings =>
{
    settings.Property("ExportedAt")
        .HasColumnTitle("Exported At")
        .HasOutputFormatter(_ => DateTimeOffset.UtcNow.ToString("O"));
});
```
