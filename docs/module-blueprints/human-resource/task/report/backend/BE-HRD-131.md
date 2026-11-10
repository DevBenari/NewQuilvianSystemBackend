# Laporan Penyelesaian Task Backend: BE-HRD-131

## 1. Informasi Task

| Atribut | Nilai |
| :--- | :--- |
| **ID Task** | `BE-HRD-131` |
| **Nama Task** | Fondasi skema database lembur, DDL PostgreSQL, dan indeks performa Slice `S-B3` |
| **Sub-Modul** | 09. Overtime Management (Administrasi Lembur Pegawai Rumah Sakit) |
| **Slice / Milestone** | `S-B3` · `MVP-27` |
| **Tanggal Selesai** | 2026-11-05 |
| **Status Konformansi** | ✅ Selesai (Memenuhi Seluruh Kriteria Penerimaan) |
| **Rujukan Keputusan** | `HRD-DEC-116` s.d. `HRD-DEC-121`, `HRD-ASM-23` s.d. `HRD-ASM-26`, `02-backend-architecture.md` Bagian 22 |

---

## 2. Deskripsi & Tujuan Bisnis

Dalam operasional rumah sakit 24 jam sehari dan 7 hari seminggu, lembur pegawai (*overtime*) merupakan instrumen penting untuk menjamin kesinambungan pelayanan medis darurat tanpa mengorbankan kepatuhan terhadap regulasi ketenagakerjaan Republik Indonesia (Peraturan Pemerintah No. 35 Tahun 2021).

Task ini menyediakan fondasi skema database yang tangguh, aditif, dan bebas henti (*zero-downtime*) untuk seluruh siklus administrasi lembur rumah sakit, meliputi:
1. **Pemisahan Kategori Lembur Dual-Mode (`HRD-DEC-116`)**: Mendukung pengajuan pra-dinas terencana (*Planned Overtime* minimal H-1) dan pelaporan klaim darurat medis pasca-dinas (*Emergency Cito Overtime* maksimal H+1) dengan kewajiban nomor rekam medis / insiden darurat.
2. **Pilihan Bentuk Kompensasi (`HRD-DEC-118`)**: Fleksibilitas memilih antara kompensasi finansial (*Financial Payout*) atau libur pengganti (*Compensatory Time Off / CTO*), dengan proteksi mutlak bagi pejabat struktural manajerial (*Overtime Exempt*).
3. **Eskalasi Darurat Klinis Direktur Pelayanan Medik (`HRD-DEC-117`)**: Penanda khusus dispensasi bila penyelamatan nyawa pasien menuntut jam lembur di atas kuota regulasi PP 35/2021 (4 jam/hari atau 18 jam/minggu).
4. **Verifikasi Silang Biometrik & Penanganan Anomali Discrepancy (`HRD-DEC-119`)**: Pencatatan tap mesin absensi fisik, selisih waktu, dan durasi sah yang disetujui HR.
5. **Penutupan Periode Lembur Bulanan & Rekonsiliasi Retrospektif (`HRD-DEC-121`)**: Penguncian snapshot cut-off tanggal 20 dan penanda rapel retrospektif pada penggajian bulan berikutnya.

---

## 3. Komponen Skema Database & Migrasi yang Dikerjakan

### 3.1 Ekstensi Model C# & Pemetaan EF Core

1. **`TrxOvertimePlan.cs` & `TrxOvertimePlanConfiguration.cs`**:
   - Menambahkan kolom `CompensationType` (`VARCHAR(50)`, default `'FinancialPayout'`).
   - Menambahkan kolom `IsEmergencyEscalation` (`BOOLEAN`, default `false`).
   - Menambahkan kolom `WeeklyQuotaHoursAtPlan` (`DECIMAL(5,2)` opsional).
   - Menambahkan indeks performa: `idx_ot_plan_date_unit` pada `("OrganizationUnitId", "PlanStartDate", "PlanStatus")`.

2. **`TrxOvertimeRealization.cs` & `TrxOvertimeRealizationConfiguration.cs`**:
   - Menambahkan kolom `OvertimeCategory` (`VARCHAR(50)`, default `'Planned'`).
   - Menambahkan kolom `EmergencyClinicalReferenceNo` (`VARCHAR(100)`).
   - Menambahkan kolom `ClinicalActionSummary` (`VARCHAR(2000)`).
   - Menambahkan kolom `CompensationType` (`VARCHAR(50)`, default `'FinancialPayout'`).
   - Menambahkan kolom `IsMedicalDirectorEscalation` (`BOOLEAN`, default `false`).
   - Menambahkan relasi foreign key `MedicalDirectorApprovedById` ke `MstWorkforceProfile` (`DeleteBehavior.Restrict`).
   - Menambahkan kolom `IsRetrospectiveAdjustment` (`BOOLEAN`, default `false`).
   - Menambahkan indeks performa: `idx_ot_realization_emp_date` pada `("WorkforceProfileId", "ActualStartDate", "RealizationStatus")`.

3. **`TrxOvertimeVerification.cs` & `TrxOvertimeVerificationConfiguration.cs`**:
   - Menambahkan kolom `BiometricCheckIn` & `BiometricCheckOut` (`TIMESTAMPTZ`).
   - Menambahkan kolom `BiometricExcessMinutes` & `VerifiedPayableMinutes` (`INTEGER`).
   - Menambahkan kolom `DiscrepancyReason` (`VARCHAR(255)`) & `ManualAdjustmentNotes` (`VARCHAR(2000)`).
   - Menambahkan relasi foreign key `VerifiedByHrAdminId` ke `MstWorkforceProfile` (`DeleteBehavior.Restrict`).
   - Menambahkan indeks performa: `idx_ot_verification_state` pada `("VerificationStatus", "IsAttendanceMatched", "HasVariance")`.

4. **`TrxCompensatoryTimeOff.cs` & `TrxCompensatoryTimeOffConfiguration.cs`**:
   - Menambahkan kolom `GrantedDays` (`DECIMAL(4,1)`).
   - Menambahkan kolom `RemainingHours` (`DECIMAL(5,2)`).
   - Menambahkan kolom `IsExpired` (`BOOLEAN`, default `false`).
   - Menambahkan indeks performa: `idx_cto_emp_expiry` pada `("WorkforceProfileId", "ExpiryDate", "IsExpired")`.

5. **`TrxOvertimePeriod.cs` & `TrxOvertimePeriodConfiguration.cs`**:
   - Menambahkan kolom `CutoffDate` (`DATE`).
   - Menambahkan kolom `TotalCtoGrantedHours` (`DECIMAL(6,2)`).
   - Menambahkan kolom `IsHandoffExecuted` (`BOOLEAN`, default `false`).
   - Menambahkan indeks performa: `idx_ot_period_lookup` pada `("StartDate", "EndDate", "PeriodStatus")`.

6. **Berkas Migrasi EF Core**:
   - [`Migrations/20261105140000_AddOvertimeSliceB3ExtensionsAndPerformanceIndices.cs`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Migrations/20261105140000_AddOvertimeSliceB3ExtensionsAndPerformanceIndices.cs): Mengimplementasikan skrip PostgreSQL idempoten berstruktur `IF NOT EXISTS` dan `CREATE INDEX IF NOT EXISTS`.

---

## 4. Contoh Skenario Penggunaan Nyata di Rumah Sakit

### Skenario: Penanganan Pasca-Dinas Cito Operasi Darurat
* **Kondisi**: Dokter bedah dan perawat anestesi menjalankan operasi darurat cito apendisitis perforasi pada pukul 21:00 hingga 02:00 (5 jam lembur, melebihi kuota 4 jam harian PP 35/2021).
* **Alur Sistem**:
  1. Pada H+1, perawat menginput klaim pasca-dinas dengan kategori `EmergencyCito`.
  2. Sistem mewajibkan pengisian Nomor Rekam Medis pasien (`EmergencyClinicalReferenceNo = "RM-2026-11-9981"`) dan ringkasan tindakan bedah (`ClinicalActionSummary = "Laparotomi emergensi apendisitis perforasi"`).
  3. Karena total jam (5 jam) melampaui batas harian 4 jam, kolom `IsMedicalDirectorEscalation` otomatis diatur menjadi `true`.
  4. Berkas klaim dialihkan ke antrean otorisasi eksekutif Direktur Pelayanan Medik sebelum diverifikasi presensi biometriknya.

---

## 5. Kepatuhan Kriteria Penerimaan

| Kriteria Penerimaan | Status | Bukti Implementasi |
| :--- | :---: | :--- |
| Skrip DDL PostgreSQL berhasil dieksekusi secara idempoten | ✅ Lolos | Migrasi `20261105140000` menggunakan klausa `ADD COLUMN IF NOT EXISTS` dan blok anonim `DO $$ BEGIN ... END $$;`. |
| Indeks performa `idx_ot_plan_date_unit`, `idx_ot_realization_emp_date`, `idx_ot_verification_state`, `idx_cto_emp_expiry`, dan `idx_ot_period_lookup` terpasang | ✅ Lolos | Seluruh 5 indeks terkonfigurasi di EntityTypeConfiguration dan terdaftar pada skrip migrasi. |
| Konfigurasi DbContext dan relasi navigasi model terdaftar dengan `DeleteBehavior.Restrict` | ✅ Lolos | Relasi navigasi `MedicalDirectorApprovedBy` dan `VerifiedByHrAdmin` dikonfigurasi `OnDelete(DeleteBehavior.Restrict)`. |
