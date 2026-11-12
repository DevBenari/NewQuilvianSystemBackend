# Laporan Task FE-HRD-258: Komponen Kalkulator Pesangon Deterministik PP 35/2021 & Lembar Rincian Finansial

## 1. Informasi Task
* **Task ID**: `FE-HRD-258`
* **Slice / MVP**: `S-C5 / MVP-56`
* **Nama Fitur**: Komponen Kalkulator Pesangon Deterministik PP 35/2021 & Lembar Rincian Finansial
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun mesin hitung dan kartu visual rincian kompensasi pesangon deterministik sesuai regulasi ketenagakerjaan PP No. 35 Tahun 2021 (`SeveranceCalculatorCard` / terintegrasi di modal separasi):
1. Formula Deterministik 4 Komponen Finansial (Pasal 40 PP 35/2021 & VR-EMP-17):
   - Uang Pesangon (UP): Tabel progresif masa kerja (< 1 tahun = 1 bulan upah s.d. >= 8 tahun = 9 bulan upah) dikalikan pengali faktor alasan (0x untuk resign sukarela, 0.5x efisiensi defisit, 1.75x pensiun).
   - Uang Penghargaan Masa Kerja (UPMK): Tabel bertingkat masa kerja (3-6 tahun = 2 bulan upah s.d. >= 24 tahun = 10 bulan upah) dikalikan faktor alasan.
   - Uang Penggantian Hak (UPH): Kompensasi sisa hak cuti tahunan yang belum gugur dihitung dengan formula baku `(Sisa Cuti / 25) x Upah Bulanan Tetap`.
   - Uang Pisah: Kompensasi uang pisah bagi pegawai yang mengundurkan diri sukarela sesuai kesepakatan Peraturan Perusahaan/PKB RS.
2. Gerbang Persetujuan Direktur Keuangan: Tombol sinkronisasi ke payroll berstatus disabled hingga Direktur Keuangan memasukkan kode otorisasi (*Finance Approval Code*).
3. Terintegrasi langsung dengan antarmuka pengakhiran kerja (`relations-separations-view.jsx`).

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/utils/hr/relations/relations-display-utils.js` (Fungsi `calculateSeverancePP35`)
* `QuilvianSystemFrontendDev/src/components/view/hrd/relations/relations-separations-view.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Perhitungan UP, UPMK, UPH dan total pesangon terbukti deterministik dan sesuai skenario pengujian regulasi; otorisasi Direktur Keuangan membuka kunci kesiapan pencairan.
* **Linter CI/CD**: `npm run lint:errors` lulus dengan 0 error.
* **Build Produksi**: Kompilasi Next.js Turbopack sukses dengan kode keluar 0.
* **Standar UI**: Zero-Tailwind, CSS Modules `relations.module.css`, dan Design Tokens Quilvian.
