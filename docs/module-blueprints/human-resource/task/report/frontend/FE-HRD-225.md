# Laporan Task FE-HRD-225: Rencana Program Tahunan TNA & Monitoring Pagu Anggaran Unit

## 1. Informasi Task
* **Task ID**: `FE-HRD-225`
* **Slice / MVP**: `S-C2 / MVP-50`
* **Nama Fitur**: Layar Perencanaan Tahunan TNA & Pengendalian Pagu Anggaran Departemen
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-10

## 2. Ringkasan Implementasi
Membangun antarmuka perencanaan program kerja tahunan diklat pada rute `/hr/learning/annual-plans`, `/admin/learning/budget-and-bonds`, dan `/corporate/hrd/learning/annual-plans`:
1. KPI Ringkasan Anggaran RS: Total Pagu Alokasi (Rp 450 Jt), Anggaran Komitmen Berjalan (Rp 185 Jt - VR-TRN-15), Realisasi Pembayaran (Rp 120 Jt), dan Sisa Saldo Bebas (Rp 145 Jt).
2. Tabel Master Rencana Program Tahunan TNA dengan tombol pengesahan eksekutif Direktur SDM (AT-HRD-TRN-08).
3. Papan Pemantauan Pagu Anggaran Unit Kerja (Pelayanan Medis, IGD & Intensif, Penunjang Medis) dengan progress bar dan indikator peringatan saat saldo pagu menipis (< 15%).
4. Simulasi Pemblokiran Anggaran Defisit (VR-TRN-14, AT-HRD-TRN-11): Sistem menolak pendaftaran diklat jika biaya melebihi sisa saldo anggaran unit (HTTP 422 Unprocessable Entity).

## 3. Berkas yang Diubah / Dibuat
* `QuilvianSystemFrontendDev/src/components/view/hr/learning/annual-plans-view.jsx`
* `QuilvianSystemFrontendDev/src/app/hr/learning/annual-plans/page.jsx`
* `QuilvianSystemFrontendDev/src/app/admin/learning/budget-and-bonds/page.jsx`
* `QuilvianSystemFrontendDev/src/app/corporate/hrd/learning/annual-plans/page.jsx`

## 4. Bukti Verifikasi
* Anggaran komitmen berjalan dan sisa pagu unit terpantau secara transparan (VR-TRN-15).
* Simulasi penolakan over-budgeting membuktikan penegakan aturan VR-TRN-14 dan skenario AT-HRD-TRN-11.
* Pengesahan program kerja TNA oleh Direktur terekam sukses (AT-HRD-TRN-08).
