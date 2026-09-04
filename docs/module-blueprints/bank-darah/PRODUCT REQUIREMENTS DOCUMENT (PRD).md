# PRODUCT REQUIREMENTS DOCUMENT (PRD)
# Modul Bank Darah — Quilvian V2

**Document ID:** PRD-BD-V2  
**Parent:** BRD-BD-V2  
**Module Key:** `bank-darah`  
**Status:** DRAFT FOR BLUEPRINT GENERATION

---

# 1. Product Objective

Membangun Bank Darah V2 sebagai operational workspace yang menghubungkan order pasien dengan blood unit inventory secara aman, auditable dan transactionally consistent.

Primary flow:

`Service Order -> Bank Darah Order -> Blood Unit Selection -> Allocation -> Issue -> Fulfillment`

Alternative flow:

`Allocation/Issue -> Return/Restock/Cancel`

Integration:

`Blood Bank Procedure -> Billing Contract`

Discovery:

`Laboratory Label Goldar <-> Bank Darah`

`HCLAB BANK DARAH / BBW / GL <-> Bank Darah`

---

# 2. Product Navigation

Berdasarkan evidence, navigation candidate:

## Bank Darah

### 1. Pesan Baru
Status: `[EVIDENCE]`, detail flow belum lengkap.

### 2. List Order
Status: `[EVIDENCE]`, P0.

### 3. Inventory Darah
Status: `[EVIDENCE]`, P0.

### 4. Laporan
Status: `[EVIDENCE]`, requirement detail `[UNRESOLVED]`.

### 5. Setup
Status: `[EVIDENCE]`, requirement detail `[UNRESOLVED]`.

Claude WAJIB memeriksa route/menu convention frontend V2 sebelum menentukan URL final.

---

# 3. Functional Requirements

## FR-BD-001 — Order Listing

### User Story

Sebagai Petugas BDRS, saya ingin melihat dan mencari order darah agar dapat menentukan order yang perlu diproses.

### Input Filter

- date from/to;
- MRN;
- legacy MRN jika tersedia;
- patient name;
- registration type;
- status bila lifecycle telah dibekukan.

### Output Minimum

- order number;
- ordered date/time;
- patient;
- MRN;
- registration type;
- ward/polyclinic;
- blood component;
- requested bag count;
- issued count;
- outstanding count;
- status.

### Acceptance Criteria

- server-side pagination;
- filter tidak mengunduh seluruh dataset;
- invalid page/filter ditangani aman;
- empty state jelas;
- unauthorized user tidak menerima data;
- search dibatasi panjangnya;
- stale request tidak merusak UI.

---

## FR-BD-002 — Order Detail

### User Story

Sebagai Petugas BDRS, saya ingin melihat seluruh konteks satu order tanpa kehilangan hubungan ke pelayanan asal.

### Sections

1. Patient information — read-only from owner module.
2. Encounter/registration.
3. Originating department/location.
4. Referring doctor.
5. Ordered components.
6. Fulfillment summary.
7. Allocated/issued units.
8. Bank Darah procedures.
9. History/audit.

### Acceptance Criteria

Order detail tidak menduplikasi authoritative patient master.

---

## FR-BD-003 — Blood Component Order Item

Satu order dapat mempunyai satu atau lebih item.

Conceptual fields:

- `orderItemId`
- `orderId`
- `componentReference`
- `requestedQuantity`
- `issuedQuantity` — derived/read model
- `outstandingQuantity` — derived/read model
- optional requested ABO/Rh only if business source is validated.

Quantity:

`requestedQuantity > 0`

Tidak boleh menggunakan free-text component jika authoritative catalog tersedia.

---

## FR-BD-004 — Inventory Search

### Filter Candidate

[EVIDENCE]

- blood component;
- ABO group;
- Rhesus.

[PROPOSED/REPO-DEPENDENT]

- unit number;
- availability status;
- location.

### Result

Minimal:

- blood unit identifier;
- component;
- ABO;
- Rh;
- current availability/status.

Additional safety fields hanya boleh ditambahkan setelah source/repository verification.

---

## FR-BD-005 — Reserve/Allocate Blood Unit

### Preconditions

- authenticated;
- authorized;
- order active;
- order item active;
- blood unit exists;
- blood unit currently available;
- quantity can still be fulfilled;
- no conflicting active allocation.

### Atomic Operation

Dalam satu business transaction:

1. validate order;
2. validate order item;
3. validate unit;
4. acquire concurrency protection;
5. create allocation;
6. update unit lifecycle;
7. create audit/event record;
8. commit.

### Response

Harus mengembalikan current canonical state.

### Conflicts

Gunakan `409 Conflict` untuk concurrency/state conflict sesuai convention API V2.

---

## FR-BD-006 — Issue/Give Blood Unit

### Preconditions

- valid active allocation;
- correct order;
- correct unit;
- unit belum issued;
- actor authorized.

### Result

- allocation becomes issued;
- blood unit becomes issued;
- issued/outstanding summary changes;
- audit created;
- order status recalculated.

### Idempotency

Request yang terulang tidak boleh menghasilkan dua issue.

---

## FR-BD-007 — Partial Fulfillment

Misal requested quantity = 3.

Setelah 1 blood unit issued:

- requested = 3;
- issued = 1;
- outstanding = 2;
- order = `PARTIALLY_FULFILLED` atau equivalent canonical state.

Counter harus derived.

---

## FR-BD-008 — Return / Restock

### Required Input

- allocation/unit reference;
- reason;
- concurrency token bila digunakan repository.

### Requirements

- validate allowable transition;
- preserve issue/allocation history;
- create inventory movement;
- update current state atomically.

Clinical reusability rule setelah blood unit meninggalkan Bank Darah adalah `[UNRESOLVED]`.

Jangan mengimplementasikan automatic restock eligibility sebelum decision tersebut tersedia.

---

## FR-BD-009 — Cancel / Void Order

### Required

- reason;
- actor;
- timestamp.

### Rules

Order dengan dependent transaction tidak di-hard-delete.

UI label boleh mengikuti terminology bisnis, tetapi backend semantic harus tidak menghapus clinical audit history.

---

## FR-BD-010 — Blood Bank Procedure

Conceptual data:

- order reference;
- department reference;
- BDRS doctor reference;
- BDRS staff reference;
- performed date/time;
- patient class reference;
- procedure/tariff reference;
- optional external billing transaction reference;
- state/audit.

### Billing

Billing side effect harus idempotent menggunakan stable business reference.

Failure handling harus dijelaskan dalam dependency contract.

---

## FR-BD-011 — Label Golongan Darah

Status awal:

`BLOCKED_BY_BUSINESS_CONTRACT`

Sebelum endpoint dibuat harus terjawab:

- authoritative ABO/Rh source;
- validator;
- print eligibility;
- duplicate print behavior;
- label fields;
- label identifier;
- audit requirement.

---

## FR-BD-012 — Sampling Golongan Darah

Status:

`DISCOVERY_REQUIRED`

Claude harus menelusuri laboratory/specimen model existing.

Jangan membuat specimen model baru apabila Laboratory sudah memilikinya.

---

## FR-BD-013 — HCLAB Workstation

Known evidence:

- Name: `BANK DARAH`
- Code: `BBW`
- Lab Sec: `GL`
- Context: `Workstation Results`

Product requirement saat ini hanya:

`investigate and document integration contract`

Bukan implementasi interface.

---

# 4. Conceptual Domain Model

Nama berikut adalah conceptual name, BUKAN izin langsung membuat tabel.

Claude harus terlebih dahulu mengecek reusable entity/schema existing.

## BloodBankOrder

Candidate responsibilities:

- lifecycle order Bank Darah;
- references to patient/encounter/location;
- order metadata.

Candidate fields:

- Id
- OrderNumber
- PatientId
- EncounterId / RegistrationId
- RequestingLocationId
- ReferringDoctorId
- OrderedAt
- Status
- Notes
- concurrency field
- audit fields

## BloodBankOrderItem

- Id
- BloodBankOrderId
- BloodComponentId/Code
- RequestedQuantity
- audit/concurrency fields as applicable

## BloodUnit

Candidate only if Bank Darah is proven owner.

- Id
- UnitNumber
- BloodComponentId/Code
- ABOGroup
- Rhesus
- Status
- ownership/location references if validated
- concurrency/audit

Do NOT silently invent expiry, donor or screening fields.

If safety review establishes them as mandatory, record an explicit decision first.

## BloodUnitAllocation

- Id
- BloodBankOrderItemId
- BloodUnitId
- Status
- ReservedAt
- ReservedBy
- IssuedAt
- IssuedBy
- ReturnedAt
- ReturnedBy
- return/cancel reason
- concurrency/audit

Unique active-allocation invariant is mandatory.

## BloodUnitMovement

Recommended for auditability if compatible with architecture:

- Id
- BloodUnitId
- MovementType
- FromState
- ToState
- BusinessReference
- ActorId
- OccurredAt
- Reason

## BloodBankProcedure

- Id
- BloodBankOrderId
- ProcedureReferenceId
- Tariff/Billing reference
- DepartmentId
- DoctorId
- StaffId
- ClassId
- PerformedAt
- Status
- audit

---

# 5. ERD Requirements

Create:

`erd/bank-darah-erd.md`

Include:

1. Mermaid conceptual ERD.
2. Table/entity mapping to actual V2 source.
3. External ownership explicitly marked.
4. Cardinality.
5. unique constraints.
6. indexes.
7. concurrency fields.
8. delete behavior.
9. lifecycle ownership.

External entity relationships must be shown as references, not copied tables.

Target conceptual relation:

```text
Patient
  |
Encounter
  |
BloodBankOrder
  |
  +-- BloodBankOrderItem
          |
          +-- BloodUnitAllocation -- BloodUnit
                                      |
                                      +-- BloodUnitMovement

BloodBankOrder
  |
  +-- BloodBankProcedure
          |
          +-- Billing/Tariff Contract
```

---

# 6. API Contract Requirements

Claude must derive final routes from current backend conventions.

Conceptual API only:

```text
GET    /.../blood-bank/orders
GET    /.../blood-bank/orders/{orderId}
POST   /.../blood-bank/orders
POST   /.../blood-bank/orders/{orderId}/cancel

GET    /.../blood-bank/inventory

POST   /.../blood-bank/orders/{orderId}/allocations
POST   /.../blood-bank/allocations/{allocationId}/issue
POST   /.../blood-bank/allocations/{allocationId}/return

GET    /.../blood-bank/orders/{orderId}/procedures
POST   /.../blood-bank/orders/{orderId}/procedures
```

Endpoints for Label Goldar/Sampling/HCLAB remain blocked until contract is resolved.

Every endpoint catalog entry requires:

- use case;
- route;
- HTTP method;
- permission;
- request DTO;
- response DTO;
- validation;
- status codes;
- idempotency;
- concurrency behavior;
- audit behavior;
- dependency;
- error codes;
- test IDs.

---

# 7. Error Contract

Minimum conceptual errors:

- `BLOOD_BANK_ORDER_NOT_FOUND`
- `BLOOD_BANK_ORDER_INVALID_STATE`
- `BLOOD_BANK_ORDER_QUANTITY_EXCEEDED`
- `BLOOD_UNIT_NOT_FOUND`
- `BLOOD_UNIT_NOT_AVAILABLE`
- `BLOOD_UNIT_ALREADY_ALLOCATED`
- `BLOOD_UNIT_INVALID_TRANSITION`
- `ALLOCATION_NOT_FOUND`
- `DUPLICATE_OPERATION`
- `CONCURRENCY_CONFLICT`
- `DEPENDENCY_UNAVAILABLE`
- `BILLING_OPERATION_CONFLICT`
- `LABEL_SOURCE_NOT_VALIDATED`
- `VALIDATION_ERROR`
- `FORBIDDEN`

Claude harus menyesuaikan error envelope dan naming convention terhadap repository existing.

Client-facing errors tidak boleh membawa:

- stack trace;
- SQL;
- filesystem path;
- secret;
- internal exception detail.

---

# 8. Frontend Requirements

## Technology

Ikuti actual frontend `QuilvianDevV2`.

Jangan membuat duplicate base component jika sudah ada reusable component.

Prioritaskan reuse terhadap pattern V2 seperti:

- `Hero`
- `DataFilter`
- `DataTable`
- `FilterSelect`
- `FilterDatePicker`
- `BaseButton`
- `StatusBadge`
- `ConfirmModal`
- `ToastStack`
- `AccessDeniedGate`
- editor/form abstraction existing bila sesuai.

## Screens

### FE-BD-SCR-001 — Order List

Contains:

- Hero;
- search/filter;
- period;
- registration type;
- paginated DataTable;
- fulfillment summary;
- status;
- action to detail/process.

### FE-BD-SCR-002 — Order Detail

Contains:

- patient/encounter summary;
- order items;
- requested/issued/outstanding counts;
- allocations;
- procedures;
- audit/history where permission permits.

### FE-BD-SCR-003 — Select Blood Unit

Contains:

- order requirement;
- inventory filter;
- available blood units;
- selection;
- confirmation.

Do not perform clinical compatibility decision in JavaScript.

### FE-BD-SCR-004 — Issue Confirmation

Use confirmation UI.

Show minimal safe context required to avoid human selection error.

Prevent double click/submission while mutation is running.

### FE-BD-SCR-005 — Return/Restock

Reason mandatory.

Allowed action determined by backend state.

### FE-BD-SCR-006 — Procedure

Use existing V2 form architecture.

Tariff reference comes from owner API.

### FE-BD-SCR-007 — Label/Sampling

Do not implement until corresponding contract becomes READY.

---

# 9. Frontend Security Requirements

- Never use `dangerouslySetInnerHTML` for API-controlled content.
- Render API strings as normal React text.
- Strictly encode path/query parameters.
- No authorization decision based solely on hidden button.
- Backend remains authorization authority.
- Sanitize/normalize search input.
- Bounded input lengths.
- Prevent duplicate submit.
- Abort/ignore stale request where appropriate.
- Do not store unnecessary patient information in localStorage/sessionStorage.
- No access token logging.
- Do not expose internal errors.
- Do not hardcode credentials.
- Safe file/print handling if introduced.
- Clear server `403` through existing Access Denied UX.

---

# 10. Backend Security Requirements

All controllers/endpoints:

- authenticated by default;
- exact `AccessController/AccessAction/AccessPermission` style if repository uses it;
- DTO allow-list;
- no entity binding directly from client;
- strict Guid/enum/date/string/quantity validation;
- maximum page-size enforcement;
- object-level authorization;
- parameterized ORM queries;
- transactional state changes;
- concurrency controls;
- idempotency strategy;
- PHI-safe logging;
- structured audit.

Special pentest cases:

- IDOR/BOLA;
- privilege escalation;
- mass assignment;
- invalid GUID/resource enumeration;
- duplicate request;
- race allocation;
- issue after cancellation;
- cancel after issue;
- forged status;
- oversized payload;
- search injection;
- XSS payload reflected from notes/name;
- SQL injection probes;
- verbose errors;
- unauthorized export/print.

---

# 11. Data Consistency Requirements

## Quantity

Never trust client-calculated:

- issued count;
- outstanding count;
- inventory count;
- total procedure charge.

Server returns canonical value.

## Concurrent Allocation

Test scenario:

User A and User B select the same available blood unit at nearly the same time.

Expected:

- exactly one succeeds;
- the other receives deterministic conflict;
- one active allocation exists;
- inventory remains correct.

## Duplicate Issue

Same issue command transmitted twice.

Expected:

- one business effect only;
- no duplicate movement;
- no duplicate billing.

---

# 12. Performance Requirements

Order list:

- server pagination;
- indexed date/status/foreign reference query as appropriate;
- no N+1 query;
- projection/read DTO rather than loading complete aggregate where unnecessary.

Inventory:

- server filtering;
- bounded page size;
- index candidate documented from observed query patterns.

Frontend:

- lazy load large workspace where appropriate;
- debounce search;
- memoize expensive mapping only when measurable/useful;
- avoid duplicated requests;
- no uncontrolled growth of client-side option lists.

---

# 13. Testing Requirements

## Domain Tests

- valid lifecycle transitions;
- invalid lifecycle transitions;
- requested/issued/outstanding invariant;
- unique active allocation;
- cancellation behavior;
- return behavior;
- procedure association.

## Repository/Database Tests

- unique blood unit identifier;
- active allocation constraint/concurrency;
- transaction rollback;
- no orphan records;
- delete restriction.

## API Tests

- 200/201 success;
- 400 validation;
- 401 unauthenticated;
- 403 unauthorized;
- 404 missing resource;
- 409 concurrency/state conflict;
- pagination;
- search;
- duplicate operation.

## Security Tests

- BOLA;
- mass assignment;
- forged status;
- XSS;
- injection;
- oversized input;
- permission matrix;
- error leakage.

## Frontend Tests

- loading;
- error;
- empty;
- access denied;
- pagination;
- filters;
- request cancellation/stale response;
- action disabled during mutation;
- conflict response;
- cancel/return reason validation.

## E2E P0

### E2E-BD-001

Order -> allocate one blood unit -> issue -> fulfilled.

### E2E-BD-002

Multi-unit order -> partial issue -> remaining outstanding.

### E2E-BD-003

Allocate -> cancel/return -> inventory consistency.

### E2E-BD-004

Two users allocate same unit concurrently -> only one succeeds.

### E2E-BD-005

Unauthorized actor attempts issue -> denied, no state change.

### E2E-BD-006

Billing dependency fails -> documented rollback/compensation behavior.

---

# 14. Traceability

Every P0 item must have this chain:

```text
Evidence
  -> BR-BD-xxx
  -> FR-BD-xxx
  -> Contract
  -> Entity/API
  -> BD-BE/FE Task
  -> TEST-BD-xxx
  -> Evidence of completion
```

No orphan task.

No orphan endpoint.

No implementation without requirement.

---

# 15. Product Readiness Gate

## READY_FOR_BLUEPRINT

When repository evidence and requirement evidence are indexed.

## READY_FOR_BACKEND

Only when:

- domain ownership resolved;
- ERD accepted;
- lifecycle accepted;
- dependency contracts accepted;
- endpoint/error/RBAC contracts frozen;
- P0 unresolved blockers removed.

## READY_FOR_FRONTEND

Only when relevant backend API contract is frozen.

## READY_FOR_RELEASE

Only when:

- implementation complete;
- tests complete;
- security checks complete;
- cross-module dependencies verified;
- unresolved blockers are either resolved or explicitly deferred.