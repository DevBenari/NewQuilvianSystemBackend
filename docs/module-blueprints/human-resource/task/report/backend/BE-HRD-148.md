# Laporan Penyelesaian Backend Task: BE-HRD-148

## 1. Identitas Task
* **ID Task**: `BE-HRD-148`
* **Nama Task**: Precondition Gate Validator pengecekan penguncian waktu kerja Sub-07, 08, 09 (`PayrollPreconditionGateService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-30: Fondasi Siklus Penggajian & Penghasilan Tetap`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-05
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam siklus penggajian rumah sakit yang beroperasi 24/7 non-stop, salah satu sumber kesalahan fatal terbesar adalah pembukaan batch penggajian saat data kehadiran, cuti, atau lembur nakes dan staf belum selesai divalidasi oleh kepala ruangan atau HR operasional. 

Aturan bisnis `VR-PAY-02` menetapkan gerbang prasyarat keras (*Hard Precondition Gate*):
* Sistem **DILARANG KERAS** mengizinkan pembuatan batch penggajian bulanan apabila modul hulu belum berstatus tuntas/terkunci:
  1. **Sub-07 Presensi & Kehadiran**: Seluruh log presensi harian perawat, dokter, dan staf hingga tanggal cut-off 20 harus berstatus `Processed`. Tidak boleh ada absensi gantung atau anomali yang belum diselesaikan.
  2. **Sub-08 Cuti & Izin**: Tidak boleh ada permohonan cuti, sakit ber-SIP, atau izin mandiri dengan status `WaitingApproval` untuk periode tanggal $\le 20$.
  3. **Sub-09 Lembur**: Periode lembur bulanan (`OTP-YYYY-MM`) harus berstatus `Locked` atau `Closed` dan proses serah terima (*handoff*) ke payroll telah tereksekusi (`IsHandoffExecuted == true`).

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollPreconditionDtos.cs`
* DTO yang dibuat:
  - `PreconditionGateCheckResponse`: Menyajikan status kelayakan `CanInitialize` (true/false), pesan ringkasan informatif, dan daftar rincian status per modul hulu (`AttendanceModule`, `LeaveModule`, `OvertimeModule`).
  - `ModuleLockStatusDto`: Menampung nama modul, status penguncian (`IsLocked`), status penyelesaian (`IsReady`), dan daftar temuan pending (misal: "12 log presensi masih Unprocessed").

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollPreconditionGateService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollPreconditionGateService.cs`
* Logika Pemeriksaan:
  1. Menghitung rentang tanggal siklus kerja: Tanggal 21 bulan (M-1) sampai dengan tanggal 20 bulan M.
  2. Memeriksa tabel `TrxAttendanceDaily` apakah terdapat status yang bukan `Processed` pada rentang tanggal tersebut.
  3. Memeriksa tabel `TrxLeaveRequest` apakah terdapat permohonan yang masih menggantung (`WaitingApproval`, `PendingSupervisorApproval`).
  4. Memeriksa tabel `TrxOvertimePeriod` apakah periode lembur bulan tersebut belum berstatus `Locked` atau `Closed`.
  5. Menghasilkan keputusan deterministik: `CanInitialize = AttendanceOk && LeaveOk && OvertimeOk`.

---

## 4. Alur Proses Bisnis & Skenario Penolakan Rumah Sakit
* **Skenario Penolakan di Ruang Rawat Inap**:
  Petugas Payroll RS mencoba membuka batch gaji periode November 2026 pada tanggal 21 November pukul 09.00 WIB.
  - Pada Ruang Rawat Inap Melati, Kepala Ruangan belum menyetujui klaim lembur darurat cito 3 orang perawat pelaksana.
  - Akibatnya, periode lembur Sub-09 belum dapat dikunci (`TrxOvertimePeriod.PeriodStatus == "Open"`).
* **Respons Sistem**:
  - `PreconditionGateCheckResponse.CanInitialize` bernilai `false`.
  - Pesan error: *"Precondition Gate Gagal: Periode lembur untuk bulan 2026-11 belum berstatus Locked/Closed atau belum diserahterimakan ke modul payroll."*
  - Pembuatan batch payroll diblokir secara otomatis dan menyajikan daftar kendala secara transparan sehingga petugas payroll dapat meminta Kepala Ruangan segera menuntaskan approval.

---

## 5. Bukti Verifikasi & Definition of Done
* [x] Layanan `PayrollPreconditionGateService` berhasil memverifikasi status modul Sub-07, 08, dan 09.
* [x] Pesan penolakan deskriptif dan transparan bagi staf penggajian.
* [x] Endpoint pengecekan mandiri terintegrasi pada `GET /api/v1/corporate/human-resource/payroll/runs/preconditions`.
* [x] Terdaftar pada Dependency Injection `Program.cs`.
