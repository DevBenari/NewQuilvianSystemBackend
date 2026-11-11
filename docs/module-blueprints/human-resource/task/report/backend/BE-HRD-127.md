# Laporan Penyelesaian Task: BE-HRD-127

## 1. Ikhtisar Task
* **ID Task:** `BE-HRD-127`
* **Nama Task:** Eksekusi restore saldo sisa recall & event pemulihan jadwal Roster 06 (`LeaveRecallExecutionService`)
* **Slice / Milestone:** `S-B2` (Administrasi Cuti & Izin Pegawai Rumah Sakit) / `MVP-26`
* **Status:** ✅ Selesai
* **Tanggal Selesai:** 2026-11-05
* **Pengembang:** Antigravity AI Agent & Human Resource Technical Team
* **Target Arsitektur:** Sub-Modul 08 (Leave Management) terintegrasi ke Sub-Modul 06 (Scheduling & Roster)

---

## 2. Latar Belakang & Kebutuhan Rumah Sakit
Ketika pegawai rumah sakit bersedia dipanggil kembali bertugas di tengah-tengah masa cuti resminya demi kepentingan pelayanan pasien:
1. **Keadilan Hak Pegawai (`HRD-DEC-113`, `LEV-W4`):** Hari cuti yang batal dinikmati akibat dipanggil dinas darurat wajib dikembalikan 100% ke saldo hak cuti pegawai pada buku besar.
2. **Kalkulasi Akurat (`VR-LEV-16`):** Hari yang dipulihkan (`RestoredDays`) dihitung sejak tanggal efektif pegawai mulai bertugas kembali sampai tanggal akhir cuti semula (`EndDate - EffectiveRecallDate + 1`).
3. **Kesiapan Roster Tugas (`INT-HR-46`):** Jadwal shift pegawai di modul penjadwalan dinas (Sub-Modul 06) yang sebelumnya berstatus `OnLeave` wajib otomatis dipulihkan menjadi aktif bertugas (`Scheduled`) tanpa penalti absensi.
4. **Status Permohonan Cuti:** Permohonan cuti yang terkena recall diubah statusnya menjadi `Recalled`.

---

## 3. Komponen yang Dibuat & Diperbarui

### 3.1 Pemulihan Saldo via `LeaveBalanceLedgerService.PostRecallRestoreAsync`
* **File:** `Areas/Corporate/HumanResource/LeaveManagement/Services/LeaveBalanceLedgerService.cs`
* **Eksekusi Mutasi:**
  - Menghitung sisa saldo sebelum mutasi (`balanceBefore`).
  - Menambahkan mutasi kredit `RecallRestore` sebesar `recall.RestoredDays`.
  - Menghasilkan sisa saldo baru (`balanceAfter = balanceBefore + RestoredDays`).
  - Mencatat nomor referensi permohonan dinas darurat pada `ReferenceRecallId`.

### 3.2 Pemulihan Jadwal Roster Shift (`INT-HR-46`)
* **File:** `Areas/Corporate/HumanResource/LeaveManagement/Services/LeaveRecallWorkflowService.cs`
* **Logika Pemulihan:**
  - Mengambil seluruh rekaman `TrxShiftAssignment` pegawai dari rentang `EffectiveRecallDate` hingga `EndDate`.
  - Memperbarui `AssignmentStatus = "Scheduled"` dan `HasLeaveConflict = false`.
  - Mengubah status tiket `WfpLeaveRequest` menjadi `Recalled`.

---

## 4. Kepatuhan Aturan Bisnis & Validasi
* **`HRD-DEC-113` & `LEV-W4`:** Saldo cuti yang batal dinikmati terpulihkan otomatis dan seketika pada buku besar.
* **`VR-LEV-16`:** Sisa hari dihitung positif dan tidak melebihi durasi cuti awal.
* **`INT-HR-46`:** Sinkronisasi status jadwal dinas shift roster pulih aktif tanpa memerlukan intervensi manual admin.

---

## 5. Kesimpulan
Eksekusi pemulihan saldo sisa recall dan pengaktifan kembali jadwal dinas shift telah beroperasi secara atomik dan konsisten antar-modul.
