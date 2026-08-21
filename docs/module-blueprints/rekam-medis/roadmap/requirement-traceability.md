# Requirement Traceability — Rekam Medis Existing-First

## Metadata

```yaml
blueprint_id: QV-RM-001
blueprint_revision: 4
roadmap_revision: 1
status: DRAFT_FORWARD_TEST
approval_scope: DEVELOPMENT_UAT_ONLY
approval_package_sha256: BB05D5697505ED6B809A0C8F16426C42F4B3F37A2937AEDD6414F0165C5751B3
backend_source_commit: 5103e68eec5529540d369673c8a4e2651be0344b
frontend_source_commit: c4e2ef2a6080f3ce328d2faad79be1893ac13e22
input_revisions:
  blueprint-manifest.md: 4
  00-interview-decisions.md: 1
  01-existing-capability-map.md: 1
contract_versions:
  api: rm-existing-clinical-api-evidence-v0.2-draft
  state: rm-existing-clinical-state-v0.2-draft
  validation: rm-existing-clinical-validation-v0.2-draft
  integration: rm-existing-clinical-integration-v0.2-draft
  permission_audit: rm-existing-clinical-permission-v0.2-draft
  acceptance: rm-existing-clinical-acceptance-v0.2-draft
artifact_hashes:
  api: C30C05960BE6B3165EAEDD1901166EE84140363C430574B67668CF0842829D88
  state: B78104870F555374E37AED3AE4A077C7E2A48F2BBACED027A61AA4033B2018C2
  validation: 70D91723D6422F474D135888FA43EABE5612EB38194E4ADC296AF991C74A81B9
  integration: C74337CA4223091782F20F0EE5B8968F0D7EF411249B0326378BF61C28D63B87
  permission_audit: 1D1510BD2A68FEB6A8C49C0EE633332A8FC81313785FC60C2C7AF428EA0C135E
  acceptance: 4E20EB8AFAB82D2F602C9F054F012C2B5F655E32EBAEB45B337750193D9AB06F
```

Status `Blocked` pada dokumen ini berarti task sudah direncanakan, tetapi belum boleh diserahkan ke
builder. Penyebab bersama seluruh task adalah kontrak dan acceptance matrix yang terikat paket
approval masih menyatakan `draft`.

## 1. Requirement yang dipetakan ke roadmap existing-first

| Requirement/decision | Design/capability | Kontrak saat ini | Backend | Frontend | Bukti target | Status |
| --- | --- | --- | --- | --- | --- | --- |
| Patient dan encounter tetap milik owner canonical (`RM-SCP-001`) | `RM-CAP-01`, `RM-CAP-03` | API/Validation draft | `BE-RM-001` | `FE-RM-001` | Context integration test | Blocked |
| Patient–encounter mismatch tidak membuka fakta klinis (`RM-PRV-001`) | Backend validation boundary | Validation/Permission draft | `BE-RM-001` | `FE-RM-001` | Negative safety/security | Blocked |
| Rawat jalan memakai assessment, SOAP, diagnosis utama (`RM-CLS-005`) | `RM-CAP-07`–`09` | API/Validation draft | `BE-RM-002`, `BE-RM-003`, `BE-RM-006` | `FE-RM-002`, `FE-RM-003`, `FE-RM-006` | Finalization integration | Blocked |
| Finalisasi existing atomik dan stale ditolak | `RM-CAP-08` | API/State draft | `BE-RM-002` | `FE-RM-002` | Rollback/concurrency tests | Blocked |
| Completion existing bukan signature/closure RM (`RM-SIG-001`) | State architecture | State/Validation draft | `BE-RM-002`, `BE-RM-003` | `FE-RM-002`, `FE-RM-003` | Response/UI semantic test | Blocked |
| Catatan final tidak ditimpa; correction terpisah (`RM-COR-001`, `RM-COR-002`) | `RM-CAP-07`, `08`, `12`, `13` | State/Validation draft | `BE-RM-003`–`005` | `FE-RM-003`–`005` | Negative PUT/cancel/delete | Blocked; correction gap |
| Diagnosis utama tepat satu | `RM-CAP-09`, `RM-CAP-15` | Validation draft | `BE-RM-006` | `FE-RM-006` | Concurrency set-primary | Blocked |
| Tindakan conditional hanya setelah event nyata (`RM-CLS-006`, `RM-CLS-007`) | Procedure owner | State/Integration draft | `BE-RM-006` | `FE-RM-006` | Planned-vs-executed mapping | Blocked; checklist gap |
| Alergi dan vital kritis tidak hilang | `RM-CAP-15` | State/Permission draft | `BE-RM-007` | `FE-RM-006` | Negative delete + alert regression | Blocked |
| CPPT mempertahankan provenance dan finality | `RM-CAP-12` | API/State draft | `BE-RM-004` | `FE-RM-004` | Create-from-consultation + immutable test | Blocked |
| Dokumen/consent signed immutable | `RM-CAP-13`, `RM-CAP-15` | API/State/Permission draft | `BE-RM-005` | `FE-RM-005` | Status mutation matrix | Blocked |
| Akses normal memerlukan role dan assignment aktif (`RM-PRV-001`–`003`) | `RM-CAP-05`, `RM-CAP-19` | Permission draft | `BE-RM-008` | `FE-RM-001`, `FE-RM-007` | Authorization time-window test | Blocked |
| Mutation diaudit tanpa payload klinis (`RM-GOV-002`, `RM-RPT-001`) | Permission/audit architecture | Permission/Integration draft | `BE-RM-009` | — | Audit sink + log scan | Blocked |
| Resep tetap milik Pharmacy (`RM-INT-001`) | `RM-CAP-17` | Integration draft | `BE-RM-002` | `FE-RM-006` | Owner/reference test | Blocked |
| Billing tidak menahan signature/closure RM (`RM-BIL-001`) | Integration boundary | Validation/Integration draft | `BE-RM-002` | `FE-RM-002` | Negative billing dependency | Blocked |
| Duplicate submit tidak menggandakan mutation (`RM-EXC-002`) | Validation/integration | Draft; key belum dikunci per endpoint | `BE-RM-002` dan task owner terkait | `FE-RM-002` | Replay/conflict test | Blocked; coverage parsial |
| Existing doctor workspace tetap dipakai (`RM-UI-001`) | Frontend architecture | API/Acceptance draft | — | `FE-RM-001`–`007` | Route/regression test | Blocked |
| Tidak membuat menu medis yang belum ada | Frontend architecture | Acceptance draft | — | `FE-RM-007` | Assertion absence menu | Blocked |

## 2. Coverage gap yang tidak boleh disembunyikan

| Gap | Requirement/decision | Capability | Mengapa belum menjadi task implementasi | Pemilik/langkah berikutnya |
| --- | --- | --- | --- | --- |
| `CG-RM-00` | Seluruh task existing-first | — | Lima kontrak dan acceptance matrix masih `draft` di dalam paket signed | Tiga authority menerbitkan revisi approved + paket target baru |
| `CG-RM-01` | Episode `Aktif`/`Belum Lengkap`/`Ditutup Final` (`RM-LFC-*`) | `RM-CAP-04` Missing | Aggregate/API episode RM belum dirancang pada contract revision ini | Unit RM + Clinical; design/approval revision berikutnya |
| `CG-RM-02` | Signature evidence, reauth, hash (`RM-SIG-001`) | `RM-CAP-14` Missing | Hanya guard finality existing yang direncanakan; evidence universal belum ada | Unit RM + Identity/Security + Clinical |
| `CG-RM-03` | Correction, addendum, `Entered in Error` (`RM-COR-*`, `RM-EVD-*`) | `RM-CAP-14` Missing | Workflow maker-checker dan endpoint belum dirancang | Unit RM + Clinical + Privacy |
| `CG-RM-04` | Checklist versi, conditional item, deadline/escalation (`RM-CLS-*`) | `RM-CAP-10`, `11` Missing/Adapter | Existing validator hard-coded bukan policy engine | Unit RM + Komite Medis |
| `CG-RM-05` | Break-glass dan kategori sensitif (`RM-PRV-004`–`017`) | `RM-CAP-20`, `21` Missing | User meminta menu deferred; contract aktivasi/review belum material | Privacy/Legal + Komite Medis; tetap fail-closed |
| `CG-RM-06` | Release informasi (`RM-RLS-*`) | `RM-CAP-22` Missing | Request/evidence/approval/delivery aggregate dan menu belum ada | Unit RM + Privacy/Legal; tetap disabled |
| `CG-RM-07` | Worklist/laporan/retensi/legal hold (`RM-RPT-*`) | `RM-CAP-23`–`25` Missing | Reporting, retention policy, dan legal hold belum dirancang untuk source | Unit RM + Privacy/Legal + Platform |
| `CG-RM-08` | Hasil lab/radiologi berversi (`RM-INT-001`, `RM-INT-002`) | `RM-CAP-16` Missing | Owner transaksi order/result belum terbukti pada source | Owner Lab/Radiologi + Integration |
| `CG-RM-09` | Durable event, downtime, retry, reconciliation (`RM-EXC-*`) | `RM-CAP-26`–`29` Missing | Event/outbox/downtime contract tetap deferred | Platform/Integration + Unit RM |
| `CG-RM-10` | Workspace IGD dan rawat inap terpadu | Frontend missing | User meminta membuat yang existing lebih dahulu | Product/UI setelah capability dan contract tersedia |
| `CG-RM-11` | Pembatalan/duplikat/reopen episode (`RM-LFC-*`) | Episode RM missing | Bergantung pada aggregate episode dan authority workflow | Unit RM + Clinical |

## 3. Acceptance matrix: yang dapat dan belum dapat dibuktikan

| Kelompok acceptance | Task | Keadaan |
| --- | --- | --- |
| Ownership patient/encounter dan konsistensi konteks | `BE-RM-001`, `FE-RM-001` | Planned, blocked |
| Assessment/SOAP/finalization/concurrency | `BE-RM-002`, `BE-RM-003`, `FE-RM-002`, `FE-RM-003` | Planned, blocked |
| Diagnosis/procedure/allergy/vital | `BE-RM-006`, `BE-RM-007`, `FE-RM-006` | Planned, blocked |
| CPPT/document/consent finality | `BE-RM-004`, `BE-RM-005`, `FE-RM-004`, `FE-RM-005` | Planned, blocked |
| Contextual authorization dan logging sensitif | `BE-RM-008`, `BE-RM-009`, `FE-RM-007` | Planned, blocked |
| Existing-first UI dan deferred menu | `FE-RM-007` | Planned, blocked |
| Signature evidence, checklist RM, durable event | Tidak ada task pada revision ini | Coverage gap |
| Break-glass dan release disabled | Static/scope assertion pada `FE-RM-007`; tidak ada fitur | Fail-closed, bukan fitur selesai |
| No migration/model/seed RM baru | `BE-RM-010` | Planned, blocked |

## 4. Syarat membuka task untuk builder

1. Terbitkan contract revision baru dengan status `approved`, version non-draft, dan hash per file.
2. Terbitkan approval target package baru yang mengikat revision tersebut; approval tetap dapat
   dibatasi Development/UAT.
3. Perbarui manifest dan roadmap input hash tanpa mengubah arti approval lama.
4. Jalankan impact scan jika SHA backend atau frontend berubah.
5. Pilih tepat satu task, berikan wewenang tulis repository target, dan gunakan builder yang sesuai.
6. Jangan menggabungkan coverage gap ke task existing-first tanpa desain serta approval baru.
