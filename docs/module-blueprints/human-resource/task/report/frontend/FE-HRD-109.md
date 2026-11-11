# Laporan Task FE-HRD-109: Layar Formulir Pengajuan Cuti Mandiri & Cuti Sakit ESS

## 1. Informasi Task
* **Task ID**: `FE-HRD-109`
* **Slice / MVP**: `S-B2` / `MVP-25`
* **Nama Fitur**: Layar Formulir Pengajuan Cuti ESS (`FE-HRD-151` `/hr/leave/request`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Membangun antarmuka pengajuan cuti mandiri bagi seluruh karyawan rumah sakit:
1. Pilihan 10 jenis cuti & izin resmi RS (Tahunan, Setengah Hari, Sakit, Melahirkan, Suami Mendampingi, Duka, Menikah, Khitan, Cuti Bersama, Cuti Tanpa Upah).
2. Integrasi penuh dengan `LeaveDateRangePicker` dan `MedicalCertificateUploadModal`.
3. Input nomor kontak darurat (HP/WhatsApp) dan justifikasi alasan permohonan.
4. Kartu panduan ketentuan hak cuti dan kebijakan recall dinas cito pada kolom kanan.
5. Penegakan validasi: submit dicegah bila saldo kurang atau surat dokter sakit $ge 2$ hari belum diunggah.

## 3. Berkas yang Diubah / Dibuat
* `src/components/view/hr/leave/leave-request-view.jsx` (Dibuat)
* `src/app/hr/leave/request/page.jsx` (Dibuat)

## 4. Bukti Verifikasi
* Lolos linter ESLint 0 error.
* Rute `/hr/leave/request` terkompilasi bersih pada Next.js Turbopack build.
