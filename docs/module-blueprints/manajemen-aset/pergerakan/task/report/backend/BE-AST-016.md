# Laporan Perubahan Backend — `BE-AST-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-016` |
| Judul | Tabel pergerakan dan migration `AddAssetMovement` |
| Gelombang | `MVP-2` — sub-modul `pergerakan` |
| Roadmap | `pergerakan/roadmap/backend-roadmap.md` — kartu `BE-AST-016` |
| Trace | `CAP-05.*`; `pergerakan/data/data-dictionary.md` §2–6; `02-backend-architecture.md` §5.1, §5.4, §5.6, §8; `02-module-map.md` §3.2 urutan 2 |
| Contract version | `AST-MOV-1.0.0` (bagian dari `AST-BP-001` rev 1, `approved` 2026-11-12) |
| Dependency | `BE-AST-004` [REG] ✅ (`AddAssetManagementFoundation`) |
| Klasifikasi | `STANDARD` — 5 entity baru, 4 enum, 5 configuration, migration aditif; tanpa endpoint |
| Task mode | `BACKEND`, diminta Rizki 2026-11-19 ("lanjutkan langkah selanjutnya") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/Movement/**`, `Repositories/Configurations/Corporate/AssetManagement/Movement/**`, `ApplicationDbContext.cs` (5 DbSet); migration dibuat Rizki |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `9514ed6b` (model, branch `rizkiG`); migration belum di-commit saat laporan ditulis |
| Tanggal | 19 November 2026 |
| Status | ✅ SELESAI 19 November 2026 — ketiga kriteria terpetakan ke source dan dibuktikan di DB; migration ikut membuat dua tabel HR (`MstJobDescription`, `MstJobDescriptionItem`) yang modelnya belum punya migration, dan **Rizki memutuskan migration dipakai apa adanya** dengan memberi tahu pemiliknya (bagian 5.3) |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Movement` |
| Pemilik / prefix registry | `AssetManagement / Movement / Pergerakan Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` (+ `TOUCHED`: `ApplicationDbContext.cs`, 5 DbSet dan 1 `using`) |
| QBE yang berlaku | `QBE-ENT-001` (`IdentityModel`), `QBE-CFG-001` (configuration per entity), `QBE-MOD-001`/`002` (registry), `QBE-NAM-001` (prefix `Ast`), `QBE-DB-001` (migration aditif) |
| Tidak berlaku | `QBE-API-*`, `QBE-PERM-*`, `QBE-SVC-*`, `QBE-CODE-*` — tanpa endpoint, service, atau penomoran |

---

## 1. Masalah yang diperbaiki

Mutasi antar unit dan peminjaman aset belum punya tempat penyimpanan. `BE-AST-017` (mutasi) dan `BE-AST-018`
(peminjaman) butuh lima tabel dengan jejak pelaku per tahap, token konkurensi, dan aturan yang dijaga database:
unit asal ≠ tujuan, tanggal kembali ≥ tanggal mulai, dan paling banyak satu perpanjangan terbuka per pinjaman.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Movement/Enums/AssetMovementEnums.cs` | Baru — `TransferStatus`, `TransferReason`, `LoanStatus`, `LoanExtensionStatus` (nilai sesuai arsitektur §5.4) |
| `Movement/Models/AstTransfer.cs`, `AstTransferItem.cs` | Baru — mutasi beserta pelaku/waktu per tahap, `IsUnitApprovalByRequester`, `CorrectsTransferId` (FK ke diri sendiri), `SourceStockTakeResultId`, `RowVersion` |
| `Movement/Models/AstLoan.cs`, `AstLoanItem.cs`, `AstLoanExtension.cs` | Baru — pinjaman, `OriginalPlannedReturnDate`, `IsApprovalByRequester`, `RowVersion`, perpanjangan |
| `Repositories/Configurations/Corporate/AssetManagement/Movement/*.cs` | Baru — 5 configuration; enum `HasConversion<int>()`; semua FK `Restrict`; `RowVersion` `IsConcurrencyToken()` |
| `Repositories/ApplicationDbContext.cs` | + region `MOVEMENT` 5 DbSet |
| `Migrations/20261119042534_AddAssetMovement.cs` (+ Designer, snapshot) | Dibuat dan diterapkan Rizki |

Nol baris komentar pada berkas baru.

---

## 3. Isi migration dan bukti database

Dicek dari berkas migration dan dari DB dev (`QuilvianNewDevRizki`) dengan koneksi baca-saja
(`default_transaction_read_only=on`):

| Pemeriksaan | Hasil |
| --- | --- |
| `__EFMigrationsHistory` | `20261119042534_AddAssetMovement` tercatat |
| Isi `Up()` | 7 `CreateTable` + 25 `CreateIndex`; tanpa `AlterColumn`/`DropColumn`/`AddColumn` pada tabel lama |
| Isi `Down()` | 7 `DropTable` |
| Tabel Ast | `AstTransfer` 36 kolom, `AstTransferItem` 15, `AstLoan` 36, `AstLoanItem` 15, `AstLoanExtension` 21 |
| Check constraint | `CK_AstTransfer_DifferentUnits`, `CK_AstLoan_DifferentUnits`, `CK_AstLoan_ReturnAfterStart` |
| FK | 12 FK, seluruhnya `ON DELETE RESTRICT` (`r`) |
| Index unik parsial | `IX_AstLoanExtension_OpenPerLoan` — `UNIQUE ("LoanId") WHERE "Status" = 1 AND "IsDelete" = false` |
| Snapshot | Bertambah 5 entity Ast + 2 entity HR (bagian 5.3); sebelum migration, snapshot `rizkiG` identik dengan `origin/QuilvianIntegrationBackend` (2.251 blok) |

---

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (WorkingTree, sebelum commit `9514ed6b`) | `Files evaluated: 12`, `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Bentrok nama enum/kelas | Tidak ada `TransferStatus`, `LoanStatus`, `AstTransfer`, `AstLoan` lain di source | `PASS` |
| Build | DLL 2026-11-19 11:27:30 (dibangun saat `migrations add`) memuat model | `PASS` |
| Migration diterapkan | Lihat bagian 3 | `PASS` |

---

## 5. Temuan

### 5.1 Cabang tertinggal dari integration

Sebelum migration, `rizkiG` tertinggal 36 commit dari `origin/QuilvianIntegrationBackend` (3 migration Billing/Payroll,
`ApplicationDbContext.cs` ikut berubah). Rizki menggabungkan integration lebih dulu (PR #360) dan menjalankan
`database update`, lalu migration dibuat. Dengan urutan ini snapshot tidak bercabang.

### 5.2 Lima berkas migration HR tanpa atribut

`20261109110000..150000` (Benefit, HR Service, Credentialing, MHamzah1) tidak memiliki `[Migration]`/Designer
sehingga tidak dikenali EF dan tidak tercatat di riwayat DB. Sudah begitu sejak sebelum 16 Nov; tidak menyangkut task
ini dan tidak diubah.

### 5.3 Migration ikut membuat tabel HR

Model `MstJobDescription` dan `MstJobDescriptionItem` (HRD-05, MHamzah1, `974f7208a` 2026-11-16) masuk ke model EF
tanpa migration di branch mana pun. Akibatnya `dotnet ef migrations add AddAssetMovement` ikut membuat kedua tabel itu
(DB dev: 19 dan 15 kolom, 0 baris). **Keputusan Rizki 2026-11-19: migration dipakai apa adanya dan Hamzah dikabari**
supaya migration HR tidak membuat kedua tabel itu lagi. Risiko yang diterima: bila Hamzah sudah lebih dulu membuat
migration sendiri di integration, saat merge akan ada dua migration yang membuat tabel yang sama dan salah satunya
harus dihapus.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Lima tabel sesuai data dict, check constraint unit berbeda dan tanggal kembali ≥ mulai | 5 model + configuration | DB: 5 tabel, 3 check constraint | ✅ |
| 2. `RowVersion` pada mutasi dan pinjaman | `AstTransfer.RowVersion`, `AstLoan.RowVersion`, `IsConcurrencyToken()` | Configuration | ✅ |
| 3. Migration hanya menambah lima tabel; `Down()` menghapusnya | `20261119042534_AddAssetMovement` | Menambah 5 tabel Ast **+ 2 tabel HR** (5.3); `Down()` menghapus ketujuhnya; tidak mengubah tabel lama | ✅ dengan pengecualian disetujui Rizki |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, migration diterapkan, isi DB diperiksa | Terpenuhi |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Model `9514ed6b`; migration ikut commit bersama `BE-AST-017` |

---

## 7. Catatan penutup

- **Untuk Hamzah:** tabel `MstJobDescription` dan `MstJobDescriptionItem` sudah dibuat oleh migration
  `20261119042534_AddAssetMovement` di `rizkiG`. Migration HRD-05 jangan membuat kedua tabel itu lagi.
- **Task berikutnya:** `BE-AST-017` (mutasi antar unit) bersama `FE-AST-012`.
