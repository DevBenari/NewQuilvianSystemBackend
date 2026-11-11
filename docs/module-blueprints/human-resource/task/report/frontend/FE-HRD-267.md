# Laporan Task FE-HRD-267: Modal Asesmen Kembali Bekerja (Return to Work / RTW)

## 1. Informasi Task
* **Task ID**: `FE-HRD-267`
* **Slice / MVP**: `S-C6 / MVP-57`
* **Nama Fitur**: Modal Asesmen Kembali Bekerja (Return to Work / RTW)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun modal dialog asesmen evaluasi klinis kembali bekerja (`ReturnToWorkModal.jsx`):
1. Pengesahan Pemulihan Medis: Dilakukan oleh dokter Hiperkes setelah nakes menyelesaikan masa perawatan/pemulihan klinis.
2. Pencabutan Gembok Roster Sub-06 (`INT-HR-89`): Memberikan otoritas medis resmi untuk mencabut pembatasan tugas dan membuka kembali izin penugasan normal pada jadwal dinas rumah sakit.
3. Catatan Evaluasi & Resume Pemulihan: Menyimpan resume hasil pemeriksaan akhir dan tanggal efektif kembali bekerja penuh.
4. Update State: Memperbarui status restriksi menjadi `REVOKED_RTW` dan status kelaikan nakes menjadi `FIT_FOR_DUTY`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/occupational-health/modal/ReturnToWorkModal.jsx`
* `QuilvianSystemFrontendDev/src/components/view/hr/occupational-health/ohs-work-restrictions-view.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Pencabutan restriksi via modal RTW berhasil melepaskan status gembok hard-stop roster secara deterministik.
* **Standar UI**: Zero-Tailwind, dialog modal terisolasi.
