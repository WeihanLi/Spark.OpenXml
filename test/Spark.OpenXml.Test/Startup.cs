// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using Spark.OpenXml.Test.MappingProfiles;

namespace Spark.OpenXml.Test;

public class Startup
{
    public void Configure()
    {
        AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);
        // ---------- load excel mapping profiles ----------------
        FluentSettings.LoadMappingProfiles(typeof(NoticeProfile).Assembly);
    }
}
