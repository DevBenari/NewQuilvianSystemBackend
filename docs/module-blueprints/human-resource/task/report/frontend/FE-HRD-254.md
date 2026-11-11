# Laporan Task FE-HRD-254: Layar Pengaduan Keluhan Staf (Grievance ESS) & Notulen Mediasi SLA 14 Hari

## 1. Informasi Task
* **Task ID**: `FE-HRD-254`
* **Slice / MVP**: `S-C5 / MVP-55`
* **Nama Fitur**: Layar Pengaduan Keluhan Staf (Grievance ESS) & Notulen Mediasi SLA 14 Hari
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka pengaduan perselisihan dan keluhan hubungan kerja dua mode (HR Relations & Mandiri ESS) (`relations-grievance-view.jsx`):
1. Mode Mandiri Pegawai (Portal ESS `/portal/grievance`): Pegawai dapat mendaftarkan keluhan seputar beban dinas, pembagian roster shift malam, klaim kompensasi lembur, atau konflik rekan kerja, serta memantau status secara transparan.
2. Mode Pengelolaan HR Relations (`/hrd/relations/grievance`): Tim HR memantau antrean tiket pengaduan seluruh unit rumah sakit dan memfasilitasi mediasi.
3. Pemantau Batas Waktu SLA Mediasi 14 Hari Kerja (VR-EMP-15): Sistem menghitung mundur sisa batas hari penanganan; jika mendekati atau melampaui 14 hari, indikator berubah menjadi peringatan kuning/merah tegas.
4. Pencatatan Notulen Mediasi Damai: Form pencatatan rangkuman klarifikasi para pihak, rumusan kesepakatan damai, atau keputusan eskalasi ke perundingan Bipartit RS.
5. Terhubung ke rute Next.js App Router `/hrd/relations/grievance`, `/corporate/hrd/relations/grievance`, dan `/portal/grievance`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hrd/relations/relations-grievance-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/relations/grievance/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/relations/grievance/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/portal/grievance/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Penghitungan batas SLA 14 hari tepat; filter tab memisahkan tiket aktif vs selesai; modal notulen mediasi dapat menyimpan kesepakatan damai; mode ESS hanya menampilkan tiket milik pengguna aktif.
* **Linter CI/CD**: `npm run lint:errors` lulus dengan 0 error.
* **Build Produksi**: Kompilasi Next.js Turbopack sukses dengan kode keluar 0.
* **Standar UI**: Zero-Tailwind, CSS Modules `relations.module.css`, dan Design Tokens Quilvian.
