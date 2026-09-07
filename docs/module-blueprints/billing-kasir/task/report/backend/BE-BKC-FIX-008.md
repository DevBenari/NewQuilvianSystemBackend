# Laporan Perubahan Backend — `BE-BKC-FIX-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-008` (ad-hoc, di luar roadmap, permintaan langsung pengguna) |
| Judul | Endpoint baru `GET .../invoices/payment-history` untuk halaman "Riwayat Pembayaran" lintas invoice/pasien |
| Slice | Independen — permintaan fitur baru, bukan lanjutan investigasi coverage/PPN sebelumnya |
| Roadmap | `NOT APPLICABLE` |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — endpoint baru murni read-only, tidak mengubah kontrak yang sudah ada |
| Backend Governance Preflight | Area `HealthServices`, Module `BillingManagement`, Submodule `Billing` — sudah terdaftar. Keberlakuan: `TOUCHED LEGACY` |
| Dependency | `NONE` |
| Klasifikasi | `MEDIUM` — query baru lintas beberapa tabel (invoice, encounter, guarantor, settlement, tender, calculation version), murni baca, tidak mengubah data/bisnis apa pun |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `BillingPaymentHistoryDtos.cs` (baru), `BillingInvoiceService.cs`, `BillingInvoicesController.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 7 September 2026 |
| Status | Source selesai. Build/test **TIDAK dijalankan** (instruksi eksplisit pengguna). **Belum diverifikasi hidup** — menunggu rebuild |

---

## 1. Masalah

Pengguna meminta halaman "Riwayat Pembayaran" yang menampilkan SEMUA pembayaran yang sudah
dilakukan (lintas invoice/pasien, bukan hanya "Running Invoice" yang ada saat ini), dengan nomor
Kwitansi tiap pembayaran dan keterangan cicilan ke berapa atau lunas. `InvoiceSummaryResponse`
(dipakai `GET .../invoices` untuk Running Invoice) tidak membawa info penjamin, pembayaran, atau
Kwitansi sama sekali — endpoint baru dibutuhkan.

**Tiga keputusan scope dikonfirmasi eksplisit ke pengguna sebelum implementasi** (lewat
`AskUserQuestion`): (1) satu baris tabel = satu INVOICE, bukan satu baris per pembayaran/tender;
(2) aksi "Lihat Kwitansi" menampilkan DAFTAR semua Kwitansi invoice itu (bukan hanya yang terakhir);
(3) halaman ini menampilkan SEMUA invoice yang sudah punya minimal satu pembayaran (termasuk yang
masih cicilan berjalan, bukan hanya yang sudah lunas total).

---

## 2. Perubahan yang dikerjakan

### 2.1 `Dtos/BillingPaymentHistoryDtos.cs` (baru)

`PaymentHistoryQuery` (request: search, serviceType, visitDateFrom/To, pageNumber/pageSize),
`PaymentHistoryItemResponse` (satu invoice: identitas pasien, tipe pasien, tipe layanan, nama
penjamin, `ClaimMethod`, total tagihan (`PatientAmount` kalkulasi terakhir), total dibayar, status
lunas, daftar `Tenders`), `PaymentHistoryTenderResponse` (satu Kwitansi: id, settlementId,
kwitansiNumber, amount, attemptedAt).

### 2.2 `Services/BillingInvoiceService.cs` — `GetPaymentHistoryAsync`

Dua-pass (pola sama dengan `GetActiveEncounterOptionsAsync` yang sudah ada di file yang sama):

1. **Pass 1**: filter invoice yang punya ≥1 tender `SUCCEEDED` pada settlement bertujuan
   `InvoicePayment` (bukan `DepositTopUp`), join encounter+patient untuk pencarian/tampilan,
   filter search/serviceType/rentang tanggal kunjungan, paginasi.
2. **Pass 2**: batch lookup per ID halaman itu (bukan correlated subquery per baris) — penjamin
   aktif (`TrxPatientEncounterGuarantor`, nama+tipe+`InsuranceProviderId`), `ClaimMethod` dari
   `MstInsuranceProvider` untuk provider yang dirujuk, `PatientAmount` kalkulasi TERAKHIR per
   invoice (dikelompokkan di memori, bukan `GroupBy+OrderBy+FirstOrDefault` dalam query, supaya
   tidak bergantung pada dukungan translasi SQL provider untuk pola itu), dan seluruh tender
   `SUCCEEDED` lintas SEMUA settlement `InvoicePayment` invoice itu (satu invoice bisa punya lebih
   dari satu settlement — cicilan kedua dibuka di wadah baru setelah wadah pertama `SETTLED`).

`MapPaymentTypeLabel` (helper privat yang sudah ada di file yang sama, dipakai
`GetActiveEncounterOptionsAsync`) dipakai ulang untuk label Tunai/Asuransi/Penjamin Perusahaan —
tidak ada logika mapping baru yang diduplikasi.

### 2.3 `Controllers/BillingInvoicesController.cs`

`GET .../invoices/payment-history` (`SortOrder = 16`, melanjutkan nomor tertinggi yang sudah
dipakai controller ini). Hak akses `BillingInvoice:Read` dipakai ulang — murni view baca lain atas
data invoice yang sama, tidak ada kewenangan baru (pola sama dengan `BE-BKC-023`/`BKC-DEC-092`).

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Baca ulang seluruh nama field/DbSet yang dipakai (`TrxPatientEncounterGuarantor.PaymentSourceNameSnapshot`/`InsuranceProviderId`, `MstInsuranceProvider.ClaimMethod`, `BilCalculationVersion.PatientAmount`, `BilTender.KwitansiNumber`/`AttemptedAt`, `BilSettlement.Purpose`/`Tenders`) langsung dari model, bukan ditebak | Seluruh nama field dikonfirmasi ada persis seperti dipakai | `PASS` |
| Analisis: rute `payment-history` vs `{id:guid}` | `payment-history` bukan GUID valid, ASP.NET Core route constraint `{id:guid}` tidak akan mencocokkannya — tidak ada collision, pola yang sama dengan `other-charge-types`/`encounter-options` yang sudah ada | `PASS` (analisis) |
| Analisis: `SortOrder` tidak collision | Nilai tertinggi sebelumnya di controller ini adalah 15 (`insurance-invoice-document`) - dipakai 16 | `PASS` |

**`AUTOMATED TEST: BLOCKED`** — build/test tidak dijalankan (instruksi eksplisit pengguna).
**`MANUAL TEST: BLOCKED`** — belum di-rebuild pengguna.

---

## 4. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Belum diverifikasi hidup | Berdasar penelusuran kode + verifikasi nama field langsung dari model, bukan hasil query nyata pasca-rebuild |
| Performa | Pass 2 melakukan beberapa query batch (bukan N+1 per baris) - volumenya terbatas oleh page size (maks 100 invoice per halaman); belum diukur waktu eksekusi nyata terhadap data produksi |
| Perubahan sampingan | `NONE` |
| Status Git | Baru: `BillingPaymentHistoryDtos.cs`. Modified: `BillingInvoiceService.cs`, `BillingInvoicesController.cs`. Belum staged/commit |
| Langkah berikutnya | Rebuild backend, verifikasi `GET .../invoices/payment-history` mengembalikan daftar invoice yang benar (Lunas/Cicilan, Kwitansi per invoice, filter tanggal/layanan/pencarian) |

