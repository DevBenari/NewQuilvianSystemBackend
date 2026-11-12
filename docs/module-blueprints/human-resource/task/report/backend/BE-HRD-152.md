# Laporan Penyelesaian Backend Task: BE-HRD-152

## 1. Identitas Task
* **ID Task**: `BE-HRD-152`
* **Nama Task**: Engine kalkulasi iuran BPJS Kesehatan dan BPJS Ketenagakerjaan berplafon upah resmi (`PayrollBpjsCalculationService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-30: Fondasi Siklus Penggajian & Penghasilan Tetap`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Sistem jaminan sosial nasional Indonesia mewajibkan rumah sakit selaku pemberi kerja untuk memotong dan menyetorkan iuran BPJS Kesehatan dan BPJS Ketenagakerjaan sesuai regulasi ketenagakerjaan pemerintah.

Dasar upah pengenaan iuran BPJS (*Calculation Base Wage*) adalah **Gaji Pokok + Tunjangan Tetap** (`BaseSalary + TotalRecurringEarning`).

Rincian skema tarif dan batas plafon upah resmi:
1. **BPJS Kesehatan (Sesuai Perpres No. 64/2020 & Perpres No. 59/2024)**:
   - Batas plafon upah maksimal: **Rp 12.000.000,00**
   - Bagian Pekerja: **1,00%** (dipotong dari slip gaji pegawai) $\rightarrow$ Komponen `DED_BPJS_KES`
   - Bagian Rumah Sakit: **4,00%** (beban operasional fasilitas kesehatan) $\rightarrow$ Komponen `ER_BPJS_KES`
2. **BPJS Ketenagakerjaan (4 Program Resmi)**:
   - **Jaminan Kecelakaan Kerja (JKK)**: **0,89%** (Tingkat risiko rumah sakit, beban RS 100%) $\rightarrow$ Komponen `ER_BPJS_JKK`
   - **Jaminan Kematian (JKM)**: **0,30%** (Beban RS 100%) $\rightarrow$ Komponen `ER_BPJS_JKM`
   - **Jaminan Hari Tua (JHT)**: Bebas plafon upah
     * Bagian Pekerja: **2,00%** $\rightarrow$ Komponen `DED_BPJS_JHT`
     * Bagian Rumah Sakit: **3,70%** $\rightarrow$ Komponen `ER_BPJS_JHT`
   - **Jaminan Pensiun (JP)**: Berplafon upah resmi pemerintah (**Rp 10.042.300,00**)
     * Bagian Pekerja: **1,00%** $\rightarrow$ Komponen `DED_BPJS_JP`
     * Bagian Rumah Sakit: **2,00%** $\rightarrow$ Komponen `ER_BPJS_JP`

Seluruh perhitungan dilakukan dengan pembulatan moneter matematis standar perbankan/pemerintah (`MidpointRounding.AwayFromZero`) bertipe `decimal(18,2)`.

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollBpjsDtos.cs`
* DTO yang dibuat:
  - `CalculateBpjsResultDto`: Agregasi batch BPJS (total potongan pegawai, total kontribusi RS, total BPJS Kesehatan, total BPJS Ketenagakerjaan, dan daftar rincian).
  - `EmployeeBpjsCalculationDto`: Rincian per pegawai (dasar upah, dasar berplafon Kesehatan & JP, nominal rinci potongan pegawai per program, nominal beban RS per program, dan total kontribusi).

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollBpjsCalculationService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollBpjsCalculationService.cs`
* Logika Bisnis:
  1. Validasi status batch: Menolak kalkulasi jika batch telah berstatus final (`Approved`, `PaymentProcessing`, `Paid`, `Posted`, `Closed`) atau `IsLocked == true`.
  2. Pembersihan komponen lama: Komponen bertipe sumber `Insurance` lama dibersihkan secara aman sebelum perhitungan baru dibuat.
  3. Evaluasi Plafon Upah:
     - Dasar BPJS Kesehatan: `Math.Min(baseWage, setting.BpjsKesehatanCapAmount)`
     - Dasar BPJS Ketenagakerjaan JP: `Math.Min(baseWage, setting.BpjsKetenagakerjaanJpCapAmount)`
     - Dasar JHT, JKK, JKM: `baseWage` penuh
  4. Pembentukan 7 baris komponen `TrxPayrollEmployeeComponent` (3 potongan pegawai & 4 kontribusi pemberi kerja).
  5. Pembaruan Akumulasi Pegawai:
     - `emp.TotalEmployeeInsuranceContribution = bpjsKesEE + bpjsJhtEE + bpjsJpEE`
     - `emp.TotalEmployerInsuranceContribution = bpjsKesER + bpjsJkkER + bpjsJkmER + bpjsJhtER + bpjsJpER`
     - `emp.TotalDeduction = emp.TotalEmployeeInsuranceContribution`
     - `emp.NetPay = emp.GrossPay - emp.TotalDeduction - emp.TotalTax`
  6. Pembaruan Akumulasi Batch `TrxPayrollRun`.

### C. Controller REST API Bergaya Swagger
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunModernController.cs`
* Tag Swagger: `[Tags("HR Payroll - Runs & Batches")]`

| Method | Path | Deskripsi | Status HTTP | Otorisasi |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/calculate-bpjs` | Hitung iuran BPJS Kesehatan dan BPJS Ketenagakerjaan seluruh batch | 200 OK / 422 Unprocessable | Update |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/employees/{employeeRunId}/calculate-bpjs` | Hitung iuran BPJS untuk 1 pegawai spesifik | 200 OK / 404 Not Found | Update |

---

## 4. Alur Proses Bisnis & Contoh Kasus Nyata Rumah Sakit
* **Kasus Dokter Spesialis Bedah**:
  - Nama: dr. Rahmat Hidayat, Sp.B
  - Gaji Pokok + Tunjangan Tetap: Rp 15.000.000,00 (Melampaui plafon Kesehatan Rp 12jt dan JP Rp 10.042.300)
* **Hasil Eksekusi Engine**:
  1. **BPJS Kesehatan**:
     - Dasar pengenaan: Rp 12.000.000,00 (mentok plafon)
     - Potongan Pekerja (1%): Rp 120.000,00 (`DED_BPJS_KES`)
     - Beban Rumah Sakit (4%): Rp 480.000,00 (`ER_BPJS_KES`)
  2. **BPJS Ketenagakerjaan**:
     - JKK (0,89% × Rp 15.000.000): Rp 133.500,00 (`ER_BPJS_JKK`)
     - JKM (0,30% × Rp 15.000.000): Rp 45.000,00 (`ER_BPJS_JKM`)
     - JHT Pekerja (2% × Rp 15.000.000): Rp 300.000,00 (`DED_BPJS_JHT`)
     - JHT Rumah Sakit (3,7% × Rp 15.000.000): Rp 555.000,00 (`ER_BPJS_JHT`)
     - JP Dasar Pengenaan: Rp 10.042.300,00 (mentok plafon)
     - JP Pekerja (1% × Rp 10.042.300): Rp 100.423,00 (`DED_BPJS_JP`)
     - JP Rumah Sakit (2% × Rp 10.042.300): Rp 200.846,00 (`ER_BPJS_JP`)
  - **Total Potongan BPJS Pegawai**: Rp 120.000 + Rp 300.000 + Rp 100.423 = **Rp 520.423,00**
  - **Total Kontribusi Beban RS**: Rp 480.000 + Rp 133.500 + Rp 45.000 + Rp 555.000 + Rp 200.846 = **Rp 1.414.346,00**

---

## 5. Verifikasi Acceptance Criteria
* [x] **Penerapan Plafon Upah BPJS Kesehatan**: Menegakkan plafon resmi Rp 12.000.000 sesuai peraturan pemerintah.
* [x] **Penerapan Plafon Upah BPJS Ketenagakerjaan JP**: Menegakkan plafon resmi Rp 10.042.300.
* [x] **Pemisahan Potongan Pegawai & Kontribusi RS**: Komponen kontribusi pemberi kerja ditandai `EmployerContribution` dan tidak mengurangi slip gaji pekerja.
* [x] **Presisi Moneter 2 Desimal**: Menggunakan tipe data `decimal(18,2)` dengan pembulatan matematis `MidpointRounding.AwayFromZero`.
* [x] **Kompilasi Sukses**: Solusi backend terkompilasi bersih tanpa error (`dotnet build`).
