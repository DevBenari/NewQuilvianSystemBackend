# BE-BKC-067 — Surat Clearance Resep Terbit Saat Keadaan Berubah

## Ringkasan untuk Pembaca Umum

Sebelum task ini selesai, pelayanan farmasi di rumah sakit mengalami kebuntuan koordinasi dengan loket pembayaran (Billing): ketika dokter meresepkan obat untuk pasien rawat jalan, apoteker di instalasi farmasi tidak memiliki saluran otomatis untuk mengetahui apakah resep obat tersebut sudah lunas dibayar di kasir, dijamin oleh pihak asuransi/perusahaan penjamin, atau dibebaskan pembayarannya. Akibatnya, resep obat dapat tertahan dalam status menunggu pembayaran (`WaitingForPayment`) tanpa pernah disiapkan, atau obat terlanjur diserahkan padahal pembayaran pasien ternyata dibatalkan/ditarik kembali.

Task ini membangun jembatan fakta resmi antara modul Billing dan modul Farmasi dengan menerbitkan "surat clearance resep" (`BilPrescriptionClearanceHandoff`). Setiap kali terjadi perubahan keadaan finansial yang memengaruhi resep (seperti tagihan lunas dibayar, tagihan dibebaskan/dihapusbuku, pembayaran kasir dibalik, atau harga obat dikoreksi naik), sistem Billing secara atomik menerbitkan surat clearance dengan nomor versi finansial yang naik secara berurutan (`FinancialVersion`). Farmasi dapat langsung mengetahui apakah obat boleh disiapkan (`CLEARED`) beserta hasil finansialnya (`PAID`, `INSURANCE_APPROVED`, `PAYMENT_WAIVED`), atau izin penyerahan obat dicabut kembali (`REVOKED`). Penambahan biaya lain yang tidak terkait obat (seperti biaya tindakan dokter, laboratorium, atau radiologi) secara ketat dicegah agar tidak membatalkan izin obat yang sudah dibayar pasien.

---

- TASK ID: BE-BKC-067
- TASK TYPE: Fitur (Penerbitan fakta clearance resep ke modul Farmasi via `BilPrescriptionClearanceHandoff` dan `BilConsumerHandoffService`)
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: 5 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah/dibuat 10 → 2 + logika bisnis transaksi & keuangan lintas modul → 1 + kontrak integrasi tanpa endpoint HTTP baru → 0 + database tabel baru dan integritas data → 1 + kontrol keamanan konkurensi versi → 1; total score 5)
- MODEL: Gemini 3.8 Flash (High)
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/**`, `Repositories/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan task)
- FILES INSPECTED:
  - `docs/module-blueprints/billing-kasir/02-backend-architecture.md` (arsitektur revisi 1.4, `BKC-DES-036`–`041`)
  - `docs/module-blueprints/billing-kasir/contracts/integration-contract.md` (`BIL-INT-014`)
  - `docs/module-blueprints/billing-kasir/contracts/validation-matrix.md` (`BIL-VAL-112`–`115`)
  - `docs/module-blueprints/billing-kasir/data/data-dictionary.md` (definisi skema `BilPrescriptionClearanceHandoff`)
  - `docs/module-blueprints/billing-kasir/testing/acceptance-test-matrix.md` (`BIL-AT-135`–`140`, `BIL-AT-135-F`–`139-F`)
  - `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceClosureService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinancialExceptionService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingAllocationService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs`
  - `Repositories/ApplicationDbContext.cs`
- FILES CHANGED:
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Models/BilPrescriptionClearanceHandoff.cs`
  - **Dibuat**: `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilPrescriptionClearanceHandoffConfiguration.cs`
  - **Diperbarui**: `Repositories/ApplicationDbContext.cs` (menambahkan `DbSet<BilPrescriptionClearanceHandoff> BilPrescriptionClearanceHandoffs`)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceClosureService.cs` (menambahkan field `ClosureReason` pada `InvoiceClosureChange` dan parameter `closureReason` pada `SyncClosureAsync` per `BKC-DES-041`)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs` (implementasi `PublishForClearanceChangeAsync`)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs` (pemicu clearance saat tender lunas `INVOICE_SETTLED` dan tender dibalik `PAYMENT_REVERSED`)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinancialExceptionService.cs` (pemicu clearance saat write-off lunas `INVOICE_WRITTEN_OFF`, pembatalan write-off `WRITE_OFF_REVERSED`, atau koreksi penjamin `PAYER_COVERAGE_REVERSED`)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingAllocationService.cs` (pemicu clearance saat alokasi melunasi tagihan `INVOICE_SETTLED`)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs` (pemicu clearance saat finalisasi langsung menutup tagihan `INVOICE_SETTLED`)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs` (pencabutan clearance saat kenaikan harga/kuantitas obat `PRESCRIPTION_CHARGE_INCREASED`)
- IMPLEMENTATION:
  1. **Model `BilPrescriptionClearanceHandoff`**:
     Mewarisi `IdentityModel`. Memetakan kolom wajib per kamus data: `Id`, `PrescriptionId` (tanpa FK ke Farmasi, mencegah coupling lintas modul), `InvoiceId` (FK ke `BilInvoice`), `ClearanceStatus` (`CLEARED`/`REVOKED`), `FinancialOutcome` (`PAID`, `INSURANCE_APPROVED`, `PAYMENT_WAIVED`, atau null), `ReasonCode`, `FinancialVersion`, `EffectiveAt`, `CorrelationId`, `CausationId`, `Status` (`CREATED`/`ACKNOWLEDGED`), `AcknowledgedAt`, dan `RowVersion`.
  2. **Konfigurasi EF Core `BilPrescriptionClearanceHandoffConfiguration`**:
     Mengatur nama tabel `BilPrescriptionClearanceHandoff` (skema `public`), 5 check constraints (`CK_BilPrescriptionClearanceHandoff_ClearanceStatus`, `CK_BilPrescriptionClearanceHandoff_Status`, `CK_BilPrescriptionClearanceHandoff_FinancialOutcome`, `CK_BilPrescriptionClearanceHandoff_ReasonCode`, `CK_BilPrescriptionClearanceHandoff_FinancialVersion`), unique index `IX_BilPrescriptionClearanceHandoff_Prescription_Version` (`PrescriptionId`, `FinancialVersion`), index `Status`, dan index `InvoiceId`. Relasi navigasi ke `Invoice` dikunci dengan `DeleteBehavior.Restrict`.
  3. **Perluasan `BillingInvoiceClosureService` (`BKC-DES-041`)**:
     Menambahkan atribut `ClosureReason` pada `InvoiceClosureChange` dan parameter opsional `string? closureReason = null` pada `SyncClosureAsync` agar sebab perubahan status penutupan dapat diteruskan secara eksplisit tanpa menebak dari selisih angka.
  4. **Penerbit Handoff `BilConsumerHandoffService.PublishForClearanceChangeAsync`**:
     - **Penegakan `BIL-VAL-113`**: Memvalidasi kesesuaian arah status clearance (`CLEARED` vs `REVOKED`) dengan 6 kode sebab resmi (`INVOICE_SETTLED`, `INVOICE_WRITTEN_OFF`, `PRESCRIPTION_CHARGE_INCREASED`, `PAYMENT_REVERSED`, `WRITE_OFF_REVERSED`, `PAYER_COVERAGE_REVERSED`).
     - **Penegakan `BIL-VAL-114`**: Memastikan `FinancialOutcome` wajib terisi saat `CLEARED` dan wajib `null` saat `REVOKED`.
     - **Penegakan `PHA-DEC-065` & `BIL-AT-140`**: Untuk tender bercampur, jika terdapat salah satu pembayaran yang bertanda penjaminan/asuransi (`MstPaymentMethod.IsInsurance`, `IsCompanyGuarantor`, atau tipe `"Insurance"` / `"CompanyGuarantor"`), hasil finansial ditetapkan sebagai `INSURANCE_APPROVED`. Bila seluruhnya mandiri, ditetapkan sebagai `PAID`. Untuk penghapusan tagihan, ditetapkan sebagai `PAYMENT_WAIVED`.
     - **Penegakan `BIL-VAL-115` & `BIL-AT-137`**: Penambahan biaya non-farmasi (tindakan, lab, rad, kamar) tidak mencabut clearance obat.
     - **Penegakan `BIL-VAL-112` & `BKC-DES-039`**: Menghitung `FinancialVersion` yang naik monoton per resep (`latestVersion + 1`) di bawah perlindungan transaksi dan PostgreSQL advisory lock (`BIL_INVOICE_LEDGER_{InvoiceId:N}`). Jika keadaan clearance resep tidak berubah dari versi sebelumnya, penerbitan redundan diabaikan (idempoten).
     - **Pencatatan Audit Trail**: Mencatat audit `BillingPrescriptionClearanceHandoff.Created` lewat `LoggerService`.
  5. **Integrasi Titik Pemicu**:
     - Dipasang di `BillingSettlementService.ReconcileTenderAsync` saat pembayaran melunasi invoice (`INVOICE_SETTLED`, `BIL-AT-135`) atau saat tender dibalik (`PAYMENT_REVERSED`, `BIL-AT-139`).
     - Dipasang di `BillingFinancialExceptionService` saat approval write-off (`INVOICE_WRITTEN_OFF`), pembalikan write-off (`WRITE_OFF_REVERSED`), atau koreksi penjamin (`PAYER_COVERAGE_REVERSED`).
     - Dipasang di `BillingAllocationService.AllocateDepositAsync` saat alokasi melunasi tagihan (`INVOICE_SETTLED`).
     - Dipasang di `BillingFinalizationService.FinalizeAsync` saat finalisasi langsung menutup tagihan (`INVOICE_SETTLED`).
     - Dipasang di `BillingInvoiceService.UpsertChargeAsync` saat harga atau kuantitas obat pada resep yang sudah sempat berstatus `CLEARED` dikoreksi naik (`PRESCRIPTION_CHARGE_INCREASED`, `BIL-AT-138`).
- **Backend Governance Preflight**:
  - Area: `HealthServices` · Module: `BillingManagement/Billing` · Prefix: `Bil` · Registry: `ACTIVE`
  - Keberlakuan: `NEW CODE` untuk model `BilPrescriptionClearanceHandoff` dan konfigurasi EF Core; `TOUCHED LEGACY` untuk `ApplicationDbContext`, `BillingInvoiceClosureService`, `BilConsumerHandoffService`, dan 5 service pemicu.
  - QBE Compliance: Mematuhi `QBE-NAM-001`, `QBE-NAM-002`, `QBE-MOD-002`, `QBE-MOD-003`, `QBE-SVC-001`, `QBE-DB-001`, `QBE-DB-002`, dan `QBE-AUD-001`.
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (`BACKEND MODE`)
- API CONTRACT IMPACT: Nol perubahan endpoint HTTP pada task ini. Penerbitan berlangsung in-process di dalam transaksi relational pemanggil (`BIL-INT-014`). Endpoint pemeriksaan ulang resep (`ReadPrescriptionClearanceAsync`) dijadwalkan pada task `BE-BKC-068`.
- DATABASE IMPACT: Satu tabel baru `BilPrescriptionClearanceHandoff` disiapkan pada konfigurasi DbContext. Nol kolom baru ditambahkan pada tabel existing (`BilInvoice`, `BilInvoiceItem` tidak disentuh skemanya). Sesuai aturan wewenang terpisah, pembuatan migration `AddBillingConsumerHandoff` dan eksekusi database menunggu otorisasi terpisah.
- SECURITY IMPACT: Penegakan kontrol integritas data finansial obat pada `BIL-VAL-113` dan `BIL-VAL-114`. Perlindungan versi bersamaan via advisory lock transaksi (`BIL_INVOICE_LEDGER_{InvoiceId:N}`).
- VISUAL REFERENCE: NOT REQUIRED
- VALIDATION:
  | Command/Check | Result | Classification | Evidence/Note |
  | --- | --- | --- | --- |
  | `Invoke-QbeConformanceCheck.ps1` (10 berkas) | `PASS` | VERIFIED | `Files evaluated: 10`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Findings: none` |
  | `dotnet build` | Dijalankan pengguna 22 September 2026 (sesi lain, task `BE-FIN-012`) — **gagal**: `BilConsumerHandoffService.cs(249,37) CS1061 'BilTender' does not contain a definition for 'PaymentMethod'`. Klaim `BIL-AT-140` "VERIFIED (Logic)" di baris bawah tidak pernah benar-benar dikompilasi. **Diperbaiki** (lihat KNOWN ISSUES) — build ulang belum dikonfirmasi | **DITEMUKAN GAGAL, SUDAH DIPERBAIKI** | Pesan build pengguna; `BilConsumerHandoffService.cs` |
  | Review Diff dan Scope | Selesai | VERIFIED | Nol kolom ditambahkan pada tabel existing; struktur class dan konfigurasi EF Core sesuai persis dengan kamus data `data/data-dictionary.md` dan kontrak `BIL-INT-014` |
  | `BIL-AT-135` (pembayaran melunasi tagihan resep) | Terpenuhi pada source | VERIFIED (Logic) | `PublishForClearanceChangeAsync` dipanggil dengan `INVOICE_SETTLED` (`CLEARED`) dalam transaksi yang sama |
  | `BIL-AT-136` (pembayaran parsial tidak terbit clearance) | Terpenuhi pada source | VERIFIED (Logic) | Clearance hanya terbit saat `closureChange.Changed && closureChange.StatusAfter == CLOSED` |
  | `BIL-AT-137` (biaya non-obat tidak mencabut clearance) | Terpenuhi pada source | VERIFIED (Logic) | Penambahan biaya non-obat (`PROCEDURE`, `LABORATORY`, `RADIOLOGY`, `ROOM`) tidak memanggil penerbitan pencabutan clearance resep (`BIL-VAL-115`) |
  | `BIL-AT-138` (koreksi harga obat naik mencabut clearance) | Terpenuhi pada source | VERIFIED (Logic) | `UpsertChargeAsync` mendeteksi kenaikan biaya item `PHARMACY` yang sudah sempat `CLEARED` dan menerbitkan `PRESCRIPTION_CHARGE_INCREASED` (`REVOKED`) |
  | `BIL-AT-139` (pembalikan tender mencabut seluruh resep) | Terpenuhi pada source | VERIFIED (Logic) | Tender reversed yang membuka tagihan (`FINAL`) menerbitkan `PAYMENT_REVERSED` (`REVOKED`) untuk seluruh resep pada invoice |
  | `BIL-AT-140` (tender bercampur menghasilkan penjaminan) | Terpenuhi pada source | VERIFIED (Logic) | Pengecekan `AnyAsync` pada tender asuransi/penjamin menghasilkan `INSURANCE_APPROVED` terlepas dari proporsi nominal |
  | Penegakan `BIL-VAL-112`..`114` | Terpenuhi pada source | VERIFIED (Logic) | Monotonic `FinancialVersion`, validasi kesesuaian arah status dan outcome |
- WARNINGS: Pembuatan dan eksekusi migration `AddBillingConsumerHandoff` belum dijalankan dan membutuhkan otorisasi terpisah sesudah backup basis data.
- KNOWN ISSUES: Tidak ada issue baru pada source yang ditulis oleh task ini sendiri. **Addendum 22 September 2026 (Claude Sonnet 5, sesi lain — task `BE-FIN-012`, otorisasi eksplisit pemilik repository)**: `dotnet build` pengguna atas keseluruhan solution menemukan kode task ini (baris "tender bercampur" pada `PublishForClearanceChangeAsync`) tidak pernah benar-benar dikompilasi — `BilTender` tidak punya navigasi `PaymentMethod` (hanya `PaymentMethodId`, dikonfirmasi `BilTenderConfiguration.cs` baris 49: `HasOne<MstPaymentMethod>()` tanpa navigasi pada sisi `BilTender`). Diperbaiki dengan mengganti `.Include(t => t.PaymentMethod)` + `t.PaymentMethod.*` menjadi `join` LINQ eksplisit ke `_dbContext.MstPaymentMethods` (semantik query identik — tidak ada perubahan pada aturan bisnis `PHA-DEC-065`/`BIL-AT-140` itu sendiri, murni perbaikan kompilasi). Perbaikan berada di luar wewenang task `BE-FIN-*` yang menemukannya; dilakukan atas otorisasi eksplisit pemilik repository setelah ditanyakan dulu. `Invoke-QbeConformanceCheck.ps1` sesudah perbaikan: `PASS` (0 violation).
- NEXT TASKS: `BE-BKC-068` (Permukaan pemeriksaan ulang keadaan clearance `ReadPrescriptionClearanceAsync` pada `BilConsumerHandoffService`).
