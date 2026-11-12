# Laporan Task FE-HRD-294: Layar Matriks Tarif Lumpsum Uang Harian Per Diem & Zonasi Wilayah

## 1. Informasi Task
* **Task ID**: `FE-HRD-294`
* **Slice / MVP**: `S-D5 / Perjalanan Dinas & Biaya Operasional (SPPD)`
* **Nama Fitur**: Layar Matriks Tarif Lumpsum Uang Harian Per Diem & Zonasi Wilayah (`/hrd/travel/rates-and-policies`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka konfigurasi kebijakan dan matriks tarif uang harian dinas:
1. **Layar Matriks Tarif & Kebijakan (`unified-travel-rates-view.jsx`)**:
   - Menampilkan tabel matriks tarif lumpsum per diem per kombinasi 4 klasifikasi zonasi wilayah (Zona 1 Metropolitan 1.25x, Zona 2 Kota Besar 1.0x, Zona 3 Kabupaten 0.85x, Luar Negeri 2.5x) dan 5 tingkatan golongan nakes (Gol I s.d. Gol V).
   - Menampilkan batas pagu plafon akomodasi hotel per malam beserta rekomendasi kelas hotel berbintang (Bintang 2 Budget s.d. Bintang 5 Executive).
   - Menampilkan 5 ketetapan wajib kepatuhan dinas: Plafon uang muka maks 80% (`VR-TRV-03`), batas settlement 7 hari (`VR-TRV-07`), pembekuan SPPD & potong payroll (`INT-HR-99`), gembok roster bangsal (`INT-HR-97`), dan batas biaya darurat tanpa nota maks Rp 150.000 (`VR-TRV-08`).
2. **Pendaftaran Rute Next.js App Router**:
   - `/hrd/travel/rates-and-policies`: Rute matriks tarif pada segment `(dashboard)/hrd`.
   - `/corporate/hrd/travel/rates-and-policies`: Rute matriks tarif pada segment `(dashboard)/corporate/hrd`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/travel/unified-travel-rates-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/travel/rates-and-policies/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/travel/rates-and-policies/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Beralih antar tab zonasi memperbarui angka tarif per diem dan plafon hotel secara dinamis; ketentuan kebijakan tercantum transparan.
* **Standar UI**: Zero-Tailwind, CSS Modules `business-travel.module.css`, dan Design Tokens Quilvian.
