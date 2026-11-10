# Laporan Task FE-HRD-288: Layar Beranda Portal Layanan Mandiri Pegawai (ESS) & Rute App Router

## 1. Informasi Task
* **Task ID**: `FE-HRD-288`
* **Slice / MVP**: `S-A7-V2 / Kotak Masuk Persetujuan Terpadu & ESS V2`
* **Nama Fitur**: Layar Beranda Portal Layanan Mandiri Pegawai (ESS) & Rute App Router
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun portal beranda terpadu Layanan Mandiri Karyawan / Employee Self-Service (Sub-Modul 15):
1. **Layar Portal Beranda ESS (`unified-ess-portal-view.jsx`)**:
   - Menampilkan banner sambutan personal bagi staf dengan info jabatan, divisi, dan status kepegawaian aktif.
   - *Kartu Saldo Hak Staf Terintegrasi*:
     - Saldo Cuti Tahunan: Sisa 9 hari dari kuota 12 hari per tahun.
     - Akumulasi Lembur Bulan Berjalan: 14.5 jam (memperingatkan jika mendekati batas 14 jam per minggu).
     - Plafon Restitusi Medis: Sisa Rp 4.250.000 dari plafon Rp 7.500.000.
     - Pinjaman Aktif: Status angsuran berjalan.
   - *Menu Cepat Pengajuan Mandiri*: Tombol pintas pengajuan Cuti, Rencana Lembur, Tukar Shift, Klaim Restitusi, Pinjaman Karyawan, Pengunduran Diri (Resign), dan Permohonan Surat HR.
   - *Widget Status Permohonan Terkini*: Menampilkan 3 tiket pengajuan terakhir staf beserta badge status (Dalam Review, Disetujui, Perlu Revisi).
2. **Pendaftaran Rute Next.js App Router**:
   - `/hrd/self-service`: Rute beranda portal ESS pada segment `(dashboard)/hrd`.
   - `/corporate/hrd/self-service`: Rute beranda portal ESS pada segment `(dashboard)/corporate/hrd`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/approvals/unified-ess-portal-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/self-service/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/self-service/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Portal ESS menampilkan saldo hak staf secara personal dan aman; kartu saldo menyajikan angka yang valid; navigasi menu cepat membuka modul terkait dengan tepat.
* **Standar UI**: Zero-Tailwind, CSS Modules `approvals.module.css`, dan Design Tokens Quilvian.
