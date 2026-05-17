// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace Spark.OpenXml;

internal static class OpenXmlTemplateProcessor
{
    public static readonly TemplateOptions TemplateOptions = new();

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static void ApplyTemplate<TEntity>(OpenXmlSheetExport sheet, IEnumerable<TEntity?> entities,
        object? extraData)
    {
        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var propertyColumnDictionary = InternalHelper.GetPropertyColumnDictionary(configuration);
        var globalDictionary = extraData.ParseParamInfo()
            .ToDictionary(x => TemplateOptions.TemplateGlobalParamFormat.FormatWith(x.Key), x => x.Value);

        foreach (var propertyConfiguration in propertyColumnDictionary)
        {
            globalDictionary[TemplateOptions.TemplateHeaderParamFormat.FormatWith(propertyConfiguration.Key.Name)] =
                propertyConfiguration.Value.ColumnTitle;
        }

        var dataFuncDictionary = propertyColumnDictionary
            .ToDictionary(x => TemplateOptions.TemplateDataParamFormat.FormatWith(x.Key.Name),
                x => x.Key.GetValueGetter<TEntity>());
        foreach (var key in propertyColumnDictionary.Keys)
        {
            if (InternalCache.OutputFormatterFuncCache.TryGetValue(key, out var formatterFunc) &&
                formatterFunc?.Method is not null)
            {
                dataFuncDictionary[TemplateOptions.TemplateDataParamFormat.FormatWith(key.Name)] = entity =>
                {
                    var val = key.GetValueGetter<TEntity>()?.Invoke(entity);
                    try
                    {
                        return formatterFunc.Method.Invoke(entity, [val]);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        InvokeHelper.OnInvokeException?.Invoke(e);
                        return val;
                    }
                };
            }
        }

        int dataStartRow = -1, dataRowsCount = 0;
        foreach (var rowEntry in sheet.Rows.ToArray())
        {
            foreach (var cellEntry in rowEntry.Value.ToArray())
            {
                if (cellEntry.Value is not string cellValue || cellValue.IsNullOrEmpty())
                {
                    continue;
                }

                var beforeValue = cellValue;
                if (dataStartRow <= 0 || dataRowsCount <= 0)
                {
                    if (dataStartRow >= 0)
                    {
                        if (cellValue.Contains(TemplateOptions.TemplateDataEnd))
                        {
                            dataRowsCount = rowEntry.Key - dataStartRow + 1;
                            cellValue = cellValue.Replace(TemplateOptions.TemplateDataEnd, string.Empty);
                        }
                    }
                    else if (cellValue.Contains(TemplateOptions.TemplateDataBegin))
                    {
                        dataStartRow = rowEntry.Key;
                        cellValue = cellValue.Replace(TemplateOptions.TemplateDataBegin, string.Empty);
                    }
                }

                foreach (var param in globalDictionary.Keys)
                {
                    if (cellValue.Contains(param))
                    {
                        cellValue = cellValue.Replace(param, globalDictionary[param]?.ToString() ?? string.Empty);
                    }
                }

                if (beforeValue != cellValue)
                {
                    sheet.Rows[rowEntry.Key][cellEntry.Key] = cellValue;
                }
            }
        }

        if (dataStartRow < 0 || dataRowsCount <= 0)
        {
            return;
        }

        var templateRows = sheet.Rows
            .Where(x => x.Key >= dataStartRow && x.Key < dataStartRow + dataRowsCount)
            .Select(x => x.Value.ToDictionary(c => c.Key, c => c.Value))
            .ToArray();
        var tailRows = sheet.Rows
            .Where(x => x.Key >= dataStartRow + dataRowsCount)
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (var rowIndex in tailRows.Keys.Concat(Enumerable.Range(dataStartRow, dataRowsCount)).ToArray())
        {
            sheet.Rows.Remove(rowIndex);
        }

        var currentRow = dataStartRow;
        foreach (var entity in entities)
        {
            foreach (var templateRow in templateRows)
            {
                var row = new SortedDictionary<int, object?>();
                foreach (var cell in templateRow)
                {
                    var cellValue = cell.Value?.ToString() ?? string.Empty;
                    foreach (var param in dataFuncDictionary.Keys)
                    {
                        if (cellValue.Contains(param))
                        {
                            var replacement = entity is null
                                ? string.Empty
                                : dataFuncDictionary[param]?.Invoke(entity)?.ToString() ?? string.Empty;
                            cellValue = cellValue.Replace(param, replacement);
                        }
                    }

                    row[cell.Key] = cellValue;
                }

                sheet.Rows[currentRow++] = row;
            }
        }

        foreach (var tailRow in tailRows.OrderBy(x => x.Key))
        {
            sheet.Rows[currentRow++] = tailRow.Value;
        }

        sheet.RowsCount = Math.Max(0, currentRow - 1);
    }
}
