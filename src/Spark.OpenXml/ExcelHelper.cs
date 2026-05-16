// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using System.Data;
using WeihanLi.Common;
using WeihanLi.Common.Models;
using WeihanLi.Common.Services;
using WeihanLi.Extensions;
using Spark.OpenXml.Settings;

namespace Spark.OpenXml;

/// <summary>
///     ExcelHelper
/// </summary>
public static class ExcelHelper
{
    /// <summary>
    ///     Default excel setting for export excel files.
    /// </summary>
    public static ExcelSetting DefaultExcelSetting
    {
        get;
        set => field = Guard.NotNull(value);
    } = new();

    /// <summary>
    /// Default Data Validator.
    /// </summary>
    public static IValidator DefaultDataValidator
    {
        get;
        set => field = Guard.NotNull(value);
    } = DataAnnotationValidator.Instance;

    internal static bool ValidateExcelFilePath(string excelPath, out string msg, bool isExport = false)
    {
        if (string.IsNullOrWhiteSpace(excelPath))
        {
            throw new ArgumentNullException(nameof(excelPath));
        }

        if (isExport || File.Exists(excelPath))
        {
            if (Path.GetExtension(excelPath).EqualsIgnoreCase(".xlsx"))
            {
                msg = string.Empty;
                return true;
            }

            msg = "Only .xlsx Open XML workbooks are supported.";
            return false;
        }

        msg = Resource.FileNotFound;
        return false;
    }

    internal static void EnsureXlsx(ExcelFormat excelFormat)
    {
        if (excelFormat != ExcelFormat.Xlsx)
        {
            throw new NotSupportedException("Only ExcelFormat.Xlsx is supported by Spark.OpenXml.");
        }
    }

    public static IReadOnlyList<string> GetSheetNames(string excelPath)
    {
        if (!ValidateExcelFilePath(excelPath, out var msg))
        {
            throw new ArgumentException(msg, nameof(excelPath));
        }

        using var stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return OpenXmlWorkbookReader.GetSheetNames(stream);
    }

    public static IReadOnlyList<string> GetSheetNames(byte[] excelBytes)
    {
        Guard.NotNull(excelBytes);
        using var stream = new MemoryStream(excelBytes);
        return OpenXmlWorkbookReader.GetSheetNames(stream);
    }

    public static List<TEntity?> ToEntityList<TEntity>(byte[] excelBytes) where TEntity : new()
        => ToEntityList<TEntity>(excelBytes, ExcelFormat.Xlsx, 0);

    public static List<TEntity?> ToEntityList<TEntity>(byte[] excelBytes, int sheetIndex) where TEntity : new()
        => ToEntityList<TEntity>(excelBytes, ExcelFormat.Xlsx, sheetIndex);

    public static List<TEntity?> ToEntityList<TEntity>(byte[] excelBytes, ExcelFormat excelFormat)
        where TEntity : new()
        => ToEntityList<TEntity>(excelBytes, excelFormat, 0);

    public static List<TEntity?> ToEntityList<TEntity>(byte[] excelBytes, ExcelFormat excelFormat, int sheetIndex)
        where TEntity : new()
        => ToEntities<TEntity>(excelBytes, excelFormat, sheetIndex).ToList();

    public static IEnumerable<TEntity?> ToEntities<TEntity>(byte[] excelBytes,
        ExcelFormat excelFormat = ExcelFormat.Xlsx, int sheetIndex = 0)
        where TEntity : new()
    {
        Guard.NotNull(excelBytes);
        using var stream = new MemoryStream(excelBytes);
        return ToEntities<TEntity>(stream, excelFormat, sheetIndex).ToList();
    }

    public static (List<TEntity?> EntityList, Dictionary<int, ValidationResult> ValidationResults)
        ToEntityListWithValidationResult<TEntity>(
            byte[] excelBytes,
            ExcelFormat excelFormat = ExcelFormat.Xlsx,
            int sheetIndex = 0,
            IValidator<TEntity>? validator = null)
        where TEntity : new()
    {
        Guard.NotNull(excelBytes);
        using var stream = new MemoryStream(excelBytes);
        return ToEntityListWithValidationResult(stream, excelFormat, sheetIndex, validator);
    }

    public static List<TEntity?> ToEntityList<TEntity>(Stream excelStream) where TEntity : new()
        => ToEntityList<TEntity>(excelStream, ExcelFormat.Xlsx, 0);

    public static List<TEntity?> ToEntityList<TEntity>(Stream excelStream, int sheetIndex)
        where TEntity : new()
        => ToEntityList<TEntity>(excelStream, ExcelFormat.Xlsx, sheetIndex);

    public static List<TEntity?> ToEntityList<TEntity>(Stream excelStream, ExcelFormat excelFormat)
        where TEntity : new()
        => ToEntityList<TEntity>(excelStream, excelFormat, 0);

    public static List<TEntity?> ToEntityList<TEntity>(Stream excelStream, ExcelFormat excelFormat, int sheetIndex)
        where TEntity : new()
        => ToEntities<TEntity>(excelStream, excelFormat, sheetIndex).ToList();

    public static IEnumerable<TEntity?> ToEntities<TEntity>(Stream excelStream,
        ExcelFormat excelFormat = ExcelFormat.Xlsx, int sheetIndex = 0)
        where TEntity : new()
    {
        EnsureXlsx(excelFormat);
        Guard.NotNull(excelStream);
        return OpenXmlEntityMapper.SheetToEntities<TEntity>(
            OpenXmlWorkbookReader.ReadSheet(excelStream, sheetIndex),
            sheetIndex);
    }

    public static (List<TEntity?> EntityList, Dictionary<int, ValidationResult> ValidationResults)
        ToEntityListWithValidationResult<TEntity>(
            Stream excelStream,
            ExcelFormat excelFormat = ExcelFormat.Xlsx,
            int sheetIndex = 0,
            IValidator<TEntity>? validator = null)
        where TEntity : new()
    {
        EnsureXlsx(excelFormat);
        Guard.NotNull(excelStream);

        var validationResults = new Dictionary<int, ValidationResult>();
        var entities = OpenXmlEntityMapper.SheetToEntities<TEntity>(
            OpenXmlWorkbookReader.ReadSheet(excelStream, sheetIndex),
            sheetIndex,
            (entity, configuration, rowIndex) =>
            {
                var validatorEffective = configuration.Validator;
                if (validator is not null)
                {
                    validatorEffective = validator.GetCommonValidator();
                }

                validatorEffective ??= DefaultDataValidator;
                var validationResult = validatorEffective.Validate(entity);
                if (!validationResult.Valid)
                {
                    validationResults[rowIndex] = validationResult;
                }
            }).ToList();

        return (entities, validationResults);
    }

    public static List<TEntity?> ToEntityList<TEntity>(string excelPath) where TEntity : new()
        => ToEntityList<TEntity>(excelPath, 0);

    public static List<TEntity?> ToEntityList<TEntity>(string excelPath, int sheetIndex) where TEntity : new()
        => ToEntities<TEntity>(excelPath, sheetIndex).ToList();

    public static IEnumerable<TEntity?> ToEntities<TEntity>(string excelPath, int sheetIndex = 0)
        where TEntity : new()
    {
        if (!ValidateExcelFilePath(excelPath, out var msg))
        {
            throw new ArgumentException(msg, nameof(excelPath));
        }

        using var stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return ToEntities<TEntity>(stream, ExcelFormat.Xlsx, sheetIndex).ToList();
    }

    public static (List<TEntity?> EntityList, Dictionary<int, ValidationResult> ValidationResults)
        ToEntityListWithValidationResult<TEntity>(
            string excelPath,
            int sheetIndex = 0,
            IValidator<TEntity>? validator = null)
        where TEntity : new()
    {
        if (!ValidateExcelFilePath(excelPath, out var msg))
        {
            throw new ArgumentException(msg, nameof(excelPath));
        }

        using var stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return ToEntityListWithValidationResult(stream, ExcelFormat.Xlsx, sheetIndex, validator);
    }

    public static DataTable ToDataTable<TEntity>(string excelPath) where TEntity : new()
        => ToDataTable<TEntity>(excelPath, 0);

    public static DataTable ToDataTable<TEntity>(string excelPath, int sheetIndex) where TEntity : new()
    {
        var sheetSetting = InternalHelper.GetExcelConfigurationMapping<TEntity>().SheetSettings.TryGetValue(sheetIndex,
            out var configuredSheetSetting)
            ? configuredSheetSetting
            : InternalHelper.GetExcelConfigurationMapping<TEntity>().SheetSettings[0];
        return ToDataTable(excelPath, sheetIndex, sheetSetting.HeaderRowIndex);
    }

    public static DataTable ToDataTable(string excelPath) => ToDataTable(excelPath, 0, 0);

    public static DataTable ToDataTable(string excelPath, int sheetIndex)
        => ToDataTable(excelPath, sheetIndex, 0);

    public static DataTable ToDataTable(string excelPath, int sheetIndex, int headerRowIndex,
        bool removeEmptyRows = false, int? maxColumns = null)
    {
        if (!ValidateExcelFilePath(excelPath, out var msg))
        {
            throw new ArgumentException(msg, nameof(excelPath));
        }

        using var stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return ToDataTable(stream, sheetIndex, headerRowIndex, removeEmptyRows, maxColumns);
    }

    public static DataTable ToDataTable(byte[] excelBytes, ExcelFormat excelFormat = ExcelFormat.Xlsx,
        bool removeEmptyRows = false, int? maxColumns = null)
        => ToDataTable(excelBytes, excelFormat, 0, 0, removeEmptyRows, maxColumns);

    public static DataTable ToDataTable(byte[] excelBytes, ExcelFormat excelFormat, int sheetIndex,
        bool removeEmptyRows = false, int? maxColumns = null)
        => ToDataTable(excelBytes, excelFormat, sheetIndex, 0, removeEmptyRows, maxColumns);

    public static DataTable ToDataTable(byte[] excelBytes, ExcelFormat excelFormat, int sheetIndex,
        int headerRowIndex, bool removeEmptyRows = false, int? maxColumns = null)
    {
        EnsureXlsx(excelFormat);
        Guard.NotNull(excelBytes);
        using var stream = new MemoryStream(excelBytes);
        return ToDataTable(stream, sheetIndex, headerRowIndex, removeEmptyRows, maxColumns);
    }

    public static DataTable ToDataTable(Stream excelStream, int sheetIndex = 0, int headerRowIndex = 0,
        bool removeEmptyRows = false, int? maxColumns = null)
    {
        Guard.NotNull(excelStream);
        return OpenXmlDataTableMapper.SheetToDataTable(
            OpenXmlWorkbookReader.ReadSheet(excelStream, sheetIndex),
            headerRowIndex,
            removeEmptyRows,
            maxColumns);
    }

    public static DataSet ToDataSet(string excelPath) => ToDataSet(excelPath, 0);

    public static DataSet ToDataSet(string excelPath, int headerRowIndex)
    {
        if (!ValidateExcelFilePath(excelPath, out var msg))
        {
            throw new ArgumentException(msg, nameof(excelPath));
        }

        using var stream = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return OpenXmlDataTableMapper.SheetsToDataSet(OpenXmlWorkbookReader.ReadSheets(stream), headerRowIndex);
    }
}
