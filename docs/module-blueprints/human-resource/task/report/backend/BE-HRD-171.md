# Laporan Penyelesaian Backend Task: BE-HRD-171

## 1. Identitas Task
* **ID Task**: `BE-HRD-171`
* **Nama Task**: Layanan investigasi keluhan salah potong & rekonsiliasi pengembalian dana bebas PPh 21
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-35 & Verifikasi: Darurat Kasir 24/7, Exit Clearance & Verifikasi Akhir`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam operasional rumah sakit dengan rotasi shift dan ribuan staf medis serta penunjang, potensi kekeliruan pemotongan gaji (seperti kelebihan potong kasbon, salah input denda aset, atau duplikasi cicilan koperasi) dapat terjadi. Ketika staf mengajukan komplain atau tiket keluhan salah potong, Departemen SDM (HR Payroll Officer) membutuhkan sarana investigasi berbasis bukti yang sah tanpa merusak catatan historis slip gaji lampau (`PAY-B6`, `DED-B4`).

Layanan `InvestigateAndResolveSalaryGrievanceAsync` menyediakan tata kelola investigasi keluhan dengan penegakan invariant bisnis:
1. **Perekaman Bukti Telaah & Justifikasi Audit (`HRD-DEC-131`, `VR-DED-05`)**: Merekam identitas investigator payroll officer, nomor slip/run penggajian yang terdampak, nominal klaim keluhan pegawai, dan nominal pengembalian yang disetujui setelah diverifikasi dengan bukti mutasi rekening atau log pemotongan.
2. **Pencegahan Nominal Fiktif / Eksesif**: Nominal pengembalian yang disetujui (`ApprovedRefundAmount`) wajib lebih besar dari Rp 0 dan dilarang melebihi nilai keluhan salah potong yang diajukan staf (`ClaimedExcessDeductionAmount`).
3. **Perlindungan Bebas Pajak PPh 21 Mutlak (`DED-B5`, `HRD-ASM-33`)**: Dana pengembalian salah potong bukan merupakan objek penghasilan baru, melainkan pemulihan hak upah yang sebelumnya telah dipotong pajak. Komponen restitusi adjustment dikunci secara permanen dengan atribut `IsTaxable = false` guna mencegah pengenaan pajak berganda (*Zero Double-Taxation*).
4. **Penerbitan Restitusi Koreksi (`TrxPayrollRestitution`)**: Menerbitkan komponen restitusi berstatus `Approved` dengan nomor unik berformat `RST-GRV-YYYYMM-XXXXXX` yang langsung siap diinjeksi ke siklus penggajian berjalan.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/RestitutionClaimDtos.cs`
* `SalaryGrievanceInvestigationRequestDto`:
  - `EmployeeId`: ID pegawai yang mengajukan keluhan.
  - `AffectedPayrollRunId`: ID siklus penggajian lampau tempat terjadinya salah potong.
  - `AffectedPayslipId`: ID slip gaji lampau spesifik (opsional).
  - `ClaimedExcessDeductionAmount`: Nilai kelebihan potong yang dikeluhkan (Rp 1.000 s.d. Rp 100.000.000).
  - `ApprovedRefundAmount`: Nilai pengembalian yang disahkan petugas setelah investigasi.
  - `InvestigationFindings`: Catatan bukti telaah dan kronologi temuan audit (minimal 15 karakter).
  - `TargetPayrollRunId`: Siklus penggajian berjalan tujuan pengembalian dana (opsional).
* `SalaryGrievanceInvestigationResultDto`:
  - `RestitutionId`: ID entitas restitusi yang diterbitkan.
  - `RestitutionNumber`: Nomor unik restitusi (`RST-GRV-YYYYMM-XXXXXX`).
  - `EmployeeId`, `EmployeeName`, `EmployeeNik`: Data identitas pegawai penerima.
  - `AffectedPayrollRunId`: Referensi slip asal yang salah potong.
  - `ApprovedRefundAmount`: Nominal pengembalian bebas PPh 21.
  - `IsTaxable`: Terkunci bernilai `false`.
  - `Status`: `Approved`.
  - `InvestigationFindings`: Catatan audit investigasi.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/PayrollManagement/Services/IRestitutionClaimService.cs`
* `Areas/Corporate/HumanResource/PayrollManagement/Services/RestitutionClaimService.cs`
Metode: `InvestigateAndResolveSalaryGrievanceAsync`

### C. Controller REST API
File: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/RestitutionClaimController.cs`
Tag Swagger: `[Tags("HR Deductions - Restitutions")]`

---

## 4. Spesifikasi Endpoint REST API (Swagger)

| Method | Path | Deskripsi | Otorisasi | Request Body | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/hr-deductions/restitutions/grievance-investigation` | Investigasi keluhan salah potong gaji oleh HR & penerbitan restitusi koreksi bebas PPh 21 | Bearer JWT | `SalaryGrievanceInvestigationRequestDto` | `SalaryGrievanceInvestigationResultDto` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Investigasi Kelebihan Potong Kasbon Perawat ICU**:
  - **Kondisi**: Ns. Rini (Perawat Pelaksana ICU) melaporkan bahwa pada slip gaji Oktober 2026, cicilan kasbon RS miliknya terpotong 2 kali (Rp 1.000.000, seharusnya Rp 500.000) karena duplikasi sinkronisasi batch.
  - **Tindakan**: HR Payroll Officer melakukan audit pencocokan silang antara snapshot Keuangan dengan riwayat potongan slip Oktober (`AffectedPayrollRunId`). Ditemukan kelebihan potong sebesar Rp 500.000.
  - **Eksekusi**: Petugas mengirimkan `POST /api/hr-deductions/restitutions/grievance-investigation`:
    * `ClaimedExcessDeductionAmount`: Rp 500.000
    * `ApprovedRefundAmount`: Rp 500.000
    * `InvestigationFindings`: *"Verifikasi mutasi kasbon Oktober 2026: Terjadi duplikasi input pemotongan cicilan ke-3 sebesar Rp 500.000. Disetujui restitusi pengembalian 100% pada penggajian November 2026."*
  - **Hasil**: Sistem menerbitkan restitusi koreksi `RST-GRV-202611-B72A1F` berstatus `Approved` dengan flag `IsTaxable = false`. Pada slip gaji November 2026, dana Rp 500.000 dikembalikan utuh tanpa pajak PPh 21.

---

## 6. Verifikasi & Integritas Bukti
* **Zero Error Kompilasi .NET**: Kompilasi backend berhasil lulus tanpa error.
* **Audit Trail Lengkap**: Seluruh riwayat investigasi dan identitas reviewer tersimpan pada kolom `Justification` dan `CreateBy`.
* **Kepatuhan Regulasi Pajak**: Menjamin pemenuhan PMK 168/2023 dan `HRD-ASM-33` bahwa kompensasi kesalahan pemotongan upah masa lalu bukan objek pajak baru.
