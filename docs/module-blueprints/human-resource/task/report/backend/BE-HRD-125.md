# Laporan Penyelesaian Task: BE-HRD-125

## 1. Ikhtisar Task
* **ID Task:** `BE-HRD-125`
* **Nama Task:** Pembatalan pre-leave sebelum hari-H dan pemulihan saldo 100% otomatis (`LeaveCancellationService`)
* **Slice / Milestone:** `S-B2` (Administrasi Cuti & Izin Pegawai Rumah Sakit) / `MVP-25`
* **Status:** ✅ Selesai
* **Tanggal Selesai:** 2026-11-05
* **Pengembang:** Antigravity AI Agent & Human Resource Technical Team
* **Target Arsitektur:** Sub-Modul 08 (Leave Management) — Pembatalan Mandiri Pegawai (Pre-Leave Cancellation)

---

## 2. Latar Belakang & Kebutuhan Rumah Sakit
Dalam operasional rumah sakit:
1. **Hak Pembatalan Mandiri Pra-Pelaksanaan (`HRD-ASM-21`):** Pegawai yang permohonan cutinya telah disetujui berhak membatalkan cuti tersebut secara mandiri jika rencana cuti urung dilaksanakan, selama pembatalan diajukan **sebelum hari pelaksanaan dimulai** (`Today < StartDate`).
2. **Penolakan Pembatalan Terlambat (`VR-LEV-17`):** Jika pegawai baru mengajukan pembatalan pada saat atau setelah tanggal cuti mulai berjalan (`Today >= StartDate`), pembatalan mandiri ditolak keras oleh sistem (`400 Bad Request`). Untuk kondisi tersebut, hanya dapat dilakukan melalui mekanisme dinas darurat (*Leave Recall*) oleh manajemen RS.
3. **Immutabilitas Transaksi (`LEV-B4`, `VR-LEV-19`, `VR-LEV-20`, `HRD-DEC-115`):** Cuti yang telah berstatus `Completed` atau telah dikunci dalam periode penggajian (`IsPayrollClosed = true`) dilarang dibatalkan dan mengembalikan `409 Conflict`.
4. **Pemulihan Saldo Otomatis 100% (`HRD-ASM-21`):** Pembatalan pre-leave yang sah wajib memulihkan saldo cuti pegawai 100% ke buku besar secara atomik dengan jenis mutasi `CancellationRestore`.
5. **Pemulihan Jadwal Roster (`INT-HR-46`):** Jadwal shift pegawai di modul penjadwalan dikembalikan dari status `OnLeave` menjadi `Scheduled` aktif.

---

## 3. Komponen yang Dibuat & Diperbarui

### 3.1 Peningkatan Service `LeaveCancellationService.cs`
* **File:** `Areas/Corporate/HumanResource/LeaveManagement/Services/LeaveCancellationService.cs`
* **Metode Baru `CancelPreLeaveAsync`:**
  1. Validasi eksistensi dan status permohonan cuti.
  2. Validasi `Today < StartDate` (`VR-LEV-17`, `HRD-ASM-21`):
     Jika hari ini $\ge$ `StartDate`, mengembalikan error `400 Bad Request`: *"Pembatalan mandiri hanya dapat diajukan sebelum tanggal pelaksanaan cuti dimulai."*
  3. Validasi Immutabilitas (`VR-LEV-19`, `VR-LEV-20`):
     Menolak pembatalan dengan `409 Conflict` jika `LeaveRequestStatus == "Completed"` atau `IsPayrollClosed == true`.
  4. Memulihkan saldo 100% via `LeaveBalanceLedgerService.PostCancellationRestoreAsync`.
  5. Memulihkan status jadwal dinas pada tabel `TrxShiftAssignment` kembali menjadi `Scheduled` aktif (`INT-HR-46`).
  6. Mencatat dokumen riwayat audit pada `TrxLeaveCancellationRequest` dengan status `Applied`.

### 3.2 Endpoint Terbuka pada Controller `LeaveRequestEssController.cs`
* **File:** `Areas/Corporate/HumanResource/LeaveManagement/Controllers/LeaveRequestEssController.cs`
* **Endpoint:** `POST /api/v1/corporate/human-resource/leave/requests/{id}/cancel` & `/api/leave/requests/{id}/cancel`
* **Otorisasi:** `[AccessPermission("LeaveRequest", "CancelSelf")]`
* **Respons:** Model `ApiResponse<LeaveLifecycleActionResponse>` berisi konfirmasi pemulihan saldo 100%.

---

## 4. Kepatuhan Aturan Validasi & Matriks
* **`VR-LEV-17` (`HRD-ASM-21`):** Pembatalan pada hari-H atau sesudahnya ditolak `400 Bad Request`.
* **`VR-LEV-19` & `VR-LEV-20` (`LEV-B4`, `HRD-DEC-115`):** Cuti `Completed` dan `PayrollClosed` terlindungi dari pembalikan sepihak (`409 Conflict`).
* **`INT-HR-46`:** Roster shift dipulihkan kembali ke kesiapan bertugas.

---

## 5. Kesimpulan
Fasilitas pembatalan pre-leave mandiri telah selesai diimplementasikan dengan pengamanan batas tanggal yang ketat dan jaminan integritas saldo 100% pada buku besar mutasi cuti.
