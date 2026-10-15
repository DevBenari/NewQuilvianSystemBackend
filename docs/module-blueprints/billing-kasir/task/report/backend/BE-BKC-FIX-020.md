# Laporan Perubahan Backend — `BE-BKC-FIX-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-020` |
| Judul | Daftar Invoice & Billing Kasir menandai kekinian angka dan pelayanan yang belum masuk invoice |
| Sumber | [`PLAN-REPAIR-BKC-001`](../../../docs/plan-repair/plan-repair-001-integrasi-checkout-ke-billing-kasir.md) `FIX-BKC-001-09`; temuan `ISS-BKC-001-07` |
| Keputusan | `BKC-DEC-141` (K-07) |
| Konsumen | `FE-BKC-FIX-013` |
| Target tulis | `NewQuilvianSystemBackend` |
| Commit saat dikerjakan | `d9089882` (`MHamzah`) + working tree; belum di-commit |
| Tanggal | 2026-10-12 |
| Status | ✅ **Source selesai**, build 0 error. Uji query terhadap data nyata `NOT RUN` |

## 1. Masalah

`GetCashierOverviewAsync` memakai hitungan terakhir menurut nomor versi tanpa melihat apakah item berubah sesudahnya. Upsert charge menambah item tanpa menghitung ulang, sehingga daftar dapat menampilkan kewajiban lama seolah terkini. Tanpa hitungan, pasien penjamin diberi sisa Rp0 — tampak "ditanggung penuh" padahal belum dihitung. Tidak ada penanda pelayanan yang belum masuk invoice.

## 2. Proses bisnis sesudah perbaikan

Daftar tetap hanya membaca; **tidak ada hitungan baru yang ditulis** karena daftar dibuka. Per baris backend menambahkan:

| Field | Arti |
| --- | --- |
| `calculationState` | `CURRENT` (hitungan terakhir mencakup semua item dan diskon), `STALE` (ada item/diskon berubah sesudah `calculatedAt`), `NOT_CALCULATED` |
| `calculatedAt` | Waktu hitungan terakhir |
| `runningGrossAmount` | Σ Qty × harga item aktif saat ini (sebelum admin, kamar, pajak, diskon, penjamin) |
| `isPatientAmountProvisional` | `true` bila `totalBillAmount`/`outstandingAmount` bukan hasil hitungan terkini |
| `isCashPayer` | Kunjungan tunai (tanpa penjamin) |
| `pendingChargeCount` | Pelayanan layak tagih yang belum menjadi item (efek `Pending`/`Failed`/`ReconciliationRequired` + fakta belum diterima folio) |

Aturan kekinian sama dengan penjaga finalisasi (`IsCalculationCurrentAsync`), sehingga daftar dan finalisasi tidak saling membantah. Field lama tidak berubah makna (kompatibel ke belakang).

> **Contoh.** Hitungan Rp100.000 pukul 09.00, tindakan Rp50.000 masuk pukul 10.00 → `calculationState = STALE`, `totalInvoiceAmount` tetap Rp100.000 (angka hitungan), `runningGrossAmount` Rp150.000, `isPatientAmountProvisional = true`. Pasien asuransi yang belum dihitung → `NOT_CALCULATED`, `outstandingAmount` 0 tetapi `isPatientAmountProvisional = true` dan `isCashPayer = false` — layar menulis "Belum dihitung".

**Batas.** Biaya kamar rawat inap yang terus berjalan tidak menandai `STALE`; ia bertambah per waktu, bukan per item.

## 3. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `BillingManagement/Billing/Services/BillingInvoiceService.cs` | `GetCashierOverviewAsync`: `CalculatedAt` ikut dibaca; batch `MAX(UpdateDateTime ?? CreateDateTime)` item dan diskon per invoice; `BillingPendingChargeQuery` per kunjungan; enam field baru diisi |
| `BillingManagement/Billing/Services/BillingPendingChargeQuery.cs` | **Baru**. Hitungan pelayanan tertunda per kunjungan (dipakai juga `BE-BKC-FIX-021`) |
| `BillingManagement/Billing/Dtos/CashierBillingInvoiceDtos.cs` | Field baru + `CashierCalculationStates` |

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET /invoices/cashier-overview`: enam field tambahan; tidak ada field yang dihapus/diubah |
| Database | Tidak ada migration. Tiga query batch tambahan per halaman (item, diskon, pelayanan tertunda), tanpa N+1 |
| Keamanan | `BillingInvoice : Read` tetap |

### Health Services / Billing Management / Billing / Invoices

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/cashier-overview` | Daftar kasir + kekinian angka + pelayanan tertunda | `BillingInvoice : Read` |

## 4. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `dotnet build` | **0 error**, warning baru 0 |
| Uji `Hanya_pelayanan_eligible_yang_belum_masuk_invoice_yang_dihitung_tertunda` | **Lulus** (5 tertunda; 4 bila `TARIFF_NOT_FOUND` dikecualikan; `NotApplicable`/`Synced`/`Resolved` dan fakta terselesaikan tidak dihitung) |
| Pembandingan daftar vs detail pada data nyata; `EXPLAIN` query | `NOT RUN` |

## 5. Acceptance criteria (`FIX-BKC-001-09`)

| AC | Bukti |
| --- | --- |
| 1. Rp100.000 lalu charge Rp50.000 → bukan Rp100.000 seolah terkini | `STALE` + `runningGrossAmount`; layar menandai "Perlu Hitung Ulang" (`FE-BKC-FIX-013`) |
| 2. Penjamin tanpa kalkulasi tidak diberi sisa nol final | `isPatientAmountProvisional` + `isCashPayer = false` → "Belum dihitung"; penjamin 100% yang sudah dihitung tetap `CURRENT` dan boleh Rp0 |
| 3. Setelah recalculate daftar dan detail konsisten; pagination tidak menulis | Aturan kekinian sama dengan finalisasi; method hanya membaca (`AsNoTracking`) |
