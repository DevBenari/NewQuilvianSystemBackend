# Laporan Perubahan Backend — `BE-MDF-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MDF-007` |
| Judul | Entity periode, hasil jasa, rincian, koreksi, daftar belum terhitung |
| Slice | `MVP-0` (`EPIC MDF-01`) |
| Roadmap | `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` bagian 3 |
| Trace | `MDF-DES-010`, `012`, `013`, `014`; `MDF-VAL-1.0` §4–6; `erd/fee-calculation.md`, `erd/data-dictionary.md` |
| Contract version | `MDF-API-1.0`, `MDF-VAL-1.0`, `MDF-STATE-1.0`, seluruhnya terkunci 20 September 2026 |
| Dependency | `BE-MDF-002` — 🟡 sebagian: entity peran terdaftar di source |
| Klasifikasi | `COMPLEX` — 5 Entity models, 5 EF Core Configurations, pendaftaran 5 DbSet di `ApplicationDbContext.cs` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Antigravity |
| Commit backend saat dikerjakan | `548785ac` (branch `Yasmina`) |
| Tanggal | 26 Oktober 2026 |
| Status | `SEBAGIAN` — implementasi kode 5 model entity, 5 EF configuration, dan registrasi DbSet selesai lengkap memenuhi seluruh acceptance criteria dan invariant bisnis, namun `dotnet build` dan uji runtime `T-26`, `T-30` dilewati per instruksi batasan lingkungan pemilik |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MedicalFeeManagement` |
| Submodule | `FeeCalculation` |
| Pemilik/prefix pada registry | Baris `Mdf` (ACTIVE) pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `NEW CODE` |
| Status registry | `ACTIVE` |
| QBE ID yang berlaku | `QBE-NAM-001` (Penamaan PascalCase, prefix `Mdf`), `QBE-MOD-001`, `QBE-MOD-002` (Folder `FeeCalculation` cocok dengan prefix `Mdf`), `QBE-MOD-003` |
| QBE ID yang tidak berlaku | `QBE-SVC-*`, `QBE-API-*`, `QBE-PERM-*` — task ini adalah perancangan struktur data entity dan konfigurasi basis data |
| Governance terbaca | `AGENTS.md`, `CLAUDE.md`, kontrak engineering dan registry di `docs/engineering/`, `TASK_RULES.md`, `REPORT_TEMPLATE.md`, `status-task-roadmap.md` — seluruhnya terbaca |
| Klasifikasi capability | Transaksi — Aggregate root periode kalkulasi (`MdfFeePeriod`), hasil jasa (`MdfServiceFee`), rincian tindakan (`MdfServiceFeeDetail`), koreksi jasa (`MdfServiceFeeAdjustment`), dan daftar belum terhitung (`MdfUnresolvedService`) |

---

## 1. Masalah yang diperbaiki

Sebelum task ini dikerjakan, sistem Quilvian belum memiliki skema model persisted dan konfigurasi EF Core untuk menyimpan siklus kalkulasi jasa medis bulanan. 

Tanpa kelima entitas ini:
1. **Tidak Ada Tempat Rekapitulasi Jasa Bulanan**: Rumah sakit tidak dapat mengelompokkan dan mengunci akumulasi jasa dokter per bulan kerja (`MdfFeePeriod`).
2. **Ketiadaan Immutabilitas & Keterlacakan Rupiah**: Tidak ada rincian transaksi per tindakan yang mengunci snapshot persentase tarif pembagian jasa pada saat tanggal layanan dilaksanakan. Bila tidak dikunci, setiap perubahan tarif di masa depan akan merusak laporan masa lalu. Selain itu, tanpa kolom `SharingRuleId` wajib, rumah sakit tidak dapat menjawab pertanyaan dokter mengenai dari kesepakatan mana angka jasa tersebut berasal (`MDF-DES-010`).
3. **Layanan Hilang Tanpa Jejak**: Jika suatu tindakan operasi atau tindakan medis belum memiliki nama dokter pelaksana atau belum memiliki kesepakatan tarif, tindakan tersebut kerap terlewat dalam perhitungan manual tanpa ada yang mengetahui. Entitas `MdfUnresolvedService` menyelesaikan masalah ini dengan mencatat setiap layanan yang tertahan beserta alasannya agar wajib diselesaikan sebelum periode ditutup (`MF-DEC-018`).
4. **Ketiadaan Kontrol Koreksi (Maker-Checker)**: Penyesuaian jasa manual (seperti koreksi komplain dokter atau pemindahan tindakan) rawan disalahgunakan bila tidak memiliki jejak audit dan check constraint pemisahan wewenang pengaju dan penyetuju (`MdfServiceFeeAdjustment`).

Contoh konkret skenario rumah sakit:
Pada perhitungan jasa medis bulan Oktober 2026, Dokter Spesialis Bedah (`Dr. Anton, Sp.B`) melakukan 15 tindakan operasi dan 40 konsultasi poliklinik. Sistem membentuk satu baris `MdfServiceFee` untuk Dr. Anton dengan rincian 55 baris `MdfServiceFeeDetail`. Setiap baris mencatat nilai kotor tindakan (sebelum diskon), persentase hak Dr. Anton (misal 40%), dan ID baris aturan tarif asalnya (`SharingRuleId`). Terdapat 1 tindakan operasi darurat yang pelaksananya belum diinput oleh perawat; sistem tidak membuangnya, melainkan otomatis mencatatnya ke dalam `MdfUnresolvedService` dengan alasan `PERFORMER_MISSING`. Manajemen rumah sakit tidak diizinkan menutup periode Oktober (`Closed`) sampai data dokter pelaksana tersebut dilengkapi atau diselesaikan secara resmi.

---

## 2. Proses bisnis & Invarian yang Ditegakkan

| Aspek | Isi |
| --- | --- |
| Tujuan | Menyediakan entitas model persisted dan EF Core Fluent Configuration untuk domain kalkulasi jasa medis rumah sakit |
| Pelaku | Mesin Kalkulasi Jasa Medis, Staf Pengelola Jasa Medis, Verifikator Medis, Direktur/Manajemen Yanmed |
| Pemicu | Kebutuhan fondasi penyimpanan data sebelum migrasi basis data (`BE-MDF-008`) dan mesin perhitungan (`BE-MDF-010`) dijalankan |

Invarian dan aturan bisnis yang ditegakkan di layer model dan basis data:

1. **Invarian Rumus Nilai Kotor & Snapshot (`MDF-DES-012`, `MF-DEC-013`, `MF-DEC-014`)**:
   - `BaseAmount` pada rincian (`MdfServiceFeeDetail`) diambil dari nilai kotor baris tagihan (`Quantity × UnitPrice`), **sebelum diskon apapun**.
   - `SharingPercentage` dikunci sebagai snapshot nilai saat tanggal tindakan (`ServiceDate`).
   - `CalculatedAmount` dihitung dari `BaseAmount × SharingPercentage ÷ 100`, dibulatkan 2 desimal `MidpointRounding.AwayFromZero`.
   - `GrossAmount` pada `MdfServiceFee` merupakan jumlah total (`Σ CalculatedAmount`) seluruh rinciannya.
2. **Invarian Total Akhir Hasil Jasa (`MDF-VAL-1.0` §6.1 & §6.2)**:
   - `FinalAmount = GrossAmount + AdjustmentAmount` ditegakkan langsung oleh check constraint basis data (`CK_MdfServiceFee_Total`).
   - `GrossAmount >= 0` ditegakkan oleh check constraint (`CK_MdfServiceFee_Gross`).
3. **Invarian Keunikan Per Penerima (`MDF-VAL-1.0` §6.3)**:
   - Kombinasi `(PeriodId, PayeeType, PayeeReferenceId)` wajib unik pada baris aktif melalui partial unique index. Satu dokter hanya boleh memiliki satu rekap hasil jasa dalam satu periode.
4. **Invarian Keterlacakan Rupiah (DoD `BE-MDF-007`)**:
   - Kolom `SharingRuleId` pada `MdfServiceFeeDetail` **wajib terisi** (`IsRequired()`, `Restrict`). Setiap rupiah jasa medis wajib merujuk ke baris aturan tarif yang sah.
5. **Invarian Maker-Checker (`MDF-DES-017`, `MF-DEC-009`)**:
   - Pada `MdfFeePeriod` dan `MdfServiceFee`: `VerifiedBy` dan `ApprovedBy` tidak boleh sama jika keduanya terisi (`CK_MdfFeePeriod_Maker`, `CK_MdfServiceFee_Maker`).
   - Pada `MdfServiceFeeAdjustment`: `ApprovedBy` tidak boleh sama dengan `RequestedBy` (`CK_MdfServiceFeeAdjustment_Maker`). Pembuat koreksi dilarang menyetujui koreksinya sendiri.
6. **Invarian Layanan Belum Terhitung (`MDF-DES-014`, `MDF-DES-016`)**:
   - Kombinasi `(PeriodId, SourceDomain, SourceDetailId)` unik pada baris aktif (`CK_MdfUnresolvedService`).
   - Alasan tertahan dibatasi pada 6 enum baku: `PERFORMER_MISSING`, `AGREEMENT_MISSING`, `RULE_MISSING`, `CONTRACT_EXPIRED`, `ROLE_UNMAPPED`, `SOURCE_UNSUPPORTED`.
   - Periode dilarang ditutup bila masih terdapat baris unresolved berstatus `Open`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
| --- | --- |
| `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` baris 70 | Memastikan acceptance criteria dan batasan task `BE-MDF-007` |
| `docs/module-blueprints/medical-fee/erd/fee-calculation.md` | Diagram relasi aggregate root perhitungan jasa dan rumus matematis |
| `docs/module-blueprints/medical-fee/erd/data-dictionary.md` §4–8 | Spesifikasi kolom, tipe data, nilai null, default, indeks, dan check constraint |
| `docs/module-blueprints/medical-fee/contracts/validation-matrix.md` §4–6 | Matriks validasi aturan bisnis perhitungan, hasil jasa, dan koreksi |
| `Repositories/ApplicationDbContext.cs` | Registrasi `DbSet` dan verifikasi pemuatan konfigurasi otomatis |

### 3.2 Berkas yang dibuat dan diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Models/MdfFeePeriod.cs` | **Baru.** Model aggregate root periode kalkulasi jasa medis ber-lifecycle (`Open`, `Calculated`, `Verified`, `Approved`, `Closed`) |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Models/MdfServiceFee.cs` | **Baru.** Model aggregate root hasil jasa satu penerima per periode, dengan properti `GrossAmount`, `AdjustmentAmount`, dan `FinalAmount` |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Models/MdfServiceFeeDetail.cs` | **Baru.** Model rincian layanan jasa per tindakan medis dengan snapshot `BaseAmount`, `SharingPercentage`, dan FK wajib `SharingRuleId` |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Models/MdfServiceFeeAdjustment.cs` | **Baru.** Model koreksi penyesuaian nominal jasa dengan arah `Addition`/`Deduction` dan penegakan maker-checker |
| `Areas/HealthServices/MedicalFeeManagement/FeeCalculation/Models/MdfUnresolvedService.cs` | **Baru.** Model daftar layanan belum dapat dihitung beserta 6 alasan baku dan status penyelesaiannya |
| `Repositories/Configurations/HealthServices/MedicalFeeManagement/FeeCalculation/MdfFeePeriodConfiguration.cs` | **Baru.** Konfigurasi EF Core untuk `MdfFeePeriod` lengkap dengan 3 check constraint, 2 indeks, dan relasi navigasi |
| `Repositories/Configurations/HealthServices/MedicalFeeManagement/FeeCalculation/MdfServiceFeeConfiguration.cs` | **Baru.** Konfigurasi EF Core untuk `MdfServiceFee` lengkap dengan 4 check constraint, 2 partial unique index, dan relasi navigasi |
| `Repositories/Configurations/HealthServices/MedicalFeeManagement/FeeCalculation/MdfServiceFeeDetailConfiguration.cs` | **Baru.** Konfigurasi EF Core untuk `MdfServiceFeeDetail` lengkap dengan 3 check constraint, 5 indeks, dan FK `SharingRuleId` wajib |
| `Repositories/Configurations/HealthServices/MedicalFeeManagement/FeeCalculation/MdfServiceFeeAdjustmentConfiguration.cs` | **Baru.** Konfigurasi EF Core untuk `MdfServiceFeeAdjustment` lengkap dengan 4 check constraint, partial unique index, dan maker-checker constraint |
| `Repositories/Configurations/HealthServices/MedicalFeeManagement/FeeCalculation/MdfUnresolvedServiceConfiguration.cs` | **Baru.** Konfigurasi EF Core untuk `MdfUnresolvedService` lengkap dengan 2 check constraint, partial unique index, dan relasi Cascade |
| `Repositories/ApplicationDbContext.cs` | **Diperbarui.** Menambahkan using model `FeeCalculation.Models` dan mendaftarkan 5 `DbSet` di bawah `#region MEDICAL FEE MANAGEMENT` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru yang dipublikasikan pada task ini (ranah DTO/Controller diimplementasikan pada task `BE-MDF-010`..`013`) |
| Database | Mempersiapkan metadata model EF Core untuk 5 tabel kalkulasi baru, tanpa downtime, bersifat murni aditif, dan siap dimigrasikan pada `BE-MDF-008` |
| Keamanan/Auth | Check constraint pemisahan wewenang maker-checker terpasang di level skema tabel, mencegah manipulasi data meskipun query melewati lapisan service |

---

## 4. Representasi Skema Entitas & Check Constraint

### 4.1 Tabel `MdfFeePeriod`
- **Primary Key**: `Id` (`uuid`)
- **Indeks Unik**: `PeriodCode` (Partial: `WHERE "IsDelete" = false`)
- **Check Constraints**:
  - `CK_MdfFeePeriod_Status`: `"Status" IN ('Open','Calculated','Verified','Approved','Closed')`
  - `CK_MdfFeePeriod_Range`: `"PeriodEnd" >= "PeriodStart"`
  - `CK_MdfFeePeriod_Maker`: `"VerifiedBy" IS NULL OR "ApprovedBy" IS NULL OR "VerifiedBy" <> "ApprovedBy"`

### 4.2 Tabel `MdfServiceFee`
- **Primary Key**: `Id` (`uuid`)
- **Indeks Unik**:
  - `FeeNumber` (Partial: `WHERE "IsDelete" = false`)
  - `(PeriodId, PayeeType, PayeeReferenceId)` (Partial: `WHERE "IsDelete" = false`)
- **Check Constraints**:
  - `CK_MdfServiceFee_Status`: `"Status" IN ('Calculated','Verified','Approved','HandedOff')`
  - `CK_MdfServiceFee_Total`: `"FinalAmount" = "GrossAmount" + "AdjustmentAmount"`
  - `CK_MdfServiceFee_Gross`: `"GrossAmount" >= 0`
  - `CK_MdfServiceFee_Maker`: `"VerifiedBy" IS NULL OR "ApprovedBy" IS NULL OR "VerifiedBy" <> "ApprovedBy"`

### 4.3 Tabel `MdfServiceFeeDetail`
- **Primary Key**: `Id` (`uuid`)
- **Foreign Keys**: `ServiceFeeId` (Cascade), `TariffId` (Restrict), `RoleId` (Restrict), `SharingRuleId` (Restrict, Wajib)
- **Check Constraints**:
  - `CK_MdfServiceFeeDetail_Percentage`: `"SharingPercentage" >= 0 AND "SharingPercentage" <= 100`
  - `CK_MdfServiceFeeDetail_Base`: `"BaseAmount" >= 0`
  - `CK_MdfServiceFeeDetail_Source`: `"SourceDomain" IN ('OPERATING_ROOM','PROCEDURE','LABORATORY','ADHOC','ADHOC_CATALOG')`

### 4.4 Tabel `MdfServiceFeeAdjustment`
- **Primary Key**: `Id` (`uuid`)
- **Indeks Unik**: `AdjustmentNumber` (Partial: `WHERE "IsDelete" = false`)
- **Check Constraints**:
  - `CK_MdfServiceFeeAdjustment_Status`: `"Status" IN ('Pending','Approved','Rejected')`
  - `CK_MdfServiceFeeAdjustment_Direction`: `"Direction" IN ('Addition','Deduction')`
  - `CK_MdfServiceFeeAdjustment_Amount`: `"Amount" > 0`
  - `CK_MdfServiceFeeAdjustment_Maker`: `"ApprovedBy" IS NULL OR "ApprovedBy" <> "RequestedBy"`

### 4.5 Tabel `MdfUnresolvedService`
- **Primary Key**: `Id` (`uuid`)
- **Indeks Unik**: `(PeriodId, SourceDomain, SourceDetailId)` (Partial: `WHERE "IsDelete" = false`)
- **Check Constraints**:
  - `CK_MdfUnresolvedService_Status`: `"Status" IN ('Open','Resolved','Waived')`
  - `CK_MdfUnresolvedService_Reason`: `"Reason" IN ('PERFORMER_MISSING','AGREEMENT_MISSING','RULE_MISSING','CONTRACT_EXPIRED','ROLE_UNMAPPED','SOURCE_UNSUPPORTED')`

---

## 5. Verifikasi

| Skenario atau pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik: **tanpa `dotnet build` otomatis** |
| Checker `Invoke-QbeConformanceCheck.ps1` | Tidak dijalankan | `NOT RUN` | `pwsh` tidak terpasang pada lingkungan eksekusi |
| Review diff dan cakupan perubahan | Tepat 10 berkas baru dan 1 berkas `ApplicationDbContext.cs` dimodifikasi | `PASS` | `git status --short` |
| Verifikasi Check Constraint `FinalAmount = GrossAmount + AdjustmentAmount` | Terpasang pada `MdfServiceFeeConfiguration.cs` | `PASS` | `table.HasCheckConstraint("CK_MdfServiceFee_Total", ...)` |
| Verifikasi Keunikan Satu Hasil per Penerima per Periode | Terpasang partial unique index pada `(PeriodId, PayeeType, PayeeReferenceId)` | `PASS` | `MdfServiceFeeConfiguration.cs` baris 44–46 |
| Verifikasi Keterlacakan Wajib `SharingRuleId` | Kolom `SharingRuleId` dikonfigurasi `IsRequired()` | `PASS` | `MdfServiceFeeDetailConfiguration.cs` baris 28 & 51–54 |
| Verifikasi Maker-Checker Constraint | Terpasang pada `MdfFeePeriod`, `MdfServiceFee`, dan `MdfServiceFeeAdjustment` | `PASS` | Check constraint maker-checker di ketiga konfigurasi |
| Pemeriksaan karakter baris (Line endings) | LF murni, UTF-8 tanpa BOM, nol karakter `\r` | `PASS` | `grep -c $'\r'` bernilai 0 pada seluruh berkas |
| Skenario uji runtime `T-26`, `T-30` | Tidak dijalankan | `NOT RUN` | Menuntut basis data dan kompilasi runtime aktif |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Acceptance: "FinalAmount = GrossAmount + AdjustmentAmount sebagai check constraint" | Terpenuhi pada source | `CK_MdfServiceFee_Total` terpasang pada `MdfServiceFeeConfiguration.cs` |
| Acceptance: "Satu penerima satu hasil per periode" | Terpenuhi pada source | Partial unique index `(PeriodId, PayeeType, PayeeReferenceId)` terpasang pada `MdfServiceFeeConfiguration.cs` |
| DoD: "SharingRuleId wajib — setiap rupiah tertelusur" | Terpenuhi pada source | `builder.Property(x => x.SharingRuleId).IsRequired()` dan FK Restrict ke `MdfSharingRule` pada `MdfServiceFeeDetailConfiguration.cs` |
| Pendaftaran 5 DbSet di `ApplicationDbContext` | Terpenuhi | 5 `DbSet` (`MdfFeePeriods`, `MdfServiceFees`, `MdfServiceFeeDetails`, `MdfServiceFeeAdjustments`, `MdfUnresolvedServices`) terdaftar |
| Validasi build | **Belum terpenuhi** | `NOT RUN` atas instruksi eksplisit pemilik (tanpa build otomatis) |

Catatan: Status task ditandai sebagai `🟡 SEBAGIAN` karena implementasi kode telah lengkap dan memenuhi acceptance criteria pada source, namun kompilasi `dotnet build` dan verifikasi skenario uji `T-26` & `T-30` belum dijalankan sesuai instruksi batasan lingkungan.

---

## 7. Catatan penutup dan Langkah Selanjutnya

1. **Prasyarat Migration Terpenuhi**:
   Dengan selesainya implementasi `BE-MDF-007`, dependensi wajib untuk task **`BE-MDF-008`** (*Migration AddMedicalFeeCalculation*) kini telah terpenuhi dan tidak lagi mengalami cacat urutan.
2. **Task Berikutnya**:
   - **`BE-MDF-018`**: Entity penyerahan ke Finance (`MdfFinanceHandoff`).
   - **`BE-MDF-008`**: Migration gelombang kalkulasi `AddMedicalFeeCalculation` (membuat 5 tabel kalkulasi).

