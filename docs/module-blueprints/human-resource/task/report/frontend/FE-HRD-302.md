# Laporan Task FE-HRD-302: Layar Radar Pemantau Keterlambatan Settlement Overdue (VR-TRV-07 & INT-HR-99)

## 1. Informasi Task
* **Task ID**: `FE-HRD-302`
* **Slice / MVP**: `S-D5 / Perjalanan Dinas & Biaya Operasional (SPPD)`
* **Nama Fitur**: Layar Radar Pemantau Keterlambatan Settlement Overdue (`/hrd/travel/overdue-settlements`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun konsol radar pengawasan keterlambatan pertanggungjawaban dinas dan penegakan sanksi bertingkat:
1. **Layar Radar Overdue (`unified-travel-overdue-view.jsx`)**:
   - Menampilkan edukasi 4 tahapan SLA disiplin pertanggungjawaban: Hari 1-3 (Masa Aman Draf), Hari 4-7 (Peringatan H-1 Notifikasi), Hari &gt; 14 (Pembekuan Izin SPPD Baru), dan Hari &gt; 30 (Pemotongan Otomatis Slip Gaji Payroll).
   - Tabel radar nakes yang menunggak penyerahan berkas kuitansi (&gt; 7 hari kalender - `VR-TRV-07`), lengkap dengan nama nakes, unit kerja, tanggal kepulangan, durasi hari keterlambatan, dan sisa uang muka kasir yang menggantung.
   - Tombol pengiriman sirene pengingat cepat (*Nudge Reminder*) via WhatsApp & Portal ESS.
2. **Panel Simulasi Pemotongan Otomatis Slip Gaji (`INT-HR-99`)**:
   - Mensimulasikan pemotongan kasbon pada slip gaji pegawai di modul Penggajian Sub-10 untuk tunggakan yang melampaui 30 hari kalender.
   - Mengirimkan instruksi eksekusi pemotongan gaji secara idempoten.
3. **Pendaftaran Rute Next.js App Router**:
   - `/hrd/travel/overdue-settlements`: Rute radar overdue pada segment `(dashboard)/hrd`.
   - `/corporate/hrd/travel/overdue-settlements`: Rute radar overdue pada segment `(dashboard)/corporate/hrd`.

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/travel/unified-travel-overdue-view.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/hrd/travel/overdue-settlements/page.jsx`
* `QuilvianSystemFrontendDev/src/app/(dashboard)/corporate/hrd/travel/overdue-settlements/page.jsx`

## 4. Bukti Verifikasi & Hasil Pengujian
* **Hasil Verifikasi**: Radar overdue menampilkan nakes yang menunggak settlement secara akurat; simulasi potong payroll menghitung nominal kasbon secara presisi.
* **Kepatuhan Aturan Bisnis**: Menegakkan batasan batas waktu `VR-TRV-07` dan integrasi penggajian `INT-HR-99`.
