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
| Status | ✅ **SELESAI — diperbarui 16 November 2026.** `AT-PP-50`, `51`, `64` terbukti runtime; `PBJ-VAL-071` kini mencakup PR, rencana kebutuhan, rencana paket, dan pagu |

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

## Verifikasi lanjutan — 16 November 2026

Dijalankan bersama `BE-PBJ-007`..`011`: build Release `PASS` (0 error, 601 warning = baseline), QBE Strict `PASS` (53
berkas), `has-pending-model-changes` bersih, seluruh migration Pengadaan diterapkan ke `QuilvianNewDevSukma` atas izin
pemilik, dan uji runtime HTTP ke `https://localhost:7185` dengan aktor non-SuperAdmin `TEST-PBJ`.

**Fixture uji (`TEST-PBJ-20261116110649`, dibuat lewat API sebagai SuperAdmin, belum dibersihkan).** Lima unit uji
(`TEST-PBJ IGD/Radiologi/Pengadaan/Manajemen/Keuangan Uji`), tujuh jabatan, delapan akun `Employee` **tanpa role**
(`reqA`, `reqA2`, `headA`, `reqB`, `proc`, `mgr`, `dirk`, `keu`), dan policy Akses Role per unit × jabatan sesuai peta
peran `permission-audit-matrix.md` bagian 3. Konfigurasi approval uji (izin pemilik "nilai uji di dev"): definisi
`PBJ-*` diaktifkan, satu baris `MstApprovalMatrix` fallback `SpecificUser` per langkah, dan empat
`MstRejectionReason` — semuanya bertanda `DATA UJI PBJ (bukan nilai SOP)`. Komitmen pagu uji Rp10.000.000
(`SourceDocumentType = UJI_PBJ`) disisipkan lewat DB sesuai catatan `AT-PP-45`. Sisa fixture percobaan pertama
(`TEST-PBJ-20261116105253`) juga masih ada.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `A-01` | Tim Pengadaan membuat kategori uji | `PASS` | http=200; message=Kategori berhasil dibuat. |
| `A-02` | Nonaktifkan barang uji | `PASS` | http=200 |
| `AT-PP-50` | Tambah barang → kode KTL dari NumberSeries | `PASS` |  |
| `AT-PP-51a` | Barang serupa (beda huruf besar/kecil) → 409 PBJ-VAL-070 | `PASS` | http=409; code=None; message=Barang serupa sudah ada: KTL-000009 TEST-PBJ Kursi Roda Standar 1794802030. |
| `AT-PP-51b` | Hapus barang terpakai (PR, rencana, paket) / kategori terpakai / sumber dana terpakai → ditolak; data tetap ada (PBJ-VAL-071) | `PASS` |  |
| `AT-PP-62` | Skema: 14 tabel Pengadaan, index unik parsial, CHECK kuantitas/nominal, kolom ActivationConditionCode varchar(50) NULL | `PASS` | tables=14; uniquePartial={'IX_PrcNeedPlan_CurrentVersion': True, 'IX_PrcBudget_CurrentVersion': True, 'IX_PrcBudgetCommitment_ActiveSource': True, 'IX_PrcPurchaseRequest_RequestNumber': True, 'IX_PrcPackagePlan_PackagePlanNumber': True, 'IX_PrcNeedPlan_PlanNum |
| `AT-PP-64` | Seeder master + definisi PBJ-* dijalankan ulang (server start ulang 3×) → jumlah baris sama | `PASS` | before={'types': 4, 'umum': 1, 'funds': 2, 'wfl_defs': 5, 'wfl_steps': 11}; after={'types': 4, 'umum': 1, 'funds': 2, 'wfl_defs': 5, 'wfl_steps': 11} |

7 skenario, 7 `PASS`.

## Tambahan `PBJ-DEC-041` — 17 November 2026

Dikerjakan sesudah audit kesiapan (`testing/readiness-report.md`, temuan G-05) dan keputusan pengguna 17 November 2026:
hapus master yang sudah dipakai **tetap 400** sesuai standar endpoint master data, ditambah `errors.code`
`PBJ-VAL-071`. `validation-matrix.md` (1.1) dan `AT-PP-51` (`PBJ-PP-TEST` 1.1) diselaraskan ke 400.

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `MasterData` (`Mst`, `ACTIVE`) |
| Keberlakuan | `NEW CODE` (berkas milik task ini) |
| QBE yang berlaku | `QBE-API-001` (bentuk respons galat), `QBE-SVC-001`, `QBE-PERM-001` (tidak berubah) |

| Berkas | Perubahan |
| --- | --- |
| `MasterData/Services/ProcurementMasterDataExceptions.cs` | `ProcurementMasterInUseException` membawa `Code = "PBJ-VAL-071"` |
| `MasterData/Controllers/ProcurementItemsController.cs`, `ProcurementItemCategoriesController.cs`, `ProcurementFundSourcesController.cs` | Tangkapan `ProcurementMasterInUseException` mengirim `ApiResponse.Fail(400, pesan, new { code })`, pola yang sama dengan `PurchaseRequestsController` |

**Contoh respons:**

```json
{ "success": false, "statusCode": 400, "message": "Kategori ini tidak dapat dihapus karena masih dipakai katalog barang. Nonaktifkan saja bila tidak dipakai lagi.", "data": null, "errors": { "code": "PBJ-VAL-071" }, "timestamp": "…" }
```

| Verifikasi | Hasil | Klasifikasi |
| --- | --- | --- |
| Build | `dotnet build -c Release`: 0 error, 603 warning; 0 warning di `Areas/Corporate/ProcurementManagement` (build ulang `--no-incremental`) | `PASS` |
| QBE | `Invoke-QbeConformanceCheck.ps1 -Mode Strict`, 6 berkas yang disentuh `PBJ-DEC-041`/`042`: VIOLATION 0, REVIEW 0, INFO 0 | `PASS` |
| Migration / DB | Tidak ada perubahan entity atau migration. Uji memakai `QuilvianNewDevSukma` lewat backend uji `https://localhost:7185` | — |
| `AT-PP-51c` hapus kategori yang dipakai katalog | 400; `errors.code = PBJ-VAL-071`; pesan menyebut katalog barang | `PASS` |
| `AT-PP-51d` kategori tetap ada sesudah ditolak | `GET /{id}` 200 | `PASS` |
| `AT-PP-51e` regresi: kode kategori ganda | Tetap 409 (`PBJ-VAL-070`) | `PASS` |
| `AT-PP-51f` hapus barang yang tidak dipakai dokumen | 200 | `PASS` |
| `AT-PP-51g` hapus kategori yang sudah tidak dipakai | 200 (data uji terhapus lewat API) | `PASS` |

5 skenario, 5 `PASS` (`dec_041_042.py`, sesi 17 November 2026). Jalur barang terpakai PR/rencana/paket dan sumber dana
terpakai memakai kelas exception dan tangkapan controller yang sama; kasusnya sudah terbukti ditolak di `AT-PP-51b`
(16 November 2026), sehingga tidak diulang.

**Catatan DB uji (izin pengguna 17 November 2026).** Pembuatan unit baru di `QuilvianNewDevSukma` sempat gagal 500
(`23505 IX_MstDepartment_DepartmentCode`). Penyebabnya bug HR lama: `OrganizationController.GenerateDepartmentCodeAsync`
memilih nomor kosong terkecil hanya dari unit **aktif**, padahal index unik juga memuat unit yang sudah dihapus lunak.
Nomor `DPT-RSMMC-00030`..`00039` dipegang 10 unit uji `TEST-PBJ` yang dihapus lunak 16 November 2026. Dengan izin
pengguna, kode 10 unit uji itu diganti menjadi `DPT-RSMMC-000NN-DEL` (SQL `UPDATE`, 10 baris, hanya baris
`IsDelete = true` bernama `TEST-PBJ%`). Bug generator dilaporkan ke pemilik HR: setiap unit yang dihapus lunak akan
memblokir pembuatan unit berikutnya.
