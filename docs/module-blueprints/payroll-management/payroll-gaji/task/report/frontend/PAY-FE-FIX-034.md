# PAY-FE-FIX-034 — Persetujuan Payroll — PPh Belum Final: label, tahap, dan modal (frontend)

Tanggal: 25 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `5a34cb9c8` (working tree belum di-commit). Backend: PAY-BE-FIX-034.

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang; BE belum dibuild). Status UI paling tinggi 🟡.

## Perubahan
- `payroll-run-constants.js`: badge `disahkan_bersyarat` → "Disetujui Administratif"; teks aksi/modal → "Persetujuan Payroll — PPh Belum Final", tombol "Setujui Administratif"; panel "Disetujui Administratif — Menunggu Finalisasi PPh"; `PAYROLL_CONTINUE_APPROVAL_TEXT` disusun ulang (bagian, label tahap, konsekuensi); panel "Alasan Pengesahan Final Belum Dapat Dilakukan" dihapus.
- `payroll-run-utils.js`: `approvalStage` pada list item dan detail (Backend lama = kosong); `buildContinueApprovalItems` memuat Status Persetujuan.
- `payroll-run-tax-modals.jsx` (`ContinueApprovalModal`): urutan Ringkasan Run → Keputusan Persetujuan (referensi, waktu, alasan) → Ringkasan PPh → Pegawai Terdampak → Penahan Lain (hanya bila ada penahan NYATA dari Backend) → Tindak Lanjut Finalisasi PPh (peringatan + langkah) → Konsekuensi → tombol. Aksi: Selesaikan Data PPh, Profil Pajak Pegawai, Buka Detail, Kembalikan; Sahkan hanya bila Backend menyatakan siap final.
- `payroll-run-table-columns.jsx`: run yang sudah disetujui menampilkan "Menunggu finalisasi PPh" (bukan "Sahkan ditahan"). `payroll-run-provisional-tax-panel.jsx`: label "Disetujui Administratif".
- Tes: `payroll-continue-approval.test.mjs` (diperbarui, 9 tes).

Tidak diubah: tombol mengikuti `availableActions` Backend dan izin RBAC (`resolveRunActions`, hook); `canApprove` normal tetap tertutup selama PPh sementara (itu jalur final, bukan persetujuan administratif); tanpa tombol ganda; refresh daftar/detail lewat `onChanged`.

## Validasi
`AUTOMATED TEST` (root workspace): `payroll-*.test.mjs` 608 tes, 603 lulus, 5 gagal tidak terkait (menu/rute/workforce-config pada berkas yang tidak diubah). ESLint 0 error. `check-css-usage.mjs` 0 temuan pada berkas yang diubah.

## UAT manual (belum dijalankan)
Run PR-202612-E08FC5: daftar menampilkan "Disetujui Administratif" + "Menunggu finalisasi PPh" + referensi CA-…-1; tombol Lanjutkan Pengesahan membuka modal berurutan tanpa panel penghalang; Return → Calculated → hitung ulang → ajukan → Sahkan; akun tanpa izin Approve tidak melihat tombol; klik ganda tidak membuat riwayat ganda.
