# Laporan Penyelesaian Backend Task: BE-HRD-150

## 1. Identitas Task
* **ID Task**: `BE-HRD-150`
* **Nama Task**: Engine kalkulasi penghasilan tetap: gaji pokok, tunjangan struktural, tunjangan fungsional profesi & risiko medis (`PayrollEarningsCalculationService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-30: Fondasi Siklus Penggajian & Penghasilan Tetap`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-05
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Penghasilan tetap di rumah sakit memiliki kompleksitas berlapis yang berbeda dari korporasi umum, karena memadukan:
1. **Gaji Pokok**: Berdasarkan jenjang kepangkatan, masa kerja, dan grade remunerasi (`WfpSalaryAssignment`).
2. **Tunjangan Jabatan / Struktural**: Diberikan kepada pejabat manajerial rumah sakit (Direktur, Kepala Bidang, Kepala Instalasi, Kepala Ruangan/Karu, Penanggung Jawab Shift).
3. **Tunjangan Fungsional Profesi Medis/Paramedis**: Didasarkan pada kualifikasi profesi klinis (Dokter Spesialis/Konsultan, Dokter Umum, Apoteker Klinis, Perawat Jenjang Karir PK I s.d. PK IV, Radiografer, Pranata Laboratorium).
4. **Tunjangan Risiko Medis & Unit Berisiko Tinggi (*Hazard Allowance*)**: Diberikan kepada tenaga kesehatan yang bertugas di area dengan pajanan bahaya tinggi (ICU/PICU/NICU, Ruang Isolasi/PINERE/TB-MDR, Instalasi Gawat Darurat/IGD, Kamar Operasi/Bedah Sentral, Radiologi/Kedokteran Nuklir, Laboratorium/Patologi, Hemodialisa, dan Bank Darah).

Sesuai aturan `VR-PAY-04` dan `VR-PAY-05`, seluruh perhitungan wajib menggunakan tipe data presisi moneter (`decimal(18,2)`) tanpa pembulatan moneter yang merugikan pegawai, serta setiap komponen penghasilan harus tercatat terpisah sebagai baris komponen `TrxPayrollEmployeeComponent` agar transparan di slip gaji.

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollEarningsDtos.cs`
* DTO yang dibuat:
  - `PayrollEarningsCalculationResultDto`: Hasil kalkulasi batch penghasilan tetap, total gaji pokok, total tunjangan berulang, dan total kotor.
  - `PayrollEmployeeEarningsResultDto`: Rincian per pegawai (Gaji Pokok, Tunjangan Struktural, Tunjangan Fungsional, Tunjangan Risiko Medis, Total Penghasilan Kotor, dan daftar komponen).
  - `PayrollComponentSummaryDto`: Komponen kode, nama, jenis, nominal, dan status pajak.

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollEarningsCalculationService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollEarningsCalculationService.cs`
* Logika Kalkulasi:
  1. Validasi status batch: Menolak penghitungan jika batch telah berstatus `Approved`, `Paid`, `Posted`, atau `IsLocked == true`.
  2. Evaluasi Tunjangan Struktural:
     - Direktur / Wadir: Rp 6.000.000
     - Kabid / Manager: Rp 3.500.000
     - Ka. Instalasi / Ka. Seksi: Rp 2.500.000
     - Ka. Ruangan / Karu / Supervisor: Rp 1.500.000
     - Koordinator / PJ Shift: Rp 750.000
  3. Evaluasi Tunjangan Fungsional Profesi:
     - Dokter Spesialis / Konsultan: Rp 4.500.000
     - Dokter Umum / Gigi: Rp 2.500.000
     - Apoteker: Rp 1.500.000
     - Perawat PK IV: Rp 1.750.000, PK III: Rp 1.250.000, PK II: Rp 900.000, PK I: Rp 600.000
     - Radiografer / Analis Lab: Rp 800.000
     - Fisioterapis / Nutrisionis: Rp 650.000
  4. Evaluasi Tunjangan Risiko Medis & Unit Khusus:
     - Isolasi / PINERE: Rp 1.500.000
     - Radiologi / Bahaya Radiasi: Rp 1.250.000
     - ICU / PICU / NICU: Rp 1.200.000
     - IGD & Kamar Bedah (OK): Rp 1.000.000
     - Laboratorium Patologi: Rp 750.000
     - Hemodialisa: Rp 600.000
     - Bank Darah / Sterilisasi CSSD: Rp 500.000
  5. Pencatatan Komponen Rinci:
     - Kode `GP01`: Gaji Pokok
     - Kode `TJ01`: Tunjangan Struktural / Jabatan
     - Kode `TF01`: Tunjangan Fungsional Profesi Klinis
     - Kode `TR01`: Tunjangan Risiko Medis & Unit Khusus
  6. Agregasi Saldo:
     - `TotalRecurringEarning = TJ01 + TF01 + TR01`
     - `GrossPay = BaseSalary + TotalRecurringEarning`
     - `RunStatus = "Calculating"`
     - `CalculatedAt = DateTime.UtcNow`

### C. Controller REST API Bergaya Swagger
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunModernController.cs`
* Tag Swagger: `[Tags("HR Payroll - Runs & Batches")]`

| Method | Path | Deskripsi | Status HTTP | Otorisasi |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/calculate-earnings` | Hitung penghasilan tetap seluruh pegawai dalam batch | 200 OK / 422 Unprocessable | Update |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/employees/{employeeRunId}/calculate-earnings` | Hitung ulang penghasilan tetap 1 pegawai spesifik | 200 OK / 404 Not Found | Update |

---

## 4. Alur Proses Bisnis & Contoh Kasus Nyata Rumah Sakit
* **Kasus Perawat Primer ICU**:
  - Nama: Ns. Tri Handayani, S.Kep.
  - Jabatan: Penanggung Jawab Shift (PJ Shift)
  - Unit Kerja: Intensive Care Unit (ICU)
  - Jenjang Karir Klinis: Perawat Klinis III (PK III)
  - Gaji Pokok: Rp 4.500.000
* **Hasil Eksekusi Engine**:
  1. Gaji Pokok (`GP01`): Rp 4.500.000
  2. Tunjangan Struktural PJ Shift (`TJ01`): Rp 750.000
  3. Tunjangan Fungsional Perawat PK III (`TF01`): Rp 1.250.000
  4. Tunjangan Risiko Unit ICU (`TR01`): Rp 1.200.000
  - **Total Tunjangan Tetap Berulang**: Rp 3.200.000
  - **Total Penghasilan Kotor (*Gross Pay*) Sementara**: Rp 7.700.000
  - Tercatat rinci ke 4 baris komponen `TrxPayrollEmployeeComponent` dengan status pegawai berubah menjadi `Calculated`.

---

## 5. Bukti Verifikasi & Definition of Done
* [x] Kalkulasi penghasilan tetap presisi tanpa selisih nominal moneter.
* [x] Komponen tersimpan transparan pada tabel `TrxPayrollEmployeeComponent`.
* [x] Status batch run teragregasi secara otomatis pada `TrxPayrollRun`.
* [x] Endpoint Swagger terverifikasi siap pakai dan terdaftar pada Dependency Injection `Program.cs`.
