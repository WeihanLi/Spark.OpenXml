# 快速开始

安装包：

```sh
dotnet add package Spark.OpenXml
```

导入和导出实体：

```csharp
using Spark.OpenXml;

var users = new[]
{
    new User { Id = 1, Name = "Alice" },
    new User { Id = 2, Name = "Bob" }
};

users.ToExcelFile("users.xlsx");

var imported = ExcelHelper.ToEntityList<User>("users.xlsx");

public sealed class User
{
    [Column("Id")]
    public int Id { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;
}
```

Excel 文件仅支持 `.xlsx`。CSV API 仍然可以通过 `CsvHelper` 和 CSV 扩展方法使用。
