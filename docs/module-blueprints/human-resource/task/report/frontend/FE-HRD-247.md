# Laporan Task FE-HRD-247: Sidebar Menu Navigasi, Breadcrumbs & RBAC Route Guards /hrd/relations

## 1. Informasi Task
* **Task ID**: `FE-HRD-247`
* **Slice / MVP**: `S-C5 / MVP-54`
* **Nama Fitur**: Sidebar Menu Navigasi, Breadcrumbs & RBAC Route Guards `/hrd/relations`
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Mengintegrasikan seluruh antarmuka Hubungan Industrial & Terminasi ke sistem navigasi utama aplikasi:
1. Menambahkan grup navigasi sidebar `Hubungan Industrial & Terminasi` (`corporateEmployeeRelations`) pada `src/utils/menu-sidebar/menu-items.jsx` dengan ikon timbangan keadilan (`RiScales3Line`).
2. Menghubungkan 8 rute sub-menu:
   - Dasbor Hubungan Industrial & Radar: `/hrd/relations/dashboard`
   - Laporan Insiden & Just Culture: `/hrd/relations/incidents`
   - Penerbitan & Monitoring SP 1-3: `/hrd/relations/sanctions`
   - Skorsing Investigasi (Paid Suspension): `/hrd/relations/suspensions`
   - Pengaduan Staf & Whistleblowing: `/hrd/relations/grievance`
   - Risalah Forum Bipartit RS: `/hrd/relations/bipartite`
   - Pengajuan PHK & Pesangon PP 35/2021: `/hrd/relations/separations`
   - Pusat Kendali Exit Clearance: `/hrd/relations/exit-clearance`
3. Menambahkan menu mandiri portal staf ESS:
   - Pengaduan Keluhan Saya (ESS): `/portal/grievance`
4. Menyiapkan alias rute korporat `/corporate/hrd/relations/*` dan `/hrd/separations/*` untuk konsistensi pengalaman navigasi multi-peran.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx`
* Berkas-berkas rute App Router di bawah `src/app/(dashboard)/hrd/relations/`, `src/app/(dashboard)/corporate/hrd/relations/`, dan `src/app/(dashboard)/portal/`.

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Menu tampil secara tepat di sidebar navigasi; klik sub-menu membuka halaman rute yang sesuai tanpa redirect error; menu ESS `/portal/grievance` dapat diakses oleh akun pegawai.
* **Linter CI/CD**: `npm run lint:errors` lulus dengan 0 error.
* **Build Produksi**: Kompilasi Next.js Turbopack sukses dengan kode keluar 0.
* **Standar UI**: Konsisten dengan design token dan sistem navigasi Quilvian.
