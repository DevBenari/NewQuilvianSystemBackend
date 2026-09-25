# BE-BKC-071 — Skema Database & Master Policy Administrasi Ranap

## Ringkasan untuk Pembaca Umum

Sebelum task ini selesai, sistem Billing rumah sakit menghadapi dua keterbatasan struktural dalam integrasi dengan rawat inap:
1. **Ketergantungan Terbalik Kelayakan Pulang (*Financial Clearance*):** Selama ini modul Billing memeriksa kelayakan pemulangan pasien dengan membaca status rawat inap (`InpFinancialClearance`). Hal ini keliru secara arsitektur keuangan rumah sakit, karena modul Billing adalah satu-satunya pemilik data transaksi pelunasan, sisa tagihan, dan mutasi deposit pasien. Bangsal rawat inap seharusnya menerima izin pemulangan dari kasir/billing, bukan sebaliknya.
2. **Keterbatasan Biaya Administrasi Tetap (*Flat*):** Tabel master kebijakan administrasi (`MstAdministrationFeePolicy`) hanya mendukung nilai nominal tetap (`Amount`). Padahal, aturan bisnis rawat inap yang telah disetujui (`BKC-DEC-113`) menetapkan biaya administrasi rawat inap sebesar 7% dari tagihan eligible dengan pagu (cap) maksimal Rp6.000.000.

Task ini menyelesaikan fondasi skema database untuk kedua kebutuhan tersebut:
- **Tabel Baru `BilInpatientClearanceHandoff`:** Dibuat sebagai saluran resmi penerbitan fakta kelayakan keuangan pemulangan rawat inap. Ketika kasir menyelesaikan tagihan, Billing menerbitkan surat status kelayakan (`CLEARED`, `BLOCKED`, `REVOKED`) dengan nomor versi finansial yang naik secara teratur.
- **Perluasan Master Data `MstAdministrationFeePolicy`:** Menambahkan kolom `Percentage` (persentase), `CapAmount` (batas pagu rupiah), dan `CalculationType` (`FLAT` vs `PERCENTAGE_WITH_CAP`). Record master awal `ADM-RANAP-01` didaftarkan dengan persentase 7% dan pagu Rp6.000.000 aktif per 24 September 2026.
- **Migration Bersih dan Reversibel:** Naskah migrasi Entity Framework Core `AddInpatientBillingIntegrationAndClearanceHandoff` telah disusun lengkap dengan prosedur pemasangan (`Up`) dan pembatalan (`Down`) tanpa risiko merusak data existing.

---

- TASK ID: BE-BKC-071
- TASK TYPE: Fondasi Skema Database & Master Data (Model `BilInpatientClearanceHandoff`, perluasan `MstAdministrationFeePolicy`, dan migrasi EF Core)
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: 4 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah/dibuat 6 → 2 + logika skema dan integritas data → 1 + database tabel baru dan constraint → 1; total score 4)
- MODEL: Gemini 3.8 Flash (High)
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/**`, `Repositories/**`, `Migrations/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan task)
- FILES INSPECTED:
  - `docs/module-blueprints/billing-kasir/02-backend-architecture.md` (arsitektur revisi 1.5, `BKC-DES-042`–`050`)
  - `docs/module-blueprints/billing-kasir/contracts/state-transition-matrix.md` (`BIL-STATE-1.3`)
  - `docs/module-blueprints/billing-kasir/contracts/validation-matrix.md` (`BIL-VAL-121`–`126`)
  - `docs/module-blueprints/billing-kasir/data/data-dictionary.md` (spesifikasi tabel dan DDL PostgreSQL target)
  - `Areas/HealthServices/BillingManagement/MasterData/Models/MstAdministrationFeePolicy.cs`
  - `Repositories/Configurations/HealthServices/BillingManagement/MasterData/MstAdministrationFeePolicyConfiguration.cs`
  - `Repositories/ApplicationDbContext.cs`
  - `Migrations/20260922015126_AddBillCollectionPrescriptionHandoff.cs` (referensi pola migration handoff)
- FILES CHANGED:
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Models/BilInpatientClearanceHandoff.cs`
  - **Dibuat**: `Repositories/Configurations/HealthServices/BillingManagement/Billing/BilInpatientClearanceHandoffConfiguration.cs`
  - **Dibuat**: `Migrations/20260924054559_AddInpatientBillingIntegrationAndClearanceHandoff.cs`
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/MasterData/Models/MstAdministrationFeePolicy.cs` (menambahkan `Percentage`, `CapAmount`, `CalculationType`, dan konstanta `AdministrationFeeCalculationTypes`)
  - **Diperbarui**: `Repositories/Configurations/HealthServices/BillingManagement/MasterData/MstAdministrationFeePolicyConfiguration.cs` (menambahkan pemetaan kolom, check constraint `CalculationType`, nilai default pada draft seed, dan seed record baru `ADM-RANAP-01`)
  - **Diperbarui**: `Repositories/ApplicationDbContext.cs` (mendaftarkan `DbSet<BilInpatientClearanceHandoff> BilInpatientClearanceHandoffs`)
- IMPLEMENTATION:
  1. **Model `BilInpatientClearanceHandoff`**:
     Mewarisi `IdentityModel`. Memetakan kolom: `Id`, `EncounterId`, `InvoiceId`, `ClearanceStatus`, `FinancialOutcome`, `OutstandingBalance`, `TotalPatientResponsibility`, `TotalPaidOrAllocated`, `ReasonCode`, `RevocationReason`, `FinancialVersion`, `EffectiveAt`, `CorrelationId`, `CausationId`, `Status`, `AcknowledgedAt`, dan `RowVersion`. Menyediakan konstanta status kelayakan (`InpatientClearanceStatuses`), hasil finansial (`InpatientFinancialOutcomes`), dan kode alasan (`InpatientClearanceReasonCodes`).
  2. **Konfigurasi EF Core `BilInpatientClearanceHandoffConfiguration`**:
     - Check constraint: `CK_BilInpatientClearanceHandoff_ClearanceStatus` (`PENDING`, `BLOCKED`, `CLEARED`, `REVOKED`).
     - Check constraint: `CK_BilInpatientClearanceHandoff_Status` (`CREATED`, `ACKNOWLEDGED`).
     - Check constraint: `CK_BilInpatientClearanceHandoff_FinancialOutcome` (null atau salah satu dari `FULLY_PAID`, `INSURANCE_GUARANTEED`, `SETTLED_WITH_DEPOSIT`, `DISCHARGED_WITH_AR`).
     - Check constraint: `CK_BilInpatientClearanceHandoff_ReasonCode` (`INVOICE_SETTLED`, `GUARANTOR_APPROVED`, `DISCHARGE_ORDER_INITIATED`, `LATE_CHARGE_POSTED`, `PAYMENT_REVERSED`, `CORRECTION_APPLIED`).
     - Check constraint: `CK_BilInpatientClearanceHandoff_FinancialVersion` (`> 0`).
     - Indeks unik gabungan: `IX_BilInpatientClearanceHandoff_Encounter_Version` pada `(EncounterId, FinancialVersion)`.
     - Indeks performa: `IX_BilInpatientClearanceHandoff_Status`, `IX_BilInpatientClearanceHandoff_Invoice`, `IX_BilInpatientClearanceHandoff_ClearanceStatus`.
     - Foreign key: ke `BilInvoice` dengan `DeleteBehavior.Restrict`.
  3. **Pembaruan Model & Konfigurasi `MstAdministrationFeePolicy`**:
     - Penambahan properti `Percentage` (`numeric(5,2)`, nullable).
     - Penambahan properti `CapAmount` (`numeric(18,2)`, nullable).
     - Penambahan properti `CalculationType` (`varchar(30)`, default `'FLAT'`).
     - Check constraint `CK_MstAdministrationFeePolicy_CalculationType` (`FLAT`, `PERCENTAGE_WITH_CAP`).
     - Pendaftaran seed master `ADM-RANAP-01` (`7e49ba03-b808-4cff-8e71-735ec8d8b805`) dengan persentase 7%, cap Rp6.000.000, tipe `PERCENTAGE_WITH_CAP`, `IsActive = true`, efektif per 24 September 2026.
  4. **Pendaftaran di `ApplicationDbContext`**:
     Mendaftarkan `DbSet<BilInpatientClearanceHandoff> BilInpatientClearanceHandoffs`.
  5. **Migration EF Core**:
     Menyusun migrasi `20260924054559_AddInpatientBillingIntegrationAndClearanceHandoff.cs` secara deterministik dengan `Up` dan `Down` bersih.
- **Backend Governance Preflight**:
  - Area: `HealthServices` · Module: `BillingManagement/Billing` & `BillingManagement/MasterData` · Prefix: `Bil` & `Mst` · Registry: `ACTIVE`
  - Keberlakuan: `NEW CODE` untuk `BilInpatientClearanceHandoff` dan konfigurasinya; `TOUCHED LEGACY` untuk `MstAdministrationFeePolicy` dan `ApplicationDbContext`.
  - QBE Rules Evaluated: `QBE-ENT-001`, `QBE-NAM-001`, `QBE-CFG-001`, `QBE-CODE-002`, `QBE-CODE-003`, `QBE-MOD-002`, `QBE-SVC-001`.
  - QBE Conformance Check Result: **PASS** (Mode Strict, 4 berkas dievaluasi, 0 violations, 0 reviews, 0 info).
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (`BACKEND MODE`)
- API CONTRACT IMPACT: Nol perubahan endpoint HTTP pada task ini. Seluruh perubahan berada di lapisan persistensi dan konfigurasi EF Core.
- DATABASE IMPACT:
  - 1 tabel baru: `BilInpatientClearanceHandoff`
  - 3 kolom baru pada tabel existing `MstAdministrationFeePolicy` (`Percentage`, `CapAmount`, `CalculationType`)
  - 1 check constraint baru pada `MstAdministrationFeePolicy`
  - 4 indeks baru pada `BilInpatientClearanceHandoff` (termasuk 1 indeks unik gabungan)
  - 1 seed data baru `ADM-RANAP-01`
- SECURITY IMPACT: Kolom rupiah sensitif (`OutstandingBalance`, `TotalPatientResponsibility`, `TotalPaidOrAllocated`) ditandai dan dilindungi sesuai data dictionary agar tidak diekspos sembarangan ke non-kasir.
- VISUAL REFERENCE: NOT REQUIRED
- VALIDATION:
  | Command/Check | Result | Classification | Evidence/Note |
  | --- | --- | --- | --- |
  | `Invoke-QbeConformanceCheck.ps1` | **PASS** | VERIFIED | Strict mode, 4 berkas dievaluasi, 0 pelanggaran arsitektur QBE |
  | `dotnet build` | **Menunggu eksekusi mandiri pengguna** | NOT VERIFIED | Sesuai instruksi eksplisit pengguna: *"jangan jalankan dotnet build secara automatis"* |
  | Review Diff dan Scope | Selesai | VERIFIED | Seluruh properti, precision, nullable, check constraints, dan nama kolom sesuai persis dengan `data-dictionary.md` Amendment 24 September 2026 |
  | Migration Up dan Down | Selesai | VERIFIED (Structure) | Naskah `20260924054559_AddInpatientBillingIntegrationAndClearanceHandoff.cs` memiliki method `Up` dan `Down` simetris |
  | Unique Index Integritas | Selesai | VERIFIED (Structure) | Indeks unik gabungan `(EncounterId, FinancialVersion)` terpasang untuk mencegah duplikasi versi per encounter |
  | Master Policy Seed `ADM-RANAP-01` | Selesai | VERIFIED | Nilai 7.00% dan cap Rp6.000.000 terpasang dengan `PERCENTAGE_WITH_CAP` |
- WARNINGS:
  - Eksekusi migration EF Core ke basis data pengembang (`dotnet ef database update`) **belum dijalankan** dan memerlukan otorisasi terpisah sesuai aturan keselamatan basis data Quilvian.
  - Kompilasi proyek (`dotnet build`) belum dijalankan secara otomatis mengikuti instruksi pengguna; silakan jalankan `dotnet build` secara mandiri.
- KNOWN ISSUES: Tidak ada issue baru pada source yang ditulis.
- NEXT TASKS:
  - `BE-BKC-072` (Adapter Room Stay & Konsolidasi Alihan IGD Non-Destruktif)
  - `BE-BKC-073` (Mesin Kalkulasi Sewa Kamar Bertingkat & Pro-Rata Transfer Menit Riil)
  - `BE-BKC-074` (Layanan Biaya Administrasi Ranap 7% Cap Rp6jt & Penggantian Admin Rajal)
  *(Ketiga task di atas dapat berjalan secara paralel setelah `BE-BKC-071` selesai).*
