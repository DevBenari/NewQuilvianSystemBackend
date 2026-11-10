# Laporan Task FE-HRD-266: Layar Registri Pembatasan Tugas Medis (Work Restrictions)

## 1. Informasi Task
* **Task ID**: `FE-HRD-266`
* **Slice / MVP**: `S-C6 / MVP-57`
* **Nama Fitur**: Layar Registri Pembatasan Tugas Medis (Work Restrictions)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun layar pengawasan pembatasan tugas kerja nakes pada rute `/hrd/occupational-health/restrictions`:
1. Registri Restriksi Aktif: Menampilkan seluruh nakes yang memiliki batasan penugasan kerja medis beserta alasan klinis dan dokter pemeriksa.
2. Penegakan Hard-Stop Roster Sub-06 (`INT-HR-89`, `VR-OHS-04`): Indikator visual gembok roster bertanda merah yang mengunci sistem penjadwalan dinas agar tidak menempatkan staf pada giliran kerja/ruangan berbahaya.
3. Filter & Status: Pencarian berdasarkan kategori larangan dinas dan status restriksi (Aktif / Telah Dicabut melalui RTW).
4. Rute App Router: `/hrd/occupational-health/restrictions` dan `/corporate/hrd/occupational-health/restrictions`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/occupational-health/ohs-work-restrictions-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/occupational-health/restrictions/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/occupational-health/restrictions/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Daftar restriksi tampil lengkap dengan penanda hard-stop roster aktif; integrasi tombol asesmen RTW untuk pencabutan restriksi.
* **Standar UI**: Zero-Tailwind, CSS Modules `occupational-health.module.css`.
