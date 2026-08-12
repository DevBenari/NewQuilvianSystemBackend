# Quilvian Backend Development Rules

## Scope

This file applies to the entire `NewQuilvianSystemBackend` repository. This is a large existing application. Keep each task narrowly scoped and preserve established domain boundaries, API contracts, data access, authorization, and workflow behavior.

The primary rule is:

> Follow existing code. Do not invent a new architecture.

Before implementation, inspect the nearest comparable controller, DTO, model, service, data-access usage, validation, authorization rule, workflow, EF configuration, migration, and endpoint as applicable.

## Codex Governance Operating Layer

`AGENTS.md` remains the authoritative repository constitution. Read the following only when their condition applies:

- Every implementation task: `.codex/TASK_RULES.md`
- Classification and model selection: `.codex/TASK_CLASSIFICATION.md`
- Cross-repository task: `.codex/CROSS_REPO_RULES.md`
- Before completion: `.codex/REVIEW_RULES.md`
- Local handoff/report: `.codex/REPORT_TEMPLATE.md`
- API/controller/DTO/contract work: `.codex/API_RULES.md`
- Entity/EF/database/migration work: `.codex/DATABASE_RULES.md`

These documents supplement rather than replace the repository-specific safety, architecture, branch, security, validation, database, and write-scope rules in this file. Simple read-only questions do not require loading the full operating layer.

## Repository Identity and Branch Workflow

- Repository: `NewQuilvianSystemBackend`
- Primary development branch: `AgentCodexBackend`
- Frontend reference repository: `D:\Projects\QuilvianSystemFrontendDev`

Before backend implementation, verify the current backend branch with read-only Git commands. If it differs from `AgentCodexBackend`, stop and report the mismatch. Do not switch branches or repair Git state automatically.

Source implementation and Git publication are separate operations. Unless explicitly requested, do not commit, push, pull, merge, rebase, switch or checkout branches, reset, force checkout, stash, cherry-pick, create a pull request, or deploy.

## Current Backend Platform

- Application type: ASP.NET Core Web API
- Project SDK: `Microsoft.NET.Sdk.Web`
- Target framework: `.NET 9` (`net9.0`)
- SDK baseline from `global.json`: `9.0.316`, rolling to the latest patch
- Entity Framework Core and ASP.NET Core Identity: `9.0.18`
- PostgreSQL provider: `Npgsql.EntityFrameworkCore.PostgreSQL` `9.0.4`
- Main solution: `QuilvianSystemBackend.sln`
- Main project: `QuilvianSystemBackend.csproj`
- Application composition and dependency registration: `Program.cs`
- Application DbContext: `Repositories/ApplicationDbContext.cs`
- EF migrations: `Migrations/`

No separate test project was detected when these rules were created. Reinspect the workspace before assuming that tests remain unavailable.

Do not upgrade the SDK, target framework, packages, or infrastructure dependencies unless the task explicitly requires it.

## Existing Architecture Map

- Root API and infrastructure: `Controllers/`, `DTOs/`, `Models/`, `Services/`, `Repositories/`, `Responses/`, `Attributes/`, `Filters/`, `Middlewares/`, and `Shared/`
- Domain APIs: `Areas/`
  - `Areas/Administrator/`
  - `Areas/Corporate/`
  - `Areas/HealthServices/`
  - `Areas/SelfServices/`
- Human Resource domains include the actual source boundaries under:
  - `Areas/Corporate/HumanResource/WorkforceCore/`
  - `Areas/Corporate/HumanResource/WorkforceProfileManagement/`
  - `Areas/Corporate/HumanResource/WorkflowManagement/`
  - `Areas/Corporate/HumanResource/LeaveManagement/`
  - `Areas/Corporate/HumanResource/OvertimeManagement/`
  - `Areas/Corporate/HumanResource/SchedulingManagement/`
  - `Areas/SelfServices/HumanResource/`
- DTOs, models, controllers, and services are normally placed inside the owning domain when that pattern exists.
- Shared response contracts include `Responses/ApiResponse.cs` and `Responses/PagedResult.cs`.

Respect these boundaries. Do not move classes between domains, create a new root architecture, or introduce a generic layer merely because it appears cleaner. The repository currently uses `ApplicationDbContext` directly in many controllers and domain services; do not invent a repository abstraction where the closest implementation does not use one.

## Controller and API Conventions

Before creating or changing an endpoint, inspect the closest controller and preserve its established conventions, including as applicable:

- `[ApiController]` and `ControllerBase`;
- versioned routes beginning with `api/v1/...`;
- Area/domain route naming;
- HTTP verb and route template;
- request binding and DTO validation;
- `ApiResponse<T>` success and failure envelopes;
- `PagedResult<T>` pagination shape;
- status codes, error messages, filters, sort order, and pagination defaults;
- Swagger tags;
- cancellation and asynchronous EF patterns; and
- soft-delete and active-status conventions.

Do not infer a route from a frontend URL. Confirm the actual controller route and action first.

## API Contract Discipline

Frontend requirements do not automatically redefine backend contracts. Before modifying an API, inspect the existing controller route, action, HTTP verb, request DTO, response DTO, enum or status values, validation, authorization, pagination/filter behavior, and workflow or business rules.

Preserve backward compatibility where practical. Do not rename, remove, or break an existing endpoint, field, response envelope, enum/status value, or action unless the task explicitly requires the breaking change and its consumers have been assessed.

If frontend and backend disagree, report the mismatch. During backend work the frontend may explain current consumer behavior, but it does not override backend business or security rules automatically.

## DTO, Model, and Validation Rules

- Keep transport contracts in the owning `DTOs/` folder when that domain pattern exists.
- Do not expose EF entities merely to avoid defining an established response DTO.
- Preserve nullable behavior, date/time types, identifier types, default values, and validation attributes.
- Existing DTOs commonly use data annotations such as `[Required]`, `[MaxLength]`, and `[Range]`; inspect the closest contract before choosing validation behavior.
- Keep persistence models in the owning `Models/` folder and preserve existing table/schema, base-model, relationship, audit, soft-delete, and active-status patterns.
- Treat entity changes, API contract changes, migration generation, and database execution as separate decisions.

## Entity Framework and Data Access Rules

The application uses `Repositories/ApplicationDbContext.cs`, ASP.NET Core Identity integration, Entity Framework Core, and PostgreSQL through Npgsql.

Before changing persistence behavior:

1. Inspect the relevant model and `ApplicationDbContext` registration/configuration.
2. Inspect the nearest query and mutation patterns in controllers or services.
3. Preserve tracking versus `AsNoTracking`, relationship loading, transaction, concurrency, soft-delete, and audit conventions where applicable.
4. Avoid broad query rewrites or schema changes outside the requested domain.

Do not automatically create, delete, reset, or rewrite migrations. Migration generation requires explicit task authorization. Running a migration or `Update-Database` against any database requires a separate explicit instruction. A model change does not itself authorize either operation.

## Database Safety

Never execute destructive database operations automatically. Do not drop databases or tables, truncate business tables, mass-delete records, reset migrations, overwrite environment configuration, or update a production/shared database unless explicitly authorized with a clearly bounded target.

Do not run database commands merely to validate a source change. Report when database validation remains pending.

## Authorization and Current-User Rules

Preserve the existing security model. Inspect the actual use of:

- `[Authorize]`;
- controller metadata such as `[AccessController]`;
- action metadata such as `[AccessAction]` and `[AccessPermission]`;
- `AccessTypes`;
- authenticated current-user and claim resolution;
- role and permission checks;
- self-service ownership;
- actor and delegated-actor workflow authorization; and
- action availability returned by the backend.

Never solve a frontend visibility problem by weakening backend authorization. Do not accept arbitrary actor, workforce, or user identifiers where the existing self-service pattern derives ownership from the authenticated user.

## Workflow and Business Rules

The backend remains authoritative for workflow transitions, actor authorization, `AvailableActions`, approval and rejection, validation, status transitions, and domain business rules.

Before modifying workflow behavior, inspect the owning controller, DTOs, status constants/enums, lifecycle service, integration service, actor resolution, idempotency handling, and downstream effects. Do not move security-sensitive or business-critical decisions into frontend assumptions.

## Services and Background Processing

Use the closest domain service pattern. The repository contains domain services, query services, lifecycle/integration services, and hosted scheduler services. Preserve dependency-registration, transaction, retry, idempotency, logging, and cancellation patterns when relevant.

Do not start hosted services, schedulers, or the backend application unless the task explicitly requires runtime execution.

## Secrets and Configuration Safety

Never expose or place passwords, API keys, connection strings, tokens, private keys, SMTP credentials, user-secret values, or other sensitive configuration in reports, source comments, Codex responses, or Git commits.

When configuration inspection is necessary, report structure and key names only. Do not print secret values from `appsettings*`, user secrets, environment variables, deployment configuration, or data-protection material.

## Cross-Repository Task Modes

Only repositories explicitly authorized by the active task mode may be modified. If a mode or write target is missing or ambiguous, default to `AUDIT MODE`.

### AUDIT MODE

- Backend: read-only.
- Frontend: read-only.
- No source changes.

Use for architecture audits, gap mapping, API contract inspection, cross-repository analysis, and planning.

### FRONTEND MODE

- Frontend: write target.
- Backend: strict read-only source of truth.

The backend may be inspected for actual contracts and business behavior, but no backend file or Git state may be modified.

### BACKEND MODE

- Backend: write target.
- Frontend: read-only reference.

The frontend may be inspected to understand current callers, routes, Redux handling, UI flow, request usage, and response expectations. Do not modify tracked frontend source or configuration.

Backend implementation is allowed only when the prompt explicitly declares `TASK MODE: BACKEND` or otherwise identifies the backend as an explicit write target under `CROSS-REPO MODE`.

### CROSS-REPO MODE

Use only for an explicitly coordinated cross-repository task. Modify only the repository or repositories explicitly declared as write targets. Never assume both repositories are writable. Prefer sequential backend-first or frontend-first changes when practical.

## Cross-Repository Source of Truth and Safety Boundary

For frontend tasks, current backend source is authoritative for API contracts and backend business behavior. For backend tasks, frontend code is a consumer reference and does not automatically override backend rules.

If a backend task reveals a frontend defect, do not silently modify the frontend. Report it. If a frontend task reveals a backend defect, do not silently modify the backend. Stop that portion and report the issue unless the active mode explicitly authorizes the additional repository.

## Scope Control

Keep changes tightly scoped. Unless explicitly requested, do not reformat or rename unrelated files, reorganize folders, upgrade dependencies, rewrite architecture, modernize unrelated source, or fix unrelated warnings.

Report out-of-scope findings without modifying them.

## Git Safety

Codex must not automatically commit, push, pull, merge, rebase, switch branches, reset, force checkout, stash, or cherry-pick. Do not stage files, create a pull request, or deploy unless the task explicitly requests that separate operation.

Always inspect `git status --short` at the end of an implementation and distinguish changes made by the current task from pre-existing user changes. Never discard or overwrite unrelated work.

## Backend Validation

Validation must be proportional to the requested scope. It may include `dotnet build`, targeted tests, or solution/project tests when requested or reasonably required and not prohibited by the task.

Before running tests, inspect whether a relevant test project exists. Never report a build or test as passing unless the command actually completed successfully. Database migration execution, deployment, and runtime infrastructure remain separately authorized operations.

## Implementation Workflow

For implementation tasks:

1. Follow the conditional governance references above, beginning with `.codex/TASK_RULES.md`.
2. Keep the task bounded and apply all repository-specific rules in this file.
3. Report changed files, actual validation, migration state, risks, and `git status --short`.

Do not stage, commit, or push unless explicitly requested.

## Local ChatGPT Handoff Reporting

Shared local reporting lives in the frontend workspace at `D:\Projects\QuilvianSystemFrontendDev\.quilvian-local\`. Do not create duplicate tracked reporting infrastructure in this backend repository.

After meaningful `BACKEND MODE` or `CROSS-REPO MODE` work, Codex may update the ignored local files:

- `.quilvian-local/CHATGPT_HANDOFF.md`;
- `.quilvian-local/CURRENT_ISSUES.md`; and
- `.quilvian-local/reports/YYYY-MM-DD_HHMM_<short-task-name>.md`.

These reports are local workspace artifacts, not frontend source modifications. They must remain ignored and must not be committed or pushed.

Record evidence such as domain, controller, endpoint, HTTP method, request and response DTOs, enum/status, authorization, business/workflow behavior, changed files, actual build/test results, migration state, and Git status. Do not include secrets.

## Unclear Requirements

When an answer can be established from repository source, inspect it before asking. Ask for clarification only when a genuine contract, security, database, or business decision cannot be determined safely. Never invent a business rule.
