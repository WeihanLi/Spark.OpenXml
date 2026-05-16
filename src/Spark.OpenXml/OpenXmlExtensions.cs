// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using System.Data;
using WeihanLi.Common;
using Spark.OpenXml.Settings;

namespace Spark.OpenXml;

/// <summary>
/// Extension methods for exporting entities and <see cref="DataTable" /> instances to Open XML workbooks.
/// </summary>
public static class OpenXmlExtensions
{
    public static void ToExcelFile<TEntity>(this IList<TEntity> entityList, string excelPath)
        => ToExcelFile((IEnumerable<TEntity?>)entityList, excelPath, 0);

    public static void ToExcelFile<TEntity>(this IEnumerable<TEntity?> entityList, string excelPath)
        => ToExcelFile(entityList, excelPath, 0);

    public static void ToExcelFile<TEntity>(this IEnumerable<TEntity?> entityList, string excelPath, int sheetIndex)
    {
        Guard.NotNull(entityList);
        Guard.NotNull(excelPath);
        if (!ExcelHelper.ValidateExcelFilePath(excelPath, out var msg, true))
        {
            throw new ArgumentException(msg, nameof(excelPath));
        }

        InternalHelper.EnsureFileIsNotReadOnly(excelPath);
        var dir = Path.GetDirectoryName(excelPath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var stream = File.Create(excelPath);
        entityList.ToExcelStream(stream, ExcelFormat.Xlsx, sheetIndex);
    }

    public static void ToExcelStream<TEntity>(this IEnumerable<TEntity?> entityList, Stream stream)
        => ToExcelStream(entityList, stream, ExcelFormat.Xlsx, 0);

    public static void ToExcelStream<TEntity>(this IEnumerable<TEntity?> entityList, Stream stream,
        ExcelFormat excelFormat)
        => ToExcelStream(entityList, stream, excelFormat, 0);

    public static void ToExcelStream<TEntity>(this IEnumerable<TEntity?> entityList, Stream stream,
        ExcelFormat excelFormat, int sheetIndex)
    {
        Guard.NotNull(entityList);
        Guard.NotNull(stream);
        ExcelHelper.EnsureXlsx(excelFormat);

        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var sheet = OpenXmlEntityMapper.EntitiesToSheet(entityList, sheetIndex);
        OpenXmlWorkbookWriter.Write(stream, new[] { sheet }, configuration.ExcelSetting);
    }

    public static void ToExcelStream<TEntity>(this IList<TEntity> entityList, Stream stream,
        ExcelFormat excelFormat = ExcelFormat.Xlsx)
    {
        Guard.NotNull(entityList);
        Guard.NotNull(stream);
        ExcelHelper.EnsureXlsx(excelFormat);

        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var sheets = OpenXmlEntityMapper.EntitiesToSheets(entityList, excelFormat);
        OpenXmlWorkbookWriter.Write(stream, sheets, configuration.ExcelSetting);
    }

    public static byte[] ToExcelBytes<TEntity>(this IEnumerable<TEntity?> entityList)
        => ToExcelBytes(entityList, ExcelFormat.Xlsx, 0);

    public static byte[] ToExcelBytes<TEntity>(this IEnumerable<TEntity?> entityList, ExcelFormat excelFormat)
        => ToExcelBytes(entityList, excelFormat, 0);

    public static byte[] ToExcelBytes<TEntity>(this IEnumerable<TEntity?> entityList, ExcelFormat excelFormat,
        int sheetIndex)
    {
        Guard.NotNull(entityList);
        ExcelHelper.EnsureXlsx(excelFormat);

        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var sheet = OpenXmlEntityMapper.EntitiesToSheet(entityList, sheetIndex);
        return OpenXmlWorkbookWriter.WriteToBytes(new[] { sheet }, configuration.ExcelSetting);
    }

    public static byte[] ToExcelBytes<TEntity>(this IList<TEntity> entityList,
        ExcelFormat excelFormat = ExcelFormat.Xlsx)
    {
        Guard.NotNull(entityList);
        ExcelHelper.EnsureXlsx(excelFormat);

        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var sheets = OpenXmlEntityMapper.EntitiesToSheets(entityList, excelFormat);
        return OpenXmlWorkbookWriter.WriteToBytes(sheets, configuration.ExcelSetting);
    }

    public static void ToExcelFile(this DataTable dataTable, string excelPath)
        => ToExcelFile(dataTable, excelPath, null);

    public static void ToExcelFile(this DataTable dataTable, string excelPath, ExcelSetting? excelSetting)
    {
        Guard.NotNull(dataTable);
        Guard.NotNull(excelPath);
        if (!ExcelHelper.ValidateExcelFilePath(excelPath, out var msg, true))
        {
            throw new ArgumentException(msg, nameof(excelPath));
        }

        InternalHelper.EnsureFileIsNotReadOnly(excelPath);
        var dir = Path.GetDirectoryName(excelPath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var stream = File.Create(excelPath);
        dataTable.ToExcelStream(stream, ExcelFormat.Xlsx, excelSetting);
    }

    public static void ToExcelStream(this DataTable dataTable, Stream stream)
        => ToExcelStream(dataTable, stream, ExcelFormat.Xlsx, null);

    public static void ToExcelStream(this DataTable dataTable, Stream stream, ExcelFormat excelFormat)
        => ToExcelStream(dataTable, stream, excelFormat, null);

    public static void ToExcelStream(this DataTable dataTable, Stream stream, ExcelFormat excelFormat,
        ExcelSetting? excelSetting)
    {
        Guard.NotNull(dataTable);
        Guard.NotNull(stream);
        ExcelHelper.EnsureXlsx(excelFormat);

        var sheets = OpenXmlDataTableMapper.DataTableToSheets(dataTable, excelFormat, excelSetting);
        OpenXmlWorkbookWriter.Write(stream, sheets, excelSetting ?? ExcelHelper.DefaultExcelSetting);
    }

    public static byte[] ToExcelBytes(this DataTable dataTable) => ToExcelBytes(dataTable, ExcelFormat.Xlsx);

    public static byte[] ToExcelBytes(this DataTable dataTable, ExcelFormat excelFormat)
        => ToExcelBytes(dataTable, excelFormat, null);

    public static byte[] ToExcelBytes(this DataTable dataTable, ExcelFormat excelFormat,
        ExcelSetting? excelSetting)
    {
        Guard.NotNull(dataTable);
        ExcelHelper.EnsureXlsx(excelFormat);

        var sheets = OpenXmlDataTableMapper.DataTableToSheets(dataTable, excelFormat, excelSetting);
        return OpenXmlWorkbookWriter.WriteToBytes(sheets, excelSetting ?? ExcelHelper.DefaultExcelSetting);
    }

    public static void ToExcelFileByTemplate<TEntity>(this IEnumerable<TEntity?> entities, string templatePath,
        string excelPath, int sheetIndex = 0, object? extraData = null)
    {
        Guard.NotNull(templatePath);
        using var stream = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var bytes = entities.ToExcelBytesByTemplate(stream, sheetIndex, extraData);
        File.WriteAllBytes(excelPath, bytes);
    }

    public static void ToExcelFileByTemplate<TEntity>(this IEnumerable<TEntity?> entities, byte[] templateBytes,
        string excelPath, int sheetIndex = 0, object? extraData = null)
    {
        File.WriteAllBytes(excelPath, entities.ToExcelBytesByTemplate(templateBytes, sheetIndex, extraData));
    }

    public static byte[] ToExcelBytesByTemplate<TEntity>(this IEnumerable<TEntity?> entities, string templatePath,
        int sheetIndex = 0, object? extraData = null)
    {
        Guard.NotNull(templatePath);
        using var stream = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return entities.ToExcelBytesByTemplate(stream, sheetIndex, extraData);
    }

    public static byte[] ToExcelBytesByTemplate<TEntity>(this IEnumerable<TEntity?> entities, byte[] templateBytes,
        int sheetIndex = 0, object? extraData = null)
    {
        Guard.NotNull(templateBytes);
        using var stream = new MemoryStream(templateBytes);
        return entities.ToExcelBytesByTemplate(stream, sheetIndex, extraData);
    }

    public static byte[] ToExcelBytesByTemplate<TEntity>(this IEnumerable<TEntity?> entities, Stream templateStream,
        int sheetIndex = 0, object? extraData = null)
    {
        Guard.NotNull(entities);
        Guard.NotNull(templateStream);

        var sheets = OpenXmlWorkbookReader.ReadSheets(templateStream)
            .Select(x => new OpenXmlSheetExport { Name = x.Name }.CopyRowsFrom(x))
            .ToArray();
        if (sheetIndex < 0 || sheetIndex >= sheets.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(sheetIndex));
        }

        OpenXmlTemplateProcessor.ApplyTemplate(sheets[sheetIndex], entities, extraData);
        return OpenXmlWorkbookWriter.WriteToBytes(sheets, ExcelHelper.DefaultExcelSetting);
    }

    private static OpenXmlSheetExport CopyRowsFrom(this OpenXmlSheetExport target, OpenXmlSheet source)
    {
        foreach (var row in source.Rows)
        {
            target.Rows[row.Key] = row.Value.ToDictionary(x => x.Key, x => (object?)x.Value)
                .ToSortedDictionary();
        }

        target.RowsCount = Math.Max(0, target.Rows.Keys.DefaultIfEmpty(0).Max());
        return target;
    }

    private static SortedDictionary<TKey, TValue> ToSortedDictionary<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary)
        where TKey : notnull
        => new(dictionary);
}
