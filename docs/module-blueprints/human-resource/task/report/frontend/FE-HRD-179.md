# Laporan Task FE-HRD-179: Formulir Pengajuan Tiket ESS Multi-Langkah & Validasi Drag-and-Drop Lampiran 5MB (FE-HRD-182)

## 1. Informasi Task
* **Task ID**: `FE-HRD-179`
* **Slice / MVP**: `S-D4 / MVP-39`
* **Nama Fitur**: Formulir Pengajuan Tiket ESS Multi-Langkah & Validasi Drag-and-Drop Lampiran 5MB (Layar FE-HRD-182)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Mengimplementasikan wizard multi-langkah pengajuan tiket mandiri (Langkah 1: Pilih Kategori & Jenis Layanan, Langkah 2: Deskripsi Permohonan & Pemilihan Prioritas Cito vs Standar dengan validasi surat disposisi medis, Langkah 3: Area Drag-and-Drop Berkas Lampiran dengan batas ukuran maksimal 5MB dan format PDF/JPG/PNG yang tervalidasi, serta Langkah 4: Ringkasan & Konfirmasi Pengajuan Tiket). Submit tiket menghasilkan nomor seri registrasi tiket unik dan mengarahkan ke tab riwayat tiket saya.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/features/portal/services/TicketSubmissionWizard.jsx`
* Terintegrasi pada `portal-service-requests-view.jsx`

## 4. Bukti Verifikasi
* Validasi berkas lampiran menolak file di atas 5MB dan format non-dokumen/gambar.
* Validasi prioritas Cito (&le; 2 Jam) mewajibkan checkbox konfirmasi kepemilikan surat disposisi dan input nomor disposisi resmi.
* Submit permohonan berhasil menambah tiket ke antrean riwayat ESS.
