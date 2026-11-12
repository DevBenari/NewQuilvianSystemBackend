# Laporan Task FE-HRD-226: Tata Kelola Kontrak Ikatan Dinas Retensi Nakes & Perhitungan Denda Resign

## 1. Informasi Task
* **Task ID**: `FE-HRD-226`
* **Slice / MVP**: `S-C2 / MVP-50`
* **Nama Fitur**: Layar Administrasi & Monitoring Ikatan Dinas Tenaga Medis
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka pengelolaan ikatan dinas nakes pada rute `/hr/learning/bonds`, `/admin/learning/bonds`, dan `/corporate/hrd/learning/bonds`:
1. Tabel Pemantauan Nakes dengan Ikatan Dinas Aktif pasca pelatihan/fellowship bernilai > Rp 10 Juta atau durasi > 1 bulan (VR-TRN-16, AT-HRD-TRN-17).
2. Kolom parameter: Nomor Akta Resmi (AKT/HRD-IKD/...), Nama Nakes, Judul Fellowship yang Dibiayai RS, Total Biaya Investasi RS, Durasi Retensi (Maksimal 36 Bulan - VR-TRN-18), dan Visual Counter Sisa Bulan Retensi.
3. Badge status ikatan dinas (Aktif Berjalan, Wanprestasi / Denda, Selesai).
4. Tombol aksi pembuka modal akta perjanjian dan kalkulator tagihan ganti rugi resign.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/learning/training-bonds-view.jsx`
* `QuilvianSystemFrontendDev/src/app/hr/learning/bonds/page.jsx`
* `QuilvianSystemFrontendDev/src/app/admin/learning/bonds/page.jsx`
* `QuilvianSystemFrontendDev/src/app/corporate/hrd/learning/bonds/page.jsx`

## 4. Bukti Verifikasi
* Pelatihan berbiaya tinggi > Rp 10 Juta terdeteksi wajib akta ikatan dinas (VR-TRN-16 / AT-HRD-TRN-17).
* Batas masa retensi kerja 36 bulan terpantau akurat (VR-TRN-18).
* Memenuhi kriteria penerimaan AT-HRD-TRN-18.
