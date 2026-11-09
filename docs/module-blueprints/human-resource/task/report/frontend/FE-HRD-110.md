# Laporan Task FE-HRD-110: Komponen Banner Peringatan Kuota Pelayanan Staf (Safe Staffing Coverage)

## 1. Informasi Task
* **Task ID**: `FE-HRD-110`
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Nama Fitur**: Banner Peringatan Rasio Ketenagaan (`SafeStaffingCoverageWarningBanner`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Membangun komponen banner keselamatan pasien (`SafeStaffingCoverageWarningBanner.jsx`) sesuai `HRD-DEC-111`:
1. Jika kuota dinas shift aman: merender banner hijau dengan icon perisai (*Ketersediaan Staf Dinas Memenuhi Standar Aman*).
2. Jika kuota dinas shift kritis di bawah rasio minimal RS: merender banner peringatan oranye/kuning yang memuat informasi jumlah staf tersisa vs batas minimal perawat serta tanggal dinas kritis.
3. Menampilkan penegasan mandat: *Wajib menunjuk Staf Pengganti (Mitigasi) sebelum cuti dapat disetujui*.

## 3. Berkas yang Diubah / Dibuat
* `src/components/features/hr/leave/SafeStaffingCoverageWarningBanner.jsx` (Dibuat)
* `src/style/hr/leave/leave.module.css` (Gaya banner aman vs kritis)

## 4. Bukti Verifikasi
* Lolos linter ESLint 0 error.
* Tampilan visual reaktif menginformasikan batas aman ketenagaan ke atasan unit.
