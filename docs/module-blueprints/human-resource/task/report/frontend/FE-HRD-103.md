# Laporan Task FE-HRD-103: Komponen Metrik Kartu Saldo & Progress Bar Pemakaian Cuti Tahunan

## 1. Informasi Task
* **Task ID**: `FE-HRD-103`
* **Slice / MVP**: `S-B2` / `MVP-24`
* **Nama Fitur**: Komponen Metrik Saldo & Progress Bar Cuti (`LeaveBalanceMetricCards`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Membangun komponen visual kartu metrik responsif (`LeaveBalanceMetricCards.jsx`) yang menampilkan:
1. **Hak Cuti Berjalan**: Hak standar 12 hari kerja per tahun berjalan.
2. **Saldo Aktif Dapat Dipakai**: Nilai saldo siap pakai dengan progress bar sisa kuota.
3. **Saldo Bawaan (Carry-Forward)**: Sisa kuota cuti tahun lalu dengan lencana peringatan kedaluwarsa 30 Juni (`HRD-DEC-110`) serta countdown hari tersisa.
4. **Cuti Telah Terpakai**: Total hari yang telah dinikmati dengan progress bar pemakaian.

## 3. Berkas yang Diubah / Dibuat
* `src/components/features/hr/leave/LeaveBalanceMetricCards.jsx` (Dibuat)
* `src/style/hr/leave/leave.module.css` (Gaya kartu, progress bar, badge kedaluwarsa)

## 4. Bukti Verifikasi
* Lolos audit ESLint 0 error.
* Desain responsif di berbagai resolusi layar tanpa styling Tailwind (Zero-Tailwind).
