# WeihanLi.Npoi Parity

This article tracks user-visible WeihanLi.Npoi capabilities against
Spark.OpenXml. The goal is not to copy NPOI object-model APIs. Spark.OpenXml
should support capabilities that can be represented safely through Excel Open XML
workbooks or CSV workflows.

## Research Scope

Reviewed public package and indexed repository documentation for
WeihanLi.Npoi 3.4.0 and current Spark.OpenXml repository summaries. Local command
execution was unavailable during this pass, so source-level verification must be
completed before implementing code gaps.

Primary source references:

- WeihanLi.Npoi GitHub repository: `https://github.com/WeihanLi/WeihanLi.Npoi`
- WeihanLi.Npoi NuGet package: `https://www.nuget.org/packages/WeihanLi.Npoi`
- Spark.OpenXml GitHub repository: `https://github.com/WeihanLi/Spark.OpenXml`
- Spark.OpenXml README: `https://github.com/WeihanLi/Spark.OpenXml/blob/main/README.md`

## Status Values

| Status | Meaning |
| --- | --- |
| Already supported | Spark.OpenXml is documented as supporting equivalent high-level behavior. |
| Missing and in scope | No equivalent Spark.OpenXml behavior is known yet, and the behavior appears feasible with Open XML or CSV. |
| Partially supported | Spark.OpenXml appears to support part of the behavior, but source-level verification or additional behavior is needed. |
| Excluded | The capability is NPOI-specific, unsafe, or outside Open XML/CSV scope. |
| Deferred | The capability needs source-level verification before final classification. |

## Parity Inventory

| Source capability | Workflow | Spark.OpenXml status | Evidence | Rationale |
| --- | --- | --- | --- | --- |
| Import Excel to `List<TEntity>` or `IEnumerable<TEntity>` | Import | Already supported | Spark.OpenXml repository summary lists entity import/export. | High-level entity import maps to Open XML workbook reading. |
| Import Excel to `DataTable` | Import | Already supported | Spark.OpenXml repository summary lists `DataTable` import/export. | DataTable import is a core Spark.OpenXml workflow. |
| Import CSV to entities | CSV | Already supported | Spark.OpenXml repository summary lists CSV handling. | CSV entity import is format-independent from NPOI. |
| Import CSV to `DataTable` | CSV | Already supported | Spark.OpenXml repository summary lists CSV handling and `DataTable` workflows. | CSV DataTable import is feasible and documented as supported. |
| Custom header row selection | Import | Partially supported | WeihanLi.Npoi package lists custom header rows; Spark.OpenXml details need source verification. | Open XML supports row-based reads, but exact public options must be confirmed. |
| Sheet selection by index or name | Import | Partially supported | WeihanLi.Npoi package lists sheet selection; Spark.OpenXml lists multiple sheets. | Multiple-sheet support exists, but exact import selection coverage must be verified. |
| Automatic type conversion and data mapping | Import | Already supported | Spark.OpenXml repository summary lists mapping/configuration and import/export. | Type conversion and mapping are expected high-level import behavior. |
| Export `IEnumerable<TEntity>` to Excel | Export | Already supported | Spark.OpenXml repository summary lists entity import/export. | High-level entity export maps to Open XML workbook writing. |
| Export `DataTable` to Excel | Export | Already supported | Spark.OpenXml repository summary lists `DataTable` import/export. | DataTable export is a core Spark.OpenXml workflow. |
| Export Excel to file, bytes, or stream | Export | Partially supported | WeihanLi.Npoi package lists bytes and streams; Spark.OpenXml exact overloads need source verification. | Open XML supports stream and file output, but public overload parity must be checked. |
| Export CSV to file or byte array | CSV | Partially supported | WeihanLi.Npoi package lists CSV files/bytes; Spark.OpenXml lists CSV handling. | CSV export exists, but overload and byte-array parity must be verified. |
| Template-based export with placeholders | Template | Already supported | Spark.OpenXml repository summary lists templates. | Placeholder template export is feasible with Open XML documents. |
| Multi-sheet export in one workbook | Multi-sheet | Already supported | Spark.OpenXml repository summary lists multiple sheets. | Open XML workbooks natively support multiple worksheets. |
| Attribute configuration with column and sheet metadata | Configuration | Already supported | Spark.OpenXml repository summary lists attribute configuration. | Attribute-based mapping is a high-level configuration feature. |
| Fluent API configuration | Configuration | Already supported | Spark.OpenXml repository summary lists Fluent API configuration. | Fluent mapping is a high-level configuration feature. |
| Custom column mapping | Configuration | Already supported | Spark.OpenXml repository summary lists mapping/configuration. | Mapping to workbook columns is within Open XML scope. |
| InputFormatter and OutputFormatter | Formatting | Already supported | Spark.OpenXml repository summary lists formatters. | Import/export value transformation is not NPOI-specific. |
| ColumnInputFormatter and ColumnOutputFormatter | Formatting | Partially supported | WeihanLi.Npoi package lists column-specific transformations; Spark.OpenXml lists formatters. | Exact column-level formatter parity needs source verification. |
| CellReader custom cell reading logic | Import | Deferred | WeihanLi.Npoi 2.1.0 release notes mention `HasCellReader`; Spark.OpenXml source must be checked. | Open XML can expose cell values, but a custom reader contract may require new public API. |
| Shadow properties or columns not present on the model | Configuration | Already supported | WeihanLi.Npoi package lists shadow properties; Spark.OpenXml repository summary also lists the feature. | This is a mapping/configuration concern, not an NPOI-only feature. |
| Column width configuration | Formatting | Deferred | WeihanLi.Npoi update notes describe attribute and Fluent API column width configuration. | Open XML supports column widths, but Spark.OpenXml option parity must be verified. |
| Auto column width | Formatting | Deferred | WeihanLi.Npoi update notes describe sheet-level auto column width. | Open XML stores widths, but true auto-fit can be environment-dependent and must be reviewed. |
| Column formatter string such as date/time format | Formatting | Already supported | Spark.OpenXml repository summary lists formatters. | Number/date format output is feasible with Open XML styles. |
| RowFilter and CellFilter for import | Filtering | Deferred | WeihanLi.Npoi release notes mention RowFilter and CellFilter support. | Filtering imported rows/cells is feasible, but public hook parity must be checked. |
| Freeze panes | Workbook configuration | Already supported | Spark.OpenXml repository summary lists freeze panes. | Freeze panes are native Open XML worksheet view settings. |
| Workbook properties such as author, title, description, subject | Workbook configuration | Deferred | WeihanLi.Npoi examples show workbook metadata configuration. | Open XML supports core properties, but Spark.OpenXml public options need verification. |
| Load Excel from file path | Import | Already supported | Spark.OpenXml repository summary lists Excel import. | File-based workbook import is a standard library workflow. |
| Load Excel from byte array or stream | Import | Partially supported | WeihanLi.Npoi update notes list stream and byte-array overloads. | Open XML supports stream input, but public overload parity must be verified. |
| Read workbook while file is opened by another process | Import | Excluded | WeihanLi.Npoi release notes list this as a file access feature. | This depends on file sharing behavior rather than Open XML workbook semantics. |
| Public NPOI workbook, sheet, row, and cell extension APIs | NPOI object model | Excluded | Spark.OpenXml repository summary says NPOI workbook/sheet/row/cell APIs are removed when they do not map cleanly to Open XML SDK. | These APIs expose NPOI types and are intentionally outside Spark.OpenXml scope. |
| `.xls` binary workbook support | Workbook format | Excluded | WeihanLi.Npoi supports `.xls` through NPOI; Spark.OpenXml is an Open XML rewrite. | `.xls` is not an Open XML workbook format. |

## Import

Spark.OpenXml should retain entity and `DataTable` import, custom mapping, header
row handling, sheet selection, formatters, filters, and safe cell-reading hooks
where they map to Open XML workbook reads.

Implementation follow-up:

- Verify custom header row and sheet selection public options.
- Verify or add column-level input formatter parity.
- Verify or add safe custom cell reader parity.
- Verify or add row and cell import filter parity.

## Export

Spark.OpenXml should retain entity and `DataTable` export to `.xlsx` workbooks,
including file, stream, and byte-array workflows where public APIs support them.

Implementation follow-up:

- Verify file, stream, and byte-array export overloads.
- Verify DataTable and entity paths for each shared export feature.

## Template

Spark.OpenXml should retain placeholder-based template export when the template is
an Open XML workbook.

Implementation follow-up:

- Verify placeholder syntax and examples against current Spark.OpenXml docs.
- Exclude NPOI-only template behavior that depends on NPOI workbook APIs.

## Formatting

Spark.OpenXml should support value formatters, column-level transformations,
column widths, and workbook styles that are expressible through Open XML styles.

Implementation follow-up:

- Verify column-level input/output formatter parity.
- Verify column width and date/time format configuration.
- Decide whether auto column width can be supported deterministically.

## Filtering

Spark.OpenXml should support row and cell import filters if they can be expressed
as safe callbacks over parsed workbook values.

Implementation follow-up:

- Verify current filter APIs.
- Add tests for skipped rows, skipped cells, null values, and formatter ordering
  if filter gaps are found.

## Multi-Sheet

Spark.OpenXml should retain multiple-sheet export and sheet selection for import.
Features that operate across sheets must be tested for both entity and
`DataTable` workflows when shared.

## Freeze Panes

Spark.OpenXml should retain freeze pane configuration because it maps directly to
Open XML worksheet view settings.

## CSV

Spark.OpenXml should retain CSV import/export for entities and `DataTable`
workflows, including delimiter, header, quoting, null, empty, and round-trip
behavior where affected by parity changes.

Implementation follow-up:

- Verify byte-array and stream overload parity.
- Verify column-level formatter parity for CSV.

## Migration Examples

### Entity import/export

When a WeihanLi.Npoi workflow imports an Excel sheet into entity objects or
exports entity objects to an Excel workbook, migrate it to the equivalent
Spark.OpenXml entity import/export workflow. Keep the mapping configuration close
to the model when the mapping is stable, and use Fluent API configuration when
the workbook shape varies by scenario.

Migration status: supported for high-level entity workflows; source-level
verification is still required for overload parity such as byte arrays and
streams.

### DataTable import/export

When a WeihanLi.Npoi workflow uses `DataTable`, migrate it to the Spark.OpenXml
`DataTable` import/export workflow. Verify column order, headers, and value
formatting for each workbook that relies on positional mapping.

Migration status: supported for high-level `DataTable` workflows; shared
features must be tested through both entity and `DataTable` paths when changed.

### Template export

When a WeihanLi.Npoi workflow fills an Excel template, migrate only templates
that are Open XML workbooks. Keep placeholder behavior documented beside the
template so unsupported NPOI workbook object-model assumptions do not leak into
the Spark.OpenXml version.

Migration status: supported for Open XML template workflows; exclude templates
that depend on public NPOI workbook, sheet, row, or cell APIs.

### CSV workflows

When a WeihanLi.Npoi workflow imports or exports CSV, migrate the scenario by
checking delimiter, header, quoting, null value, empty value, and round-trip
expectations. Treat CSV parity as a separate workflow from Excel workbook parity
because formatting and escaping behavior are user-visible.

Migration status: supported for high-level CSV workflows; stream and byte-array
overload parity still needs source-level verification.

## Workbook Configuration

Spark.OpenXml should support workbook metadata and worksheet configuration when
Open XML exposes a deterministic representation.

Implementation follow-up:

- Verify support for author, title, description, and subject metadata.
- Exclude file-access features that depend on NPOI or operating-system sharing
  behavior rather than workbook content.

## Exclusions

The following categories are intentionally out of scope:

- Public APIs that expose NPOI workbook, sheet, row, or cell types.
- `.xls` binary workbook behavior.
- File-sharing behavior such as reading a workbook already opened by another
  process.
- Any behavior that evaluates formulas, follows external links, expands paths
  from cell content, or logs full user-provided input content.

## Implementation Checklist

| Item | Status | Next action |
| --- | --- | --- |
| Source-level verification of Spark.OpenXml support | User verification required | Local sandbox prevented source reads; verify against local source before release. |
| Column-level formatter parity | User verification required | Verify existing APIs and add focused tests if gaps exist. |
| CellReader parity | User verification required | Decide whether a safe Open XML cell-reader contract already exists or should be added. |
| RowFilter and CellFilter parity | User verification required | Verify existing filter APIs and add import tests if gaps exist. |
| Column width and auto column width | User verification required | Verify deterministic Open XML support and public API parity. |
| Workbook metadata configuration | User verification required | Verify public API support for workbook properties. |
| CSV stream/byte overload parity | User verification required | Verify public overloads and add round-trip tests if needed. |

## Verification Notes

Automated test execution was intentionally not run in this implementation pass.
The remaining verification steps are delegated to the maintainer because the
local sandbox intermittently prevented source reads and command execution.

Suggested maintainer checks:

- Confirm the parity inventory against current local source, tests, README, and
  docs.
- Add focused tests for any source-verified gap before changing library code.
- Run `dotnet test --project test/Spark.OpenXml.Test/Spark.OpenXml.Test.csproj`.
- Run `dotnet build.cs --target=test` before package-facing completion.

## Compatibility Notes

New public API surface must be additive unless a separate breaking-change review
approves otherwise. Compatibility is measured by observable workbook or CSV
behavior, not by matching WeihanLi.Npoi internal names or NPOI type exposure.

Before adding a public member for a parity gap, record whether the behavior can
be expressed through an existing Spark.OpenXml option, an additive option, or a
new opt-in workflow. Breaking changes are not part of this parity pass.

No public API changes were made in this pass because source-level verification
was not available. Items that require local source inspection remain marked as
user verification required instead of being implemented speculatively.

## Safety Notes

Parity work must not execute formulas, evaluate external links, expand file paths
from cell content, or log full spreadsheet/CSV input. Unsafe or file-system
specific behavior stays excluded unless a safe explicit API contract is designed.

Unsafe NPOI-specific capabilities remain excluded even if they are convenient in
the source project. The Spark.OpenXml contract is safe Open XML workbook and CSV
processing, not full NPOI behavior compatibility.
