# Laporan Task FE-HRD-295: Layar Pusat Penugasan & Riwayat SPPD Pegawai & Rute App Router

## 1. Informasi Task
* **Task ID**: `FE-HRD-295`
* **Slice / MVP**: `S-D5 / Perjalanan Dinas & Biaya Operasional (SPPD)`
* **Nama Fitur**: Layar Pusat Penugasan & Riwayat SPPD Pegawai (`/hrd/travel/requests`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka pusat arsip dan pencatatan riwayat Surat Perintah Perjalanan Dinas (SPPD) nakes:
1. **Layar Riwayat SPPD (`unified-travel-requests-view.jsx`)**:
   - Menampilkan tabel komprehensif riwayat penugasan dinas luar kota seluruh tenaga medis dan staf rumah sakit.
   - Kolom data terstruktur: Nomor Registrasi SPPD, Nama Nakes & Unit Kerja, Kota Tujuan & Maksud Dinas, Jadwal Keberangkatan & Kepulangan, Total Estimasi Anggaran, Uang Muka yang Diterima, dan Status Alur Kerja.
   - Fasilitas pencarian instan berbasis nomor tiket, nama nakes, atau kota tujuan, serta dropdown filter status SPPD.
   - Tombol cetak dokumen sah PDF Surat Perintah Perjalanan Dinas (SPPD) ber-barcode untuk verifikasi keabsahan penugasan.
2. **Pendaftaran Rute Next.js App Router**:
   - `/hrd/travel/requests`: Rute riwayat SPPD pada segment `(dashboard)/hrd`.
   - `/corporate/hrd/travel/requests`: Rute riwayat SPPD pada segment `(dashboard)/corporate/hrd`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/travel/unified-travel-requests-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/travel/requests/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/travel/requests/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Seluruh riwayat SPPD tersaji rapi dan terisolasi per pengguna; filter status dan pencarian berfungsi cepat; cetak surat tugas berjalan stabil.
* **Standar UI**: Zero-Tailwind, CSS Modules `business-travel.module.css`, dan Design Tokens Quilvian.
