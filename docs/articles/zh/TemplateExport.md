# 模板导出

模板导出支持 `.xlsx` 模板文件和占位符替换。

```csharp
using Spark.OpenXml;

users.ToExcelFileByTemplate("template.xlsx", "users.xlsx", extraData: new
{
    Title = "User Export"
});
```

模板重载支持模板路径、字节数组或流。基于 NPOI workbook 和 sheet 的模板重载已移除。
