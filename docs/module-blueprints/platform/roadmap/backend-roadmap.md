# Roadmap Delivery Backend — Modul Platform / Alokasi Nomor Bisnis

## Metadata

```yaml
module_id: platform
module_name: NumberSeriesManagement
entity_prefix: Num
blueprint_id: PLT-BP-001
blueprint_shape: SINGLE
blueprint_root: docs/module-blueprints/platform/
roadmap_revision: 1
status: DRAFT
approval_gate: BLUEPRINT_APPROVED
contract_version: v1 (approved)
backend_source_sha: f0d6855
backend_branch: sukmagp
frontend_source_sha: 101ec5d3a560bd6e54d4665ae53d425f255c609f
frontend_branch: sukmagpV2
decision_revision: 4
capability_map_revision: 3
owners:
  - "Pemilik kontrak engineering backend: Andry — mesin alokasi & INV-PLT-001..004"
  - "Pemilik registry engineering: baris registry Platform/Num"
approved_by:
  - "Sukma Giri Pratama (sukmagp) — blueprint PLT-SLICE-01 & set kontrak v1, 2026-09-09"
approved_at: "2026-09-09"
approval_note: >-
  Approval blueprint 2026-09-09 membuka penjadwalan task. Roadmap ini sendiri belum
  disetujui; approval roadmap adalah tindakan terpisah.
backend_source_sha_note: >-
  Rentang 4a1da7d..f0d6855 mengubah nol berkas .cs. Working tree memuat 17 berkas .cs
  milik BE-BD-005/BE-BD-011 yang belum ter-commit; nol di antaranya menyentuh kemampuan
  penomoran, sehingga bukti slice ini tidak terpengaruh.
```

---

## 0. Peringatan yang tidak boleh dilewati

**Roadmap ini tidak memberi wewenang menulis source.** Approval membuka **penjadwalan** task.
Wewenang menulis diberikan terpisah, satu task satu wewenang, lewat `build-module-backend`.

**Preflight QBE dan kesesuaian engineering diselesaikan pada waktu eksekusi** dari `AGENTS.md`
backend target dan dokumen engineering canonical — bukan di dokumen ini.

**Kedua gerbang sudah tertutup** per 9 September 2026 — `P0` approval blueprint, `P1` baris
registry. **`PLT-BE-001`, `PLT-BE-002`, dan `PLT-BE-003` seluruhnya selesai** pada hari yang sama,
sehingga gerbang `G4` Bank Darah **tertutup secara kemampuan**: providernya ada.

**Kelima task backend Platform selesai.** `PLT-BE-005` **selesai** 10 September 2026 — empat
endpoint baca berdiri, 22 kasus uji baru seluruhnya lulus. `PLT-BE-004` ✅ **selesai** 10 September
2026 — keenam uji durabilitas **lulus di PostgreSQL sungguhan** pada database pengembangan personal
`QuilvianNewDevSukma`, lewat opt-in `QUILVIAN_BILLING_TEST_DB_ALLOW_PERSONAL` yang diputuskan
`RJ-BIL-DEC-019`. **Riwayat:** sebelumnya 🟡 **sebagian** karena role database ditolak
`42501: permission denied to create database`; hak `CREATEDB` itu kini **tidak lagi diperlukan**,
karena pemilik memilih tidak membuat database test baru.

⚠️ **Empat endpoint `PLT-BE-005` belum dapat dipanggil.** Migration
`20260909070218_AddNumNumberSeries` **belum dijalankan**, sehingga tabel `NumNumberSeries` belum
ada di lingkungan mana pun. Kodenya berdiri dan terbukti; yang belum ada adalah tabelnya.
Menjalankan migration adalah **wewenang terpisah**.

✅ **`AC-PLT-003`, `AC-PLT-004`, `AC-PLT-005`, dan `AC-PLT-012` terbukti 10 September 2026** di
PostgreSQL sungguhan — `PLT-BE-004`, **6 lulus dari 6**. Klaim durabilitas yang dulu menahan
penjadwalan task Bank Darah **sudah diperiksa**, sehingga syarat urutan aman pada bagian 6.1
terpenuhi. **Riwayat:** sampai percobaan kedua 10 September 2026 keempatnya belum pernah dijalankan,
karena role database belum berhak membuat database test.

**Migration, eksekusi database, deployment, dan publikasi Git tetap wewenang tersendiri** yang
diminta per tindakan.

---

## 1. Cara membaca roadmap ini

| Penanda | Arti | Boleh dijadwalkan? |
| --- | --- | --- |
| ✅ | **SELESAI** — bukti tercatat di `task/report/backend/` | Sudah selesai |
| 🟡 | **PENDING** — seluruh prasyaratnya terpenuhi | **Ya** |
| ⛔ | **BLOCKED** — ada prasyarat yang belum tersedia | **Tidak** |

---

## 2. Gerbang

| Gate | Isi | Pemilik | Keadaan |
| --- | --- | --- | --- |
| `P0` | Approval blueprint `PLT-SLICE-01` & set kontrak `v1` | Pemilik kebutuhan | ✅ **TERTUTUP** 2026-09-09 oleh `Sukma Giri Pratama` |
| `P1` | Baris registry `Platform` / `Num` dicatat dan `ACTIVE` | Pemilik registry engineering | ✅ **TERTUTUP** 2026-09-09 — `OQ-PLT-014` tertutup |

**Nol gerbang tersisa.** Kedua gerbang modul ini sudah tertutup.

### 2.1 `P1` — bagaimana ditutup, dan dua hal yang ditemukan saat menutupnya

Baris yang **benar-benar dicatat** pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`:

| Kolom | Nilai |
| --- | --- |
| Area | `Platform` |
| Module/pemilik | `NumberSeriesManagement / Number Series` |
| Category | **`BUSINESS DOMAIN / SHARED PLATFORM CAPABILITY`** |
| Prefix | `Num` |
| Lifecycle | **`ACTIVE`** |

**Terbukti diterima checker, bukan diyakini.** `Resolve-RegistryOwnership` atas
`Areas/Platform/NumberSeriesManagement/Models/NumNumberSeries.cs` memulangkan
`Resolved: True`.

**Dua penyimpangan dari usulan desain semula, keduanya kebutuhan mekanis — bukan perubahan
keputusan:**

| Yang diusulkan desain | Yang dicatat | Sebab, diverifikasi langsung |
| --- | --- | --- |
| `Category: SHARED PLATFORM CAPABILITY` | `BUSINESS DOMAIN / SHARED PLATFORM CAPABILITY` | Baris 271 checker menguji `-match '^businessdomain'`. Disimulasikan: tanpa awalan → token `sharedplatformcapability` → **ditolak**. `NumNumberSeries` akan terblokir `QBE-MOD-002` walaupun barisnya ada. Mengikuti perbaikan `Mst` 2026-09-04 yang identik masalahnya |
| `Lifecycle: PLANNED` lalu `ACTIVE` | `ACTIVE` langsung | Checker hanya menerima token `active` dan `legacy`; `PLANNED` disimulasikan dan **ditolak**. Diminta eksplisit pemilik |

Label semantik `SHARED PLATFORM CAPABILITY` dipertahankan utuh di dalam nilai Category; prefix,
pemilik, dan scope modul **tidak berubah**. Dicatat sebagai `FACT-PLT-013` dan `FACT-PLT-014`.

**Temuan lokasi registry — `FACT-PLT-012`.** Registry yang **ditegakkan** ternyata ada di dalam
repository backend (`docs/engineering/`), bukan di suite skill: `Invoke-QbeConformanceCheck.ps1`
baris 200 membacanya dari sana, dan itulah tempat `Bbk` didaftarkan lewat commit `ed7fba8`.
Salinan pada plugin cache terpasang **tertinggal satu entri**. Ketiga salinan perlu disinkronkan
pemilik registry — di luar scope slice ini.

---

## 3. Ringkasan status

| Penanda | Jumlah | Task |
| --- | ---: | --- |
| ✅ SELESAI | 5 | `PLT-BE-001`, `PLT-BE-002`, `PLT-BE-003`, `PLT-BE-004`, `PLT-BE-005` |
| 🟡 SELESAI SEBAGIAN | 0 | — `PLT-BE-004` naik ke ✅ 10 September 2026; sebelumnya 🟡, tertahan hak `CREATEDB` |
| 🟡 PENDING | 0 | — |
| ⛔ BLOCKED | 0 | — |
| **Total** | **5** | |

---

## 4. Urutan dependency

```text
✅ PLT-BE-001 (baris registry Platform/Num, ACTIVE)   SELESAI 9 September 2026
       │      dep: P0 ✅ · gerbang P1 ✅ TERTUTUP
       │      BUKAN task source — tindakan registry, terbukti Resolved: True
       │
       └── ✅ PLT-BE-002 (tabel NumNumberSeries + migration)   SELESAI 9 September 2026
                  │      dep: PLT-BE-001 ✅ · migration dibuat, belum dijalankan
                  │
                  └── ✅ PLT-BE-003 (NumberSeriesAllocator — mesin durabel)   SELESAI 9 September 2026
                             │      dep: PLT-BE-002 ✅ · INTI SLICE INI
                             │      └──> G4 Bank Darah TERTUTUP secara kemampuan
                             │
                             ├── ✅ PLT-BE-004 (uji integrasi PostgreSQL)   SELESAI 10 September 2026
                             │          dep: PLT-BE-003 ✅ · menuntut PostgreSQL berjalan
                             │          6 dari 6 LULUS di QuilvianNewDevSukma (RJ-BIL-DEC-019)
                             │          AC-PLT-003/004/005/012 terbukti; hak CREATEDB tak lagi perlu
                             │
                             └── ✅ PLT-BE-005 (layar pemantauan — SHOULD HAVE)   SELESAI 10 September 2026
                                        dep: PLT-BE-003 ✅ · gelombang MVP-2
                                        4 endpoint baca; 22 kasus uji baru, seluruhnya lulus
                                        BELUM dapat dipanggil — migration belum dijalankan
```

**Rantai ini lurus dan tidak dapat dipotong.** Tabel tidak dapat dibuat sebelum prefixnya sah,
alokator tidak dapat menulis ke tabel yang belum ada, dan durabilitasnya tidak dapat dibuktikan
sebelum alokatornya ada.

**`G4` Bank Darah tertutup pada `PLT-BE-003`**, bukan pada `PLT-BE-004`. Uji integrasi membuktikan
perilakunya, tetapi kemampuannya sudah ada begitu alokator berdiri. Meski begitu, **menjadwalkan
task Bank Darah sebelum `PLT-BE-004` lulus adalah risiko yang disadari** — lihat bagian 6.

---

## 5. Task

### ✅ `PLT-BE-001` — Baris registry `Platform` / `Num` dicatat dan diaktifkan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI** 9 September 2026. Baris registry dicatat pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` beserta kepanjangan prefix dan catatan lifecycle. **Terbukti diterima checker:** `Resolve-RegistryOwnership` atas `NumNumberSeries` memulangkan `Resolved: True`. Dua penyimpangan mekanis dari usulan desain dicatat di bagian 2.1 |
| **⚠️ Bukan task source** | Ini **tindakan registry**, bukan implementasi. `build-module-backend` **tidak** dipakai. **Koreksi terhadap perkiraan awal:** berkasnya ternyata **di dalam** repository backend — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — bukan di suite skill. Lihat `FACT-PLT-012` |
| **Outcome** | Modul Platform punya kepemilikan dan prefix yang sah, sehingga entity `Num*` boleh dibuat |
| **Trace** | `DEC-PLT-009`, `DEC-PLT-010`, `OQ-PLT-014` · `QBE-MOD-002`, `QBE-MOD-003`, `QBE-NAM-004` |
| **Kontrak** | `02-backend-architecture.md` §A.3 — baris siap-salin |
| **Scope** | Satu baris tabel kepemilikan + satu baris kepanjangan prefix + satu catatan perubahan lifecycle. Lifecycle **langsung `ACTIVE`**, melewati `PLANNED` — `PLANNED` ditolak checker |
| **Dependency** | `P0` ✅ |
| **Acceptance** | ✅ Baris ada di registry dengan Category `BUSINESS DOMAIN / SHARED PLATFORM CAPABILITY` dan Lifecycle `ACTIVE`; `Num` = *Number Series* tercatat di tabel kepanjangan |
| **Verification** | ✅ **Dijalankan.** `Resolve-RegistryOwnership 'Areas/Platform/NumberSeriesManagement/Models/NumNumberSeries.cs' 'NumNumberSeries'` → `Resolved: True`, Area `Platform`, prefix `Num`, lifecycle `ACTIVE`. Kebalikannya juga disimulasikan: Category tanpa awalan `BUSINESS DOMAIN` → **ditolak**; lifecycle `PLANNED` → **ditolak** |
| **Risk/owner** | Rendah / pemilik registry engineering |
| **DoD** | ✅ Gerbang `P1` tertutup; `OQ-PLT-014` ditandai tertutup pada decision log; dua penyimpangan mekanis dicatat sebagai `FACT-PLT-013`/`FACT-PLT-014`; temuan lokasi registry sebagai `FACT-PLT-012` |

---

### ✅ `PLT-BE-002` — Tabel pencacah deret berdiri

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI** 9 September 2026. Bukti: [laporan](../task/report/backend/PLT-BE-002.md). 1 entity + 1 configuration + 1 konstanta + 1 `DbSet`; `dotnet build` solution `0 Error(s)` dan **210 warning — sama persis baseline**; `UnitTests.Sqlite` **187 lulus** (naik dari 177, 10 uji baru); `QBE-MOD-002` dibuktikan `Resolved: True`. **Migration dibuat, belum dijalankan** |
| **Kenapa kini terbuka** | Baris registry `Platform`/`Num` sudah ada dan `ACTIVE`, dan resolusinya sudah **dibuktikan** terhadap checker — bukan diasumsikan |
| **Outcome** | Ada satu tempat menyimpan pencacah deret nomor yang dijaga unik di tingkat database |
| **Trace** | `FR-PLT-008`..`FR-PLT-011` · `DEC-PLT-002`, `INV-PLT-001`, `INV-PLT-002` |
| **Kontrak** | `data/data-dictionary.md` §`NumNumberSeries`; `02-backend-architecture.md` §D.1, §F |
| **Scope** | `NumNumberSeries` + `NumNumberSeriesConfiguration` + `DbSet` + `NumberSeriesResetPolicies` + **satu migration** |
| **Dependency** | `PLT-BE-001` ✅ |
| **Acceptance** | ✅ `AC-PLT-011` **terbukti** — penyisipan kembar melempar `DbUpdateException` dan hitungan baris tetap 1; kedua check constraint terbukti menolak pencacah nol dan kebijakan asing |
| **Verification** | ✅ **Dijalankan.** `UnitTests.Sqlite` 187 lulus; migration `Up()` hanya 1 `CreateTable` + 1 `CreateIndex`; snapshot **323 tambahan, 0 penghapusan** |
| **Risk/owner** | Rendah / `Andry` |
| **DoD** | ✅ Migration dibuat, **belum dijalankan** — dinyatakan apa adanya. Nol baris di-seed, dibuktikan `TabelLahirKosong_NolBarisDiSeed` |

---

### ✅ `PLT-BE-003` — Alokator nomor yang pencacahnya bertahan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI** 9 September 2026. Bukti: [laporan](../task/report/backend/PLT-BE-003.md). `dotnet build` solution `0 Error(s)`, **210 warning — sama persis baseline**; `UnitTests.Sqlite` **209 lulus** (naik dari 187 — 19 uji alokator + 3 penjaga komposisi); `QBE-CODE-002`/`003` dipindai **bersih**. Ketujuh acceptance criteria task ini terbukti. **Kompatibilitas `AddDbContextFactory` diverifikasi, nol konflik** |
| **Outcome** | Modul mana pun dapat meminta satu nomor bisnis yang unik, permanen, dan **tidak pernah terbit dua kali** — walau pekerjaan yang memintanya kemudian dibatalkan |
| **Trace** | `FR-PLT-001`..`FR-PLT-007` · `DEC-PLT-007`, `DEC-PLT-008`, `CONF-PLT-002` · `INV-PLT-001`..`004` |
| **Kontrak** | `contracts/integration-contract.md` §1 (tanda tangan method); `contracts/validation-matrix.md` (`VAL-PLT-001`..`007`); `02-backend-architecture.md` §B |
| **Reuse** | Algoritma diambil utuh dari `AllocateNumberAsync` pada `BillingNumberSeriesService` — kunci penasihat, kunci deret, kunci scope, perakitan `prefix-scope-urutan` |
| **Scope** | `NumberSeriesAllocator` + `NumberAllocationRequest` + pendaftaran DI. **Perubahan satu-satunya terhadap mesin asal:** pencacah di-`commit` pada transaksi dan koneksi tersendiri lewat `IDbContextFactory` |
| **Dependency** | `PLT-BE-002` ✅ |
| **Acceptance** | ✅ Ketujuhnya **terbukti** — `AC-PLT-001`, `AC-PLT-002`, `AC-PLT-006`..`AC-PLT-010` |
| **Verification** | ✅ **Dijalankan.** 19 uji alokator + 3 uji komposisi lulus. **Durabilitas dan antrean tetap belum terbukti di mana pun** — keduanya memang milik `PLT-BE-004`, dan mustahil di luar PostgreSQL |
| **Risk/owner** | **Tinggi / `Andry`.** Menyentuh komposisi aplikasi lewat `AddDbContextFactory` yang berdampingan dengan `AddDbContext` existing |
| **⚠️ Yang wajib dicek builder** | **SUDAH DIPERIKSA — nol konflik.** Diverifikasi sebelum satu baris alokator ditulis, lewat `NumberSeriesCompositionTests` (3 lulus, `validateScopes` aktif). Satu keputusan teknis menyertainya: factory didaftarkan **`ServiceLifetime.Scoped`**, bukan `Singleton` bawaan, supaya `DbContextOptions` tidak punya dua pendaftaran berbeda lifetime. Pendaftaran `AddDbContext` existing **tidak disentuh** |
| **DoD** | ✅ Seluruhnya terpenuhi dan **dipindai**, bukan diyakini: nol jalur penurun pencacah; nol endpoint alokasi (`QBE-CODE-002` bersih); nol `Count+1`/`Max+1` (`QBE-CODE-003` bersih); nol berkas Billing disentuh |
| **Membuka** | ✅ **Gerbang `G4` modul Bank Darah — secara kemampuan.** Disarankan menunggu `PLT-BE-004` lulus sebelum task Bank Darah dijadwalkan; lihat bagian 6.1 |

---

### ✅ `PLT-BE-004` — Durabilitas dan antrean dibuktikan di PostgreSQL sungguhan

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 10 September 2026.** Keenam uji durabilitas **lulus di PostgreSQL sungguhan** — `dotnet test` filter `NumberSeriesDurabilityTests` → **6 lulus, 0 gagal** terhadap `QuilvianNewDevSukma`, lewat opt-in `QUILVIAN_BILLING_TEST_DB_ALLOW_PERSONAL` (`RJ-BIL-DEC-019`). Keempat acceptance criteria terbukti. `dotnet build` solution `0 Error(s)`, **210 warning — sama persis baseline**; `UnitTests.Sqlite` filter `~Platform` **54 lulus, 0 gagal**. Pengaman fixture diuji lima skenario penolakan, seluruhnya ditolak sebelum koneksi dibuka. Bukti: [laporan](../task/report/backend/PLT-BE-004.md). **Riwayat:** 🟡 **SELESAI SEBAGIAN** 9 September 2026, **diperiksa ulang 10 September 2026**. Bukti: [laporan](../task/report/backend/PLT-BE-004.md). **Enam uji ditulis dan compile**; `dotnet build` solution `0 Error(s)`, 210 warning sama baseline. **Nol uji dijalankan dan nol dari empat acceptance criteria terbukti** — `AC-PLT-003`, `AC-PLT-004`, `AC-PLT-005`, dan `AC-PLT-012` seluruhnya belum. Percobaan ulang 10 September 2026 pada `843430b`: `dotnet test` filter `~Platform` → **0 lulus, 6 gagal dalam 13 ms**, seluruhnya `42501: permission denied to create database`. **Blocker dipersempit:** server PostgreSQL **terjangkau** dan target `QuilvianNumberSeriesTest` **diterima** ketiga penjagaan fixture; yang belum ada tinggal **hak `CREATEDB`** pada role-nya. Nol database dibuat, nol schema berubah |
| **Kenapa task tersendiri** | Yang diuji di sini adalah **apa yang terjadi pada `COMMIT` dan `ROLLBACK`**, dan bagaimana dua permintaan bersamaan diantrekan. Provider InMemory tidak punya transaksi sungguhan maupun `pg_advisory_xact_lock`, sehingga uji ini akan **lulus tanpa membuktikan apa pun** di sana — bentuk kegagalan yang paling berbahaya |
| **Outcome** | Ada bukti yang dapat ditunjukkan bahwa nomor benar-benar hangus saat pekerjaan batal, dan benar-benar tidak pernah kembar saat berebut |
| **Trace** | `AC-PLT-003`, `AC-PLT-004`, `AC-PLT-005`, `AC-PLT-012` · `DEC-PLT-008`, `INV-PLT-001`, `INV-PLT-003` |
| **Kontrak** | `testing/acceptance-test-matrix.md` |
| **Scope** | Uji pada `IntegrationTests.Postgres`: pembatalan pemanggil, alokasi bersamaan deret sama, alokasi bersamaan deret berbeda, dan **pembuktian empat deret Billing tidak berubah perilakunya** |
| **Dependency** | `PLT-BE-003` ✅ · **PostgreSQL yang berjalan** |
| **Acceptance** | ✅ **Keempatnya terbukti 10 September 2026** di PostgreSQL — `AC-PLT-003` pencacah tetap naik setelah pemanggil batal; `AC-PLT-004` dua puluh alokasi bersamaan menghasilkan dua puluh nomor berbeda; `AC-PLT-005` deret B selesai walau kunci deret A ditahan; `AC-PLT-012` deret Billing tetap memakai ulang nomor saat batal. **Riwayat:** sampai 10 September 2026 siang, nol dari empat terbukti |
| **Verification** | Angka hasil uji dicatat apa adanya, termasuk yang `NOT RUN` beserta alasannya |
| **Risk/owner** | **Tinggi / `Andry`.** Menuntut database berjalan — wewenang eksekusi terpisah |
| **Yang dibutuhkan agar naik ✅** | ✅ **Terpenuhi 10 September 2026 lewat jalan lain.** Pemilik memilih tidak membuat database test baru; `RJ-BIL-DEC-019` membuka database pengembangan personal `QuilvianNewDevSukma` lewat opt-in eksplisit, sehingga hak `CREATEDB` tidak lagi diperlukan. **Riwayat:** **Dipersempit 10 September 2026 menjadi satu tindakan pemilik server:** `ALTER ROLE <role> CREATEDB;`, **atau** DBA membuatkan `QuilvianNumberSeriesTest` dengan role itu sebagai pemiliknya. Sesudah itu `QUILVIAN_BILLING_TEST_DB` diisi dan keenam uji dijalankan ulang. Nama database **wajib** memuat `test` dan **dilarang** memuat `dev`, `prod`, `staging`, `uat`, atau `shared` — penjagaan itu ada sejak temuan `RJ-BIL-BE-002`. Menyediakan server PostgreSQL **tidak lagi diperlukan** |
| **DoD** | ✅ **Terpenuhi 10 September 2026.** `AC-PLT-003`/`AC-PLT-004` lulus **di PostgreSQL**, bukan InMemory. Empat deret Billing terbukti tidak berubah perilakunya — jalur invoice diuji langsung, tiga deret lain memakai mesin privat yang sama (`BillingNumberSeriesService.cs:141`) yang tidak berubah sejak `058e070`. Angka hasil uji dicatat apa adanya, termasuk riwayat `NOT RUN`. Nol uji inti dinyatakan lulus berdasarkan provider tanpa transaksi — justru itu sebabnya berkas ini terpisah dari uji SQLite |

---

### ✅ `PLT-BE-005` — Layar pemantauan deret dapat dibaca administrator

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI** 10 September 2026. Bukti: [laporan](../task/report/backend/PLT-BE-005.md). `dotnet build` `0 Error(s)`, `186 Warning(s)` — **nol dari berkas task ini**; `dotnet test` filter `~Platform` **54 lulus, 0 gagal** (naik dari 32; selisih 22 kasus adalah uji baru task ini). Keempat acceptance criteria terbukti |
| **Kenapa bukan `MUST HAVE`** | Gerbang `G4` tertutup tanpa satu layar pun. Menjadikannya wajib akan menahan sembilan task Bank Darah demi daftar yang dibuka administrator beberapa kali setahun |
| **Outcome** | Administrator dapat melihat keadaan deret saat menelusuri keluhan nomor — dan **menjelaskan** lompatan nomor tanpa membuka database |
| **Trace** | `FR-PLT-012`, `FR-PLT-013` |
| **Kontrak** | `contracts/api-contract.md` §2 — empat endpoint, **seluruhnya baca**; `contracts/permission-audit-matrix.md` |
| **Scope** | `NumberSeriesQueryService` + `NumberSeriesController` + DTO. Butir hak akses `NumberSeries : Read` |
| **Dependency** | `PLT-BE-003` ✅ |
| **Acceptance** | ✅ **Keempatnya terbukti** — empat endpoint memulangkan bentuk sesuai kontrak; butir `NumberSeries : Read` lahir dari controller dan dapat dicentang di layar Akses Role; pasangan hak akses tidak menghasilkan `403` permanen; nol endpoint hanya terlindungi `[Authorize]` |
| **Verification** | ✅ **Dijalankan.** 14 uji query service + 8 contract test hak akses bergaya `BloodBankRoleAccessContractTests`, seluruhnya lulus |
| **Risk/owner** | Rendah / `Andry` |
| **DoD** | ✅ Seluruhnya terpenuhi dan **dipindai**, bukan diyakini: nol endpoint tulis (`SeluruhEndpoint_HanyaGet_NolPostPutPatchDelete`); nol butir hak akses tulis (`ModulHanyaMendaftarkanButirRead_NolButirTulis`); membaca seluruh permukaan tidak mengubah satu baris pun (`MembacaSeluruhPermukaan_TidakMengubahSatuBarisPun`) |
| **⚠️ Belum dapat dipanggil** | **Migration `20260909070218_AddNumNumberSeries` belum dijalankan**, sehingga tabelnya belum ada di lingkungan mana pun. Risiko warisan dari `PLT-BE-002`, bukan tambahan task ini |
| **Cacat yang ditangkap uji** | SQLite menolak `DateTimeOffset` pada `MAX` **dan** `ORDER BY`. Bukan cacat produksi — PostgreSQL menerjemahkan keduanya — tetapi membuat jalur ringkasan mustahil dibuktikan tanpa PostgreSQL. Diperbaiki dengan memindahkan kedua jalur itu ke sisi klien; rinciannya di laporan §5.1 |

---

## 6. Dampak lintas modul

### 6.1 Bank Darah — `G4` tertutup pada `PLT-BE-003`

| Task Platform | Akibat bagi Bank Darah |
| --- | --- |
| `PLT-BE-001`..`002` | Belum ada. `G4` tetap ⛔ |
| **`PLT-BE-003`** | **`G4` tertutup.** Sembilan task backend Bank Darah — `BE-BD-003`, `004`, `006`, `007`, `008`, `009`, `010`, `012`, `015` — terbuka |
| `PLT-BE-004` | Bukti perilakunya. **Disarankan lulus lebih dulu** sebelum task Bank Darah dijadwalkan — ✅ **lulus 10 September 2026**, 6 dari 6 di PostgreSQL |

**Risiko yang disadari dan dinyatakan terbuka.** Secara teknis `G4` tertutup begitu `PLT-BE-003`
berdiri. Tetapi menjadwalkan `BE-BD-003` sebelum `PLT-BE-004` lulus berarti membangun order darah
di atas alokator yang **durabilitasnya belum dibuktikan**. Bila `AC-PLT-003` ternyata gagal,
perbaikannya menyentuh mesin yang sudah dipakai order darah. Urutan yang disarankan:
`PLT-BE-003` → `PLT-BE-004` lulus → baru jadwalkan Bank Darah.

✅ **Syarat urutan ini terpenuhi 10 September 2026.** `PLT-BE-004` lulus 6 dari 6 di PostgreSQL
sungguhan, sehingga durabilitas alokator tidak lagi berupa klaim. Menandai `G4` tertutup pada
roadmap Bank Darah **tidak** dilakukan dari task ini — gerbang itu milik pemiliknya, `Andry`, dan
dicatat lewat pemeliharaan blueprint Bank Darah.

### 6.2 Billing — nol perubahan pada slice ini

Empat deret produksi — invoice, deposit, shift kasir, kwitansi — **tidak dipindahkan**, dan
`BillingNumberSeriesService` **tidak disentuh satu baris pun**. Dasarnya `DEC-PLT-003` (migrasi
bertahap menurut risiko) dan `INV-PLT-003` (satu deret satu mekanisme).

`PLT-BE-004` justru menguji secara eksplisit bahwa keempatnya **tetap** berperilaku lama
(`AC-PLT-012`), supaya perubahan diam-diam ketahuan.

Pemindahannya adalah `PLT-SLICE-02` dan menuntut `DEC-PLT-006` dijawab lebih dulu.

---

## 7. Gerbang dan blocker yang masih terbuka

| ID | Isi | Menahan | Pemilik |
| --- | --- | --- | --- |
| **`P1`** / `OQ-PLT-014` | Baris registry `Platform`/`Num` dicatat dan `ACTIVE` | `PLT-BE-002`..`005`, lalu `G4` | Pemilik registry engineering |
| Verifikasi `AddDbContextFactory` | Berdampingan dengan `AddDbContext` existing | Detail implementasi `PLT-BE-003` | `Andry` |
| Eksekusi migration | Wewenang terpisah | Pemakaian nyata, bukan pembangunan | Pemilik database |
| ~~**Hak `CREATEDB`**~~ | ✅ **Tidak lagi diperlukan** 10 September 2026 — `RJ-BIL-DEC-019` membuka `QuilvianNewDevSukma` lewat opt-in, dan `PLT-BE-004` lulus tanpa database test baru. Semula: role database belum boleh membuat `QuilvianNumberSeriesTest`, ditemukan lewat `42501` | Tidak lagi menahan | — |
| `DEC-PLT-006` | Penerimaan pelanggaran `INV-PLT-001` selama peralihan | `PLT-SLICE-02` | Pemilik platform |
| `OQ-PLT-005` | Panjang nomor dan deret hampir habis | `PLT-SLICE-03`. Sementara ditahan `VAL-PLT-007` | Pemilik platform |
| `OQ-PLT-006` | Kode fasilitas di dalam awalan | `LATER SLICE` | Pemilik platform |
| `OQ-PLT-009` | Penelusuran nomor kembar yang mungkin sudah terbit | `PLT-SLICE-04` | Pemilik platform |

---

## 8. Yang sengaja tidak ada di roadmap ini

| Butir | Alasan |
| --- | --- |
| Task memindahkan deret Billing | `PLT-SLICE-02`, menuntut `DEC-PLT-006` |
| Task penyeragaman format dan panjang nomor | `PLT-SLICE-03`, menuntut `OQ-PLT-005` |
| Task penelusuran nomor kembar produksi | `PLT-SLICE-04`, menuntut akses data produksi |
| Task frontend | Ada di [frontend-roadmap.md](frontend-roadmap.md) |
| Penelusuran requirement → test | Ada di [requirement-traceability.md](requirement-traceability.md) |
| Task menjalankan migration | Wewenang terpisah yang diminta per tindakan, bukan task roadmap |
