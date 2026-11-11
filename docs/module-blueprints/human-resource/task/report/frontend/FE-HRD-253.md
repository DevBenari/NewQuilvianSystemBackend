# Laporan Task FE-HRD-253: Layar Pengelolaan Skorsing Investigasi (Paid Suspension) & Status Akses SIMRS

## 1. Informasi Task
* **Task ID**: `FE-HRD-253`
* **Slice / MVP**: `S-C5 / MVP-55`
* **Nama Fitur**: Layar Pengelolaan Skorsing Investigasi (*Paid Suspension*) & Status Akses SIMRS
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka pengelolaan penetapan skorsing investigasi dugaan pelanggaran berat (`relations-suspensions-view.jsx`):
1. Jaminan Hak Upah Penuh 100% (Pasal 157A UU Ketenagakerjaan & VR-EMP-14): Validasi sistem mencegah pemotongan upah sepihak; upah pokok dan tunjangan tetap diwajibkan 100% terbayar selama proses pemeriksaan berjalan.
2. Pemutusan Akses Sistem SIMRS Otomatis (INT-HR-88):
   - Selama masa skorsing aktif, akses kredensial ke sistem informasi rumah sakit, sistem EMR rekam medis pasien, PACS radiologi, serta kartu akses pintu RS dinonaktifkan (revoked).
   - Disediakan saklar kontrol pemulihan akses (*Restore Access*) apabila terperiksa terbukti tidak bersalah.
3. Batasan Waktu Investigasi: Pemantauan batas durasi pemeriksaan maksimal 6 bulan (180 hari).
4. Penutupan Skorsing: Aksi pemulihan nama baik dan hak kerja (*Reinstatement*) atau pengalihan ke proses Pemutusan Hubungan Kerja (PHK).
5. Terhubung ke rute Next.js App Router `/hrd/relations/suspensions` dan `/corporate/hrd/relations/suspensions`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hrd/relations/relations-suspensions-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/relations/suspensions/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/relations/suspensions/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Form menolak jika hak upah < 100%; saklar revokasi akses SIMRS mengubah status secara visual dan memanggil endpoint backend INT-HR-88; tombol pemulihan merekam waktu penutupan kasus.
* **Linter CI/CD**: `npm run lint:errors` lulus dengan 0 error.
* **Build Produksi**: Kompilasi Next.js Turbopack sukses dengan kode keluar 0.
* **Standar UI**: Zero-Tailwind, CSS Modules `relations.module.css`, dan Design Tokens Quilvian.
