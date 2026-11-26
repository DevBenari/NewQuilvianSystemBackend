# BE-PBJ-012 — Saringan tanggal dan periode daftar

| Field | Nilai |
| --- | --- |
| Task | `BE-PBJ-012` (roadmap backend revisi 2) |
| Trace | `PBJ-DEC-055`, `PBJ-FE-004`; `PBJ-PP-API` 1.4 (konvensi "Saringan tanggal daftar"); `PBJ-PP-VAL` 1.2 (`PBJ-VAL-094`); `AT-PP-65` |
| Dependency | `BE-PBJ-008` ✅, `BE-PBJ-011` ✅ |
| Branch / SHA awal | `sukmagp` @`c6b86a6e` |
| Tanggal | 17 November 2026 |
| Status | ✅ **SELESAI.** Build 0 error, QBE Strict `PASS` (27 berkas), runtime `AT-PP-65` 13/13 `PASS` |

## Apa yang dikerjakan

Semua daftar Pengadaan kini bisa disaring menurut **tanggal data dibuat**, lewat rentang tanggal atau periode cepat.
Saringan ini dibutuhkan baris filter standar `PBJ-FE-004` (Tanggal awal, Tanggal akhir, Periode).

**Contoh:** pada 17-11-2026, `GET /purchase-requests?customPeriod=thismonth` mengembalikan PR yang dibuat 01-11-2026
pukul 00.00 WIB sampai sebelum 01-12-2026 pukul 00.00 WIB. `GET /need-plans?startDate=2026-11-30&endDate=2026-11-01`
ditolak 422 `PBJ-VAL-094` "Tanggal akhir tidak boleh sebelum tanggal awal."

| Parameter | Arti |
| --- | --- |
| `startDate`, `endDate` | `yyyy-MM-dd`; tanggal akhir inklusif. Batas hari = 00.00 WIB (tanggal operasional aplikasi), lalu diubah ke UTC karena `CreateDateTime` disimpan UTC |
| `customPeriod` | `custom` (pakai `startDate`/`endDate`), `today`, `last7days`, `last30days`, `thismonth`, `lastmonth`. Selain `custom`, `startDate`/`endDate` diabaikan |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Common`, `Request`, `Planning`, `MasterData` |
| Registry | `Prc` `ACTIVE` (`PBJ-DEC-035`); folder `MasterData` → baris `Mst` |
| Keberlakuan | `NEW CODE` (seluruh berkas milik sub-modul ini) |
| Arketipe endpoint | Tidak ada endpoint baru; parameter baca tambahan pada `GET /` dan `GET /filters/metadata` yang sudah ada |
| QBE yang berlaku | `QBE-API-001`, `QBE-SVC-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PERM-001` (tidak berubah) |
| Hasil checker | QBE Strict `PASS` — 27 berkas, VIOLATION 0, REVIEW 0, INFO 0 |

## Endpoint yang berubah

#### Corporate / Procurement Management (10 daftar)

| Method | Path | Tambahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/procurement-management/purchase-requests` | `customPeriod` (tanggal sudah ada) | `ProcurementRequest : Read` |
| `GET` | `/v1/corporate/procurement-management/purchase-request-verifications` | `customPeriod` (tanggal sudah ada) | `ProcurementRequestVerification : Read` |
| `GET` | `/v1/corporate/procurement-management/purchase-request-verifications/stock-checks` | `startDate`, `endDate`, `customPeriod` (tanggal PR dibuat) | `ProcurementRequestVerification : Read` |
| `GET` | `/v1/corporate/procurement-management/need-plans` | `startDate`, `endDate`, `customPeriod` | `ProcurementNeedPlan : Read` |
| `GET` | `/v1/corporate/procurement-management/budgets` | `startDate`, `endDate`, `customPeriod` | `ProcurementBudget : Read` |
| `GET` | `/v1/corporate/procurement-management/package-plans` | `startDate`, `endDate`, `customPeriod` | `ProcurementPackagePlan : Read` |
| `GET` | `/v1/corporate/procurement-management/master-data/items` | `startDate`, `endDate`, `customPeriod` | `ProcurementItem : Read` |
| `GET` | `/v1/corporate/procurement-management/master-data/item-categories` | `startDate`, `endDate`, `customPeriod` | `ProcurementItemCategory : Read` |
| `GET` | `/v1/corporate/procurement-management/master-data/fund-sources` | `startDate`, `endDate`, `customPeriod` | `ProcurementFundSource : Read` |
| `GET` | `/v1/corporate/procurement-management/master-data/procurement-types` | `startDate`, `endDate`, `customPeriod` | `ProcurementType : Read` |
| `GET` | `…/filters/metadata` (PR, rencana, pagu, paket, katalog, kategori, sumber dana) | `customPeriods: [{ value, label, description, usesStartDate, usesEndDate }]` | `Read` masing-masing |

## Berkas

| Berkas | Perubahan |
| --- | --- |
| `Common/Services/ProcurementPeriodFilter.cs` (baru) | `Resolve` (isian → batas UTC dari tanggal operasional WIB), `WhereCreatedWithin<T>` untuk entity `IdentityModel`, `ProcurementPeriodRange`, `ProcurementPeriodFilterException` (`PBJ-VAL-094`) |
| `Common/DTOs/ProcurementPeriodFilterDtos.cs` (baru) | `ProcurementPeriodOptionResponse` + `Defaults()` (pola periode Master Data Supplier) |
| `MasterData/DTOs/ProcurementMasterDataDtos.cs`, `Planning/DTOs/BudgetDtos.cs`, `NeedPlanDtos.cs`, `PackagePlanDtos.cs`, `Request/DTOs/PurchaseRequestDtos.cs`, `PurchaseRequestVerificationDtos.cs` | Field `StartDate`/`EndDate`/`CustomPeriod` pada 7 query; `CustomPeriods` pada 6 respons metadata |
| 7 service daftar (`ProcurementItemService`, `ProcurementItemCategoryService`, `ProcurementFundSourceService`, `ProcurementTypeService`, `ProcurementBudgetService`, `ProcurementNeedPlanService`, `ProcurementPackagePlanService`) | Satu baris `WhereCreatedWithin(Resolve(...))` di `GetPagedAsync` |
| `Request/Services/ProcurementPurchaseRequestService.cs` | Saringan tanggal lama diganti helper; galat rentang `PBJ-VAL-007` → `PBJ-VAL-094` |
| `Request/Services/ProcurementPurchaseRequestVerificationService.cs` | Cek stok disaring menurut tanggal PR dibuat; antrean verifikasi mewarisi saringan daftar PR |
| 9 controller daftar | Tangkapan `ProcurementPeriodFilterException` → 422 `{ code: "PBJ-VAL-094" }`; `GET /` katalog, kategori, dan sumber dana kini lewat `ExecuteAsync`; `ProcurementTypesController.Get` diberi try/catch |

## Verifikasi

| Verifikasi | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build -c Release` | 0 error, 603 warning; 0 warning di `Areas/Corporate/ProcurementManagement` (`--no-incremental`) | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` (27 berkas) | VIOLATION 0, REVIEW 0, INFO 0 | `PASS` |
| Migration / DB | Tidak ada perubahan entity atau migration | — |
| Runtime `be_pbj_012_checks.py` ke `https://localhost:7185` (`QuilvianNewDevSukma`, SuperAdmin) | 13 skenario, tabel di bawah | `PASS` |

| ID | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| `AT-PP-65a` | 10 daftar × 6 nilai `customPeriod` | 60/60 → 200 | `PASS` |
| `AT-PP-65b` | Rentang terbalik (30-11 → 01-11) di 10 daftar | 10/10 → 422 `PBJ-VAL-094` | `PASS` |
| `AT-PP-65c` | `customPeriod=kemarin` di 10 daftar | 10/10 → 422 `PBJ-VAL-094` | `PASS` |
| `AT-PP-65d` | `filters/metadata` 7 endpoint | 6 pilihan periode, urutan sama | `PASS` |
| `AT-PP-65e` | Hari ini | Kategori uji baru masuk; `UMUM` (dibuat 16-11-2026 WIB) tidak | `PASS` |
| `AT-PP-65f` | Bulan ini | `UMUM` dan kategori baru masuk | `PASS` |
| `AT-PP-65g` | Bulan lalu | Keduanya tidak masuk | `PASS` |
| `AT-PP-65h` | Custom 16-11-2026 s.d. 16-11-2026 | `UMUM` masuk, kategori baru tidak | `PASS` |
| `AT-PP-65i` | Tanggal awal 17-11-2026 | `UMUM` tidak masuk, kategori baru masuk | `PASS` |
| `AT-PP-65j` | `lastmonth` + `startDate`/`endDate` | Tanggal diabaikan; hasil sama dengan Bulan lalu | `PASS` |
| `AT-PP-65k` | Katalog, Hari ini | Barang uji baru masuk | `PASS` |
| `AT-PP-65l` | Katalog, Bulan lalu | Barang uji baru tidak masuk | `PASS` |
| `AT-PP-65m` | Bersihkan data uji | Barang dan kategori uji dihapus lewat API (200) | `PASS` |

Daftar dokumen (PR, rencana, pagu, paket) di DB uji tidak punya baris aktif sesudah pembersihan 16 November 2026, sehingga
pembuktian isi saringan memakai data master. Kode penyaringnya sama (`WhereCreatedWithin`), dan kesepuluh daftar sudah
terbukti menerima parameter serta menolak isian salah.

## Delta dan risiko

| Butir | Penjelasan |
| --- | --- |
| Perubahan perilaku daftar PR | Batas hari saringan tanggal PR sebelumnya 00.00 **UTC**; kini 00.00 **WIB**. PR yang dibuat 00.00–07.00 WIB kini masuk ke tanggal WIB-nya. Ini perbaikan sesuai `AppDateTimeHelper.OperationalDateToUtc` |
| Kode galat rentang PR | `PBJ-VAL-007` (yang di matriks berarti "tanggal kebutuhan") diganti `PBJ-VAL-094`. Layar PR belum mengirim tanggal sebelum `FE-PBJ-009`, jadi tidak ada pemakai yang terdampak |
| Kotak masuk `Wfl` dan konsolidasi | Sengaja tidak disentuh (`PBJ-DEC-057`) |
| Masalah yang diketahui | `NONE` |

## Task berikutnya

`FE-PBJ-009` — baris filter satu baris dan hero tanpa tombol di semua layar daftar Pengadaan.
