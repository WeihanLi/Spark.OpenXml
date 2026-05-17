// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using Spark.OpenXml.Configurations;
using System.Diagnostics.CodeAnalysis;
using WeihanLi.Common.Services;

namespace Spark.OpenXml;

/// <summary>
/// Provides convenience extension methods for configuring Excel import/export metadata.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    ///     excel data validator
    /// </summary>
    /// <param name="configuration">configuration</param>
    /// <param name="validator">validator</param>
    /// <typeparam name="TEntity">entity type</typeparam>
    /// <returns>current configuration</returns>
    public static IExcelConfiguration<TEntity> WithValidator<TEntity>(this IExcelConfiguration<TEntity> configuration,
        IValidator<TEntity> validator)
    {
        return configuration.WithValidator(validator.GetCommonValidator());
    }

    /// <summary>
    ///     property configuration
    /// </summary>
    /// <typeparam name="TEntity">TEntity</typeparam>
    /// <param name="excelConfiguration">excelConfiguration</param>
    /// <param name="propertyName">propertyName</param>
    /// <returns>PropertyConfiguration</returns>
    [RequiresDynamicCode(AotCompatibilityMessages.DynamicGenericMapping)]
    public static IPropertyConfiguration<TEntity, string> Property<
        [DynamicallyAccessedMembers(AotCompatibilityMessages.EntityAccessedMembers)] TEntity>(
        this IExcelConfiguration<TEntity> excelConfiguration, string propertyName) =>
        excelConfiguration.Property<string>(propertyName);

    /// <summary>
    ///     has column output formatter
    /// </summary>
    /// <typeparam name="TEntity">entity type</typeparam>
    /// <typeparam name="TProperty">property type</typeparam>
    /// <param name="configuration">property configuration</param>
    /// <param name="formatter">column output formatter</param>
    /// <returns>property configuration</returns>
    public static IPropertyConfiguration<TEntity, TProperty> HasColumnOutputFormatter<TEntity, TProperty>(
        this IPropertyConfiguration<TEntity, TProperty> configuration, Func<TProperty?, object?>? formatter)
    {
        if (formatter is null)
        {
            return configuration.HasOutputFormatter(null);
        }

        return configuration.HasOutputFormatter((_, prop) => formatter.Invoke(prop));
    }
}
