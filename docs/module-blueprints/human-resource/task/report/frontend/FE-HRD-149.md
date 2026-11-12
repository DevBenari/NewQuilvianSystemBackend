# Laporan Task FE-HRD-149: Layar FE-HRD-170 – Master Jadwal Angsuran, Peringatan Rasio Potong Upah, dan Simulasi Hard-Cap 50%

## 1. Informasi Task
* **Task ID**: `FE-HRD-149`
* **Slice / MVP**: `S-B6 / MVP-32`
* **Nama Fitur**: Layar FE-HRD-170 – Master Jadwal Angsuran, Peringatan Rasio Potong Upah, dan Simulasi Hard-Cap 50%
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Mengimplementasikan halaman App Router untuk Master Lembar Kerja Jadwal Angsuran Pinjaman RS pada rute /corporate/hrd/payroll/deductions/schedules dan alias /hr/payroll/deductions/schedules, menyediakan pencarian cepat NIK/nama pegawai, filter status cicilan aktif/lunas/ditunda, badge rasio potongan upah, serta trigger modal simulasi dan drawer koreksi lampau.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/app/corporate/hrd/payroll/deductions/schedules/page.jsx`
* `QuilvianSystemFrontendDev/src/app/hr/payroll/deductions/schedules/page.jsx`

## 4. Bukti Verifikasi
* Tabel jadwal angsuran interaktif, pencarian responsif, aksi simulasi dan koreksi lampau dapat dipicu langsung dari baris data.
