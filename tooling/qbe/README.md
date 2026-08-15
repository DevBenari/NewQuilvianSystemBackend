# QBE Conformance Checker

`Invoke-QbeConformanceCheck.ps1` is a delta-aware checker for new and changed C# files. It reads `AGENTS.md`, the [Backend Engineering Contract](../../docs/engineering/BACKEND_ENGINEERING_CONTRACT.md), and the Module Ownership & Prefix Registry at runtime; those documents remain normative.

Supported scopes are working tree (default), `-BaseRef` with `-HeadRef`, and `-Path`. Modes are `ReportOnly` (default) and `Strict`. Implemented detectors cover QBE-ENT-001, QBE-NAM-001, QBE-CFG-001, QBE-CODE-002, QBE-CODE-003, QBE-MOD-002 (partial), and QBE-SVC-001 (partial). Untouched legacy is excluded; changed legacy is evaluated only from added diff lines in both modes.

Examples:

```powershell
./tooling/qbe/Invoke-QbeConformanceCheck.ps1
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -BaseRef origin/main -HeadRef HEAD
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -BaseRef origin/main -HeadRef HEAD -Mode Strict
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path Areas/HealthServices/Foo.cs
```

## Exit behavior

The terminal report always states checker mode, scope, counts, and a final result.

- `ReportOnly` is backward-compatible: `VIOLATION`, `REVIEW`, `INFO`, or no findings all exit `0`.
- `Strict`: one or more `VIOLATION` findings exit `1` with `STRICT CONFORMANCE FAILURE` and the unique blocking QBE IDs. `REVIEW` and `INFO` remain advisory and exit `0` when no violation exists.
- Tooling/governance failures, including an unsupported mode or missing canonical authority, exit `2` and report `Final result: TOOL ERROR` when the script can handle the failure.

Strict mode enforces only the current delta-aware scope; it does not scan untouched legacy. It is suitable for future GitRange CI use, but G6-E2A does not add CI integration, exceptions, or machine-readable output. This tool does not implement, migrate, or remediate legacy code.
