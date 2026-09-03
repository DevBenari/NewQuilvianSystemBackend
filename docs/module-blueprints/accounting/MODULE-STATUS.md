# Accounting — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `ACC-BP-001` |
| Module name | `Accounting` |
| Revision | `10` — dinaikkan 3 September 2026. Owner meratifikasi `ACC-TD-013` dan `ACC-TD-014`, sehingga `ACC-API` dan `ACC-VALIDATION` sama-sama naik `0.2` → `0.3`. Utang teknis terkumpul di [UTANG-TEKNIS.md](UTANG-TEKNIS.md) |
| Module status | `IN_PROGRESS` |
| Current phase | `ACC-PH-005` — `ACC-PH-004` tuntas 2 September 2026 |
| Last verified at | `3 September 2026` — `BE-ACC-011`..`014` terimplementasi. **Nol test otomatis** atas keempatnya (`ACC-TD-017`); bukti yang ada hanya `dotnet build` 0 error |
| Backend source SHA — **approved** | `aa837d784ff51cb2b889cf975ada3a204018f1f5` (branch `rizkiG`) — baseline dasar approval, **tidak diganti** |
| Backend source SHA — **verification** | `f879944` — tempat verifikasi terakhir dijalankan |
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
43 keputusan bisnis tertutup, blueprint target lengkap, dan **sepuluh task backend selesai** —
`BE-ACC-001` sampai `BE-ACC-010`. **`MVP-0` tuntas**, dan `MVP-1` berjalan: tiga endpoint master
data berdiri, ditambah jalur CRUD jurnal beserta penomorannya.

`ACC-DEC-041` (2 September 2026) menurunkan MVP menjadi **satu badan hukum**, sehingga
`ACC-DEP-008` tidak lagi memblokir task mana pun. Ia tetap `OPEN` tetapi `NON-BLOCKING`, dan
berubah menjadi prasyarat sebelum badan hukum **kedua** didaftarkan. Mekanismenya disempurnakan
`ACC-DEC-043` menjadi penjaga `IsDefault`, setelah database ternyata memuat tiga badan hukum.

| Dependency | Menahan | Pemilik | Keadaan |
|---|---|---|---|
| `ACC-DEP-008` | **Nol task** | Security / Platform | `OPEN`, `NON-BLOCKING`. Wajib selesai sebelum badan hukum kedua |
| `ACC-DEP-007` | Merge ke integration, bukan penulisan kode | Lead / pemilik registry | `OPEN` |

Dua catatan yang berada **di dalam** wewenang owner modul:

1. **`POST /journal-types/seed` sudah dipanggil** 3 September 2026 — `ACC-TD-011` **`CLOSED`**.
   `AccJournalType` kini memuat 4 baris (`JB`, `JP`, `JU`, `SA`), dan idempotensinya terbukti
   pada database sungguhan. Jalur jurnal kini dapat dipakai; yang masih kosong adalah
   **daftar akun** dan **periode**, keduanya menunggu owner.
2. **Penjaga badan hukum sudah dibangun dan terbukti** (`BE-ACC-007` acceptance 5b,
   `ACC-DEC-043`). Ia menolak keras bila badan hukum bertanda `IsDefault` bukan tepat satu.
   Keadaan sekarang: tiga badan hukum aktif, satu bertanda utama — penjaga lolos.

**FINAL OWNER APPROVAL diberikan Rizki, 1 September 2026**, atas `ACC-BP-001` revisi 5 beserta enam kontrak dan kedua roadmap. Rinciannya beserta batas wewenangnya di [blueprint-manifest.md](blueprint-manifest.md) bagian *Status approval*.

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| `ACC-PH-001`, `ACC-PH-002`, `ACC-PH-003`, `ACC-PH-004` | **`ACC-PH-005`** | — |

| Fase | Isi | Status |
|---|---|---|
| `ACC-PH-001` | Penutupan 37 keputusan bisnis | `DONE` — 1 September 2026, bukti pada decision log |
| `ACC-PH-002` | Penyusunan blueprint target: arsitektur, ERD, enam kontrak, PRD ke MVP | `DONE` — 1 September 2026, 15 artefak canonical |
| `ACC-PH-003` | Roadmap delivery vertical slice | `DONE` — 1 September 2026. 14 task backend, 11 task frontend, traceability, dan evidence tersusun. Status `DRAFT_FORWARD_TEST` |
| `ACC-PH-004` | Pembuatan entity dan migration | **`DONE`** — 2 September 2026. Tujuh entity (`BE-ACC-001`..`005`) ditambah migration `20260902081432_AddAccountingFoundation` yang diterapkan owner (`BE-ACC-006`). `CONTAMINATION GUARD` `CLEAN`, snapshot 545 tabel, 0 deletion |
| `ACC-PH-005` | Implementasi backend dan frontend MVP | **`IN_PROGRESS`** — **seluruh 14 task backend ditulis**; `BE-ACC-011`..`014` menunggu verifikasi manual. Task frontend menunggu `ACC-FE-001` (`ACC-TD-009`) |
| `ACC-PH-006` | Phase 2: integrasi otomatis, jurnal berulang, tutup buku | `NOT_STARTED` — menunggu 9 pertanyaan `DEFERRED`, `ACC-XM-001`, dan dua gerbang skill |

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| **14/14 ditulis** — 10 `DONE`, 4 `IMPLEMENTED` | `NOT_STARTED` | `NOT_STARTED` | `PENDING` — verifikasi manual owner |

| Task | Status | Bukti |
|---|---|---|
| `BE-ACC-001` kerangka modul, enum, test harness | **`DONE`** 1 September 2026 | `task/report/backend/be-acc-001-kerangka-modul-enum-dan-test-harness.md` |
| `BE-ACC-002` audit hak akses badan hukum | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-002-audit-hak-akses-badan-hukum.md` |
| `BE-ACC-003` entity daftar akun dan jenis jurnal | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-003-entity-daftar-akun-dan-jenis-jurnal.md` |
| `BE-ACC-004` entity periode akuntansi | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-004-entity-periode-akuntansi.md` |
| `BE-ACC-005` entity jurnal, baris, riwayat, alokator nomor | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-005-entity-jurnal-baris-dan-riwayat-persetujuan.md` |
| `BE-ACC-006` migration pertama dan data master awal | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-006-migration-pertama-dan-data-master-awal.md`; gate dijalankan ulang di `evidence/04-migration-coordination-gate.md` bagian 10 |
| `BE-ACC-007` API daftar akun | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-007-api-daftar-akun.md`. 8 endpoint, kelima acceptance terbukti **20 test** |
| `BE-ACC-008` API jenis jurnal | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-008-api-jenis-jurnal.md`. 4 endpoint + `POST /seed`, ketiga acceptance terbukti **18 test**. `ACC-TD-004` ditutup |
| `BE-ACC-009` API periode akuntansi | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-009-api-periode-akuntansi.md`. 5 endpoint, kelima acceptance terbukti **36 test**, nol delta kontrak |
| `BE-ACC-010` jurnal draft dan penomoran | **`DONE`** 3 September 2026 | `task/report/backend/be-acc-010-jurnal-draft-dan-penomoran.md`. 5 endpoint, **kedelapan acceptance terbukti 22 test di PostgreSQL sungguhan**, `GAP-ACC-004` **TERTUTUP**, nol delta kontrak. Berkas test-nya dihapus sesudah hijau atas instruksi owner — `ACC-TD-016` |
| `BE-ACC-011` pengajuan, persetujuan, penolakan, pengesahan | **`IMPLEMENTED`** 3 September 2026 | `task/report/backend/be-acc-011-jurnal-pengajuan-persetujuan-pengesahan.md`. 4 endpoint aksi, sembilan syarat diperiksa dua kali, nol delta kontrak. **Menunggu verifikasi manual** |
| `BE-ACC-012` buku besar dan neraca saldo | **`IMPLEMENTED`** 3 September 2026 | `task/report/backend/be-acc-012-buku-besar-dan-neraca-saldo.md`. 3 endpoint baca, nol tabel buku besar. Verifikasi performa tertunda (`ACC-TD-018`) |
| `BE-ACC-013` pembalikan dan penyesuaian | **`IMPLEMENTED`** 3 September 2026 | `task/report/backend/be-acc-013-pembalikan-dan-jurnal-penyesuaian.md`. Endpoint `reverse`; seluruh 10 endpoint grup Journal kini berdiri |
| `BE-ACC-014` saldo awal | **`IMPLEMENTED`** 3 September 2026 | `task/report/backend/be-acc-014-saldo-awal.md`. **Nol baris kode berubah** — verifikasi atas jalur yang sudah ada |

`BE-ACC-001`..`003` di-commit pada `e1ee173`; `BE-ACC-004` pada `a4df550`; `BE-ACC-005` pada
`2b152aa`; migration `BE-ACC-006` pada `f40177a` dan seeder-nya pada `0f86e84`; `ACC-DEC-041` pada
`d9a5a6e`; `BE-ACC-007` pada `5c81ae4`; `ACC-DEC-043` pada `d9a9111`; `BE-ACC-008` dan
`BE-ACC-009` pada `5918828`; **`BE-ACC-010` dan `BE-ACC-011` bersama-sama pada `0d4ad3a`**
(`BE-ACC-010` masih belum ter-commit saat `BE-ACC-011` dimulai, sehingga keduanya masuk satu
commit — menyimpang dari aturan satu commit per task, dan dicatat apa adanya); `BE-ACC-012` pada
`1083815`; `BE-ACC-013` pada `f879944`.

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

Diperbarui 3 September 2026. **Backend Accounting sudah ditulis seluruhnya.** Yang tersisa
bukan lagi pekerjaan menulis kode.

1. ~~Panggil `POST /journal-types/seed`~~ — **selesai 3 September 2026** (`ACC-TD-011` `CLOSED`).
2. **Susun daftar akun awal.** Tidak ada seeder untuk ini dan memang tidak boleh ada —
   daftar akun adalah kebijakan akuntansi rumah sakit (`02-backend-architecture.md` bagian 9.3).
3. **Bangkitkan periode** tahun buku berjalan lewat `POST /periods/generate`.
4. **Jalankan skrip uji manual** pada laporan `BE-ACC-011`..`014`, lalu naikkan statusnya dari
   `IMPLEMENTED` ke `DONE`. Selama belum, keempatnya **tidak punya bukti eksekusi apa pun**
   (`ACC-TD-017`).
5. **Putuskan `ACC-FE-001` dan `ACC-FE-003`** — dua keputusan UI yang menahan sebelas task
   frontend (`ACC-TD-009`). Ini yang paling murah dibuka dan paling besar dampaknya.
6. **Teruskan `ACC-TD-015` ke lead** — registry berselisih dua arah, menahan merge ke integration.

## Optional deterministic delivery progress

**14 dari 25 task ditulis (56%).** Backend **14 dari 14**, frontend 0 dari 11.

Dari 14 backend: **10 `DONE`** (`BE-ACC-001`..`010`) dan **4 `IMPLEMENTED`**
(`BE-ACC-011`..`014`) yang menunggu verifikasi manual owner.

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
