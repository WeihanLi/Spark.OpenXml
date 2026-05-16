# Introduction

`Spark.OpenXml` is an Open XML SDK based Excel and CSV import/export library.

It is the OpenXML rewrite of `Spark.OpenXml`. The package keeps high-level entity, `DataTable`, `DataSet`, template, formatter, validation, multi-sheet, and CSV workflows. Public NPOI workbook/sheet/row/cell APIs were removed because they do not map cleanly to `DocumentFormat.OpenXml`.

Supported Excel files are `.xlsx` only. `.xls`, NPOI object callbacks, formula evaluation, and image import/export are not supported.
