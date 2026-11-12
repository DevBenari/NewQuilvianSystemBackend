# Laporan Task FE-HRD-134: Layar Lembar Kerja Kalkulasi Gaji, Komponen Rincian & Filter Unit/Profesi (/hr/payroll/runs/[id]/worksheet)

## 1. Informasi Task
* **Task ID**: `FE-HRD-134`
* **Slice / MVP**: `S-B5` / `MVP-30`
* **Nama Fitur**: Layar Lembar Kerja Kalkulasi Gaji, Komponen Rincian & Filter Unit/Profesi (/hr/payroll/runs/[id]/worksheet)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Mengembangkan layar lembar kerja spreadsheet kalkulasi gaji rumah sakit (FE-HRD-164) untuk memproses remunerasi seluruh pegawai:
1. Tombol eksekusi pipeline kalkulasi bertahap ("Hitung Seluruh Pegawai") dengan visual loading indicator.
2. Filter komprehensif data nakes berdasarkan pencarian NIK/Nama, unit kerja instalasi, kelompok profesi medis (Dokter Spesialis, Perawat/Bidan, Penunjang Medis, Non-Klinis), dan toggle khusus anomali variansi.
3. Grid spreadsheet menyajikan Gaji Pokok, Tunjangan Tetap, Lembur & Premi Malam, Total Bruto, Potongan Terpusat, Gaji Bersih (Net Pay), dan status alert variansi.
4. Baris pegawai dengan lonjakan deviasi neto >= 20% otomatis diberi penanda visual highlight warna kuning/merah.
5. Tombol aksi membuka dialog rincian slip gaji pegawai dan drawer investigasi anomali finansial.

## 3. Berkas yang Diubah / Dibuat
* `src/components/view/hr/payroll/payroll-worksheet-view.jsx (Dibuat)`
* `src/app/hr/payroll/runs/[id]/worksheet/page.jsx (Dibuat)`

## 4. Bukti Verifikasi
* Lolos uji kalkulasi angka rupiah, filter kombinasi multi-profesi, highlight baris deviasi >= 20%, ESLint 0 error, build Next.js 0 error.
