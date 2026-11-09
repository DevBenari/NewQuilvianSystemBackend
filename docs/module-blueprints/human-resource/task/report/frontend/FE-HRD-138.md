# Laporan Task FE-HRD-138: Layar Dasbor Otorisasi 3-Jenjang Penggajian & Rekapitulasi Eksekutif Finansial (/hr/payroll/approvals)

## 1. Informasi Task
* **Task ID**: `FE-HRD-138`
* **Slice / MVP**: `S-B5` / `MVP-31`
* **Nama Fitur**: Layar Dasbor Otorisasi 3-Jenjang Penggajian & Rekapitulasi Eksekutif Finansial (/hr/payroll/approvals)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-09

## 2. Ringkasan Implementasi
Mengembangkan layar dasbor persetujuan eksekutif penggajian (FE-HRD-166) bagi HR Manager dan Direktur:
1. Menyajikan 4 kartu rekapitulasi beban finansial rumah sakit: Total Beban Gaji Bersih, Total Beban Bruto, Kewajiban Penyetoran PPh 21 TER, dan Kontribusi Iuran BPJS Rumah Sakit (Pemberi Kerja).
2. Switcher simulasi peran peninjau aktif (HR Manager vs Direktur SDM/Keuangan) untuk memvalidasi alur otorisasi multi-role.
3. Daftar batch penggajian yang menunggu keputusan dengan visual status alur berjenjang.
4. Integrasi persetujuan dan penolakan langsung ke API otorisasi backend.

## 3. Berkas yang Diubah / Dibuat
* `src/components/view/hr/payroll/payroll-approvals-view.jsx (Dibuat)`
* `src/app/hr/payroll/approvals/page.jsx (Dibuat)`

## 4. Bukti Verifikasi
* Lolos uji pergantian wewenang peran persetujuan, rekonsiliasi beban finansial, ESLint 0 error, build Next.js 0 error.
