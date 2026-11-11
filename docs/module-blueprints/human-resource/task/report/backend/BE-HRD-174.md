# Laporan Penyelesaian Backend Task: BE-HRD-174

## 1. Identitas Task
* **ID Task**: `BE-HRD-174`
* **Nama Task**: Integrasi pelunasan utang pinjaman pada Exit Clearance pesangon Sub-20 (`INT-HR-60`, `DED-B7`) & webhook callback ke Finance (`INT-HR-58`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-35 & Verifikasi: Darurat Kasir 24/7, Exit Clearance & Verifikasi Akhir`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Saat seorang pegawai mengakhiri masa kerjanya di rumah sakit (baik karena mengundurkan diri/resign, habis kontrak, maupun pemutusan hubungan kerja/PHK), rumah sakit berkepentingan untuk memastikan tidak ada sisa kewajiban piutang atau pinjaman yang belum diselesaikan (*Zero Bad-Debt Separation*).

Layanan `ExitClearanceReceivableService` dan webhook callback `FinanceDeductionSyncService` menegakkan aturan rekayasa finansial:
1. **Pengecualian Batas Hard-Cap 50% hingga 100% Pesangon (`DED-B7`, `VR-DED-07`)**: Sesuai `HRD-DEC-132`, aturan hard-cap 50% upah bulanan (PP 36/2021) dikecualikan secara sah pada slip gaji penutupan perpisahan (*Final Clearance Payslip*). Paket kompensasi akhir (pesangon, uang penggantian hak cuti, uang penghargaan masa kerja) dapat dipotong hingga **100%** untuk melunasi seluruh saldo sisa pinjaman rumah sakit.
2. **Alokasi Pelunasan Mengikuti Prioritas 1 s.d. 4**: Dana pesangon dialokasikan berurutan:
   - Prioritas 1: Ganti Rugi Kasir / Kerusakan Aset RS.
   - Prioritas 2: Kasbon Pokok Rumah Sakit.
   - Prioritas 3: Cicilan Koperasi Pegawai.
   - Prioritas 4: Iuran Paguyuban / Sosial.
3. **Penerbitan Surat Tagihan Residu & Penahanan Clearance (`DED-W5`, `INT-HR-60`)**: Jika total hak pesangon lebih kecil daripada sisa pinjaman (defisit), sistem otomatis menerbitkan *Outstanding Receivable Statement* (`STATEMENT-YYYYMM-XXXXXX`) dan menetapkan status clearance menjadi `BlockedPendingSettlement`. Penandatanganan formulir bebas tugas di Sub-modul 20 (Exit Clearance) ditahan hingga pegawai melunasi sisa defisit via loket Kasir RS.
4. **Penutupan Jadwal Cicilan (`ClearedAtSeparation`)**: Jadwal pinjaman yang lunas langsung ditutup dengan status `ClearedAtSeparation` dan tanggal akhir dicatat.
5. **Webhook Callback Realisasi Pemotongan ke Keuangan (`INT-HR-58`, `HRD-DEC-129`)**: Setiap tanggal 25 saat penggajian bulanan dieksekusi, HR mengirimkan webhook transmisi data pemotongan riil (`FinanceRealizedDeductionCallbackDto`) ke Modul Keuangan (Finance AR) untuk rekonsiliasi buku besar piutang pegawai.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/ExitClearanceReceivableDtos.cs`
* `ExitClearanceReceivableCalculationRequestDto`: Input paket pesangon dan alasan keluar.
* `ExitClearanceReceivableCalculationResultDto`: Hasil kalkulasi pemotongan 100%, sisa bersih pesangon, defisit utang, dan status clearance.
* `ConfirmExitClearanceSettlementRequestDto`: Eksekusi mutasi pelunasan pada jadwal pinjaman di basis data.
* `FinanceRealizedDeductionCallbackDto`: Webhook payload realisasi pemotongan per tgl 25 ke Departemen Keuangan (`INT-HR-58`).
* `FinanceCallbackAcknowledgmentDto`: Respons konfirmasi penerimaan callback.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/PayrollManagement/Services/IExitClearanceReceivableService.cs`
* `Areas/Corporate/HumanResource/PayrollManagement/Services/ExitClearanceReceivableService.cs`
* Pembaruan `IFinanceDeductionSyncService.cs` & `FinanceDeductionSyncService.cs` (metode `SendDeductionRealizationCallbackToFinanceAsync`)

### C. Controller REST API
File: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/ExitClearanceReceivableController.cs`
Tag Swagger: `[Tags("HR Deductions - Exit Clearance & Webhooks")]`

---

## 4. Spesifikasi Endpoint REST API (Swagger)

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/hr-deductions/exit-clearance/calculate` | Simulasi kalkulasi pelunasan utang pada pesangon keluar (100% Cap) | Bearer JWT | `ExitClearanceReceivableCalculationRequestDto` | `ExitClearanceReceivableCalculationResultDto` |
| `POST` | `/api/hr-deductions/exit-clearance/confirm-settlement` | Konfirmasi eksekusi pelunasan pesangon & penutupan jadwal pinjaman | Bearer JWT | `ConfirmExitClearanceSettlementRequestDto` | `ExitClearanceReceivableCalculationResultDto` |
| `POST` | `/api/hr-deductions/exit-clearance/finance-realization-callback` | Transmisi webhook callback realisasi pemotongan tgl 25 ke Finance | Bearer JWT | `periodYear`, `periodMonth`, `payrollRunId` | `FinanceCallbackAcknowledgmentDto` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 — Resign dengan Pesangon Mencukupi (Lunas Penuh)**:
  - Staf Farmasi (Apt. Dian) resign dengan hak pesangon dan kompensasi cuti sebesar Rp 12.000.000.
  - Apt. Dian memiliki sisa kasbon RS sebesar Rp 4.000.000.
  - Simulasi `POST /api/hr-deductions/exit-clearance/calculate`:
    * Total Pesangon Kotor: Rp 12.000.000
    * Total Sisa Utang: Rp 4.000.000 (Dipotong 100%)
    * Pesangon Bersih Diterima: Rp 8.000.000
    * Sisa Utang Defisit: Rp 0
    * Status Exit Clearance: **`Cleared`**
  - Jadwal cicilan diubah menjadi `ClearedAtSeparation` dan formulir Exit Clearance lolos verifikasi.

* **Skenario 2 — PHK dengan Utang Lebih Besar (Defisit & Clearance Tertahan)**:
  - Seorang staf umum terkena PHK dengan paket pesangon Rp 5.000.000, namun memiliki sisa pinjaman renovasi RS sebesar Rp 8.500.000.
  - Simulasi menghasilkan:
    * Total Potongan Pesangon: Rp 5.000.000 (seluruh pesangon diserap)
    * Pesangon Bersih: Rp 0
    * Defisit Sisa Utang: Rp 3.500.000
    * Status Exit Clearance: **`BlockedPendingSettlement`**
    * Diterbitkan Surat Tagihan Piutang: `STATEMENT-202611-F04A92`
  - Exit clearance ditahan hingga staf menyetor tunai Rp 3.500.000 ke kasir rumah sakit.

---

## 6. Verifikasi & Integritas Bukti
* **Perlindungan Aset Rumah Sakit**: Memastikan tidak ada nakes atau staf non-medis yang keluar membawa kewajiban piutang tanpa teridentifikasi.
* **Sinkronisasi Otomatis Buku Besar Finance**: Menjamin Modul Keuangan menerima laporan realisasi pemotongan tepat waktu pada tanggal 25 (`INT-HR-58`).
