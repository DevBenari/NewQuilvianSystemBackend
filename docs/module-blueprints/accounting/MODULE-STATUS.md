# Accounting — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `ACC-BP-001` |
| Module name | `Accounting` |
| Revision | `7` — dinaikkan 2 September 2026 atas keputusan owner `ACC-DEC-041`; `ACC-PERMISSION` naik `0.1` → `0.2` |
| Module status | `IN_PROGRESS` |
| Current phase | `ACC-PH-005` — `ACC-PH-004` tuntas 2 September 2026 |
| Last verified at | `2 September 2026` — `BE-ACC-006` selesai; migration diterapkan owner, `CONTAMINATION GUARD` `CLEAN`, `ACC-DEP-009` **CLOSED** |
| Backend source SHA — **approved** | `aa837d784ff51cb2b889cf975ada3a204018f1f5` (branch `rizkiG`) — baseline dasar approval, **tidak diganti** |
| Backend source SHA — **verification** | `f40177a` — tempat verifikasi terakhir dijalankan; commit migration Accounting |
| Canonical integration baseline | `f90bcbe9a0b18d4f4425a4678a5a39a44356677b` — **sudah termuat**, terbukti leluhur `HEAD` |
| Frontend source SHA — **approved** | `31a82c8052a3c59445ae49e6f1ccce2bf717d6c0` (branch `QuilvianIntegrationFrontend`) |
| Frontend source SHA — **verification** | `5336c4457c8ad77abe5c9d2c134760f34a334f55` — `31a82c8` adalah leluhurnya, fast-forward murni |

Baseline approved sengaja **tidak** digeser supaya tidak ada penggantian diam-diam atas dasar
approval. Selisih `2b152aa..f40177a` adalah 71 commit, sebagian besar merge canonical integration
yang memang menjadi syarat penutupan `ACC-DEP-009`. **Nol berkas Accounting berubah** di dalamnya;
satu-satunya perubahan Accounting adalah commit migration `f40177a` sendiri.

**`ACC-DEP-009` CLOSED.** Baseline verification tidak lagi tertinggal: `f90bcbe` terbukti leluhur
`HEAD`. Migration Accounting dibuat dari baseline yang sudah lengkap, dan snapshot bertambah
**751 baris tanpa satu pun deletion** — kerusakan pola `ACC-DEP-001` tidak terulang.

Status `IN_PROGRESS` berarti ada pekerjaan aktif yang sudah diberi wewenang. Rinciannya:
40 keputusan bisnis tertutup, blueprint target lengkap, dan **enam task backend selesai** —
`BE-ACC-001` sampai `BE-ACC-006`. **`MVP-0` tuntas**: tujuh entity berdiri di database lewat satu
migration bersih, dan data master awal punya mekanisme pengisiannya.

**`BE-ACC-007` kini dapat dimulai.** `ACC-DEC-041` (2 September 2026) menurunkan MVP menjadi
**satu badan hukum**, sehingga `ACC-DEP-008` tidak lagi memblokir task mana pun. Ia tetap `OPEN`
tetapi `NON-BLOCKING`, dan berubah menjadi prasyarat sebelum badan hukum **kedua** didaftarkan.

| Dependency | Menahan | Pemilik | Keadaan |
|---|---|---|---|
| `ACC-DEP-008` | **Nol task** | Security / Platform | `OPEN`, `NON-BLOCKING`. Wajib selesai sebelum badan hukum kedua |
| `ACC-DEP-007` | Merge ke integration, bukan penulisan kode | Lead / pemilik registry | `OPEN` |

Dua catatan yang berada **di dalam** wewenang owner modul:

1. **Seeder jenis jurnal belum punya call site**, sehingga `AccJournalType` di database masih
   kosong. Keputusan owner, sejalan dengan `02-backend-architecture.md` bagian 6 yang melarang
   pemanggilan seeder di `Program.cs`. Pengisiannya menunggu `BE-ACC-008`; ini **blocker
   fungsional `BE-ACC-010`**, bukan `BE-ACC-007`.
2. **Penjaga jumlah badan hukum wajib dibangun di `BE-ACC-007`** (acceptance 5b). Ini syarat
   `ACC-DEC-041`, bukan tambahan opsional: tanpanya, mendaftarkan badan hukum kedua memberi setiap
   pengguna akses ke dua buku besar sekaligus tanpa ada yang menyadari.

**FINAL OWNER APPROVAL diberikan Rizki, 1 September 2026**, atas `ACC-BP-001` revisi 5 beserta enam kontrak dan kedua roadmap. Rinciannya beserta batas wewenangnya di [blueprint-manifest.md](blueprint-manifest.md) bagian *Status approval*.

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| `ACC-PH-001`, `ACC-PH-002`, `ACC-PH-003`, **`ACC-PH-004`** | — | `ACC-PH-005` |

| Fase | Isi | Status |
|---|---|---|
| `ACC-PH-001` | Penutupan 37 keputusan bisnis | `DONE` — 1 September 2026, bukti pada decision log |
| `ACC-PH-002` | Penyusunan blueprint target: arsitektur, ERD, enam kontrak, PRD ke MVP | `DONE` — 1 September 2026, 15 artefak canonical |
| `ACC-PH-003` | Roadmap delivery vertical slice | `DONE` — 1 September 2026. 14 task backend, 11 task frontend, traceability, dan evidence tersusun. Status `DRAFT_FORWARD_TEST` |
| `ACC-PH-004` | Pembuatan entity dan migration | **`DONE`** — 2 September 2026. Tujuh entity (`BE-ACC-001`..`005`) ditambah migration `20260902081432_AddAccountingFoundation` yang diterapkan owner (`BE-ACC-006`). `CONTAMINATION GUARD` `CLEAN`, snapshot 545 tabel, 0 deletion |
| `ACC-PH-005` | Implementasi backend dan frontend MVP | **`READY`** — `ACC-DEC-041` melepas `ACC-DEP-008` sebagai penghalang; `BE-ACC-007` dapat dimulai atas instruksi owner. Task frontend masih menunggu `ACC-FE-001` |
| `ACC-PH-006` | Phase 2: integrasi otomatis, jurnal berulang, tutup buku | `NOT_STARTED` — menunggu 9 pertanyaan `DEFERRED`, `ACC-XM-001`, dan dua gerbang skill |

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| `IN_PROGRESS` — **6/14 task**, `MVP-0` tuntas | `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` |

| Task | Status | Bukti |
|---|---|---|
| `BE-ACC-001` kerangka modul, enum, test harness | **`DONE`** 1 September 2026 | `task/report/backend/be-acc-001-kerangka-modul-enum-dan-test-harness.md` |
| `BE-ACC-002` audit hak akses badan hukum | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-002-audit-hak-akses-badan-hukum.md` |
| `BE-ACC-003` entity daftar akun dan jenis jurnal | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-003-entity-daftar-akun-dan-jenis-jurnal.md` |
| `BE-ACC-004` entity periode akuntansi | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-004-entity-periode-akuntansi.md` |
| `BE-ACC-005` entity jurnal, baris, riwayat, alokator nomor | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-005-entity-jurnal-baris-dan-riwayat-persetujuan.md` |
| `BE-ACC-006` migration pertama dan data master awal | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-006-migration-pertama-dan-data-master-awal.md`; gate dijalankan ulang di `evidence/04-migration-coordination-gate.md` bagian 10 |

`BE-ACC-001` sampai `BE-ACC-003` di-commit pada `e1ee173`; `BE-ACC-004` pada `a4df550`;
`BE-ACC-005` pada `2b152aa`; migration `BE-ACC-006` pada `f40177a`. Seeder dan test `BE-ACC-006`
**belum di-commit**.

**`MVP-0` tuntas.** Tujuh entity persisted — `AccChartOfAccount`, `AccJournalType`,
`AccAccountingPeriod`, `AccJournal`, `AccJournalLine`, `AccJournalApproval`, `AccNumberSeries` —
kini berdiri di database lewat satu migration bersih. Model EF Core tidak lagi mendahului
`ApplicationDbContextModelSnapshot.cs`: snapshot memuat **545 tabel** dengan **7 `Acc*`**, dan
migration `20260902081432_AddAccountingFoundation` memuat tepat **tujuh `CreateTable` dan 21
`CreateIndex`** seperti yang diperkirakan. Snapshot bertambah **751 baris tanpa satu pun
deletion**.

**Dua pertentangan antar artefak canonical sudah SELESAI**, 2 September 2026, lewat
`ACC-DEC-039` (nama entity riwayat tetap `AccJournalApproval`) dan `ACC-DEC-040` (tanggal bisnis
`date`, waktu peristiwa `timestamp with time zone`). Keduanya menguatkan pilihan yang sudah
diimplementasikan, sehingga **nol berkas kode berubah**. `erd/data-dictionary.md` diperbaiki agar
DDL contohnya tidak lagi bertentangan dengan diagram ERD-nya sendiri.

## Blockers and owners

| Blocker ID | Ringkasan | Owner | Fase terdampak | Kelanjutan yang tetap aman |
| --- | --- | --- | --- | --- |
| ~~`ACC-DEP-002`~~ | ~~Prefix belum terdaftar~~ | — | — | **CLOSED 1 September 2026.** `Acc` terdaftar, Lifecycle `PLANNED` |
| ~~`ACC-DEP-006`~~ | ~~Lifecycle `PLANNED`~~ | — | — | **CLOSED 1 September 2026** lewat `ACC-DEC-038`. Lifecycle `ACTIVE` |
| `ACC-DEP-007` | **Separuh selesai.** PR #72 `b19c01e` memulihkan path checker (2 Sep 2026), tetapi registry backend masih nol baris `Acc`. Checker kini hidup dan diperkirakan menolak `QBE-MOD-002` | Lead / pemilik registry | Merge ke integration | Penulisan kode lokal tetap jalan. Perlu satu baris registry; rinciannya di `evidence/04-migration-coordination-gate.md` bagian 8 |
| ~~`ACC-DEP-009`~~ | ~~Baseline `rizkiG` tertinggal 5 migration dan 8 tabel~~ | — | — | **CLOSED 2 September 2026.** Owner menyegarkan `rizkiG`; `f90bcbe` terbukti leluhur `HEAD`. Bukti: `evidence/04-migration-coordination-gate.md` bagian 10 |
| `ACC-DEP-005` | Aturan koordinasi migration bersama (`QBE-MIG-001`/`002`) belum canonical | Lead | **Tidak lagi mengikat task.** `BE-ACC-006` sudah lewat memakai teks usulannya | Seluruh task. Tetap terbuka sebagai pekerjaan governance lead, bukan penghalang Accounting |
| `ACC-XM-001` | Siapa penerbit kejadian keuangan resmi — `CROSS_MODULE_DECISION_REQUIRED` | Owner Billing + Owner Finance/Yasmin + Rizki | `ACC-PH-006` Phase 2 | **Tidak memblokir MVP.** Bentuk batasnya sudah ditulis di `ACC-XMOD-0.1` |
| `ACC-DEP-008` | **Legal Entity Authorization Model Availability** — mekanismenya tidak ada. Status `OPEN` tetapi **`NON-BLOCKING`** sejak `ACC-DEC-041` | **Security / Platform** | **Nol task.** Prasyarat sebelum badan hukum **kedua** didaftarkan | **Seluruh task.** MVP satu badan hukum; pemisahan data tetap ditegakkan, dan penjaga jumlah badan hukum (`BE-ACC-007` 5b) menahan pintunya |
| `ACC-FE-001`, `ACC-FE-003` | Letak menu dan bentuk layar rincian jurnal | Product owner | Task frontend | Seluruh task backend tidak terpengaruh |
| `ACC-XM-001` | Siapa menerbitkan kejadian keuangan resmi | Owner Billing, owner Finance, Rizki | `ACC-PH-006` Phase 2 | **Tidak memblokir MVP** — rilis pertama tanpa jurnal otomatis |

**Catatan lama yang sudah tidak berlaku, dipertahankan sebagai riwayat.** Paragraf di posisi ini
sebelumnya menyatakan `ACC-DEP-002` masih menutup karena registry `origin/QuilvianIntegrationBackend`
memuat nol baris `Acc`. Pernyataan itu memeriksa salinan yang keliru — registry canonical yang
mengikat agent ada di suite skill `QuilvianEngineeringSkills`, dan di sana baris `Acc` sudah
`ACTIVE` sejak 1 September 2026.

Selisih itu sendiri nyata dan tetap dilacak, tetapi **bukan** sebagai blocker Accounting,
melainkan sebagai bagian `ACC-DEP-007`: salinan registry di backend adalah `synced consumer` yang
tertinggal tepat satu pendaftaran — 48 baris berbanding 52, dan selisihnya seluruhnya baris
Accounting.

## Stale evidence

| Artefak/bukti | SHA tercatat | SHA terkini | Impact review yang dibutuhkan |
| --- | --- | --- | --- |
| Baseline backend | `aa837d7` | `ca6b7e0` | **Tidak perlu.** Selisihnya 28 berkas, seluruhnya dokumentasi blueprint. Source aplikasi identik |
| Baseline frontend | `31a82c8` | `5336c44` | **Tidak perlu untuk task backend.** `31a82c8` terbukti leluhur `5336c44`, jadi fast-forward murni. Impact scan menyusul bila task frontend dimulai |

Kedua baseline diverifikasi ulang 2 September 2026 sebelum `BE-ACC-002` dijalankan, dan 17 dari
17 hash artefak canonical cocok.

Baseline frontend di-rebase pada 1 September 2026 dari `fc49cc7` (`RizkiV2`) ke `31a82c8`
(`QuilvianIntegrationFrontend`) setelah impact scan. Drift-nya 30 commit dan 161 berkas, tetapi
**seluruhnya di `health-services`** sementara Accounting berada di `Corporate`; empat dari enam
anchor reuse tidak berubah dan dua sisanya hanya bertambah. Nol artefak perlu direvisi, sehingga
`revision` tetap `3`. Buktinya di
[evidence/02-frontend-rebaseline-impact-scan.md](evidence/02-frontend-rebaseline-impact-scan.md).

`RizkiV2` terkandung penuh di dalam `QuilvianIntegrationFrontend` (0 ahead, 26 behind), jadi
perpindahan ini fast-forward murni — tidak ada pekerjaan yang hilang.

## Next recommended task

Diperbarui 2 September 2026. Dua yang pertama **bukan pekerjaan coding** — keduanya mengeskalasi
temuan kepada pemiliknya masing-masing:

1. **Teruskan `evidence/03-acc-dep-007-governance-propagation.md` ke lead.** Gerbang CI QBE mati
   pada setiap PR ke integration. Perbaikannya lima baris dan sudah pernah ditinjau lewat PR #63.
2. **Teruskan `evidence/02-legal-entity-authority.md` ke owner keamanan platform.** Mekanisme hak
   akses badan hukum tidak ada, dan dampaknya melampaui Accounting — 40 model menyimpan
   `LegalEntityId`, sebagian besar milik Human Resource.
3. **Putuskan `ACC-FE-001`** letak menu Accounting. Murah, tetapi menahan seluruh rantai task
   frontend. Dua preseden segar ada di `src/utils/menu-sidebar/menu-items.jsx` — seksi
   "Rekam Medis" dan "Operasi".

Task backend berikutnya yang sah dikerjakan adalah **`BE-ACC-003`**, entity daftar akun dan jenis
jurnal. Ia `EXECUTION_READY` dan **tidak** tertahan `ACC-DEP-008`, karena menyimpan kolom
`LegalEntityId` berbeda dari menegakkannya. Tetap menunggu instruksi eksplisit owner — approval
roadmap bukan perintah jalan.

Roadmap sudah tersusun; **tidak perlu** menjalankan `/plan-module-delivery` lagi. Pertimbangkan
`/trace-existing-capabilities` bila capability map yang masih parsial ingin dilengkapi.

## Optional deterministic delivery progress

**2 dari 25 task selesai (8%).** Roadmap memuat 25 task — 14 backend dan 11 frontend. Blueprint
dan roadmap sudah `APPROVED` sejak 1 September 2026, sehingga denominatornya kini bermakna.

Rinciannya: backend 2 dari 14 (`BE-ACC-001`, `BE-ACC-002`), frontend 0 dari 11.

## Status contract

`DRAFT` berarti identitas sudah ada tetapi pengumpulan masukan belum lengkap. `DISCOVERY` sedang
mengumpulkan keputusan dan bukti. `READY` berarti fase yang direncanakan boleh mulai. `PARTIAL`
berarti setidaknya satu fase siap sementara fase lain terblokir atau belum diketahui. `BLOCKED`
berarti tidak ada satu pun fase material yang aman dijalankan. `IN_PROGRESS` berarti ada
pekerjaan aktif yang sudah diberi wewenang. `VERIFYING` menunggu bukti kesiapan. `DONE` menuntut
bukti verifikasi yang memadai. `SUPERSEDED` mencatat blueprint penggantinya.

Status fase memakai `NOT_STARTED`, `READY`, `IN_PROGRESS`, `BLOCKED`, `DONE`, dan `SUPERSEDED`.
Sebuah fase menjadi `DONE` hanya bila bukti acceptance-nya tercatat; keberadaan berkas saja tidak
cukup.
