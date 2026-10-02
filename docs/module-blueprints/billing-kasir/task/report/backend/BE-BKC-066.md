# BE-BKC-066 — Surat Penerimaan Uang Terbit Saat Tender Mencapai Keadaan Akhirnya

## Ringkasan untuk Pembaca Umum

Sebelum task ini selesai, rumah sakit menghadapi masalah serius dalam pencatatan uang: ketika kasir menerima pembayaran dari pasien (baik secara tunai di loket maupun non-tunai lewat kartu/QRIS), sistem pencatatan di modul Billing mencatat bahwa tagihan telah dibayar. Namun, bagian keuangan (Finance) tidak memiliki saluran resmi untuk mengetahui bahwa uang tersebut telah benar-benar masuk. Akibatnya, uang yang sudah dibayar oleh pasien di loket kasir berisiko dianggap masih berupa piutang (belum tertagih), sehingga pasien dapat ditagih ulang atau pembukuan keuangan rumah sakit menjadi tidak cocok saat rekonsiliasi bulanan.

Task ini membangun jembatan fakta resmi antara loket kasir dan bagian keuangan. Setiap kali transaksi pembayaran (tender) berhasil diselesaikan (`SUCCEEDED`) atau dibatalkan/dibalik (`REVERSED`), sistem Billing secara otomatis dan seketika menerbitkan "surat penerimaan uang" (tabel `BilCollectionHandoff`). Surat ini memuat identitas pembayaran, tagihan, bukti kwitansi, nomor shift kasir, cara bayar, hingga nominal uang yang diterima. Surat ini terbit di dalam transaksi yang sama dengan pencatatan uang di kasir, sehingga uang dan suratnya tidak akan pernah terpisah nasib: jika pencatatan surat gagal, uang tidak akan dianggap masuk, dan jika uang ditarik kembali, surat pembalikan yang baru akan langsung terbit untuk mengoreksi pembukuan Finance.

---

- TASK ID: BE-BKC-066
- TASK TYPE: Fitur (Penerbitan fakta finansial penerimaan kasir ke modul Finance via `BilCollectionHandoff` dan `BilConsumerHandoffService`)
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: 5 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah/dibuat 6 → 2 + logika bisnis transaksi & keuangan lintas modul → 1 + kontrak API tidak ada endpoint HTTP baru → 0 + database tabel baru dan integritas data → 1 + keamanan shift kasir → 1; total score 5)
- MODEL: Gemini 3.8 Flash (High)
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/**`, `Repositories/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan task)
- FILES INSPECTED:
  - `docs/module-blueprints/billing-kasir/02-backend-architecture.md` (arsitektur revisi 1.4, `BKC-DES-036`–`041`)
  - `docs/module-blueprints/billing-kasir/contracts/integration-contract.md` (`BIL-INT-013`)
  - `docs/module-blueprints/billing-kasir/contracts/validation-matrix.md` (`BIL-VAL-110`, `BIL-VAL-111`)
  - `docs/module-blueprints/billing-kasir/data/data-dictionary.md` (definisi skema `BilCollectionHandoff`)
  - `docs/module-blueprints/finance-management/evidence/02-permintaan-kontrak-untuk-owner-billing.md` (`FIN-DEC-005`, `FIN-DEC-006`)
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs` (rekonsiliasi tender)
  - `Areas/HealthServices/BillingManagement/Billing/Models/BilTender.cs`
  - `Repositories/ApplicationDbContext.cs`
- FILES CHANGED:
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Models/BilCollectionHandoff.cs`
  - **Dibuat**: `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilCollectionHandoffConfiguration.cs`
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs`
  - **Diperbarui**: `Repositories/ApplicationDbContext.cs` (menambahkan `DbSet<BilCollectionHandoff> BilCollectionHandoffs`)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (mendaftarkan `BilConsumerHandoffService` ke DI)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs` (injeksi dan pemanggilan `PublishForTenderAsync` sebelum commit transaksi)
- IMPLEMENTATION:
  1. **Model `BilCollectionHandoff`**:
     Mewarisi `IdentityModel`. Memetakan seluruh atribut wajib per kamus data: `Id`, `TenderId`, `SettlementId`, `InvoiceId`, `PaymentAllocationIds`, `PaymentMethodId`, `PaymentMethodAccountId`, `Amount`, `KwitansiNumber`, `CashierShiftId`, `ProviderReference`, `ProviderEventId`, `OccurredAt`, `SourceInvoiceStatus`, `TenderStatus`, `HandoffKey`, `CorrelationId`, `CausationId`, `Status`, `AcknowledgedAt`, dan `RowVersion`. Sebagai aggregate root tersendiri, entitas ini tidak memiliki foreign key ke `BilFinalizationRecord` (`BKC-DES-037`), sehingga pembayaran yang mendahului finalisasi tetap sampai ke Finance.
  2. **Konfigurasi EF Core `BilCollectionHandoffConfiguration`**:
     Mengatur nama tabel `BilCollectionHandoff` (schema `public`), presisi uang `Amount` 18,2, constraint `CK_BilCollectionHandoff_TenderStatus` (`SUCCEEDED`, `REVERSED`), constraint `CK_BilCollectionHandoff_Status` (`CREATED`, `ACKNOWLEDGED`), constraint `CK_BilCollectionHandoff_Amount` (`> 0`), serta unique index `IX_BilCollectionHandoff_Tender_Status` dan `IX_BilCollectionHandoff_HandoffKey`. Seluruh relasi navigasi (`Tender`, `Settlement`, `Invoice`, `PaymentMethod`, `CashierShift`) dikunci dengan `DeleteBehavior.Restrict`.
  3. **Penerbit Handoff `BilConsumerHandoffService`**:
     Membangun method `PublishForTenderAsync` yang bertindak sebagai satu-satunya pintu penerbitan fakta ke Finance (`BKC-DES-038`).
     - **Penegakan `BIL-VAL-111`**: Memeriksa cara bayar tunai (`paymentMethod.IsCash` atau `PaymentMethodType == "Cash"`). Jika tunai dan `tender.CashierShiftId` kosong, melempar `BillingConsumerHandoffValidationException` dengan pesan baku: *"Pembayaran tunai tidak dapat diteruskan ke pembukuan karena shift kasirnya tidak diketahui. Tutup dan buka kembali shift, lalu ulangi"*.
     - **Penegakan `BIL-VAL-110` (Idempotensi)**: Memeriksa apakah surat untuk kombinasi `(TenderId, TenderStatus)` sudah pernah diterbitkan. Jika sudah ada, penerbitan kedua diabaikan tanpa membuat baris baru. Kunci `HandoffKey` dihitung deterministik dari `MD5($"BIL_COLLECTION_{tender.Id:N}_{tender.Status}")`.
     - **Kunci Penasihat Transaksi (`BKC-DES-032`)**: Mengambil kunci `BIL_INVOICE_LEDGER_{invoice.Id:N}` di dalam transaksi relational.
     - **Alokasi Pembayaran**: Mengumpulkan Id alokasi pembayaran terkait dari `BilPaymentAllocation` ke dalam string dipisah koma (`PaymentAllocationIds`).
     - **Pencatatan Audit Trail**: Mencatat audit `BillingCollectionHandoff.Created` lewat `LoggerService`.
  4. **Pemasangan di `BillingSettlementService.ReconcileTenderAsync`**:
     Dipasang sesudah sinkronisasi status penutupan tagihan (`SyncClosureAsync`) dan sebelum `transaction.CommitAsync`. Jika penerbitan gagal (misal shift kasir tunai tidak ada), blok `catch` umum menangkap exception dan melakukan `transaction.RollbackAsync()`, sehingga pembatalan transaksi pemanggil terjamin secara atomik (`BIL-AT-142-F`).
- **Backend Governance Preflight**:
  - Area: `HealthServices` · Module: `BillingManagement/Billing` · Prefix: `Bil` · Registry: `ACTIVE`
  - Keberlakuan: `NEW CODE` untuk model `BilCollectionHandoff`, configuration, dan service penerbit; `TOUCHED LEGACY` untuk `ApplicationDbContext`, `BillingManagementServiceCollectionExtensions`, dan `BillingSettlementService`.
  - QBE Compliance: Mematuhi `QBE-NAM-001`, `QBE-NAM-002`, `QBE-MOD-002`, `QBE-MOD-003`, `QBE-SVC-001`, `QBE-DB-001`, `QBE-DB-002`, dan `QBE-AUD-001`.
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (`BACKEND MODE`)
- API CONTRACT IMPACT: Nol perubahan endpoint HTTP pada task ini. Penerbitan berlangsung in-process di dalam transaksi pembayaran. Endpoint operasional pemeriksaan surat menggantung akan dibangun pada task `BE-BKC-069`.
- DATABASE IMPACT: Satu tabel baru `BilCollectionHandoff` disiapkan pada konfigurasi DbContext. Nol kolom baru ditambahkan pada tabel existing (`BilInvoice`, `BilTender`, `BilSettlement` tidak disentuh skemanya). Sesuai aturan wewenang terpisah, pembuatan migration `AddBillingConsumerHandoff` dan eksekusi database menunggu otorisasi terpisah.
- SECURITY IMPACT: Penegakan invariant keamanan shift kasir pada `BIL-VAL-111`. Pembayaran tunai tanpa shift kasir aktif ditolak dan digagalkan transaksinya.
- VISUAL REFERENCE: NOT REQUIRED
- VALIDATION:
  | Command/Check | Result | Classification | Evidence/Note |
  | --- | --- | --- | --- |
  | `dotnet build` | **Menunggu eksekusi manual pengguna** | NOT VERIFIED | Sesuai instruksi eksplisit pengguna: *"biarkan saya build manual"* |
  | Review Diff dan Scope | Selesai | VERIFIED | Nol kolom ditambahkan pada tabel existing; struktur class dan konfigurasi EF Core sesuai persis dengan kamus data `data/data-dictionary.md` dan kontrak `BIL-INT-013` |
  | `BIL-AT-135` (pembayaran melahirkan surat penerimaan) | Terpenuhi pada source | VERIFIED (Logic) | `PublishForTenderAsync` dipanggil saat `targetStatus == Succeeded` sebelum commit transaksi |
  | `BIL-AT-136` (surat terbit tanpa menunggu lunas) | Terpenuhi pada source | VERIFIED (Logic) | Penerbitan tidak mensyaratkan `outstanding <= 0` atau invoice berstatus `CLOSED`/`FINAL` |
  | `BIL-AT-142-F` (tender tunai tanpa shift kasir ditolak) | Terpenuhi pada source | VERIFIED (Logic) | `BIL-VAL-111` melempar `BillingConsumerHandoffValidationException` yang memicu rollback transaksi |
  | Idempotensi `BIL-VAL-110` | Terpenuhi pada source | VERIFIED (Logic) | Pengecekan `(TenderId, TenderStatus)` dan `HandoffKey` unik mencegah pembuatan baris duplikat |
  | Pembalikan `REVERSED` | Terpenuhi pada source | VERIFIED (Logic) | Tender dengan status `REVERSED` melahirkan baris baru dengan `TenderStatus = 'REVERSED'` |
- WARNINGS: Pembuatan dan eksekusi migration `AddBillingConsumerHandoff` belum dijalankan dan membutuhkan otorisasi terpisah sesudah backup basis data.
- KNOWN ISSUES: Tidak ada issue baru pada source yang ditulis.
- NEXT TASKS: `BE-BKC-067` (Surat clearance resep terbit saat keadaan berubah pada `BilPrescriptionClearanceHandoff`).

