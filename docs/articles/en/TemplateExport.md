# Template Export

Template export supports `.xlsx` template files and placeholder replacement.

```csharp
using Spark.OpenXml;

users.ToExcelFileByTemplate("template.xlsx", "users.xlsx", extraData: new
{
    Title = "User Export"
});
```

Template overloads accept a template path, byte array, or stream. NPOI workbook and sheet template overloads were removed.
