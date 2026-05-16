// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using System.Diagnostics.CodeAnalysis;

namespace Spark.OpenXml;

internal static class AotCompatibilityMessages
{
    public const DynamicallyAccessedMemberTypes EntityAccessedMembers =
        DynamicallyAccessedMemberTypes.PublicProperties |
        DynamicallyAccessedMemberTypes.PublicParameterlessConstructor;

    public const string ReflectionMapping =
        "Entity mapping uses reflection over runtime types and is not fully compatible with trimming. Use explicit mappings and preserve entity members for Native AOT scenarios.";

    public const string DynamicGenericMapping =
        "Entity mapping creates closed generic configuration types at runtime, which can require native code that is not available under Native AOT.";

    public const string MappingProfileDiscovery =
        "Mapping profile discovery scans assemblies and creates profile instances dynamically. Use LoadMappingProfile<TEntity, TMappingProfile>() for explicit registration.";

    public const string RuntimeConversion =
        "Runtime value conversion can use TypeConverter paths that are not fully compatible with trimming.";
}
