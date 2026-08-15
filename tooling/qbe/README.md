# QBE Conformance Checker

`Invoke-QbeConformanceCheck.ps1` is a report-only, delta-aware checker for new and changed C# files. It reads `AGENTS.md`, the Backend Engineering Contract, and the Module Ownership & Prefix Registry at runtime; those documents remain normative.

Supported scopes are working tree (default), `-BaseRef` with `-HeadRef`, and `-Path`. Implemented detectors cover QBE-ENT-001, QBE-NAM-001, QBE-CFG-001, QBE-CODE-002, QBE-CODE-003, QBE-MOD-002 (partial), and QBE-SVC-001 (partial). Untouched legacy is excluded; changed legacy is evaluated only from added diff lines.

Examples:

```powershell
./tooling/qbe/Invoke-QbeConformanceCheck.ps1
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -BaseRef origin/main -HeadRef HEAD
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path Areas/HealthServices/Foo.cs
```

Findings are `VIOLATION`, `REVIEW`, or `INFO`; findings do not produce a non-zero exit. G6-E2 may add strict mode, CI integration, exceptions, and further detectors. This tool does not implement, migrate, or remediate legacy code.
