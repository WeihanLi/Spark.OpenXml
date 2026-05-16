// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using WeihanLi.Extensions;

namespace Spark.OpenXml.Settings;

/// <summary>
///     Excel Sheet Settings
/// </summary>
public sealed class SheetSetting
{
    /// <summary>
    ///     SheetName
    /// </summary>
    public string SheetName
    {
        get;
        set
        {
            if (value.IsNotNullOrWhiteSpace())
            {
                field = value;
            }
        }
    } = "Sheet0";

    /// <summary>
    ///     StartRowIndex
    /// </summary>
    public int StartRowIndex
    {
        get;
        set
        {
            if (value >= 0)
            {
                field = value;
            }
        }
    } = 1;

    /// <summary>
    ///     HeaderRowIndex
    /// </summary>
    public int HeaderRowIndex => StartRowIndex - 1;

    /// <summary>
    ///     EndRowIndex, included
    /// </summary>
    public int? EndRowIndex { get; set; }

    /// <summary>
    ///    Gets or set whether to enable auto column width, disabled by default.
    /// </summary>
    public bool AutoColumnWidthEnabled { get; set; }

    /// <summary>
    ///    Gets or sets whether to skip column index adjustment based on header row during import.
    ///    When false (default), column indices are automatically adjusted based on header row.
    ///    When true, column indices are used as-is.
    /// </summary>
    public bool SkipHeaderRow { get; set; }

    /// <summary>
    ///     StartColumnIndex when importing.
    /// </summary>
    public int StartColumnIndex { get; set; }

    /// <summary>
    ///     EndColumnIndex when importing. A negative value means no upper bound.
    /// </summary>
    public int EndColumnIndex { get; set; } = -1;

    internal bool IncludesColumn(int columnIndex) =>
        columnIndex >= StartColumnIndex && (EndColumnIndex < StartColumnIndex || columnIndex <= EndColumnIndex);
}
