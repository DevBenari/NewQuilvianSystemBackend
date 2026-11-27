# PAY-FE-FIX-040 — Tabel Penghasilan dan Potongan pada detail Slip Gaji Saya (frontend)

Tanggal: 26 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina` (working tree belum di-commit).

`VISUAL CHECK: NOT FEASIBLE`, `MANUAL TEST: NOT FEASIBLE` (dev server/build dilarang). Status UI paling tinggi 🟡. Backend: tanpa perubahan.

## Masalah (dari screenshot)
Tabel Penghasilan dan Potongan memakai tata letak otomatis tanpa lebar kolom tetap, sehingga kolom Keterangan dan Nominal berada pada posisi berbeda di dua tabel; kolom Keterangan hampir selalu berisi "-".

## Perubahan
`payroll-payslip-table-columns.jsx` (`buildPayslipLineColumns`): lebar kolom tetap Keterangan 160 dan Nominal 200 (Komponen mengisi sisa), opsi `showNote`. `payroll-payslip-detail-view.jsx`: kedua tabel memakai kolom yang sama (`lineColumns`); pada slip pribadi kolom Keterangan hanya tampil bila ada baris prorata pada penghasilan atau potongan. Tetap memakai `DataTable`, `PayrollSection`, dan kelas module existing; tanpa CSS baru. Slip administrator tidak berubah selain lebar kolom. Tes: `payroll-my-payslips.test.mjs`.

## Validasi
`payroll-*.test.mjs`: lulus kecuali 5 kegagalan lama yang tidak terkait (menu/rute/workforce-config). ESLint 0 error; `check-css-usage.mjs` 0 temuan.

## Risiko
Tampilan belum dilihat. Nama komponen pajak yang panjang dari snapshot (mis. "Pajak Penghasilan (PPh Pasal 21, ... kebijakan PPH21-2026-v1)") tetap ditampilkan apa adanya dan membungkus; mengubah nama itu adalah keputusan terpisah (isi snapshot tidak diubah).
