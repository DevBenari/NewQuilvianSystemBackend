# Laporan Perubahan Backend — `BE-BKC-FIX-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-021` |
| Judul | Finalisasi invoice tertahan selama masih ada pelayanan layak tagih yang belum menjadi item (`BIL-FIN-022`) |
| Sumber | [`PLAN-REPAIR-BKC-001`](../../../docs/plan-repair/plan-repair-001-integrasi-checkout-ke-billing-kasir.md) `FIX-BKC-001-11`; temuan `ISS-BKC-001-T1` |
| Keputusan | Kontrak readiness/finalisasi yang ada diperluas; tidak ada kebijakan tarif baru |
| Target tulis | `NewQuilvianSystemBackend` |
| Commit saat dikerjakan | `d9089882` (`MHamzah`) + working tree; belum di-commit |
| Tanggal | 2026-10-12 |
| Status | ✅ **Source selesai**, build 0 error, uji query lulus. Uji finalisasi di database `NOT RUN` |

## 1. Masalah

`AreAllOrdersCompleteAsync` hanya memeriksa item invoice yang **sudah ada**; charge yang belum pernah menjadi item tidak terlihat. Penahan efek hanya mencari `TARIFF_NOT_FOUND`. Efek `Pending`/`Failed`, racikan/sumber yang ditolak (`SOURCE_REJECTED`), atau fakta yang belum diterima folio tidak menahan finalisasi. Jalur "selesaikan tanpa kewajiban pasien" dan "tutup otomatis sesudah lunas" bahkan tidak memeriksa kode penahan.

## 2. Proses bisnis sesudah perbaikan

1. Readiness (`GET /finalizations/invoices/{invoiceId}/preview`) menghitung pelayanan tertunda lewat `BillingPendingChargeQuery` untuk kunjungan invoice: efek `Pending`/`Failed`/`ReconciliationRequired` (selain `TARIFF_NOT_FOUND`, yang sudah punya `BIL-FIN-021`) dan fakta tagih `Pending`/`OutcomeUnknown` yang belum diselesaikan.
2. Bila ada, `blockingCodes` memuat **`BIL-FIN-022`** dengan alasan yang terbaca; `isReadyForNormalFinalization = false` sehingga finalisasi otomatis sesudah pembayaran juga tidak berjalan.
3. Finalisasi manual normal ditolak. **Departure exception** (meninggal, transfer darurat, APS) tidak ditahan (`BKC-DEC-033`); tagihan yang menyusul menjadi adjustment.
4. "Selesaikan tanpa kewajiban pasien" dan "tutup otomatis sesudah lunas" kini memakai seluruh kode penahan yang sama (termasuk `BIL-FIN-020`/`021`).
5. Penahan lepas sendiri begitu efek berhasil diteruskan (`Synced`), diselesaikan di antrean rekonsiliasi (`Resolved`), atau memang tidak ditagih (`NotApplicable`). Tahap yang belum eligible — misalnya resep rawat inap yang belum diserahkan — tidak pernah menahan.

> **Contoh.** Tindakan Rp75.000 gagal diteruskan karena gangguan sementara (`Failed`, dicoba ulang). Kasir menekan Finalisasi → ditolak "Masih ada 1 pelayanan yang sudah layak ditagih tetapi belum masuk invoice…". Setelah pekerja sinkron berhasil, item masuk, hitung ulang, lalu finalisasi lolos tanpa charge kedua.

## 3. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `BillingManagement/Billing/Services/BillingFinalizationService.cs` | `BuildReadinessAsync` + kode `BIL-FIN-022`; pemeriksaan pada `FinalizeAsync` (kecuali departure exception), `CompleteInvoiceWithoutPatientPaymentAsync`, `TryCompleteSettledInvoiceAsync`; konstanta `BillingFinalizationBlockingCodes.PendingClinicalCharge` |
| `BillingManagement/Billing/Services/BillingPendingChargeQuery.cs` | Query bersama (dibuat di `BE-BKC-FIX-020`) dengan opsi `excludeTariffNotFound` |
| `BillingManagement/Billing/Dtos/BillingFinalizationDtos.cs` | Dokumentasi `BlockingCodes` |

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nilai baru `BIL-FIN-022` pada `blockingCodes` readiness dan penolakan finalisasi; bentuk respons tetap |
| Database | Tidak ada migration |
| Keamanan | Tidak berubah |

### Health Services / Billing Management / Billing / Finalizations

Base URL: `api/v1/health-services/billing-management/billing/finalizations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/invoices/{invoiceId}/preview` | Readiness, kini dengan `BIL-FIN-022` | `BillingFinalization : Read` |
| `POST` | `/invoices/{invoiceId}` | Finalisasi; ditolak bila `BIL-FIN-022` (kecuali departure exception) | `BillingFinalization : Create` |

## 4. Verifikasi

| Perintah | Hasil |
| --- | --- |
| `dotnet build` | **0 error**, warning baru 0 |
| Uji `Hanya_pelayanan_eligible_yang_belum_masuk_invoice_yang_dihitung_tertunda` | **Lulus** |
| Uji delay/gagal kirim, retry, tarif hilang, gratis, clearance di PostgreSQL | `NOT RUN` |

## 5. Acceptance criteria (`FIX-BKC-001-11`)

| AC | Bukti |
| --- | --- |
| 1. Charge eligible Pending/Failed yang belum jadi item tidak dapat dinyatakan lengkap | `BIL-FIN-022` pada readiness dan finalisasi normal |
| 2. Sinkron/rekonsiliasi sah melepas penahan tanpa charge kedua | Query hanya menghitung status tertunda; `Synced`/`Resolved` keluar dari hitungan |
| 3. FoC/nonbillable dan tahap ranap belum eligible tidak mengunci | Status `NotApplicable` tidak dihitung |
| 4. Finalisasi otomatis dan manual memakai pemeriksaan yang sama | Tiga jalur memakai `BuildReadinessAsync` + kode penahan |

## 6. Risiko dan catatan

- Efek lama yang tertinggal `Pending`/`Failed` (sebelum pekerja sinkron/kebijakan aktif) kini **menahan finalisasi** invoice kunjungannya sampai diselesaikan di antrean rekonsiliasi (`/billing/charge-reconciliations`). Ini disengaja; kasir perlu diberi tahu saat rilis.
- Fakta yang ditolak kontrak (`Rejected`) tidak dihitung — ia tidak pernah layak tagih.
