# 样式

`Spark.OpenXml` 支持基础 workbook 元数据、列宽、冻结窗格和自动筛选设置。

`Spark.OpenXml` 中的 NPOI 样式回调已移除，包括 `RowAction`、`CellAction` 和 `SheetAction`。新的公开 API 不再暴露 workbook、sheet、row 或 cell 对象。

如果需要高级 OpenXML 样式，可以在生成 `.xlsx` 后使用 `DocumentFormat.OpenXml` 做后处理。
