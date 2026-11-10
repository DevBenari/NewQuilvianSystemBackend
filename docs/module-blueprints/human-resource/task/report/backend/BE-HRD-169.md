# Laporan Penyelesaian Backend Task: BE-HRD-169

## 1. Identitas Task
* **ID Task**: `BE-HRD-169`
* **Nama Task**: Layanan kompensasi salah potong masa lalu tanpa revisi slip final dengan audit link slip asal (`PAY-B6`, `HRD-DEC-131`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-34: Portal ESS Klaim Restitusi Mandiri & Alur Approval 3-Tier`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam audit dan tata kelola pembukuan rumah sakit, lembar slip gaji dan batch penggajian final yang telah dibayarkan kepada pegawai bersifat **permanen dan tidak boleh diubah (Immutable, aturan `PAY-B6`)**. Mengubah angka pada slip gaji masa lalu yang sudah ditransfer akan merusak rekonsiliasi buku besar kas, pelaporan SPT Masa PPh 21, dan audit eksternal rumah sakit.

Ketika terjadi insiden keluhan salah potong di masa lalu—misalnya pegawai salah dikenakan denda keterlambatan padahal ia hadir dinas luar, atau cicilan koperasi terpotong ganda—penyelesaiannya wajib dilakukan melalui mekanisme kompensasi restitusi resmi (`HRD-DEC-131`, `DED-B4`).

Layanan `CreateSalaryCorrectionAsync` dibangun dengan aturan:
1. **Prinsip Immutability Slip Lampau (`PAY-B6`, `DED-B4`)**: Slip gaji masa lalu tidak pernah diedit nilainya.
2. **Audit Link ke Slip Asal**: Entitas `TrxPayrollRestitution` bertipe `SalaryAdjustmentCorrection` menautkan ID batch payroll lampau (`AffectedPayrollRunId`) dan slip yang terimbas (`AffectedPayslipId`) sebagai bukti sah.
3. **Status Non-Objek Pajak PPh 21 (`DED-B5`, `HRD-ASM-33`)**: Dana kompensasi pengembalian salah potong bukan merupakan penghasilan bruto baru, melainkan hak gaji neto yang tertunda, sehingga wajib diset `IsTaxable = false` agar pegawai tidak terkena pemotongan pajak PPh 21 ganda.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/RestitutionClaimDtos.cs`
* `CreateSalaryCorrectionRequestDto`:
  - `EmployeeId`: ID pegawai yang berhak menerima pengembalian.
  - `AffectedPayrollRunId`: ID payroll run periode lampau yang salah potong.
  - `AffectedPayslipId`: ID lembar slip gaji lampau yang terimbas.
  - `Amount`: Nominal dana yang dikompensasikan.
  - `Justification`: Alasan justifikasi audit (minimal 10 karakter).
  - `TargetPayrollRunId`: Siklus penggajian bulan berjalan tempat uang dikembalikan.
* `PayrollRestitutionResponseDto`: Data pengembalian restitusi terstruktur ber-audit link.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/PayrollManagement/Services/IRestitutionClaimService.cs`
* `Areas/Corporate/HumanResource/PayrollManagement/Services/RestitutionClaimService.cs`
Metode: `CreateSalaryCorrectionAsync` dan `GetPayrollRestitutionsAsync`
Alur Logika:
1. Verifikasi Keberadaan Slip Lampau: Memastikan `AffectedPayrollRunId` terdaftar sah pada database.
2. Pembangkitan Nomor Registrasi Unik: Format `RST-CORR-{Year}{Month:D2}-{RandomHex6}`.
3. Penetapan Status Bebas Pajak: Mengunci kolom `IsTaxable = false` (`DED-B5`).
4. Persistensi ke Tabel `TrxPayrollRestitution` berstatus `Approved` dan siap diinjeksi ke payroll bulan berjalan.

---

## 4. Spesifikasi Endpoint REST API (Swagger)
File Controller: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/RestitutionClaimController.cs`
Tag Swagger: `[Tags("HR Deductions - Restitutions")]`

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/hr-deductions/restitutions/salary-correction` | Menerbitkan kompensasi salah potong masa lalu non-pajak | Bearer JWT | `CreateSalaryCorrectionRequestDto` | `PayrollRestitutionResponseDto` |
| `GET` | `/api/hr-deductions/restitutions` | Daftar komponen restitusi dan koreksi penggajian | Bearer JWT | Query filter | `List<PayrollRestitutionResponseDto>` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Investigasi Salah Potong Denda Presensi Laboran**:
  - Mas Agus (Laboran Patologi) pada slip gaji September 2026 terpotong denda alpa Rp 500.000 karena mesin absensi laboratorium sempat *offline*.
  - Mas Agus mengajukan klarifikasi beserta surat tugas kepala lab, dan disetujui HRD pada November 2026.
  - **Hasil Eksekusi Layanan**:
    * Slip gaji September 2026 tetap utuh dan tidak diubah (`PAY-B6`).
    * Sistem menerbitkan `TrxPayrollRestitution` nomor `RST-CORR-202611-009988` sebesar Rp 500.000 dengan tautan `AffectedPayrollRunId = Run September 2026`.
    * Status restitusi: `IsTaxable = false` (bebas pajak PPh 21) dan siap masuk ke slip gaji November 2026.
