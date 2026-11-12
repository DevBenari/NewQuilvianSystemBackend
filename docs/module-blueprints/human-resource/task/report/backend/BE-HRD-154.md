# Laporan Penyelesaian Backend Task: BE-HRD-154

## 1. Identitas Task
* **ID Task**: `BE-HRD-154`
* **Nama Task**: Integrasi penarikan potongan pinjaman koperasi & restitusi dari Sub-modul 11 (`PayrollDeductionsIngestionService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-31: Integrasi Potongan Koperasi, Variance Guard, & Alur Otorisasi 3-Jenjang`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Pegawai rumah sakit sering memanfaatkan fasilitas pembiayaan internal seperti pinjaman belanja kebutuhan harian koperasi karyawan, pinjaman dana darurat medis keluarga, atau penyelesaian kelebihan klaim restitusi rawat jalan/kacamata dari Sub-modul 11 (`BenefitManagement`). Tagihan cicilan ini wajib dipotongkan secara otomatis dari slip penghasilan bulanan melalui kontrak integrasi `INT-HR-55`.

Namun demikian, regulasi ketenagakerjaan dan standar remunerasi rumah sakit menegakkan aturan keselamatan finansial pegawai (**`VR-PAY-25` — Proteksi Penghidupan Layak**):
1. **Batas Maksimal Pemotongan Gaji (*Loan Ceiling*)**: Akumulasi potongan pinjaman internal dibatasi maksimal **50% dari penghasilan kotor (*Gross Pay*)** atau disesuaikan dengan parameter konfigurasi master `MaxLoanDeductionPercentage`.
2. **Pencegahan Gaji Bersih Negatif**: Nilai potongan pinjaman tidak boleh melampaui sisa gaji bersih sebelum pinjaman (*remaining net before loan*) setelah dikurangi potongan wajib pemerintah (PPh 21 dan BPJS).
3. **Pencatatan Sebagian (*Partially Paid*)**: Apabila nilai angsuran melebihi pagu proteksi 50%, sistem secara aman hanya memotong sebesar sisa pagu yang tersedia, mencatat sisa saldo tertunggak pada `OutstandingAmount` di entitas `TrxEmployeeLoanInstallment`, dan menandai status angsuran menjadi `PartiallyPaid` tanpa membatalkan batch payroll.

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollDeductionIngestionDtos.cs`
* DTO yang dibuat:
  - `IngestDeductionsResultDto`: Ringkasan penyerapan batch seluruh pegawai (total cicilan terpotong, total nominal potongan pinjaman, jumlah pegawai yang terkena pemangkasan pagu/capped, dan daftar detail per pegawai).
  - `EmployeeDeductionSummaryDto`: Rincian per pegawai (gaji kotor, batas maksimal pagu potongan yang diizinkan, nominal cicilan yang diajukan, nominal aktual yang berhasil dipotong, sisa take-home pay, dan daftar komponen).

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollDeductionsIngestionService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollDeductionsIngestionService.cs`
* Logika Bisnis:
  1. Validasi status batch: Menolak penyerapan jika batch berstatus final (`Approved`, `PaymentProcessing`, `Paid`, `Closed`) atau `IsLocked == true`.
  2. Resolusi parameter konfigurasi: Membaca batas `MaxLoanDeductionPercentage` dari snapshot JSON batch penggajian atau fallback ke setting aktif.
  3. Penarikan Angsuran Pinjaman:
     - Mengueri `TrxEmployeeLoanInstallment` berstatus `Scheduled` atau `PartiallyPaid` dengan `DueDate` $\le$ akhir periode dan status pinjaman induk `Approved` / `Disbursed` / `Active`.
  4. Penegakan Plafon Proteksi:
     - `maxAllowedLoan = Math.Round(emp.GrossPay * (maxLoanPercentage / 100m), 2)`
     - `loanCeiling = Math.Min(maxAllowedLoan, remainingNetBeforeLoan)`
  5. Pembentukan Komponen Gaji:
     - Dibuat baris `TrxPayrollEmployeeComponent` kode `DED_LOAN_KOP` (untuk koperasi) atau `DED_LOAN` (pinjaman umum) dengan deskripsi nomor pinjaman dan nomor angsuran berjalan.
  6. Pembaruan Data Sumber:
     - Angsuran ditandai `PayrollRunEmployeeId = emp.Id`, `PaidAt = DateTime.UtcNow`, `PaymentReferenceNumber = $"{runNumber}-LOAN-{instNumber}"`.
     - Status angsuran menjadi `Paid` (jika lunas) atau `PartiallyPaid` (jika terpotong sebagian).
  7. Pembaruan Akumulasi:
     - `emp.TotalDeduction` bertambah sesuai potongan pinjaman aktual.
     - `emp.NetPay = Math.Max(0m, emp.GrossPay - emp.TotalDeduction)`.
     - Agregasi batch pada `TrxPayrollRun`.

### C. Controller REST API Bergaya Swagger
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunModernController.cs`
* Tag Swagger: `[Tags("HR Payroll - Runs & Batches")]`

| Method | Path | Deskripsi | Status HTTP | Otorisasi |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/ingest-deductions` | Serap potongan pinjaman koperasi & internal seluruh pegawai batch | 200 OK / 422 Unprocessable | Update |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/employees/{employeeRunId}/ingest-deductions` | Serap potongan pinjaman untuk 1 pegawai spesifik | 200 OK / 404 Not Found | Update |

---

## 4. Alur Proses Bisnis & Contoh Kasus Nyata Rumah Sakit
* **Kasus Staf Radiologi**:
  - Nama: Bagus Setiawan, A.Md.Rad
  - Penghasilan Kotor (`GrossPay`): Rp 6.000.000,00
  - Potongan Wajib Pemerintah (BPJS + PPh 21): Rp 450.000,00
  - Batas Maksimal Potongan Pinjaman (50% Gross): $\text{Rp } 3.000.000,00$
  - Tagihan Cicilan Pinjaman Berjalan:
    1. Cicilan Belanja Koperasi Karyawan: Rp 800.000,00 (Angsuran ke-4 dari 10)
    2. Cicilan Pinjaman Renovasi Rumah Sakit: Rp 1.500.000,00 (Angsuran ke-2 dari 12)
    - Total Tagihan Pinjaman: Rp 2.300.000,00 (Masih di bawah pagu Rp 3.000.000,00)
* **Hasil Eksekusi Engine**:
  1. Komponen `DED_LOAN_KOP`: Terpotong penuh Rp 800.000,00 (Angsuran ke-4 berstatus `Paid`).
  2. Komponen `DED_LOAN`: Terpotong penuh Rp 1.500.000,00 (Angsuran ke-2 berstatus `Paid`).
  - Total Potongan Keseluruhan: Rp 450.000 + Rp 2.300.000 = **Rp 2.750.000,00**.
  - Gaji Bersih Diterima (*Net Pay*): **Rp 3.250.000,00** (Hak hidup layak pegawai di atas 50% tetap terlindungi).

---

## 5. Verifikasi Acceptance Criteria
* [x] **Pemotongan Otomatis Cicilan Aktif**: Tagihan pinjaman yang jatuh tempo terserap langsung ke dalam lembar kerja gaji pegawai.
* [x] **Penegakan Batas Pagu 50% (`VR-PAY-25`)**: Pemotongan tidak melebihi 50% dari penghasilan kotor pegawai.
* [x] **Pencatatan Nomor Referensi & Angsuran**: Komponen slip gaji mencantumkan nomor pinjaman dan urutan angsuran berjalan secara transparan.
* [x] **Penanganan Potongan Parsial**: Sisa angsuran yang belum terbayar tercatat pada `OutstandingAmount` tanpa menyebabkan take-home pay negatif.
* [x] **Zero Warning Kompilasi Baru**: Solusi backend lulus `dotnet build` dengan **0 Error(s)**.
