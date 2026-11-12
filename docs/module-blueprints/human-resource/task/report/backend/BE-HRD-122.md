# Laporan Penyelesaian Task: BE-HRD-122

## 1. Ikhtisar Task
* **ID Task:** `BE-HRD-122`
* **Nama Task:** Service pengajuan cuti mandiri ESS, kalkulasi half-day 0.5 hari & reservasi saldo (`LeaveRequestWorkflowLifecycleService` / `LeaveRequestService`)
* **Slice / Milestone:** `S-B2` (Administrasi Cuti & Izin Pegawai Rumah Sakit) / `MVP-25`
* **Status:** ✅ Selesai
* **Tanggal Selesai:** 2026-11-05
* **Pengembang:** Antigravity AI Agent & Human Resource Technical Team
* **Target Arsitektur:** Sub-Modul 08 (Leave Management) — Pengajuan Mandiri Pegawai (ESS)

---

## 2. Latar Belakang & Kebutuhan Rumah Sakit
Pegawai rumah sakit membutuhkan fasilitas pengajuan cuti mandiri (*Employee Self-Service* / ESS) yang fleksibel namun terlindungi oleh aturan ketat rumah sakit:
1. Fasilitas cuti setengah hari (0.5 hari kerja) untuk keperluan singkat tanpa menghabiskan hak cuti penuh 1 hari (`HRD-ASM-20`).
2. Larangan keras pengajuan cuti yang bertabrakan dengan dinas shift aktif yang telah diterbitkan tanpa izin penggantian staf (`LEV-B2`, `VR-LEV-10`).
3. Pembatasan cuti tahunan berturut-turut maksimal 12 hari kerja untuk menjaga kelancaran rotasi pelayanan medis (`LEV-B7`, `VR-LEV-11`, `HRD-ASM-22`).
4. Kewajiban nomor telepon darurat yang dapat dihubungi selama cuti demi kesiapsiagaan tanggap darurat rumah sakit (`VR-LEV-22`).
5. Pemesanan kuota saldo (*balance reservation*) agar saldo tidak dapat diajukan ganda (*double booking*).

---

## 3. Komponen yang Dibuat & Diperbarui

### 3.1 Perluasan Validasi `LeaveRequestCalculationService.cs`
* **File:** `Areas/Corporate/HumanResource/LeaveManagement/Services/LeaveRequestCalculationService.cs`
* **Penyempurnaan:**
  1. `ValidateRequestMode`: Memvalidasi cuti setengah hari (`IsHalfDay = true`):
     - Membatasi durasi cuti setengah hari hanya boleh diajukan untuk 1 tanggal kerja (`StartDate == EndDate`).
     - Memvalidasi periode setengah hari (`FirstHalf` atau `SecondHalf`).
     - Menolak cuti setengah hari jika jenis cuti bukan Cuti Tahunan (`VR-LEV-05`, `HRD-ASM-20`).
  2. `CalculateDays`: Menghitung durasi pemotongan cuti setengah hari tepat 0.5 hari kerja (`days[0].CountedDays = 0.5m`, `VR-LEV-04`).
  3. `CalculateTotals`: Menolak pengajuan cuti tahunan berurutan yang melebihi batas 12 hari kerja tanpa izin khusus Direktur RS (`VR-LEV-11`, `LEV-B7`, `HRD-ASM-22`).

### 3.2 Penguatan Guard Submit pada `LeaveRequestService.cs`
* **File:** `Areas/Corporate/HumanResource/LeaveManagement/Services/LeaveRequestService.cs`
* **Penyempurnaan Metode `SubmitAsync`:**
  1. Memvalidasi keberadaan nomor telepon darurat (`VR-LEV-22`):
     Jika `string.IsNullOrWhiteSpace(entity.ContactNumberDuringLeave)`, pengajuan ditolak dengan `400 Bad Request`.
  2. Menolak submit bila terdeteksi tabrakan dinas shift aktif terbit pada roster (`VR-LEV-10`, `LEV-B2`):
     Mengembalikan `409 Conflict`: *"Anda telah terdaftar dinas shift pada roster terbit. Koordinasikan ganti petugas sebelum mengajukan cuti."*
  3. Mengintegrasikan `SafeStaffingCoverageGuard`:
     Secara otomatis mengevaluasi ketersediaan staf unit dan mencatat status `HasStaffingCoverageWarning` serta rincian pesan advisory pada permohonan cuti.
  4. Menerapkan reservasi saldo via `LeaveRequestReservationService.ReserveAsync`.

---

## 4. Kepatuhan Aturan Validasi & Matriks
* **`VR-LEV-01` (`LEV-B1`):** Saldo cuti mencukupi, tidak mengizinkan saldo minus.
* **`VR-LEV-04` & `VR-LEV-05` (`HRD-ASM-20`):** Cuti setengah hari memotong kuota tepat 0.5 hari kerja dan khusus untuk Cuti Tahunan.
* **`VR-LEV-10` (`LEV-B2`):** Tabrakan jadwal shift roster ditolak keras dengan `409 Conflict`.
* **`VR-LEV-11` (`LEV-B7`):** Pengajuan berturut-turut > 12 hari ditolak `400 Bad Request`.
* **`VR-LEV-22`:** Nomor kontak darurat wajib diisi demi standar keselamatan RS.

---

## 5. Kesimpulan
Alur pengajuan cuti mandiri ESS telah dilengkapi proteksi validasi komprehensif, reservasi saldo atomik, dan kalkulasi cuti setengah hari 0.5 hari kerja sesuai standar tata kelola rumah sakit.
