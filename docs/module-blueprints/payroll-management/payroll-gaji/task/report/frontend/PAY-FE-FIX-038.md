# PAY-FE-FIX-038 — Modal Lanjutkan Pengesahan: aksi utama Kembalikan untuk Dihitung Ulang (frontend)

Tanggal: 26 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `856875e6` (working tree belum di-commit). Backend: PAY-BE-FIX-038.

`VISUAL CHECK: NOT FEASIBLE`, `MANUAL TEST: NOT FEASIBLE` (dev server/build dilarang). Status UI paling tinggi 🟡.

## Perubahan
`payroll-run-utils.js`: `taxHistoryResolved` (gagal-tertutup) pada detail dan hasil evaluasi. `payroll-run-constants.js`: `followUpNoticeRecalculate`. `payroll-run-tax-modals.jsx` (`ContinueApprovalModal`): bila Backend menyatakan histori sudah lengkap dan pengguna boleh mengembalikan, tombol utama = "Kembalikan untuk Dihitung Ulang" (aksi Return existing, alasan wajib) dan "Selesaikan Data PPh"/"Profil Pajak Pegawai" disembunyikan; teks menjelaskan saldo awal sudah disetujui dan yang tersisa hanya hitung ulang. Tanpa komponen/CSS baru. Tes: `payroll-continue-approval.test.mjs`.

## Validasi
`payroll-*.test.mjs`: 622 tes, 617 lulus; 5 gagal tidak terkait (menu/rute/workforce-config pada berkas yang tidak diubah); ESLint 0 error; `check-css-usage.mjs` 0 temuan.
