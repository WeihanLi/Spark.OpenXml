# Spark.OpenXml Release Notes

## [1.0.0]

- Rewrote Excel import/export on top of `DocumentFormat.OpenXml`.
- Released as the new package `Spark.OpenXml`.
- Kept high-level entity, template, formatter, validation, multi-sheet, and CSV workflows.
- Changed Excel support to `.xlsx` only.
- Removed public NPOI workbook/sheet/row/cell APIs.
- Removed NPOI-specific callbacks and filters such as `RowAction`, `CellAction`, `SheetAction`, `RowFilter`, and `CellFilter`.
- Removed `.xls`, formula evaluation, and image import/export support.
