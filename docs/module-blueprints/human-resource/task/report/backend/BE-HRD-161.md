# Laporan Penyelesaian Backend Task: BE-HRD-161

## 1. Identitas Task
* **ID Task**: `BE-HRD-161`
* **Nama Task**: Fondasi skema database restitusi klaim & pemotongan penggajian (6 entitas DDL & migrasi EF Core)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-33: Fondasi Database, Kebijakan Plafon, Engine Guard 50% & Sinkronisasi Finance`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam operasional rumah sakit modern, pengelolaan hak keuangan tenaga medis dan staf pendukung memerlukan integrasi erat antara penggajian bulanan (Sub-modul 10), piutang/pinjaman rumah sakit di Modul Keuangan (Finance), perlindungan batas upah layak tenaga kerja sesuai perundang-undangan (Pasal 65 PP 36/2021 jo PP 49/2025), serta kompensasi pengembalian dana restitusi klaim kesehatan mandiri maupun salah potong masa lalu (`HRD-DEC-128` s.d. `HRD-DEC-133`).

Sebelum logika kalkulasi pemotongan dan alur persetujuan klaim dapat dieksekusi, sistem membutuhkan fondasi persistensi data yang kuat, aman, dan berintegritas tinggi untuk:
1. **Konfigurasi Master Kebijakan Plafon Restitusi & Pemotongan (`HrdRestitutionSetting`)**: Menyimpan batas maksimal pemotongan upah (hard-cap default 50% Gross Pay) dan plafon tahunan reimbursement benefit per golongan/grade staf RS.
2. **Klaim Restitusi Mandiri Pegawai di ESS (`TrxEmployeeRestitutionClaim`)**: Menampung pengajuan reimbursement kacamata kerja, faskes luar darurat, atau dinas darurat medis dengan bukti nota/kuitansi digital dan alur verifikasi berjenjang.
3. **Komponen Pengembalian Gaji Restitusi (`TrxPayrollRestitution`)**: Menampung klaim restitusi approved untuk diinjeksi ke slip gaji bulanan, serta memproses kompensasi salah potong periode lampau berstatus non-pajak PPh 21 tanpa mengubah lembar slip gaji final lama (`PAY-B6`).
4. **Master Jadwal Cicilan Pemotongan Gaji (`TrxPayrollDeductionSchedule`)**: Mengelola jadwal angsuran cicilan piutang/pinjaman yang ditarik dari Modul Keuangan (Finance) lengkap dengan hirarki prioritas potongan 1 s.d. 4.
5. **Rincian Eksekusi Potongan Gaji per Periode (`TrxPayrollDeductionItem`)**: Mencatat realisasi nominal terpotong aktual, nominal ditunda (*carry-over*) akibat mentok batas 50%, dan alasan penundaan resmi.
6. **Voucher Pencairan Kasir Darurat Cito 24/7 (`TrxCashierDisbursementVoucher`)**: Menampung voucher klaim darurat medis nakes cito ber-token SHA-256 kedaluwarsa 48 jam untuk pencairan tunai langsung di loket kasir rumah sakit.

---

## 3. Entitas & Konfigurasi Basis Data EF Core yang Dibuat

### A. Model Data Baru
1. `Areas/Corporate/HumanResource/PayrollManagement/Models/HrdRestitutionSetting.cs`:
   - Menyimpan `GradeLevelCode`, `BenefitCategory`, `AnnualLimitAmount`, `MinimumServiceMonthsRequired`, `MaxDeductionPercentageOfGross` (default 50.00%), `AllowEmergencyCitoCashier`, dan status `IsActive`.
2. `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxEmployeeRestitutionClaim.cs`:
   - Menyimpan nomor klaim unik `ClaimNumber`, relasi ke pegawai `EmployeeId`, kategori klaim, tanggal pengeluaran `ExpenseDate`, nominal klaim dan nominal disetujui, URL lampiran nota `ReceiptAttachmentUrl`, resume medis, status alur klaim, catatan alasan, serta riwayat approval (Supervisor, HR Reviewer, Final Approver).
3. `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollRestitution.cs`:
   - Menyimpan `RestitutionNumber`, relasi ke pegawai, tautan ke klaim asal `ClaimId`, tipe restitusi (`BenefitReimbursement` atau `SalaryAdjustmentCorrection`), nominal pengembalian, referensi slip gaji masa lalu yang salah potong (`AffectedPayrollRunId`, `AffectedPayslipId`), target payroll run tempat dana dicairkan, flag non-objek pajak `IsTaxable = false`, serta status.
4. `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollDeductionSchedule.cs`:
   - Menyimpan `ScheduleNumber`, relasi ke pegawai, kategori potongan (`GantiRugiAsetKasir`, `KasbonPinjamanRS`, `CicilanKoperasi`, `IuranPaguyuban`), tingkat prioritas (`PriorityLevel` 1 s.d. 4), kode piutang eksternal Finance `ExternalReferenceId`, pokok pinjaman, angsuran bulanan, sisa pokok, tanggal mulai/selesai, status, dan flag `AutoCarryOverOnCapExceeded = true`.
5. `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollDeductionItem.cs`:
   - Menyimpan relasi ke `PayrollRunId`, jadwal cicilan `DeductionScheduleId`, pegawai, kategori potongan, prioritas, nominal terjadwal `ScheduledAmount`, nominal terpotong aktual `ActualDeductedAmount`, nominal ditunda `DeferredAmount`, status eksekusi (`DeductedInFull`, `PartiallyDeducted`, `DeferredDueToSalaryCap`), alasan penundaan `DeferralReason`, dan tanggal eksekusi.
6. `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxCashierDisbursementVoucher.cs`:
   - Menyimpan nomor voucher unik `VoucherNumber`, relasi ke klaim cito dan pegawai, nominal voucher, hash token keamanan SHA-256 `SecurityTokenHash`, tanggal terbit dan tanggal kedaluwarsa 48 jam `ExpiredAt`, status voucher (`Issued`, `ClaimedAtCashier`, `DisbursedViaCashier`, `Expired`, `Cancelled`), user kasir penjamin `CashedByUserId`, waktu pencairan, dan nomor bukti kasir.

### B. Konfigurasi Entity Type Fluent API
Dibuat 6 berkas konfigurasi EF Core di `Repositories/Configurations/Corporate/HumanResource/PayrollManagement/`:
* `HrdRestitutionSettingConfiguration.cs`
* `TrxEmployeeRestitutionClaimConfiguration.cs`
* `TrxPayrollRestitutionConfiguration.cs`
* `TrxPayrollDeductionScheduleConfiguration.cs`
* `TrxPayrollDeductionItemConfiguration.cs`
* `TrxCashierDisbursementVoucherConfiguration.cs`

### C. Pendaftaran DbContext & Skrip Migrasi
* Pendaftaran 6 `DbSet` pada `Repositories/ApplicationDbContext.cs`.
* Berkas migrasi resmi EF Core: `Migrations/20261106153000_AddRestitutionAndPayrollDeductionSliceB6Core.cs`.

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 (Klaim Kacamata Perawat ICU)**:
  Suster Nani (Grade II, masa kerja 2 tahun) mengajukan klaim penggantian lensa kacamata anti-radiasi sebesar Rp 1.500.000 dengan melampirkan kuitansi optik resmi. Sistem mencatatnya ke `TrxEmployeeRestitutionClaim` dengan nomor `CLM-202611-001234` untuk diverifikasi bertahap.
* **Skenario 2 (Potongan Cicilan Kasbon vs Koperasi Dokter Bedah)**:
  dr. Rahmat memiliki angsuran kasbon alat operasi Rp 3.000.000 (Prioritas 2) dan cicilan belanja koperasi RS Rp 1.500.000 (Prioritas 3). Sistem mengelola jadwal tagihan ini pada `TrxPayrollDeductionSchedule` dan siap mengeksekusi batasan 50% gajinya saat penggajian bulanan.

---

## 5. Status Verifikasi & Hasil Uji
* **Kompilasi C#**: Lulus tanpa error sintaksis.
* **Integritas Relasi**: Foreign key ke `MstEmployee`, `TrxPayrollRun`, `TrxPayrollPayslip`, dan `AspNetUsers` terdefinisi aman dengan perilaku penghapusan `Restrict` / `SetNull`.
* **Indeks Unik**: Nomor dokumen unik (`uq_employee_restitution_claim_number`, `uq_payroll_deduction_schedule_number`, `uq_cashier_disbursement_voucher_number`, `uq_payroll_restitution_number`) dan constraint komposit grade-benefit terverifikasi.
