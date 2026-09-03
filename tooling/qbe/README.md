# QBE Conformance Checker

`Invoke-QbeConformanceCheck.ps1` is a delta-aware checker for new and changed C# files. It reads `AGENTS.md`, the [Backend Engineering Contract](../../docs/engineering/BACKEND_ENGINEERING_CONTRACT.md), and the Module Ownership & Prefix Registry at runtime; those documents remain normative.

Supported scopes are working tree (default), `-BaseRef` with `-HeadRef`, and `-Path`. Modes are `ReportOnly` (default) and `Strict`. Implemented detectors cover QBE-ENT-001, QBE-NAM-001, QBE-CFG-001, QBE-CODE-002, QBE-CODE-003, QBE-MOD-002 (partial), and QBE-SVC-001 (partial). Untouched legacy is excluded; changed legacy is evaluated only from added diff lines in both modes. With `-Path`, a tracked file is evaluated only from HEAD-to-WORKTREE added lines; an untracked/new file is evaluated in full as NEW CODE.

Examples:

```powershell
./tooling/qbe/Invoke-QbeConformanceCheck.ps1
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -BaseRef origin/main -HeadRef HEAD
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -BaseRef origin/main -HeadRef HEAD -Mode Strict
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path Areas/HealthServices/Foo.cs
./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -JsonOutputPath tooling/qbe/qbe-result.json
```

## Evaluation scope exclusions

Two classes of file are removed from evaluation before the detectors run.

**Generated build output.** Any path containing a `bin/` or `obj/` segment is dropped from every scope (`WorkingTree`, `GitRange`, `-Path`). Build output is never a source of record and is not evaluated.

**Test projects.** Files under a detected test project are excluded from the persisted-entity detectors QBE-ENT-001, QBE-CFG-001, and QBE-MOD-002. Those three rules are scoped by the [Backend Engineering Contract](../../docs/engineering/BACKEND_ENGINEERING_CONTRACT.md) to persisted domain entities and operational modules; a test class is neither. Without this exclusion a test that merely names `IdentityModel` — for example one asserting QBE-ENT-001 itself — was reported as a new persisted entity that fails all three rules.

Test projects are detected from `*.csproj` content, never from file or folder names, so a business file cannot escape review by being named like a test. A project is test scope when its `csproj` declares `<IsTestProject>true</IsTestProject>` or references `Microsoft.NET.Test.Sdk`, `xunit`, `NUnit`, or `MSTest`. Every `.cs` file under that project directory is test scope. A `csproj` sitting at the repository root is ignored for this purpose so that a misplaced project cannot exclude the whole repository. Detection runs once per invocation and is cached.

The exclusion is deliberately narrow. Test-scope files are still evaluated by QBE-NAM-001, QBE-CODE-002, QBE-CODE-003, and QBE-SVC-001, and still count toward `Files evaluated`. It is a scope correction, not a relaxation: no rule changes meaning, and the canonical contract is unchanged.

Both exclusions are reported rather than silent. The terminal report prints `Generated files excluded (bin/obj)` and `Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002`, and the JSON names every excluded test-scope file, so a reviewer can always see what was skipped.

## Exit behavior

The terminal report always states checker mode, scope, counts, and a final result.

- `ReportOnly` is backward-compatible: `VIOLATION`, `REVIEW`, `INFO`, or no findings all exit `0`.
- `Strict`: one or more `VIOLATION` findings exit `1` with `STRICT CONFORMANCE FAILURE` and the unique blocking QBE IDs. `REVIEW` and `INFO` remain advisory and exit `0` when no violation exists.
- Tooling/governance failures, including an unsupported mode or missing canonical authority, exit `2` and report `Final result: TOOL ERROR` when the script can handle the failure.

Strict mode enforces only the current delta-aware scope; it does not scan untouched legacy. CI invokes it with GitRange comparisons. This tool does not implement, migrate, or remediate legacy code.

## Structured output and exceptions

`-JsonOutputPath` is optional. It preserves terminal output and writes deterministic JSON containing schema/checker version, mode, scope and Git range, counts, suppressed-violation count, blocking RuleIds, result, and visible findings. Finding fields include repository-relative file, line, evidence/reason, recommended action, `suppressed`, and `exceptionId`. Scope exclusions are also recorded: `generatedFilesExcluded` counts dropped `bin/`/`obj/` sources, while `testScopeExcludedFileCount` and `testScopeExcludedFiles` report how many files were held out of the persisted-entity rules and exactly which ones.

The repository-owned authority is [QBE_EXCEPTIONS.json](../../docs/engineering/QBE_EXCEPTIONS.json). Each record requires `ExceptionId`, `RuleId`, a specific repository-relative file `Scope`, `Reason`, `Status` (`ACTIVE`, `EXPIRED`, or `REVOKED`), approval (`ApprovedBy` or `ApprovalReference`), and either `ExpiresAt` or `NoExpiryRationale`. Wildcards, whole-repository scope, absolute paths, traversal, empty RuleIds, and unknown QBE RuleIds are rejected. Malformed/invalid registries are tooling errors (`2`).

An ACTIVE matching exception leaves its finding visible as `SUPPRESSED` and prevents only that `VIOLATION` from blocking Strict. Expired or revoked records do not suppress. Exceptions do not change the QBE contract or establish convention. Developers and Codex must request explicit approval; they must not create or broaden an exception merely to make Strict pass.

## CI pull-request enforcement

The `QBE Conformance / QBE Strict GitRange` workflow runs for pull requests targeting `QuilvianIntegrationBackend`. It checks out full Git history, compares the exact pull-request base and head SHAs, and runs `Strict` GitRange mode. `VIOLATION` findings fail the job; `REVIEW` and `INFO` findings remain advisory. Untouched legacy is not scanned, and touched legacy is evaluated only on added lines.

The workflow writes `qbe-conformance-result` as a JSON artifact and publishes a concise job summary. Artifact and summary steps run even after a conformance failure. The canonical exception registry is read from `docs/engineering/QBE_EXCEPTIONS.json`; CI never creates or mutates exceptions.

Manual workflow dispatch requires explicit `base_ref` and `head_ref` inputs. This prevents an arbitrary fallback baseline or repository-wide legacy scan.
