# Styling

`Spark.OpenXml` writes basic workbook metadata and supports column width, freeze panes, and auto filter settings.

The NPOI style customization callbacks from `Spark.OpenXml`, including `RowAction`, `CellAction`, and `SheetAction`, were removed. Direct workbook, sheet, row, and cell object styling is not part of the public API.

If advanced OpenXML styling is required, post-process the generated `.xlsx` with `DocumentFormat.OpenXml`.
