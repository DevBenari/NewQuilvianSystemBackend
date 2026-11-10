# Laporan Task FE-HRD-251: Layar Penerbitan & Monitoring Surat Peringatan (SP 1-3 PP 35/2021)

## 1. Informasi Task
* **Task ID**: `FE-HRD-251`
* **Slice / MVP**: `S-C5 / MVP-54`
* **Nama Fitur**: Layar Penerbitan & Monitoring Surat Peringatan (SP 1-3 Masa 6 Bulan PP 35/2021)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka penerbitan dan pemantauan Surat Peringatan (SP) sesuai tata tertib RS dan PP No. 35 Tahun 2021 (`relations-sanctions-view.jsx`):
1. Otomasi Masa Berlaku Sanksi (Pasal 151 PP 35/2021 & VR-EMP-07, VR-EMP-08):
   - SP 1, SP 2, dan SP 3 otomatis terhitung berlaku tepat 6 (enam) bulan kalender.
   - Teguran Lisan resmi berlaku maksimal 3 (tiga) bulan.
2. Sistem Radar Peringatan Kedaluwarsa H-14 (VR-EMP-12): Memberikan sorotan visual (*warning badge & countdown*) pada sanksi aktif yang mendekati sisa waktu 14 hari sebelum diputihkan kembali.
3. Penegakan Prinsip Sanksi Berjenjang (VR-EMP-10): Penjatuhan langsung sanksi SP 3 tanpa SP 2 mewajibkan input kode otorisasi (*Approval Token*) Direktur SDM.
4. Tabel Registri Sanksi & Filter: Tab pemisah untuk Sanksi Aktif, Mendekati Kedaluwarsa (H-14), dan Dalam Sidang Banding.
5. Terhubung ke rute Next.js App Router `/hrd/relations/sanctions` dan `/corporate/hrd/relations/sanctions`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hrd/relations/relations-sanctions-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/relations/sanctions/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/relations/sanctions/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Tanggal kedaluwarsa otomatis bertambah 6 bulan dari tanggal terbit; validasi bypass berjenjang meminta token otorisasi Direktur saat SP 3 dipilih langsung; filter H-14 menyaring sanksi dengan sisa hari <= 14.
* **Linter CI/CD**: `npm run lint:errors` lulus dengan 0 error.
* **Build Produksi**: Kompilasi Next.js Turbopack sukses dengan kode keluar 0.
* **Standar UI**: Zero-Tailwind, CSS Modules `relations.module.css`, dan Design Tokens Quilvian.
