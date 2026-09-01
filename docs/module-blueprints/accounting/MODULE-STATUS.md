# Accounting — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `ACC-BP-001` |
| Module name | `Accounting` |
| Revision | `5` |
| Module status | `IN_PROGRESS` |
| Current phase | `ACC-PH-003` |
| Last verified at | `1 September 2026` — **FINAL OWNER APPROVAL** revisi `5`, lifecycle `Acc` `ACTIVE` |
| Backend source SHA | `aa837d784ff51cb2b889cf975ada3a204018f1f5` (branch `rizkiG`) |
| Frontend source SHA | `31a82c8052a3c59445ae49e6f1ccce2bf717d6c0` (branch `QuilvianIntegrationFrontend`) |

Status `PARTIAL` berarti sebagian pekerjaan sudah aman dijalankan sementara sebagian lain masih
terblokir. Rinciannya: 37 keputusan bisnis sudah tertutup dan blueprint target sudah tersusun
lengkap. Pembuatan entity masih terhalang **satu** prasyarat, yaitu pendaftaran prefix
(`ACC-DEP-002`). Prasyarat snapshot EF (`ACC-DEP-001`) sudah selesai pada 30 Agustus 2026 dan
diverifikasi 1 September 2026.

**FINAL OWNER APPROVAL diberikan Rizki, 1 September 2026**, atas `ACC-BP-001` revisi 5 beserta enam kontrak dan kedua roadmap. Rinciannya beserta batas wewenangnya di [blueprint-manifest.md](blueprint-manifest.md) bagian *Status approval*.

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| `ACC-PH-001`, `ACC-PH-002`, `ACC-PH-003` | Menunggu approval owner | `ACC-PH-004`, `ACC-PH-005` |

| Fase | Isi | Status |
|---|---|---|
| `ACC-PH-001` | Penutupan 37 keputusan bisnis | `DONE` — 1 September 2026, bukti pada decision log |
| `ACC-PH-002` | Penyusunan blueprint target: arsitektur, ERD, enam kontrak, PRD ke MVP | `DONE` — 1 September 2026, 15 artefak canonical |
| `ACC-PH-003` | Roadmap delivery vertical slice | `DONE` — 1 September 2026. 14 task backend, 11 task frontend, traceability, dan evidence tersusun. Status `DRAFT_FORWARD_TEST` |
| `ACC-PH-004` | Pembuatan entity dan migration | `BLOCKED` oleh `ACC-DEP-002` saja. `ACC-DEP-001` selesai 30 Agustus 2026 |
| `ACC-PH-005` | Implementasi backend dan frontend MVP | `BLOCKED` — 2 task `READY` bila blueprint disetujui, sisanya berantai |
| `ACC-PH-006` | Phase 2: integrasi otomatis, jurnal berulang, tutup buku | `NOT_STARTED` — menunggu 9 pertanyaan `DEFERRED`, `ACC-XM-001`, dan dua gerbang skill |

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` |

## Blockers and owners

| Blocker ID | Ringkasan | Owner | Fase terdampak | Kelanjutan yang tetap aman |
| --- | --- | --- | --- | --- |
| ~~`ACC-DEP-002`~~ | ~~Prefix belum terdaftar~~ | — | — | **CLOSED 1 September 2026.** `Acc` terdaftar, Lifecycle `PLANNED` |
| ~~`ACC-DEP-006`~~ | ~~Lifecycle `PLANNED`~~ | — | — | **CLOSED 1 September 2026** lewat `ACC-DEC-038`. Lifecycle `ACTIVE` |
| `ACC-DEP-007` | Governance yang dibaca checker hilang dari repo backend; gerbang CI QBE mati | Lead | Merge ke integration | Penulisan kode lokal tetap jalan |
| `ACC-DEP-005` | Aturan koordinasi migration bersama (`QBE-MIG-001`/`002`) belum canonical | Lead | `BE-ACC-006` saja | Seluruh task lain. Gate tetap dijalankan memakai teks usulan di `06-shared-migration-coordination-rule.md` |
| `ACC-XM-001` | Siapa penerbit kejadian keuangan resmi — `CROSS_MODULE_DECISION_REQUIRED` | Owner Billing + Owner Finance/Yasmin + Rizki | `ACC-PH-006` Phase 2 | **Tidak memblokir MVP.** Bentuk batasnya sudah ditulis di `ACC-XMOD-0.1` |
| Hak atas badan hukum | Bagaimana hak `LegalEntityId` diberikan kepada pengguna belum jelas | Owner keamanan platform | `ACC-PH-005` | Perancangan selesai; penegakannya yang perlu dipastikan |
| `ACC-FE-001`, `ACC-FE-003` | Letak menu dan bentuk layar rincian jurnal | Product owner | Task frontend | Seluruh task backend tidak terpengaruh |
| `ACC-XM-001` | Siapa menerbitkan kejadian keuangan resmi | Owner Billing, owner Finance, Rizki | `ACC-PH-006` Phase 2 | **Tidak memblokir MVP** — rilis pertama tanpa jurnal otomatis |

Diperiksa ulang 1 September 2026: `ACC-DEP-002` **masih menutup**. Registry di
`origin/QuilvianIntegrationBackend` memuat nol baris `Acc` dan masih hanya berisi
`| Finance | Finance | BUSINESS DOMAIN | Fin | ACTIVE |`. Berkas registry itu sendiri juga belum
ada di `rizkiG`. Backend pun masih bersih: nol folder `*accounting*` dan nol entity `class Acc*`,
sehingga delivery state `NOT_STARTED` akurat.

## Stale evidence

| Artefak/bukti | SHA tercatat | SHA terkini | Impact review yang dibutuhkan |
| --- | --- | --- | --- |
| Tidak ada | `aa837d7` / `31a82c8` | sama | — |

Baseline frontend di-rebase pada 1 September 2026 dari `fc49cc7` (`RizkiV2`) ke `31a82c8`
(`QuilvianIntegrationFrontend`) setelah impact scan. Drift-nya 30 commit dan 161 berkas, tetapi
**seluruhnya di `health-services`** sementara Accounting berada di `Corporate`; empat dari enam
anchor reuse tidak berubah dan dua sisanya hanya bertambah. Nol artefak perlu direvisi, sehingga
`revision` tetap `3`. Buktinya di
[evidence/02-frontend-rebaseline-impact-scan.md](evidence/02-frontend-rebaseline-impact-scan.md).

`RizkiV2` terkandung penuh di dalam `QuilvianIntegrationFrontend` (0 ahead, 26 behind), jadi
perpindahan ini fast-forward murni — tidak ada pekerjaan yang hilang.

## Next recommended task

Tiga hal, dapat berjalan paralel:

1. **Ajukan pendaftaran prefix `Acc`** kepada pemilik registry di `QuilvianIntegrationBackend`.
   Ia satu-satunya yang memblokir gelombang `MVP-0`. Baris yang diajukan ada di
   `evidence/01-design-verification-evidence.md` bagian `EV-ACC-005`.
2. **Mintakan approval owner** atas blueprint, enam kontrak, dan roadmap.
3. **Putuskan `ACC-FE-001`** letak menu Accounting. Keputusan ini murah tetapi menahan seluruh
   rantai task frontend, jadi sebaiknya diambil lebih dahulu. Sejak 1 September 2026 tersedia dua
   preseden segar di `src/utils/menu-sidebar/menu-items.jsx` — seksi "Rekam Medis" dan "Operasi" —
   yang dapat dipakai sebagai contoh bentuk.

Setelah approval turun, dua task backend sudah dapat dikerjakan tanpa menunggu prefix:
`BE-ACC-001` kerangka modul dan enum, serta `BE-ACC-002` audit hak akses badan hukum. Keduanya
sengaja dirancang agar tidak menyentuh `Models/`, sehingga tidak tertahan `ACC-DEP-002`.

Roadmap sudah tersusun; **tidak perlu** menjalankan `/plan-module-delivery` lagi. Pertimbangkan
`/trace-existing-capabilities` bila capability map yang masih parsial ingin dilengkapi.

## Optional deterministic delivery progress

**0 dari 0 task yang disetujui.** Roadmap sudah memuat 25 task — 14 backend dan 11 frontend —
tetapi belum satu pun disetujui untuk dikerjakan, sehingga denominatornya masih nol. Persentase
baru bermakna setelah approval turun.

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
