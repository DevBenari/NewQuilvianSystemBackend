# Laporan Perubahan Backend — `BE-AST-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-020` |
| Judul | Tabel opname dan migration `AddAssetPhysicalInventory` |
| Gelombang | `MVP-3` — sub-modul `inventarisasi` |
| Roadmap | `inventarisasi/roadmap/backend-roadmap.md` — kartu `BE-AST-020` |
| Trace | `CAP-07.*`; `inventarisasi/data/data-dictionary.md` §2–7; `02-backend-architecture.md` §5.1, §5.4, §5.6, §7–8 |
| Contract version | `AST-INV-1.0.0` (bagian dari `AST-BP-001` rev 1, `approved` 2026-11-12) |
| Dependency | `BE-AST-004` [REG] ✅ (`AddAssetManagementFoundation`) |
| Klasifikasi | `STANDARD` — 6 entity baru, 4 enum, 6 configuration, migration aditif; tanpa endpoint |
| Task mode | `BACKEND`, diminta Rizki 2026-11-23 ("lanjut modul aset") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/PhysicalInventory/**`, `Repositories/Configurations/Corporate/AssetManagement/PhysicalInventory/**`, `ApplicationDbContext.cs` (6 DbSet); migration dibuat Rizki |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `c80d1e3c` (branch `rizkiG`), source belum di-commit |
| Tanggal | 23 November 2026 |
| Status | 🟡 SEBAGIAN 23 November 2026 — kriteria 1 dan 2 sudah ada di source (QBE Strict PASS); kriteria 3 menunggu Rizki merge integration, build, lalu `migrations add` + `database update` |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `PhysicalInventory` |
| Pemilik / prefix registry | `AssetManagement / PhysicalInventory / Inventarisasi Fisik`, prefix `Ast`, `ACTIVE` (registry 2026-11-12) |
| Keberlakuan | `NEW CODE` (+ `TOUCHED`: `ApplicationDbContext.cs`, 6 DbSet dan 1 `using`) |
| QBE yang berlaku | `QBE-ENT-001` (`IdentityModel`), `QBE-CFG-001` (configuration per entity), `QBE-MOD-001`/`002` (registry), `QBE-NAM-001` (prefix `Ast`), `QBE-DB-001` (migration aditif) |
| Tidak berlaku | `QBE-API-*`, `QBE-PERM-*`, `QBE-SVC-*`, `QBE-CODE-*`: task ini tidak membuat endpoint, service, atau penomoran |

---

## 1. Masalah yang diperbaiki

Sesi inventarisasi fisik belum punya tempat penyimpanan. `BE-AST-021` sampai `BE-AST-024` butuh enam tabel:
sesi beserta jejak pelaku per tahap, tim dengan satu ketua, snapshot aset yang diharapkan, pindaian, hasil
pencocokan, dan usulan tindak lanjut. Dua aturan dijaga database: satu unit hanya boleh punya satu sesi aktif, dan
satu sesi hanya punya satu ketua.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `PhysicalInventory/Enums/AssetPhysicalInventoryEnums.cs` | Baru — `StockTakeStatus`, `StockTakeResultCategory`, `StockTakeFollowUpType`, `StockTakeFollowUpStatus` (nilai sesuai arsitektur §5.4) |
| `PhysicalInventory/Models/AstStockTakeSession.cs` | Baru — sesi, `ExpectedCount`/`ScannedCount`, pelaku/waktu mulai, tutup, ajukan, verifikasi, batal, `RowVersion` |
| `PhysicalInventory/Models/AstStockTakeTeamMember.cs` | Baru — anggota tim, `IsLeader` |
| `PhysicalInventory/Models/AstStockTakeExpectedItem.cs` | Baru — snapshot ruangan, penanggung jawab, kondisi, unit peminjam |
| `PhysicalInventory/Models/AstStockTakeScan.cs` | Baru — pindaian/temuan, foto lewat `AstAttachment`, `IsLatest` bawaan `true` |
| `PhysicalInventory/Models/AstStockTakeResult.cs` | Baru — kategori, tiga penanda selisih, lokasi/kondisi harapan vs temuan, `TeamNote` |
| `PhysicalInventory/Models/AstStockTakeFollowUp.cs` | Baru — jenis, status bawaan `Usulan`, dokumen tujuan, pelaku proses/abaikan |
| `Repositories/Configurations/Corporate/AssetManagement/PhysicalInventory/*.cs` | Baru — 6 configuration; enum `HasConversion<int>()`; semua FK `Restrict`; `RowVersion` `IsConcurrencyToken()`; check `CK_AstStockTakeSession_Dates` |
| `Repositories/ApplicationDbContext.cs` | + region `PHYSICAL INVENTORY` 6 DbSet, + 1 `using` |

Tidak ada baris komentar pada berkas baru. Configuration dipasang lewat `ApplyConfigurationsFromAssembly`, jadi tidak
ada pendaftaran tambahan.

### 2.1 Index yang menjaga aturan

| Index | Definisi | Aturan |
| --- | --- | --- |
| `IX_AstStockTakeSession_ActivePerUnit` | `UNIQUE ("OrganizationUnitId") WHERE "Status" IN (2, 3, 4) AND "IsDelete" = false` | Satu sesi `Berjalan`/`Pencocokan`/`MenungguVerifikasi` per unit (`AST-RULE-023`) |
| `IX_AstStockTakeTeamMember_LeaderPerSession` | `UNIQUE ("SessionId") WHERE "IsLeader" = true AND "IsDelete" = false` | Paling banyak satu ketua per sesi; "tepat satu" dijaga service `BE-AST-021` (`AST-VAL-INV-004`) |
| `IX_AstStockTakeTeamMember_Session_Employee` | `UNIQUE ("SessionId", "EmployeeId") WHERE "IsDelete" = false` | Pegawai tidak tercatat dua kali dalam satu tim |
| `IX_AstStockTakeExpectedItem_Session_Asset` | `UNIQUE ("SessionId", "AssetId") WHERE "IsDelete" = false` | Satu baris snapshot per aset |
| `IX_AstStockTakeFollowUp_Result_Type` | `UNIQUE ("ResultId", "Type") WHERE "IsDelete" = false` | Satu hasil maksimal satu usulan per jenis |
| `IX_AstStockTakeSession_SessionNumber` | `UNIQUE ("SessionNumber") WHERE "IsDelete" = false` | Nomor `AST-OPN-######` |
| `IX_AstStockTakeScan_Session_Asset_Latest` | `("SessionId", "AssetId", "IsLatest")` | Baca pindaian terakhir (`AST-DES-029`) |

---

## 3. Isi migration yang diharapkan

Bagian ini dipakai Rizki untuk mencocokkan hasil `dotnet ef migrations add AddAssetPhysicalInventory`:

| Pemeriksaan | Harapan |
| --- | --- |
| `Up()` | Tepat **6 `CreateTable`** + **27 `CreateIndex`**; tidak ada `AddColumn`/`AlterColumn`/`DropColumn`/`CreateTable` tabel lain |
| `Down()` | 6 `DropTable` |
| Kolom | `AstStockTakeSession` 32, `AstStockTakeTeamMember` 14, `AstStockTakeExpectedItem` 17, `AstStockTakeScan` 24, `AstStockTakeResult` 23, `AstStockTakeFollowUp` 23 — total 133 (termasuk 10 kolom audit `IdentityModel` per tabel) |
| FK | 21, seluruhnya `ON DELETE RESTRICT`: sesi 2, tim 2, snapshot 5, pindaian 5, hasil 5, usulan 2 |
| Check constraint | `CK_AstStockTakeSession_Dates` (`"PlannedEndDate" >= "PlannedStartDate"`) |
| Snapshot | Bertambah tepat 6 entity `AstStockTake*` |

Index per tabel: sesi 5, tim 3, snapshot 5, pindaian 5, hasil 5, usulan 4. Index FK yang sudah tertutup index unik atau
komposit tidak dibuat ulang oleh EF, seperti pada `IX_AstLoanExtension_OpenPerLoan` di `AddAssetMovement`.

---

## 4. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (ExplicitFiles: 13 berkas baru + `ApplicationDbContext.cs`) | `Files evaluated: 14`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` |
| Bentrok nama | Tidak ada kelas `AstStockTake*` atau enum `StockTake*` lain di source; satu-satunya rujukan lama adalah `AstTransfer.SourceStockTakeResultId` (tanpa FK, sesuai kamus data pergerakan) dan `AssetStockTakeGate` | `PASS` |
| Model vs snapshot (statis: DbSet + `IEntityTypeConfiguration<>` vs `modelBuilder.Entity(...)` di snapshot) | `rizkiG`: 6 tabel task **+ `HrdOvertimeSetting`** (bagian 5.1). `origin/QuilvianIntegrationBackend` (`ab340bc7`): model dan snapshot selaras | `REVIEW` — selesai lewat merge integration |
| Build | Belum: dijalankan Rizki | `NOT RUN` |
| Migration dibuat dan diterapkan | Belum: wewenang Rizki | `NOT RUN` |

Cek statis membandingkan daftar entity, bukan kolom. Perubahan kolom pada tabel lama baru terlihat di isi `Up()`
hasil `migrations add`, jadi isi itu tetap diperiksa terhadap bagian 3.

---

## 5. Temuan

### 5.1 `HrdOvertimeSetting` tidak tercatat di snapshot `rizkiG`

Model `HrdOvertimeSetting` (OvertimeManagement, MHamzah1, `e750883d` 2026-11-19) dibuat lewat migration SQL mentah
`20261118100000_AddHrdOvertimeSetting`, yang tidak memperbarui snapshot di `rizkiG`. Snapshot integration sudah memuat
entity ini. Kalau `migrations add` dijalankan di `rizkiG` sekarang, migration aset ikut membuat `HrdOvertimeSetting`,
kasus yang sama dengan `MstJobDescription` pada `AddAssetMovement` (19 Nov).

### 5.2 Cabang tertinggal dari integration

Pada 23 Nov `rizkiG` 2 commit di depan dan **11 commit di belakang** `origin/QuilvianIntegrationBackend`. Integration
membawa satu migration Payroll baru (`...AddPayGradeAndStructureToTransportAllowancePolicy`), perubahan snapshot, 1 baris
`ApplicationDbContext.cs`, dan 5 berkas model/configuration Payroll/HR. Merge integration lebih dulu menyelesaikan 5.1
dan mencegah snapshot bercabang; urutan ini sama dengan `BE-AST-004` (16 Nov) dan `BE-AST-016` (19 Nov).

### 5.3 Delta terhadap blueprint

- Arsitektur §6 menulis `Enums/ 4` (empat berkas). Keempat enum ditaruh dalam satu berkas
  `AssetPhysicalInventoryEnums.cs`, sama dengan `AssetMovementEnums.cs`, `AssetRegisterEnums.cs`, dan
  `AssetValuationEnums.cs`.
- Diagram kelas §4 menulis `AssetCondition? ObservedCondition` pada `AstStockTakeScan`. Kamus data §5 (dasar AC-1)
  mewajibkannya, jadi kolom dibuat `NOT NULL`.
- Kamus data §8 menulis `timestamp` untuk `ScannedAt`. Yang dipakai `timestamp with time zone`, sesuai seluruh kolom
  waktu modul Aset.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Enam tabel sesuai data dict | 6 model + 6 configuration | QBE PASS; bukti DB menyusul migration | 🟡 source ada |
| 2. Unik parsial satu sesi aktif per unit; satu ketua per sesi | `IX_AstStockTakeSession_ActivePerUnit`, `IX_AstStockTakeTeamMember_LeaderPerSession` | Configuration; bukti DB menyusul | 🟡 source ada |
| 3. Migration hanya menambah enam tabel | — | Menunggu Rizki; harapan di bagian 3 | belum |

| DoD | Status |
|---|---|
| `V-BE` QBE | Terpenuhi |
| Build, migration diterapkan, isi DB diperiksa | Belum, menunggu Rizki |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Belum |

---

## 7. Langkah berikutnya

1. Rizki: merge `origin/QuilvianIntegrationBackend` ke `rizkiG`, lalu `dotnet build -p:RunAnalyzers=false`.
2. Agent: ulangi cek model↔snapshot. Harapannya selisih tinggal 6 entity `AstStockTake*`.
3. Rizki: `dotnet ef migrations add AddAssetPhysicalInventory --no-build`, lalu `dotnet ef database update` (sekaligus
   menerapkan migration Payroll dari integration bila belum ada di DB).
4. Agent: periksa isi `Up()`/`Down()` terhadap bagian 3 dan DB baca-saja, lalu naikkan status ke ✅.
5. Task berikutnya: `BE-AST-021` (sesi jadwal/mulai/batal dan gerbang penahanan) bersama `FE-AST-015`.

Selama migration belum diterapkan, model EF lebih maju daripada skema. Tidak ada endpoint yang membaca tabel baru,
jadi tidak ada endpoint yang terdampak.
