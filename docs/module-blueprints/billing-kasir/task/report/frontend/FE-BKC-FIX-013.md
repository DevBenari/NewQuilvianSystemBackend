# Laporan Perubahan Frontend — `FE-BKC-FIX-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-013` |
| Judul | Daftar Invoice & Billing Kasir memisahkan Total Tagihan, Sudah Dibayar, Sisa Pasien, dan Status Tagihan |
| Sumber | [`PLAN-REPAIR-BKC-001`](../../../docs/plan-repair/plan-repair-001-integrasi-checkout-ke-billing-kasir.md) `FIX-BKC-001-10`; temuan `ISS-BKC-001-08` |
| Keputusan | `BKC-DEC-141` (K-07) |
| Dependency | `BE-BKC-FIX-020` (field `calculationState`, `runningGrossAmount`, `isPatientAmountProvisional`, `isCashPayer`, `pendingChargeCount`). Tanpa backend baru, respons lama dibaca sebagai "Terkini" sehingga layar tetap berjalan |
| Wewenang UI | `REUSE`: `DataTable`, `StatusBadge`, `InformationAlert`, `BaseButton`, `DataFilter`. Tidak ada base component baru |
| Target tulis | `QuilvianSystemFrontendDev` |
| Commit saat dikerjakan | `7d44526ff` (`HamzahV2`) + working tree; belum di-commit |
| Tanggal | 2026-10-12 |
| Status | ✅ **Source selesai**; lint 0 error; build lulus; uji unit lulus. Pemeriksaan visual dengan backend baru `NOT RUN` |

## 1. Masalah

Kolom "Biaya & Tgl Bayar" berisi **pembayaran** (`totalPaidAmount`), sehingga invoice Rp200.000 yang belum dibayar tampil Rp0 dan terbaca "biaya nol". Total invoice yang sudah dikirim backend tidak ditampilkan. Angka dari hitungan lama atau sisa Rp0 penjamin yang belum dihitung tampil sebagai kewajiban pasti.

## 2. Tampilan sesudah perbaikan

```text
Invoice & Billing Kasir
[ Cari nama/RM/penjamin ] [ Semua Periode ▾ ] [Tanggal Mulai] — [Tanggal Akhir]  [ Cari ] [ Muat Ulang ]

⚠ Ada pelayanan yang belum masuk invoice
  2 pelayanan pada halaman ini sudah layak ditagih tetapi belum menjadi item invoice … Lihat antrean rekonsiliasi

No  Pasien    Total Tagihan        Sudah Dibayar & Tgl  Sisa Pasien               Status Tagihan                    Info Deposit …  Aksi
1   Pasien A  Rp150.000            Rp0 / -              Rp150.000  [Belum Bayar]  [Terkini]                         …               ⋮
2   Pasien B  Belum dihitung       Rp0 / -              Belum dihitung            [Belum Dihitung]                  …               ⋮
              Item berjalan Rp80.000
3   Pasien C  Rp100.000            Rp0 / -              Rp100.000                 [Perlu Hitung Ulang]              …               ⋮
              Item berjalan Rp150.000                   Perkiraan, belum terkini  2 pelayanan belum masuk invoice
```

| Keadaan | Perilaku |
| --- | --- |
| Memuat / gagal | Perilaku `DataTable`/`InformationAlert` yang ada; angka tidak diganti Rp0; banner tertunda disembunyikan |
| Belum dihitung, penjamin | Total dan Sisa "Belum dihitung"; item berjalan sebagai keterangan |
| Belum dihitung, tunai | Sisa = perkiraan dari item (fallback lama) + "Perkiraan, belum terkini" |
| Perlu hitung ulang | Angka hitungan + item berjalan + "Perkiraan, belum terkini" |
| Penjamin penuh sudah dihitung | Total positif, Sisa Pasien Rp0 sah |
| Pelayanan tertunda | Jumlah per baris + banner halaman dengan tautan ke `/health-services/billing-management/billing/charge-reconciliations` |
| Muat Ulang | Membaca ulang daftar; tidak membuat item atau hitungan baru |

## 3. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/billing-management/billing-invoices/cashier-overview/cashier-overview-view.jsx` | Kolom baru "Total Tagihan" dan "Status Tagihan"; "Biaya & Tgl Bayar" → "Sudah Dibayar & Tgl"; "Sisa Pembayaran" → "Sisa Pasien" dengan penanganan angka sementara; banner pelayanan tertunda; tombol "Muat Ulang" |
| `.../cashier-overview/cashier-overview-view.module.css` | `.amountNote`, `.pendingChargeNote`, `.pendingChargeLink` |
| `src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-calculation-breakdown.js` | `resolveCashierBillingState(item)` — membaca keadaan dari backend, tidak menghitung coverage |
| `src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-constants.js` | `CASHIER_CALCULATION_STATE_CONFIG`, `CASHIER_PENDING_CHARGE_ROUTE` |
| `tests/unit/cashier-billing-overview.test.mjs` | Uji susunan kolom diperbarui mengikuti desain K-07 (10 kolom; "Biaya & Tgl Bayar" dinyatakan tidak ada lagi); 6 uji baru FIX-10 |

## 4. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/cashier-billing-overview.test.mjs` | **21/21 lulus** |
| `npx eslint <berkas yang diubah>` | exit 0, 0 error, 0 warning |
| `npx eslint src --quiet` | exit 0 (0 error) |
| `npm run build` | **Lulus** (exit 0) |
| Pemeriksaan visual daftar pada keadaan bagian 2 dengan backend `BE-BKC-FIX-020` berjalan | `NOT RUN` |

## 5. Acceptance criteria (`FIX-BKC-001-10`)

| AC | Bukti |
| --- | --- |
| 1. Rp200.000 belum bayar → Total Rp200.000, Sudah Dibayar Rp0, Sisa sesuai coverage | Kolom terpisah; uji `FIX-10 AC1` |
| 2. Penjamin penuh: total positif terbedakan dari sisa Rp0 | Uji `FIX-10 AC2` |
| 3. Memuat/gagal/belum dihitung tidak menjadi Rp0; detail invoice yang sama tetap dapat dibuka | Uji `FIX-10 AC3`; aksi Detail Pembayaran tidak berubah |
