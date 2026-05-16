// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using Spark.OpenXml.Configurations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using WeihanLi.Common;
using WeihanLi.Common.Helpers;
using WeihanLi.Extensions;

namespace Spark.OpenXml;

/// <summary>
///     CsvHelper provides utilities for reading and writing CSV files, 
///     supporting conversion between CSV data and strongly-typed entities.
/// </summary>
public static class CsvHelper
{
    /// <summary>
    ///     CSV separator character, ',' by default.
    ///     Can be changed to support different CSV formats (e.g., ';' for European format).
    /// </summary>
    public static char CsvSeparatorCharacter = ',';

    /// <summary>
    ///     CSV quote character used to escape values containing special characters, <c>"</c> by default.
    ///     Values containing the separator character will be wrapped with this quote character.
    /// </summary>
    public static char CsvQuoteCharacter = '"';

    /// <summary>
    ///     Converts CSV file data to a strongly-typed entity list with default options.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="filePath">Path to the CSV file</param>
    /// <returns>A list of entities populated from CSV data</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static List<TEntity?> ToEntityList<TEntity>(string filePath)
        => ToEntityList<TEntity>(filePath, CsvOptions.Default);

    /// <summary>
    ///     Converts CSV file data to a strongly-typed entity list with custom options.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="filePath">Path to the CSV file</param>
    /// <param name="csvOptions">Custom CSV parsing options</param>
    /// <returns>A list of entities populated from CSV data</returns>
    /// <exception cref="ArgumentException">Thrown when the file does not exist</exception>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static List<TEntity?> ToEntityList<TEntity>(string filePath, CsvOptions csvOptions)
    {
        Guard.NotNull(filePath);
        if (!File.Exists(filePath))
        {
            throw new ArgumentException(Resource.FileNotFound, nameof(filePath));
        }

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return ToEntityList<TEntity>(fs, csvOptions);
    }

    /// <summary>
    ///     Converts CSV file data to a lazy-loaded sequence of strongly-typed entities.
    ///     Use this method for large files to avoid loading all data into memory at once.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="filePath">Path to the CSV file</param>
    /// <param name="csvOptions">Optional custom CSV parsing options</param>
    /// <returns>A lazy-loaded enumerable of entities</returns>
    /// <exception cref="ArgumentException">Thrown when the file does not exist</exception>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static IEnumerable<TEntity?> ToEntities<TEntity>(string filePath, CsvOptions? csvOptions = null)
    {
        Guard.NotNull(filePath);
        if (!File.Exists(filePath))
        {
            throw new ArgumentException(Resource.FileNotFound, nameof(filePath));
        }
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        // https://stackoverflow.com/questions/1539114/yield-return-statement-inside-a-using-block-disposes-before-executing
        foreach (var entity in ToEntities<TEntity>(fs, csvOptions))
        {
            yield return entity;
        }
    }

    /// <summary>
    ///     Converts CSV byte data to a strongly-typed entity list with default options.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="csvBytes">CSV data as byte array</param>
    /// <returns>A list of entities populated from CSV data</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static List<TEntity?> ToEntityList<TEntity>(byte[] csvBytes)
        => ToEntityList<TEntity>(csvBytes, CsvOptions.Default);

    /// <summary>
    ///     Converts CSV byte data to a strongly-typed entity list with custom options.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="csvBytes">CSV data as byte array</param>
    /// <param name="csvOptions">Custom CSV parsing options</param>
    /// <returns>A list of entities populated from CSV data</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static List<TEntity?> ToEntityList<TEntity>(byte[] csvBytes, CsvOptions csvOptions)
    {
        Guard.NotNull(csvBytes);
        using var ms = new MemoryStream(csvBytes);
        return ToEntityList<TEntity>(ms, csvOptions);
    }

    /// <summary>
    ///     Converts CSV byte data to a lazy-loaded sequence of strongly-typed entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="csvBytes">CSV data as byte array</param>
    /// <param name="csvOptions">Optional custom CSV parsing options</param>
    /// <returns>A lazy-loaded enumerable of entities</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static IEnumerable<TEntity?> ToEntities<TEntity>(byte[] csvBytes, CsvOptions? csvOptions = null)
    {
        Guard.NotNull(csvBytes);
        using var ms = new MemoryStream(csvBytes);
        foreach (var entity in ToEntities<TEntity>(ms, csvOptions))
        {
            yield return entity;
        }
    }

    /// <summary>
    ///     Converts CSV stream data to a strongly-typed entity list with default options.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="csvStream">Stream containing CSV data</param>
    /// <returns>A list of entities populated from CSV data</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static List<TEntity?> ToEntityList<TEntity>(Stream csvStream)
        => ToEntityList<TEntity>(csvStream, CsvOptions.Default);

    /// <summary>
    ///     Converts CSV stream data to a strongly-typed entity list with custom options.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="csvStream">Stream containing CSV data</param>
    /// <param name="csvOptions">Custom CSV parsing options</param>
    /// <returns>A list of entities populated from CSV data</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static List<TEntity?> ToEntityList<TEntity>(Stream csvStream, CsvOptions csvOptions)
    {
        Guard.NotNull(csvStream);
        return ToEntities<TEntity>(csvStream, csvOptions).ToList();
    }

    /// <summary>
    ///     Converts CSV stream data to a lazy-loaded sequence of strongly-typed entities.
    ///     This method is memory-efficient for large CSV files.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="csvStream">Stream containing CSV data</param>
    /// <param name="csvOptions">Optional custom CSV parsing options</param>
    /// <returns>A lazy-loaded enumerable of entities</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static IEnumerable<TEntity?> ToEntities<TEntity>(Stream csvStream, CsvOptions? csvOptions = null)
    {
        Guard.NotNull(csvStream);

        var lines = GetLines();
        foreach (var entity in GetEntities<TEntity>(lines, csvOptions))
        {
            yield return entity;
        }

        IEnumerable<string> GetLines()
        {
            csvStream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(csvStream, csvOptions?.Encoding ?? Encoding.UTF8);
            while (true)
            {
                var strLine = reader.ReadLine();
                if (strLine.IsNullOrEmpty())
                    yield break;

                yield return strLine;
            }
        }
    }

    /// <summary>
    ///     Parses CSV text and converts it to a strongly-typed entity list.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="csvText">CSV data as a string</param>
    /// <param name="csvOptions">Optional custom CSV parsing options</param>
    /// <returns>A list of entities populated from CSV data</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static List<TEntity?> GetEntityList<TEntity>(string csvText, CsvOptions? csvOptions = null)
        => GetEntities<TEntity>(csvText, csvOptions).ToList();

    /// <summary>
    ///     Parses CSV text and converts it to a lazy-loaded sequence of strongly-typed entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="csvText">CSV data as a string</param>
    /// <param name="csvOptions">Optional custom CSV parsing options</param>
    /// <returns>A lazy-loaded enumerable of entities</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static IEnumerable<TEntity?> GetEntities<TEntity>(string csvText, CsvOptions? csvOptions = null)
    {
        Guard.NotNull(csvText);
        var lines = GetLines();
        foreach (var entity in GetEntities<TEntity>(lines, csvOptions))
        {
            yield return entity;
        }

        IEnumerable<string> GetLines()
        {
            using var reader = new StringReader(csvText);
            while (true)
            {
                var strLine = reader.ReadLine();
                if (strLine.IsNullOrEmpty())
                    yield break;

                yield return strLine;
            }
        }
    }

    /// <summary>
    ///     Converts CSV lines to a strongly-typed entity list.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="csvLines">Enumerable collection of CSV lines</param>
    /// <param name="csvOptions">Optional custom CSV parsing options</param>
    /// <returns>A list of entities populated from CSV data</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static List<TEntity?> GetEntityList<TEntity>(IEnumerable<string> csvLines, CsvOptions? csvOptions = null)
        => GetEntities<TEntity>(csvLines, csvOptions).ToList();

    /// <summary>
    ///     Converts CSV lines to a lazy-loaded sequence of strongly-typed entities.
    ///     Supports both basic types and complex objects with property mapping.
    ///     For complex types, column headers are matched to property names or configured column titles.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to map CSV data to</typeparam>
    /// <param name="csvLines">Enumerable collection of CSV lines</param>
    /// <param name="csvOptions">Optional custom CSV parsing options</param>
    /// <returns>A lazy-loaded enumerable of entities</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static IEnumerable<TEntity?> GetEntities<TEntity>(IEnumerable<string> csvLines, CsvOptions? csvOptions = null)
    {
        if (csvLines is null)
        {
            throw new ArgumentNullException(nameof(csvLines));
        }
        csvOptions ??= CsvOptions.Default;
        var entityType = typeof(TEntity);
        if (entityType.IsBasicType())
        {
            var lines = csvOptions.IncludeHeader ? csvLines.Skip(1) : csvLines;
            foreach (var strLine in lines)
            {
                yield return strLine.To<TEntity>();
            }
        }
        else
        {
            var configuration = InternalHelper.GetExcelConfigurationMapping<TEntity>();
            var propertyColumnDictionary = InternalHelper.GetPropertyColumnDictionary<TEntity>();
            var propertyColumnDic = csvOptions.IncludeHeader
                ? propertyColumnDictionary.ToDictionary(p => p.Key, p => new PropertyConfiguration
                {
                    ColumnIndex = -1,
                    ColumnFormatter = p.Value.ColumnFormatter,
                    ColumnTitle = p.Value.ColumnTitle,
                    ColumnWidth = p.Value.ColumnWidth,
                    IsIgnored = p.Value.IsIgnored
                })
                : propertyColumnDictionary;
            var isFirstLine = csvOptions.IncludeHeader;
            var lineIndex = -1;
            foreach (var strLine in csvLines)
            {
                var cols = ParseLine(strLine, csvOptions);
                lineIndex++;
                if (isFirstLine)
                {
                    for (var index = 0; index < cols.Count; index++)
                    {
                        var setting = propertyColumnDic.GetPropertySetting(cols[index]);
                        if (setting is not null)
                        {
                            setting.ColumnIndex = index;
                        }
                    }

                    if (propertyColumnDic.Values.Any(p => p.ColumnIndex < 0))
                    {
                        propertyColumnDic = propertyColumnDictionary;
                    }

                    isFirstLine = false;
                }
                else
                {
                    var entity = NewFuncHelper<TEntity>.Instance();
                    if (entityType.IsValueType)
                    {
                        var obj = (object)entity!; // boxing for value types

                        foreach (var key in propertyColumnDic.Keys)
                        {
                            var colIndex = propertyColumnDic[key].ColumnIndex;
                            if (colIndex >= 0 && colIndex < cols.Count && key.CanWrite)
                            {
                                var columnValue = key.PropertyType.GetDefaultValue();
                                var valueApplied = false;
                                if (InternalCache.ColumnInputFormatterFuncCache.TryGetValue(key,
                                        out var formatterFunc) && formatterFunc?.Method is not null)
                                {
                                    var cellValue = cols[colIndex];
                                    try
                                    {
                                        // apply custom formatterFunc
                                        columnValue = formatterFunc.DynamicInvoke(cellValue);
                                        valueApplied = true;
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine(e);
                                        InvokeHelper.OnInvokeException?.Invoke(e);
                                    }
                                }

                                if (valueApplied == false)
                                {
                                    columnValue = cols[colIndex].ToOrDefault(key.PropertyType);
                                }

                                key.GetValueSetter()?.Invoke(entity!, columnValue);
                            }
                        }

                        entity = (TEntity)obj; // unboxing
                    }
                    else
                    {
                        foreach (var key in propertyColumnDic.Keys)
                        {
                            var colIndex = propertyColumnDic[key].ColumnIndex;
                            if (colIndex >= 0 && colIndex < cols.Count && key.CanWrite)
                            {
                                var columnValue = key.PropertyType.GetDefaultValue();

                                var valueApplied = false;
                                if (InternalCache.ColumnInputFormatterFuncCache.TryGetValue(key,
                                        out var formatterFunc) && formatterFunc?.Method is not null)
                                {
                                    var cellValue = cols[colIndex];
                                    try
                                    {
                                        // apply custom formatterFunc
                                        columnValue = formatterFunc.DynamicInvoke(cellValue);
                                        valueApplied = true;
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine(e);
                                        InvokeHelper.OnInvokeException?.Invoke(e);
                                    }
                                }

                                if (valueApplied == false)
                                {
                                    columnValue = cols[colIndex].ToOrDefault(key.PropertyType);
                                }

                                key.GetValueSetter()?.Invoke(entity!, columnValue);
                            }
                        }
                    }

                    if (entity is not null)
                    {
                        foreach (var propertyInfo in propertyColumnDic.Keys.Where(p => p.CanWrite))
                        {
                            var propertyValue = propertyInfo.GetValueGetter()?.Invoke(entity);
                            if (InternalCache.InputFormatterFuncCache.TryGetValue(propertyInfo,
                                    out var formatterFunc) && formatterFunc?.Method is not null)
                            {
                                try
                                {
                                    // apply custom formatterFunc
                                    var formattedValue = formatterFunc.DynamicInvoke(entity, propertyValue);
                                    propertyInfo.GetValueSetter()?.Invoke(entity, formattedValue);
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine(e);
                                    InvokeHelper.OnInvokeException?.Invoke(e);
                                }
                            }
                        }
                    }

                    if (configuration.DataFilter?.Invoke(entity) == false)
                    {
                        continue;
                    }

                    configuration.PostImportAction?.Invoke(entity, lineIndex);

                    yield return entity;
                }
            }
        }
    }

    /// <summary>
    ///     Parses a single CSV line into individual field values using default options.
    /// </summary>
    /// <param name="line">The CSV line to parse</param>
    /// <returns>A read-only list of field values</returns>
    public static IReadOnlyList<string> ParseLine(string line) => ParseLine(line, CsvOptions.Default);

    /// <summary>
    ///     Parses a single CSV line into individual field values with custom options.
    ///     Handles quoted values, escaped quotes, and separator characters within quoted fields.
    /// </summary>
    /// <param name="line">The CSV line to parse</param>
    /// <param name="csvOptions">Custom CSV parsing options (separator, quote character)</param>
    /// <returns>A read-only list of field values</returns>
    /// <exception cref="ArgumentException">Thrown when the line contains improperly escaped quotes</exception>
    public static IReadOnlyList<string> ParseLine(string line, CsvOptions csvOptions)
    {
        if (string.IsNullOrEmpty(line))
        {
            return Array.Empty<string>();
        }

        var columnBuilder = new StringBuilder();
        var fields = new List<string>();

        var inColumn = false;
        var inQuotes = false;

        // Iterate through every character in the line
        for (var i = 0; i < line.Length; i++)
        {
            var character = line[i];

            // If we are not currently inside a column
            if (!inColumn)
            {
                // If the current character is a double quote then the column value is contained within
                // double quotes, otherwise append the next character
                inColumn = true;
                if (character == csvOptions.QuoteCharacter)
                {
                    inQuotes = true;
                    continue;
                }
            }

            // If we are in between double quotes
            if (inQuotes)
            {
                if (i + 1 == line.Length)
                {
                    break;
                }

                if (character == csvOptions.QuoteCharacter && line[i + 1] == csvOptions.SeparatorCharacter) // quotes end
                {
                    inQuotes = false;
                    inColumn = false;
                    i++; //skip next
                }
                else if (character == csvOptions.QuoteCharacter && line[i + 1] == csvOptions.QuoteCharacter) // quotes
                {
                    i++; //skip next
                }
                else if (character == csvOptions.QuoteCharacter)
                {
                    throw new ArgumentException($"unable to escape {line}");
                }
            }
            else if (character == csvOptions.SeparatorCharacter)
            {
                inColumn = false;
            }

            // If we are no longer in the column clear the builder and add the columns to the list
            if (!inColumn)
            {
                fields.Add(columnBuilder.ToString());
                columnBuilder.Clear();
            }
            else // append the current column
            {
                columnBuilder.Append(character);
            }
        }

        fields.Add(columnBuilder.ToString());

        return fields;
    }

    /// <summary>
    ///     Saves a collection of entities to a CSV file with default options (includes header).
    /// </summary>
    /// <typeparam name="TEntity">The entity type to export</typeparam>
    /// <param name="entities">The collection of entities to export</param>
    /// <param name="filePath">The destination file path</param>
    /// <returns>True if the file was successfully created; otherwise, false</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static bool ToCsvFile<TEntity>(this IEnumerable<TEntity> entities, string filePath) =>
        ToCsvFile(entities, filePath, CsvOptions.Default);

    /// <summary>
    ///     Saves a collection of entities to a CSV file with optional header.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to export</typeparam>
    /// <param name="entities">The collection of entities to export</param>
    /// <param name="filePath">The destination file path</param>
    /// <param name="includeHeader">Whether to include property names as column headers</param>
    /// <returns>True if the file was successfully created; otherwise, false</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static bool ToCsvFile<TEntity>(this IEnumerable<TEntity> entities, string filePath, bool includeHeader)
    {
        return ToCsvFile(Guard.NotNull(entities), filePath, includeHeader ? CsvOptions.Default : new CsvOptions()
        {
            IncludeHeader = false
        });
    }

    /// <summary>
    ///     Saves a collection of entities to a CSV file with custom CSV options.
    ///     Property values are formatted according to configured output formatters.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to export</typeparam>
    /// <param name="entities">The collection of entities to export</param>
    /// <param name="filePath">The destination file path</param>
    /// <param name="csvOptions">Custom CSV formatting options</param>
    /// <returns>True if the file was successfully created; otherwise, false</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static bool ToCsvFile<TEntity>(this IEnumerable<TEntity> entities, string filePath, CsvOptions csvOptions)
    {
        if (entities is null)
        {
            throw new ArgumentNullException(nameof(entities));
        }
        Guard.NotNull(csvOptions);

        var csvTextData = GetCsvText(entities, csvOptions);
        if (csvTextData.IsNullOrEmpty())
        {
            return false;
        }

        var dir = Path.GetDirectoryName(filePath);
        if (dir.IsNotNullOrEmpty())
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        File.WriteAllText(filePath, csvTextData, csvOptions.Encoding);
        return true;
    }

    /// <summary>
    ///     Asynchronously saves a collection of entities to a CSV file.
    ///     This method is more memory-efficient for large collections as it streams lines to the file.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to export</typeparam>
    /// <param name="entities">The collection of entities to export</param>
    /// <param name="filePath">The destination file path</param>
    /// <param name="csvOptions">Optional custom CSV formatting options</param>
    /// <returns>A task that represents the asynchronous operation, containing true if successful</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static async Task<bool> ToCsvFileAsync<TEntity>(this IEnumerable<TEntity> entities, string filePath, CsvOptions? csvOptions = null)
    {
        if (entities is null)
        {
            throw new ArgumentNullException(nameof(entities));
        }

        csvOptions ??= CsvOptions.Default;

        InternalHelper.EnsureFileIsNotReadOnly(filePath);
        var dir = Path.GetDirectoryName(filePath);
        if (dir.IsNotNullOrEmpty())
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        var lines = GetCsvLines(entities, csvOptions);
        using var file = File.CreateText(filePath);
        foreach (var line in lines)
        {
            await file.WriteLineAsync(line).ConfigureAwait(false);
        }
        return true;
    }

    /// <summary>
    ///     Converts a collection of entities to CSV formatted byte array with default encoding (includes header).
    /// </summary>
    /// <typeparam name="TEntity">The entity type to convert</typeparam>
    /// <param name="entities">The collection of entities to convert</param>
    /// <returns>CSV data as a byte array</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static byte[] ToCsvBytes<TEntity>(this IEnumerable<TEntity> entities) => ToCsvBytes(entities, CsvOptions.Default);

    /// <summary>
    ///     Converts a collection of entities to CSV formatted byte array with optional header.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to convert</typeparam>
    /// <param name="entities">The collection of entities to convert</param>
    /// <param name="includeHeader">Whether to include property names as column headers</param>
    /// <returns>CSV data as a byte array</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static byte[] ToCsvBytes<TEntity>(this IEnumerable<TEntity> entities, bool includeHeader) =>
        GetCsvText(entities, includeHeader).GetBytes();

    /// <summary>
    ///     Converts a collection of entities to CSV formatted byte array with custom options.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to convert</typeparam>
    /// <param name="entities">The collection of entities to convert</param>
    /// <param name="csvOptions">Custom CSV formatting options</param>
    /// <returns>CSV data as a byte array</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static byte[] ToCsvBytes<TEntity>(this IEnumerable<TEntity> entities, CsvOptions csvOptions) =>
        GetCsvText(entities, csvOptions).GetBytes(csvOptions.Encoding);

    /// <summary>
    ///     Converts a collection of entities to CSV formatted text with optional header (default includes header).
    /// </summary>
    /// <typeparam name="TEntity">The entity type to convert</typeparam>
    /// <param name="entities">The collection of entities to convert</param>
    /// <param name="includeHeader">Whether to include property names as column headers</param>
    /// <returns>CSV data as a string</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static string GetCsvText<TEntity>(this IEnumerable<TEntity> entities, bool includeHeader = true)
    {
        return GetCsvText(Guard.NotNull(entities), includeHeader ? CsvOptions.Default : new CsvOptions()
        {
            IncludeHeader = false
        });
    }

    /// <summary>
    ///     Converts a collection of entities to CSV formatted text with custom options.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to convert</typeparam>
    /// <param name="entities">The collection of entities to convert</param>
    /// <param name="csvOptions">Custom CSV formatting options</param>
    /// <returns>CSV data as a string</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static string GetCsvText<TEntity>(this IEnumerable<TEntity> entities, CsvOptions csvOptions) =>
        GetCsvLines(entities, csvOptions).StringJoin(Environment.NewLine);

    /// <summary>
    ///     Converts a collection of entities to a sequence of CSV formatted lines.
    ///     For basic types, each entity is converted to a single line.
    ///     For complex types, properties are mapped to columns with proper CSV escaping.
    ///     Values containing separator characters are automatically quoted.
    /// </summary>
    /// <param name="entities">The collection of entities to convert</param>
    /// <param name="csvOptions">Optional custom CSV formatting options</param>
    /// <typeparam name="TEntity">The entity type to convert</typeparam>
    /// <returns>CSV formatted lines</returns>
    [RequiresUnreferencedCode(AotCompatibilityMessages.ReflectionMapping)]
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static IEnumerable<string> GetCsvLines<TEntity>(this IEnumerable<TEntity> entities, CsvOptions? csvOptions = null)
    {
        if (entities is null)
        {
            throw new ArgumentNullException(nameof(entities));
        }
        csvOptions ??= CsvOptions.Default;

        var isBasicType = typeof(TEntity).IsBasicType();
        if (isBasicType)
        {
            if (csvOptions.IncludeHeader)
            {
                yield return csvOptions.PropertyNameForBasicType;
            }
            foreach (var entity in entities)
            {
                if (entity is IFormattable formattableEntity)
                    yield return formattableEntity.ToString(null, null) ?? string.Empty;
                else
                    yield return Convert.ToString(entity) ?? string.Empty;
            }
        }
        else
        {
            var dic = InternalHelper.GetPropertyColumnDictionary<TEntity>();
            var props = InternalHelper.GetPropertiesForCsvHelper<TEntity>();
            if (csvOptions.IncludeHeader)
            {
                yield return Enumerable.Range(0, props.Count)
                    .Select(i => dic[props[i]].ColumnTitle)
                    .StringJoin(csvOptions.SeparatorString);
            }

            foreach (var entity in entities)
            {
                var line = GetCsvLine().StringJoin(csvOptions.SeparatorString);
                yield return line;

                IEnumerable<string> GetCsvLine()
                {
                    for (var i = 0; i < props.Count; i++)
                    {
                        var propertyValue = props[i].GetValueGetter<TEntity>()?.Invoke(entity);
                        if (InternalCache.OutputFormatterFuncCache.TryGetValue(props[i], out var formatterFunc) &&
                            formatterFunc?.Method is not null)
                        {
                            try
                            {
                                // apply custom formatterFunc
                                propertyValue = formatterFunc.DynamicInvoke(entity, propertyValue);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e);
                                InvokeHelper.OnInvokeException?.Invoke(e);
                            }
                        }

                        // https://stackoverflow.com/questions/4617935/is-there-a-way-to-include-commas-in-csv-columns-without-breaking-the-formatting
                        var val = propertyValue?.ToString()?.Replace(
                            csvOptions.QuoteString,
                            $"{csvOptions.QuoteString}{csvOptions.QuoteString}"
                        );
                        if (val is { Length: > 0 })
                        {
                            yield return val.IndexOf(csvOptions.SeparatorCharacter) > -1 ? $"{csvOptions.QuoteCharacter}{val}{csvOptions.QuoteCharacter}" : val;
                        }
                        else
                        {
                            yield return string.Empty;
                        }
                    }
                }
            }
        }
    }

}
