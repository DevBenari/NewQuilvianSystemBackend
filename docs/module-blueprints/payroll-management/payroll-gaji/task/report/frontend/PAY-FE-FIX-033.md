# PAY-FE-FIX-033 — Lanjutkan Pengesahan pada Persetujuan Penggajian (frontend)

Tanggal: 25 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `5a34cb9c8` (working tree belum di-commit). Backend: PAY-BE-FIX-033.

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang; BE belum dibuild; tanpa transaksi nyata). Status UI paling tinggi 🟡.

## Perubahan
- `lib/constants/payroll/payroll-run-constants.js`: aksi `CONTINUE_APPROVAL`, `PAYROLL_RUN_ROW_ACTIONS.continueApproval`, `PAYROLL_CONTINUE_APPROVAL_TEXT`. `payroll-constants.js`: `PAYROLL_ROUTES.openingTaxBalances`.
- `utils/payroll/payroll-run-utils.js`: `normalizeRunDetail` (5 field baru, gagal-tertutup), `resolveRunActions.canContinueApproval` (tanpa fallback status), `evaluateRunDecision("continueApproval")` (status `continue`; `finalReady` hanya bila `canApproveFinal === true` dan Approve tersedia), `buildContinueApprovalItems`.
- `lib/hooks/payroll/use-payroll-run-row-action.js`: `canContinueApproval` (izin PayrollRun:Approve); `confirm` hanya untuk `finalReady` (endpoint approve existing, tanpa `approvalMode`). `use-payroll-run-detail.js`: `canContinueApproval`.
- `payroll-run-table-columns.jsx`: tombol **Lanjutkan Pengesahan** hanya untuk Submitted + PPh sementara + sudah disahkan bersyarat; Sahkan Bersyarat tidak muncul lagi setelah itu; kolom Aksi layar persetujuan 400 px. `payroll-run-list-view.jsx`, `payroll-run-detail-view.jsx`: tombol yang sama.
- `payroll-run-tax-modals.jsx` (`ContinueApprovalModal`) + `payroll-run-row-action-modal.jsx`: modal lebar 820 px: Ringkasan Payroll Run, Status PPh (valid vs estimasi, histori), alasan dan langkah dari Backend, aksi **Selesaikan Data PPh** (Saldo Awal PPh), **Profil Pajak Pegawai**, **Buka Detail Run**, **Kembalikan untuk Dihitung Ulang** (aksi Return existing dengan alasan wajib), Tutup. Bila Backend menyatakan siap final: tombol **Sahkan Payroll Run**.
- `style/payroll/payroll-run.module.css`: `.pphSteps` (token saja). Tes: `payroll-continue-approval.test.mjs` (baru, 6), `payroll-run-screens.test.mjs` (kunci `canContinueApproval`).

## UI GATE
REUSE `BaseButton`, `ConfirmModal`, `StatusBadge`, `DataTable`, `PayrollNoticeList`, kelas module yang ada; satu tambahan kelas CSS Module domain existing. Tanpa komponen base baru.

## Validasi
`AUTOMATED TEST` (root workspace): `payroll-*.test.mjs`: 605 tes, 600 lulus, 5 gagal yang tidak terkait (menu/rute/workforce-config: `payroll-data-readiness`, `payroll-module-contract` x2, `payroll-policy`, `payroll-run-screens` menu) pada berkas yang tidak diubah. ESLint: 0 error (61 peringatan lama). `check-css-usage.mjs`: 0 temuan pada 5 berkas run yang diubah.

## UAT manual (belum dijalankan)
Run PR-202612-E08FC5: Submitted + PPh sementara + disahkan bersyarat → tombol Lanjutkan Pengesahan (tanpa Sahkan Bersyarat); modal menampilkan alasan/langkah; Kembalikan → Calculated, hitung ulang, ajukan, Sahkan normal; akun tanpa izin Approve tidak melihat tombol; pengaju = pengesah dengan dua izin; tombol sejajar header AKSI; modal responsif.

## Risiko
Tampilan belum dilihat; BE belum dibuild; aksi Lanjutkan Pengesahan tidak muncul sampai BE baru men-deploy `ContinueApproval` pada availableActions (gagal-tertutup).
