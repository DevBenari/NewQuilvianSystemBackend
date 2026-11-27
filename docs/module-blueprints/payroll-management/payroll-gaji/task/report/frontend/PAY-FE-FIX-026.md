# PAY-FE-FIX-026 — Metode Perhitungan dikunci untuk komponen terpakai (frontend)

Tanggal: 24 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `b05275ec` (working tree memuat PAY-FE-FIX-025 yang belum di-commit). Pasangan: PAY-BE-FIX-026.

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang). Status tugas UI: paling tinggi sebagian.

## File
| File | Perubahan |
| --- | --- |
| `src/lib/hooks/payroll/master-data/payroll-component/use-master-data-payroll-component-editor.jsx` | Pada mode Ubah, bila `detail.isUsedByPayroll`, field `calculationMethod` menjadi `readOnly` dengan keterangan |
| `src/lib/constants/payroll/master-data/payroll-component/payroll-component-constants.jsx` | Baris detail "Dipakai Payroll" |
| `tests/unit/payroll-component-default-values.test.mjs` | +2 tes (total 16 pada berkas ini) |

## Perilaku
Form Ubah komponen terpakai: Metode Perhitungan tidak dapat diubah dan bertuliskan "Komponen sudah dipakai data payroll, sehingga Metode Perhitungan tidak dapat diubah. Buat komponen baru bila metode perlu berbeda." Nama dan nilai default tetap dapat diubah. Penguncian hanya pada mode Ubah dan hanya setelah detail yang sesuai dimuat. Backend tetap menolak permintaan langsung (400); kunci UI hanya mencegah kegagalan yang pasti terjadi. Pergantian kategori ke tipe berbeda tidak dikunci di UI; penolakan Backend tampil sebagai toast galat.

## UI GATE
REUSE: `field.readOnly` dan `field.description` bawaan `BaseEditorField`; tanpa komponen atau CSS baru.

## Validasi
`AUTOMATED TEST`: `payroll-component-default-values` dan `payroll-component-category-refactor`: 32 lulus. `payroll-*.test.mjs` dari root workspace: 523 tes, 520 lulus, 3 gagal lama (module-contract x2, run-screens x1; tidak dibandingkan dengan HEAD bersih). ESLint: 0 error, 1 peringatan lama. `check-css-usage.mjs`: 0 temuan.

## Ketergantungan
Backend PAY-BE-FIX-026 harus diterapkan bersamaan: tanpa `isUsedByPayroll` pada detail, UI tidak mengunci field (tidak berbahaya, hanya tanpa petunjuk dini).
