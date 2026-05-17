// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the Apache license.

using Spark.OpenXml.Configurations;
using Spark.OpenXml.Test.Models;
using WeihanLi.Extensions;

namespace Spark.OpenXml.Test.MappingProfiles;

public sealed class NoticeProfile : IMappingProfile<Notice>
{
    public void Configure(IExcelConfiguration<Notice> noticeSetting)
    {
        noticeSetting.HasExcelSetting(excel =>
            {
                excel.Author = "Spark";
                excel.Title = "Notice";
                excel.Description = "Spark.OpenXml test";
            })
            .HasSheetSetting(setting =>
            {
                setting.SheetName = "NoticeList";
                setting.AutoColumnWidthEnabled = true;
            })
            ;
        noticeSetting.Property(x => x.Id)
            .HasColumnIndex(0);
        noticeSetting.Property(x => x.Title)
            .HasColumnIndex(1);
        noticeSetting.Property(x => x.Content)
            .HasColumnIndex(2);
        noticeSetting.Property(x => x.Publisher)
            .HasColumnIndex(3);
        noticeSetting.Property(x => x.PublishedAt)
            .HasColumnIndex(4)
            .HasColumnOutputFormatter(x => x.ToTimeString());
    }
}
