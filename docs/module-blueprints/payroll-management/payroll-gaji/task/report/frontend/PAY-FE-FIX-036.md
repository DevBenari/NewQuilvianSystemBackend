# PAY-FE-FIX-036 — Popup manual Saldo Awal PPh dan impor massal (frontend)

Tanggal: 26 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `856875e6` (working tree belum di-commit). Backend: PAY-BE-FIX-036 (deploy BE lebih dulu).

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang; BE belum dibuild). Status UI paling tinggi 🟡.

## Perubahan
- Popup manual (`payroll-opening-tax-balance-form-modal.jsx`, `payroll-run.module.css`): `modalClassName` 840 px (class khusus, ConfirmModal global tidak berubah), body bergulir dengan footer tetap, grid dua kolom (Pegawai/Dokter penuh; Tahun+Sumber; Bulan penuh; nominal 2×2: Kena Pajak, PPh Dipotong, Premi, Pensiun), bulan 6 kolom (4 tablet, 3 mobile) dengan singkatan + tooltip nama lengkap; satu kolom ≤575 px. Payload, validasi, dan field tidak berubah.
- Impor: tombol "Impor Saldo Awal PPh" (izin Create + ViewAmount) → `payroll-opening-tax-balance-import-modal.jsx` (pilih berkas + Unduh Template → pratinjau hasil validasi Backend: ringkasan, tabel per baris dengan paginasi dan filter "hanya bermasalah", kode galat tertulis → hasil). `use-payroll-opening-tax-balance-import.js`: berkas hanya diuraikan menjadi baris teks mentah; Backend memvalidasi; konfirmasi hanya bila `invalidRows === 0`; commit memvalidasi ulang; penolakan 422 mengganti pratinjau; kunci anti kirim ganda.
- `payroll-variable-input-utils.js`: `parseCsvRecords` (nomor baris fisik, abaikan baris `#`); `parseCsvText` kini membungkusnya (perilaku sama). Utils/service/konstanta impor ditambahkan di berkas Saldo Awal PPh.
- Tes: `payroll-opening-tax-balance-import.test.mjs` (baru, 7); `payroll-opening-tax-balance.test.mjs` (15 pasangan izin).

## UI GATE
REUSE `ConfirmModal`, `DataTable`, `BaseCheckboxField`, `BaseFormControl`, `PayrollInfoGrid`, `StatusBadge`, `InformationAlert`, input berkas seperti impor penghasilan variabel; EXTEND tambahan parser CSV (aditif); CSS Module domain existing (token saja).

## Validasi
`AUTOMATED TEST` (root workspace): `payroll-*.test.mjs` 620 tes, 615 lulus, 5 gagal tidak terkait (menu/rute/workforce-config pada berkas yang tidak diubah). ESLint 0 error; `check-css-usage.mjs` 0 temuan (3 berkas).

## UAT manual (belum dijalankan)
Popup pada 1440/1024/768/390 px; unduh template (UTF-8, dibuka di Excel); unggah berkas valid (pegawai dan dokter), berkas dengan header salah, kode tidak dikenal/ambigu, duplikat, bulan/nominal tidak valid, konflik draf/Approved/run final; commit → draf muncul; ulangi commit → ditolak; ajukan lalu setujui oleh pengguna lain.
