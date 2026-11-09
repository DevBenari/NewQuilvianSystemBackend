# Laporan Penyelesaian Backend Task: BE-HRD-163

## 1. Identitas Task
* **ID Task**: `BE-HRD-163`
* **Nama Task**: Integrasi sinkronisasi jadwal piutang aktif dari Finance H-3 cut-off tgl 17 (`FinanceDeductionSyncService`, `INT-HR-57`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-33: Fondasi Database, Kebijakan Plafon, Engine Guard 50% & Sinkronisasi Finance`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Sesuai ketetapan arsitektur `HRD-DEC-009` dan `HRD-DEC-129`, kepemilikan buku besar piutang pegawai (*Accounts Receivable Ledger*) berada di Modul Keuangan (Finance & Accounting). Modul HRD tidak mengelola pinjaman baru atau memegang master saldo piutang secara mandiri, melainkan bertindak sebagai eksekutor pemotongan upah pada saat penggajian bulanan.

Guna memastikan data cicilan akurat sebelum proses cut-off presensi/lembur per tanggal 20, sistem HRD melakukan sinkronisasi otomatis dua arah (`INT-HR-57`) pada **H-3 cut-off (tanggal 17 setiap bulan)**. Selain itu, ditegakkan aturan keselamatan `VR-DED-17`, di mana jadwal cicilan bulan berjalan terkunci secara permanen per tanggal 20 pukul 23:59:59 guna mencegah perubahan data dadakan saat perhitungan gaji berlangsung.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/DeductionScheduleDtos.cs`
* `FinanceReceivableSyncItemDto`: Payload data piutang staf dari Finance (ID pegawai, kategori potongan, prioritas 1-4, nomor referensi AR Finance, total pokok pinjaman, angsuran bulanan, sisa pokok, dan jangka waktu cicilan).
* `FinanceSyncResultDto`: Laporan hasil sinkronisasi batch (jumlah data diproses, jadwal baru dibuat, jadwal diperbarui, total nominal cicilan, status penguncian, dan pesan audit).
* `DeductionScheduleResponseDto`: Detail jadwal angsuran cicilan pegawai lengkap dengan nama pegawai, NIK, kategori, prioritas, dan sisa pokok.
* `DeductionScheduleFilterDto`: Filter pencarian jadwal cicilan.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/PayrollManagement/Services/IFinanceDeductionSyncService.cs`
* `Areas/Corporate/HumanResource/PayrollManagement/Services/FinanceDeductionSyncService.cs`
Metode Utama:
1. `SyncFinanceReceivablesAsync`:
   - Menegakkan batas penguncian `VR-DED-17` (menolak update jika lewat tgl 20 pukul 23:59:59).
   - Melakukan penarikan / pemrosesan data piutang Finance.
   - Melakukan pencocokan nomor referensi AR (`ExternalReferenceId`) secara idempoten: memperbarui sisa pokok bila sudah ada, atau membentuk entitas `TrxPayrollDeductionSchedule` baru dengan nomor registrasi unik `DED-YYYYMM-XXXXXX`.
2. `GetActiveSchedulesForEmployeeAsync`: Mengambil seluruh jadwal cicilan aktif seorang pegawai yang diurutkan secara mutlak berdasarkan `PriorityLevel` (1 $\rightarrow$ 2 $\rightarrow$ 3 $\rightarrow$ 4).
3. `GetSchedulesAsync`: Menyediakan query jadwal pemotongan untuk tim HR C&B dan Finance.
4. `LockCurrentMonthSchedulesAsync`: Membekukan jadwal cicilan per tanggal 20 23:59:59.

---

## 4. Spesifikasi Endpoint REST API (Swagger)
File Controller: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollDeductionScheduleController.cs`
Tag Swagger: `[Tags("HR Deductions - Cashier Vouchers & Schedules")]`

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/hr-deductions/schedules/sync-finance` | Sinkronisasi batch jadwal piutang dari Finance H-3 tgl 17 | Bearer JWT | Query `periodYear, periodMonth`, Body `List<FinanceReceivableSyncItemDto>?` | `FinanceSyncResultDto` |
| `GET` | `/api/hr-deductions/schedules` | Daftar jadwal cicilan pemotongan dengan filter | Bearer JWT | `DeductionScheduleFilterDto` | `List<DeductionScheduleResponseDto>` |
| `GET` | `/api/hr-deductions/schedules/employee/{employeeId}` | Jadwal cicilan aktif pegawai urut prioritas 1-4 | Bearer JWT | `employeeId: Guid` | `List<DeductionScheduleResponseDto>` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Sinkronisasi Piutang H-3 Tanggal 17**:
  Pada tanggal 17 November 2026, sistem memicu sinkronisasi `SyncFinanceReceivablesAsync(2026, 11, ...)`:
  - Modul Keuangan mengirimkan 3 data cicilan aktif:
    1. Perawat Siti: Kasbon Pinjaman RS Rp 1.000.000 / bln (`PriorityLevel = 2`, Ref: `FIN-AR-202611-001`).
    2. Apoteker Budi: Cicilan Koperasi Karyawan Rp 500.000 / bln (`PriorityLevel = 3`, Ref: `FIN-AR-202611-002`).
    3. dr. Anton: Cicilan Koperasi RS Rp 1.200.000 / bln (`PriorityLevel = 3`, Ref: `FIN-AR-202611-003`).
  - Sistem HRD menyerap seluruh data, membentuk `TrxPayrollDeductionSchedule`, dan membekukan status per tanggal 20 23:59:59 tanpa perubahan sepihak.
