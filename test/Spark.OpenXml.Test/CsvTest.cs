// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using System.Text;
using WeihanLi.Extensions;
using Spark.OpenXml.Configurations;
using Spark.OpenXml.Test.Models;
using Xunit;

namespace Spark.OpenXml.Test;

public class CsvTest
{
    public CsvTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Fact]
    public void BasicImportExportTest()
    {
        var list = new List<Notice>();
        for (var i = 0; i < 10; i++)
        {
            list.Add(new Notice()
            {
                Id = i + 1,
                Content = $"content_{i}",
                Title = $"title_{i}",
                PublishedAt = DateTime.UtcNow.AddDays(-i),
                Publisher = $"publisher_{i}"
            });
        }

        list.Add(new Notice()
        {
            Id = 11,
            Content = $"content",
            Title = $"title",
            PublishedAt = DateTime.UtcNow.AddDays(1),
        });
        var noticeSetting = FluentSettings.For<Notice>();
        lock (noticeSetting)
        {
            var csvBytes = list.ToCsvBytes();
            var importedList = CsvHelper.ToEntityList<Notice>(csvBytes);
            Assert.Equal(list.Count, importedList.Count);
            for (var i = 0; i < list.Count; i++)
            {
                Assert.NotNull(importedList[i]);
                var item = importedList[i]!;
                Assert.Equal(list[i].Id, item.Id);
                Assert.Equal(list[i].Title ?? "", item.Title);
                Assert.Equal(list[i].Content ?? "", item.Content);
                Assert.Equal(list[i].Publisher ?? "", item.Publisher);
                Assert.Equal(list[i].PublishedAt.ToTimeString(), item.PublishedAt.ToTimeString());
            }
        }
    }

    [Fact]
    public void ImportWithNotSpecificColumnIndex()
    {
        IReadOnlyList<Notice> list = Enumerable.Range(0, 10).Select(i => new Notice()
        {
            Id = i + 1,
            Content = $"content_{i}",
            Title = $"title_{i}",
            PublishedAt = DateTime.UtcNow.AddDays(-i),
            Publisher = $"publisher_{i}"
        }).ToArray();
        //
        var noticeSetting = FluentSettings.For<Notice>();
        lock (noticeSetting)
        {
            var excelBytes = list.ToCsvBytes();

            noticeSetting.Property(_ => _.Publisher)
                .HasColumnIndex(4);
            noticeSetting.Property(_ => _.PublishedAt)
                .HasColumnIndex(3);

            var importedList = CsvHelper.ToEntityList<Notice>(excelBytes);
            Assert.Equal(list.Count, importedList.Count);
            for (var i = 0; i < list.Count; i++)
            {
                Assert.NotNull(importedList[i]);
                var item = importedList[i]!;
                Assert.Equal(list[i].Id, item.Id);
                Assert.Equal(list[i].Title ?? "", item.Title);
                Assert.Equal(list[i].Content ?? "", item.Content);
                Assert.Equal(list[i].Publisher ?? "", item.Publisher);
                Assert.Equal(list[i].PublishedAt.ToTimeString(), item.PublishedAt.ToTimeString());
            }

            noticeSetting.Property(_ => _.Publisher)
                .HasColumnIndex(3);
            noticeSetting.Property(_ => _.PublishedAt)
                .HasColumnIndex(4);
        }
    }

    [Theory]
    [InlineData("\"XXXXX\"")]
    [InlineData("XXX")]
    [InlineData("\"X,XXX\"")]
    [InlineData("XX\"X")]
    [InlineData("XX\"\"X")]
    [InlineData("\"dd\"\"d,1\"")]
    [InlineData("ddd\nccc")]
    [InlineData("ddd\r\nccc")]
    [InlineData(@"bbb
        ccc")]
    [InlineData("")]
    public void ParseCsvLineTest(string str)
    {
        var data = new object[] { 1, "tom", 33, str };
        var lineData = string.Join(CsvHelper.CsvSeparatorCharacter, data);
        var cols = CsvHelper.ParseLine(lineData);
        Assert.Equal(data.Length, cols.Count);

        for (var i = 0; i < cols.Count; i++)
        {
            Assert.Equal(TrimQuotes(data[i].ToString()), cols[i]);
        }
    }

    [Fact]
    public void GetCsvTextTest_BasicType()
    {
        var text = Enumerable.Range(1, 5)
            .GetCsvText(false);

        var expected = Enumerable.Range(1, 5)
            .StringJoin(Environment.NewLine);
        Assert.Equal(expected, text);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetCsvLines_BasicType(bool includeHeader)
    {
        var option = new CsvOptions() { IncludeHeader = includeHeader };
        var list = new List<int>() { 1, 2, 3 };
        var lines = list.GetCsvLines(option).ToArray();
        Assert.Equal(includeHeader ? list.Count + 1 : list.Count, lines.Length);
        var importedList = CsvHelper.GetEntityList<int>(lines, option);
        Assert.Equal(list.Count, importedList.Count);

        for (var i = 0; i < list.Count; i++)
        {
            Assert.Equal(list[i], importedList[i]);
        }
    }

    [Theory]
    [InlineData("test.csv")]
    [InlineData("/tmp/test.csv")]
    public async Task CsvFileTest(string csvPath)
    {
        var list = new List<Job>() { new Job() { Id = 1, Name = "123" }, new Job() { Id = 2, Name = "234" } };
        Assert.True(list.ToCsvFile(csvPath));

        var importedList = CsvHelper.ToEntityList<Job>(csvPath);
        Assert.Equal(list.Count, importedList.Count);
        for (var i = 0; i < list.Count; i++)
        {
            Assert.Equal(list[i], importedList[i]);
        }

        File.Delete(csvPath);

        await Task.CompletedTask;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetCsvLines_Entity(bool includeHeader)
    {
        var option = new CsvOptions() { IncludeHeader = includeHeader };

        var list = new List<Job>() { new() { Id = 1, Name = "123" }, new() { Id = 2, Name = "234" } };
        var lines = list.GetCsvLines(option).ToArray();
        Assert.Equal(includeHeader ? list.Count + 1 : list.Count, lines.Length);
        var importedList = CsvHelper.GetEntityList<Job>(lines, option);
        Assert.Equal(list.Count, importedList.Count);
        for (var i = 0; i < list.Count; i++)
        {
            Assert.Equal(list[i], importedList[i]);
        }
    }

    [Fact]
    public void GetCsvTextTest_Entity()
    {
        var list = Enumerable.Range(1, 5)
            .Select(i => new Job() { Id = i + 1, Name = "test" }).ToArray();
        var csvText = list.GetCsvText();
        var bytes = csvText.GetBytes();

        var importedList = CsvHelper.ToEntityList<Job>(bytes);
        Assert.Equal(list.Length, importedList.Count);
        for (var i = 0; i < list.Length; i++)
        {
            Assert.True(list[i] == importedList[i]);
        }
    }

    [Fact]
    public void CsvStringListTest()
    {
        var arr = Enumerable.Range(1, 10)
            .Select(x => $"str_{x}")
            .ToArray();

        var csvBytes = arr.ToCsvBytes();
        Assert.NotNull(csvBytes);
        var list = CsvHelper.ToEntityList<string>(csvBytes);
        Assert.Equal(arr.Length, list.Count);
        Assert.True(arr.SequenceEqual(list));
    }

    [Fact]
    public void CsvOptionTest_CustomSeparatorCharacter()
    {
        var list = new Notice[]
        {
            new()
            {
                Id = 1,
                Content = "test",
                Title = "test",
                Publisher = "test",
                PublishedAt = DateTime.Now
            }
        };
        var text = list.GetCsvText();
        var text2 = list.GetCsvText(new CsvOptions() { SeparatorCharacter = '\t' });
        Assert.NotEqual(text, text2);
        Assert.Equal(text, text2.Replace('\t', ','));
    }

    [Fact]
    public void CsvToListEncodingTest()
    {
        var list = new List<TestModel>() { new() { Age = 1, Name = "中华小当家" } };
        var encoding = Encoding.GetEncoding("gb2312");
        var bytes = list.ToCsvBytes(new CsvOptions() { Encoding = encoding });
        var importedList = CsvHelper.ToEntityList<TestModel>(bytes, new CsvOptions() { Encoding = encoding });
        Assert.Equal(list.Count, importedList.Count);
        for (var i = 0; i < list.Count; i++)
        {
            Assert.Equal(list[i], importedList[i]);
        }
    }

    [Fact]
    public void CsvToListEncodingTest_NotTheSameEncoding()
    {
        var list = new List<TestModel>() { new() { Age = 1, Name = "中华小当家" } };
        var encoding = Encoding.GetEncoding("gb2312");
        var bytes = list.ToCsvBytes(new CsvOptions() { Encoding = encoding });
        var importedList = CsvHelper.ToEntityList<TestModel>(bytes);
        Assert.Equal(list.Count, importedList.Count);
        for (var i = 0; i < list.Count; i++)
        {
            Assert.NotEqual(list[i], importedList[i]);
        }
    }

    private static string TrimQuotes(string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return string.Empty;
        }
        //

        if (str[0] == CsvHelper.CsvQuoteCharacter)
        {
            return str.Substring(1, str.Length - 2).Replace("\"\"", "\"");
        }

        return str;
    }

    private sealed record TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
