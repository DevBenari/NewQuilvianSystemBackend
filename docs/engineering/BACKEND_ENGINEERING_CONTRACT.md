# Quilvian Backend Engineering Contract

This is the canonical contract for NEW CODE, TOUCHED LEGACY and LEGACY MIGRATION. It governs developers, Codex, Skills and future checkers. The normative terms MUST, MUST NOT, SHOULD and MAY apply as defined below; it does not authorize implementation, migration, or deployment by itself.

## Authority, applicability and ratchet

NEW CODE MUST comply. TOUCHED LEGACY SHOULD repair applicable violations only when safe, scoped and authorized. UNTOUCHED LEGACY MUST NOT trigger a mass rewrite. LEGACY MIGRATION is an explicit bounded campaign. The approved [Module Ownership & Prefix Registry](MODULE_OWNERSHIP_PREFIX_REGISTRY.md) is the authority for operational naming.

## Canonical rules

| ID | Requirement | Feasibility |
|---|---|---|
| QBE-ENT-001 | MUST / NEW CODE: persisted domain entity inherits `IdentityModel`. | AUTOMATABLE |
| QBE-ENT-002 | SHOULD / NEW CODE: Guid, domain fields, navigation and nullability follow domain semantics. | PARTIALLY_AUTOMATABLE |
| QBE-ENT-003 | MUST NOT / NEW CODE: add presentation-only persisted fields. | REVIEW_ONLY |
| QBE-NAM-001 | MUST NOT / NEW CODE: use `Trx*` for operational entity/file/configuration/DbSet. | AUTOMATABLE |
| QBE-NAM-002 | MUST / NEW CODE: use approved registry prefix; unknown prefix blocks creation. | PARTIALLY_AUTOMATABLE |
| QBE-NAM-003 | MUST / LEGACY MIGRATION: normalize source and physical table together. | REVIEW_ONLY |
| QBE-CFG-001 | MUST / NEW CODE: provide `IEntityTypeConfiguration<T>` with mapping/keys/indexes/relations. | AUTOMATABLE |
| QBE-CFG-002 | SHOULD / TOUCHED LEGACY: repair configuration safely in scope. | PARTIALLY_AUTOMATABLE |
| QBE-MOD-001 | MUST / NEW CODE: place capability under owning Area/Module/Submodule. | PARTIALLY_AUTOMATABLE |
| QBE-MOD-002 | MUST / NEW CODE: operational module has approved registry entry before first entity. | PARTIALLY_AUTOMATABLE |
| QBE-SVC-001 | MUST / NEW CODE: Module Service owns domain CRUD/orchestration; controller does not direct-context it. | PARTIALLY_AUTOMATABLE |
| QBE-API-001 | MUST / NEW CODE: use established API boundary, response, status and validation. | PARTIALLY_AUTOMATABLE |
| QBE-PERM-001 | MUST / NEW CODE: use applicable Access metadata. | PARTIALLY_AUTOMATABLE |
| QBE-LOG-001 | MUST / NEW CODE: produce actor-aware state-change log/event. | REVIEW_ONLY |
| QBE-CODE-001 | MUST / NEW CODE: Service owns code need/format; allocation is deterministic and database-safe. | REVIEW_ONLY |
| QBE-CODE-002 | MUST NOT / NEW CODE: controller generates or allocates business number. | AUTOMATABLE |
| QBE-CODE-003 | MUST NOT / NEW CODE: use Count+1, unprotected Max/Last+1, static/local counter, or process-local lock as sole allocator. | AUTOMATABLE |
| QBE-CODE-004 | MUST / NEW CODE: unique business code has scope-appropriate DB unique constraint/index. | PARTIALLY_AUTOMATABLE |
| QBE-CODE-005 | MUST / NEW CODE: module owns format/prefix/reset/scope. | REVIEW_ONLY |
| QBE-CODE-006 | MUST / NEW CODE: shared provider supports durable scoped atomic allocation/retry observability. | REVIEW_ONLY |
| QBE-VAL-001 | MUST / NEW CODE: validate request and business invariants. | PARTIALLY_AUTOMATABLE |
| QBE-TXN-001 | SHOULD / NEW CODE: transact cross-record/workflow consistency. | REVIEW_ONLY |
| QBE-DTO-001 | MUST / NEW CODE: do not expose EF entity as API contract. | PARTIALLY_AUTOMATABLE |
| QBE-ENUM-001 | SHOULD / NEW CODE: keep needed enum module-owned. | REVIEW_ONLY |
| QBE-PAGE-001 | SHOULD / NEW CODE: list capability uses established paging/search/sort. | PARTIALLY_AUTOMATABLE |
| QBE-OPT-001 | SHOULD / NEW CODE: provide options/metadata only when consumed. | REVIEW_ONLY |
| QBE-DEL-001 | MUST / NEW CODE: respect delete/cancel lifecycle and actor audit. | REVIEW_ONLY |
| QBE-DB-001 | MUST / LEGACY MIGRATION: audit physical dependencies before rename. | REVIEW_ONLY |
| QBE-DB-002 | MUST NOT / LEGACY MIGRATION: use destructive DROP+CREATE when data-preserving rename is safe. | PARTIALLY_AUTOMATABLE |
| QBE-AUD-001 | MUST / ALL: preserve database audit separate from application logging. | PARTIALLY_AUTOMATABLE |

## Entity, naming and PostgreSQL

New generic persisted presentation `SortOrder` is prohibited; real business ordering uses a semantic field. DTO/form/permission/UI `SortOrder` remains valid. `Mst*` is master/reference and is not deprecated. For `LabOrder`: `LabOrder.cs`, `LabOrderConfiguration`, `LabOrders`, and `public."LabOrder"`. DbSet is pluralized entity name; table is singular PascalCase matching entity; schema is `public`.

## API/service and business number boundary

New flow is Controller → Module Service → DbContext/shared infrastructure/external integration. Code flow is Module Service → module formatter/definition → shared atomic PostgreSQL number-series provider → PostgreSQL. Provider supports SequenceKey, ScopeKey, atomic allocation and NEVER/YEARLY/MONTHLY/DAILY/domain scope. Unique + monotonic per scope is default; strict gaplessness requires separately approved legal/domain rule. DB unique constraints are mandatory when a code is unique.

## Legacy normalization and exceptions

Legacy `Trx*` normalization is incomplete until class, file, configuration, DbSet, references and `public."Trx..."` table are normalized together. Each batch audits FK/index/constraint/raw SQL/migration/consumer impact and verifies row count, integrity, build, API/frontend smoke and rollback. Exceptions MUST identify QBE ID, rationale, scope and must not silently establish convention.
