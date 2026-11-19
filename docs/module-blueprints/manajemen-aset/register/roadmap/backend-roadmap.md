# Register Aset — Roadmap Backend

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12, `plan-module-delivery` |
| Status | `APPROVED_DESIGN_PLANNED` — desain `approved` (`AST-DEC-051`); task belum diberi wewenang implementasi |
| Blueprint | `AST-BP-001` revision `1`, sub-modul `register` |
| Contract | `AST-REG-1.0.0` `approved` 2026-11-12 |
| Masukan | decision revision `5`; `02-backend-architecture.md` hash `manifest`; `02-module-map.md` rev 1 |
| Backend / Frontend SHA | `1f54a175` (`rizkiG`) / `e49e8a81` (`RizkiV2`) |
| Task | `BE-AST-001` s.d. `BE-AST-013` (13 task) — deret bersama modul; ID bebas berikutnya dicatat di manifest modul |
| Handoff | Setiap task dijalankan `build-module-backend`, satu task per pemanggilan, dengan wewenang `TASK MODE: BACKEND` eksplisit. QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi dari `AGENTS.md` backend dan `docs/engineering/` |

**Legenda tanda status:** ✅ selesai dan terbukti · 🟡 sebagian · ⛔ terblokir (blocker disebut) · tanpa tanda = belum dikerjakan.

**Verifikasi baku backend (`V-BE`)** — dipakai semua task di bawah, sesuai `rules/backend/TEST_POLICY.md`:
QBE preflight/conformance `tooling/qbe/Invoke-QbeConformanceCheck.ps1` (periksa `Files evaluated`, jangan
memakai `-File` dengan array path); review diff dan cakupan; `dotnet restore` + `dotnet build
QuilvianSystemBackend.csproj` (dijalankan pemilik secara bawaan, atau agent bila diperintahkan); verifikasi kontrak API di Swagger (Tags, route, `[AccessPermission]`);
verifikasi proses bisnis pada skenario `AT-REG-*` yang disebut, dijalankan manual/runtime. Automated test
**tidak** diwajibkan.

**DoD baku backend (`DoD-BE`)**: setiap acceptance criteria terpetakan ke source yang ada; `V-BE`
dijalankan dan dicatat apa adanya; laporan `task/report/backend/<TASK-ID>.md` ditulis; status di roadmap
ini dan `requirement-traceability.md` diperbarui; `git status --short` dilaporkan; tanpa commit/push kecuali
diminta.

## Grafik Urutan Dependency

Grafik dipotong menjadi dua blok karena lebar; `BE-AST-006` muncul lagi sebagai sambungan blok kedua.

```text
BE-AST-001 ✅ ─> BE-AST-002 ✅ ─> BE-AST-003 ✅ ─┬─> BE-AST-004 ✅ ─> BE-AST-005 ✅ ─> BE-AST-006 ✅
                                                 │
                    BE-AST-014 [NLI] ✅ ─────────┘
```

```text
BE-AST-015 [NLI] 🟡 ─┬─> BE-AST-007 ✅ ─┬─> BE-AST-008 ✅
                     │                  │
      BE-AST-006 ✅ ─┘                  ├─> BE-AST-011 🟡
                                        │
                                        ├─> BE-AST-009 ✅┬─> BE-AST-012 ✅
                                        │               │
                                        │               └──────┐
                                        │                      │
                                        └─> BE-AST-010 ✅ ─────┴─> BE-AST-013 ✅
```

`[NLI]` = task backend sub-modul `nilai` pada `../../nilai/roadmap/backend-roadmap.md`, cermin baca-saja.

Pasangan prasyarat→task: **15** (sama dengan kolom `Dependency`).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-AST-001` |
| 2 | `BE-AST-001` | `BE-AST-002` |
| 3 | `BE-AST-002` | `BE-AST-003` |
| 4 | `BE-AST-003`, `BE-AST-014` [NLI] | `BE-AST-004` |
| 5 | `BE-AST-004` | `BE-AST-005` |
| 6 | `BE-AST-005` | `BE-AST-006` |
| 7 | `BE-AST-006`, `BE-AST-015` [NLI] | `BE-AST-007` |
| 8 | `BE-AST-007` | `BE-AST-008`, `BE-AST-009`, `BE-AST-010`, `BE-AST-011` — boleh paralel |
| 9 | `BE-AST-009` (dan `BE-AST-010` untuk `013`) | `BE-AST-012`, `BE-AST-013` — boleh paralel |

## Slice

| Slice | Gelombang MVP | Task | Status |
|---|---|---|---|
| `REG-S1` Fondasi: registry, model, migration, service bersama, master | `MVP-0` | `BE-AST-001` s.d. `BE-AST-006` | ✅ `BE-AST-001` ✅ 2026-11-12; `BE-AST-002` ✅ 2026-11-12; `BE-AST-003` ✅ 2026-11-12; `BE-AST-004` ✅ 2026-11-16; `BE-AST-005` ✅ 2026-11-16; `BE-AST-006` ✅ 2026-11-16 |
| `REG-S2` Register aset: pencatatan, label, penempatan, kondisi, dokumen, pemanfaatan, impor, dashboard | `MVP-1` | `BE-AST-007` s.d. `BE-AST-013` | 🟡 `BE-AST-007` ✅ 2026-11-16; `BE-AST-008` ✅ 2026-11-17; `BE-AST-009` ✅ 2026-11-17; `BE-AST-010` ✅ 2026-11-18; `BE-AST-011` 🟡 2026-11-19 (pekerja harian `Disetujui`→`Berjalan` terbukti 19 Nov; `AT-REG-15` positif menyusul 20 Nov); `BE-AST-012` ✅ 2026-11-18; `BE-AST-013` ✅ 2026-11-19 |

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-AST-001` | Registry mengizinkan model `Ast*` di `AssetManagement` | `AST-DEC-004`, `QBE-MOD-002/003` | `02-module-map.md` §1 | Registry existing | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | — | Kartu | `V-BE` (QBE) | Pemilik registry harus menyetujui baris; Rizki | `DoD-BE` |
| `BE-AST-002` | Delapan master tersimpan | `CAP-02.*`, `AST-DES-014/015` | data dict §2 | `IdentityModel` | `MasterData/{Models,Enums}`, configuration, DbSet | `BE-AST-001` | Kartu | `V-BE` | — | `DoD-BE` |
| `BE-AST-003` | Dua belas tabel transaksi register terdefinisi | `CAP-03..08` | data dict §3 | `IdentityModel`, FK HR/Finance | `Register/{Models,Enums}`, configuration, DbSet | `BE-AST-002` | Kartu | `V-BE` | — | `DoD-BE` |
| `BE-AST-004` | Migration fondasi siap diterapkan | `AST-DES-003` | `02-module-map.md` §3.2 | — | `Migrations/*AddAssetManagementFoundation*` | `BE-AST-003`, `BE-AST-014` [NLI] | Kartu | `V-BE` | Konflik snapshot EF dengan branch lain | `DoD-BE` |
| `BE-AST-005` | Service bersama tersedia untuk lima sub-modul | `AST-DES-004/005/016/020` | integration §3 | `NumberSeriesAllocator`, `WfpOrganizationAssignment` | 5 service + opsi konfigurasi | `BE-AST-004` | Kartu | `V-BE`, `AT-REG-46`, `48` | `AST-UNK-004` | `DoD-BE` |
| `BE-AST-006` | API master dapat dipakai | `CAP-02.*`, `CAP-EXT-05` | API 2.1–2.7 | Pola controller→service | 7 controller, `AssetMasterDataService`, seeder non-produksi | `BE-AST-005` | Kartu | `V-BE`, `AT-REG-34`, `35` | — | `DoD-BE` |
| `BE-AST-007` | Aset dapat dicatat sampai `Terdaftar` | `CAP-03.1..03.3`, `AST-DEC-043` | API 2.8, state §1 | `AssetBookService` [NLI] | `AssetsController`, `AssetRecordService` | `BE-AST-006`, `BE-AST-015` [NLI] | Kartu | `V-BE`, `AT-REG-01..03`, `30..33` | — | `DoD-BE` |
| `BE-AST-008` | Label QR dan pencarian nomor | `CAP-03.4` | API 2.8 (`/labels`, `/by-number`) | QRCoder, QuestPDF | `AssetLabelDocumentService` | `BE-AST-007` | Kartu | `V-BE`, `AT-REG-13` | Ukuran label `AST-OQ-016` | `DoD-BE` |
| `BE-AST-009` | Aset dapat diserahkan dan ditempatkan | `CAP-04.*`, `AST-DEC-044` | API 2.9–2.10, state §4 | — | `AssetPlacementService`, 2 controller | `BE-AST-007` | Kartu | `V-BE`, `AT-REG-04`, `05`, `36`, `37` | — | `DoD-BE` |
| `BE-AST-010` | Kondisi dilaporkan dan ditetapkan | `CAP-06.*`, `AST-DEC-017` | API 2.11, state §2–3 | Pola upload Finance | `AssetConditionService`, controller | `BE-AST-007` | Kartu | `V-BE`, `AT-REG-06`, `07`, `39`, `40` | — | `DoD-BE` |
| `BE-AST-011` | Dokumen, pengamanan, pemanfaatan berjalan | `CAP-03.5`, `08.*`, `AST-DEC-021/029` | API 2.12–2.14, state §5 | `BackgroundService` | 3 service, 3 controller, `AssetDailyStatusWorker` | `BE-AST-007` | Kartu | `V-BE`, `AT-REG-08`, `09`, `14`, `15`, `41`, `42` | Batas Direktur `AST-OQ-004` (data) | `DoD-BE` |
| `BE-AST-012` | Aset lama masuk lewat impor | `CAP-EXT-01`, `AST-DEC-037/050` | API 2.15, state §6 | ClosedXML, pola batch Finance | `AssetImportService`, controller | `BE-AST-009` | Kartu | `V-BE`, `AT-REG-10`, `43..45` | Tanggal awal `AST-OQ-006` (data) | `DoD-BE` |
| `BE-AST-013` | Dashboard, laporan, History & Audit | `CAP-01.1..01.3`, `01.6`, `11.1`, `11.3`, `11.7` | API 2.16–2.17 | — | 2 query service, 2 controller | `BE-AST-009`, `BE-AST-010` | Kartu | `V-BE`, `AT-REG-12`, `16` | — | `DoD-BE` |

## Kartu task

### ✅ `BE-AST-001` — Pendaftaran registry `AssetManagement` / `Ast`

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 12 November 2026.** Ketiga acceptance criteria terbukti: enam baris `Ast` `ACTIVE` + kepanjangan + catatan perubahan (diff 8 insertions, 0 deletions); harness fungsi asli `Resolve-RegistryOwnership` 13/13 kasus sesuai (kelima submodule → `Ast`, `MasterData` → `Mst`, `Trx*` ditolak, folder belum terdaftar ambigu, Finance/Payroll tanpa regresi); baseline HEAD membuktikan `AstAsset` sebelumnya ditolak. QBE checker WorkingTree `PASS` dengan `Files evaluated: 0` (bukan bukti, tidak ada `.cs`). `dotnet build` `NOT APPLICABLE` (nol source). Di-commit dan di-push ke `origin/rizkiG` 12 November 2026 atas perintah Rizki, bersama `BE-AST-002` dan `BE-AST-003`. Laporan: [`BE-AST-001.md`](../task/report/backend/BE-AST-001.md) |
| Cakupan | Enam baris registry pada `02-module-map.md` bagian 1 + baris catatan perubahan; pemeriksaan bahwa `AssetManagement/MasterData` tercakup baris `Mst` tanpa ambiguitas (preseden `PatientRefund`) |
| Acceptance criteria | 1. Enam baris `Ast` ada dengan Area, Owner, Category `BUSINESS DOMAIN / MODULE`, Lifecycle `ACTIVE`. 2. Catatan perubahan menyebut `AST-DEC-004` dan penyetujunya. 3. QBE checker menyelesaikan kepemilikan `Areas/Corporate/AssetManagement/Register/Models/AstAsset.cs` (berkas contoh) tanpa ambiguitas |
| Catatan | Bila pemilik registry bukan Rizki, task berhenti dan meminta persetujuan pemilik itu |

### ✅ `BE-AST-002` — Model, enum, dan configuration delapan master

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 12 November 2026.** `dotnet build` dijalankan Rizki dan berhasil: DLL 14:44:25 WIB, sesudah suntingan terakhir 14:24:42 WIB, memuat 20 dari 20 penanda (8 model, 8 configuration, 2 namespace, 2 DbSet); jumlah warning tidak dilaporkan. Di-commit dan di-push ke `origin/rizkiG` atas perintah Rizki. **Riwayat — sebelumnya 🟡 SEBAGIAN 12 November 2026:** keempat acceptance criteria terpetakan ke source: 8 model `Mst*` di `Areas/Corporate/AssetManagement/MasterData/Models/`, 8 configuration di `Repositories/Configurations/Corporate/AssetManagement/MasterData/`, 8 DbSet di `ApplicationDbContext` (+15 baris, 0 dihapus); nol enum karena kamus §2 tidak memuat kolom enum. QBE checker WorkingTree ReportOnly dan Strict `PASS` dengan `Files evaluated: 17`, 0 temuan; harness fungsi asli checker 8/8 entity (`IdentityModel`, configuration, DbSet, pemilik `Mst`) dan 4/4 kontrol negatif; pencocokan statis kamus §2 243 pemeriksaan 0 gagal (uji mutasi menangkap 4 dari 4 mutasi). Butir DoD `V-BE` **`dotnet restore` + `dotnet build` belum terpenuhi** — menunggu build oleh Rizki; validitas model EF terbukti paling lambat pada `BE-AST-004`. Tanpa migration, tanpa perubahan database, belum di-commit. Laporan: [`BE-AST-002.md`](../task/report/backend/BE-AST-002.md) |
| Cakupan | 8 berkas `Mst*` di `Areas/Corporate/AssetManagement/MasterData/Models/`; configuration di `Repositories/Configurations/Corporate/AssetManagement/MasterData/`; DbSet |
| Acceptance criteria | 1. Kolom, panjang, index unik, dan FK sesuai `data/data-dictionary.md` §2. 2. `InventoryCode` maks 11. 3. Tidak ada `SortOrder`. 4. Semua FK `Restrict` |

### ✅ `BE-AST-003` — Model, enum, dan configuration dua belas tabel transaksi

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 12 November 2026.** `dotnet build` dijalankan Rizki dan berhasil: DLL 15:11:35 WIB, sesudah suntingan terakhir 15:02:55 WIB, memuat 44 dari 44 penanda (12 model, 12 configuration, 14 enum, 2 namespace, 2 DbSet, 2 tipe master); jumlah warning tidak dilaporkan. Di-commit dan di-push ke `origin/rizkiG` atas perintah Rizki. **Riwayat — sebelumnya 🟡 SEBAGIAN 12 November 2026:** keempat acceptance criteria terpetakan ke source: 12 model `Ast*` di `Areas/Corporate/AssetManagement/Register/Models/`, 14 enum di `Register/Enums/AssetRegisterEnums.cs`, 12 configuration di `Repositories/Configurations/Corporate/AssetManagement/Register/`, 12 DbSet (+19 baris `ApplicationDbContext`, 0 dihapus). Unik parsial `IX_AstAssetPlacement_Current`, `IX_AstConditionReport_Open`, `IX_AstAsset_InventoryNumber` persis DDL kamus §4; `RowVersion` token tepat pada 5 tabel; 18 properti enum `HasConversion<int>()`. QBE checker ReportOnly dan Strict `PASS` dengan `Files evaluated: 42`, 0 temuan; harness fungsi asli checker 12/12 entity dan 6/6 kontrol negatif; pencocokan statis kamus §3 + DDL §4 + enum §5.4 1030 pemeriksaan 0 gagal atas 173 kolom (uji mutasi menangkap 8 dari 8 mutasi). Butir DoD `V-BE` **`dotnet restore` + `dotnet build` belum terpenuhi** — menunggu build oleh Rizki; validitas model EF terbukti paling lambat pada `BE-AST-004`. Tanpa migration, tanpa perubahan database, belum di-commit. Laporan: [`BE-AST-003.md`](../task/report/backend/BE-AST-003.md) |
| Cakupan | 12 model `Ast*`, 14 enum (`02-backend-architecture.md` §5.4), configuration, DbSet |
| Acceptance criteria | 1. Kolom sesuai data dict §3. 2. Unique parsial: penempatan berjalan per aset, laporan kondisi terbuka per aset, nomor inventaris bila tidak null. 3. `RowVersion` token konkurensi pada aset, penyerahan, laporan kondisi, pemanfaatan, batch impor. 4. Enum `HasConversion<int>()` |

### ✅ `BE-AST-004` — Migration `AddAssetManagementFoundation`

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 16 November 2026.** Migration `20261116030049_AddAssetManagementFoundation` dibuat dan diterapkan Rizki: `Up()` 25 `CreateTable` + 91 `CreateIndex` tanpa operasi lain, `Down()` 25 `DropTable`; snapshot +2815/−0 (25 blok Aset) = Designer (1033 blok); pencocokan isi ↔ source 113/0, uji mutasi 3/3; QBE Strict `PASS`; build Rizki (DLL 10:15:04 memuat migration); basis data memuat migration, 25 tabel, 535 kolom, 46 FK `Restrict`, 116 index, check constraint buku. Jumlah warning tidak dilaporkan. Laporan: [`BE-AST-004.md`](../task/report/backend/BE-AST-004.md) |
| Cakupan | Satu migration berisi 20 tabel `register` + 5 tabel `nilai` (`BE-AST-014`); berkas migration, Designer, snapshot |
| Acceptance criteria | 1. Migration hanya menambah 25 tabel; tidak mengubah tabel modul lain. 2. `Down()` menghapus 25 tabel. 3. Snapshot memuat seluruh entity baru tanpa duplikasi blok entity branch lain |
| Catatan | Penerapan ke database (`dotnet ef database update`) oleh pemilik, bukan agent |

### ✅ `BE-AST-005` — Service bersama

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 16 November 2026.** `dotnet build` dijalankan Rizki dan berhasil: DLL 10:47:47 WIB, sesudah suntingan terakhir 10:28:40 WIB, memuat 23 dari 23 penanda; jumlah warning tidak dilaporkan. Kelima service (`AssetActorResolver`, `AssetHistoryService`, `AssetProcessLockService`, `AssetAttachmentService`, `AssetNumberingService`), opsi `AssetManagement:Attachment`, dan `AddAssetManagement()` (+1 baris pemanggilan di `Program.cs`) dibuat; penjaga append-only `AstAssetHistory` di `ApplicationDbContext`. Keenam acceptance criteria terpetakan ke source; QBE Strict `Files evaluated: 10` `PASS` 0 temuan; nol baris komentar. Deklarasi penanda `AssetRegister : ReadAllUnits` menyusul `BE-AST-007`. Laporan: [`BE-AST-005.md`](../task/report/backend/BE-AST-005.md) |
| Cakupan | `AssetActorResolver`, `AssetHistoryService`, `AssetProcessLockService`, `AssetAttachmentService`, `AssetNumberingService`; opsi `AssetManagement:Attachment`; registrasi `AddScoped` |
| Acceptance criteria | 1. Pengguna tanpa penugasan aktif → daftar unit kosong (fail-closed). 2. Penugasan manajerial aktif → Kepala Unit unit itu. 3. Penanda `AssetRegister : ReadAllUnits` dibaca lewat `HasAccessAsync`. 4. Kunci proses menolak `409` bila sudah terpasang. 5. Lampiran menolak `503` bila pengaturan kosong, `413` bila terlalu besar. 6. Nomor aset `AST-<KODE>-######` lewat `NumberSeriesAllocator` (`NEVER`) |

### ✅ `BE-AST-006` — API master

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 16 November 2026.** 7 controller (58 endpoint: 6 master koleksi × 9 baseline + `document-requirements` + `inventory-codes`, pengaturan `GET`/`PUT`), `AssetMasterDataService`, `AssetEntityResolver` (badan hukum/site dari konfigurasi), seeder `IHostedService`. QBE Strict `PASS`; atribut akses 339/0; build Rizki (DLL 11:14:03); Swagger 58 endpoint cocok; runtime API 56/56 termasuk `AT-REG-34`; seeder + pengaturan 14/14; nol baris komentar. `AT-REG-35` lulus 2026-11-16 pada uji `BE-AST-007` (409 `053`). Delta kontrak (`filters/metadata`, `summary`, `DELETE`, paging) di laporan. Laporan: [`BE-AST-006.md`](../task/report/backend/BE-AST-006.md) |
| Cakupan | 7 controller master + `AssetMasterDataService`; seeder data awal yang menolak produksi |
| Acceptance criteria | 1. Seluruh endpoint 2.1–2.7 dengan Tags dan `[AccessPermission]` persis kontrak. 2. `AST-VAL-REG-050..056`. 3. Kode kelompok terkunci setelah dipakai (dapat diverifikasi sesudah `BE-AST-007`). 4. Seeder mengisi isi minimum `02-backend-architecture.md` §9 di non-produksi |

### ✅ `BE-AST-007` — Pencatatan, persetujuan, dan penomoran aset

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 16 November 2026.** `AssetsController` (13 endpoint), `AssetRecordService`, DTO, penanda `AssetRegister : ReadAllUnits`. QBE Strict `Files evaluated: 9` `PASS`, nol baris komentar. `dotnet build` dijalankan Rizki dan berhasil (DLL 15:45:37 WIB); jumlah warning tidak dilaporkan. Swagger 13 operasi; registry akses memuat 7 aksi termasuk `ReadAllUnits`. Runtime **52/52 + 14/14 `PASS`**: `AT-REG-01`, `02`, `03`, `30`, `31`, `33`, `35`, `47`, penolakan, dan 403 batas unit. DB baca-saja: 1 baris kotak keluar hibah `Held`, deret nomor tanpa nomor terbuang. Kedelapan acceptance criteria teruji di runtime. **Dikecualikan atas persetujuan Rizki:** `AT-REG-32` (alokator gagal; tidak dapat dipicu lewat API, dijaga source). Belum di-commit. **Riwayat:** 🟡 2026-11-16 sore, ketika baru jalur pengaju yang teruji (36/36). Laporan: [`BE-AST-007.md`](../task/report/backend/BE-AST-007.md) |
| Cakupan | `AssetsController` (selain `/labels`, `/by-number`), `AssetRecordService`; panggilan `AssetBookService.CreateForRegisteredAsset` dan `AssetAccountingEventService.StageDonationAcquisition` (`BE-AST-015`) |
| Acceptance criteria | 1. 1–200 draft per permintaan. 2. Atribut medis wajib saat diajukan. 3. Persetujuan: nomor terbit, `Terdaftar`, buku nilai terbentuk, hibah → satu baris kotak keluar. 4. Persetujuan massal per aset transaksi sendiri. 5. Pengaju ≠ penyetuju. 6. Tolak → `Draft` tanpa nomor. 7. Rupiah hanya `/amounts`. 8. Batas unit sesuai `AST-DES-016` |

### ✅ `BE-AST-008` — Label QR dan pencarian nomor

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 17 November 2026.** `AssetLabelDocumentService` (PDF A4 3 × 7 stiker 63,5 × 38,1 mm, default `AST-OQ-016`; QR QRCoder berisi nomor inventaris saja; nomor dan tahun perolehan sebaris), `POST /labels` dan `GET /by-number/{inventoryNumber}` (`AssetRegister : Read`, batas unit `AST-VAL-REG-016`, `/labels` tanpa log audit). QBE Strict `PASS` (6 berkas, dua kali); `dotnet build` Rizki berhasil (DLL 2026-11-17 10:34:00); runtime **13/13** termasuk `AT-REG-13`: 40 label → PDF 2 halaman, 40/40 QR berisi nomor saja (39 terbaca zxing-js, 1 matriks identik referensi). Putaran pertama gagal 500 (`DocumentLayoutException`, lebar kolom 0,28 pt melebihi area cetak), diperbaiki dalam task. Data uji: 29 kursi `AST-MBL-000008..000036`. Belum di-commit. Laporan: [`BE-AST-008.md`](../task/report/backend/BE-AST-008.md) |
| Cakupan | `AssetLabelDocumentService` (label), endpoint `/labels` dan `/by-number/{inventoryNumber}` |
| Acceptance criteria | 1. PDF label untuk aset terpilih; QR berisi nomor inventaris saja; tahun perolehan tercetak di samping nomor. 2. `/by-number` mengembalikan aset beserta lokasi tercatat, menghormati batas unit |

### ✅ `BE-AST-009` — Penyerahan dan penempatan

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 17 November 2026.** `AssetPlacementService` (serahkan/konfirmasi/tolak/batal, `ApplyPlacementAsync` untuk pergerakan), `AssetHandoversController` (9 endpoint 2.9 + `filters/metadata`, `summary`), `AssetPlacementsController` (2 endpoint 2.10), PDF Berita Acara Penyerahan (nama, jabatan, waktu WIB), titik panggil opname `AssetStockTakeGate` (`026`, diisi `BE-AST-021`). Penerima = kepala unit tujuan atau pegawai penanggung jawab, tanpa pengecualian peran. QBE Strict `PASS` (11 berkas); `dotnet build` Rizki berhasil (DLL 2026-11-17 14:38:32); runtime **33/33** (`AT-REG-04`, `05` lulus; `36` lulus untuk kunci penyerahan; `37` lulus dengan aktor bukan-penerima; `38` menunggu `BE-AST-021`). Putaran pertama menemukan nomor `AST-PNY` terbuang oleh percobaan `024`, diperbaiki dalam task. Belum di-commit. Laporan: [`BE-AST-009.md`](../task/report/backend/BE-AST-009.md) |
| Cakupan | `AssetPlacementService` (termasuk `ApplyPlacement` untuk `pergerakan`), `AssetHandoversController`, `AssetPlacementsController`, PDF Berita Acara Penyerahan |
| Acceptance criteria | 1. Matriks state §4 lengkap. 2. Penempatan berubah hanya saat konfirmasi penerima yang dituju. 3. `PindahDalamUnit` hanya di unit yang sama. 4. Kunci proses dipasang/dilepas. 5. Berita acara memuat nama, jabatan, waktu (`AST-DEC-046`). 6. Pemeriksaan unit sedang opname (`AST-VAL-REG-026`) disiapkan sebagai titik panggil yang diisi `BE-AST-021` |

### ✅ `BE-AST-010` — Laporan dan penetapan kondisi

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 18 November 2026.** `AssetConditionService` (lapor, tetapkan/tolak, setujui hilang, `OpenReportAsync` untuk sub-modul lain), `AssetConditionReportsController` (9 endpoint: 6 kontrak 2.11 + `filters/metadata`, `summary`, unduh lampiran), PDF Berita Acara Kehilangan. Kelima kriteria terpetakan ke source; QBE Strict 10 berkas `PASS` 0 temuan; nol baris komentar; `dotnet build` Rizki berhasil (DLL 08:48:58). Uji runtime **34/34** (`AT-REG-06`, `07`, `39`, `40` lulus) + 2 pemeriksaan tambahan (Hilang lewat `decide` → 400; sumber Pemeriksaan Bagian Aset tanpa `Decide` → 403, policy uji dipulihkan); PDF diperiksa visual. `OpenReportAsync` belum teruji runtime karena pemanggilnya (`BE-AST-018`, `inventarisasi`) belum ada. Delta kontrak dicatat di laporan. Belum di-commit. Laporan: [`BE-AST-010.md`](../task/report/backend/BE-AST-010.md) |
| Cakupan | `AssetConditionService` (termasuk `OpenReport` untuk sub-modul lain), controller, PDF Berita Acara Kehilangan |
| Acceptance criteria | 1. Matriks state §2–3. 2. Foto wajib dari unit. 3. Hilang butuh berita acara dan Kepala Bagian Aset bukan pelapor. 4. Satu laporan terbuka per aset. 5. Linimasa `KondisiBerubah` |

### 🟡 `BE-AST-011` — Dokumen kepemilikan, pengamanan, pemanfaatan

| Field | Isi |
|---|---|
| **Status** | 🟡 **SEBAGIAN 18 November 2026.** `AssetDocumentService` (dokumen kepemilikan, kelengkapan per kategori, dokumen habis/hampir habis menurut pengaturan), `AssetUtilizationService` (draft → ajukan dengan penyetuju dan batas rupiah dikunci → setujui/teruskan → keputusan Direktur; akhiri, batal), `AssetAttachmentAccessService`, `AssetDailyStatusWorker` (bawaan mati), `GET /assets/idle` + `isIdle`; 3 controller (23 endpoint). QBE Strict 20 berkas `PASS` 0 temuan; atribut akses 39/39; nol baris komentar; `dotnet build` Rizki berhasil (DLL 10:39:38). Runtime **60/60** + **13/15** (2 kegagalan = harapan skrip keliru, data membuktikan backend benar): `AT-REG-08` (persetujuan), `09`, `14`, `41`, `42` lulus, kriteria 1, 3, 4 terbukti. Pekerja harian sesudah restart backend 11:01: `Berjalan`→`Berakhir` 4/4 (`AST-PMF-000007`, kunci lepas, linimasa). Kriteria 5 bagian `Disetujui`→`Berjalan` terbukti 19 November 2026: pekerja harian mengubah `AST-PMF-000008` menjadi Berjalan pukul 02:33 UTC, kunci aset tetap, linimasa `AST-MBL-000005` "Dimanfaatkan" (`cek-pekerja-19nov.mjs` 3/3). **Belum terbukti:** `AT-REG-15` kasus positif (butuh aset Tidak Digunakan lebih dari N hari; mulai 20 Nov lewat batas = 1). Delta kontrak dicatat di laporan. Di-commit `ae054ee8`. Laporan: [`BE-AST-011.md`](../task/report/backend/BE-AST-011.md) |
| Cakupan | `AssetDocumentService`, `AssetUtilizationService`, `AssetDailyStatusWorker` (bawaan mati), 3 controller (2.12–2.14) |
| Acceptance criteria | 1. Kelengkapan dokumen per kategori dan dokumen hampir habis menurut pengaturan. 2. Aset menganggur sebagai keadaan turunan. 3. Penyetuju pemanfaatan dihitung dan dikunci saat diajukan (batas dari `nilai`). 4. Aset bukan milik RS ditolak. 5. Pekerja harian memindahkan `Disetujui`→`Berjalan`→`Berakhir` |

### ✅ `BE-AST-012` — Impor data awal

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 18 November 2026.** `AssetImportService` (unggah + validasi seluruh baris, setujui data aset, setujui angka dengan validasi ulang lalu penerapan satu transaksi, tolak, batal), `AssetImportWorkbook` (templat 19 kolom + 3 lembar kode, parsing ClosedXML), `AssetImportsController` (11 endpoint), helper `AssetBookService.CheckOpeningBalance`. QBE Strict 12 berkas `PASS`; atribut akses 11/11; nol baris komentar; `dotnet build` Rizki berhasil (DLL 15:02:51). Runtime **33/33**: `AT-REG-43` (2.400 baris, 1 salah → Gagal Validasi, 0 aset), `AT-REG-44` (kedua tahap 403 `067`), `AT-REG-45` (master berubah → 409, 0 aset, batch tetap), `AT-REG-10` pada **30 baris** (30 aset + buku saldo awal, **0 kotak keluar**, cek DB baca-saja); keempat kriteria terbukti. Perbaikan format pesan `061` sesudah uji terbukti pada build ulang (DLL 15:35:25). Risiko: penerapan ±189 ms/baris (2.400 baris ±7,5 menit dalam satu request). Kolom templat dan `/rows` = delta. Belum di-commit. Laporan: [`BE-AST-012.md`](../task/report/backend/BE-AST-012.md) |
| Cakupan | `AssetImportService` (ClosedXML), controller 2.15, templat Excel |
| Acceptance criteria | 1. Satu baris salah → seluruh berkas `GagalValidasi`. 2. Dua persetujuan, bukan pengunggah. 3. Penerapan satu transaksi: aset `Terdaftar` + `Ditempatkan` + buku saldo awal (`AssetBookService.CreateOpeningBalance`), **tanpa** kotak keluar. 4. Batas 5.000 baris dari konfigurasi |

### ✅ `BE-AST-013` — Dashboard, laporan register, History & Audit

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 19 November 2026.** `AssetDashboardQueryService` (5 kartu + 5 kondisi + aset rusak terbaru, tiap kartu membawa saringan daftarnya; per unit), `AssetReportQueryService` + `AssetReportWorkbook` (11.1, 11.3, 11.7, ekspor Excel tanpa rupiah, batas 10.000 baris), 2 controller (7 endpoint); saringan daftar aset dipisah ke `FilteredAssetsAsync` supaya kartu dan daftar memakai saringan yang sama. QBE Strict 9 berkas `PASS`; atribut akses 7/7; nol baris komentar; `dotnet build` Rizki berhasil (DLL 2026-11-19 08:36:06). Runtime **74/74**: `AT-REG-16` (10 kartu dan baris per unit = daftar), History = isi `AstAssetHistory` di DB total dan per sub-modul, 4 berkas ekspor dibaca tanpa kolom/sel rupiah, LOKET 403, akun tanpa `ReadAllUnits` terbatas dan unit lain 403. Uji akun terbatas 21/21 sesudah Siti (PENERIMA) diberi penugasan Kepala Unit Instalasi Rawat Inap: `AT-REG-12` positif (45 aset unitnya) dan `AT-REG-46` lulus. Keempat kriteria terbukti. Delta kontrak (metadata, DTO History, kartu ber-`filter`) dicatat di laporan. Laporan: [`BE-AST-013.md`](../task/report/backend/BE-AST-013.md) |
| Cakupan | `AssetDashboardQueryService`, `AssetReportQueryService`, 2 controller, ekspor Excel |
| Acceptance criteria | 1. Kartu = hitungan daftar bersaringan. 2. Batas unit berlaku. 3. History & Audit membaca `AstAssetHistory` lintas sub-modul. 4. Ekspor tanpa kolom rupiah |
