# Laporan Task FE-HRD-303: Layar Portal Klaim Pengeluaran Operasional Non-Trip & Rute App Router

## 1. Informasi Task
* **Task ID**: `FE-HRD-303`
* **Slice / MVP**: `S-D5 / Perjalanan Dinas & Biaya Operasional (SPPD)`
* **Nama Fitur**: Layar Portal Klaim Pengeluaran Operasional Non-Trip (`/hrd/travel/expenses`) & Rute App Router
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka pengelolaan klaim penggantian biaya operasional langsung non-trip staf rumah sakit:
1. **Layar Klaim Non-Trip (`unified-travel-expenses-view.jsx`)**:
   - Memfasilitasi klaim reimburse pembelian logistik reagen/obat CITO IGD, konsumsi rapat pleno nakes, kurir berkas akreditasi/legalitas, dan operasional rutin RS tanpa perlu menerbitkan surat perintah perjalanan dinas luar kota.
   - Tabel riwayat klaim non-trip dengan kolom nomor registrasi, nama pemohon, kategori pengeluaran, nama toko/vendor rekanan, nomor nota struk kasir, nominal kuitansi, kanal pencairan (Payroll Sub-10 atau Kasir Tunai CITO), dan status verifikasi Keuangan RS.
   - Dropdown filter multi-kategori dan pencarian cepat.
   - Tombol cetak formulir tanda terima klaim operasional.
2. **Pendaftaran Rute Next.js App Router**:
   - `/hrd/travel/expenses`: Rute klaim non-trip pada segment `(dashboard)/hrd`.
   - `/corporate/hrd/travel/expenses`: Rute klaim non-trip pada segment `(dashboard)/corporate/hrd`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/travel/unified-travel-expenses-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/travel/expenses/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/travel/expenses/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Klaim operasional non-trip terkelola mandiri dan terpisah dari SPPD; filter kategori dan pencarian berjalan responsif.
* **Standar UI**: Zero-Tailwind, CSS Modules `business-travel.module.css`, dan Design Tokens Quilvian.
