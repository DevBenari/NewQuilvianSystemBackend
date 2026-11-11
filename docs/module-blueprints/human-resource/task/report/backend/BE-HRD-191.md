# Laporan Penyelesaian Backend Task: BE-HRD-191

## 1. Identitas Task
* **ID Task**: `BE-HRD-191`
* **Nama Task**: Model EF Core, Enums & Migrasi PostgreSQL `HrServiceManagement`
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13 — HR Service Management & Request
* **Slice / Milestone**: `Slice S-D4` · `Fase MVP-39: Katalog, Template Token Dinamis & Agregat Tiket Helpdesk`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Sub-modul 13 mengelola seluruh siklus hidup layanan administrasi personalia mandiri pegawai (*Employee Self-Service / ESS*) dan meja kerja bantuan SDM (*HR Helpdesk & Shared Services*). Di lingkungan rumah sakit yang beroperasi 24 jam dengan ratusan tenaga medis dan staf pendukung, efisiensi penanganan permohonan surat keterangan dinas, pemutakhiran data rekening payroll, serta bantuan personalia membutuhkan skema data yang kokoh, terisolasi, dan dapat diaudit secara mendalam.

Fitur-fitur utama yang dibangun dalam task ini:
1. **Definisi Domain Enums Standar (`HrServiceEnums.cs`)**:
   - `PriorityTier`: Tingkatan prioritas penanganan (`Standar`, `Khusus`, `Cito`).
   - `HrServiceSlaStatus`: Status pemenuhan batas waktu SLA (`WithinSla`, `Warning75`, `Overdue`, `Escalated`).
   - `HrServiceRequestStatus`: Status siklus hidup tiket (`Draft`, `Submitted`, `Assigned`, `InReview`, `WaitingStaffResponse`, `Approved`, `Rejected`, `InProgress`, `Resolved`, `Closed`, `Cancelled`).
   - `DocumentIssuanceStatus`: Status legalitas naskah dinas resmi (`Active`, `Expired`, `Revoked`).
   - `DocumentApprovalStatus`: Status persetujuan dokumen naskah dinas (`Submitted`, `CbApproved`, `Authorized`, `Rejected`).
2. **Entitas Audit Jejak Perubahan Profil (`HrdProfileChangeHistory.cs`)**:
   - Merekam setiap riwayat pemutakhiran data rekening bank, alamat, atau status perkawinan nakes yang berasal dari persetujuan tiket helpdesk SDM.
   - Menyimpan snapshot nilai sebelum (`OldValuesJson`) dan nilai sesudah (`NewValuesJson`) dalam tipe kolom `jsonb` PostgreSQL untuk integritas audit kepatuhan.
3. **Pengayaan Model Inti Layanan SDM**:
   - `MstEmployeeDocumentType`: Diperkaya kolom nomor urut gapless (`NumberingPattern`, `LastSequenceNumber`, `CurrentYear`, `ValidityPeriodDays`, `RequiresCbApproval`).
   - `TrxHrServiceRequest`: Diperkaya kolom SLA jam kerja (`PriorityTier`, `SlaTargetHours`, `SlaDeadlineUtc`, `SlaStatus`).
   - `TrxEmployeeDocumentIssuance`: Diperkaya kolom hash verifikasi QR (`VerificationHash`, `SignerName`, `SignerTitle`, `SignerSignatureHash`, `SecurityPinHash`, `SalaryBrutoAmount`, `SalaryNettoAmount`, `RevocationBapNumber`).
4. **Persiapan Berkas Migrasi Entity Framework Core**:
   - Berkas migrasi `20261109140000_AddHrServiceManagementAndProfileChangeHistoryEnhancements.cs` disiapkan secara mandiri tanpa eksekusi `dotnet ef database update` otomatis sesuai arahan pengguna.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Berkas Enum & Model Baru
* `Areas/Corporate/HumanResource/HrServiceManagement/Models/HrServiceEnums.cs`
* `Areas/Corporate/HumanResource/HrServiceManagement/Models/HrdProfileChangeHistory.cs`
* Pembaruan `MstEmployeeDocumentType.cs`, `TrxHrServiceRequest.cs`, `TrxEmployeeDocumentIssuance.cs`.

### B. Pendaftaran Entity di ApplicationDbContext
File: `Repositories/ApplicationDbContext.cs`
* Pendaftaran `DbSet<HrdProfileChangeHistory> HrdProfileChangeHistories`.

### C. Berkas Migrasi EF Core
File: `Migrations/20261109140000_AddHrServiceManagementAndProfileChangeHistoryEnhancements.cs`

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario Perubahan Rekening Bank Payroll Perawat IGD**:
  - Perawat Nita mengganti rekening bank payroll dari Bank Mandiri ke Bank BCA untuk penerimaan gaji bulanan.
  - Tiket diajukan di portal ESS dan disetujui oleh Analis C&B SDM.
  - Sistem mengeksekusi pembaruan data dan secara otomatis mencatat snapshot di `HrdProfileChangeHistory`:
    * `ComponentModified`: `"BankAccount"`
    * `OldValuesJson`: `{"BankName": "Mandiri", "AccountNumber": "1370012345678"}`
    * `NewValuesJson`: `{"BankName": "BCA", "AccountNumber": "8830123456"}`
    * `ApprovedByUserId`: User ID Supervisor HR.
  - Data lama tersimpan abadi dan tidak dapat dimanipulasi secara retrospektif.

---

## 5. Verifikasi & Integritas Bukti
* **Integritas Skema**: Seluruh entitas mewarisi `IdentityModel` lengkap dengan audit trails (`CreateDateTime`, `UpdateDateTime`, `IsDelete`).
* **Non-Breaking Migration**: Tabel dan kolom baru tidak mengganggu data operasional rumah sakit eksisting.