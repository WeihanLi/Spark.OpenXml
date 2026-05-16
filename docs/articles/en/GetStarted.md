# Get Started

Install the package:

```sh
dotnet add package Spark.OpenXml
```

Import/export entities:

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

`Spark.OpenXml` supports `.xlsx` only for Excel files. CSV APIs remain available through `CsvHelper` and CSV extension methods.
