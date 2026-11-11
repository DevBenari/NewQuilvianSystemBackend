# Laporan Task FE-HRD-271: Layar Surveilans Paparan Bahaya Ruangan Khusus TLD Radiasi

## 1. Informasi Task
* **Task ID**: `FE-HRD-271`
* **Slice / MVP**: `S-C6 / MVP-58`
* **Nama Fitur**: Layar Surveilans Paparan Bahaya Ruangan Khusus TLD Radiasi
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun layar surveilans bahaya okupasional ruangan dan radiasi pada rute `/hrd/occupational-health/surveillance`:
1. Pemantauan Dosis Radiasi Badge TLD Triwulanan: Mencatat nomor seri badge dosimeter TLD, dosis pengukuran triwulan, dan dosis kumulatif tahunan staf radiologi & ruang intervensi.
2. Penegakan Ambang Batas Bapeten 20 mSv/Tahun (`VR-OHS-17`): Deteksi otomatis akumulasi dosis radiasi mendekati/melebihi Nilai Batas Dosis (NBD 20 mSv) yang memicu banner bahaya merah dan rekomendasi rotasi ruangan non-radiasi.
3. Pemantauan Bahaya Okupasional Non-Radiasi: Tab rekaman audiometri (kebisingan mesin boiler/CSSD) dan spirometri (kapasitas paru ruang isolasi/laboratorium).
4. Rute App Router: `/hrd/occupational-health/surveillance` dan `/corporate/hrd/occupational-health/surveillance`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/occupational-health/ohs-surveillance-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/occupational-health/surveillance/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/occupational-health/surveillance/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Dosis kumulatif TLD radiasi > 15 mSv memicu status WASPADA, dan dosis >= 20 mSv memunculkan instruksi pemblokiran penugasan di ruang radiasi.
* **Standar UI**: Zero-Tailwind, CSS Modules `occupational-health.module.css`.
