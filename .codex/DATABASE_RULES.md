# Quilvian Database and EF Rules

These rules preserve the existing EF Core and PostgreSQL implementation. `AGENTS.md` remains authoritative; inspect the owning model, `ApplicationDbContext`, controller/service, and nearest migration before making persistence decisions.

## Canonical QBE alignment

Read `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` and the registry before persistence work. Apply QBE-ENT-001, QBE-CFG-001, QBE-NAM-001–003, QBE-DB-001–002, QBE-CODE-004 and QBE-MOD-002. A reference implementation does not supersede the canonical contract.

## Model and context discipline

- Keep entity/model ownership inside the established domain. Preserve table/schema, base-model, relationship, audit, soft-delete, and active-status conventions.
- Inspect `Repositories/ApplicationDbContext.cs` registration/configuration and the relevant model before changing persistence behavior. Do not invent a repository layer where the nearest implementation uses `ApplicationDbContext` directly.
- Keep configuration and dependency registration consistent with `Program.cs`; do not introduce a new persistence architecture or configuration path without explicit authorization.

## Query and relationship discipline

- Preserve existing query patterns, filters, ordering, tracking, projections, and pagination. Use `AsNoTracking` for read-only queries where the nearest pattern does; retain tracking when mutation or existing behavior requires it.
- Prefer projection/selects when the owning domain already supports them, rather than materializing unnecessary entities or navigation graphs.
- Load relationships deliberately using the nearest `Include`, projection, or query pattern. Avoid broad query rewrites, accidental N+1 behavior, or unrelated relationship changes.
- Preserve existing soft-delete, active-status, audit metadata, concurrency, transaction, retry, and idempotency behavior where relevant.

## Mutation, transactions, and safety

- Follow the owning controller/service's create/update/delete and `SaveChangesAsync` pattern. Use transactions only where the existing multi-step workflow/service establishes a transaction boundary.
- Never automatically drop databases/tables, truncate business data, mass-delete records, reset migrations, overwrite configuration, or update a production/shared database.
- Database commands are not routine source validation. Report database validation as pending when it was not explicitly authorized.

## Migration and execution authorization

An entity change does **not** automatically authorize any of the following:

- migration generation;
- `Update-Database`;
- database execution; or
- deployment.

Schema/entity impact, migration generation, database execution, and deployment are separate explicitly authorized actions. Do not create, delete, reset, rewrite, or execute migrations without the applicable authorization and a clearly bounded target.

## Representative evidence

- DbContext and DbSet/configuration ownership: `Repositories/ApplicationDbContext.cs`
- Composition and DbContext/service registration: `Program.cs`
- Master-data entity/persistence counterpart: `Areas/Administrator/MasterData/Models/MstBank.cs`; `Areas/Administrator/MasterData/Controllers/BankController.cs`
- Transaction/query/current-user evidence: `Areas/SelfServices/HumanResource/Services/OvertimeSelfServiceService.cs`; `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowService.cs`
- Migration convention example: `Migrations/20260521081743_initializeLeaveBalanceAndLeaveRequest.cs`

## Multi-developer consistency

New modules follow the nearest mature domain implementation. Do not introduce a new persistence model, migration convention, repository abstraction, query architecture, or error model merely because a module is new.
