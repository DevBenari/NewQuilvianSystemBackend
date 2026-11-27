# BE-BKC-FIX-027 — Pembulatan pembayaran ke atas ke rupiah penuh untuk semua metode (backend)

Tanggal: 25 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `c073f8c1` (working tree belum di-commit). Frontend: FE-BKC-FIX-017.

`dotnet build`: **NOT RUN** (dilarang). `MIGRATION`: tidak ada. `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat tes statis FE (`billing-payment-round-up.test.mjs`, `billing-cash-payment-sync.test.mjs`). Preflight: Area HealthServices/BillingManagement/Billing, `Bil`, `TOUCHED LEGACY`, tanpa entity baru.

## Keputusan pemilik Billing (25 Nov 2026)
Nominal pembayaran pasien dibulatkan **ke atas** ke rupiah penuh untuk **semua metode** (tunai dan non-tunai). Contoh: sisa Rp24.461.828,74 → nominal Rp24.461.829; selisih Rp0,26 dicatat sebagai `BilAdjustment` pembulatan (Debit); kembalian dari Rp25.000.000 = Rp538.171 (bulat). Kebijakan lama: hanya tunai, pembulatan terdekat (0,5 naik).

## Perubahan (`Areas/HealthServices/BillingManagement/Billing/Services/`)
- `BillingPaymentRounding.cs` (baru): `RoundUp(amount)` = `amount <= 0 ? 0 : Math.Ceiling(decimal.Round(amount, 2))`. Satu-satunya sumber pembulatan pembayaran.
- `BillingSettlementService.cs`: batas `maxCollectible = Max(collectible, RoundUp(collectible))` untuk semua metode; `BuildOverCollectibleException` kehilangan parameter `isCash`; blok penyesuaian pembulatan di rekonsiliasi tidak lagi bersyarat `IsCash` dan memakai `RoundUp`; guard pembuatan settlement, `MapSettlement`, pemetaan tender memakai `RoundUp`; teks audit `BilAdjustment` diselaraskan ("pembulatan pembayaran ke atas").
- `BillingCalculationService.cs`: `roundedPayable = RoundUp(originalPayable)`.
- `BillingInvoiceService.cs`: pengecekan riwayat lunas dan pratinjau pembayaran memakai `RoundUp`; pratinjau tidak lagi `isCash &&` (`IsCashRoundingApplied` dipertahankan sebagai nama kontrak, kini berarti "pembulatan diterapkan").
- `BillingEncounterPaymentHistoryService.cs`: `roundedTarget = RoundUp(target)`.
- `Dtos/BillingInvoiceDtos.cs`: hanya komentar.

Tidak diubah: urutan guard (RowVersion, status Settled), tender CREATED/PENDING, idempotensi, advisory lock + Serializable, `ResolveCashBreakdown` (uang diterima wajib ≥ Amount, kembalian dihitung server, konfirmasi kembalian), `ValidateMoney` (maks 2 desimal), pesan 409/422 spesifik BE-BKC-FIX-026.

## Risiko / perlu keputusan
- Pembulatan ke atas selalu menghasilkan **Debit** (pasien membayar lebih besar dari tagihan dua desimal; selisih < Rp1 dicatat sebagai penyesuaian, bukan pendapatan lain/kembalian).
- Settlement dengan AR Karyawan/split: penyesuaian hanya dibuat bila `tender.Amount == RoundUp(outstanding invoice)` dan selisih < Rp1; bila tidak, kelebihan < Rp1 tanpa penyesuaian. Perlu UAT.
- Provider non-tunai (QRIS/transfer/EDC) kini menerima nominal bulat; periksa rekonsiliasi dengan penyedia.
- Sumber desimal pada perhitungan tagihan tidak diubah.
- Data lama dengan penyesuaian pembulatan ke bawah (negatif) tetap valid.
- Belum dibuild; kompilasi perlu build. `Migrations/ApplicationDbContextModelSnapshot.cs` masih modified di working tree (bukan dari pekerjaan ini).
