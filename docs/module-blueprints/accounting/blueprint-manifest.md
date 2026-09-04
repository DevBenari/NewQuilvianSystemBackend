# Accounting — Blueprint Manifest

```yaml
blueprint_id: ACC-BP-001
module_name: Accounting
module_slug: accounting
module_prefix: ACC
revision: 9
status: approved
current_phase: ACC-PH-005
created_at: 2026-09-01T09:53:36+07:00
updated_at: 2026-09-04T00:00:00+07:00
last_verified_at: 2026-09-03T21:00:00+07:00
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
verification_backend_source_sha: 822d48a
verification_frontend_source_sha: a57074f3d
verification_baseline_note: >
  BASELINE FRONTEND DIGESER 4 September 2026: 1a86d9322 -> a57074f3d. Impact scan dijalankan
  lebih dahulu. Dua commit: bf4fd0ed6 milik owner, 53 berkas 6.754 baris, SELURUHNYA pekerjaan
  Accounting FE-ACC-001..006 beserta store.jsx dan menu-items.jsx -- nol berkas asing, nol modul
  lain tersentuh. a57074f3d memperbaiki dua cacat temuan owner pada form jurnal, tepat 2 berkas.
  IMPACT SCAN 4 September 2026, dijalankan /qv-lanjut sebelum FE-ACC-002..004 karena kedua
  baseline verification meleset dari yang tercatat. Backend f879944 -> 822d48a: 27 commit,
  dan di dalam Areas/Corporate/AccountingManagement/ hanya DUA berkas berubah, keduanya milik
  JournalType (ACC-TD-019, RequiresApproval dikunci true dan dicabut dari request DTO).
  ChartOfAccount 0 berkas, AccountingPeriod 0 berkas -- FE-ACC-002 dan FE-ACC-004 nol dampak.
  FE-ACC-003 TERDAMPAK: form jenis jurnal tidak boleh memuat isian RequiresApproval, dan ini
  berbeda dari daftar DTO ACC-API-0.3 yang masih mencantumkannya (ACC-GAP-004 pada
  testing/readiness-report.md). Source yang diikuti, delta dicatat.
  Frontend 5336c44 -> 1a86d933: 17 commit. Sebelas anchor reuse FE-ACC-002..004 diperiksa satu
  per satu; SEPULUH identik, satu berubah dan perubahannya ADITIF -- resource-filter-select.jsx
  bertambah prop selectedOption, +7/-0, nol breaking change. Build, lint, dan 434 unit test
  hijau pada 1a86d933. Kedua baseline karena itu digeser ke SHA di atas.

  Backend bergerak aa837d7 -> ca6b7e0 -> e1ee173 -> a4df550 -> 2b152aa -> f40177a sepanjang
  2 September 2026, dan tiap pergeseran diverifikasi impact scan lebih dahulu. ca6b7e0 menambah
  28 berkas dokumentasi blueprint saja. e1ee173 adalah commit BE-ACC-001..003 oleh owner.
  a4df550 adalah commit BE-ACC-004, tepat 8 berkas. 2b152aa adalah commit BE-ACC-005, 16 berkas.
  Kelimanya nol sentuhan pada Migrations/, ModelSnapshot, Program.cs, tooling/, agents/,
  .github/, dan modul lain.
  PERGESERAN TERAKHIR BERBEDA SIFATNYA. 2b152aa -> f40177a adalah 71 commit, sebagian besar merge
  canonical integration yang memang menjadi syarat penutupan ACC-DEP-009. Impact scan: NOL berkas
  Accounting berubah di dalamnya; Program.cs bergerak +9/-2 seluruhnya milik Radiology dan Medical
  Record, nol baris Accounting dan nol pemanggilan seeder. f40177a sendiri menyentuh tepat 3
  berkas — migration, designer, snapshot. Seluruh bukti terhadap aa837d7 tetap berlaku, dan
  17/17 hash artefak canonical cocok saat diverifikasi ulang.
  5918828 adalah commit BE-ACC-008 dan BE-ACC-009 oleh owner, tepat 15 berkas: 6 source, 2 berkas
  test, dan 7 dokumen blueprint. Nol sentuhan Migrations/, ModelSnapshot, entity, configuration, dan
  modul lain; Program.cs hanya bertambah 4 baris registrasi service Accounting. BE-ACC-010 berjalan
  di atas SHA ini dengan working tree bersih, dan 17/17 hash artefak canonical cocok saat
  diverifikasi ulang 3 September 2026 sebelum implementasi dimulai.
  0f86e84 adalah commit BE-ACC-006 oleh owner, tepat 7 berkas: seeder, test-nya, laporan task, dan
  4 register. Nol sentuhan Migrations/, ModelSnapshot, Program.cs, entity, configuration, dan modul
  lain. Frontend: 31a82c8 adalah leluhur 5336c44 (fast-forward murni), tidak relevan untuk backend.
canonical_integration_baseline: f90bcbe9a0b18d4f4425a4678a5a39a44356677b
integration_baseline_note: >
  TERSELESAIKAN 2 September 2026. rizkiG dulu tertinggal 5 migration dan 8 tabel dari f90bcbe,
  dan itu MENAHAN BE-ACC-006. Owner menyegarkan branch lebih dulu, sehingga f90bcbe kini terbukti
  leluhur HEAD dan ACC-DEP-009 CLOSED. Migration BE-ACC-006 dibuat dari baseline yang sudah
  lengkap: snapshot bertambah 751 baris TANPA satu pun deletion, jadi pola kerusakan ACC-DEP-001
  tidak terulang. Snapshot kini 545 tabel dengan 7 Acc*.
  Bukti: evidence/04-migration-coordination-gate.md bagian 10.
verified_at: 2026-09-03
skill_suite_version: 1.0.0-rc2
input_revision_hash: ACC-PRD-001@0.1 + 00-interview-decisions@3
decision_revision: 1.6
input_revisions:
  interview_decisions: 7
  capability_map: 2
  requirement_gate: null
  hospital_domain_architecture: null
contract_versions:
  api: ACC-API-0.4
  state: ACC-STATE-0.1
  validation: ACC-VALIDATION-0.3
  integration: ACC-INTEGRATION-0.2
  permission: ACC-PERMISSION-0.3
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
  00-interview-decisions.md: aaeb385f6194d707294777ab5c90ce178e216fc3c28097fd8198d51676759038
  01-existing-capability-map.md: df5c5375f04ba9f688a49ac6504f53d05995545507b75a05c19dcf707e5e59ea
  02-backend-architecture.md: 4a77b937cf2953ace1a7060f704f729674e26eb4545fc7f0fced1e7bcfa057a9
  03-frontend-architecture.md: a8e3f7c002359683c6c24a5d6cdc170c916fe4017cf01b7bb7cf05b79375c544
  04-prd-to-mvp.md: 1da14a42f09030625641f9769ebd1839773125c4e8b94e36016e363e311ca081
  06-shared-migration-coordination-rule.md: e1111572749627931b81da86c779c472197ab821790a6e5568900068b608d428
  contracts/api-contract.md: b4a20208526e5cac65983d4a8bddb40a2c2a553a134824f8437d363422f1b7b0
  contracts/state-transition-matrix.md: 34ef47ca2fb0b8dce9c8e5336b267e16f9878635d75ab7bd033affe0fca687b5
  contracts/validation-matrix.md: 11df3f472f2d71e5a57e34db606d1c3a78105ebfce008ddb0a3a509ac09b4643
  contracts/integration-contract.md: 1c773b03b30a272459de9db436bded581d0593e1897a89e847fdbc023679e094
  contracts/permission-audit-matrix.md: 6200bdf8d32a568f3aff7aeb7d9c1446e6fec9061937a1d05ff06945ecc78d82
  contracts/cross-module-contract.md: a17b2449c9d21471af8473e97e254b5f6f3e8dfda73793d22abf79b71cceef9f
  testing/acceptance-test-matrix.md: 78017727be1c7dd773987b96b4e3a8d5b9572013d350eb78aa598bfe673ca7c1
  erd/data-dictionary.md: 2315d2f525ae5870cc7c0a8a2af2b3051b16c71b6e89e3d25ad22145d00ad1f1
  roadmap/backend-roadmap.md: cad5f84ea2fc0def429c4ea504340f1d6523ed5e4eee47b6617636e1a084515f
  roadmap/frontend-roadmap.md: f29e99de8d78a841bf20ffd41a1d13a275f9b179c1188c17c52d2ccf64d98158
  roadmap/requirement-traceability.md: b2826cfc29531ea69cab31a922faaad1aaf691cb4efe23e531aa99520211690f
active_dependency_ids: [ACC-DEP-003, ACC-DEP-004, ACC-DEP-005, ACC-DEP-007, ACC-DEP-008]   # 001, 002, 006, 009 CLOSED; 008 OPEN tapi NON-BLOCKING sejak ACC-DEC-041
entity_prefix:
  prefix: Acc
  status: REGISTERED
  lifecycle: ACTIVE
  registered_at: 2026-09-01
  activated_at: 2026-09-01
  activation_decision: ACC-DEC-038
  registry_path: QuilvianEngineeringSkills/agents/rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md
  entity_creation_authorized: true    # source model persisted saja
  migration_authorized: true          # turun & dipakai 2 Sep 2026 — migration BE-ACC-006 diterapkan owner pada f40177a
  database_execution_authorized: true # terbatas pada `dotnet ef database update` BE-ACC-006 oleh owner; TIDAK berlaku untuk task lain
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

### 2 September 2026 — `ACC-DEC-043`, revisi 8 → 9

Dinaikkan atas keputusan owner setelah **pemeriksaan read-only pertama terhadap database
sungguhan**. `ACC-PERMISSION` naik `0.2` → `0.3`.

#### Apa yang ditemukan

Owner meminta badan hukum pertama dibuatkan, dengan keterangan hanya membangun untuk satu rumah
sakit. Pemeriksaan dijalankan lebih dahulu — dan menemukan **tiga badan hukum aktif**, bukan nol:

| Kode | Nama | `IsDefault` | Site | Unit | Cost Center | Lokasi |
|---|---|:---:|---:|---:|---:|---:|
| `LE-MMC-001` | PT Metropolitan Medical Centre | **Ya** | 1 | 5 | 5 | 3 |
| `LE-MDC-001` | PT Metropolitan Diagnostic Centre | — | 1 | 0 | 0 | 0 |
| `LE-MHS-001` | PT Metropolitan Healthcare Services | — | 1 | 0 | 0 | 0 |

Penjaga `ACC-DEC-041` versi pertama menolak bila badan hukum **aktif** lebih dari satu, sehingga ia
akan langsung mematikan seluruh modul Accounting. Bila permintaan owner dituruti tanpa memeriksa,
jumlahnya menjadi empat dan keadaannya makin jauh dari rancangan.

#### Yang diputuskan

**Accounting berjalan di atas badan hukum bertanda `IsDefault`, dan penjaga menuntut tepat satu
default.** Nol default maupun default ganda tetap ditolak keras `409`.

Bahaya yang dijaga bukan "ada lebih dari satu badan hukum di master", melainkan **ketidakjelasan
buku besar mana yang disentuh**. `IsDefault` sudah menjawabnya, dan ia kolom platform yang sudah
ada — bukan konsep baru yang dikarang Accounting.

`ACC-DEC-041` **tidak dibatalkan**; hanya mekanismenya yang disempurnakan. MVP tetap berjalan di
atas satu badan hukum, dan `ACC-DEP-008` tetap `OPEN` serta `NON-BLOCKING`.

#### Batas yang dijaga

**Nol data modul lain disentuh.** `LE-MDC-001` dan `LE-MHS-001` dibiarkan apa adanya — keduanya
punya `MstHospitalSite` dan mungkin dirujuk modul lain. Menonaktifkannya keputusan pemilik master
data organisasi, dicatat sebagai `ACC-TD-010`.

**Nol tulis ke database.** Seluruh pemeriksaan `SELECT`, dan badan hukum yang diminta owner
**tidak jadi dibuat** — karena ternyata sudah ada, dan menambah satu lagi justru merugikan.

Diverifikasi terhadap database sungguhan: badan hukum utama berjumlah **1**, penjaga **lolos**,
Accounting berjalan di atas `LE-MMC-001`.

#### Yang berubah

| Artefak | Perubahan |
|---|---|
| `00-interview-decisions.md` | `ACC-DEC-043`; `interview_decisions` 6 → 7 |
| `contracts/permission-audit-matrix.md` | `0.2` → `0.3`; uraian penjaga |
| `roadmap/backend-roadmap.md` | Acceptance `BE-ACC-007` (5b) |
| `UTANG-TEKNIS.md` | `ACC-TD-002` diperbarui; `ACC-TD-010` baru |
| `AccountingLegalEntityGuard.cs` | Mekanisme `IsDefault`, ditambah `AmbilBadanHukumUtamaAsync` |

Test Accounting 42 → **44**, project `Tests/` **220**, nol gagal. Nol migration, snapshot tetap
545 tabel.


### 2 September 2026 — `ACC-DEC-042` dan `BE-ACC-007` selesai, revisi 7 → 8

Dinaikkan atas keputusan owner. Satu kontrak berubah versi: **`ACC-API-0.1` → `ACC-API-0.2`**.

#### `ACC-DEC-042` — kode akun dapat diubah selama belum dipakai

Menyelesaikan pertentangan yang ditemukan saat `BE-ACC-007`: `ACC-API-0.1` menulis `PUT` hanya
mengubah "nama, induk, atau keterangan", sedangkan `ACC-VALIDATION-0.2` bagian 1 dan acceptance
`BE-ACC-007` (4) sama-sama mengandaikan `AccountCode` **dapat** diubah pada keadaan lain. Aturan
validasi yang melarang sesuatu yang tidak pernah mungkin adalah aturan kosong.

Owner memilih bacaan validation matrix. Deskripsi `PUT` diperbaiki, dan implementasi sudah memakai
bacaan itu sejak awal — **nol berkas kode berubah** karena keputusan ini.

#### `BE-ACC-007` `DONE`

Delapan endpoint daftar akun, lima berkas (1.127 baris), satu baris `AddScoped`. Kelima acceptance
terbukti **18 test**; seluruh Accounting kini **42 test**, project `Tests/` **218 test**, nol
gagal, nol regresi.

Dua butir paling berisiko lolos: penyaring `JournalStatus == Posted` pada acceptance (3), dan
penjaga `ACC-DEC-041` pada (5b). Dengan (5b) terbukti, syarat yang mengikat `ACC-DEC-041`
terpenuhi — bukan sekadar dibangun.

#### Register utang teknis dibuka

Atas instruksi owner *"hiraukan blocking agar project bisa selesai, tinggal catat kekurangannya"*,
dibuat **[UTANG-TEKNIS.md](UTANG-TEKNIS.md)** berisi sembilan butir `ACC-TD-001`..`009`. Dua yang
berat: `ACC-TD-002` (penyaringan badan hukum, milik Security/Platform) dan `ACC-TD-009` (dua
keputusan UI yang menahan seluruh frontend — **milik owner sendiri**).

Satu butir baru ditemukan justru karena test dijalankan: **`ACC-TD-001`** — check constraint
`CK_AccJournalLine_TepatSatuSisiTerisi` mustahil dipenuhi di SQLite karena EF menyimpan `decimal`
sebagai TEXT di sana. Bukan cacat produksi; migration dan configuration keduanya benar untuk
PostgreSQL.

#### Yang berubah

| Artefak | Perubahan |
|---|---|
| `00-interview-decisions.md` | `ACC-DEC-042` ditambahkan; `interview_decisions` 5 → 6 |
| `contracts/api-contract.md` | `ACC-API-0.1` → `0.2`; deskripsi `PUT` diperbaiki |
| `roadmap/backend-roadmap.md` | `BE-ACC-007` → `DONE` |
| `MODULE-STATUS.md` | 7/14 task `DONE` |
| `UTANG-TEKNIS.md` | **Baru** — 9 butir |
| `evidence/07-acc-dep-007-ringkasan-untuk-lead.md` | **Baru** — serah terima satu baris registry |

**Yang TIDAK berubah:** ERD, kamus data, `02-backend-architecture.md`, lima kontrak lain, dan
`ACC-DEC-001`..`041`. Nol migration, snapshot tetap 545 tabel.


### 2 September 2026 — `ACC-DEC-041`, revisi 6 → 7

Dinaikkan atas **keputusan owner Rizki**, 2 September 2026. Berbeda dari `ACC-DEC-039`/`040` yang
sengaja **tidak** menaikkan revisi, keputusan ini **mengubah target**, sehingga revisinya naik.

#### Kenapa revisi naik

Satu kontrak berubah versi: **`ACC-PERMISSION-0.1` → `ACC-PERMISSION-0.2`**. Aturan kedua pada
bagian 5 — *"pengguna hanya boleh menyentuh badan hukum yang menjadi haknya"* — ditandai
`DEFERRED` dan diberi pengganti sementara. Menurut aturan revisi di berkas ini, perubahan versi
kontrak adalah perubahan material.

#### Isi keputusan

**MVP berjalan pada satu badan hukum.** Yang ditunda **hanya penyaringan per pengguna**; yang
tetap berlaku adalah **pemisahan data**. Keduanya sering tertukar, jadi ditegaskan:

| Hal | Keadaan |
|---|---|
| Kode akun unik per badan hukum | **Tetap** — unique index sudah berdiri di database |
| Satu jurnal tidak mencampur dua badan hukum | **Tetap** — `BE-ACC-010` acceptance (7) |
| `LegalEntityId` pada tiga tabel | **Tetap ada.** Kolom tidak dibuang, `ACC-DEC-037` tidak dibatalkan |
| Penolakan `403` atas badan hukum bukan hak pengguna | **`DEFERRED`** — mekanismenya tidak ada di platform |
| **Penjaga jumlah badan hukum** | **BARU, wajib** — `BE-ACC-007` acceptance (5b) |

#### Kenapa menunda, bukan membuang kolomnya

Owner sempat mempertimbangkan membuang `LegalEntityId` sepenuhnya. Tiga hal membuat penundaan
lebih baik daripada pembuangan, dan ketiganya diperiksa lebih dahulu:

1. **Konsepnya tidak dapat dihindari.** `MstCostCenter.LegalEntityId` berstatus `[Required]`,
   sedangkan Accounting wajib merujuk cost center untuk akun beban (`ACC-DEC-019`). Membuang kolom
   dari tabel `Acc*` hanya membuat konsepnya implisit dan tidak terlacak.
2. **Titik murahnya sudah lewat.** Laporan `BE-ACC-005` memperingatkan perubahan semacam ini
   *"murah sekarang dan mahal setelah migration terbit"*. Migration sudah diterapkan pada
   `f40177a`, sehingga pembuangan menuntut migration baru (3 `DropForeignKey`, 3 `DropIndex`,
   3 `DropColumn`, 3 `CreateIndex`) dan Migration Coordination Gate dijalankan ulang.
3. **Hasil praktisnya sama.** Tujuan owner — tidak ada yang perlu membangun otorisasi badan hukum,
   dan modul tidak berhenti — tercapai penuh tanpa satu pun migration.

#### Apa yang sebenarnya membuka blokir

Bukan kelonggaran, melainkan pembacaan ulang artefak yang sudah ada sejak awal:

- `04-prd-to-mvp.md` baris 106 sudah menetapkan MVP selesai ketika **satu badan hukum** berjalan
  dari saldo awal sampai neraca saldo.
- `UAT-15` menguji **pemisahan data** — penguji yang sama membuka kedua badan hukum dan saldonya
  tidak tercampur. Ia tidak pernah menguji bahwa pengguna ditolak.
- Dari lima acceptance `BE-ACC-007`, hanya butir (5) yang menyentuh otorisasi. Empat lainnya tidak.

Jadi `ACC-DEP-008` selama ini memblokir **satu baris acceptance di tiap task**, bukan keseluruhan
task — dan blokir itu berasal dari klaim multi-badan-hukum yang tidak pernah menjadi syarat MVP.

#### Yang berubah

| Artefak | Perubahan |
|---|---|
| `00-interview-decisions.md` | `ACC-DEC-041` ditambahkan; `input_revisions.interview_decisions` 4 → 5 |
| `contracts/permission-audit-matrix.md` | `ACC-PERMISSION-0.1` → `0.2`; aturan kedua bagian 5 `DEFERRED` |
| `roadmap/backend-roadmap.md` | `BE-ACC-007` acceptance (5) `DEFERRED`, (5b) baru; status `BE-ACC-007`..`014` menjadi `ROADMAP_READY` |
| `testing/acceptance-test-matrix.md` | `UAT-15` `DEFERRED`, tidak dihapus |
| `05-prerequisite-readiness.md` | `ACC-DEP-008` menjadi `OPEN` tetapi `NON-BLOCKING` |
| `MODULE-STATUS.md` | `ACC-PH-005` menjadi `READY` |

**Yang TIDAK berubah:** ERD, kamus data, `02-backend-architecture.md`, lima kontrak lain, dan
`ACC-DEC-001`..`040`. Nol berkas kode berubah, nol migration, snapshot tetap 545 tabel.

#### Batas yang mengikat keputusan ini

`ACC-DEP-008` **tidak ditutup**. Ia wajib selesai **sebelum badan hukum kedua didaftarkan**, dan
sampai saat itu penjaga pada `BE-ACC-007` yang menahan pintunya. Bila penjaga itu tidak dibangun,
`ACC-DEC-041` kehilangan dasarnya.


### 2 September 2026 — `BE-ACC-006` selesai dan `ACC-DEP-009` CLOSED, revisi tetap 6

`revision` **tetap 6**. Tidak ada arsitektur target, contract version, ERD, kamus data, maupun
keputusan `APPROVED` yang berubah. Yang berubah hanya status task, satu dependency yang tertutup,
dan dua wewenang yang memang sudah dijadwalkan turun di gerbang `BE-ACC-006`.

| Yang berubah | Dari | Menjadi |
|---|---|---|
| `verification_backend_source_sha` | `2b152aa` | `f40177a`, lalu `0f86e84` setelah owner meng-commit `BE-ACC-006` |
| `current_phase` | `ACC-PH-003` | `ACC-PH-005` |
| `migration_authorized` | `false` | `true` — dipakai sekali, oleh owner, untuk `BE-ACC-006` |
| `database_execution_authorized` | `false` | `true` — **terbatas** pada `dotnet ef database update` `BE-ACC-006` |
| `active_dependency_ids` | memuat `ACC-DEP-009` | `ACC-DEP-009` **CLOSED** |
| `roadmap/backend-roadmap.md` hash | `4f4dd68…` | `84a2f51…` |

**Kedua wewenang itu tidak berlaku umum.** Ia turun untuk satu migration `BE-ACC-006` yang isinya
sudah diperiksa dan lulus `CONTAMINATION GUARD`, dan **tidak** memberi wewenang `dotnet ef
migrations add` berikutnya, perubahan shared database di luar itu, deployment, maupun production
activation. Setiap task sesudah ini kembali memerlukan wewenangnya sendiri.

#### Kenapa `ACC-DEP-009` tertutup

Bukti git, bukan pernyataan: `git merge-base --is-ancestor f90bcbe HEAD` bernilai benar, sehingga
ketertinggalan lima migration dan delapan tabel yang dulu menggagalkan Migration Coordination Gate
sudah hilang. Migration yang dihasilkan bersih — 7 `CreateTable` dan 21 `CreateIndex`, seluruhnya
`Acc*`, nol operasi asing — dan snapshot bertambah **751 baris tanpa satu pun deletion**, sehingga
pola kerusakan `ACC-DEP-001` tidak terulang.

**Satu batas dicatat apa adanya:** gate **tidak** dijalankan ulang sebelum migration dibuat.
Langkah 5 pada `evidence/04-migration-coordination-gate.md` bagian 7 dilewati, dan yang ada
sekarang adalah verifikasi sesudahnya. Ia memeriksa hal yang sama persis, tetapi urutannya bukan
yang diatur aturan, dan itu dicatat di bagian 10 berkas tersebut.

#### Satu butir DoD yang belum tertutup penuh

Seeder empat jenis jurnal sudah ada dan terbukti enam test, tetapi **belum punya call site**,
sehingga `AccJournalType` di database masih kosong. Ini keputusan owner pada hari yang sama, dan
sejalan dengan `02-backend-architecture.md` bagian 6 yang melarang pemanggilan seeder di
`Program.cs`. Pengisiannya menunggu `BE-ACC-008`.


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
