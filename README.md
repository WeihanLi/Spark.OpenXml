# Spark.OpenXml

See [WeihanLi.Npoi Parity](docs/articles/npoi-parity.md) for migration guidance and the current support inventory.

`Spark.OpenXml` is a `DocumentFormat.OpenXml` based Excel and CSV import/export library for .NET.

This package is the Open XML rewrite of `Spark.OpenXml`. It keeps the high-level import/export, mapping, formatter, validation, template, multi-sheet, and CSV APIs, and removes the public NPOI workbook/sheet/row/cell APIs that do not map cleanly to Open XML SDK.

## Supported

- `.xlsx` import/export
- CSV import/export
- Entity list import/export
- Attribute mapping and Fluent API mapping
- Input/output formatters
- Import validation
- Sheet header/start row/start column/end column settings
- Multi-sheet export
- Basic freeze panes, column width, and auto filter metadata
- Template placeholder export from `.xlsx` templates

## Install

```sh
dotnet add package Spark.OpenXml
```

## Export Entities

```csharp
using Spark.OpenXml;

var users = new[]
{
    new User { Id = 1, Name = "Alice" },
    new User { Id = 2, Name = "Bob" }
};

users.ToExcelFile("users.xlsx");

public sealed class User
{
    [Column("Id")]
    public int Id { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;
}
```

## Import Entities

```csharp
using Spark.OpenXml;

var users = ExcelHelper.ToEntityList<User>("users.xlsx");
```

## Mapping

```csharp
using Spark.OpenXml;

FluentSettings.For<User>(settings =>
{
    settings.HasSheetConfiguration(sheet =>
    {
        sheet.SheetName = "Users";
        sheet.StartRowIndex = 0;
        sheet.StartColumnIndex = 0;
    });

    settings.Property(user => user.Name)
        .HasColumnTitle("User Name")
        .HasOutputFormatter(name => name?.ToString()?.Trim());
});
```

## Template Export

Template export works with `.xlsx` templates and replaces placeholders such as `{{Name}}`.

```csharp
using Spark.OpenXml;

users.ToExcelFileByTemplate("template.xlsx", "users.xlsx", extraData: new
{
    Title = "User Export"
});
```

## CSV

CSV APIs are still available and are independent of the Open XML workbook implementation.

```csharp
using Spark.OpenXml;

users.ToCsvFile("users.csv");
var imported = CsvHelper.ToEntityList<User>("users.csv");
```
