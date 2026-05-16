// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using System.Diagnostics.CodeAnalysis;
using WeihanLi.Common;

namespace Spark.OpenXml;

/// <summary>
/// Extension methods for exporting entities to Open XML workbooks.
/// </summary>
public static class OpenXmlExtensions
{
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static void ToExcelFile<TEntity>(this IList<TEntity> entityList, string excelPath)
        => ToExcelFile((IEnumerable<TEntity?>)entityList, excelPath, 0);

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static void ToExcelFile<TEntity>(this IEnumerable<TEntity?> entityList, string excelPath)
        => ToExcelFile(entityList, excelPath, 0);

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
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
        entityList.ToExcelStream(stream, sheetIndex);
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static void ToExcelStream<TEntity>(this IEnumerable<TEntity?> entityList, Stream stream)
        => ToExcelStream(entityList, stream, 0);

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static void ToExcelStream<TEntity>(this IEnumerable<TEntity?> entityList, Stream stream, int sheetIndex)
    {
        Guard.NotNull(entityList);
        Guard.NotNull(stream);

        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var sheet = OpenXmlEntityMapper.EntitiesToSheet(entityList, sheetIndex);
        OpenXmlWorkbookWriter.Write(stream, new[] { sheet }, configuration.ExcelSetting);
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static void ToExcelStream<TEntity>(this IList<TEntity> entityList, Stream stream)
    {
        Guard.NotNull(entityList);
        Guard.NotNull(stream);

        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var sheets = OpenXmlEntityMapper.EntitiesToSheets(entityList);
        OpenXmlWorkbookWriter.Write(stream, sheets, configuration.ExcelSetting);
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static byte[] ToExcelBytes<TEntity>(this IEnumerable<TEntity?> entityList)
        => ToExcelBytes(entityList, 0);

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static byte[] ToExcelBytes<TEntity>(this IEnumerable<TEntity?> entityList, int sheetIndex)
    {
        Guard.NotNull(entityList);

        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var sheet = OpenXmlEntityMapper.EntitiesToSheet(entityList, sheetIndex);
        return OpenXmlWorkbookWriter.WriteToBytes(new[] { sheet }, configuration.ExcelSetting);
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static byte[] ToExcelBytes<TEntity>(this IList<TEntity> entityList)
    {
        Guard.NotNull(entityList);

        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var sheets = OpenXmlEntityMapper.EntitiesToSheets(entityList);
        return OpenXmlWorkbookWriter.WriteToBytes(sheets, configuration.ExcelSetting);
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static void ToExcelFileByTemplate<TEntity>(this IEnumerable<TEntity?> entities, string templatePath,
        string excelPath, int sheetIndex = 0, object? extraData = null)
    {
        Guard.NotNull(templatePath);
        using var stream = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var bytes = entities.ToExcelBytesByTemplate(stream, sheetIndex, extraData);
        File.WriteAllBytes(excelPath, bytes);
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static void ToExcelFileByTemplate<TEntity>(this IEnumerable<TEntity?> entities, byte[] templateBytes,
        string excelPath, int sheetIndex = 0, object? extraData = null)
    {
        File.WriteAllBytes(excelPath, entities.ToExcelBytesByTemplate(templateBytes, sheetIndex, extraData));
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static byte[] ToExcelBytesByTemplate<TEntity>(this IEnumerable<TEntity?> entities, string templatePath,
        int sheetIndex = 0, object? extraData = null)
    {
        Guard.NotNull(templatePath);
        using var stream = new FileStream(templatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return entities.ToExcelBytesByTemplate(stream, sheetIndex, extraData);
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static byte[] ToExcelBytesByTemplate<TEntity>(this IEnumerable<TEntity?> entities, byte[] templateBytes,
        int sheetIndex = 0, object? extraData = null)
    {
        Guard.NotNull(templateBytes);
        using var stream = new MemoryStream(templateBytes);
        return entities.ToExcelBytesByTemplate(stream, sheetIndex, extraData);
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
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
