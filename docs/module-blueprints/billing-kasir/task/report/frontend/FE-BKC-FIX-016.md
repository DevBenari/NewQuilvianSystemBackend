# FE-BKC-FIX-016 — Pembayaran tunai tanpa pemotongan desimal, sinkron settlement, dan Ringkasan Pembayaran rapi (frontend)

Tanggal: 25 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `5a34cb9c8` (working tree belum di-commit). Backend: BE-BKC-FIX-026 (`Yasmina`, `c073f8c1`).

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang; BE belum dibuild; tidak ada transaksi nyata). Status UI: paling tinggi sebagian.

## Perubahan
1. **Hentikan pemotongan desimal**: `detail/add-tender-modal.jsx` memakai `Math.trunc(collectible)` untuk "Gunakan sisa/Copy Nominal" sehingga Rp24.461.828,74 terkirim 24.461.828. Kini `to2Decimals` (presisi dua desimal, sama dengan seluruh Menu Pembayaran). `submitOneRow` mengirim `to2Decimals(Number(row.amount))`.
2. **Sinkron sebelum POST** (`use-billing-settlement.js`, `submitAllocations`): bila settlement sudah ada, settlement diambil ulang dari Backend lebih dulu; RowVersion yang dipakai milik server saat ini; setiap baris diperiksa berurutan terhadap sisa yang dapat ditagih terbaru (`findOverCollectibleRow`, tunai boleh sampai pembulatan rupiah penuh seperti Backend). Baris yang tidak muat TIDAK dikirim: ditandai gagal dengan pesan spesifik (sisa, pembayaran menunggu, pembayaran yang sudah tercatat), settlement dimuat ulang, dan tidak ada POST.
3. **Error 422/409**: 422 "melebihi saldo" kini menyinkronkan settlement (seperti 409). Jawaban validasi 400/422 tampil pada barisnya dan tidak lagi memunculkan toast generik kedua; 409 dan kegagalan tanpa jawaban pasti tetap lewat handler umum. Baris gagal mempertahankan inputnya (nominal, metode, uang diterima); percobaan hanya dilepas pada 409.
4. **Helper murni** (`billing-split-payment-rows.js`): `maxCollectibleForMethod`, `findOverCollectibleRow`, `buildOverCollectibleMessage`.
5. **Ringkasan Pembayaran** (`menu-pembayaran-view.jsx`, `menu-pembayaran.module.css`): setelah blok tagihan hingga "Harus Dibayar Pasien", tiga kelompok berjudul: **Kewajiban Pasien** (Sudah Dibayar terkonfirmasi berwarna sukses, Sisa Tagihan, Menunggu Konfirmasi), **Alokasi Pembayaran (draft)** (AR Karyawan, Draft Deposit, Draft Pembayaran, Total Draft, **Sisa Belum Dialokasikan** menonjol, Saldo Deposit estimasi), **Rincian Tunai (draft)** (Uang Tunai Diterima, Kembalian, petunjuk kembalian harus diserahkan dan dikonfirmasi). Baris draft memakai garis putus-putus di kiri agar berbeda dari pembayaran berhasil. Baris ringkasan memakai grid: label rata kiri dan boleh membungkus, nominal rata kanan, `tabular-nums`, `nowrap`; ≤400 px label di atas dan nominal rata kanan di bawahnya. Uang fisik tidak masuk alokasi: sisa tetap `remainingAllocationAmount` (NETO). Rincian Tunai kini tampil juga untuk uang pas.

Tidak diubah: field Nominal Pembayaran Tagihan / Uang Tunai Diterima / Kembalian (read-only) / konfirmasi kembalian / tombol Uang Pas (`payment-allocation-row.jsx`), split payment, pemulihan dan pembatalan tender CREATED.

## File
`src/components/view/health-services/billing-management/billing-invoices/detail/add-tender-modal.jsx`, `…/menu-pembayaran/menu-pembayaran-view.jsx`, `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-settlement.js`, `…/billing-split-payment-rows.js`, `src/style/health-services/billing-management/menu-pembayaran.module.css`, `tests/unit/billing-cash-payment-sync.test.mjs` (baru, 9 tes), `tests/unit/billing-employee-ar-draft.test.mjs` (asersi kelas baris diselaraskan).

## UI GATE
REUSE `BaseButton`, `StatusBadge`, kelas module yang ada; satu perluasan CSS Module domain existing (`menu-pembayaran.module.css`, nilai berbasis token `--pay-*`). Tanpa komponen base baru.

## Validasi
`AUTOMATED TEST` (dari folder frontend): `billing-*.test.mjs`: 433 tes, 427 lulus, 6 gagal yang **sudah ada sebelum perubahan ini** (`backend: filter EntityId…`, `FE-BKC-042 AC-6`, tiga tes `billing-settlement-active-selection`, `finalisasi otomatis…`); tidak ada kegagalan baru. 9 tes baru lulus. ESLint: 0 error (65 peringatan lama). `check-css-usage.mjs`: 7 temuan lama pada baris 1059–1668 `menu-pembayaran-view.jsx` (di luar perubahan ini); tidak ada temuan baru pada kode yang diubah.

## Skenario UI
TC-13/14: VERIFIED STATIC (tes perilaku). TC-17: IMPLEMENTED (input dipertahankan). TC-18/19: VERIFIED STATIC pada CSS dan teks source; tampilan **NOT RUN**. TC-20: IMPLEMENTED, **NOT RUN**.

## UAT manual
1. Invoice dengan sisa berdesimal (mis. Rp24.461.828,74), metode Tunai: pilih Gunakan sisa; nominal tampil dan terkirim 24.461.828,74; uang diterima Rp25.000.000; kembalian Rp538.171,26; centang konfirmasi kembalian; Proses Pembayaran.
2. Ulangi Proses Pembayaran segera: harus tertahan (settlement lunas/pesan spesifik), tanpa pembayaran kedua dan tanpa toast ganda.
3. Split Tunai + Transfer; ringkasan desktop/tablet/mobile; label panjang tidak merusak perataan.

## Risiko
- Tampilan belum dilihat; Backend belum dibuild.
- Penyebab 422 aktual belum terbukti tanpa data settlement/tender (lihat BE-BKC-FIX-026).
- Pemeriksaan awal klien hanya pelengkap; Backend tetap memutuskan.
