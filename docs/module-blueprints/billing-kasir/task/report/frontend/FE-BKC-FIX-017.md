# FE-BKC-FIX-017 — Pembulatan pembayaran ke atas untuk semua metode (frontend)

Tanggal: 25 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `5a34cb9c8` (working tree belum di-commit). Backend: BE-BKC-FIX-027.

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang; BE belum dibuild). Status UI: paling tinggi sebagian.

## Perubahan
- `billing-split-payment-rows.js`: `roundUpToWholeRupiah`, `roundUpWhenCoversRemainder`, `computeRoundedCopyNominalAmount`, `maxCollectibleWithRounding` (menggantikan `maxCollectibleForMethod`); `findOverCollectibleRow` tanpa `isCashRow`; toleransi total `PAYMENT_ROUNDING_ALLOWANCE = 0.99` (dulu 0,5 hanya tunai); kekurangan alokasi (pembayaran sebagian) kini ditandai bila > Rp0,005 karena pembulatan hanya naik.
- `use-billing-settlement.js`: Copy Nominal dibulatkan ke atas; `commitRowAmount(localId)` membulatkan nominal yang menutup seluruh sisa saat blur; preflight tanpa `isCashRow`.
- `payment-allocation-row.jsx`: `onAmountCommit` pada `onBlur` input nominal.
- `menu-pembayaran-view.jsx`: baris "Pembulatan Pembayaran (ke atas)" tampil untuk metode apa pun (bukan hanya tunai); target `roundUpToWholeRupiah`.
- `detail/add-tender-modal.jsx`: "Gunakan sisa" = `roundUpToWholeRupiah(sisa)` (deposit tetap persis).
- Tes: `billing-payment-round-up.test.mjs` (baru, 8 tes); diperbarui `billing-cash-payment-sync`, `billing-payment-summary-cash-rounding`, `billing-split-payment-rows`.

## Validasi
`AUTOMATED TEST` (dari folder frontend): `billing-*.test.mjs`: 441 tes, 435 lulus, 6 gagal yang **sudah ada sebelum perubahan** (`backend: filter EntityId…`, `FE-BKC-042 AC-6`, tiga `billing-settlement-active-selection`, `finalisasi otomatis…`). ESLint: 0 error (65 peringatan lama). `check-css-usage.mjs`: 7 temuan lama di `menu-pembayaran-view.jsx` (baris 1059–1668), tidak ada yang baru. UI GATE: REUSE komponen/kelas yang ada; tanpa CSS/komponen baru.

## UAT manual
1. Sisa Rp24.461.828,74, Tunai: Gunakan sisa → 24.461.829; uang diterima 25.000.000 → kembalian 538.171; Proses → tender SUCCEEDED, adjustment Debit Rp0,26.
2. Ulangi dengan QRIS/Transfer: nominal 24.461.829, sukses, adjustment sama.
3. Split Tunai Rp10.000.000 + Transfer sisanya: nominal sebagian tidak berubah; sisa dibulatkan ke atas pada baris yang menutup sisa.
4. Ketik 24461828,74 lalu blur: berubah menjadi 24461829.

## Risiko
Tampilan belum dilihat; BE belum dibuild; kasus AR Karyawan/split dan provider non-tunai butuh UAT (lihat BE-BKC-FIX-027).
