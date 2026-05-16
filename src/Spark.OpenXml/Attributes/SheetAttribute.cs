// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using Spark.OpenXml.Settings;

namespace Spark.OpenXml.Attributes;

/// <summary>
/// Declares per-sheet metadata for an entity mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SheetAttribute : Attribute
{
    private int _endColumnIndex = -1;

    private int _startColumnIndex;

    /// <summary>
    ///     Initializes an attribute backed by a fresh <see cref="SheetSetting" />.
    /// </summary>
    public SheetAttribute() => SheetSetting = new SheetSetting();

    /// <summary>
    ///     Target sheet index (zero-based).
    /// </summary>
    public int SheetIndex { get; set; }

    /// <summary>
    ///     Gets or sets the sheet name override.
    /// </summary>
    public string SheetName
    {
        get => SheetSetting.SheetName;
        set => SheetSetting.SheetName = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     Gets or sets the first row to start reading/writing (zero-based).
    /// </summary>
    public int StartRowIndex
    {
        get => SheetSetting.StartRowIndex;
        set => SheetSetting.StartRowIndex = value;
    }

    /// <summary>
    ///     Gets the header row index.
    /// </summary>
    public int HeaderRowIndex => SheetSetting.HeaderRowIndex;

    /// <summary>
    ///     Gets or sets the last row (inclusive) participating in the mapping.
    /// </summary>
    public int EndRowIndex
    {
        get => SheetSetting.EndRowIndex ?? -1;
        set => SheetSetting.EndRowIndex = value >= 0 ? value : -1;
    }

    /// <summary>
    ///     StartColumnIndex
    ///     Start Column Index when import
    /// </summary>
    public int StartColumnIndex
    {
        get => _startColumnIndex;
        set
        {
            if (value >= 0)
            {
                _startColumnIndex = value;
                SheetSetting.StartColumnIndex = _startColumnIndex;
                SheetSetting.EndColumnIndex = _endColumnIndex;
            }
        }
    }

    /// <summary>
    ///     EndColumnIndex
    ///     End Column Index when import
    /// </summary>
    public int EndColumnIndex
    {
        get => _endColumnIndex;
        set
        {
            if (value >= _startColumnIndex)
            {
                _endColumnIndex = value;
                SheetSetting.StartColumnIndex = _startColumnIndex;
                SheetSetting.EndColumnIndex = _endColumnIndex;
            }
            else
            {
                _endColumnIndex = -1;
                SheetSetting.StartColumnIndex = _startColumnIndex;
                SheetSetting.EndColumnIndex = _endColumnIndex;
            }
        }
    }

    /// <summary>
    ///     Gets or sets whether column widths should be auto-sized.
    /// </summary>
    public bool AutoColumnWidthEnabled
    {
        get => SheetSetting.AutoColumnWidthEnabled;
        set => SheetSetting.AutoColumnWidthEnabled = value;
    }

    internal SheetSetting SheetSetting { get; }
}
