# 多 Sheet 导出

多 Sheet 导出使用高层集合 API。新包不再公开 workbook 或 sheet 对象。

```csharp
using Spark.OpenXml;

var sheets = new[]
{
    new[] { new User { Id = 1, Name = "Alice" } },
    new[] { new User { Id = 2, Name = "Bob" } }
};

sheets.ToExcelFile("users.xlsx");
```

Sheet 名称、表头、起始行、起始列和结束列通过 Attribute 或 Fluent API 配置。
