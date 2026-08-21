# Dokter / Rawat Jalan Billing — Module Status

| Field | Value |
|---|---|
| Blueprint ID | `RJ-BIL-BP-001` |
| Module name | Dokter / Rawat Jalan Billing |
| Revision | `11` |
| Module status | `PARTIAL` |
| Current phase | `RJ-BIL-PH-008` — Delivery Planning |
| Last verified at | `2026-08-21T00:00:00+07:00` |
| Backend source SHA | `9b26be382ce1c7f3be8555bd2d98fc0aab3d39fc` |
| Frontend source SHA | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| IMPLEMENTATION_AUTHORITY | `NOT_GRANTED` |
| BUILDER_EXECUTION | `NOT_AUTHORIZED` |
| External adapter | `RJ-BIL-DEP-009 = INACTIVE / OUT_OF_SCOPE` |

## Phase state

| Completed phases | Active phases | Blocked phases |
|---|---|---|
| `RJ-BIL-PH-001` — interview/closure; `RJ-BIL-PH-002` — capability audit; `RJ-BIL-PH-003` — requirement gate; `RJ-BIL-PH-004` — core domain architecture; `RJ-BIL-PH-006` — target blueprint draft; `RJ-BIL-PH-007` — owner approval | `RJ-BIL-PH-008` — delivery planning | `RJ-BIL-PH-005` — external adapter activation |

## Delivery state

| Backend | Frontend | Integration | Verification |
|---|---|---|---|
| `APPROVED_FOR_EXECUTION` | `APPROVED_FOR_EXECUTION` | `NOT_STARTED` | `NOT_STARTED` |

## Blockers and owners

| Blocker ID | Summary | Owner | Affected phase | Independent continuation |
|---|---|---|---|---|
| `RJ-BIL-DEP-009` | Kontrak, keamanan, sandbox/UAT, idempotency, status-query, dan reconciliation external adapter belum tersedia | Payer/Insurance + Integration | External activation | Manual/internal payer workflow tetap berjalan |
| `RJ-BIL-CONFLICT-001` | Payment source encounter as-is one-to-one bertentangan dengan multi-payer target | Registration + Billing/Finance | Implementation adapter dan migration | Domain design mengikuti target owner-approved |
| `RJ-BIL-CONFLICT-006` | Pharmacy masih memiliki financial mutation legacy | Pharmacy + Billing/Payer | Compatibility and migration | Clinical fulfillment dan read-only projection dapat didesain |

## Evidence state

Capability map revision `2` sudah diperbarui melalui scoped impact scan working tree. Commit SHA
backend tetap `9b26be3...`, tetapi perubahan belum commit pada `Program.cs`,
`Repositories/ApplicationDbContext.cs`, dan `BillingManagement/Operational/**` wajib dianggap
sebagai evidence provisional sampai builder melakukan preflight, build, test, dan migration
review.

## Next recommended task

Roadmap delivery revision `1` dan seluruh task `RJ-BIL-BE-001..009` serta `RJ-BIL-FE-001..007`
telah disetujui. Builder tetap memerlukan handoff task, wewenang tulis, dan preflight eksekusi.
Jangan mengaktifkan external adapter `RJ-BIL-DEP-009`.
