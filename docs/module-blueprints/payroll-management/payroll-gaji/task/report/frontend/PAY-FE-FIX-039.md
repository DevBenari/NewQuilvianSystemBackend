# PAY-FE-FIX-039 — Halaman Slip Gaji Saya (frontend)

Tanggal: 26 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `1eeefd40` (working tree belum di-commit). Backend: PAY-BE-FIX-039 (deploy BE dulu untuk pencarian, filter tahun, dan nama periode).

`VISUAL CHECK: NOT FEASIBLE`, `MANUAL TEST: NOT FEASIBLE` (dev server/build dilarang; BE belum dibuild). Status UI paling tinggi 🟡.

## Tidak diubah (NO CHANGE REQUIRED)
Rute `/payroll/my-payslips` dan `/payroll/my-payslips/{id}` (scope mine), redirect `/ess/payslips`, entri sidebar `corporateMyPayslips` (grup Layanan Mandiri, izin `MyPayslip:Read`), service `listMine/detailMine/downloadMine`, hook detail.

## Perubahan
`payroll-my-payslip-view.jsx`: eyebrow "Layanan Mandiri > Slip Gaji Saya" + subjudul sesuai keputusan; ringkasan (Total Slip Diterbitkan, Periode dan Gaji Bersih slip terbaru, tanggal terbit) dari muat awal tanpa filter; filter Tahun (`FilterSelect`) + pencarian dikirim ke Backend; tombol aksi mengikuti izin `MyPayslip:ReadDetail` dan `MyPayslip:Download`; pesan galat Backend (mis. akun belum terhubung profil) tampil sebagai alert. `payroll-payslip-table-columns.jsx`: kolom Periode Penggajian, Nomor Slip, Tanggal Terbit, Status Slip, Gaji Bersih, Aksi (Lihat Detail + Unduh PDF; tanpa nomor run); kode komponen teknis disembunyikan pada slip pribadi (`showCode: false`). `payroll-payslip-detail-view.jsx`: eyebrow Layanan Mandiri, kode komponen dan "Versi Kebijakan Hitung" disembunyikan untuk scope mine. `use-payroll-my-payslips.js`: filter `year`, `overview`. `payroll-payslip-utils.js`: `periodName`, `buildPayslipYearOptions`. `unified-ess-portal-view.jsx`: pintasan "Slip Gaji Saya" ke `/payroll/my-payslips`. Tes: `payroll-my-payslips.test.mjs` (7).

## UI GATE
REUSE `Hero`, `DataFilter`, `FilterSelect`, `DataTable`, `PayrollSection/PayrollInfoGrid`, `BaseButton`, `StatusBadge`; tanpa komponen/CSS baru.

## Validasi
`payroll-*.test.mjs` 629 tes, 624 lulus; 5 gagal tidak terkait (menu/rute/workforce-config, termasuk regex menu kebijakan pajak pada berkas yang tidak diubah). ESLint 0 error; `check-css-usage.mjs` 0 temuan (3 berkas).
