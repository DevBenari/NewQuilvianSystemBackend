# Laporan Penyelesaian Backend Task: BE-HRD-149

## 1. Identitas Task
* **ID Task**: `BE-HRD-149`
* **Nama Task**: Engine inisiasi batch siklus penggajian bulanan & proteksi anti-duplikasi periode (`PayrollRunInitializationService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-30: Fondasi Siklus Penggajian & Penghasilan Tetap`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-05
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Inisiasi siklus penggajian adalah pintu gerbang awal pembuatan dokumen pembayaran gaji bulanan di rumah sakit. 

Aturan bisnis `VR-PAY-01` mewajibkan:
1. **Proteksi Anti-Duplikasi Periode**: Sistem dilarang keras membuat lebih dari 1 batch penggajian aktif untuk tahun dan bulan yang sama pada jenis penggajian yang sama (`Regular`). Percobaan pembuatan kedua wajib ditolak dengan `HTTP 409 Conflict`.
2. **Pembentukan Nomor Batch Resmi**: Penomoran resmi batch gaji berformat terstandar `PRUN-YYYYMM-XXX` (contoh: `PRUN-202611-001`).
3. **Pembekuan Snapshot Parameter**: Menyimpan salinan snapshot konfigurasi aktif dari `HrdPayrollSetting`.
4. **Populasi Seluruh Pegawai Aktif**: Membaca seluruh data master `MstWorkforceProfile` aktif, memetakan unit kerja, jabatan, dan assignment gaji pokok (`WfpSalaryAssignment`) ke dalam tabel detail `TrxPayrollRunEmployee` berstatus awal `Pending`.

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollRunDtos.cs`
* DTO yang dibuat:
  - `CreatePayrollRunCommand`: Input pembuatan batch (`PeriodYear`, `PeriodMonth`, `RunType`, `BypassPreconditions`, `Notes`).
  - `PayrollRunSummaryDto`: Ringkasan batch untuk daftar tabel dashboard payroll.
  - `PayrollRunDetailDto`: Informasi lengkap batch, statistik keuangan, dan daftar pegawai.
  - `PayrollRunEmployeeItemDto`: Ringkasan pegawai dalam batch gaji.
  - `PayrollRunOperationResultDto`: Respons hasil operasi pembuatan batch.

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollRunInitializationService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollRunInitializationService.cs`
* Alur Kerja Utama:
  1. Melakukan evaluasi `PayrollPreconditionGateService` (kecuali di-bypass dengan otorisasi khusus).
  2. Mengecek apakah batch periode tersebut sudah ada (`VR-PAY-01` $\rightarrow$ `HTTP 409 Conflict`).
  3. Mengambil atau membuat entitas `MstPayrollPeriod` dengan cut-off tanggal 20 dan tanggal bayar 25.
  4. Menghasilkan nomor batch terurut unik `PRUN-YYYYMM-XXX`.
  5. Mengambil daftar seluruh pegawai aktif beserta `WfpSalaryAssignment` efektif, membentuk baris `TrxPayrollRunEmployee`.
  6. Menginisiasi `TrxPayrollRun` dengan status awal `Draft` dan menyimpan snapshot pengaturan.

### C. Controller REST API Bergaya Swagger
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunModernController.cs`
* Tag Swagger: `[Tags("HR Payroll - Runs & Batches")]`

| Method | Path | Deskripsi | Status HTTP | Otorisasi |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/human-resource/payroll/runs/preconditions` | Pengecekan gerbang prasyarat modul hulu | 200 OK | Read |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs` | Inisiasi batch penggajian bulanan baru | 201 Created / 409 Conflict / 422 Unprocessable | Create |
| `GET` | `/api/v1/corporate/human-resource/payroll/runs` | Daftar seluruh batch penggajian | 200 OK | Read |
| `GET` | `/api/v1/corporate/human-resource/payroll/runs/{id}` | Detail batch dan daftar pegawai | 200 OK / 404 Not Found | Read |

---

## 4. Alur Proses Bisnis & Contoh Skenario Rumah Sakit
1. Pada tanggal 21 November 2026, Petugas HR Payroll membuka form inisiasi batch gaji.
2. Petugas memilih Tahun `2026`, Bulan `11`, dan tipe `Regular`.
3. Precondition gate memeriksa modul presensi, cuti, dan lembur; seluruhnya telah `Locked`.
4. Sistem memverifikasi belum ada batch gaji `Regular` untuk November 2026.
5. Sistem mengumpulkan 450 tenaga medis, paramedis, dan tenaga penunjang aktif RS.
6. Batch `PRUN-202611-001` terbentuk dengan status `Draft` beserta 450 record `TrxPayrollRunEmployee`.
7. Jika petugas tidak sengaja menekan tombol "Inisiasi" untuk kedua kalinya, sistem langsung menolak dengan pesan error `409 Conflict`: *"Batch penggajian untuk periode 2026-11 dengan tipe 'Regular' sudah aktif dengan nomor PRUN-202611-001."*

---

## 5. Bukti Verifikasi & Definition of Done
* [x] Proteksi anti-duplikasi `VR-PAY-01` teruji dengan pengembalian status HTTP 409 Conflict.
* [x] Nomor batch resmi terbentuk otomatis mengikuti standar `PRUN-YYYYMM-XXX`.
* [x] Snapshot setting aktif tersimpan dalam format JSON pada kolom `ConfigurationSnapshotJson`.
* [x] Daftar pegawai aktif terpopulasi lengkap dengan relasi gaji pokok.
