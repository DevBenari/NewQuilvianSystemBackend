# Roadmap Delivery Backend — Rekam Medis Existing-First

## Metadata

```yaml
blueprint_id: QV-RM-001
blueprint_revision: 4
roadmap_revision: 1
status: DRAFT_FORWARD_TEST
approval_scope: DEVELOPMENT_UAT_ONLY
development_approval: APPROVED_VERIFIED
production_approval: NOT_APPROVED
approval_package_sha256: BB05D5697505ED6B809A0C8F16426C42F4B3F37A2937AEDD6414F0165C5751B3
approval_target_manifest_sha256: 6EF08E4A21435FB43811F848B8A92B7CA572EAF5C7473DFB0B6A9D12E8A6C8D3
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

Roadmap ini adalah rencana uji ke depan (*forward test*), bukan izin menjalankan builder. Paket
approval mengizinkan delivery planning dan implementasi Development/UAT, tetapi kontrak yang
terikat paket masih menyatakan `draft`. Karena itu seluruh task perubahan kode berstatus
`BLOCKED_CONTRACT_DRAFT` sampai pemilik menerbitkan revisi kontrak approved, versioned, dan
terkunci hash melalui paket approval baru.

Production release, deployment produksi, dan aktivasi fitur berisiko tetap dilarang.

## 1. Gate eksekusi bersama

| Gate | Keadaan | Dampak | Tindakan pemilik |
| --- | --- | --- | --- |
| Approval blueprint | `APPROVED_VERIFIED` hanya Development/UAT | Roadmap boleh disusun | Tidak ada untuk tahap ini |
| API, state, validation, integration, permission/audit | Versi `v0.2-draft`; isi menyatakan `Status: draft` | Seluruh task kode tertahan | Unit RM, owner klinis, dan privacy/legal menerbitkan revisi approved beserta hash baru |
| Acceptance matrix | `rm-existing-clinical-acceptance-v0.2-draft` | Bukti test belum menjadi acceptance terkunci | Sertakan acceptance matrix approved dalam paket revisi |
| Snapshot source | Masih cocok dengan manifest saat roadmap dibuat | Dapat dipakai sebagai baseline | Jalankan impact scan ulang bila SHA berubah |
| Production | `NOT_APPROVED` | Tidak boleh deploy/aktif di production | Memerlukan sign-off production terpisah |

Setiap handoff task backend wajib menjalankan QBE preflight dan membaca `AGENTS.md` serta dokumen
engineering canonical yang berlaku pada waktu eksekusi. Pada snapshot perencanaan ini file tersebut
tidak ditemukan; builder tidak boleh menganggap keadaan itu tetap sama.

## 2. Urutan slice existing-first

| Slice | Hasil yang dapat diperiksa | Task | Keadaan |
| --- | --- | --- | --- |
| S0 — Konteks pasien aman | Patient, encounter, assessment, dan consultation tidak dapat tertukar | `BE-RM-001` | Blocked |
| S1 — Finalisasi rawat jalan jujur | Finalisasi existing atomik dan tidak diklaim sebagai closure RM | `BE-RM-002` | Blocked |
| S2 — Catatan selesai tidak ditimpa | SOAP/assessment, CPPT, dokumen, dan consent mendapat guard finality | `BE-RM-003`–`BE-RM-005` | Blocked |
| S3 — Fakta keselamatan terlindungi | Diagnosis, tindakan, alergi, dan vital tidak dihapus sebagai fakta resmi | `BE-RM-006`, `BE-RM-007` | Blocked |
| S4 — Akses dan audit kontekstual | Permission teknis harus disertai penugasan aktif; mutation diaudit | `BE-RM-008`, `BE-RM-009` | Blocked |
| S5 — Bukti penerimaan | Regression existing-first dan bukti negatif tersedia | `BE-RM-010` | Blocked |

Urutan dependency:

```text
BE-RM-001 -> BE-RM-002 -> BE-RM-003
                    |-> BE-RM-004 -> BE-RM-005
BE-RM-001 -> BE-RM-006 -> BE-RM-007
BE-RM-001 -> BE-RM-008 -> BE-RM-009
seluruh task di atas -> BE-RM-010
```

## 3. Task backend

Semua task berikut menggunakan kontrak versi draft yang tercantum pada metadata task. Statusnya
tidak boleh diubah menjadi `READY` hanya karena kode existing tersedia.

### `BE-RM-001` — Menolak konteks pasien dan episode yang tidak cocok

| Field | Isi |
| --- | --- |
| Outcome | API clinical existing menolak fakta yang menghubungkan pasien, encounter, assessment, consultation, atau queue milik konteks lain sebelum payload klinis dibaca atau ditulis |
| Trace | `RM-SCP-001`, `RM-PRV-001`; `RM-CAP-01`, `RM-CAP-03`, `RM-CAP-08`, `RM-CAP-15` |
| Kontrak | API/Validation/Permission `v0.2-draft` — **blocker utama** |
| Reuse | `MstPatient`, `TrxPatientEncounter`, relasi assessment/consultation, pola validasi controller/service existing |
| Cakupan | Validator bersama atau adapter tipis pada sembilan provider clinical existing; tidak membuat master pasien atau episode RM baru |
| Dependency | Kontrak approved baru; impact scan source |
| Acceptance criteria | Konteks cocok diterima; patient–encounter berbeda ditolak `409/403`; data klinis pasien sebelumnya tidak ikut respons; owner timeout berbeda dari hasil kosong |
| Verifikasi | Unit test pasangan ID; integration test mismatch pada assessment, consultation, diagnosis, procedure, CPPT, document, dan consent |
| Risiko/pemilik | Salah penempatan guard dapat membocorkan existence data. Owner: Clinical Management + Security/Privacy |
| DoD | Seluruh negative test lulus, log tidak berisi MRN/payload klinis, build lulus, bukti source dan SHA dicatat |

**Contoh:** request memilih pasien A, tetapi `EncounterId` milik pasien B. Sistem mengembalikan
`409` dan tidak mengembalikan SOAP pasien B.

### `BE-RM-002` — Memperkuat validasi dan finalisasi konsultasi rawat jalan existing

| Field | Isi |
| --- | --- |
| Outcome | Dokter memperoleh daftar kekurangan SOAP/diagnosis utama; complete tetap atomik dan hasilnya hanya `Completed` milik owner, bukan `Ditutup Final` RM |
| Trace | `RM-CLS-005`, `RM-BIL-001`; `RM-CAP-08`, `RM-CAP-09`, `RM-CAP-17` |
| Kontrak | API/State/Validation `v0.2-draft` — **blocker utama** |
| Reuse | `ConsultationValidationService`, `ConsultationFinalizationService`, optimistic `ExpectedUpdatedAt`, transaksi resep/queue/encounter existing |
| Cakupan | `GET /{id}/finalization-validation`, `PATCH /{id}/complete`; tidak membuat checklist RM |
| Dependency | `BE-RM-001`; kontrak approved baru |
| Acceptance criteria | SOAP atau diagnosis utama yang kurang ditolak; stale timestamp menghasilkan `409`; kegagalan tengah transaksi rollback seluruh perubahan; status finansial tidak menjadi gate; replay identik tidak membuat finalisasi kedua |
| Verifikasi | Unit validator, integration transaction rollback, concurrency test, idempotency test setelah kontraknya dikunci |
| Risiko/pemilik | File service berada pada folder Pharmacy tetapi namespace Clinical Management; jangan memindahkannya dalam task ini. Owner: Clinical Management/Pharmacy |
| DoD | Tidak ada partial completion; response tidak menyatakan episode RM final; seluruh test lulus |

### `BE-RM-003` — Mengunci assessment dan SOAP yang sudah menjadi catatan resmi

| Field | Isi |
| --- | --- |
| Outcome | Assessment/SOAP yang memiliki bukti finality tidak bisa ditimpa atau dibatalkan melalui endpoint existing |
| Trace | `RM-SIG-001`, `RM-COR-001`, `RM-COR-002`; `RM-CAP-07`, `RM-CAP-08` |
| Kontrak | State/Validation/Permission `v0.2-draft` — **blocker utama** |
| Reuse | Status completion assessment/consultation dan endpoint existing |
| Cakupan | Guard `PUT`, SOAP update, cancel; belum membuat signature evidence atau correction endpoint baru |
| Dependency | `BE-RM-002`; definisi finality approved |
| Acceptance criteria | Record belum final tetap editable; record dengan bukti finality ditolak `409` dengan arahan koreksi/addendum; completion/cancel ulang tidak membuat state kedua; percobaan ditulis ke audit metadata |
| Verifikasi | Negative integration test pada kedua provider dan test replay |
| Risiko/pemilik | `Completed` existing tidak boleh otomatis dianggap signed. Owner harus mengunci sinyal finality sebelum implementasi |
| DoD | Tidak ada overwrite record final; perilaku record draft tidak rusak; test lulus |

### `BE-RM-004` — Melindungi CPPT tanpa memutus provenance konsultasi

| Field | Isi |
| --- | --- |
| Outcome | CPPT dari konsultasi mempertahankan provenance dan CPPT resmi tidak dapat diubah, dibatalkan, atau soft-delete |
| Trace | `RM-SIG-001`, `RM-COR-001`; `RM-CAP-12` |
| Kontrak | API/State/Validation `v0.2-draft` — **blocker utama** |
| Reuse | `TrxPatientIntegratedProgressNote`, `SourceReferenceId`, create/from-consultation existing |
| Cakupan | Create/from-consultation tetap; guard `PUT`, cancel, `DELETE`; tidak membuat menu CPPT baru |
| Dependency | `BE-RM-001`; definisi signature/finality approved |
| Acceptance criteria | CPPT baru valid tersimpan satu kali; provenance tidak berubah; signed/read-only generated ditolak pada mutation; versi lama tidak dihapus |
| Verifikasi | API integration create, provenance assertion, negative PUT/cancel/delete |
| Risiko/pemilik | Model belum mempunyai lifecycle signed universal. Owner: Clinical Management + Unit RM |
| DoD | Guard terbukti tanpa menebak bahwa semua CPPT existing sudah signed |

### `BE-RM-005` — Melindungi dokumen klinis dan consent yang telah disahkan

| Field | Isi |
| --- | --- |
| Outcome | Dokumen/consent yang sudah signed atau approved tidak dapat ditimpa atau dihapus melalui permission generic |
| Trace | `RM-SIG-001`, `RM-COR-001`, `RM-PRV-007`; `RM-CAP-13`, `RM-CAP-15` |
| Kontrak | API/State/Validation/Permission `v0.2-draft` — **blocker utama** |
| Reuse | File hash, status review/verify/approve/sign, controller existing |
| Cakupan | Guard update/cancel/delete dan pemisahan pemeriksaan authority; correction/entered-in-error tetap deferred |
| Dependency | `BE-RM-001`; authority matrix approved |
| Acceptance criteria | Hash dokumen yang diperhitungkan tersedia; signed/approved mutation ditolak; permission Update tidak otomatis menjadi clinical approval; audit tidak menyimpan path/file content |
| Verifikasi | Integration test status per status; security test audit/log |
| Risiko/pemilik | Pemisahan verifier dan approver belum ada di kontrak approved. Owner: Clinical Management + Privacy/Legal |
| DoD | Dokumen resmi immutable, state draft existing tetap berfungsi, bukti test tersimpan |

### `BE-RM-006` — Menjaga diagnosis dan tindakan sebagai fakta owner

| Field | Isi |
| --- | --- |
| Outcome | Diagnosis utama tepat satu dan tindakan baru memicu kewajiban hanya setelah event nyata; fakta final tidak ditimpa |
| Trace | `RM-CLS-005`, `RM-CLS-006`, `RM-CLS-007`; `RM-CAP-09`, `RM-CAP-15` |
| Kontrak | State/Validation/Integration `v0.2-draft` — **blocker utama** |
| Reuse | Set-primary, execute, cancel, remove-draft existing |
| Cakupan | Concurrency primary; guard finality; event nyata hanya dipersiapkan sebagai seam, bukan event RM baru |
| Dependency | `BE-RM-001`; trigger contract approved |
| Acceptance criteria | Dua set-primary bersaing tidak menghasilkan dua primary; planned procedure belum dianggap completed; cancellation setelah trigger tidak menghapus kewajiban otomatis; signed fact mutation ditolak |
| Verifikasi | Concurrency test dan domain-mapping test |
| Risiko/pemilik | Checklist/event RM belum tersedia, sehingga task tidak boleh membuat item checklist baru. Owner: Clinical Management |
| DoD | Fakta owner konsisten; tidak ada aggregate atau event deferred yang dibuat diam-diam |

### `BE-RM-007` — Menjaga alergi dan tanda vital sebagai data keselamatan

| Field | Isi |
| --- | --- |
| Outcome | Alergi aktif dan vital kritis tetap dapat ditelusuri dan tidak hilang melalui delete/cancel biasa |
| Trace | `RM-PRV-005`, `RM-SIG-001`; `RM-CAP-15` |
| Kontrak | State/Validation/Permission `v0.2-draft` — **blocker utama** |
| Reuse | Active alerts, critical alerts, verify, resolve, notify-doctor existing |
| Cakupan | Guard cancel/delete fakta resmi; audit metadata; acknowledgment tetap milik owner |
| Dependency | `BE-RM-001`; safety rule approved |
| Acceptance criteria | Alert berasal dari owner; delete fakta resmi ditolak; cancel high-risk memerlukan alasan/review; vital kritis mempertahankan jejak notify/acknowledgment owner |
| Verifikasi | Negative safety test dan regression endpoint alert |
| Risiko/pemilik | Guard yang terlalu luas dapat menghambat koreksi klinis sah. Owner: Clinical Management/Komite Medis |
| DoD | Alert tetap benar, fakta resmi tidak hilang, jalur draft/nonresmi tidak rusak |

### `BE-RM-008` — Menambahkan otorisasi berbasis penugasan aktif

| Field | Isi |
| --- | --- |
| Outcome | Permission resource/action saja tidak cukup; akses klinis juga memerlukan penugasan formal aktif dan konteks pasien yang sesuai |
| Trace | `RM-PRV-001`, `RM-PRV-002`, `RM-PRV-003`; `RM-CAP-05`, `RM-CAP-19` |
| Kontrak | Permission/Validation `v0.2-draft` — **blocker utama** |
| Reuse | ApplicationUser, workforce/doctor, encounter/queue assignment yang terbukti tersedia |
| Cakupan | Policy/authorization adapter pada provider existing; break-glass tidak dibuat |
| Dependency | `BE-RM-001`; sumber assignment dan masa berlaku harus dikunci kontrak approved |
| Acceptance criteria | Role + assignment aktif diterima; role tanpa assignment ditolak `403`; akses berakhir saat assignment dicabut/episode ditutup; mantan tim tidak mendapat akses normal |
| Verifikasi | Authorization test matrix sebelum, selama, dan setelah masa assignment |
| Risiko/pemilik | Source belum membuktikan seluruh jenis penugasan formal. Owner: Security/Privacy + Registration/Workforce |
| DoD | Semua provider dalam scope memakai policy yang sama; penolakan tidak membocorkan payload klinis |

### `BE-RM-009` — Mencatat audit mutation tanpa payload klinis

| Field | Isi |
| --- | --- |
| Outcome | Create, update, completion, cancel, verify, approve, dan percobaan terlarang meninggalkan audit metadata yang dapat ditelusuri |
| Trace | `RM-GOV-002`, `RM-RPT-001`; permission/audit matrix |
| Kontrak | Permission/Integration `v0.2-draft` — **blocker utama** |
| Reuse | Correlation/user context dan pola logging existing yang ditemukan saat preflight |
| Cakupan | Audit adapter untuk sembilan provider; tidak membuat laporan Unit RM atau retention deletion |
| Dependency | `BE-RM-008`; audit schema/sink approved |
| Acceptance criteria | Actor, role/profession, owner IDs, action, time, outcome, correlation dan alasan tersimpan; MRN, SOAP, diagnosis, vital, signer, file path, dan payload tidak tersimpan |
| Verifikasi | Security test dengan sink audit palsu dan scan log/output |
| Risiko/pemilik | Audit sink canonical belum dibuktikan pada snapshot. Owner: Security/Privacy + Platform |
| DoD | Mutation dan penolakan tercatat sekali, tidak ada data klinis sensitif di log |

### `BE-RM-010` — Menghasilkan paket bukti acceptance existing-first

| Field | Isi |
| --- | --- |
| Outcome | Product owner dapat memeriksa bahwa reuse existing aman dan tidak ada klaim episode/signature RM yang belum tersedia |
| Trace | Acceptance matrix seluruh baris existing-first |
| Kontrak | Acceptance `v0.2-draft` — **blocker utama** |
| Reuse | Test suite backend existing |
| Cakupan | Menjalankan dan mengarsipkan bukti task `BE-RM-001`–`009`; tidak mengubah migration/database di luar test lokal |
| Dependency | Seluruh task sebelumnya; acceptance matrix approved |
| Acceptance criteria | Build/test lulus; negative safety/security lulus; tidak ada model/configuration/migration/seed RM baru; source SHA hasil dicatat |
| Verifikasi | Laporan per skenario dengan command, exit code, dan artifact test |
| Risiko/pemilik | Lulus test tidak menjadi approval production. Owner: QA + Product/Domain |
| DoD | Bukti lengkap dan dapat diulang; hasil tetap berlabel Development/UAT |

## 4. Endpoint Swagger dalam scope

Endpoint berikut sudah ada pada snapshot source. Roadmap hanya merencanakan guard/adapter; tidak
mengklaim endpoint RM baru.

| Grup Swagger | Base URL | Method/path yang disentuh | Kegunaan | Hak akses existing | Status |
| --- | --- | --- | --- | --- | --- |
| Health Services / Clinical Management / Patient Assessment | `api/v1/health-services/clinical-management/patient-assessments` | `POST /`, `PUT /{id}`, `PATCH /{id}/complete`, `PATCH /{id}/cancel` | Assessment awal dan finality guard | `PatientAssessment : Create/Update` | Existing; extension blocked |
| Health Services / Clinical Management / Doctor Consultation | `api/v1/health-services/clinical-management/doctor-consultations` | `PUT /{id}`, `PATCH /{id}/soap`, `GET /{id}/finalization-validation`, `PATCH /{id}/complete`, `PATCH /{id}/cancel` | SOAP dan finalisasi rawat jalan | `DoctorConsultation : Read/Update` | Existing; extension blocked |
| Health Services / Clinical Management / Patient Diagnosis | `api/v1/health-services/clinical-management/patient-diagnoses` | `POST /`, `PUT /{id}`, `PATCH /{id}/set-primary`, `/resolve`, `/cancel` | Diagnosis dan diagnosis utama | `PatientDiagnosis : Create/Update` | Existing; extension blocked |
| Health Services / Clinical Management / Patient Procedure | `api/v1/health-services/clinical-management/patient-procedures` | `POST /`, `PUT /{id}`, `PATCH /{id}/execute`, `/cancel`, `/remove-draft` | Tindakan dan pemicu nyata | `PatientProcedure : Create/Update` | Existing; extension blocked |
| Health Services / Clinical Management / Patient Allergy | `api/v1/health-services/clinical-management/patient-allergies` | `GET /active-alerts`, mutation dan `DELETE /{id}` | Alert alergi dan perlindungan fakta | `PatientAllergy : Read/Create/Update/Delete` | Existing; extension blocked |
| Health Services / Clinical Management / Patient Vital Sign | `api/v1/health-services/clinical-management/patient-vital-signs` | `GET /critical-alerts`, mutation dan `DELETE /{id}` | Vital/alert kritis | `PatientVitalSign : Read/Create/Update/Delete` | Existing; extension blocked |
| Health Services / Clinical Management / Patient Integrated Progress Note | `api/v1/health-services/clinical-management/patient-integrated-progress-notes` | create/from-consultation, `PUT`, cancel, `DELETE` | CPPT dan provenance | `PatientIntegratedProgressNote : Read/Create/Update/Delete` | Existing; extension blocked |
| Health Services / Clinical Management / Patient Clinical Document | `api/v1/health-services/clinical-management/patient-clinical-documents` | create/update/review/verify/approve/cancel/delete | Dokumen klinis | `PatientClinicalDocument : Create/Update/Delete` | Existing; extension blocked |
| Health Services / Clinical Management / Patient Consent | `api/v1/health-services/clinical-management/patient-consents` | create/update/sign/verify/approve/withdraw/cancel/delete | Consent | `PatientConsent : Create/Update/Delete` | Existing; extension blocked |

Arti kode utama: `400` isian belum lengkap; `403` role atau penugasan tidak berwenang; `404`
reference owner tidak ditemukan; `409` konteks, state, concurrency, atau idempotency konflik; `503`
owner tidak dapat diverifikasi. Contoh: SOAP signed yang dikirim melalui `PUT` harus menerima `409`
dengan arahan membuat koreksi, bukan menimpa isi lama.

## 5. Cakupan yang sengaja tidak menjadi task

Episode RM, signature evidence universal, correction/addendum/`Entered in Error`, checklist versi,
deadline/escalation, worklist Unit RM, break-glass, release, hasil lab/radiologi, downtime/outbox,
retention deletion, serta workspace IGD/rawat inap terpadu tidak mempunyai kontrak approved yang
cukup pada revision ini. Semua tetap coverage gap dan fail-closed; tidak boleh disisipkan ke task
existing-first.
