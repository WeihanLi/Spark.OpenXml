<!--
Sync Impact Report
Version change: unknown/template -> 1.0.0
Modified principles:
- Template placeholders -> Public API Compatibility
- Template placeholders -> Spreadsheet Input Safety
- Template placeholders -> Tested Format Behavior
- Template placeholders -> Repository Conventions
- Template placeholders -> Documentation and Examples
Added sections:
- Development Workflow
- Governance
Removed sections:
- None
Templates requiring updates:
- ⚠ pending: .specify/templates/plan-template.md (tooling unavailable during sync)
- ⚠ pending: .specify/templates/spec-template.md (tooling unavailable during sync)
- ⚠ pending: .specify/templates/tasks-template.md (tooling unavailable during sync)
- ⚠ pending: .specify/templates/commands/*.md (tooling unavailable during sync)
Follow-up TODOs:
- Re-run template propagation once repository file reads are available.
-->

# Spark.OpenXml Constitution

## Core Principles

### I. Public API Compatibility

Spark.OpenXml is a NuGet library, so public API changes MUST be deliberate,
reviewable, and compatible with existing consumers unless a breaking change is
explicitly approved for a major version. Entity import/export, `DataTable`
import/export, attributes, Fluent API configuration, templates, multiple-sheet
workflows, formatters, filters, freeze panes, and CSV APIs MUST preserve existing
behavior unless the feature specification identifies the compatibility impact.

Rationale: Consumers depend on stable method signatures, configuration behavior,
and generated package validation results across upgrades.

### II. Spreadsheet Input Safety

Spreadsheet and CSV input MUST be treated as untrusted data. Implementations MUST
NOT execute formulas, evaluate external links, expand file paths from cell
content, or write files to caller-controlled paths unless an explicit public API
contract requires it. Tests, samples, and diagnostics MUST NOT expose credentials,
private datasets, or full user-provided input content.

Rationale: Import/export helpers often process user files, and safe defaults are
required to avoid data disclosure or unintended execution behavior.

### III. Tested Format Behavior

Behavior changes MUST include focused tests in `test/Spark.OpenXml.Test` unless
the change is documentation-only or build metadata-only. Excel behavior that is
format-sensitive MUST be validated with `.xlsx` import/export coverage. Shared
Excel paths MUST cover both entity and `DataTable` scenarios where applicable.
CSV behavior MUST validate delimiters, headers, quoting, null and empty values,
and round-trip behavior when those areas are affected.

Rationale: Spreadsheet formats have edge cases that are easy to regress through
apparently small parsing, formatting, or mapping changes.

### IV. Repository Conventions

Changes MUST follow `.editorconfig`, use the .NET SDK requested by `global.json`,
and preserve the repository's C# style: file-scoped namespaces, nullable-aware
code, configured file headers, and existing `var` usage patterns. Package
versions MUST be managed in `Directory.Packages.props`. Generated API reference
files under `docs/api` MUST NOT be hand-edited unless generated outputs are being
intentionally refreshed through the documentation pipeline.

Rationale: Consistent project conventions reduce churn and keep package,
documentation, and validation tooling predictable.

### V. Documentation and Examples

Public behavior, usage patterns, supported options, and compatibility-affecting
changes MUST be reflected in `README.md` or `docs/articles` when users need new
or updated guidance. Examples MUST remain concise, compatible with the package
target, and aligned with the current public API.

Rationale: The library is consumed through its package and documentation; public
behavior changes are incomplete unless users can discover and apply them safely.

## Development Workflow

Specifications MUST identify public API impact, data-safety considerations,
format-sensitive behavior, test coverage expectations, and documentation needs.
Implementation plans MUST prefer the repository build script for validation:
`dotnet build.cs`, `dotnet build.cs --target=build`, and
`dotnet build.cs --target=test`. Direct `dotnet restore`, `dotnet build`, and
`dotnet test` commands MAY be used for narrow local validation when appropriate.

Agents and contributors MUST check `git status --short` before editing, avoid
reverting unrelated user changes, keep changes focused, and use small test data
only when new files are necessary.

## Governance

This constitution governs feature specifications, implementation plans, task
generation, code changes, tests, and documentation updates for Spark.OpenXml.
When this constitution conflicts with informal guidance, this constitution takes
precedence.

Amendments MUST be made by updating this file and reviewing dependent Spec Kit
templates for consistency. Each amendment MUST include a Sync Impact Report that
summarizes version changes, principle changes, template sync status, and any
deferred follow-up.

Versioning follows semantic versioning:

- MAJOR: removes or redefines principles or governance in a backward-incompatible
  way.
- MINOR: adds a principle, adds a required section, or materially expands
  compliance expectations.
- PATCH: clarifies language, fixes wording, or makes non-semantic refinements.

Compliance review MUST occur during specification planning and before completing
implementation. Any intentional exception MUST be documented in the relevant
plan, including the reason and the compatibility or safety impact.

**Version**: 1.0.0 | **Ratified**: 2026-06-06 | **Last Amended**: 2026-06-06
