# Laporan Perubahan Backend — `BE-AST-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-025` |
| Judul | Tabel proses dan migration `AddAssetValuationProcessing` |
| Slice | `NLI-S2` Penyusutan, perubahan nilai, kiriman, rekonsiliasi, laporan (gelombang `MVP-4`) |
| Roadmap | `nilai/roadmap/backend-roadmap.md` `REV-1`, kartu `BE-AST-025` |
| Trace | `data/data-dictionary.md` bagian 1, 2.3, 4.1, 4.2, 5; `02-backend-architecture.md` 5.4 (enum) |
| Contract version | `AST-NLI-1.0.0` — `approved` 2026-11-12 (`AST-DEC-051`). Task ini tidak menyentuh endpoint |
| Dependency | `BE-AST-015` 🟡 — kodenya ada; sisanya (`AT-NLI-07`) menunggu `BE-AST-027`, bukan prasyarat tabel |
| Klasifikasi | `MEDIUM` — 12 berkas, dampak entity/schema, kolom rupiah sensitif; tanpa logika bisnis, tanpa API, tanpa UI |
| Task mode | `BACKEND` — perintah Rizki 2026-11-24 "commit lalu lanjutkan pekerjaan" |
| Target tulis | `Areas/Corporate/AssetManagement/Valuation/{Enums,Models}/`; `Repositories/Configurations/Corporate/AssetManagement/Valuation/`; region DbSet `Repositories/ApplicationDbContext.cs`; laporan ini; baris status roadmap |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `ce9e1245` (branch `rizkiG`) |
| Tanggal | 24 November 2026 |
| Status | ✅ SELESAI 24 November 2026 — ketiga kriteria terbukti: build Rizki DLL 09:37:06, migration `20261124023459_AddAssetValuationProcessing` dibuat dan diterapkan Rizki; `Up()` = 5 `CreateTable`, 94 kolom, 12 FK `Restrict`, 16 `CreateIndex` (persis bagian 4); snapshot +531/−0 hanya lima entity; DB baca-saja cocok (bagian 3.1). **Riwayat:** 🟡 pada hari yang sama sebelum build dan migration |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Valuation` |
| Pemilik / prefix registry | `AssetManagement / Valuation / Nilai Aset`, prefix `Ast`, `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Keberlakuan | `NEW CODE` (+ `TOUCHED`: region DbSet) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-001`/`002` (prefix `Ast` terdaftar), `QBE-DB-*` untuk migration yang dibuat Rizki |
| Wewenang | Source saja. Pembuatan dan penerapan migration milik Rizki |

---

## 1. Masalah yang diperbaiki

Penyusutan bulanan, pengajuan perubahan nilai, akun kontrol subledger, dan pusat biaya per unit belum punya tempat
penyimpanan. Task `BE-AST-026` s.d. `BE-AST-031` membutuhkan kelima tabel ini lebih dulu.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Valuation/Enums/AssetValuationEnums.cs` | + `ValueChangeType` (1..4), `ValueChangeStatus` (1..4, bawaan `Diajukan`), `DepreciationAdjustmentReason` (`Tidak=0`..`Pemulihan=3`) |
| `Valuation/Models/AstDepreciationRun.cs` | Baru — badan hukum, periode, jumlah aset, total, penyesuaian, pemroses; koleksi `Lines` |
| `Valuation/Models/AstDepreciationRunLine.cs` | Baru — proses, aset, kelompok, unit akhir bulan, pusat biaya, nilai, alasan penyesuaian, bulan yang dikejar |
| `Valuation/Models/AstValueChangeRequest.cs` | Baru — nomor `AST-NLI-`, aset, jenis, nilai/umur/residu baru, faktur, bukti (`AstAttachment`), alasan, status, pengaju/pemutus, entri buku yang diterapkan, `RowVersion` |
| `Valuation/Models/AstSubledgerAccountSetup.cs` | Baru — tiga kode akun kontrol per kelompok aset |
| `Valuation/Models/AstUnitCostCenter.cs` | Baru — pusat biaya per unit organisasi |
| `Repositories/Configurations/Corporate/AssetManagement/Valuation/` | 5 configuration baru: presisi `numeric(18,2)`, `date`, `timestamp with time zone`, enum `int`, kolom audit, FK `Restrict`, index |
| `Repositories/ApplicationDbContext.cs` | + 5 DbSet di region `CORPORATE - ASSET MANAGEMENT - VALUATION` |

Tanpa baris komentar; configuration didaftarkan otomatis lewat `ApplyConfigurationsFromAssembly`.

### 2.1 Pemetaan kamus data

| Aturan kamus data | Source |
| --- | --- |
| Unik `(LegalEntityId, Period)` pada proses | `IX_AstDepreciationRun_Entity_Period` unik |
| Unik `(RunId, AssetId, AdjustmentReason)`; index `(RunId, AssetGroupId, CostCenterId)`; index `AssetId` | `IX_AstDepreciationRunLine_Run_Asset_Reason`, `_Run_Group_CostCenter`, index `AssetId` |
| `RequestNumber` unik | `IX_AstValueChangeRequest_RequestNumber` unik, `WHERE "IsDelete" = false` (pola nomor sesi opname) |
| Satu pengajuan terbuka per aset | `IX_AstValueChangeRequest_OpenPerAsset` unik `WHERE "Status" = 1 AND "IsDelete" = false` |
| `AssetGroupId` unik pada akun kontrol; `OrganizationUnitId` unik pada pusat biaya | `IX_AstSubledgerAccountSetup_AssetGroupId`, `IX_AstUnitCostCenter_OrganizationUnitId` |
| FK: `MstLegalEntity`, `AstAsset`, `MstAssetGroup`, `MstOrganizationUnit`, `MstCostCenter`, `AstAttachment`, `AstAssetValueEntry` | Semua `Restrict` |

### 2.2 Delta terhadap kamus data

| Delta | Alasan |
| --- | --- |
| Unik nomor pengajuan memakai filter `IsDelete = false` | Sama dengan nomor sesi opname dan dokumen aset lain; baris yang dihapus lunak tidak mengunci nomor |
| `ProcessedAt` bertipe `timestamp with time zone` (DDL menulis `timestamp`) | Semua kolom waktu modul Aset memakai `with time zone`; DDL kamus data bukan skrip (peringatannya tertulis di sana) |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (ExplicitFiles, 12 berkas) | `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Baris komentar baru | 0 (diff `ApplicationDbContext.cs` = 5 baris DbSet) | `PASS` |
| Model ↔ snapshot sebelum perubahan | 1.059 entity model (DbSet + configuration) seluruhnya ada di snapshot; selisih hanya 5 tipe generik ASP.NET Identity. Tidak ada tabel modul lain yang akan ikut tersapu | `PASS` |
| Build | DLL 2026-11-24 09:37:06 (Rizki) | `PASS` |
| Migration | `20261124023459_AddAssetValuationProcessing` (Rizki): `Up()` 5 `CreateTable` + 16 `CreateIndex`, tanpa operasi lain; `Down()` 5 `DropTable`; snapshot +531/−0, entity baru tepat lima | `PASS` |
| DB baca-saja | `QuilvianSystemFrontendDev/test-with-agy/asset-management/be-ast-025-20261124/cek-db.fsx` → `cek-db.txt` (3.1) | `PASS` |

### 3.1 DB `QuilvianNewDevRizki` (koneksi `default_transaction_read_only=on`)

| Pemeriksaan | Hasil |
| --- | --- |
| `__EFMigrationsHistory` | `20261124023459_AddAssetValuationProcessing` tercatat, migration terakhir (sebelumnya `AddAssetPhysicalInventory`, lalu migration HR `20261120081927`) |
| Kolom | Run 18, RunLine 19, ValueChangeRequest 29, SubledgerAccountSetup 15, UnitCostCenter 13 = 94 |
| Baris | 0 di kelima tabel |
| FK | 12, semua `ON DELETE RESTRICT` |
| Index | 16 + 5 PK; `IX_AstDepreciationRun_Entity_Period` unik `(LegalEntityId, Period)`; `IX_AstValueChangeRequest_OpenPerAsset` unik `WHERE Status = 1 AND IsDelete = false`; `IX_AstValueChangeRequest_RequestNumber` unik `WHERE IsDelete = false` |
| Tipe | `Period` `date`; `ProcessedAt` `timestamp with time zone`; rupiah `numeric(18,2)`; `RequestNumber` 30, `InvoiceNumber` 60, `Reason` 1000, kode akun 50 |

## 4. Harapan isi migration `AddAssetValuationProcessing`

Dipakai Rizki dan agent untuk memeriksa hasil `dotnet ef migrations add` sebelum `database update`.

| Tabel | Kolom (termasuk 10 audit) | FK | Index |
| --- | ---: | ---: | --- |
| `AstDepreciationRun` | 18 | 1 (`MstLegalEntity`) | 1: unik `(LegalEntityId, Period)` |
| `AstDepreciationRunLine` | 19 | 5 (`AstDepreciationRun`, `AstAsset`, `MstAssetGroup`, `MstOrganizationUnit`, `MstCostCenter`) | 6: unik `(RunId, AssetId, AdjustmentReason)`, `(RunId, AssetGroupId, CostCenterId)`, `AssetId`, + FK `AssetGroupId`, `OrganizationUnitId`, `CostCenterId` |
| `AstValueChangeRequest` | 29 | 3 (`AstAsset`, `AstAttachment`, `AstAssetValueEntry`) | 6: unik `RequestNumber` (filter), unik parsial `AssetId`, `ChangeType`, `Status`, + FK `EvidenceAttachmentId`, `AppliedEntryId` |
| `AstSubledgerAccountSetup` | 15 | 1 (`MstAssetGroup`) | 1: unik `AssetGroupId` |
| `AstUnitCostCenter` | 13 | 2 (`MstOrganizationUnit`, `MstCostCenter`) | 2: unik `OrganizationUnitId`, `CostCenterId` |
| **Jumlah** | **94** | **12** | **16** |

`Up()` semestinya hanya berisi 5 `CreateTable` dan 16 `CreateIndex` — tanpa `AlterColumn`, `AddColumn`, `DropTable`, atau
tabel modul lain (kriteria 1). Bila ada yang lain, hentikan sebelum `database update`.

Selama migration belum diterapkan, model EF lebih maju daripada skema; belum ada service yang membaca kelima tabel
ini, jadi tidak ada endpoint yang terdampak.

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Migration hanya menambah lima tabel | Model + configuration + DbSet; model ↔ snapshot bersih | `Up()` 5 `CreateTable` + 16 `CreateIndex` saja; snapshot +531/−0 | ✅ |
| 2. Unik `(LegalEntityId, Period)` pada proses | `AstDepreciationRunConfiguration` | `IX_AstDepreciationRun_Entity_Period` di `Up()` dan DB | ✅ |
| 3. Unik parsial satu pengajuan terbuka per aset | `AstValueChangeRequestConfiguration` (`Status = 1 AND IsDelete = false`) | `IX_AstValueChangeRequest_OpenPerAsset` di `Up()` dan DB | ✅ |

| DoD | Status |
|---|---|
| `V-BE` QBE | Terpenuhi |
| Build, migration, DB | Terpenuhi (build dan migration oleh Rizki, pemeriksaan oleh agent) |
| Laporan, roadmap | Ditulis; traceability tidak diubah (tidak ada baris requirement yang menunjuk `BE-AST-025`) |

---

## 6. Langkah berikutnya

1. Commit source, migration, laporan, dan roadmap (menunggu izin Rizki).
2. Berikutnya `BE-AST-026` (akun kontrol dan pusat biaya) — pasangan layarnya ada di roadmap frontend `nilai`.
