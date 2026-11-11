# Laporan Penyelesaian Backend Task: BE-HRD-151

## 1. Identitas Task
* **ID Task**: `BE-HRD-151`
* **Nama Task**: Engine penyerapan lembur resmi Sub-09 (`INT-HR-52`) & premi dinas shift malam presensi Sub-07 (`INT-HR-36`) (`PayrollOvertimeAndShiftIngestionService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-30: Fondasi Siklus Penggajian & Penghasilan Tetap`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Operasional rumah sakit 24/7 menuntut pelayanan non-stop lintas unit perawatan dan gawat darurat. Hal ini menimbulkan dua variabel penghasilan kerja kritis:
1. **Lembur Resmi Sesuai PP No. 35/2021 (`INT-HR-52`)**:
   - Seluruh jam lembur yang telah terealisasi dan disetujui pada Sub-modul 09 (`WfpOvertimeRealization` berstatus `Approved` atau `Verified`) diserap ke dalam payroll.
   - Perhitungan upah lembur per jam berpedoman teguh pada regulasi ketenagakerjaan:
     $$\text{Upah Sejam Lembur} = \frac{1}{173} \times (\text{Gaji Pokok} + \text{Tunjangan Tetap})$$
   - Jam lembur yang diserap dipisahkan antara lembur reguler periode berjalan (`OT01`) dan lembur rapel retrospektif bulan sebelumnya yang baru disetujui (`OT02`).
   - Setelah diserap ke payroll, status realisasi lembur ditandai secara permanen (`IsPayrollPosted = true`, `PayrollRunId`, dan `RealizationStatus = "PostedToPayroll"`) untuk mencegah pencairan ganda.
2. **Premi Dinas Shift Malam Presensi Sub-07 (`INT-HR-36`)**:
   - Tenaga medis dan penunjang (perawat, bidan, radiografer, analis laboratorium) yang berdinas pada jadwal shift malam (pukul 21.00 s.d. 07.00) berhak mendapatkan insentif premi jaga malam resmi rumah sakit (standar acuan konfigurasi: Rp 50.000 per shift).
   - Data shift malam diserap langsung dari presensi harian tervalidasi Sub-modul 07 (`WfpAttendanceDailyRecord`) yang telah melalui penguncian precondition gate.

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollVariableEarningsDtos.cs`
* DTO yang dibuat:
  - `IngestOvertimeAndShiftResultDto`: Ringkasan hasil penyerapan batch seluruh pegawai (total jam lembur, total rapel, total lembur terserap, total shift malam, total premi shift, dan daftar rincian per pegawai).
  - `EmployeeVariableEarningSummaryDto`: Rincian per pegawai (total jam lembur reguler, nominal lembur reguler, total jam lembur rapel, nominal rapel lembur, jumlah shift malam, nominal premi shift, total variabel kotor baru, dan daftar komponen rincian).

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollOvertimeAndShiftIngestionService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollOvertimeAndShiftIngestionService.cs`
* Logika Bisnis:
  1. Validasi status batch: Menolak penyerapan jika batch berstatus final (`Approved`, `PaymentProcessing`, `Paid`, `Posted`, `Closed`) atau `IsLocked == true`.
  2. Resolusi konfigurasi: Mengambil nilai premi shift malam dari snapshot konfigurasi batch (`DefaultNightShiftAllowanceAmount`) atau fallback ke setting master aktif.
  3. Evaluasi tarif upah sejam: Dihitung secara matematis dari `(emp.BaseSalary + emp.TotalRecurringEarning) / 173m`.
  4. Penyerapan Lembur Sub-09:
     - Mencari `WfpOvertimeRealization` yang belum diposting (`IsPayrollPosted == false`) dan telah disetujui (`Approved` / `Verified`).
     - Mengelompokkan lembur periode berjalan ke komponen `OT01` (Upah Kerja Lembur Reguler PP 35/2021).
     - Mengelompokkan lembur dari tanggal sebelum periode berjalan ke komponen `OT02` (Rapel Lembur Retrospektif).
     - Menandai data realisasi lembur sumber menjadi `IsPayrollPosted = true` dan `RealizationStatus = "PostedToPayroll"`.
  5. Penyerapan Presensi Shift Malam Sub-07:
     - Menghitung record presensi harian berstatus `Present` dengan shift malam (`ShiftCode` / `ShiftName` mengandung "malam" / "night" / "c").
     - Membentuk komponen `SH01` (Insentif Premi Dinas Shift Malam).
  6. Agregasi Total:
     - `emp.TotalOvertimeAmount = OT01 + OT02`
     - `emp.TotalAttendanceAllowance = SH01`
     - `emp.TotalVariableEarning = emp.TotalOvertimeAmount + emp.TotalAttendanceAllowance`
     - `emp.GrossPay = emp.BaseSalary + emp.TotalRecurringEarning + emp.TotalVariableEarning`
     - `emp.NetPay = emp.GrossPay - emp.TotalDeduction - emp.TotalTax`
     - Agregasi batch pada `TrxPayrollRun`.

### C. Controller REST API Bergaya Swagger
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunModernController.cs`
* Tag Swagger: `[Tags("HR Payroll - Runs & Batches")]`

| Method | Path | Deskripsi | Status HTTP | Otorisasi |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/ingest-overtime-and-shift` | Serap lembur PP 35/2021 dan premi shift malam seluruh pegawai batch | 200 OK / 422 Unprocessable | Update |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/employees/{employeeRunId}/ingest-overtime-and-shift` | Serap lembur dan premi shift malam 1 pegawai spesifik | 200 OK / 404 Not Found | Update |

---

## 4. Alur Proses Bisnis & Contoh Kasus Nyata Rumah Sakit
* **Kasus Perawat IGD Rawat Darurat**:
  - Nama: Ns. Dimas Pratama, S.Kep.
  - Gaji Pokok: Rp 4.200.000
  - Tunjangan Tetap Berulang: Rp 2.250.000 (Tunjangan Jabatan Rp 500.000, Fungsional PK II Rp 750.000, Risiko IGD Rp 1.000.000)
  - Upah Lembur Per Jam: $\frac{4.200.000 + 2.250.000}{173} = \text{Rp } 37.283,24$
  - Realisasi Lembur Resmi Disetujui Sub-09:
    * Lembur reguler periode berjalan: 12 jam (Total = Rp 447.398,88)
    * Rapel lembur retrospektif bulan lalu: 4 jam (Total = Rp 149.132,96)
  - Presensi Dinas Shift Malam Sub-07: 7 shift malam (7 × Rp 50.000 = Rp 350.000)
* **Hasil Eksekusi Engine**:
  1. Komponen `OT01` (Lembur Reguler): Rp 447.398,88
  2. Komponen `OT02` (Rapel Retrospektif): Rp 149.132,96
  3. Komponen `SH01` (Premi Shift Malam): Rp 350.000,00
  - **Total Tambahan Penghasilan Variabel Kotor**: Rp 946.531,84
  - Seluruh SPKL dan realisasi lembur ditandai `IsPayrollPosted = true` dan tidak dapat diklaim ulang.

---

## 5. Verifikasi Acceptance Criteria
* [x] **Kepatuhan Formula PP 35/2021**: Menghitung tarif lembur per jam presisi 1/173 dari total upah tetap.
* [x] **Pemisahan Lembur Reguler & Rapel**: Mengelompokkan lembur retrospektif ke kode `OT02` dan reguler ke `OT01`.
* [x] **Penyerapan Premi Shift Malam Sub-07**: Mengonsumsi data kehadiran dinas malam dan mengalikan tarif insentif dinas.
* [x] **Pencegahan Double-Claim**: Mengubah status realisasi lembur sumber menjadi `PostedToPayroll` dan `IsPayrollPosted = true`.
* [x] **Kompilasi Sukses**: `dotnet build` solusi backend lulus dengan `0 Error(s)`.
