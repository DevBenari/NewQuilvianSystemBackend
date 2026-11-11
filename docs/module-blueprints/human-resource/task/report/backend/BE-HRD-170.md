# Laporan Penyelesaian Backend Task: BE-HRD-170

## 1. Identitas Task
* **ID Task**: `BE-HRD-170`
* **Nama Task**: Engine injeksi otomatis klaim restitusi approved non-cito ke slip gaji bulanan tgl 25 sebagai komponen non-pajak
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-34: Portal ESS Klaim Restitusi Mandiri & Alur Approval 3-Tier`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Setelah berkas klaim restitusi disahkan oleh Manajemen HRD (Status `Approved`), hak penggantian biaya tenaga medis disalurkan melalui siklus penggajian bulanan rumah sakit tanggal 25. Sistem membutuhkan mesin integrasi otomatis yang menghubungkan Sub-modul 11 (Restitusi) ke Sub-modul 10 (Penggajian V2).

Engine `InjectApprovedRestitutionsToPayrollAsync` dirancang dengan invariant bisnis:
1. **Pemisahan Jalur Kasir Cito vs Jalur Slip Gaji (`DED-B6`, `HRD-DEC-133`)**: Hanya klaim regular non-cito (`IsCitoEmergency = false`) yang diserap ke antrean slip gaji. Klaim darurat nakes cito yang telah dicairkan tunai di kasir RS 24/7 tidak boleh diserap ulang guna mencegah pembayaran ganda (*Zero Double-Disbursement*).
2. **Pembentukan Komponen Slip Gaji Penambah Hak (`TrxPayrollEmployeeComponent`)**: Setiap restitusi membentuk baris komponen penerimaan bertipe `Earning`, snapshot kode `RST-REIMB` (klaim kesehatan) atau `RST-CORR` (koreksi salah potong).
3. **Perlindungan Bebas Pajak PPh 21 (`DED-B5`, `HRD-ASM-33`)**: Komponen restitusi mutlak dikunci dengan atribut `IsTaxable = false` dan `TaxableAmount = 0m` sehingga nominal pengembalian diterima utuh oleh pegawai tanpa terpotong PPh 21 TER PMK 168/2023.
4. **Pembaruan Status Atomik**: Klaim asal diperbarui statusnya menjadi `ProcessedInPayroll` dan entitas restitusi menjadi `InjectedToPayroll`.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Object (DTO)
File: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/RestitutionClaimDtos.cs`
* `InjectRestitutionToPayrollResultDto`:
  - `PayrollRunId`: ID siklus penggajian target.
  - `TotalRestitutionsInjected`: Jumlah transaksi restitusi yang berhasil diserap.
  - `TotalReimbursementAmount`: Akumulasi uang klaim kacamata/medis.
  - `TotalCorrectionAmount`: Akumulasi kompensasi salah potong masa lalu.
  - `TotalNonTaxableInjected`: Total hak dana bebas pajak PPh 21 yang masuk ke slip.
  - `InjectedItems`: Daftar rincian komponen yang diinjeksi.
  - `Message`: Laporan audit hasil injeksi.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/PayrollManagement/Services/IRestitutionClaimService.cs`
* `Areas/Corporate/HumanResource/PayrollManagement/Services/RestitutionClaimService.cs`
Metode: `InjectApprovedRestitutionsToPayrollAsync`
Alur Logika:
1. Validasi Siklus Penggajian: Memastikan `targetPayrollRunId` aktif dan belum terkunci (`IsLocked = false`).
2. Filter Antrean Restitusi: Mengambil `TrxPayrollRestitution` berstatus `Approved` yang belum diserap.
3. Pemetaan Pegawai: Menemukan entitas penerima `TrxPayrollRunEmployee` berdasarkan `WorkforceProfileId` atau `EmployeeId`.
4. Pembuatan Baris Komponen: Membentuk `TrxPayrollEmployeeComponent`:
   - `SourceType = "Restitution"`, `ComponentTypeSnapshot = "Earning"`.
   - `Amount = rst.Amount`, `TaxableAmount = 0m`, `IsTaxable = false`.
   - `IsDisplayedOnPayslip = true`.
5. Peningkatan Nilai Hak Bersih: Menambahkan nominal restitusi ke `runEmp.GrossPay` dan `runEmp.NetPay`.
6. Pembaruan Status: Mengubah `rst.Status = "InjectedToPayroll"` dan jika tertaut ke klaim, mengubah `claim.ClaimStatus = "ProcessedInPayroll"`.

---

## 4. Spesifikasi Endpoint REST API (Swagger)
File Controller: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/RestitutionClaimController.cs`
Tag Swagger: `[Tags("HR Deductions - Restitutions")]`

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/hr-deductions/restitutions/inject-to-payroll/{payrollRunId}` | Injeksi otomatis klaim restitusi approved ke payroll run tgl 25 | Bearer JWT | `payrollRunId: Guid` | `InjectRestitutionToPayrollResultDto` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Injeksi Gaji Tanggal 25 November 2026**:
  - Pada siklus penggajian November 2026, terdapat 2 restitusi approved:
    1. dr. Maya (Dokter Anak): Reimbursement Faskes Luar Rp 2.000.000 (`RST-REIMB`).
    2. Pak Joko (Petugas Keamanan): Restitusi Koreksi Lembur Salah Potong Rp 350.000 (`RST-CORR`).
  - HR Payroll memicu `POST /api/hr-deductions/restitutions/inject-to-payroll/{runId}`:
  - **Hasil Eksekusi**:
    * Pada slip gaji dr. Maya muncul baris penerimaan *"Restitusi Reimbursement Klaim Kesehatan ESS: Rp 2.000.000 (Non-Pajak)"*.
    * Pada slip gaji Pak Joko muncul baris penerimaan *"Restitusi Koreksi Salah Potong Periode Lampau: Rp 350.000 (Non-Pajak)"*.
    * Kedua nominal diterima 100% penuh tanpa dipotong pajak penghasilan PPh 21, dan status klaim di ESS pegawai berubah menjadi *"ProcessedInPayroll"*.
