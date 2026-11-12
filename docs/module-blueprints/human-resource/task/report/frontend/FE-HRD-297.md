# Laporan Task FE-HRD-297: Komponen Kalkulator Otomatis Lumpsum Uang Harian & Pilihan Hotel Sesuai Plafon Grade

## 1. Informasi Task
* **Task ID**: `FE-HRD-297`
* **Slice / MVP**: `S-D5 / Perjalanan Dinas & Biaya Operasional (SPPD)`
* **Nama Fitur**: Komponen Kalkulator Otomatis Lumpsum Uang Harian & Pilihan Hotel Sesuai Plafon Grade
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun widget kalkulator interaktif yang secara otomatis mengestimasi biaya SPPD nakes secara transparan:
1. **Kalkulator Otomatis Lumpsum Uang Harian (`TravelPerDiemCalculatorCard.jsx`)**:
   - Menghitung secara dinamis uang saku harian inklusif (makan, transportasi lokal) berdasarkan perpaduan zonasi wilayah tujuan dan tingkat golongan grade pemohon tanpa perlu input manual tarif.
   - Mengkalkulasikan plafon penginapan hotel per malam sesuai kelas golongan nakes (Bintang 2/Budget hingga Bintang 5 Deluxe).
   - Menghitung total estimasi anggaran kedinasan akumulatif (Uang Harian + Plafon Hotel + Tiket Transportasi PP).
2. **Penegakan Aturan Plafon Uang Muka 80% (`VR-TRV-03`)**:
   - Menghitung dan menampilkan batas pagu pencairan kasbon uang muka kerja maksimal 80% dari total estimasi dinas.
   - Mengunci input nominal pengajuan uang muka agar tidak dapat melebihi ambang batas 80%, serta memberikan edukasi bahwa sisa 20% diselesaikan pada tahap settlement rekonsiliasi.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/travel/components/TravelPerDiemCalculatorCard.jsx`
* `QuilvianSystemFrontendDev/src/style/hr/travel/business-travel.module.css`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Perhitungan uang harian berubah secara real-time saat parameter zona/grade diganti; batas 80% uang muka terkunci deterministik.
* **Kepatuhan Aturan Bisnis**: Menjamin kesesuaian anggaran terhadap aturan batasan uang muka kasir `VR-TRV-03`.
