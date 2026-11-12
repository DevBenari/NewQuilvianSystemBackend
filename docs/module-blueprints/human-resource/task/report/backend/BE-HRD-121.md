# Laporan Penyelesaian Task: BE-HRD-121

## 1. Ikhtisar Task
* **ID Task:** `BE-HRD-121`
* **Nama Task:** Engine evaluasi ketersediaan staf unit 24/7 (`SafeStaffingCoverageGuard`, `INT-HR-45`)
* **Slice / Milestone:** `S-B2` (Administrasi Cuti & Izin Pegawai Rumah Sakit) / `MVP-25`
* **Status:** ✅ Selesai
* **Tanggal Selesai:** 2026-11-05
* **Pengembang:** Antigravity AI Agent & Human Resource Technical Team
* **Target Arsitektur:** Sub-Modul 08 (Leave Management) terintegrasi ke Sub-Modul 06 (Scheduling & Roster Management)

---

## 2. Latar Belakang & Kebutuhan Rumah Sakit
Pada unit pelayanan 24 jam rumah sakit (seperti Instalasi Gawat Darurat/IGD, Intensive Care Unit/ICU, dan Ruang Rawat Inap), keselamatan pasien (*patient safety*) sangat bergantung pada rasio ketersediaan perawat dan staf medis aktif di setiap shift. Jika pengajuan cuti pegawai disetujui tanpa memperhitungkan jumlah perawat yang tersisa pada shift tersebut, unit berisiko mengalami kekurangan staf kritis (*understaffing*).

Berdasarkan keputusan bisnis `HRD-DEC-111` dan aturan arsitektur `02-backend-architecture.md` §21.8 (`LEV-W1`), sistem wajib mengintegrasikan pengajuan cuti ke modul penjadwalan dinas (`INT-HR-45`) melalui komponen `SafeStaffingCoverageGuard`. Guard ini bertugas mendeteksi apakah permohonan cuti menyebabkan sisa perawat aktif berada di bawah kuota minimal unit, dan menyajikan peringatan dini (*Safe Staffing Coverage Warning*) kepada Kepala Unit / Kepala Ruangan.

---

## 3. Komponen yang Dibuat & Diperbarui

### 3.1 Kontrak Integrasi `ISafeStaffingQueryService`
* **File:** `Areas/Corporate/HumanResource/SchedulingManagement/Services/ISafeStaffingQueryService.cs`
* **Fungsi:** Mendefinisikan antarmuka query evaluasi kuota shift per tanggal dan unit kerja:
  ```csharp
  Task<StaffingCoverageEvaluationResult> EvaluateCoverageAsync(
      Guid unitId, DateOnly targetDate, Guid? shiftId, CancellationToken cancellationToken);
  ```
* **Model Respons `StaffingCoverageEvaluationResult`:**
  - `IsWarningTriggered`: Penanda apakah sisa perawat < kuota minimal.
  - `MinimumRequiredStaff`: Batas minimal perawat yang diwajibkan unit.
  - `ScheduledStaffCount`: Jumlah staf yang terjadwal aktif pada shift tersebut.
  - `RemainingStaffIfApproved`: Sisa staf jika cuti disetujui (`ScheduledStaffCount - 1`).
  - `ShiftName` & `UnitName`: Nama shift dan nama unit ruangan bersangkutan.
  - `AdvisoryMessage`: Pesan peringatan pemandu operasional.

### 3.2 Service Guard `SafeStaffingCoverageGuard`
* **File:** `Areas/Corporate/HumanResource/LeaveManagement/Services/SafeStaffingCoverageGuard.cs`
* **Fungsi Utama:**
  1. Membaca penugasan dinas aktif dari tabel `TrxShiftAssignment` (status `Published`, `Confirmed`, `Scheduled`).
  2. Mengambil standar kebutuhan tenaga harian dari tabel `TrxDailyStaffingRequirement` atau menetapkan batas aman standar rumah sakit (minimal 3 perawat per shift untuk unit 24/7).
  3. Menyediakan metode evaluasi agregat `EvaluateLeaveRequestCoverageAsync` untuk mengevaluasi seluruh tanggal dari `StartDate` hingga `EndDate` pengajuan cuti pegawai.
  4. Menerapkan prinsip ketahanan sistem *fail-open with audit warning*: Jika service roster tidak dapat diakses atau terjadi kendala query, alur kerja cuti tidak diblokir total, melainkan diberikan catatan peringatan audit: *"Evaluasi kuota jadwal tidak dapat diverifikasi secara otomatis; mohon verifikasi jadwal manual"*.

---

## 4. Kepatuhan Aturan Bisnis & Validasi
* **`INT-HR-45`:** Query ketersediaan kuota staf unit in-process synchronous berjalan presisi antara Leave Management dan Scheduling.
* **`LEV-W1`:** Memunculkan status `HasStaffingCoverageWarning = true` dan rincian kuota yang terancam jika `RemainingStaffIfApproved < MinimumRequiredStaff`.
* **Kesiapan Gerbang:** Menjadi landasan bagi kotak masuk persetujuan atasan pada task `BE-HRD-124`.

---

## 5. Kesimpulan
Engine `SafeStaffingCoverageGuard` telah selesai diimplementasikan secara komprehensif, terdaftar pada DI Container di `Program.cs`, dan siap mengawal keselamatan ketenagaan unit rawat inap dan IGD RS Quilvian.
