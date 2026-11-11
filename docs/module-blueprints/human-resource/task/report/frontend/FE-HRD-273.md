# Laporan Task FE-HRD-273: Layar Registrasi Kecelakaan Kerja KAK/PAK & Olah TKP

## 1. Informasi Task
* **Task ID**: `FE-HRD-273`
* **Slice / MVP**: `S-C6 / MVP-59`
* **Nama Fitur**: Layar Registrasi Kecelakaan Kerja KAK/PAK & Olah TKP
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun layar registrasi insiden kecelakaan kerja fisik dan PAK pada rute `/hrd/occupational-health/injuries`:
1. Buku Register Kecelakaan Fisik (KAK) & PAK: Pencatatan identitas nakes korban, klasifikasi cedera/luka, tanggal & jam kejadian, saksi mata, dan lokasi spesifik tempat kejadian perkara (TKP).
2. Investigasi Keselamatan Kerja K3RS (CAPA) (`VR-OHS-18`): Formulir analisis akar masalah (root cause analysis), tindakan perbaikan, dan tindakan pencegahan dengan minimal 2 investigator resmi K3RS.
3. Pelacakan Hari Kerja Hilang (*Lost Time Injury* - LTI) (`INT-HR-90`): Perhitungan jumlah hari kerja hilang yang terintegrasi secara sinkron dengan izin istirahat sakit modul Sub-04.
4. Rute App Router: `/hrd/occupational-health/injuries` dan `/corporate/hrd/occupational-health/injuries`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/occupational-health/ohs-injuries-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/occupational-health/injuries/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/occupational-health/injuries/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Pencatatan insiden KAK/PAK terekam dalam register; validasi minimal 2 investigator K3RS terpenuhi; pelacakan LTI terhitung akurat.
* **Standar UI**: Zero-Tailwind, CSS Modules `occupational-health.module.css`.
