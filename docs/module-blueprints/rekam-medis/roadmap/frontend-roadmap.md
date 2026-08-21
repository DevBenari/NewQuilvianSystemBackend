# Roadmap Delivery Frontend — Rekam Medis Existing-First

## Metadata

```yaml
blueprint_id: QV-RM-001
blueprint_revision: 4
roadmap_revision: 1
status: DRAFT_FORWARD_TEST
approval_scope: DEVELOPMENT_UAT_ONLY
development_approval: APPROVED_VERIFIED
production_approval: NOT_APPROVED
frontend_source_commit: c4e2ef2a6080f3ce328d2faad79be1893ac13e22
backend_source_commit: 5103e68eec5529540d369673c8a4e2651be0344b
input_revisions:
  blueprint-manifest.md: 4
  00-interview-decisions.md: 1
  03-frontend-architecture.md: draft-signed-target
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

Seluruh task perubahan UI berstatus `BLOCKED_CONTRACT_DRAFT`. Paket approval mengikat blueprint
untuk Development/UAT, tetapi API, validation, state, permission/audit, integration, dan acceptance
matrix masih menyatakan `draft`. Frontend tidak boleh mendahului backend dengan menebak status,
permission, klasifikasi sensitif, atau arti tanda tangan.

Detail visual tetap `DEV_DISCRETION`: developer boleh memilih susunan komponen, jarak, dan ikon yang
sesuai pola project, tetapi tidak boleh mengubah makna, urutan kewenangan, pesan keselamatan, atau
field wajib.

## 1. Workspace existing yang dipertahankan

| Area | Lokasi | Keputusan |
| --- | --- | --- |
| Antrean dokter | `src/app/health-services/registration-management/doctor-queues/page.jsx` | Reuse route; tidak membuat menu Rekam Medis baru |
| Tampilan antrean | `src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx` | Extend fail-safe context |
| Tab konsultasi | `src/components/features/health-services/doctor-queue-features/ConsultationTabs.jsx` | Reuse hierarchy |
| SOAP | `.../doctor-queues/tabs/soap/doctor-soap-tab.jsx` | Tambah read-only/finality state setelah kontrak approved |
| CPPT | `.../doctor-queues/tabs/cppt/doctor-cppt-tab.jsx` | Sembunyikan mutation signed; bukan membuat workflow correction |
| Tindakan/resep | Tab existing | Reuse owner Clinical/Pharmacy |
| Finalisasi | `FinalizeConsultationPanel.jsx`, `FinalizeConsultationModal.jsx` | Repair validasi dan submit |

## 2. Slice dan dependency

| Slice | Hasil | Task | Keadaan |
| --- | --- | --- | --- |
| F0 — Konteks aman | UI tidak pernah menampilkan sisa data pasien sebelumnya saat konteks gagal | `FE-RM-001` | Blocked |
| F1 — Finalisasi jelas | Dokter melihat kekurangan dan konflik tanpa klaim closure RM | `FE-RM-002` | Blocked |
| F2 — Catatan resmi read-only | SOAP/CPPT/dokumen/consent signed tidak menawarkan edit/delete | `FE-RM-003`–`FE-RM-005` | Blocked |
| F3 — Fakta owner tetap satu sumber | Diagnosis, tindakan, alergi, vital, resep tetap dari owner | `FE-RM-006` | Blocked |
| F4 — Regression scope | Menu deferred tetap tidak muncul dan state UI diuji | `FE-RM-007` | Blocked |

```text
FE-RM-001 -> FE-RM-002 -> FE-RM-003
                    |-> FE-RM-004
                    |-> FE-RM-005
FE-RM-001 -> FE-RM-006
seluruh task -> FE-RM-007
```

## 3. Task frontend

### `FE-RM-001` — Membersihkan data klinis ketika konteks tidak dapat diverifikasi

| Field | Isi |
| --- | --- |
| Outcome | Perpindahan antrean/pasien tidak pernah meninggalkan SOAP, diagnosis, atau vital pasien sebelumnya ketika request baru gagal |
| Trace | `RM-PRV-001`; acceptance “Konsistensi konteks” dan “Owner unavailable” |
| Kontrak | API/Validation/Permission `v0.2-draft` — **blocker utama** |
| Reuse | Doctor queue view, fetch/state existing, patient/encounter/queue IDs |
| Cakupan | Loading, empty, timeout, mismatch, stale dan retry pada workspace existing |
| Dependency | `BE-RM-001`; kontrak approved baru |
| Acceptance criteria | State lama dibersihkan sebelum konteks baru dimuat; timeout tidak dirender sebagai empty; mismatch menutup seluruh fakta klinis; retry tidak menggandakan request |
| Verifikasi | Component/integration test pergantian pasien, timeout, dan mismatch |
| Risiko/pemilik | Flicker data lama adalah kebocoran privasi. Owner: Frontend + Security/Privacy |
| DoD | Tujuh keadaan layar teruji; tidak ada data pasien lama di DOM setelah context switch |

**Contoh:** petugas berpindah dari antrean pasien A ke pasien B, tetapi lookup encounter B timeout.
UI harus menampilkan “Sumber data belum dapat diverifikasi”, bukan SOAP pasien A atau rekam kosong.

### `FE-RM-002` — Menyatukan validasi dan submit finalisasi konsultasi

| Field | Isi |
| --- | --- |
| Outcome | Dokter melihat daftar kekurangan SOAP/diagnosis utama dan mengirim complete tepat satu kali |
| Trace | `RM-CLS-005`, `RM-BIL-001`; `RM-CAP-32`; frontend architecture finalization panel/modal |
| Kontrak | API/Validation/State `v0.2-draft` — **blocker utama** |
| Reuse | `FinalizeConsultationPanel.jsx`, `FinalizeConsultationModal.jsx`, endpoint validation/complete existing |
| Cakupan | Satu alur validation → confirmation → complete → refresh owner data |
| Dependency | `BE-RM-002`, `FE-RM-001` |
| Acceptance criteria | Kekurangan tampil per bagian; `409` stale meminta reload; double-click hanya satu mutation; sukses menampilkan `Completed`, bukan `Ditutup Final`; status billing tidak menahan tombol klinis |
| Verifikasi | Component test error/warning/stale/double-click dan integration test response mapping |
| Risiko/pemilik | UI existing memiliki mismatch checklist dan multi-request. Owner: Product/UI + Frontend |
| DoD | Tidak ada klaim signature/closure RM; lint dan test lulus |

### `FE-RM-003` — Menampilkan assessment dan SOAP completed sebagai read-only

| Field | Isi |
| --- | --- |
| Outcome | Pengguna tidak ditawari edit/cancel saat backend menyatakan catatan memiliki bukti finality |
| Trace | `RM-SIG-001`, `RM-COR-001`; `RM-CAP-07`, `RM-CAP-08` |
| Kontrak | State/API `v0.2-draft` — **blocker utama** |
| Reuse | Assessment preview dan `doctor-soap-tab.jsx` |
| Cakupan | Read-only banner, tombol mutation, pesan correction deferred |
| Dependency | `BE-RM-003`, `FE-RM-002` |
| Acceptance criteria | Draft tetap editable; final read-only; direct mutation `409` ditampilkan; tidak ada tombol correction palsu sebelum workflow tersedia |
| Verifikasi | Component test draft/final/unknown state dan API rejection |
| Risiko/pemilik | Jangan menyamakan semua `Completed` existing dengan signed. Owner: Unit RM/Product UI |
| DoD | State mengikuti response approved; tidak ada inferred signature |

### `FE-RM-004` — Melindungi CPPT signed pada tab existing

| Field | Isi |
| --- | --- |
| Outcome | CPPT tetap dapat dibaca beserta provenance, tetapi signed/read-only generated tidak dapat diedit, dibatalkan, atau dihapus |
| Trace | `RM-SIG-001`, `RM-COR-001`; `RM-CAP-12` |
| Kontrak | API/State `v0.2-draft` — **blocker utama** |
| Reuse | `doctor-cppt-tab.jsx`, timeline/create-from-consultation existing |
| Cakupan | State action per record dan indikator sumber |
| Dependency | `BE-RM-004`, `FE-RM-001` |
| Acceptance criteria | Provenance terlihat; signed dan generated read-only tidak mempunyai mutation action; draft dapat diedit sesuai permission; `409` tidak menghilangkan record |
| Verifikasi | Component test empat state dan regression create CPPT |
| Risiko/pemilik | Menyembunyikan tombol bukan security control; backend tetap wajib menolak. Owner: Frontend/Clinical Management |
| DoD | UI dan backend konsisten; lint/test lulus |

### `FE-RM-005` — Melindungi dokumen klinis dan consent signed

| Field | Isi |
| --- | --- |
| Outcome | Status, versi/hash, dan authority terlihat tanpa memberi kesan bahwa permission Update adalah approval RM |
| Trace | `RM-SIG-001`, `RM-PRV-007`; `RM-CAP-13` |
| Kontrak | API/State/Permission `v0.2-draft` — **blocker utama** |
| Reuse | Consumer/komponen dokumen dan consent existing |
| Cakupan | Read-only state, error owner, action visibility; tidak membuat menu release |
| Dependency | `BE-RM-005`, `FE-RM-001` |
| Acceptance criteria | Signed/approved tidak menawarkan update/delete; hash/version tersedia bila dikontrak; authority berbeda ditampilkan sesuai backend; data sensitif tidak dirender tanpa izin |
| Verifikasi | Component test status dan permission; privacy test hidden field |
| Risiko/pemilik | Dokumen dapat sangat sensitif. Owner: Privacy/Legal + Clinical Management |
| DoD | Tidak ada approval/release action buatan frontend; test lulus |

### `FE-RM-006` — Mempertahankan fakta klinis dan resep dari owner existing

| Field | Isi |
| --- | --- |
| Outcome | Diagnosis, tindakan, alergi, vital, dan resep tetap dirujuk dari provider masing-masing tanpa salinan editable RM |
| Trace | `RM-CLS-005`, `RM-INT-001`, `RM-BIL-001`; `RM-CAP-15`, `RM-CAP-17` |
| Kontrak | API/Integration/Validation `v0.2-draft` — **blocker utama** |
| Reuse | Tab diagnosis/procedure/prescription, assessment/vital preview, alert owner |
| Cakupan | Refresh/invalidation sesudah mutation, state alert, label provenance |
| Dependency | `BE-RM-006`, `BE-RM-007`, `FE-RM-001` |
| Acceptance criteria | Satu diagnosis utama terlihat; planned dan executed procedure berbeda; active allergy/critical vital tetap menonjol; resep tidak dapat diedit sebagai copy RM; kegagalan satu owner tidak mematikan semua tab |
| Verifikasi | Component/integration test per provider dan partial failure |
| Risiko/pemilik | Salinan frontend dapat menjadi sumber kebenaran tandingan. Owner: Clinical/Pharmacy |
| DoD | Semua mutation menuju owner canonical; tidak ada data copy persistent baru |

### `FE-RM-007` — Membuktikan scope existing-first dan state akses

| Field | Isi |
| --- | --- |
| Outcome | Route/tab dokter existing tetap bekerja; menu berisiko atau menu medis yang belum ada tidak muncul |
| Trace | `RM-UI-001`, `RM-APR-002`; acceptance “Existing-first UI” dan “Deferred menu” |
| Kontrak | Acceptance/Permission `v0.2-draft` — **blocker utama** |
| Reuse | Test suite frontend existing |
| Cakupan | Regression seluruh task frontend dan assertion absence menu |
| Dependency | `FE-RM-001`–`006`; acceptance matrix approved |
| Acceptance criteria | Tidak ada menu break-glass, release, worklist Unit RM, IGD terpadu, atau rawat inap terpadu; pengguna tanpa assignment tidak melihat data/aksi; loading/empty/error/read-only dapat diakses pembaca layar |
| Verifikasi | Route/component/regression/accessibility test |
| Risiko/pemilik | Ketiadaan menu tidak membuktikan endpoint aman; bukti backend tetap wajib. Owner: QA + Product UI |
| DoD | Lint/test lulus, hasil Development/UAT dicatat, tidak ada klaim production ready |

## 4. Kontrak API yang dikonsumsi

Frontend tetap memakai grup Swagger existing: Patient Assessment, Doctor Consultation, Patient
Diagnosis, Patient Procedure, Patient Allergy, Patient Vital Sign, Patient Integrated Progress
Note, Patient Clinical Document, dan Patient Consent pada base URL
`api/v1/health-services/clinical-management/...`.

Response `400` ditampilkan sebagai kekurangan isian; `403` sebagai tidak berwenang tanpa membuka
data; `409` sebagai konflik konteks/state yang memerlukan reload atau workflow resmi; `503` sebagai
sumber belum dapat diverifikasi. UI tidak boleh mengubah `403/503` menjadi daftar kosong.

## 5. Layar yang sengaja deferred

Tidak ada task menu break-glass, release informasi, worklist Unit RM, workspace IGD terpadu, atau
workspace rawat inap terpadu. Ketiadaan ini adalah batas scope yang diminta pengguna, bukan status
fitur selesai.
