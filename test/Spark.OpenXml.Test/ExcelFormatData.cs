// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using Xunit;

namespace Spark.OpenXml.Test;

public sealed class ExcelFormatData : TheoryData<ExcelFormat>
{
    public ExcelFormatData()
    {
        Add(ExcelFormat.Xlsx);
    }
}
