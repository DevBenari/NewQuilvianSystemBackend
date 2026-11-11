# Laporan Task FE-HRD-257: Layar Pengajuan Terminasi Kerja (Resign 30 Hari, Kontrak, Pensiun, SP3) & Radar PKWT

## 1. Informasi Task
* **Task ID**: `FE-HRD-257`
* **Slice / MVP**: `S-C5 / MVP-56`
* **Nama Fitur**: Layar Pengajuan Terminasi Kerja (Resign 30 Hari, Kontrak, Pensiun, SP3) & Radar PKWT
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka administrasi pengakhiran hubungan kerja dan pemantau kontrak PKWT (`relations-separations-view.jsx`):
1. Penegakan Kewajiban One Month Notice (VR-EMP-19):
   - Sistem secara otomatis menghitung selisih hari antara tanggal pengajuan surat dan tanggal efektif berhenti.
   - Jika notice period kurang dari 30 hari kalender pada permohonan resign sukarela, antarmuka memunculkan peringatan keras validasi tata tertib dan menuntut persetujuan dispensasi Direksi.
2. Klasifikasi Alasan Terminasi Resmi PP 35/2021: Pengunduran diri pribadi, berakhirnya masa kontrak PKWT (radar H-60 dan H-30), pensiun usia batas normal, ketidakmampuan fisik/medis permanen, efisiensi kerugian rumah sakit, dan pemutusan disiplin SP3.
3. Inisiasi Kliring & Pesangon: Pendaftaran terminasi secara otomatis memicu pembentukan berkas exit clearance 5 departemen dan penghitungan awal pesangon.
4. Terhubung ke rute Next.js App Router `/hrd/relations/separations`, `/hrd/separations`, dan `/corporate/hrd/relations/separations`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hrd/relations/relations-separations-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/relations/separations/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/separations/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/relations/separations/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Penghitungan notice days berjalan real-time saat tanggal dipilih; validasi notice < 30 hari aktif; registri menampilkan rincian alasan dan masa kerja secara akurat.
* **Linter CI/CD**: `npm run lint:errors` lulus dengan 0 error.
* **Build Produksi**: Kompilasi Next.js Turbopack sukses dengan kode keluar 0.
* **Standar UI**: Zero-Tailwind, CSS Modules `relations.module.css`, dan Design Tokens Quilvian.
