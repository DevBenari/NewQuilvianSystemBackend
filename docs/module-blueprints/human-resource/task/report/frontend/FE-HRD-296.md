# Laporan Task FE-HRD-296: Layar Formulir Pengajuan SPPD Mandiri Multi-Step & Rute App Router

## 1. Informasi Task
* **Task ID**: `FE-HRD-296`
* **Slice / MVP**: `S-D5 / Perjalanan Dinas & Biaya Operasional (SPPD)`
* **Nama Fitur**: Layar Formulir Pengajuan SPPD Mandiri Multi-Step (`/hrd/travel/requests/create`) & Rute App Router
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun wizard formulir pengajuan SPPD mandiri di portal ESS melalui alur terpandu 4 langkah:
1. **Wizard Multi-Step Pengajuan SPPD (`unified-travel-create-view.jsx`)**:
   - *Langkah 1 (Rute & Agenda Dinas)*: Pemilihan kota asal, kota tujuan, zonasi wilayah, tanggal keberangkatan dan kepulangan (otomatis menghitung durasi hari), serta pengisian maksud penugasan.
   - *Langkah 2 (Transportasi & Hotel)*: Pemilihan moda transportasi (Pesawat, Kereta Cepat, Kendaraan Operasional), estimasi tiket PP, jumlah malam menginap, serta pengunggahan berkas surat undangan simposium/nota tugas.
   - *Langkah 3 (Kalkulator Uang Harian & Uang Muka)*: Menghitung total estimasi biaya dinas, checklist pengajuan uang muka kasir, dan penegakan batasan uang muka maksimal 80% (`VR-TRV-03`).
   - *Langkah 4 (Ringkasan & Komitmen)*: Menampilkan resume lengkap estimasi anggaran dinas dan mewajibkan centang pernyataan kepatuhan disiplin settlement 7 hari kalender (`VR-TRV-07`, `INT-HR-99`) sebelum tombol submit aktif.
2. **Pendaftaran Rute Next.js App Router**:
   - `/hrd/travel/requests/create`: Rute wizard pengajuan SPPD pada segment `(dashboard)/hrd`.
   - `/corporate/hrd/travel/requests/create`: Rute wizard pengajuan SPPD pada segment `(dashboard)/corporate/hrd`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/travel/unified-travel-create-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/travel/requests/create/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/travel/requests/create/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Wizard 4-langkah memvalidasi data di tiap tahapan; kalkulasi hari dinas terhitung otomatis; pengajuan baru langsung tercatat di state Redux.
* **Kepatuhan Aturan Bisnis**: Memenuhi mitigasi risiko keuangan `VR-TRV-03` dan komitmen disiplin `VR-TRV-07`.
