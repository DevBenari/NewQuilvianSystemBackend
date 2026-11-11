# Laporan Task FE-HRD-283: Modal Penolakan (Alasan Wajib >= 15 Karakter) & Modal Pengembalian Revisi

## 1. Informasi Task
* **Task ID**: `FE-HRD-283`
* **Slice / MVP**: `S-A7-V2 / Kotak Masuk Persetujuan Terpadu & ESS V2`
* **Nama Fitur**: Modal Penolakan (Alasan Wajib >= 15 Karakter `VR-WF-05`) & Modal Pengembalian Revisi (`VR-WF-06`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka modal penolakan resmi dan pengembalian draf perbaikan dengan validasi data ketat:
1. **Modal Penolakan Permohonan (`RejectRequestModal.jsx` - `VR-WF-05`)**:
   - Menampilkan peringatan tegas bahwa tindakan penolakan bersifat final dan akan menghentikan seluruh proses tiket permohonan.
   - Pilihan kategori alasan penolakan (Pelanggaran Regulasi RS, Tidak Sesuai SOP, Bentrok Jadwal Jaga, Anggaran Tidak Memadai, Kuota Habis, Lainnya).
   - Area teks alasan penolakan dengan validasi ketat minimal 15 karakter (`VR-WF-05`), penghitung karakter (*character counter*), dan pesan peringatan jika jumlah karakter belum mencukupi.
   - Tombol "Tolak Permohonan" dinonaktifkan hingga syarat 15 karakter terpenuhi secara valid.
2. **Modal Pengembalian Draf Revisi (`ReturnForRevisionModal.jsx` - `VR-WF-06`)**:
   - Menegakkan alur non-destruktif: pengembalian tiket tidak membatalkan transaksi melainkan mengembalikannya ke status Draf Revisi pemohon di portal ESS.
   - Pilihan aspek yang wajib diperbaiki (Lampiran Dokumen Kurang Jelas, Tanggal Salah, Nominal Tidak Sesuai, Penjelasan Kurang Rinci, Konfirmasi Rekan Pengganti).
   - Area instruksi revisi yang jelas untuk memandu pemohon memperbaiki kekurangannya sebelum mengajukan ulang (*Resubmit*).

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/approvals/modal/RejectRequestModal.jsx`
* `QuilvianSystemFrontendDev/src/components/view/hr/approvals/modal/ReturnForRevisionModal.jsx`
* `QuilvianSystemFrontendDev/src/style/hr/approvals/approvals.module.css`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Validasi alasan tolak < 15 karakter mencegah submit secara deterministik; modal pengembalian revisi berhasil mengirimkan instruksi ke antrean perbaikan pemohon.
* **Kepatuhan Aturan Bisnis**: Memenuhi standar akuntabilitas penolakan `VR-WF-05` dan mekanisme revisi adaptif `VR-WF-06`.
