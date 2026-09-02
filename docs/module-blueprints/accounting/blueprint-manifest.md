# Accounting — Blueprint Manifest

```yaml
blueprint_id: ACC-BP-001
module_name: Accounting
module_slug: accounting
module_prefix: ACC
revision: 6
status: approved
current_phase: ACC-PH-003
created_at: 2026-09-01T09:53:36+07:00
updated_at: 2026-09-02T12:00:00+07:00
last_verified_at: 2026-09-02T12:00:00+07:00
approved_by: Rizki (Product/Domain Owner + Implementation Owner Accounting)
approved_at: 2026-09-01T18:00:00+07:00
owners:
  product_domain: Rizki
  api: Backend/API Owner
  security: Security Owner
  frontend_authority: Product Owner (ACC-FE-001, ACC-FE-003 masih terbuka)
# Dua baseline sengaja dipisah. Yang `approved_*` adalah baseline yang menjadi dasar approval
# owner dan TIDAK boleh diganti diam-diam. Yang `verification_*` adalah baseline tempat
# verifikasi terakhir dijalankan. Lihat bagian "Dua baseline source" di bawah.
#
# `backend_source_sha`/`frontend_source_sha` dipertahankan karena handoff-contract skill
# mewajibkan keduanya. Nilainya SELALU sama dengan `approved_*`, tidak pernah dengan
# `verification_*` — supaya konsumen lama tidak diam-diam membaca baseline yang berbeda.
backend_source_sha: aa837d784ff51cb2b889cf975ada3a204018f1f5
frontend_source_sha: 31a82c8052a3c59445ae49e6f1ccce2bf717d6c0
approved_backend_source_sha: aa837d784ff51cb2b889cf975ada3a204018f1f5
approved_frontend_source_sha: 31a82c8052a3c59445ae49e6f1ccce2bf717d6c0
verification_backend_source_sha: 2b152aaf2a28550c14b13549b315d2b743e4c039
verification_frontend_source_sha: 5336c4457c8ad77abe5c9d2c134760f34a334f55
verification_baseline_note: >
  Backend bergerak aa837d7 -> ca6b7e0 -> e1ee173 -> a4df550 sepanjang 2 September 2026, dan tiap
  pergeseran diverifikasi impact scan lebih dahulu. ca6b7e0 menambah 28 berkas dokumentasi
  blueprint saja. e1ee173 adalah commit BE-ACC-001..003 oleh owner. a4df550 adalah commit
  BE-ACC-004 oleh owner, tepat 8 berkas. 2b152aa adalah commit BE-ACC-005 oleh owner, 16 berkas.
  Keempatnya nol sentuhan pada Migrations/, ModelSnapshot, Program.cs, tooling/, agents/,
  .github/, dan modul lain, sehingga seluruh bukti terhadap aa837d7 tetap berlaku. Frontend:
  31a82c8 adalah leluhur 5336c44 (fast-forward murni), tidak relevan untuk task backend.
canonical_integration_baseline: f90bcbe9a0b18d4f4425a4678a5a39a44356677b
integration_baseline_note: >
  PERHATIAN. rizkiG@2b152aa tertinggal 5 migration dan 8 tabel dari canonical integration
  baseline f90bcbe. Snapshot lokal 530 tabel, snapshot integration 538. Ini TIDAK memengaruhi
  kode Accounting yang sudah ada, tetapi MENAHAN BE-ACC-006: migration yang dibuat dari snapshot
  basi akan menghapus 8 tabel modul lain, pola kerusakan yang sama dengan ACC-DEP-001. Dilacak
  sebagai ACC-DEP-009. Bukti: evidence/04-migration-coordination-gate.md.
verified_at: 2026-09-02
skill_suite_version: 1.0.0-rc2
input_revision_hash: ACC-PRD-001@0.1 + 00-interview-decisions@3
decision_revision: 1.3
input_revisions:
  interview_decisions: 4
  capability_map: 2
  requirement_gate: null
  hospital_domain_architecture: null
contract_versions:
  api: ACC-API-0.1
  state: ACC-STATE-0.1
  validation: ACC-VALIDATION-0.2
  integration: ACC-INTEGRATION-0.2
  permission: ACC-PERMISSION-0.1
  testing: ACC-TEST-0.1
  mvp: ACC-MVP-0.1
  cross_module: ACC-XMOD-0.1   # consumer-side Accounting disetujui; alignment Finance/Billing belum
shared_engineering_rules:
  proposed: [QBE-MIG-001, QBE-MIG-002]
  canonical_home: docs/engineering/BACKEND_ENGINEERING_CONTRACT.md@origin/QuilvianIntegrationBackend
  status: PROPOSED
cross_module:
  provides: ACC-XMOD-0.1
  consumer_module: finance
  consumer_owner: Yasmin
  consumer_blueprint_path: docs/module-blueprints/finance/
  depends_on_finance_contract: null
  open_cross_module_decisions: [ACC-XM-001]
artifact_hashes:
  00-interview-decisions.md: 3dbdd53cbaa8acc8c427c946cc8b6d19251887e6e29e2493fc20d78c07bd7895
  01-existing-capability-map.md: df5c5375f04ba9f688a49ac6504f53d05995545507b75a05c19dcf707e5e59ea
  02-backend-architecture.md: 4a77b937cf2953ace1a7060f704f729674e26eb4545fc7f0fced1e7bcfa057a9
  03-frontend-architecture.md: a68b56a043aaf5bfc99356d5477ff059c21cac35c330dfa8656f1a90e995c07f
  04-prd-to-mvp.md: 1da14a42f09030625641f9769ebd1839773125c4e8b94e36016e363e311ca081
  06-shared-migration-coordination-rule.md: e1111572749627931b81da86c779c472197ab821790a6e5568900068b608d428
  contracts/api-contract.md: 0937282e651348eed9155564b1a5b13557e38a94a50eb049ce7745e0306243ae
  contracts/state-transition-matrix.md: 34ef47ca2fb0b8dce9c8e5336b267e16f9878635d75ab7bd033affe0fca687b5
  contracts/validation-matrix.md: 1efedc7e1ea53274544f6f7a1d5b92af35e756af1f9dad20f828ff6361e6b09b
  contracts/integration-contract.md: 1c773b03b30a272459de9db436bded581d0593e1897a89e847fdbc023679e094
  contracts/permission-audit-matrix.md: 91a40a7fa0535024e6c300eb567cb54c33a0e9886bc188a7ed2e0041a7cfa195
  contracts/cross-module-contract.md: a17b2449c9d21471af8473e97e254b5f6f3e8dfda73793d22abf79b71cceef9f
  testing/acceptance-test-matrix.md: 88658490456f7d74a2ce0834b7b6bf94389e2a7273e67b953e79a7dd8bf27364
  erd/data-dictionary.md: 2315d2f525ae5870cc7c0a8a2af2b3051b16c71b6e89e3d25ad22145d00ad1f1
  roadmap/backend-roadmap.md: 4f4dd68b2c231438274bc019340dccc53ada2b50b1dedc8ca83ef585fd5f67f2
  roadmap/frontend-roadmap.md: 1cb8b8d30eb8bfdf46927a6e0a448e7dfc80281cf538aa8bb354ab49d8f3096f
  roadmap/requirement-traceability.md: b2826cfc29531ea69cab31a922faaad1aaf691cb4efe23e531aa99520211690f
active_dependency_ids: [ACC-DEP-003, ACC-DEP-004, ACC-DEP-005, ACC-DEP-007, ACC-DEP-008, ACC-DEP-009]   # 001, 002, 006 CLOSED
entity_prefix:
  prefix: Acc
  status: REGISTERED
  lifecycle: ACTIVE
  registered_at: 2026-09-01
  activated_at: 2026-09-01
  activation_decision: ACC-DEC-038
  registry_path: QuilvianEngineeringSkills/agents/rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md
  entity_creation_authorized: true    # source model persisted saja
  migration_authorized: false         # gerbang terpisah: ACC-DEP-005 + Migration Coordination Gate
  database_execution_authorized: false
  deployment_authorized: false
active_roadmap_revision: 2
roadmap_status: DRAFT_FORWARD_TEST
supersedes: null
```

## Keterangan singkat

`ACC` di berkas ini adalah **prefix blueprint**, dipakai untuk penomoran dokumen seperti
`ACC-DEC-001` atau `ACC-PH-001`. Prefix ini **tidak sama** dengan prefix penamaan entity di kode
backend. Prefix entity wajib didaftarkan lebih dahulu oleh lead; lihat `ACC-DEP-002` pada
[05-prerequisite-readiness.md](05-prerequisite-readiness.md).

## Sumber masukan

| Masukan | Nilai | Keterangan |
| --- | --- | --- |
| Dokumen requirement | `ACC-PRD-001` revisi `0.1` | Status `OWNER_APPROVED_FOR_GRILL_ME`, disetujui 1 September 2026 |
| Decision log | 40 dari 40 tertutup | 28 dijawab owner, 9 ditunda ke Phase 2 lewat `ACC-DEC-036`, ditambah `ACC-DEC-038` (lifecycle) serta `ACC-DEC-039`/`040` (penyelesai pertentangan artefak) |
| Capability map | Parsial | Pemeriksaan terarah, bukan audit penuh. `/trace-existing-capabilities` belum dijalankan |
| Requirement completeness gate | **Belum dijalankan** | Tidak diwajibkan karena MVP diklasifikasikan sebagai kemampuan non-rumah-sakit |
| Hospital domain architecture | **Belum dijalankan** | Alasan yang sama. **Wajib** dijalankan sebelum Phase 2 |

Dasar klasifikasi non-rumah-sakit ada di [02-backend-architecture.md](02-backend-architecture.md)
bagian 1, beserta syarat yang mengikatnya.

## Klasifikasi artefak

Klasifikasi ini menentukan **apa yang wajib dibaca modul lain**, khususnya Finance/Yasmin yang
mengembangkan AR/AP secara paralel. Agent Finance **tidak perlu** membaca seluruh folder ini.

### `CROSS_MODULE_REQUIRED` — wajib dibaca Finance

| Artefak | Yang Finance perlu tahu darinya |
|---|---|
| [blueprint-manifest.md](blueprint-manifest.md) | Revision dan status Accounting yang menjadi dependency; batas ownership |
| [contracts/cross-module-contract.md](contracts/cross-module-contract.md) | Kontrak Finance/AR/AP → Accounting: envelope, `CurrencyCode`, idempotency, source traceability, semantik penolakan |
| [contracts/integration-contract.md](contracts/integration-contract.md) | Batas kepemilikan yang mengikat, dan gerbang wajib sebelum Phase 2 |
| [06-shared-migration-coordination-rule.md](06-shared-migration-coordination-rule.md) | Aturan koordinasi migration bersama — mengikat kedua modul |
| [05-prerequisite-readiness.md](05-prerequisite-readiness.md) | Dependency lintas modul yang masih terbuka |

Lima berkas. Itu saja.

### `SHARED_ENGINEERING_RULE` — bukan milik Accounting

| Artefak | Keterangan |
|---|---|
| [06-shared-migration-coordination-rule.md](06-shared-migration-coordination-rule.md) | Berstatus `PROPOSED`. Rumah canonical-nya `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, dan begitu lead mengesahkannya, berkas ini hanya menjadi penunjuk |

### `INTERNAL_ONLY` — rancangan dalam Accounting

`00-business-overview.md`, `00-interview-decisions.md`, `01-existing-capability-map.md`,
`02-backend-architecture.md`, `03-frontend-architecture.md`, `04-prd-to-mvp.md`,
`MODULE-STATUS.md`, `README.md`, seluruh `erd/`, seluruh `evidence/`, seluruh `roadmap/`,
seluruh `testing/`, serta `contracts/api-contract.md`,
`contracts/state-transition-matrix.md`, `contracts/validation-matrix.md`, dan
`contracts/permission-audit-matrix.md`.

Berkas-berkas ini boleh dibaca siapa saja, tetapi **tidak** menjadi kewajiban modul lain dan
**tidak** boleh dijadikan dasar kontrak lintas modul. Ia berubah tanpa memberi tahu Finance.

### Arah sebaliknya

Accounting **tidak** membaca internal Finance. Sebelum implementasi Phase 2 Finance →
Accounting, Accounting hanya wajib membaca kontrak cross-module Finance revisi `APPROVED`
terakhir. Mekanisme referensi revisinya di
[contracts/cross-module-contract.md](contracts/cross-module-contract.md) bagian 8.

### Kedua modul adalah bounded context terpisah

```
docs/module-blueprints/
├── accounting/   Owner: Rizki    ── ACC-XMOD ──▶ dibaca Finance
└── finance/      Owner: Yasmin   ── FIN-XMOD ──▶ dibaca Accounting (Phase 2)
```

Folder keduanya **tidak digabung**. Finance bukan anak Accounting, dan Accounting bukan anak
Finance. Hubungannya hanya lewat kontrak cross-module yang eksplisit.

## Aturan revision

Revisi 3 dinaikkan pada 1 September 2026 karena arsitektur target, ERD, dan enam kontrak disusun
sekaligus — perubahan material.

Naikkan `revision` hanya bila arsitektur target, contract, dependency, atau keputusan yang sudah
disetujui berubah secara material. Perubahan status saja, penambahan bukti yang tidak mengubah
target, atau menandai task selesai **tidak** menaikkan `revision`. Setiap pembaruan tetap
mengubah `updated_at`; verifikasi mengubah `last_verified_at`.

## Riwayat verifikasi

### 2 September 2026 — `ACC-DEC-039` dan `ACC-DEC-040`, revisi tetap 6

Dua keputusan owner yang **menyelesaikan pertentangan di dalam artefak canonical**, bukan
mengubah target. Ditemukan dan dilaporkan saat `BE-ACC-005`, lalu diputuskan owner pada hari yang
sama.

| Keputusan | Isi |
|---|---|
| `ACC-DEC-039` | Nama entity riwayat adalah **`AccJournalApproval`**, bukan `AccJournalApprovalHistory`. Nama pada instruksi task **bukan** sumber kebenaran yang lebih tinggi daripada artefak canonical |
| `ACC-DEC-040` | **Tanggal bisnis memakai `date`; waktu peristiwa memakai `timestamp with time zone`.** Tanggal akuntansi menentukan periode pembukuan, bukan waktu kejadian |

`revision` **tetap 6**. Alasannya: tidak ada arsitektur target, contract version, dependency,
maupun keputusan `APPROVED` sebelumnya yang berubah. Kedua keputusan memilih di antara dua bacaan
yang sudah sama-sama ada di dalam artefak, dan bacaan yang dipilih persis yang sudah
diimplementasikan `BE-ACC-004` dan `BE-ACC-005`. `decision_revision` naik `1.2` → `1.3`.

Satu artefak diperbaiki agar tidak lagi bertentangan dengan dirinya sendiri:
`erd/data-dictionary.md`. DDL contohnya menulis `timestamp` untuk kolom yang diagram ERD-nya
menulis `date`. Sepuluh baris DDL disesuaikan mengikuti `ACC-DEC-040` — empat menjadi `date`
(`StartDate`, `EndDate`, `DocumentDate`, `AccountingDate`, ditambah `EffectiveStartDate` pada
`AccChartOfAccount`) dan enam menjadi `timestamptz` (`ClosedAt`, `ReopenedAt`, `SubmittedAt`,
`ApprovedAt`, `PostedAt`, `ActionAt`).

**Nol berkas kode berubah.** Ini bukan kebetulan yang beruntung, melainkan akibat langsung dari
melaporkan pertentangan itu alih-alih memutuskannya sepihak: implementasinya sudah menunggu di
bacaan yang kemudian disahkan.

### 2 September 2026 — `BE-ACC-002` selesai dan `ACC-DEP-008` ditemukan, revisi 5 → 6

Dinaikkan atas **persetujuan owner Rizki**, 2 September 2026, setelah menerima hasil `BE-ACC-002`.

#### Kenapa revisi naik

Satu dependency arsitektur keamanan baru ditemukan: **`ACC-DEP-008` — Legal Entity Authorization
Model Availability**. Ini setara `ACC-DEP-005` yang dulu menaikkan revisi 3 ke 4.

Empat hal perlu ditegaskan supaya kenaikan ini tidak salah dibaca:

| Pernyataan | Keterangan |
|---|---|
| **Bukan perubahan scope Accounting** | Tidak ada requirement, epic, entity, endpoint, atau layar yang ditambah, dikurangi, atau digeser. Jumlah task tetap 14 backend dan 11 frontend |
| **Ini discovery dependency platform** | `BE-ACC-002` memeriksa mekanisme yang diandaikan `ACC-PERMISSION-0.1` bagian 5, dan menemukan mekanismenya tidak ada. Temuan berlaku sistem-luas, bukan khusus Accounting |
| **`BE-ACC-003` sampai `BE-ACC-005` tetap dapat berjalan** | Ketiganya hanya **mendefinisikan** kolom `LegalEntityId` pada entity. Menyimpan kolom berbeda dari menegakkannya |
| **`BE-ACC-007` sampai `BE-ACC-014` menunggu keputusan Security/Platform** | Di titik endpoint pertama, pertanyaan "pengguna ini boleh melihat badan hukum yang mana" tidak lagi dapat dihindari |

Arsitektur target, ERD, kamus data, keenam kontrak, dan seluruh keputusan `ACC-DEC-001..038`
**tidak berubah**. Yang berubah hanya register dependency dan status task.

#### Batas approval revisi 6

`approved_by` dan `approved_at` **tetap menunjuk peristiwa 1 September 2026 atas revisi 5**. Yang
disetujui owner pada 2 September 2026 adalah **pencatatan `ACC-DEP-008` dan kenaikan revisi
itu sendiri** — bukan approval ulang atas seluruh blueprint, dan bukan wewenang baru apa pun.

Wewenang yang berlaku tetap sama persis: source model persisted saja. `dotnet ef migrations add`,
`dotnet ef database update`, perubahan shared database, deployment, production activation, commit,
dan push semuanya **tetap** wewenang terpisah.

#### Dua baseline source

Baseline approved **tidak diganti**. Sesuai instruksi owner, keduanya kini dipisah eksplisit:

| Field | Nilai | Arti |
|---|---|---|
| `approved_backend_source_sha` | `aa837d7` | Baseline yang menjadi dasar approval. **Tidak boleh diganti diam-diam** |
| `verification_backend_source_sha` | `ca6b7e0` | Tempat verifikasi 2 September 2026 dijalankan |
| `approved_frontend_source_sha` | `31a82c8` | Baseline approval frontend |
| `verification_frontend_source_sha` | `5336c44` | Keadaan frontend saat verifikasi |

**Perbedaannya hanya dokumentasi/governance; source aplikasi identik.** Selisih
`aa837d7..ca6b7e0` adalah 28 berkas, seluruhnya di `docs/module-blueprints/accounting/`, dan nol
berkas source aplikasi. Untuk frontend, `31a82c8` terbukti leluhur `5336c44` — fast-forward
murni, dan tidak relevan untuk task backend.

Karena itu tidak ada impact scan yang perlu dijalankan, dan seluruh bukti yang tercatat terhadap
`aa837d7` tetap berlaku apa adanya. 17 dari 17 hash artefak canonical cocok saat diverifikasi.

#### Dua koreksi yang ikut tercatat

1. **`ACC-DEP-007` akarnya berbeda dari yang tercatat.** Bukan `4db8909` yang menghapus
   governance, melainkan merge `3d14cac` yang membatalkan perbaikan `c9692d0` (PR #63) yang sudah
   masuk. Klasifikasinya **tetap `PLATFORM / ENGINEERING GOVERNANCE`**, tetap milik lead, dan
   **tidak** masuk ke implementasi Accounting.
2. **Catatan "`ACC-DEP-002` masih menutup"** di `MODULE-STATUS.md` memeriksa salinan registry yang
   keliru. Sudah ditandai sebagai riwayat.

### 1 September 2026 — FINAL OWNER APPROVAL dan aktivasi lifecycle, revisi tetap 5

Revisi **tidak** dinaikkan. Yang disetujui owner adalah revisi `5`, dan menaikkannya justru akan
memisahkan `approved_by` dari revisi yang benar-benar disetujui. `ACC-DEC-038` dicatat sebagai
bagian dari peristiwa approval revisi 5, bukan perubahan target sesudahnya.

`status` menjadi `approved`. `decision_revision` `1.1` → `1.2` karena satu keputusan owner baru
masuk. Lifecycle registry `Acc` dinaikkan `PLANNED` → `ACTIVE`, sehingga `ACC-DEP-006` tertutup.

### 1 September 2026 — pendaftaran prefix `Acc`, revisi 4 → 5

Prefix `Acc` = *Accounting* terdaftar di registry canonical suite skill
(`QuilvianEngineeringSkills`, dua salinan, `48279dd`), Lifecycle `PLANNED`. Baris `Finance`/`Fin`
tidak disentuh. Uniqueness terverifikasi: 19 baris, seluruh prefix unik.

`ACC-DEP-002` **CLOSED untuk naming/prefix**. Dua dependency baru muncul sebagai konsekuensi:
`ACC-DEP-006` lifecycle masih `PLANNED` sehingga checker tetap menolak entity, dan
`ACC-DEP-007` governance yang dibaca checker hilang dari repo backend sehingga gerbang CI mati.

### 1 September 2026 — final hardening sebelum owner approval, revisi 3 → 4

Satu putaran hardening atas permintaan owner, sebelum approval diberikan. Tidak ada grill ulang,
tidak ada keputusan `APPROVED` yang dibuka kembali, dan struktur blueprint dipertahankan.

Revisi naik ke `4` karena empat hal material berubah: satu kontrak baru (`ACC-XMOD-0.1`), dua
kontrak naik versi (`ACC-VALIDATION-0.2`, `ACC-INTEGRATION-0.2`), satu dependency baru
(`ACC-DEP-005`), dan satu entity tambahan pada `BE-ACC-005` (`AccNumberSeries`) yang membuat
`BE-ACC-006` menjadi tujuh `CreateTable`.

Jumlah task tidak berubah: tetap 14 backend dan 11 frontend.

### 1 September 2026 — validasi status, revisi tetap 3

Revision, 15 hash artefak canonical, dan backend source SHA `aa837d7` (branch `rizkiG`) cocok
seluruhnya. Frontend source SHA **tidak cocok**: tercatat `fc49cc7` (branch `RizkiV2`), nyatanya
`31a82c8` (branch `QuilvianIntegrationFrontend`). Impact scan dijalankan lebih dahulu sesuai
aturan, hasilnya **dampak rendah** — rinciannya di
[evidence/02-frontend-rebaseline-impact-scan.md](evidence/02-frontend-rebaseline-impact-scan.md).

`frontend_source_sha` di-rebase ke `31a82c8`. `RizkiV2` terkandung penuh di dalamnya (0 ahead,
26 behind), jadi perpindahan ini fast-forward murni, bukan penggantian baseline yang divergen.

`revision` **tetap 3**: tidak ada arsitektur target, contract, dependency, atau keputusan yang
berubah. Ini perubahan status dan bukti belaka, persis kasus yang dikecualikan aturan di atas.

## Status approval

**FINAL OWNER APPROVAL diberikan 1 September 2026** oleh Rizki, Product/Domain Owner sekaligus
Implementation Owner Accounting, atas `ACC-BP-001` **revisi 5**.

> **Catatan revisi 6, 2 September 2026.** Blueprint kini berada pada revisi `6`. Yang disetujui
> owner pada 2 September 2026 adalah **pencatatan `ACC-DEP-008` dan kenaikan revisinya**, bukan
> approval ulang atas seluruh isi blueprint. Karena itu tabel di bawah **tidak diubah**: ia
> mencatat apa yang benar-benar disetujui pada peristiwa 1 September 2026. Revisi 6 tidak
> menambah wewenang apa pun dan tidak mengubah satu pun kontrak di dalamnya.

Yang disetujui pada 1 September 2026:

| Artefak | Versi |
|---|---|
| Blueprint `ACC-BP-001` | revisi `5` |
| Decision register | seluruh `ACC-DEC-001`..`038` |
| API contract | `ACC-API-0.1` |
| State transition | `ACC-STATE-0.1` |
| Validation matrix | `ACC-VALIDATION-0.2` |
| Integration contract | `ACC-INTEGRATION-0.2` |
| Permission & audit | `ACC-PERMISSION-0.1` |
| Acceptance test matrix | `ACC-TEST-0.1` |
| PRD ke MVP | `ACC-MVP-0.1` |
| Backend roadmap | revisi `2` |
| Frontend roadmap | revisi `2` |

### Yang sengaja TIDAK ikut disetujui

| Artefak | Keadaan |
|---|---|
| `ACC-XMOD-0.1` cross-module | **Hanya sisi consumer Accounting** yang disetujui. Belum `fully APPROVED` — menunggu alignment owner Finance/Yasmin dan owner Billing |
| `QBE-MIG-001` / `QBE-MIG-002` | Tetap `PROPOSED` / `PENDING_LEAD`. Approval ini **bukan** approval governance engineering canonical |

### Batas wewenang yang diberikan approval ini

Approval blueprint dan `ACC-DEC-038` memberi wewenang **source model persisted Accounting**.
Ia **tidak** memberi wewenang `dotnet ef migrations add`, `dotnet ef database update`, perubahan
shared database, deployment, production activation, maupun bypass Migration Coordination Gate.
Keempatnya tetap wewenang terpisah, dan `BE-ACC-006` tetap punya gerbangnya sendiri.
