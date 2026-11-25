# PAY-FE-FIX-035 — Saldo Awal PPh: dropdown Pegawai / Dokter dan hapus Dokumen Sumber (frontend)

Tanggal: 25 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `5a34cb9c8` (working tree belum di-commit). Backend: PAY-BE-FIX-035 (endpoint opsi baru; deploy BE lebih dulu).

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang; BE belum dibuild). Status UI paling tinggi 🟡.

## Perubahan
- `lib/constants/payroll/payroll-opening-tax-balance-constants.js`: `OPENING_TAX_BALANCE_WORKFORCE_RESOURCE` (resource khusus: endpoint `.../opening-tax-balances/workforce/options`, `valueKey: workforceProfileId`, `labelKeys: ["label"]`) menggantikan `OPENING_TAX_BALANCE_EMPLOYEE_RESOURCE`. Resource global `workforceProfiles` TIDAK diubah (modul lain aman). Hint kunci: "Pegawai/dokter, tahun pajak, ...".
- `payroll-opening-tax-balance-form-modal.jsx`: label "Pegawai / Dokter", placeholder "Pilih pegawai atau dokter", pencarian "Cari nama, NIP, atau nomor dokter...", empty/loading text; tetap `BaseSelectField` server-side (search, paginasi, loading); field Dokumen Sumber (label, textarea, placeholder, deskripsi, error, import) dihapus. Opsi tersimpan tetap tampil lewat `selectedOption` dari nama saldo.
- `utils/payroll/payroll-opening-tax-balance-utils.js`: `sourceReference` dihapus dari state form, validasi, dan payload; pesan "Pegawai atau dokter wajib dipilih."; normalisasi tetap membaca `sourceReference` legacy dari Backend.
- Tes: `payroll-opening-tax-balance-workforce.test.mjs` (baru, 5), `payroll-opening-tax-balance-screen.test.mjs` dan `payroll-opening-tax-balance.test.mjs` (jumlah pasangan izin 11 → 12) diperbarui.

## UI GATE
REUSE `BaseSelectField`, `useSelectResource` (resource objek khusus), `ConfirmModal`, field existing. Tanpa komponen/CSS baru.

## Validasi
`AUTOMATED TEST` (root workspace): `payroll-*.test.mjs` 613 tes, 608 lulus, 5 gagal tidak terkait (menu/rute/workforce-config pada berkas yang tidak diubah). ESLint tanpa temuan pada berkas yang diubah. `check-css-usage.mjs`: 0 temuan.

## UAT manual (belum dijalankan)
Tambah saldo: cari pegawai (nama, NIP) dan dokter (nama, nomor dokter); label menampilkan jenis; value terkirim = WorkforceProfileId; simpan draf tanpa Dokumen Sumber → Ajukan → Setujui oleh pengguna lain; detail saldo lama tetap menampilkan nama; Edit draf lama tidak menghapus dokumen sumber lama; resource Workforce modul lain tidak berubah.
