// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using System.Data;
using WeihanLi.Common.Models;
using WeihanLi.Common.Services;
using WeihanLi.Extensions;
using Spark.OpenXml.Attributes;
using Spark.OpenXml.Configurations;
using Spark.OpenXml.Test.Models;
using Xunit;

namespace Spark.OpenXml.Test;

public class ExcelTest
{
    [Theory]
    [ClassData(typeof(ExcelFormatData))]
    public void BasicImportExportTest(ExcelFormat excelFormat)
    {
        var list = new List<Notice?>();
        for (var i = 0; i < 10; i++)
        {
            list.Add(new Notice
            {
                Id = i + 1,
                Content = $"content_{i}",
                Title = $"title_{i}",
                PublishedAt = DateTime.UtcNow.AddDays(-i),
                Publisher = $"publisher_{i}"
            });
        }
        list.Add(new Notice { Title = "nnnn" });
        list.Add(null);

        var excelBytes = list.ToExcelBytes(excelFormat);
        var importedList = ExcelHelper.ToEntityList<Notice>(excelBytes, excelFormat);

        Assert.Equal(list.Count, importedList.Count);
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i] is null)
            {
                Assert.Null(importedList[i]);
                continue;
            }

            Assert.NotNull(importedList[i]);
            var sourceItem = list[i]!;
            var item = importedList[i]!;
            Assert.Equal(sourceItem.Id, item.Id);
            Assert.Equal(sourceItem.Title, item.Title);
            Assert.Equal(sourceItem.Content, item.Content);
            Assert.Equal(sourceItem.Publisher, item.Publisher);
            Assert.Equal(sourceItem.PublishedAt.ToTimeString(), item.PublishedAt.ToTimeString());
        }
    }

    [Theory]
    [ClassData(typeof(ExcelFormatData))]
    public void FluentFormattersTest(ExcelFormat excelFormat)
    {
        IReadOnlyList<Notice> list = Enumerable.Range(0, 10).Select(i => new Notice
        {
            Id = i + 1,
            Content = $"content_{i}",
            Title = $"title_{i}",
            PublishedAt = DateTime.UtcNow.AddDays(-i),
            Publisher = $"publisher_{i}"
        }).ToArray();

        var settings = FluentSettings.For<Notice>();
        lock (settings)
        {
            settings.Property(x => x.Id)
                .HasColumnOutputFormatter(x => $"{x}_Test")
                .HasColumnInputFormatter(x => Convert.ToInt32(x?.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[0]));

            var excelBytes = list.ToExcelBytes(excelFormat);
            var importedList = ExcelHelper.ToEntityList<Notice>(excelBytes, excelFormat);

            Assert.Equal(list.Count, importedList.Count);
            for (var i = 0; i < list.Count; i++)
            {
                Assert.NotNull(importedList[i]);
                Assert.Equal(list[i].Id, importedList[i]!.Id);
                Assert.Equal(list[i].Title, importedList[i]!.Title);
            }

            settings.Property(x => x.Id)
                .HasColumnOutputFormatter(null)
                .HasColumnInputFormatter(null);
        }
    }

    [Theory]
    [ClassData(typeof(ExcelFormatData))]
    public void DataTableImportExportTest(ExcelFormat excelFormat)
    {
        var dt = new DataTable();
        dt.Columns.AddRange(new[]
        {
            new DataColumn("Name"),
            new DataColumn("Age"),
            new DataColumn("Desc"),
        });
        for (var i = 0; i < 10; i++)
        {
            var row = dt.NewRow();
            row.ItemArray = new object[] { $"Test_{i}", i + 10, $"Desc_{i}" };
            dt.Rows.Add(row);
        }

        var excelBytes = dt.ToExcelBytes(excelFormat);
        var importedData = ExcelHelper.ToDataTable(excelBytes, excelFormat);

        Assert.Equal(dt.Rows.Count, importedData.Rows.Count);
        Assert.Equal(dt.Columns.Count, importedData.Columns.Count);
        for (var i = 0; i < dt.Rows.Count; i++)
        {
            for (var j = 0; j < dt.Columns.Count; j++)
            {
                Assert.Equal(Convert.ToString(dt.Rows[i].ItemArray[j]), Convert.ToString(importedData.Rows[i].ItemArray[j]));
            }
        }
    }

    [Theory]
    [InlineData(@"TestData/EmptyColumns/emptyColumns.xlsx", ExcelFormat.Xlsx)]
    public void DataTableImportExportTestWithFirstColumnsEmpty(string file, ExcelFormat excelFormat)
    {
        var importedData = ExcelHelper.ToDataTable(File.ReadAllBytes(file), excelFormat);
        Assert.Equal(4, importedData.Rows.Count);
        Assert.Equal(4, importedData.Columns.Count);
        Assert.Equal("3", importedData.Rows[0][2]);
        Assert.Equal("1", importedData.Rows[2][0]);
    }

    [Theory]
    [ClassData(typeof(ExcelFormatData))]
    public void SheetNameTest_ToExcelBytes(ExcelFormat excelFormat)
    {
        IReadOnlyList<Notice> list = Enumerable.Range(0, 10).Select(i => new Notice
        {
            Id = i + 1,
            Content = $"content_{i}",
            Title = $"title_{i}",
            PublishedAt = DateTime.UtcNow.AddDays(-i),
            Publisher = $"publisher_{i}"
        }).ToArray();

        var settings = FluentSettings.For<Notice>();
        lock (settings)
        {
            settings.HasSheetSetting(s => s.SheetName = "Test");

            var excelBytes = list.ToExcelBytes(excelFormat);
            Assert.Equal("Test", ExcelHelper.GetSheetNames(excelBytes)[0]);

            settings.HasSheetSetting(s => s.SheetName = "NoticeList");
        }
    }

    [Theory]
    [ClassData(typeof(ExcelFormatData))]
    public void ValidatorTest(ExcelFormat excelFormat)
    {
        var list = new List<Job>
        {
            new()
            {
                Id = 1,
                Name = "test"
            },
            new()
        };

        var bytes = list.ToExcelBytes(excelFormat);
        var result = ExcelHelper.ToEntityListWithValidationResult<Job>(bytes, excelFormat);

        Assert.Equal(list.Count, result.EntityList.Count);
        Assert.Single(result.ValidationResults);
    }

    [Theory]
    [ClassData(typeof(ExcelFormatData))]
    public void ValidatorTest_CustomValidator(ExcelFormat excelFormat)
    {
        var list = new List<Job>
        {
            new()
            {
                Id = 1,
                Name = "test"
            }
        };
        var validator = new DelegateValidator<Job>(_ => new ValidationResult
        {
            Valid = false,
            Errors = new Dictionary<string, string[]> { { "", ["Mock error"] } }
        });

        var bytes = list.ToExcelBytes(excelFormat);
        var result = ExcelHelper.ToEntityListWithValidationResult(bytes, excelFormat, validator: validator);

        Assert.Equal(list.Count, result.EntityList.Count);
        Assert.Single(result.ValidationResults);
    }

    [Theory]
    [ClassData(typeof(ExcelFormatData))]
    public void AttributeColumnRangeImportTest(ExcelFormat excelFormat)
    {
        IReadOnlyList<CellRangeAttributeTest> list = Enumerable.Range(0, 10).Select(i => new CellRangeAttributeTest
        {
            Id = i + 1,
            Description = $"content_{i}",
            Name = $"title_{i}",
        }).ToArray();

        var excelBytes = list.ToExcelBytes(excelFormat);
        var importedList = ExcelHelper.ToEntityList<CellRangeAttributeTest>(excelBytes, excelFormat);

        Assert.Equal(list.Count, importedList.Count);
        for (var i = 0; i < importedList.Count; i++)
        {
            Assert.NotNull(importedList[i]);
            Assert.Equal(list[i].Id, importedList[i]!.Id);
            Assert.Equal(list[i].Name, importedList[i]!.Name);
            Assert.Null(importedList[i]!.Description);
        }
    }

    [Sheet(SheetName = "test", AutoColumnWidthEnabled = true, StartColumnIndex = 0, EndColumnIndex = 1)]
    private class CellRangeAttributeTest
    {
        [Column(Index = 0)]
        public int Id { get; set; }

        [Column(Index = 1)]
        public string? Name { get; set; }

        [Column(Index = 2)]
        public string? Description { get; set; }
    }
}
