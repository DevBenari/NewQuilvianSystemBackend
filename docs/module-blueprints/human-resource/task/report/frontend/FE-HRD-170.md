# Laporan Task FE-HRD-170: Layar FE-HRD-179 (ESS View) – Formulir Mandiri Pengajuan Klaim COB Berlampiran Mandatory SEP BPJS

## 1. Informasi Task
* **Task ID**: `FE-HRD-170`
* **Slice / MVP**: `S-D3 / MVP-38`
* **Nama Fitur**: Layar FE-HRD-179 (ESS View) – Formulir Mandiri Pengajuan Klaim COB Berlampiran Mandatory SEP BPJS
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Mengimplementasikan formulir pengajuan mandiri klaim koordinasi manfaat (ess-cob-submission-view.jsx) pada rute /ess/claims/cob-submission. Mewajibkan pengisian Nomor SEP BPJS Kesehatan tepat 19 digit (VR-BEN-16), kalkulator dinamis pemisahan biaya total RS dikurangi tarif INA-CBGs BPJS untuk menghitung santunan sekunder, dropzone kuitansi asli, dan riwayat klaim aktif staf.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/benefits/ess-cob-submission-view.jsx`
* `QuilvianSystemFrontendDev/src/app/ess/claims/cob-submission/page.jsx`

## 4. Bukti Verifikasi
* Formulir memblokir pengajuan jika nomor SEP bukan 19 digit, kalkulator selisih biaya menghitung santunan secara otomatis.
