# Accounting — Prerequisite Readiness

Berkas ini mencatat prasyarat yang harus beres sebelum bagian tertentu dari modul Accounting
boleh dikerjakan. Satu baris untuk satu prasyarat yang material.

Nilai `capability_status` hanya boleh salah satu dari taksonomi berikut: `READY TO REUSE`,
`REUSE WITH ADAPTER`, `EXTEND`, `REPAIR`, `MISSING`, `CONFLICT`, atau `UNKNOWN`.

| dependency_id | capability_or_module | dependency_type | owner | evidence | capability_status | required_by | blocking_impact | independent_continuation | source_sha | next_owner_or_action |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `ACC-DEP-001` | Snapshot model EF Core bersama (`Migrations/ApplicationDbContextModelSnapshot.cs`) | `MODULE_FOUNDATION` | Owner Billing, owner Operating Room, lead | `NewQuilvianSystemBackend/Migrations/ApplicationDbContextModelSnapshot.cs@aa837d7` | **`READY TO REUSE`** | — | **Tidak lagi memblokir.** Snapshot sudah pulih dan sama dengan integration | Seluruh fase bebas berjalan | `aa837d7` | Selesai. Tetap hitung operasi pada migration Accounting pertama sebagai pemeriksaan akhir |
| `ACC-DEP-002` | Registry prefix kepemilikan modul (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) | `EXTERNAL` | Rizki (pemilik modul) | `QuilvianEngineeringSkills/agents/rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md@48279dd` + salinan `Claude/.claude/rules/backend/engineering/` | **`READY TO REUSE`** | Pembuatan entity pertama | **Tidak lagi memblokir penamaan.** Baris `Acc` terdaftar 1 September 2026 | Penamaan `Acc*` kini sah dipakai di kode | `48279dd` | **CLOSED untuk naming/prefix.** Aktivasi lifecycle dilacak `ACC-DEP-006` |
| `ACC-DEP-006` | Lifecycle registry `Acc` | `EXTERNAL` | Rizki (pemilik modul) | Baris registry kini `| Corporate | AccountingManagement / Accounting | BUSINESS DOMAIN / MODULE | Acc | ACTIVE |`; simulasi resolusi checker 1 Sep 2026 sesudah aktivasi: `Category=True, Prefix=True, Lifecycle=True` → `RESOLVED` | **`READY TO REUSE`** | — | **Tidak lagi memblokir.** Entity `Acc*` sah dibuat | Seluruh task entity terbuka | `48279dd` | **CLOSED** 1 September 2026 lewat `ACC-DEC-038`. Wewenang source model saja |
| `ACC-DEP-007` | Governance canonical yang dibaca checker hilang dari repo backend | `MODULE_FOUNDATION` | Lead | Perbaikan `c9692d0` (PR #63, 31 Agu) memulihkan governance ke `docs/engineering/` **dan** mengarahkan checker ke sana; merge `3d14cac` (1 Sep) membatalkan bagian checker-nya, lalu PR #68 `fe88b1d` membawa pembatalan itu ke integration. Laporan lengkap: `evidence/03-acc-dep-007-governance-propagation.md` | `CONFLICT` | Gerbang CI QBE | **Checker tidak dapat jalan sama sekali** — `TOOL ERROR: Canonical governance missing`, exit 2, diverifikasi ulang 2 September 2026. Merge ke integration tidak terjaga | Pekerjaan lokal tetap jalan; kepatuhan diverifikasi manual terhadap registry suite | `ca6b7e0` | **Laporkan ke lead.** Perbaikannya = terapkan ulang 5 baris path pada checker dari `c9692d0`. Jangan diperbaiki sendiri — tooling QBE milik lead |
| `ACC-DEP-008` | **Legal Entity Authorization Model Availability** | `EXTERNAL` | **Security / Platform** | `Models/SysAccessPolicy.cs` tanpa dimensi badan hukum; `ApplicationUser`/`ApplicationUserOrganization` tanpa `LegalEntityId`; nol klaim `legal_entity` pada `AuthController.cs`; nol `HasQueryFilter`; `LegalEntityId` diambil dari `[FromQuery]`/`request`. Bukti lengkap: `evidence/02-legal-entity-authority.md` | **`MISSING`** | `BE-ACC-007` sampai `BE-ACC-014` | Penyaringan badan hukum tidak dapat ditegakkan. Aturan kedua `ACC-PERMISSION-0.1` bagian 5 belum punya dasar teknis | `BE-ACC-003`, `BE-ACC-004`, `BE-ACC-005` tetap jalan — menyimpan kolom berbeda dari menegakkannya | `ca6b7e0` | Security/Platform menentukan modelnya. Accounting **tidak** membuat solusi baru |
| `ACC-DEP-003` | Kontrak integrasi Billing (`BIL-INTEGRATION-0.4`) | `INTEGRATION` | Owner Billing, owner Finance | `NewQuilvianSystemBackend/docs/module-blueprints/billing-kasir/contracts/integration-contract.md@aa837d7` | `CONFLICT` | `ACC-XM-001`, Phase 2 saja | Kontrak Billing yang sudah disetujui mengarahkan akibat keuangan ke AR/AP, bukan ke Accounting | Rancangan internal Accounting tetap boleh berjalan memakai kontrak sementara | `aa837d7` | Keputusan lintas modul antara owner Billing, owner Finance, dan Rizki |
| `ACC-DEP-005` | Aturan koordinasi migration bersama (`QBE-MIG-001`, `QBE-MIG-002`) | `EXTERNAL` | Lead | Usulan lengkap di `06-shared-migration-coordination-rule.md`; rumah canonical `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md@origin/QuilvianIntegrationBackend` | `MISSING` | `BE-ACC-006` | Accounting dan Finance dapat menghasilkan migration final paralel dari snapshot yang sama, sehingga salah satu snapshot menjadi usang tanpa terlihat | Seluruh task selain `BE-ACC-006`. Gate tetap dijalankan memakai teks usulan | `aa837d7` | Lead mendaftarkan `QBE-MIG-001`/`002` ke kontrak engineering canonical |
| `ACC-DEP-004` | Modul Finance sebagai penerbit event keuangan | `EXTERNAL` | Developer Finance (Yasmin) | Belum ada kode Finance di `NewQuilvianSystemBackend@aa837d7`; blueprint Finance akan berdiri di `docs/module-blueprints/finance/` | `MISSING` | Phase 2 saja | Integrasi nyata belum bisa diuji ujung ke ujung | Pengujian memakai contoh data tiruan tetap sah selama tidak diklaim sebagai integrasi produksi | `aa837d7` | Sesuai `ACC-DEC-007`, Accounting jalan paralel dengan kontrak sementara |

## Mana yang sebenarnya memblokir MVP

Dari empat prasyarat di atas, **tinggal satu yang memblokir rilis pertama**. Ini penting supaya
tidak ada yang menyangka modul ini tersandera seluruhnya.

| Prasyarat | Memblokir MVP? | Yang terhalang |
|---|:---:|---|
| `ACC-DEP-001` snapshot EF | **Tidak lagi** | Selesai 30 Agustus 2026, diverifikasi 1 September 2026 |
| `ACC-DEP-002` prefix entity | **Tidak lagi** | Terdaftar 1 September 2026. Prefix `Acc` sah dipakai |
| `ACC-DEP-006` lifecycle | **Tidak lagi** | `ACTIVE` sejak 1 September 2026 (`ACC-DEC-038`) |
| `ACC-DEP-007` governance checker hilang | **Ya, untuk merge** | Gerbang CI mati; bukan penghalang penulisan kode |
| `ACC-DEP-008` hak akses badan hukum | **Ya, untuk endpoint** | `BE-ACC-007` ke atas. Entity `BE-ACC-003`..`005` tetap bebas |
| `ACC-DEP-003` kontrak Billing | Tidak | Hanya Phase 2. MVP tidak punya jurnal otomatis sama sekali |
| `ACC-DEP-004` modul Finance | Tidak | Hanya Phase 2. Developer sudah ditunjuk: Yasmin |
| `ACC-DEP-005` aturan koordinasi migration | **Sebagian** | Hanya `BE-ACC-006`, lewat gate-nya. Task lain bebas

Dua yang memblokir, `ACC-DEP-007` dan `ACC-DEP-008`, keduanya berada **di luar wewenang owner
modul**: yang pertama milik lead, yang kedua milik owner keamanan platform. Sementara menunggu,
seluruh perancangan sudah selesai dan tidak terpengaruh: arsitektur, ERD, kamus data, enam
kontrak, dan dokumen PRD ke MVP semuanya sudah berdiri.

### `ACC-DEP-008` — kartu dependency

Dicatat resmi pada 2 September 2026 atas persetujuan owner, bersamaan dengan kenaikan blueprint
ke revisi 6.

| Field | Isi |
|---|---|
| **ID** | `ACC-DEP-008` |
| **Title** | Legal Entity Authorization Model Availability |
| **Owner** | Security / Platform |
| **Impact** | `BE-ACC-007` sampai `BE-ACC-014` |
| **Status** | **`OPEN`** |
| **Ditemukan oleh** | `BE-ACC-002`, audit read-only, 2 September 2026 |
| **Bukti** | [evidence/02-legal-entity-authority.md](evidence/02-legal-entity-authority.md) |

**Resolution — Security/Platform harus menentukan model untuk kelima lapis berikut:**

| # | Lapis | Pertanyaan yang harus dijawab |
|---:|---|---|
| 1 | `user → legal entity assignment` | Di mana hubungan pengguna dengan badan hukum disimpan? Sekarang tidak ada tempatnya |
| 2 | `token claim` | Klaim apa yang membawanya? Sekarang token memuat 20 klaim, nol tentang badan hukum |
| 3 | `authorization filter` | Bagaimana permintaan atas badan hukum yang bukan hak pengguna ditolak? |
| 4 | `query enforcement` | Bagaimana kueri disaring ke hak pengguna, termasuk saat parameter tidak dikirim? |
| 5 | `EF/data access enforcement` | Apakah penegakan dipasang di lapisan data? Sekarang nol `HasQueryFilter` |

**Accounting tidak membuat solusi baru untuk kelimanya.** Accounting hanya mengikuti model yang
nanti ditetapkan Security/Platform.

Yang **tetap boleh** dikerjakan Accounting sementara ini: mendefinisikan kolom `LegalEntityId`
pada entity-nya sendiri. Ini ditegaskan owner saat menerima hasil `BE-ACC-002`, dan menjadi
alasan `BE-ACC-003` sampai `BE-ACC-005` tidak ikut tertahan.

### Kenapa ini bukan sesuatu yang boleh dibuat Accounting

`BE-ACC-002` memeriksa cara sistem menentukan badan hukum mana yang menjadi hak seorang pengguna.
Hasilnya: **caranya tidak ada.** Bukan tersembunyi, bukan berbentuk lain — memang tidak ada.

`LegalEntityId` memang dipakai 228 berkas, tetapi selalu sebagai **penyaring yang dikirim
pengguna**, bukan sebagai batas kewenangan. Siapa pun yang punya hak baca sebuah menu dapat
mengirim badan hukum mana pun, atau tidak mengirimnya sama sekali dan mendapat semuanya.

Godaannya adalah membuat penyaringan sendiri di dalam Accounting. Itu **tidak boleh**, karena
menghasilkan cara kedua yang berbeda dari cara platform nanti, dan menutupi persoalan yang
sebenarnya berlaku pada seluruh 40 model bermuatan `LegalEntityId` — sebagian besar milik Human
Resource, termasuk struktur gaji dan klaim biaya.

### `ACC-DEP-005` — kenapa migration paralel berbahaya

Entity Framework membandingkan model dengan satu berkas "foto" bersama sebelum membuat migration.
Bila Accounting dan Finance sama-sama membuat migration final dari foto yang sama, foto milik
salah satunya langsung menjadi usang. Migration berikutnya akan menyimpulkan tabel modul lain
"belum ada" dan ikut membawanya — persis kerusakan yang dulu tercatat sebagai `ACC-DEP-001`.

Yang diserialisasi hanya migration finalnya. **Coding kedua modul tetap boleh paralel**, dan
aturan ini tidak berlaku surut terhadap pekerjaan Finance yang sudah berjalan.

### `ACC-DEP-007` — tiga lapis cacat platform, bukan cacat Accounting

Diklasifikasikan **PLATFORM / ENGINEERING GOVERNANCE defect**. Tidak satu pun bagiannya berasal
dari Accounting, dan tidak satu pun boleh diperbaiki dari sisi Accounting.

> **Dikoreksi 2 September 2026.** Catatan lapis 1 di bawah sebelumnya berhenti pada `4db8909`.
> Ternyata ada peristiwa sesudahnya yang belum tercatat: perbaikannya **sudah pernah masuk** lewat
> PR #63 (`c9692d0`, 31 Agustus), lalu **dibatalkan** merge `3d14cac` (1 September) dan dibawa
> kembali ke integration oleh PR #68. Akar hari ini adalah pembatalan itu, bukan penghapusan
> `4db8909`. Ini mengubah usulan perbaikannya menjadi jauh lebih murah — cukup terapkan ulang lima
> baris path pada checker. Uraian lengkap beserta bukti tiga-versi git-nya ada di
> [evidence/03-acc-dep-007-governance-propagation.md](evidence/03-acc-dep-007-governance-propagation.md).

| Lapis | Keadaan | Bukti |
|---|---|---|
| 1. Checker | Membaca `agents/rules/engineering/` yang tidak ada di branch mana pun | `c9692d0` sudah mengarahkannya ke `docs/engineering/`; merge `3d14cac` mengembalikannya ke versi lama padahal git tidak melihat konflik |
| 2. Governance | Dipusatkan ke suite skill `rules/backend/engineering/` | `build-module-backend/SKILL.md`; `AGENTS.md` baris 53 |
| 3. `AGENTS.md` | **Kontradiktif** — baris 11/17–26 masih menunjuk `docs/engineering/` dan menyatakan `agents/rules/` ada di repo, sedangkan baris 40/53/60 menyatakan sebaliknya | `AGENTS.md@aa837d7` |

Lapis 3 baru: `docs/engineering/` yang dirujuk baris 11 dan 20 **tidak ada di `rizkiG`**, tetapi
**ada lengkap di `origin/QuilvianIntegrationBackend`** — tiga berkasnya utuh di sana sejak
`c9692d0`. Jadi `AGENTS.md` memuat dua model governance sekaligus, dan model lamanya menunjuk
path yang keberadaannya berbeda-beda tergantung branch.

Isi salinan backend itu **tidak menyimpang**: 48 baris tabelnya identik dengan registry suite,
dan selisihnya tepat empat baris yang seluruhnya tentang Accounting. Klasifikasinya
**`synced consumer` yang tertinggal satu pendaftaran** — bukan salinan legacy, bukan hasil
generate. Yang belum selesai adalah cara menyalurkannya, bukan isinya.

**Yang dilarang sebagai jalan pintas:** menyalin governance dari suite skill kembali ke repo
backend supaya checker hijau. Itu menciptakan governance canonical kedua dan membalikkan
pemusatan yang sudah diputuskan. Perbaikan yang benar ada di sisi checker, dan itu wewenang lead.

Nilai `dependency_type` yang diizinkan adalah `MODULE_FOUNDATION`, `PHASE`, `INTEGRATION`,
dan `EXTERNAL`. Prasyarat yang terblokir hanya memblokir fase yang membutuhkannya; fase lain
yang aman tetap boleh berjalan.

## Penjelasan untuk pembaca non-teknis

### `ACC-DEP-001` — SELESAI, dan kenapa dulu memblokir

Entity Framework menyimpan "foto" seluruh struktur database dalam satu berkas bersama yang
dipakai **semua** modul. Foto itu saat ini tidak lengkap: sejumlah definisi tabel milik modul
lain hilang saat penggabungan cabang.

> **Status per 1 September 2026: SELESAI.** Snapshot `aa837d7` berisi 530 blok dengan 28 `Bil`,
> identik dengan `origin/QuilvianIntegrationBackend@c081939`. Perbaikannya masuk lewat migration
> `20260828063909_RepairCanonicalEfModelBaseline` dan `20260830151340_RepairPostCanonicalIntegration`.
> Penjelasan di bawah dipertahankan sebagai catatan sejarah, dan sebagai alasan mengapa
> pemeriksaan hitung-operasi pada bagian 8 arsitektur backend tetap dijalankan.

**Contohnya begini.** Bayangkan foto itu seharusnya memuat 523 tabel, tetapi yang tersimpan
sekarang hanya 484. Ketika perintah pembuatan migration dijalankan, sistem membandingkan
model dengan foto tersebut, lalu menyimpulkan bahwa 39 tabel yang hilang "belum pernah ada"
dan perlu dibuat ulang. Padahal tabel itu sudah berdiri di database dan berisi data.

Akibatnya, migration Accounting yang seharusnya hanya menambah tabel akuntansi malah ikut
membawa ratusan perintah perubahan milik modul Billing dan Operating Room. Ini di luar
wilayah Accounting untuk diperbaiki sendiri, dan harus dilaporkan ke pemilik modul terkait
beserta lead.

### `ACC-DEP-002` — kenapa nama tabel belum boleh dikunci

Setiap modul punya awalan nama yang terdaftar resmi, misalnya `Bil` untuk Billing dan `Emg`
untuk IGD. Ada pemeriksa otomatis yang menolak penggabungan kode bila ada nama tabel baru
memakai awalan yang tidak terdaftar. Awalan untuk Accounting belum terdaftar, dan dokumen
registry-nya tidak tersimpan di repository ini.

Selama itu belum beres, nama seperti `AccJournal` hanya boleh dipakai sebagai nama sementara
di dokumen rancangan, bukan sebagai nama kelas di kode.

### `ACC-DEP-003` — kenapa arah aliran data dari Billing bermasalah

Modul Billing sudah punya kontrak integrasi yang disetujui pada 20 Agustus 2026. Di dalamnya,
akibat keuangan dari tagihan pasien dikirim ke Piutang (AR) dan Utang (AP), yang merupakan
wilayah Finance — **bukan** langsung ke Accounting.

Dokumen requirement Accounting masih menganggap arah ini terbuka. Dua dokumen ini harus
didamaikan lebih dulu. Kalau tidak, ada risiko satu tagihan tercatat dua kali di buku besar:
sekali karena Accounting berlangganan langsung ke Billing, sekali lagi karena Finance
meneruskan kejadian yang sama.
