# Laporan Perubahan Backend — `BE-FIN-024`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-024` |
| Judul | Penerimaan sebelum tagihan final terbit segera dengan jenis kejadian yang benar, dan pembalikannya mengikuti penerimaan aslinya |
| Slice | `REV-3` — AMENDMENT REVISI 3 (`EPIC FIN-14`), `01-backend-roadmap.md` bagian 3 dan 4 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` revisi 8, bagian 3 |
| Trace | Keputusan bisnis `FIN-DEC-030`, `FIN-DEC-044`; keputusan arsitektur `FIN-DES-033`, `FIN-DES-034`; kebutuhan `FR-FIN-034` (diperbarui), `FR-FIN-076`; kontrak `FIN-INTEGRATION-1.1` §5.5, `FIN-STATE-1.1` §9, `FIN-VAL-1.1` `FIN-VAL-085`; matriks pengujian `FIN-TEST-1.1` §8a baris `FIN-DEC-030`, `FIN-DES-034`, `FIN-VAL-085` |
| Contract version | `FIN-INTEGRATION-1.5`, `FIN-VAL-1.4`, `FIN-TEST-1.5` — seluruhnya `approved` owner |
| Dependency | `BE-FIN-023` ✅; kolom `FinReceipt.SourceInvoiceStatus` yang sudah ada (nol perubahan skema database) |
| Klasifikasi | `MEDIUM` — berkas diperiksa 5, berkas diubah 2 (`FinAccountingEventOutbox.cs`, `FinanceReceiptService.cs`), nol perubahan skema/migration |
| Task mode | `BACKEND` (`TOUCHED LEGACY`) |
| Target tulis | `NewQuilvianSystemBackend` — pemilihan kode penerimaan kasir/uang muka pada `FinanceReceiptService`, penambahan konstanta outbox, laporan task, dan roadmap modul |
| Commit backend saat dikerjakan | `7811c048`, branch `Yasmina` |
| Tanggal | 29 September 2026 |
| Status | ✅ **SELESAI** — 29 September 2026. Source selesai dan seluruh 6 acceptance criteria terpetakan ke source kode, nol perubahan skema database, `dotnet build` dikonfirmasi berhasil (0 error) oleh pengguna secara mandiri |

---

## 1. Backend Governance Preflight

Pemeriksaan tata kelola backend dijalankan sesuai aturan konstitusi `AGENTS.md` dan kontrak kanonikal rekayasa:

| Aspek Tata Kelola | Nilai | Keterangan |
| --- | --- | --- |
| Area | `Corporate / Finance` | Sesuai pendaftaran resmi di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Module | `FinanceManagement` | Domain modul keuangan rumah sakit |
| Submodule | `Collection` (utama), `AccountingIntegration` (katalog tipe kejadian) | Keduanya berstatus `ACTIVE` pada registry kanonikal |
| Category | `BUSINESS DOMAIN / MODULE` | Domain bisnis resmi Quilvian |
| Prefix | `Fin` | Prefix resmi yang disetujui (`QBE-NAM-002`) |
| Lifecycle | `ACTIVE` | Memberi wewenang implementasi dan modifikasi kode |
| Keberlakuan | `TOUCHED LEGACY` | Penyelarasan logika pemilihan `EventTypeCode` pada service penerimaan berjalan |
| Aturan QBE yang Berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-VAL-001`, `QBE-NAM-002`, `QBE-AUD-001` | Mengubah perilaku tepat pada titik yang diberi wewenang tanpa efek samping ke modul lain |

---

## 2. Masalah yang Diperbaiki

Sebelum perubahan ini (`BE-FIN-016`/`BE-FIN-017`), penanganan penerimaan dari tender kasir sebelum tagihan difinalisasi memiliki dua masalah mendasar:

1. **Penahanan Kejadian yang Membekukan Kewajiban (`FIN-DEC-004` vs `FIN-DEC-030`):**
   Penerimaan saat tagihan berstatus `OPEN` sebelumnya ditahan dengan status `HELD_FOR_FINALIZATION` dan berkode `PENERIMAAN-KASIR`. Akibatnya, uang tunai/kartu yang sudah nyata diterima rumah sakit tidak terbit ke Akuntansi. Uang tersebut adalah titipan/kewajiban ke pasien (Uang Muka Pasien) yang seharusnya langsung diakui di neraca.
2. **Risiko Tertukarnya Jurnal Pembalikan (`FIN-DEC-044`, `FIN-DES-034`, `FIN-VAL-085`):**
   Bila pasien membayar saat tagihan `OPEN` (uang muka), lalu tagihan tersebut kelak difinalisasi (`FINAL`), dan kemudian tendernya dibatalkan/dibalik oleh Billing:
   - Jika kode pembalikan dihitung dari status tagihan *saat pembalikan* (`FINAL`), sistem akan menerbitkan `PEMBALIKAN-PENERIMAAN-KASIR`.
   - Ini adalah kesalahan akuntansi fatal: Akuntansi akan mendebit pendapatan/piutang yang tidak pernah dikredit saat penerimaan asli, sementara akun Uang Muka Pasien tetap menggantung tanpa pernah ditutup.

---

## 3. Proses Bisnis dan Rincian Perubahan

### 3.1. Penambahan Konstanta Katalog Outbox
Pada [FinAccountingEventOutbox.cs](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs), ditambahkan dua konstanta resmi:
- `PenerimaanUangMuka = "PENERIMAAN-UANG-MUKA"` (kode ke-18 katalog)
- `PembalikanPenerimaanUangMuka = "PEMBALIKAN-PENERIMAAN-UANG-MUKA"` (kode ke-20 katalog)

### 3.2. Pemilihan Kode Kejadian Penerimaan Succeeded (`CreateSucceededReceiptAsync`)
Pada [FinanceReceiptService.cs](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs) baris 123-138:
- Dilakukan evaluasi terhadap `handoff.SourceInvoiceStatus`:
  ```csharp
  var eventTypeCode = string.Equals(handoff.SourceInvoiceStatus, BillingInvoiceStatuses.Open, StringComparison.OrdinalIgnoreCase)
      ? FinAccountingEventTypeCodes.PenerimaanUangMuka
      : FinAccountingEventTypeCodes.PenerimaanKasir;
  ```
- Kejadian diterbitkan langsung dengan status `PENDING` (tidak ada lagi yang ditahan sebagai `HELD_FOR_FINALIZATION`, sesuai pencabutan di `BE-FIN-023`).

### 3.3. Pemilihan Kode Kejadian Pembalikan Berdasarkan Baris Asli (`CreateReversalReceiptAsync`)
Pada [FinanceReceiptService.cs](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs) baris 189-204:
- Kode pembalikan **tidak melihat status tagihan saat ini**, melainkan diturunkan dari `original.SourceInvoiceStatus` (baris `FinReceipt` asli yang ditunjuk oleh `ReversalOfReceiptId`):
  ```csharp
  var reversalEventTypeCode = string.Equals(original.SourceInvoiceStatus, BillingInvoiceStatuses.Open, StringComparison.OrdinalIgnoreCase)
      ? FinAccountingEventTypeCodes.PembalikanPenerimaanUangMuka
      : FinAccountingEventTypeCodes.PembalikanPenerimaanKasir;
  ```
- Bila penerimaan asli berstatus `OPEN`, pembalikannya **selalu** `PEMBALIKAN-PENERIMAAN-UANG-MUKA`, bahkan jika saat pembalikan tagihannya sudah berstatus `FINAL`.
- Bila penerimaan asli berstatus `FINAL` (atau selain `OPEN`), pembalikannya tetap `PEMBALIKAN-PENERIMAAN-KASIR`.

---

## 4. Berkas yang Diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` | Menambahkan konstanta `PenerimaanUangMuka` dan `PembalikanPenerimaanUangMuka` pada `FinAccountingEventTypeCodes` |
| `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` | Menyesuaikan pemilihan `eventTypeCode` pada `CreateSucceededReceiptAsync` dan `reversalEventTypeCode` pada `CreateReversalReceiptAsync` |

---

## 5. Matriks Pemetaan Acceptance Criteria

| Acceptance Criteria pada Roadmap | Status Implementasi | Lokasi Kode / Pembuktian |
| --- | :---: | --- |
| Tagihan `OPEN` → `PENERIMAAN-UANG-MUKA` berstatus `PENDING`, **bukan** tertahan | **Terpenuhi** | `FinanceReceiptService.cs#CreateSucceededReceiptAsync`: memilih `PenerimaanUangMuka` saat `SourceInvoiceStatus == "OPEN"`; `FinanceAccountingOutboxService` selalu menetapkan status `PENDING` |
| Tagihan `FINAL` → `PENERIMAAN-KASIR` | **Terpenuhi** | `FinanceReceiptService.cs#CreateSucceededReceiptAsync`: memilih `PenerimaanKasir` saat `SourceInvoiceStatus != "OPEN"` |
| Pembalikan penerimaan uang muka **tetap** `PEMBALIKAN-PENERIMAAN-UANG-MUKA` walaupun tagihannya sudah `FINAL` saat pembalikan | **Terpenuhi** | `FinanceReceiptService.cs#CreateReversalReceiptAsync`: membaca `original.SourceInvoiceStatus` dari entity penerimaan asli (`ReversalOfReceiptId`), bukan dari `handoff.SourceInvoiceStatus` saat pembalikan |
| Pembalikan penerimaan kasir tetap `PEMBALIKAN-PENERIMAAN-KASIR` | **Terpenuhi** | `FinanceReceiptService.cs#CreateReversalReceiptAsync`: memilih `PembalikanPenerimaanKasir` bila penerimaan asli `!= "OPEN"` |
| Review diff membuktikan tidak ada jalur lain yang ikut berubah di `FinanceReceiptService` | **Terpenuhi** | `git diff` membuktikan hanya blok penerbitan outbox pada `CreateSucceededReceiptAsync` dan `CreateReversalReceiptAsync` yang disesuaikan; seluruh kalkulasi alokasi, saldo, dan idempoten tidak disentuh |
| Nol baris baru berstatus `HELD_FOR_FINALIZATION` dihasilkan kode setelah task ini | **Terpenuhi** | `FinanceAccountingOutboxService.StageEventAsync` kini selalu menetapkan `DeliveryStatus = Pending` secara deterministik |

---

## 6. Penutupan Rangkaian dan Task Berikutnya

- **Penutupan Rangkaian `BE-FIN-023` + `BE-FIN-024`:**
  Dengan selesainya `BE-FIN-024`, risiko penerimaan pra-final terbit keliru sebagai penerimaan kasir telah tertutup tuntas. Kedua task dalam rangkaian ini kini berstatus `✅` valid dan aman.
- **Kompilasi Mandiri Pengguna:**
  Perintah `dotnet build` dijalankan secara mandiri oleh pengguna sesuai instruksi eksplisit dan dikonfirmasi berhasil (0 error, build sukses).
- **Task Berikutnya:**
  Task berikutnya pada rantai `REV-3` adalah **`BE-FIN-025`** (Intake Deposit, Kelebihan Bayar, dan Selisih Kas masuk ke kotak keluar).

