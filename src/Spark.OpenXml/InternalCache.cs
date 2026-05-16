// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using System.Collections.Concurrent;
using System.Reflection;
using Spark.OpenXml.Configurations;

namespace Spark.OpenXml;

internal static class InternalCache
{
    /// <summary>
    ///     TypeExcelConfigurationCache
    /// </summary>
    public static readonly ConcurrentDictionary<Type, IExcelConfiguration> TypeExcelConfigurationDictionary = new();

    /// <summary>
    ///     Cacheable delegates that format cell values when exporting.
    /// </summary>
    public static readonly ConcurrentDictionary<PropertyInfo, Delegate?> OutputFormatterFuncCache = new();

    /// <summary>
    ///     Cacheable delegates that post-process property values after import.
    /// </summary>
    public static readonly ConcurrentDictionary<PropertyInfo, Delegate?> InputFormatterFuncCache = new();

    /// <summary>
    ///     Cacheable delegates that pre-process property values before the column formatter runs.
    /// </summary>
    public static readonly ConcurrentDictionary<PropertyInfo, Delegate?> ColumnInputFormatterFuncCache = new();

}
