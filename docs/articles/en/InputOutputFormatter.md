# Input And Output Formatters

Input and output formatters remain available through attributes and Fluent API configuration.

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

Formatters operate on values, not OpenXML cell objects.
