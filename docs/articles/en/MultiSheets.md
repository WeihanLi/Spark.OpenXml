# Multi-Sheet Export

Multi-sheet export uses the high-level collection APIs. The package no longer exposes workbook or sheet objects.

```csharp
using Spark.OpenXml;

var sheets = new[]
{
    new[] { new User { Id = 1, Name = "Alice" } },
    new[] { new User { Id = 2, Name = "Bob" } }
};

sheets.ToExcelFile("users.xlsx");
```

Sheet names, headers, and row/column ranges are configured through attributes or Fluent API sheet configuration.
