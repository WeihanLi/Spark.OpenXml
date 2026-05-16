// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Spark.OpenXml.Configurations;
using Spark.OpenXml.Settings;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using WeihanLi.Common;
using WeihanLi.Extensions;

namespace Spark.OpenXml;

internal sealed class OpenXmlSheet
{
    public string Name { get; init; } = "Sheet0";

    public SortedDictionary<int, SortedDictionary<int, string?>> Rows { get; } = new();

    public string? GetCellText(int rowIndex, int columnIndex)
    {
        return Rows.TryGetValue(rowIndex, out var row) && row.TryGetValue(columnIndex, out var value)
            ? value
            : null;
    }
}

internal sealed class OpenXmlSheetExport
{
    public string Name { get; init; } = "Sheet0";

    public SortedDictionary<int, SortedDictionary<int, object?>> Rows { get; } = new();

    public Dictionary<int, double> ColumnWidths { get; } = new();

    public bool AutoColumnWidthEnabled { get; init; }

    public IReadOnlyList<FreezeSetting> FreezeSettings { get; init; } = Array.Empty<FreezeSetting>();

    public FilterSetting? FilterSetting { get; init; }

    public int HeaderRowIndex { get; init; }

    public int RowsCount { get; set; }
}

internal static class OpenXmlWorkbookWriter
{
    public static byte[] WriteToBytes(IReadOnlyList<OpenXmlSheetExport> sheets, ExcelSetting? excelSetting = null)
    {
        using var stream = new MemoryStream();
        Write(stream, sheets, excelSetting);
        return stream.ToArray();
    }

    public static void Write(Stream stream, IReadOnlyList<OpenXmlSheetExport> sheets, ExcelSetting? excelSetting = null)
    {
        Guard.NotNull(stream);
        Guard.NotNull(sheets);

        using var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true);
        ApplyPackageProperties(document, excelSetting ?? ExcelHelper.DefaultExcelSetting);

        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
        stylesPart.Stylesheet = CreateStylesheet();
        stylesPart.Stylesheet.Save();

        var sheetsElement = workbookPart.Workbook.AppendChild(new Sheets());
        var sheetId = 1u;
        var sheetsToWrite = sheets.Count == 0
            ? new[] { new OpenXmlSheetExport { Name = "Sheet0" } }
            : sheets;
        foreach (var sheetExport in sheetsToWrite)
        {
            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = CreateWorksheet(sheetExport);

            sheetsElement.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = sheetId++,
                Name = SanitizeSheetName(sheetExport.Name)
            });
        }

        workbookPart.Workbook.Save();
    }

    private static Worksheet CreateWorksheet(OpenXmlSheetExport sheetExport)
    {
        var worksheet = new Worksheet();
        var maxColumnIndex = sheetExport.Rows.Values.SelectMany(x => x.Keys).DefaultIfEmpty(0).Max();

        var sheetViews = CreateSheetViews(sheetExport.FreezeSettings);
        if (sheetViews is not null)
        {
            worksheet.Append(sheetViews);
        }

        var columns = CreateColumns(sheetExport, maxColumnIndex);
        if (columns is not null)
        {
            worksheet.Append(columns);
        }

        var sheetData = new SheetData();
        foreach (var rowEntry in sheetExport.Rows)
        {
            var row = new Row { RowIndex = (uint)rowEntry.Key + 1 };
            foreach (var cellEntry in rowEntry.Value.OrderBy(x => x.Key))
            {
                row.Append(CreateCell(rowEntry.Key, cellEntry.Key, cellEntry.Value));
            }
            sheetData.Append(row);
        }

        worksheet.Append(sheetData);

        if (sheetExport.FilterSetting is not null && sheetExport.RowsCount > 0)
        {
            var lastColumn = sheetExport.FilterSetting.LastColumn ?? maxColumnIndex;
            worksheet.Append(new AutoFilter
            {
                Reference = $"{ColumnName(sheetExport.FilterSetting.FirstColumn)}{sheetExport.HeaderRowIndex + 1}:{ColumnName(lastColumn)}{sheetExport.HeaderRowIndex + sheetExport.RowsCount + 1}"
            });
        }

        return worksheet;
    }

    private static Columns? CreateColumns(OpenXmlSheetExport sheetExport, int maxColumnIndex)
    {
        var widths = new Dictionary<int, double>(sheetExport.ColumnWidths);
        if (sheetExport.AutoColumnWidthEnabled)
        {
            foreach (var row in sheetExport.Rows.Values)
            {
                foreach (var cell in row)
                {
                    var textWidth = FormatExportValue(cell.Value, null)?.Length + 2 ?? 0;
                    if (!widths.TryGetValue(cell.Key, out var width) || textWidth > width)
                    {
                        widths[cell.Key] = Math.Min(Math.Max(textWidth, 8), 80);
                    }
                }
            }
        }

        if (widths.Count == 0)
        {
            return null;
        }

        var columns = new Columns();
        for (var i = 0; i <= maxColumnIndex; i++)
        {
            if (widths.TryGetValue(i, out var width) && width > 0)
            {
                columns.Append(new Column
                {
                    Min = (uint)i + 1,
                    Max = (uint)i + 1,
                    Width = width,
                    CustomWidth = true
                });
            }
        }

        return columns;
    }

    private static SheetViews? CreateSheetViews(IReadOnlyList<FreezeSetting> freezeSettings)
    {
        var freeze = freezeSettings.LastOrDefault();
        if (freeze is null)
        {
            return null;
        }

        var pane = new Pane
        {
            State = PaneStateValues.Frozen,
            TopLeftCell = $"{ColumnName(Math.Max(freeze.LeftMostColumn, freeze.ColSplit))}{Math.Max(freeze.TopRow, freeze.RowSplit) + 1}"
        };

        if (freeze.ColSplit > 0)
        {
            pane.HorizontalSplit = freeze.ColSplit;
        }

        if (freeze.RowSplit > 0)
        {
            pane.VerticalSplit = freeze.RowSplit;
        }

        return new SheetViews(new SheetView(pane) { WorkbookViewId = 0 });
    }

    private static Cell CreateCell(int rowIndex, int columnIndex, object? value)
    {
        var cell = new Cell { CellReference = $"{ColumnName(columnIndex)}{rowIndex + 1}" };
        if (value is null || value == DBNull.Value)
        {
            return cell;
        }

        switch (value)
        {
            case bool boolValue:
                cell.DataType = CellValues.Boolean;
                cell.CellValue = new CellValue(boolValue ? "1" : "0");
                break;
            case byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal:
                cell.DataType = CellValues.Number;
                cell.CellValue = new CellValue(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
                break;
            default:
                cell.DataType = CellValues.InlineString;
                cell.InlineString = new InlineString(new Text(value.ToString() ?? string.Empty)
                {
                    Space = SpaceProcessingModeValues.Preserve
                });
                break;
        }

        return cell;
    }

    internal static string? FormatExportValue(object? value, string? formatter)
    {
        if (value is null || value == DBNull.Value)
        {
            return null;
        }

        if (value is DateTime time)
        {
            return string.IsNullOrWhiteSpace(formatter)
                ? time.Date == time ? time.ToDateString() : time.ToTimeString()
                : time.ToString(formatter, CultureInfo.CurrentCulture);
        }

        return value is IFormattable val && formatter.IsNotNullOrWhiteSpace()
            ? val.ToString(formatter, CultureInfo.CurrentCulture)
            : value.ToString();
    }

    internal static string ColumnName(int columnIndex)
    {
        var dividend = columnIndex + 1;
        var columnName = string.Empty;
        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar('A' + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }

    private static string SanitizeSheetName(string? sheetName)
    {
        var name = sheetName.IsNullOrWhiteSpace() ? "Sheet0" : sheetName!;
        foreach (var invalidChar in new[] { '\\', '/', '?', '*', '[', ']', ':' })
        {
            name = name.Replace(invalidChar, '_');
        }

        return name.Length > 31 ? name.Substring(0, 31) : name;
    }

    private static Stylesheet CreateStylesheet() =>
        new(
            new Fonts(new Font()),
            new Fills(new Fill(new PatternFill { PatternType = PatternValues.None })),
            new Borders(new Border()),
            new CellStyleFormats(new CellFormat()),
            new CellFormats(new CellFormat()),
            new CellStyles(new CellStyle { Name = "Normal", FormatId = 0, BuiltinId = 0 }));

    private static void ApplyPackageProperties(SpreadsheetDocument document, ExcelSetting setting)
    {
        document.PackageProperties.Creator = setting.Author;
        document.PackageProperties.Created = DateTime.Now;
        document.PackageProperties.Modified = DateTime.Now;
        document.PackageProperties.Title = setting.Title;
        document.PackageProperties.Subject = setting.Subject;
        document.PackageProperties.Category = setting.Category;
        document.PackageProperties.Description = setting.Description;
    }
}

internal static class OpenXmlWorkbookReader
{
    public static IReadOnlyList<string> GetSheetNames(Stream stream)
    {
        using var document = SpreadsheetDocument.Open(stream, false);
        var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("Invalid workbook.");
        var workbook = workbookPart.Workbook ?? throw new InvalidOperationException("Invalid workbook.");
        return workbook.Sheets?.Elements<Sheet>()
            .Select(x => x.Name?.Value ?? string.Empty)
            .ToArray() ?? Array.Empty<string>();
    }

    public static IReadOnlyList<OpenXmlSheet> ReadSheets(Stream stream)
    {
        using var document = SpreadsheetDocument.Open(stream, false);
        var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("Invalid workbook.");
        var workbook = workbookPart.Workbook ?? throw new InvalidOperationException("Invalid workbook.");
        var sheets = workbook.Sheets;
        return sheets?.Elements<Sheet>()
            .Select(sheet => ReadSheet(workbookPart, sheet))
            .ToArray() ?? Array.Empty<OpenXmlSheet>();
    }

    public static OpenXmlSheet ReadSheet(Stream stream, int sheetIndex)
    {
        using var document = SpreadsheetDocument.Open(stream, false);
        var workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("Invalid workbook.");
        var workbook = workbookPart.Workbook ?? throw new InvalidOperationException("Invalid workbook.");
        var sheets = workbook.Sheets?.Elements<Sheet>().ToArray() ?? Array.Empty<Sheet>();
        if (sheetIndex < 0 || sheetIndex >= sheets.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(sheetIndex),
                string.Format(Resource.IndexOutOfRange, nameof(sheetIndex), sheets.Length));
        }

        return ReadSheet(workbookPart, sheets[sheetIndex]);
    }

    private static OpenXmlSheet ReadSheet(WorkbookPart workbookPart, Sheet sheet)
    {
        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
        var sharedStrings = workbookPart.SharedStringTablePart?.SharedStringTable;
        var sheetResult = new OpenXmlSheet { Name = sheet.Name?.Value ?? string.Empty };
        var dateStyleIndexes = GetDateStyleIndexes(workbookPart);

        var worksheet = worksheetPart.Worksheet ?? throw new InvalidOperationException("Invalid worksheet.");
        foreach (var row in worksheet.Descendants<Row>())
        {
            if (row.RowIndex?.Value is null)
            {
                continue;
            }

            var rowIndex = (int)row.RowIndex.Value - 1;
            var rowValues = new SortedDictionary<int, string?>();
            foreach (var cell in row.Elements<Cell>())
            {
                var columnIndex = GetColumnIndex(cell.CellReference?.Value);
                rowValues[columnIndex] = ReadCellText(cell, sharedStrings, dateStyleIndexes);
            }

            sheetResult.Rows[rowIndex] = rowValues;
        }

        return sheetResult;
    }

    private static string? ReadCellText(Cell cell, SharedStringTable? sharedStrings, ISet<uint> dateStyleIndexes)
    {
        var rawValue = cell.CellValue?.InnerText;
        if (cell.DataType?.Value == CellValues.SharedString)
        {
            return int.TryParse(rawValue, out var sharedStringIndex)
                ? sharedStrings?.ElementAtOrDefault(sharedStringIndex)?.InnerText
                : rawValue;
        }

        if (cell.DataType?.Value == CellValues.InlineString)
        {
            return cell.InlineString?.InnerText ?? string.Empty;
        }

        if (cell.DataType?.Value == CellValues.Boolean)
        {
            return rawValue == "1" ? bool.TrueString : bool.FalseString;
        }

        if (rawValue is not null
            && cell.StyleIndex?.Value is uint styleIndex
            && dateStyleIndexes.Contains(styleIndex)
            && double.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var oaDate))
        {
            return DateTime.FromOADate(oaDate).ToShortDateString();
        }

        return rawValue ?? cell.CellFormula?.Text;
    }

    private static HashSet<uint> GetDateStyleIndexes(WorkbookPart workbookPart)
    {
        var result = new HashSet<uint>();
        var stylesheet = workbookPart.WorkbookStylesPart?.Stylesheet;
        if (stylesheet?.CellFormats is null)
        {
            return result;
        }

        var customFormats = stylesheet.NumberingFormats?.Elements<NumberingFormat>()
            .Where(x => x.NumberFormatId?.Value is not null && x.FormatCode?.Value is not null)
            .ToDictionary(x => x.NumberFormatId!.Value, x => x.FormatCode!.Value!) ?? new Dictionary<uint, string>();

        var index = 0u;
        foreach (var format in stylesheet.CellFormats.Elements<CellFormat>())
        {
            var formatId = format.NumberFormatId?.Value ?? 0;
            if (IsDateFormat(formatId, customFormats.TryGetValue(formatId, out var formatCode) ? formatCode : null))
            {
                result.Add(index);
            }

            index++;
        }

        return result;
    }

    private static bool IsDateFormat(uint formatId, string? formatCode)
    {
        if ((formatId >= 14 && formatId <= 22) || (formatId >= 45 && formatId <= 47))
        {
            return true;
        }

        return formatCode is not null
               && formatCode.IndexOfAny(new[] { 'y', 'm', 'd', 'h', 's' }) >= 0
               && formatCode.IndexOf("[Red]", StringComparison.OrdinalIgnoreCase) < 0;
    }

    private static int GetColumnIndex(string? cellReference)
    {
        if (cellReference.IsNullOrWhiteSpace())
        {
            return 0;
        }

        var columnIndex = 0;
        foreach (var ch in cellReference!)
        {
            if (!char.IsLetter(ch))
            {
                break;
            }

            columnIndex *= 26;
            columnIndex += char.ToUpperInvariant(ch) - 'A' + 1;
        }

        return columnIndex - 1;
    }
}

internal static class OpenXmlEntityMapper
{
    private static SheetSetting GetSheetSetting(IDictionary<int, SheetSetting> sheetSettings, int sheetIndex) =>
        sheetIndex > 0 && sheetSettings.TryGetValue(sheetIndex, out var sheetSetting)
            ? sheetSetting
            : sheetSettings[0];

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static IEnumerable<TEntity?> SheetToEntities<TEntity>(OpenXmlSheet sheet, int sheetIndex,
        Action<TEntity?, ExcelConfiguration<TEntity>, int>? dataAction = null)
        where TEntity : new()
    {
        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var sheetSetting = GetSheetSetting(configuration.SheetSettings, sheetIndex);
        var propertyColumnDictionary = InternalHelper.GetPropertyColumnDictionary(configuration);
        var propertyColumnDic = sheetSetting.HeaderRowIndex >= 0
            ? propertyColumnDictionary.ToDictionary(p => p.Key, p => CloneForImport(p.Value))
            : propertyColumnDictionary;

        if (sheetSetting.HeaderRowIndex >= 0 && !sheetSetting.SkipHeaderRow)
        {
            if (sheet.Rows.TryGetValue(sheetSetting.HeaderRowIndex, out var headerRow))
            {
                foreach (var headerCell in headerRow)
                {
                    var columnIndex = headerCell.Key;
                    var title = headerCell.Value;
                    if (!sheetSetting.IncludesColumn(columnIndex))
                    {
                        continue;
                    }

                    var col = propertyColumnDic.GetPropertySetting(title?.Trim() ?? string.Empty);
                    if (col is not null)
                    {
                        col.ColumnIndex = columnIndex;
                    }
                }
            }

            if (propertyColumnDic.Values.Any(p => p.ColumnIndex < 0))
            {
                propertyColumnDic = propertyColumnDictionary;
            }
        }

        if (sheet.Rows.Count == 0)
        {
            yield break;
        }

        var lastRowNum = sheetSetting.EndRowIndex ?? sheet.Rows.Keys.DefaultIfEmpty(sheetSetting.StartRowIndex - 1).Max();
        for (var rowIndex = Math.Max(0, sheetSetting.StartRowIndex); rowIndex <= lastRowNum; rowIndex++)
        {
            sheet.Rows.TryGetValue(rowIndex, out var row);
            if (row is null || row.Count == 0)
            {
                yield return default;
                continue;
            }

            var entity = new TEntity();
            var hasAnyValue = false;
            var boxed = configuration.EntityType.IsValueType ? (object)entity : entity!;

            foreach (var key in propertyColumnDic.Keys)
            {
                var colIndex = propertyColumnDic[key].ColumnIndex;
                if (colIndex < 0 || !key.CanWrite || !sheetSetting.IncludesColumn(colIndex))
                {
                    continue;
                }

                var cellValue = sheet.GetCellText(rowIndex, colIndex);
                if (cellValue is not null)
                {
                    hasAnyValue = true;
                }

                object? columnValue;
                if (InternalCache.ColumnInputFormatterFuncCache.TryGetValue(key, out var formatterFunc) &&
                    formatterFunc?.Method is not null)
                {
                    columnValue = formatterFunc.DynamicInvoke(cellValue);
                }
                else
                {
                    columnValue = cellValue.ToOrDefault(key.PropertyType);
                }

                key.GetValueSetter()?.Invoke(boxed, columnValue);
            }

            if (!hasAnyValue)
            {
                yield return default;
                continue;
            }

            entity = configuration.EntityType.IsValueType ? (TEntity)boxed : entity;

            foreach (var propertyInfo in propertyColumnDic.Keys)
            {
                if (!propertyInfo.CanWrite) continue;

                var propertyValue = propertyInfo.GetValueGetter()?.Invoke(entity);
                if (!InternalCache.InputFormatterFuncCache.TryGetValue(propertyInfo, out var formatterFunc) ||
                    formatterFunc?.Method is null) continue;

                var valueSetter = propertyInfo.GetValueSetter();
                if (valueSetter is null) continue;
                valueSetter.Invoke(entity, formatterFunc.DynamicInvoke(entity, propertyValue));
            }

            if (configuration.DataFilter?.Invoke(entity) == false)
            {
                continue;
            }

            dataAction?.Invoke(entity, configuration, rowIndex);
            configuration.PostImportAction?.Invoke(entity, rowIndex);
            yield return entity;
        }
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static IReadOnlyList<OpenXmlSheetExport> EntitiesToSheets<TEntity>(IList<TEntity> entityList)
    {
        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var sheetSetting = configuration.SheetSettings[0];
        var maxRowCount = InternalConstants.MaxRowCountXlsx - sheetSetting.StartRowIndex;
        var sheetCount = Math.Max(1, (entityList.Count + maxRowCount - 1) / maxRowCount);
        var result = new List<OpenXmlSheetExport>(sheetCount);
        for (var sheetIndex = 0; sheetIndex < sheetCount; sheetIndex++)
        {
            var pageItems = entityList.Skip(sheetIndex * maxRowCount).Take(maxRowCount);
            result.Add(EntitiesToSheet(pageItems, sheetIndex));
        }

        return result;
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static OpenXmlSheetExport EntitiesToSheet<TEntity>(IEnumerable<TEntity?> entityList, int sheetIndex)
    {
        var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
        var propertyColumnDictionary = InternalHelper.GetPropertyColumnDictionary(configuration);
        var sheetSetting = GetSheetSetting(configuration.SheetSettings, sheetIndex);
        var sheet = CreateSheetExport(sheetSetting, configuration, propertyColumnDictionary);

        if (sheetSetting.HeaderRowIndex >= 0)
        {
            foreach (var property in propertyColumnDictionary)
            {
                SetCell(sheet, sheetSetting.HeaderRowIndex, property.Value.ColumnIndex, property.Value.ColumnTitle);
            }
        }

        var rowIndex = 0;
        foreach (var entity in entityList)
        {
            var rowNumber = sheetSetting.StartRowIndex + rowIndex;
            if (entity is not null)
            {
                foreach (var key in propertyColumnDictionary.Keys)
                {
                    var propertyValue = key.GetValueGetter<TEntity>()?.Invoke(entity);
                    if (InternalCache.OutputFormatterFuncCache.TryGetValue(key, out var formatterFunc) &&
                        formatterFunc?.Method is not null)
                    {
                        propertyValue = formatterFunc.DynamicInvoke(entity, propertyValue);
                    }

                    var formattedValue = OpenXmlWorkbookWriter.FormatExportValue(propertyValue,
                        propertyColumnDictionary[key].ColumnFormatter);
                    SetCell(sheet, rowNumber, propertyColumnDictionary[key].ColumnIndex, formattedValue);
                }
            }
            else
            {
                sheet.Rows[rowNumber] = new SortedDictionary<int, object?>();
            }

            rowIndex++;
        }

        sheet.RowsCount = rowIndex;
        return sheet;
    }

    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    internal static OpenXmlSheetExport CreateSheetExport<TEntity>(SheetSetting sheetSetting,
        ExcelConfiguration<TEntity> configuration,
        IDictionary<PropertyInfo, PropertyConfiguration> propertyColumnDictionary)
    {
        var sheet = new OpenXmlSheetExport
        {
            Name = sheetSetting.SheetName,
            AutoColumnWidthEnabled = sheetSetting.AutoColumnWidthEnabled,
            FreezeSettings = configuration.FreezeSettings.ToArray(),
            FilterSetting = configuration.FilterSetting,
            HeaderRowIndex = sheetSetting.HeaderRowIndex
        };

        foreach (var setting in propertyColumnDictionary.Values)
        {
            if (setting.ColumnWidth > 0)
            {
                sheet.ColumnWidths[setting.ColumnIndex] = setting.ColumnWidth;
            }
        }

        return sheet;
    }

    internal static void SetCell(OpenXmlSheetExport sheet, int rowIndex, int columnIndex, object? value)
    {
        if (!sheet.Rows.TryGetValue(rowIndex, out var row))
        {
            sheet.Rows[rowIndex] = row = new SortedDictionary<int, object?>();
        }

        row[columnIndex] = value;
    }

    private static PropertyConfiguration CloneForImport(PropertyConfiguration source) => new()
    {
        ColumnIndex = -1,
        ColumnFormatter = source.ColumnFormatter,
        ColumnTitle = source.ColumnTitle,
        ColumnWidth = source.ColumnWidth,
        IsIgnored = source.IsIgnored,
        PropertyName = source.PropertyName
    };
}
