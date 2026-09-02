# Accounting — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `ACC-BP-001` |
| Module name | `Accounting` |
| Revision | `6` — dinaikkan 2 September 2026 atas persetujuan owner, karena `ACC-DEP-008` ditemukan |
| Module status | `IN_PROGRESS` |
| Current phase | `ACC-PH-004` |
| Last verified at | `2 September 2026` — `BE-ACC-002` selesai; `ACC-DEP-008` dibuka; akar `ACC-DEP-007` dikoreksi |
| Backend source SHA — **approved** | `aa837d784ff51cb2b889cf975ada3a204018f1f5` (branch `rizkiG`) — baseline dasar approval, **tidak diganti** |
| Backend source SHA — **verification** | `ca6b7e0ef3af4454cae11709739b1f36657352e2` — tempat verifikasi 2 September 2026 dijalankan |
| Frontend source SHA — **approved** | `31a82c8052a3c59445ae49e6f1ccce2bf717d6c0` (branch `QuilvianIntegrationFrontend`) |
| Frontend source SHA — **verification** | `5336c4457c8ad77abe5c9d2c134760f34a334f55` — `31a82c8` adalah leluhurnya, fast-forward murni |

**Perbedaan kedua baseline hanya dokumentasi/governance; source aplikasi identik.** Selisih
`aa837d7..ca6b7e0` adalah 28 berkas, seluruhnya di `docs/module-blueprints/accounting/`, dan nol
berkas source aplikasi. Baseline approved sengaja **tidak** digeser supaya tidak ada penggantian
diam-diam atas dasar approval.

Status `IN_PROGRESS` berarti ada pekerjaan aktif yang sudah diberi wewenang. Rinciannya:
37 keputusan bisnis tertutup, blueprint target lengkap, dan **dua task backend sudah selesai** —
`BE-ACC-001` (1 September) dan `BE-ACC-002` (2 September). Pembuatan entity **tidak lagi
terhalang**: `ACC-DEP-001`, `ACC-DEP-002`, dan `ACC-DEP-006` seluruhnya tertutup.

Yang masih terbuka dua, dan keduanya di luar wewenang owner modul: `ACC-DEP-007` menahan merge ke
integration, dan `ACC-DEP-008` menahan pembuatan endpoint mulai `BE-ACC-007`.

**FINAL OWNER APPROVAL diberikan Rizki, 1 September 2026**, atas `ACC-BP-001` revisi 5 beserta enam kontrak dan kedua roadmap. Rinciannya beserta batas wewenangnya di [blueprint-manifest.md](blueprint-manifest.md) bagian *Status approval*.

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| `ACC-PH-001`, `ACC-PH-002`, `ACC-PH-003` | `ACC-PH-004` | `ACC-PH-005` |

| Fase | Isi | Status |
|---|---|---|
| `ACC-PH-001` | Penutupan 37 keputusan bisnis | `DONE` — 1 September 2026, bukti pada decision log |
| `ACC-PH-002` | Penyusunan blueprint target: arsitektur, ERD, enam kontrak, PRD ke MVP | `DONE` — 1 September 2026, 15 artefak canonical |
| `ACC-PH-003` | Roadmap delivery vertical slice | `DONE` — 1 September 2026. 14 task backend, 11 task frontend, traceability, dan evidence tersusun. Status `DRAFT_FORWARD_TEST` |
| `ACC-PH-004` | Pembuatan entity dan migration | `IN_PROGRESS` — `BE-ACC-001` dan `BE-ACC-002` `DONE`, `BE-ACC-003` `EXECUTION_READY`. Migration `BE-ACC-006` tetap tertahan Migration Coordination Gate dan `ACC-DEP-005` |
| `ACC-PH-005` | Implementasi backend dan frontend MVP | `BLOCKED` oleh `ACC-DEP-008` — endpoint tidak dapat menegakkan penyaringan badan hukum. Task frontend juga menunggu `ACC-FE-001` |
| `ACC-PH-006` | Phase 2: integrasi otomatis, jurnal berulang, tutup buku | `NOT_STARTED` — menunggu 9 pertanyaan `DEFERRED`, `ACC-XM-001`, dan dua gerbang skill |

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| `IN_PROGRESS` — 4/14 task | `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` |

| Task | Status | Bukti |
|---|---|---|
| `BE-ACC-001` kerangka modul, enum, test harness | **`DONE`** 1 September 2026 | `task/report/backend/be-acc-001-kerangka-modul-enum-dan-test-harness.md` |
| `BE-ACC-002` audit hak akses badan hukum | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-002-audit-hak-akses-badan-hukum.md` |
| `BE-ACC-003` entity daftar akun dan jenis jurnal | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-003-entity-daftar-akun-dan-jenis-jurnal.md` |
| `BE-ACC-004` entity periode akuntansi | **`DONE`** 2 September 2026 | `task/report/backend/be-acc-004-entity-periode-akuntansi.md` |
| `BE-ACC-005` entity jurnal, baris jurnal, riwayat persetujuan | `EXECUTION_READY` | Menunggu instruksi owner |

`BE-ACC-001` sampai `BE-ACC-003` sudah **di-commit** pada `e1ee173`. `BE-ACC-004` **belum
di-commit**.

**Tiga entity persisted sudah berdiri** — `AccChartOfAccount`, `AccJournalType`, dan
`AccAccountingPeriod` — tanpa satu pun migration. Model EF Core karena itu mendahului
`ApplicationDbContextModelSnapshot.cs`. Ini disengaja: `BE-ACC-006` yang menyelesaikannya, lewat
Migration Coordination Gate.

## Blockers and owners

| Blocker ID | Ringkasan | Owner | Fase terdampak | Kelanjutan yang tetap aman |
| --- | --- | --- | --- | --- |
| ~~`ACC-DEP-002`~~ | ~~Prefix belum terdaftar~~ | — | — | **CLOSED 1 September 2026.** `Acc` terdaftar, Lifecycle `PLANNED` |
| ~~`ACC-DEP-006`~~ | ~~Lifecycle `PLANNED`~~ | — | — | **CLOSED 1 September 2026** lewat `ACC-DEC-038`. Lifecycle `ACTIVE` |
| `ACC-DEP-007` | Checker QBE membaca path governance yang tidak ada; gerbang CI mati. **Akar dikoreksi 2 September 2026:** perbaikan `c9692d0` sudah pernah masuk, lalu dibatalkan merge `3d14cac` | Lead | Merge ke integration | Penulisan kode lokal tetap jalan. Perbaikannya lima baris; laporan di `evidence/03-acc-dep-007-governance-propagation.md` |
| `ACC-DEP-005` | Aturan koordinasi migration bersama (`QBE-MIG-001`/`002`) belum canonical | Lead | `BE-ACC-006` saja | Seluruh task lain. Gate tetap dijalankan memakai teks usulan di `06-shared-migration-coordination-rule.md` |
| `ACC-XM-001` | Siapa penerbit kejadian keuangan resmi — `CROSS_MODULE_DECISION_REQUIRED` | Owner Billing + Owner Finance/Yasmin + Rizki | `ACC-PH-006` Phase 2 | **Tidak memblokir MVP.** Bentuk batasnya sudah ditulis di `ACC-XMOD-0.1` |
| `ACC-DEP-008` | **Legal Entity Authorization Model Availability** — mekanismenya tidak ada. Dibuktikan `BE-ACC-002` pada 2 September 2026, bukan lagi dugaan. Status `OPEN` | **Security / Platform** | `BE-ACC-007` sampai `BE-ACC-014` | `BE-ACC-003` sampai `BE-ACC-005` tetap jalan. Bukti di `evidence/02-legal-entity-authority.md`; kartu dependency di `05-prerequisite-readiness.md` |
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
