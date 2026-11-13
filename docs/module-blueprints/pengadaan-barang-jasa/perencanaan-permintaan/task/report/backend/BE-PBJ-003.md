# Laporan Perubahan Backend — `BE-PBJ-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-003` |
| Judul | Katalog dan master |
| Slice | `MVP-0` — fondasi (`EPIC PBJ-02`) |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-003` |
| Trace | `PBJ-DEC-022`, `024`, `031`, `033` (`G-07`), `PBJ-DEC-037`; FR-PBJ-010..012; `PBJ-VAL-070`..`072` |
| Contract version | `PBJ-PP-API` 1.0 → **1.1** dan `PBJ-PP-PERM` 1.1 → **1.2** (`PBJ-DEC-037`, `approved` 12 November 2026) |
| Dependency | `BE-PBJ-002` 🟡 (kode dan migration ada) |
| Klasifikasi | `MEDIUM` (berkas diperiksa 1, berkas diubah 2, logika 1, kontrak API 2, database 2) |
| Task mode | `BACKEND` (diberikan pemilik 12 November 2026, bersama `BE-PBJ-004`) |
| Target tulis | `Areas/Corporate/ProcurementManagement/MasterData/**`, konfigurasi EF, `ApplicationDbContext.cs`, `Program.cs` (seeder), extension DI, migration `AddProcurementCatalog`; basis data dev tidak disentuh |
| Model | Claude Opus 5.5 |
| Branch / commit | `sukmagp` @ `100c011d` (belum di-commit) |
| Tanggal | 12 November 2026 |
| Status | 🟡 **SEBAGIAN.** Kode, migration, dan seeder ada; build dan QBE `PASS`. Belum: uji runtime (`AT-PP-50`, `51`, `64`), karena migration tidak diterapkan dan aplikasi belum bisa start di basis data dev. `PBJ-VAL-071` baru dapat ditegakkan penuh setelah tabel pemakai ada (`BE-PBJ-006`/`009`) |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `MasterData` |
| Registry | Folder `MasterData` → baris `Master / Reference / MasterData` (`Mst`, `ACTIVE`), sesuai simulasi `PBJ-DEC-035` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs` dan `Program.cs` (penambahan baris) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-002` (`Mst*`), `QBE-CFG-001`, `QBE-MOD-001..003`, `QBE-SVC-001` (controller → service), `QBE-API-001`, `QBE-PERM-001`, `QBE-CODE-001..005` (kode `KTL` lewat `NumberSeriesAllocator`, unik di DB), `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-DEL-001` (soft delete + cek pemakai), `QBE-LOG-001` (audit `LoggerService` dengan aktor) |
| Penyimpangan dari kamus data | Kolom `SortOrder` pada `MstProcurementType`/`Category`/`FundSource` **tidak dibuat**: kontrak engineering melarang `SortOrder` generik yang dipersistensi untuk kode baru. Urutan memakai nama |
| Hasil checker | QBE `ReportOnly`, 41 berkas Pengadaan: **PASS** (0 temuan) |

## 1. Masalah yang diperbaiki

PR dan rencana wajib memilih barang dari katalog (`PBJ-DEC-022`), tetapi belum ada katalog, kategori, jenis
pengadaan, maupun sumber dana. Tanpa itu, kebutuhan yang sama dari unit berbeda tidak dapat digabung per
kode, dan pagu tidak punya kunci kategori.

## 2. Proses bisnis

1. Saat aplikasi start, seeder mengisi empat jenis pengadaan (Barang Stok, Aset, Barang Nonstok, Jasa),
   kategori `UMUM`, serta sumber dana `OPERASIONAL` dan `INVESTASI`. Seeder idempoten dan tidak menimpa
   perubahan admin.
2. Tim Pengadaan menambah kategori dan sumber dana sesuai kebutuhan. Kode diisi pengguna, disimpan huruf
   besar, dan unik.
3. Tim Pengadaan menambah barang ke katalog. Server membuat kode `KTL-000001` dst.
4. Form PR dan rencana mengambil pilihan barang aktif dari `GET /items/options`.

**Aturan dan contoh:**
- `PBJ-VAL-070`: "Kursi Roda Standar" / Aset / Umum / Unit sudah ada → menambah "kursi roda standar" dengan
  jenis, kategori, dan satuan yang sama ditolak (409) dengan menyebut kode yang sudah ada.
- `PBJ-VAL-072`: barang berjenis Jasa yang diberi rujukan obat ditolak (422).
- Kategori yang masih dipakai katalog tidak dapat dihapus (400); cukup dinonaktifkan.
- Jenis pengadaan hanya bisa dibaca; tidak ada endpoint ubah.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`master-data-endpoint-standard.md`, `role-access-rules.md`; pembanding `PettyCashCategoriesController`/`Service`/`Dtos`
(Finance); `RehabilitationMasterDataSeeder`; `RhbServiceSupport.AllocateNumberAsync`; `RunStartupSeederAsync` di
`Program.cs`; `MstMeasurement`, `MstDrug`; snapshot migration.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MasterData/Models/MstProcurementType.cs`, `MstProcurementItemCategory.cs`, `MstProcurementFundSource.cs`, `MstProcurementItem.cs` | **Baru.** Empat model master |
| `Repositories/Configurations/Corporate/ProcurementManagement/MasterData/*Configuration.cs` (4) | **Baru.** Kode unik terfilter `IsDelete = false`; `ItemCode` unik; FK katalog `Restrict` |
| `MasterData/DTOs/ProcurementMasterDataDtos.cs` | **Baru.** DTO referensi bersama (kategori, sumber dana), jenis, dan katalog |
| `MasterData/Services/ProcurementItemService.cs`, `ProcurementItemCategoryService.cs`, `ProcurementFundSourceService.cs`, `ProcurementTypeService.cs`, `ProcurementMasterDataExceptions.cs` | **Baru.** Satu service per master; eksepsi 422/409/400 |
| `MasterData/Controllers/ProcurementItemsController.cs`, `ProcurementItemCategoriesController.cs`, `ProcurementFundSourcesController.cs` | **Baru.** Sembilan endpoint baku master data |
| `MasterData/Controllers/ProcurementTypesController.cs` | **Baru.** Hanya baca: `GET /`, `/options`, `/{id}` |
| `MasterData/Seeders/ProcurementMasterDataSeeder.cs` | **Baru.** Data awal idempoten |
| `Common/Constants/ProcurementPermissions.cs` | Kode modul `MASTER_DATA`/`COMMON`, nama aksi, resource baru; `ProcurementMasterData` dihapus |
| `ProcurementManagementServiceCollectionExtensions.cs` | +4 service |
| `Repositories/ApplicationDbContext.cs` | +4 `DbSet` |
| `Program.cs` | +1 pemanggilan seeder |
| `Migrations/20261112140000_AddProcurementCatalog.cs` + `.Designer.cs`; snapshot | **Baru** / diperbarui |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Delta 1.0 → 1.1 (`PBJ-DEC-037`).** (a) Kategori dan sumber dana pindah ke controller sendiri (`/master-data/item-categories`, `/master-data/fund-sources`), menggantikan rute di bawah controller "Reference". (b) Ketiga master mendapat 9 endpoint baku: tambahan `/filters/metadata`, `/summary`, `PATCH /{id}/status`, `DELETE /{id}`. (c) `activate`/`deactivate` katalog diganti `PATCH /{id}/status` (bentuk baku). (d) `GET /items/options` kini ber-pagination. (e) Jenis pengadaan di `/master-data/procurement-types`, hanya baca |
| Database | 4 tabel baru, FK ke `MstMeasurement` dan `MstDrug` (`Restrict`). Migration **dibuat, tidak diterapkan** |
| Keamanan/Auth | Resource baru: `ProcurementItem`, `ProcurementItemCategory`, `ProcurementFundSource` (`Read`/`Create`/`Update`/`Delete`), `ProcurementType` (`Read`). `ProcurementMasterData` dihapus (`PBJ-PP-PERM` 1.2). Tidak ada nama peran di kode |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Master Data / Item

Base URL: `api/v1/corporate/procurement-management/master-data/items`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Konfigurasi saringan dan pilihan jenis | `ProcurementItem : Read` |
| `GET` | `/summary` | Total, aktif, nonaktif, barang, jasa, terkait obat | `ProcurementItem : Read` |
| `GET` | `/` | Daftar katalog (cari, aktif, jenis, kategori, urut, halaman) | `ProcurementItem : Read` |
| `GET` | `/options` | Pilihan barang aktif untuk form PR/rencana | `ProcurementItem : Read` |
| `GET` | `/{id}` | Detail | `ProcurementItem : Read` |
| `POST` | `/` | Tambah; kode `KTL-…` dibuat server | `ProcurementItem : Create` |
| `PUT` | `/{id}` | Ubah penuh | `ProcurementItem : Update` |
| `PATCH` | `/{id}/status` | Aktif/nonaktif | `ProcurementItem : Update` |
| `DELETE` | `/{id}` | Hapus lunak | `ProcurementItem : Delete` |

#### Corporate / Procurement Management / Master Data / Item Category

Base URL: `.../master-data/item-categories` — sembilan endpoint yang sama, resource `ProcurementItemCategory`.

#### Corporate / Procurement Management / Master Data / Fund Source

Base URL: `.../master-data/fund-sources` — sembilan endpoint yang sama, resource `ProcurementFundSource`.

#### Corporate / Procurement Management / Master Data / Procurement Type

Base URL: `.../master-data/procurement-types` — `GET /`, `GET /options`, `GET /{id}`, resource `ProcurementType : Read`.

**Kode status:** 404 tidak ditemukan atau sudah dihapus; 409 kode atau barang serupa sudah ada; 422 isian tidak
memenuhi aturan; 400 tidak dapat dihapus karena masih dipakai.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (perintah baku pemilik), bersama `BE-PBJ-004` | 0 error, 608 warning (= baseline), 0 warning dari berkas Pengadaan; 3 menit 46 detik | `PASS` | Log build |
| EF `migrations script 20261112130000_… 20261112150000_…` | 4 `CREATE TABLE` master, 4 FK `RESTRICT` (MstDrug, MstMeasurement, kategori, jenis), 3 index unik terfilter, `ItemCode` unik, 5 index lain | `PASS` | Keluaran skrip |
| Konsistensi snapshot (migration pemeriksa sementara, dihapus; snapshot dikembalikan) | Tidak memuat entity Pengadaan; sisa = drift HR lama (200 `AlterColumn`) | `PASS` | Log pemeriksaan |
| QBE (41 berkas) | 0 temuan | `PASS` | Laporan checker |
| Kesesuaian atribut akses | Setiap action memakai konstanta yang sama untuk `ControllerName`, argumen ke-1 `[AccessPermission]`, dan pasangan aksi | `PASS` (statis) | Review source |
| `AT-PP-50`, `51`, `64` | Tidak dijalankan | `NOT RUN` | Migration belum diterapkan (instruksi pemilik); aplikasi belum bisa start di basis data dev |

- **Uji manual:** `NOT FEASIBLE` saat ini.
- **AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Status migration dan basis data

`20261112140000_AddProcurementCatalog` dibuat, **tidak diterapkan**. Urutan: setelah `AddProcurementFoundation`.
Seeder `ProcurementMasterDataSeeder` berjalan saat startup. Bila tabelnya belum ada, ia gagal dengan pola yang
sama seperti seeder modul lain, jadi urutannya tetap database dulu, lalu aplikasi.

## 7. Risiko yang tersisa

| Risiko | Penanganan |
| --- | --- |
| `PBJ-VAL-071`: barang belum punya pemeriksaan pemakaian saat dihapus, karena tabel PR/rencana belum ada | Komentar wajib di `ProcurementItemService.DeleteAsync`; `BE-PBJ-006`/`009` menambah pemeriksaannya |
| Hapus sumber dana belum memeriksa pemakai | Idem untuk PR (`BE-PBJ-006`) dan pagu (`BE-PBJ-010`) |
| Unik nama barang (`PBJ-VAL-070`) dijaga service, bukan index ekspresi DB | Risiko balapan dua pengguna menambah barang serupa pada detik yang sama; dampak rendah (data master) |

## 8. Task berikutnya

`BE-PBJ-005` (gateway approval) setelah `BE-PBJ-001` dan `003`, serta `BE-PBJ-006` (PR draft).
