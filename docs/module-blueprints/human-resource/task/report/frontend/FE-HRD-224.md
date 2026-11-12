# Laporan Task FE-HRD-224: Registri Portofolio e-Sertifikat Pegawai & Verifikasi Dokumen Eksternal

## 1. Informasi Task
* **Task ID**: `FE-HRD-224`
* **Slice / MVP**: `S-C2 / MVP-49`
* **Nama Fitur**: Layar Registri Portofolio e-Sertifikat Pegawai & Verifikasi Dokumen Eksternal
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka registri sertifikat kepegawaian pada rute `/hr/learning/certificates`, `/portal/learning/certificates`, dan `/corporate/hrd/learning/certificates`:
1. Tabel portofolio sertifikat internal dan eksternal nakes dengan format nomor seri baku resmi: SRT/HRD-TRN/{TAHUN}/{BULAN}/{SEQUENCE} (VR-TRN-21).
2. Status keabsahan sertifikat (Active, ExpiringSoon, Expired, Revoked, Unverified).
3. Sinkronisasi otomatis ke berkas e-Dossier Sub-02 (INT-HR-72).
4. Verifikasi Sertifikat Eksternal (VR-TRN-19, AT-HRD-TRN-16): Dokumen dari luar rumah sakit berstatus 'Unverified' hingga diverifikasi oleh staf Diklat sebelum dapat diakui pada kredensialing.
5. Tombol pratinjau dan unduh PDF resmi ber-QR Code verifikasi publik.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/learning/training-certificates-view.jsx`
* `QuilvianSystemFrontendDev/src/app/hr/learning/certificates/page.jsx`
* `QuilvianSystemFrontendDev/src/app/portal/learning/certificates/page.jsx`
* `QuilvianSystemFrontendDev/src/app/corporate/hrd/learning/certificates/page.jsx`

## 4. Bukti Verifikasi
* Penomoran sertifikat internal mematuhi format baku resmi SRT/HRD-TRN (VR-TRN-21).
* Sertifikat eksternal berhasil divalidasi ke status sah terverifikasi (VR-TRN-19).
* Memenuhi kriteria penerimaan AT-HRD-TRN-16.
