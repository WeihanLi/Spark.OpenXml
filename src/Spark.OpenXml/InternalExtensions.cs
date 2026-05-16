// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using System.Globalization;
using System.Reflection;
using WeihanLi.Common;
using WeihanLi.Common.Models;
using WeihanLi.Common.Services;
using WeihanLi.Extensions;
using Spark.OpenXml.Configurations;

namespace Spark.OpenXml;

internal static class InternalExtensions
{
    /// <summary>
    ///     Parse obj to paramDictionary
    /// </summary>
    /// <param name="paramInfo">param object</param>
    /// <returns></returns>
    public static IDictionary<string, object?> ParseParamInfo(this object? paramInfo)
    {
        var paramDic = paramInfo.ParseParamDictionary();
        return paramDic;
    }

    /// <summary>
    ///     Wraps a strongly typed validator so it can be used without generics.
    /// </summary>
    /// <typeparam name="T">Entity type handled by the validator.</typeparam>
    /// <param name="validator">Type-specific validator.</param>
    /// <returns>Validator that operates on <see cref="object" /> instances.</returns>
    public static IValidator GetCommonValidator<T>(this IValidator<T> validator)
    {
        return new CustomValidator(o =>
        {
            if (o is T t)
            {
                return validator.Validate(t);
            }
            return ValidationResult.Failed("Invalid value");
        });
    }

    /// <summary>
    ///     GetPropertySettingByPropertyName
    /// </summary>
    /// <param name="mappingDictionary">mappingDictionary</param>
    /// <param name="propertyName">propertyName</param>
    /// <returns></returns>
    internal static PropertyConfiguration? GetPropertySettingByPropertyName(
        this IDictionary<PropertyInfo, PropertyConfiguration> mappingDictionary, string propertyName)
        => mappingDictionary.Values.FirstOrDefault(c => c.PropertyName.EqualsIgnoreCase(propertyName));

    /// <summary>
    ///     GetPropertyConfigurationByColumnName
    /// </summary>
    /// <param name="mappingDictionary">mappingDictionary</param>
    /// <param name="columnTitle">columnTitle</param>
    /// <returns></returns>
    internal static PropertyConfiguration? GetPropertySetting(
        this IDictionary<PropertyInfo, PropertyConfiguration> mappingDictionary, string columnTitle) =>
        mappingDictionary.Values.FirstOrDefault(k => k.ColumnTitle.EqualsIgnoreCase(columnTitle)) ??
        mappingDictionary.GetPropertySettingByPropertyName(columnTitle);

    private sealed class CustomValidator(Func<object?, ValidationResult> func) : IValidator
    {
        private readonly Func<object?, ValidationResult> _func = Guard.NotNull(func);

        /// <summary>
        ///     Executes the wrapped validation delegate.
        /// </summary>
        /// <param name="value">Value to validate.</param>
        /// <returns>Validation result.</returns>
        public ValidationResult Validate(object? value)
        {
            return _func.Invoke(value);
        }
    }
}
