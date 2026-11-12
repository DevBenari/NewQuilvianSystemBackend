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
BE-AST-001 ─> BE-AST-002 ─> BE-AST-003 ─┬─> BE-AST-004 ─> BE-AST-005 ─> BE-AST-006
                                        │
                    BE-AST-014 [NLI] ───┘
```

```text
BE-AST-015 [NLI] ─┬─> BE-AST-007 ─┬─> BE-AST-008
                  │               │
      BE-AST-006 ─┘               ├─> BE-AST-011
                                  │
                                  ├─> BE-AST-009 ─┬─> BE-AST-012
                                  │               │
                                  │               └──────┐
                                  │                      │
                                  └─> BE-AST-010 ────────┴─> BE-AST-013
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
| `REG-S1` Fondasi: registry, model, migration, service bersama, master | `MVP-0` | `BE-AST-001` s.d. `BE-AST-006` | belum dikerjakan |
| `REG-S2` Register aset: pencatatan, label, penempatan, kondisi, dokumen, pemanfaatan, impor, dashboard | `MVP-1` | `BE-AST-007` s.d. `BE-AST-013` | belum dikerjakan |

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

### `BE-AST-001` — Pendaftaran registry `AssetManagement` / `Ast`

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | Enam baris registry pada `02-module-map.md` bagian 1 + baris catatan perubahan; pemeriksaan bahwa `AssetManagement/MasterData` tercakup baris `Mst` tanpa ambiguitas (preseden `PatientRefund`) |
| Acceptance criteria | 1. Enam baris `Ast` ada dengan Area, Owner, Category `BUSINESS DOMAIN / MODULE`, Lifecycle `ACTIVE`. 2. Catatan perubahan menyebut `AST-DEC-004` dan penyetujunya. 3. QBE checker menyelesaikan kepemilikan `Areas/Corporate/AssetManagement/Register/Models/AstAsset.cs` (berkas contoh) tanpa ambiguitas |
| Catatan | Bila pemilik registry bukan Rizki, task berhenti dan meminta persetujuan pemilik itu |

### `BE-AST-002` — Model, enum, dan configuration delapan master

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | 8 berkas `Mst*` di `Areas/Corporate/AssetManagement/MasterData/Models/`; configuration di `Repositories/Configurations/Corporate/AssetManagement/MasterData/`; DbSet |
| Acceptance criteria | 1. Kolom, panjang, index unik, dan FK sesuai `data/data-dictionary.md` §2. 2. `InventoryCode` maks 11. 3. Tidak ada `SortOrder`. 4. Semua FK `Restrict` |

### `BE-AST-003` — Model, enum, dan configuration dua belas tabel transaksi

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | 12 model `Ast*`, 14 enum (`02-backend-architecture.md` §5.4), configuration, DbSet |
| Acceptance criteria | 1. Kolom sesuai data dict §3. 2. Unique parsial: penempatan berjalan per aset, laporan kondisi terbuka per aset, nomor inventaris bila tidak null. 3. `RowVersion` token konkurensi pada aset, penyerahan, laporan kondisi, pemanfaatan, batch impor. 4. Enum `HasConversion<int>()` |

### `BE-AST-004` — Migration `AddAssetManagementFoundation`

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | Satu migration berisi 20 tabel `register` + 5 tabel `nilai` (`BE-AST-014`); berkas migration, Designer, snapshot |
| Acceptance criteria | 1. Migration hanya menambah 25 tabel; tidak mengubah tabel modul lain. 2. `Down()` menghapus 25 tabel. 3. Snapshot memuat seluruh entity baru tanpa duplikasi blok entity branch lain |
| Catatan | Penerapan ke database (`dotnet ef database update`) oleh pemilik, bukan agent |

### `BE-AST-005` — Service bersama

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | `AssetActorResolver`, `AssetHistoryService`, `AssetProcessLockService`, `AssetAttachmentService`, `AssetNumberingService`; opsi `AssetManagement:Attachment`; registrasi `AddScoped` |
| Acceptance criteria | 1. Pengguna tanpa penugasan aktif → daftar unit kosong (fail-closed). 2. Penugasan manajerial aktif → Kepala Unit unit itu. 3. Penanda `AssetRegister : ReadAllUnits` dibaca lewat `HasAccessAsync`. 4. Kunci proses menolak `409` bila sudah terpasang. 5. Lampiran menolak `503` bila pengaturan kosong, `413` bila terlalu besar. 6. Nomor aset `AST-<KODE>-######` lewat `NumberSeriesAllocator` (`NEVER`) |

### `BE-AST-006` — API master

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | 7 controller master + `AssetMasterDataService`; seeder data awal yang menolak produksi |
| Acceptance criteria | 1. Seluruh endpoint 2.1–2.7 dengan Tags dan `[AccessPermission]` persis kontrak. 2. `AST-VAL-REG-050..056`. 3. Kode kelompok terkunci setelah dipakai (dapat diverifikasi sesudah `BE-AST-007`). 4. Seeder mengisi isi minimum `02-backend-architecture.md` §9 di non-produksi |

### `BE-AST-007` — Pencatatan, persetujuan, dan penomoran aset

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | `AssetsController` (selain `/labels`, `/by-number`), `AssetRecordService`; panggilan `AssetBookService.CreateForRegisteredAsset` dan `AssetAccountingEventService.StageDonationAcquisition` (`BE-AST-015`) |
| Acceptance criteria | 1. 1–200 draft per permintaan. 2. Atribut medis wajib saat diajukan. 3. Persetujuan: nomor terbit, `Terdaftar`, buku nilai terbentuk, hibah → satu baris kotak keluar. 4. Persetujuan massal per aset transaksi sendiri. 5. Pengaju ≠ penyetuju. 6. Tolak → `Draft` tanpa nomor. 7. Rupiah hanya `/amounts`. 8. Batas unit sesuai `AST-DES-016` |

### `BE-AST-008` — Label QR dan pencarian nomor

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | `AssetLabelDocumentService` (label), endpoint `/labels` dan `/by-number/{inventoryNumber}` |
| Acceptance criteria | 1. PDF label untuk aset terpilih; QR berisi nomor inventaris saja; tahun perolehan tercetak di samping nomor. 2. `/by-number` mengembalikan aset beserta lokasi tercatat, menghormati batas unit |

### `BE-AST-009` — Penyerahan dan penempatan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | `AssetPlacementService` (termasuk `ApplyPlacement` untuk `pergerakan`), `AssetHandoversController`, `AssetPlacementsController`, PDF Berita Acara Penyerahan |
| Acceptance criteria | 1. Matriks state §4 lengkap. 2. Penempatan berubah hanya saat konfirmasi penerima yang dituju. 3. `PindahDalamUnit` hanya di unit yang sama. 4. Kunci proses dipasang/dilepas. 5. Berita acara memuat nama, jabatan, waktu (`AST-DEC-046`). 6. Pemeriksaan unit sedang opname (`AST-VAL-REG-026`) disiapkan sebagai titik panggil yang diisi `BE-AST-021` |

### `BE-AST-010` — Laporan dan penetapan kondisi

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | `AssetConditionService` (termasuk `OpenReport` untuk sub-modul lain), controller, PDF Berita Acara Kehilangan |
| Acceptance criteria | 1. Matriks state §2–3. 2. Foto wajib dari unit. 3. Hilang butuh berita acara dan Kepala Bagian Aset bukan pelapor. 4. Satu laporan terbuka per aset. 5. Linimasa `KondisiBerubah` |

### `BE-AST-011` — Dokumen kepemilikan, pengamanan, pemanfaatan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | `AssetDocumentService`, `AssetUtilizationService`, `AssetDailyStatusWorker` (bawaan mati), 3 controller (2.12–2.14) |
| Acceptance criteria | 1. Kelengkapan dokumen per kategori dan dokumen hampir habis menurut pengaturan. 2. Aset menganggur sebagai keadaan turunan. 3. Penyetuju pemanfaatan dihitung dan dikunci saat diajukan (batas dari `nilai`). 4. Aset bukan milik RS ditolak. 5. Pekerja harian memindahkan `Disetujui`→`Berjalan`→`Berakhir` |

### `BE-AST-012` — Impor data awal

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | `AssetImportService` (ClosedXML), controller 2.15, templat Excel |
| Acceptance criteria | 1. Satu baris salah → seluruh berkas `GagalValidasi`. 2. Dua persetujuan, bukan pengunggah. 3. Penerapan satu transaksi: aset `Terdaftar` + `Ditempatkan` + buku saldo awal (`AssetBookService.CreateOpeningBalance`), **tanpa** kotak keluar. 4. Batas 5.000 baris dari konfigurasi |

### `BE-AST-013` — Dashboard, laporan register, History & Audit

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| Cakupan | `AssetDashboardQueryService`, `AssetReportQueryService`, 2 controller, ekspor Excel |
| Acceptance criteria | 1. Kartu = hitungan daftar bersaringan. 2. Batas unit berlaku. 3. History & Audit membaca `AstAssetHistory` lintas sub-modul. 4. Ekspor tanpa kolom rupiah |
