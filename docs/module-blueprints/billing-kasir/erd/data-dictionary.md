# Billing dan Kasir — Kamus Data

> Revision `0.4`, status **approved**. Semua tabel `Bil*` dan empat master policy berstatus **Baru/Rencana**. Kolom audit bawaan `IdentityModel` (`CreatedAt`, `CreatedBy`, perubahan/soft-delete sesuai model project) wajib tetapi tidak diulang. `decimal` uang memakai precision konsisten yang diputuskan migration task (rekomendasi `decimal(18,2)`); quantity dapat `decimal(18,4)`.

## Kolom baru

| Tabel | Kolom selain audit | Kunci/aturan | Sensitif |
| --- | --- | --- | :---: |
| `BilInvoice` | `Id`, `EncounterId`, `InvoiceNumber`, `ServiceType`, `Status`, `CurrentCalculationVersion`, `InvoiceDate`, `ClosedAt`, `RowVersion` | UK EncounterId, UK InvoiceNumber | EncounterId Ya |
| `BilInvoiceItem` | `Id`, `InvoiceId`, `SourceDomain`, `SourceDetailId`, `CategoryId`, `TariffId?` (**baru 2 Sep 2026**), `DescriptionSnapshot`, `Quantity`, `UnitPrice`, `DoctorShare`, `Status`, `VoidReason` | FK invoice/category/tariff(opsional); UK active source tuple | Description/Source Ya |
| `BilCalculationVersion` | `Id`, `InvoiceId`, `VersionNo`, `GrossAmount`, `ItemDiscount`, `TotalDiscount`, `TaxAmount`, `PatientAmount`, `PrimaryAmount`, `ExcessAmount`, `RoundingAmount`, `IsLocked`, `CalculatedAt`, `Reason` | UK InvoiceId+VersionNo; immutable | Ya |
| `BilDiscountApplication` | `Id`, `InvoiceId`, `InvoiceItemId?`, `DiscountPolicyId`, `DiscountType`, `RequestedAmount`, `Amount`, `ApprovalStatus`, `RequestedBy`, `ApprovedBy?`, `Reason` | doctor approver wajib untuk doctor share | Ya |
| `BilDepositAccount` | `Id`, `EncounterId`, `AccountNumber`, `AvailableBalance`, `Status`, `RowVersion` | UK encounter/account | Ya |
| `BilDepositMovement` | `Id`, `DepositAccountId`, `MovementType`, `Amount`, `SettlementId?`, `CorrelationId`, `OccurredAt`, `Reason`, `ReversesMovementId?` | UK CorrelationId; amount positif, direction dari type | Ya |
| `BilSettlement` | `Id`, `InvoiceId?`, `DepositAccountId?`, `Purpose`, `RequestedAmount`, `SuccessfulAmount`, `AllocatedAmount`, `Status`, `IdempotencyKey`, `StartedAt`, `CompletedAt?` | UK idempotency; satu target wajib | Ya |
| `BilTender` | `Id`, `SettlementId`, `PaymentMethodId`, `Amount`, `Status`, `ProviderReference?`, `ProviderStatusCode?`, `AttemptedAt`, `SettledAt?`, `CashierShiftId?` | provider ref unik bila ada | Provider ref Ya |
| `BilPaymentAllocation` | `Id`, `SettlementId`, `TargetType`, `TargetId`, `Amount`, `CalculationVersion?`, `AllocatedAt`, `ReversesAllocationId?` | total ≤ successful funds | Ya |
| `BilRefundableCredit` | `Id`, `InvoiceId`, `SourceType`, `SourceId`, `OriginalAmount`, `AvailableAmount`, `Status`, `RecognizedAt` | available 0..original | Ya |
| `BilAdjustment` | `Id`, `InvoiceId`, `AdjustmentType`, `Direction`, `Amount`, `Status`, `ReasonCode`, `Reason`, `MakerId`, `ApproverId?`, `CorrelationId`, `ReversesAdjustmentId?`, `EffectiveAt` | UK correlation; immutable after posted | Ya |
| `BilRefundCase` | `Id`, `InvoiceId`, `RefundNumber`, `Reason`, `RequestedAmount`, `ApprovedAmount`, `ExecutedAmount`, `Status`, `MakerId`, `ApproverId?`, `OriginalSettlementId`, `CreatedAtBusiness` | UK refund number; maker≠approver | Ya |
| `BilWriteOffCase` | `Id`, `InvoiceId`, `WriteOffNumber`, `Reason`, `RequestedAmount`, `ApprovedAmount`, `Status`, `MakerId`, `ApproverId?`, `ReversedByCaseId?` | UK number; maker≠approver | Ya |
| `BilCashierShift` | `Id`, `ShiftNumber`, `CashierId`, `RegisterId`, `OpeningCash`, `SystemCash`, `PhysicalCash`, `Variance`, `Status`, `OpenedAt`, `ClosedAt?`, `RowVersion` | UK shift number; one active cashier/register | CashierId Ya |
| `BilCashVarianceReview` | `Id`, `ShiftId`, `ReviewerId`, `Variance`, `Resolution`, `Reason`, `ReviewedAt`, `ReopenAuthorizedBy?` | review immutable | Actor IDs Ya |
| `BilFinalizationRecord` | `Id`, `InvoiceId`, `CalculationVersion`, `SettlementOutcome`, `DepartureExceptionType?`, `DebtorEvidence?`, `InvoiceDate`, `FinalizedAt`, `FinalizedBy` | UK invoice/final effect | Debtor evidence Ya |
| `BilArHandoff` | `Id`, `FinalizationId`, `DebtorType`, `DebtorId`, `Amount`, `InvoiceDate`, `DueDate`, `Status`, `IdempotencyKey`, `ExternalReference?` | UK key; per debtor | DebtorId Ya |
| `BilApHandoff` | `Id`, `FinalizationId`, `DoctorId`, `Amount`, `ReadinessStatus`, `ReadinessPolicy`, `IdempotencyKey`, `ExternalReference?` | UK key | DoctorId Ya |
| `BilHandoffAdjustment` | `Id`, `OriginalHandoffId`, `TargetLedger`, `Direction`, `Amount`, `Reason`, `CorrelationId`, `ExternalReference?`, `PostedAt` | UK correlation | Ya |
| `MstAdministrationFeePolicy` | `Id`, `Code`, `Name`, `ServiceType`, `Amount`, `OncePerPatientLocalDay`, `ReplacementPriority`, `Coverable`, `Discountable=false`, `EffectiveFrom`, `EffectiveTo?`, `IsActive` | no overlapping active rule | Tidak |
| `MstDiscountPolicy` | `Id`, `Code`, `Name`, `DiscountType`, `TargetComponent`, `ValueType`, `Value`, `Limit`, `RequiresApproval`, `ApproverRole?`, `EffectiveFrom`, `EffectiveTo?`, `IsActive` | doctor target only doctor share | Tidak |
| `MstTaxRule` | `Id`, `Code`, `Name`, `TaxableCategory`, `Rate`, `RoundingMode`, `AllocationRule`, `EffectiveFrom`, `EffectiveTo?`, `IsActive` | no overlapping category rule | Tidak |
| `MstRoomChargePolicy` | `Id`, `Code`, `Name`, `MinimumMinutes`, `PeriodMinutes`, `RemainderRounding`, `TariffMoment`, `LeaveRule`, `EffectiveFrom`, `EffectiveTo?`, `IsActive` | effective-dated | Tidak |

Existing `MstPaymentMethod` dan `MstBillingItemCategory` cukup direferensikan lewat `Id`; definisi penuh tetap pada model existing Billing Master Data.

## Bentuk DDL dokumentasi

**Peringatan:** potongan berikut hanya dokumentasi bentuk target, bukan skrip migration dan tidak boleh dijalankan. Karena configuration EF belum ada, ini adalah constraint minimum yang kelak harus dihasilkan oleh `IEntityTypeConfiguration<T>`, bukan DDL yang diklaim berasal dari source.

```sql
-- Bentuk target ringkas; tipe/length/nama constraint final ditetapkan Configuration EF.
CREATE TABLE BilInvoice (Id uniqueidentifier PK, EncounterId uniqueidentifier UK, InvoiceNumber nvarchar(50) UK, ServiceType nvarchar(30), Status nvarchar(30), CurrentCalculationVersion int, InvoiceDate datetimeoffset, ClosedAt datetimeoffset NULL, RowVersion rowversion);
CREATE TABLE BilInvoiceItem (Id uniqueidentifier PK, InvoiceId uniqueidentifier FK, SourceDomain nvarchar(50), SourceDetailId nvarchar(100), CategoryId uniqueidentifier FK, DescriptionSnapshot nvarchar(250), Quantity decimal(18,4), UnitPrice decimal(18,2), DoctorShare decimal(18,2), Status nvarchar(30), VoidReason nvarchar(500) NULL);
CREATE TABLE BilCalculationVersion (Id uniqueidentifier PK, InvoiceId uniqueidentifier FK, VersionNo int, GrossAmount decimal(18,2), ItemDiscount decimal(18,2), TotalDiscount decimal(18,2), TaxAmount decimal(18,2), PatientAmount decimal(18,2), PrimaryAmount decimal(18,2), ExcessAmount decimal(18,2), RoundingAmount decimal(18,2), IsLocked bit, CalculatedAt datetimeoffset, Reason nvarchar(500));
CREATE TABLE BilDiscountApplication (Id uniqueidentifier PK, InvoiceId uniqueidentifier FK, InvoiceItemId uniqueidentifier NULL FK, DiscountPolicyId uniqueidentifier FK, DiscountType nvarchar(30), RequestedAmount decimal(18,2), Amount decimal(18,2), ApprovalStatus nvarchar(30), RequestedBy uniqueidentifier, ApprovedBy uniqueidentifier NULL, Reason nvarchar(500));
CREATE TABLE BilDepositAccount (Id uniqueidentifier PK, EncounterId uniqueidentifier UK, AccountNumber nvarchar(50) UK, AvailableBalance decimal(18,2), Status nvarchar(30), RowVersion rowversion);
CREATE TABLE BilDepositMovement (Id uniqueidentifier PK, DepositAccountId uniqueidentifier FK, MovementType nvarchar(30), Amount decimal(18,2), SettlementId uniqueidentifier NULL FK, CorrelationId uniqueidentifier UK, OccurredAt datetimeoffset, Reason nvarchar(500), ReversesMovementId uniqueidentifier NULL FK);
CREATE TABLE BilSettlement (Id uniqueidentifier PK, InvoiceId uniqueidentifier NULL FK, DepositAccountId uniqueidentifier NULL FK, Purpose nvarchar(30), RequestedAmount decimal(18,2), SuccessfulAmount decimal(18,2), AllocatedAmount decimal(18,2), Status nvarchar(30), IdempotencyKey uniqueidentifier UK, StartedAt datetimeoffset, CompletedAt datetimeoffset NULL);
CREATE TABLE BilTender (Id uniqueidentifier PK, SettlementId uniqueidentifier FK, PaymentMethodId uniqueidentifier FK, Amount decimal(18,2), Status nvarchar(30), ProviderReference nvarchar(150) NULL, ProviderStatusCode nvarchar(50) NULL, AttemptedAt datetimeoffset, SettledAt datetimeoffset NULL, CashierShiftId uniqueidentifier NULL FK);
CREATE TABLE BilPaymentAllocation (Id uniqueidentifier PK, SettlementId uniqueidentifier FK, TargetType nvarchar(30), TargetId uniqueidentifier, Amount decimal(18,2), CalculationVersion int NULL, AllocatedAt datetimeoffset, ReversesAllocationId uniqueidentifier NULL FK);
CREATE TABLE BilRefundableCredit (Id uniqueidentifier PK, InvoiceId uniqueidentifier FK, SourceType nvarchar(30), SourceId uniqueidentifier, OriginalAmount decimal(18,2), AvailableAmount decimal(18,2), Status nvarchar(30), RecognizedAt datetimeoffset);
CREATE TABLE BilAdjustment (Id uniqueidentifier PK, InvoiceId uniqueidentifier FK, AdjustmentType nvarchar(30), Direction nvarchar(10), Amount decimal(18,2), Status nvarchar(30), ReasonCode nvarchar(30), Reason nvarchar(500), MakerId uniqueidentifier, ApproverId uniqueidentifier NULL, CorrelationId uniqueidentifier UK, ReversesAdjustmentId uniqueidentifier NULL FK, EffectiveAt datetimeoffset);
CREATE TABLE BilRefundCase (Id uniqueidentifier PK, InvoiceId uniqueidentifier FK, RefundNumber nvarchar(50) UK, Reason nvarchar(500), RequestedAmount decimal(18,2), ApprovedAmount decimal(18,2), ExecutedAmount decimal(18,2), Status nvarchar(30), MakerId uniqueidentifier, ApproverId uniqueidentifier NULL, OriginalSettlementId uniqueidentifier FK, CreatedAtBusiness datetimeoffset);
CREATE TABLE BilWriteOffCase (Id uniqueidentifier PK, InvoiceId uniqueidentifier FK, WriteOffNumber nvarchar(50) UK, Reason nvarchar(500), RequestedAmount decimal(18,2), ApprovedAmount decimal(18,2), Status nvarchar(30), MakerId uniqueidentifier, ApproverId uniqueidentifier NULL, ReversedByCaseId uniqueidentifier NULL FK);
CREATE TABLE BilCashierShift (Id uniqueidentifier PK, ShiftNumber nvarchar(50) UK, CashierId uniqueidentifier, RegisterId uniqueidentifier, OpeningCash decimal(18,2), SystemCash decimal(18,2), PhysicalCash decimal(18,2), Variance decimal(18,2), Status nvarchar(30), OpenedAt datetimeoffset, ClosedAt datetimeoffset NULL, RowVersion rowversion);
CREATE TABLE BilCashVarianceReview (Id uniqueidentifier PK, ShiftId uniqueidentifier FK, ReviewerId uniqueidentifier, Variance decimal(18,2), Resolution nvarchar(30), Reason nvarchar(500), ReviewedAt datetimeoffset, ReopenAuthorizedBy uniqueidentifier NULL);
CREATE TABLE BilFinalizationRecord (Id uniqueidentifier PK, InvoiceId uniqueidentifier UK, CalculationVersion int, SettlementOutcome nvarchar(30), DepartureExceptionType nvarchar(30) NULL, DebtorEvidence nvarchar(500) NULL, InvoiceDate datetimeoffset, FinalizedAt datetimeoffset, FinalizedBy uniqueidentifier);
CREATE TABLE BilArHandoff (Id uniqueidentifier PK, FinalizationId uniqueidentifier FK, DebtorType nvarchar(30), DebtorId uniqueidentifier, Amount decimal(18,2), InvoiceDate datetimeoffset, DueDate datetimeoffset, Status nvarchar(30), IdempotencyKey uniqueidentifier UK, ExternalReference nvarchar(100) NULL);
CREATE TABLE BilApHandoff (Id uniqueidentifier PK, FinalizationId uniqueidentifier FK, DoctorId uniqueidentifier, Amount decimal(18,2), ReadinessStatus nvarchar(30), ReadinessPolicy nvarchar(100), IdempotencyKey uniqueidentifier UK, ExternalReference nvarchar(100) NULL);
CREATE TABLE BilHandoffAdjustment (Id uniqueidentifier PK, OriginalHandoffId uniqueidentifier, TargetLedger nvarchar(10), Direction nvarchar(10), Amount decimal(18,2), Reason nvarchar(500), CorrelationId uniqueidentifier UK, ExternalReference nvarchar(100) NULL, PostedAt datetimeoffset);
CREATE TABLE MstAdministrationFeePolicy (Id uniqueidentifier PK, Code nvarchar(30) UK, Name nvarchar(100), ServiceType nvarchar(30), Amount decimal(18,2), OncePerPatientLocalDay bit, ReplacementPriority int, Coverable bit, Discountable bit, EffectiveFrom datetimeoffset, EffectiveTo datetimeoffset NULL, IsActive bit);
CREATE TABLE MstDiscountPolicy (Id uniqueidentifier PK, Code nvarchar(30) UK, Name nvarchar(100), DiscountType nvarchar(30), TargetComponent nvarchar(30), ValueType nvarchar(20), Value decimal(18,2), Limit decimal(18,2) NULL, RequiresApproval bit, ApproverRole nvarchar(50) NULL, EffectiveFrom datetimeoffset, EffectiveTo datetimeoffset NULL, IsActive bit);
CREATE TABLE MstTaxRule (Id uniqueidentifier PK, Code nvarchar(30) UK, Name nvarchar(100), TaxableCategory nvarchar(30), Rate decimal(18,6), RoundingMode nvarchar(30), AllocationRule nvarchar(50), EffectiveFrom datetimeoffset, EffectiveTo datetimeoffset NULL, IsActive bit);
CREATE TABLE MstRoomChargePolicy (Id uniqueidentifier PK, Code nvarchar(30) UK, Name nvarchar(100), MinimumMinutes int, PeriodMinutes int, RemainderRounding nvarchar(30), TariffMoment nvarchar(30), LeaveRule nvarchar(50), EffectiveFrom datetimeoffset, EffectiveTo datetimeoffset NULL, IsActive bit);
CREATE UNIQUE INDEX UK_BilInvoiceItem_Source
 ON BilInvoiceItem(SourceDomain, SourceDetailId) WHERE Status <> 'VOIDED';
CREATE UNIQUE INDEX UK_BilCalculationVersion_InvoiceVersion
 ON BilCalculationVersion(InvoiceId, VersionNo);
CREATE UNIQUE INDEX UK_BilCommand_Idempotency ON BilSettlement(IdempotencyKey);
CREATE UNIQUE INDEX UK_BilAdjustment_Correlation ON BilAdjustment(CorrelationId);
```

Empat master policy membutuhkan check `EffectiveTo > EffectiveFrom`, serta validasi service untuk mencegah periode aktif overlap. Seluruh FK finansial menggunakan delete restrict; histori tidak cascade-delete.

## Amendment 2 September 2026 — `BilInvoiceItem.TariffId`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `TariffId` | `Guid?` | Tidak | `null` | Index (non-unik) | FK ke `MstTariff.Id` | `Restrict` | Tidak | Diisi hanya untuk item hasil entri katalog (`SourceDomain="ADHOC_CATALOG"`); `null` untuk item lama/free-form |

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
ALTER TABLE public."BilInvoiceItem"
    ADD COLUMN "TariffId" uuid NULL;

ALTER TABLE public."BilInvoiceItem"
    ADD CONSTRAINT "FK_BilInvoiceItem_MstTariff_TariffId"
    FOREIGN KEY ("TariffId") REFERENCES public."MstTariff" ("Id") ON DELETE RESTRICT;

CREATE INDEX "IX_BilInvoiceItem_TariffId" ON public."BilInvoiceItem" ("TariffId");
```

`MstTariff` sudah ada (`Areas/HealthServices/MasterData/Models/MstTariff.cs`) — hanya kolom kunci yang relevan bagi modul ini: `Id` PK, `TariffCategoryId` FK ke `MstTariffCategory`, `NormalPrice` (dasar harga server-side), `IsActive`/`EffectiveStartDate`/`EffectiveEndDate` (dasar validasi `BIL-VAL-025`). Kolom lengkap lihat file model.

## Amendment 3 September 2026 — Dokumen Invoice Asuransi

**Tidak ada kolom baru dan tidak ada tabel baru** (`BKC-DES-003`). Tidak ada pula tabel yang berubah status menjadi `Diperbarui`, sehingga tidak ada bagian DDL tambahan pada amendment ini.

Yang berubah hanyalah **isi** satu kolom yang sudah ada:

| Tabel | Kolom | Tipe | Status kolom | Apa yang berubah |
| --- | --- | --- | --- | --- |
| `BilCalculationVersion` | `BreakdownSnapshot` | `text` (JSON) | `Sudah ada`, bentuk tidak berubah | Objek JSON di dalamnya bertambah properti: `items[].coveredNetAmount`, `items[].coveredTaxAmount`, `items[].coveredAmount`, `items[].unresolvedAmount`, `items[].patientAmount`, `administrationFee.coveredNetAmount`/`coveredTaxAmount`/`coveredAmount`, `roomCharge.coveredNetAmount`/`coveredTaxAmount`/`coveredAmount`, dan `coverage.isPerItemAllocationAvailable`. Nilai `contractVersion` di dalamnya berubah dari `BIL-CALCULATION-0.4` menjadi `BIL-CALCULATION-0.5` |

Karena kolom ini bertipe teks JSON dan bukan kolom bertipe kuat, penambahan properti **tidak** memerlukan migration, **tidak** mengubah panjang maksimum, dan **tidak** memerlukan index baru. Baris yang sudah tersimpan tetap sah apa adanya; properti yang belum ada akan terbaca sebagai nilai bawaan (`0` untuk angka, `false` untuk boolean) — dan `coverage.isPerItemAllocationAvailable = false` itulah penanda resmi bahwa rincian per baris memang tidak tersedia untuk baris tersebut, bukan bahwa tanggungannya nol.

**Contoh.** Sebuah `BilCalculationVersion` yang tersimpan 1 September 2026 memuat `{"contractVersion":"BIL-CALCULATION-0.4","items":[{"invoiceItemId":"…","grossAmount":100000,"coverable":true}], …}`. Dibaca setelah pembaruan, `items[0].coveredAmount` bernilai `0` dan `coverage.isPerItemAllocationAvailable` bernilai `false`. Angka `0` itu **MUST NOT** ditampilkan sebagai "asuransi menanggung Rp 0"; yang benar adalah "rincian per item tidak tersedia untuk tagihan ini" (`BIL-VAL-033`), sementara total tanggungannya tetap sah karena tersimpan sebagai kolom relasional `BilCalculationVersion.PrimaryAmount`.

### Kolom milik modul lain yang dibaca dokumen ini

Tabel-tabel berikut berstatus `Sudah ada` dan **tidak** berubah; hanya kolom kunci yang relevan bagi dokumen Invoice Asuransi yang dicatat di sini. Kolom lengkapnya lihat berkas model masing-masing.

`TrxPatientEncounterGuarantor` — `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs`

| Kolom | Tipe | Relasi | Sensitif | Keterangan |
| --- | --- | --- | :---: | --- |
| `Id` | `Guid` | PK | Tidak | — |
| `EncounterId` | `Guid` | FK ke `TrxPatientEncounter` | **Ya** | Penghubung ke kunjungan yang ditagihkan |
| `PaymentType` | `int` (enum) | — | Tidak | Menentukan `payerKind` dokumen: `Cash`, `Insurance`, atau `CompanyGuarantor` |
| `IsActive` | `bool` | — | Tidak | Hanya baris aktif yang dibaca |
| `InsuranceProviderId` | `Guid?` | FK ke `MstInsuranceProvider` | Tidak | Wajib terisi untuk `Insurance`; menjadi rujukan blok perusahaan |
| `PolicyNumberSnapshot` | `string(100)` | — | **Ya** | Nomor polis pada saat registrasi |
| `MemberNumberSnapshot` | `string(100)` | — | **Ya** | Nomor anggota pada saat registrasi |
| `PlanNameSnapshot` | `string(150)` | — | Tidak | Nama paket manfaat |
| `ClassNameSnapshot` | `string(150)` | — | Tidak | Kelas manfaat |
| `BenefitPlanCodeSnapshot` | `string(100)` | — | Tidak | Kode paket; juga dipakai pencocokan aturan coverage |
| `EffectiveStartDateSnapshot`, `EffectiveEndDateSnapshot` | `DateTime?` | — | Tidak | Masa berlaku kartu pada saat registrasi |
| `IsEligible`, `IsPolicyActive` | `bool` | — | Tidak | Kelayakan pada saat registrasi |
| `CardNumberSnapshot` | `string(100)` | — | **Ya** | **MUST NOT dibaca amendment ini** — lihat `02-backend-architecture.md` § Yang sengaja tidak dibuat |

`MstInsuranceProvider` — `Areas/Administrator/MasterData/Models/MstInsuranceProvider.cs`

| Kolom | Tipe | Relasi | Sensitif | Keterangan |
| --- | --- | --- | :---: | --- |
| `Id` | `Guid` | PK | Tidak | — |
| `InsuranceProviderCode` | `string(50)` | Unik secara bisnis | Tidak | Kode perusahaan |
| `InsuranceProviderName` | `string(200)` | — | Tidak | Nama yang dicetak sebagai tujuan dokumen |
| `InsuranceGroupName` | `string(100)?` | — | Tidak | Nama grup, bila ada |
| `ProviderType` | `string(50)` | — | Tidak | `PrivateInsurance`, `TPA`, `GovernmentInsurance`, `CorporateInsurance`, `Other` |
| `ClaimMethod` | `string(50)` | — | Tidak | `Cashless`, `Reimbursement`, `GuaranteeLetter`, `Mixed` |
| `ContractNumber` | `string(100)?` | — | Tidak | Nomor kontrak kerja sama, dicetak pada dokumen |
| `OfficeAddress` | `string(500)?` | — | Tidak | Alamat tujuan, dicetak pada dokumen |
| `IsActive` | `bool` | — | Tidak | Hanya perusahaan aktif yang dibaca |
| `PicName`, `PicPhoneNumber`, `PicWhatsAppNumber`, `PicEmail`, `BillingInstruction`, `ClaimInstruction` | berbagai | — | Tidak | **MUST NOT dibaca amendment ini** — data operasional internal, bukan bagian lembar tagihan |

---

## Amendment 4 September 2026 — pindah ke `data/data-dictionary.md`

Amendment 4 September 2026 (`BKC-DEC-070`–`079`, `BKC-DES-010`–`020`) **tidak menambah tabel, kolom, maupun migration**. Perubahan kontrak datanya seluruhnya berada di dalam JSON `BilCalculationVersion.BreakdownSnapshot` dan pada kolom-kolom yang berhenti atau mulai dibaca — dan itu dicatat di lokasi kamus data yang berlaku menurut struktur keluaran blueprint saat ini:

> **[`../data/data-dictionary.md`](../data/data-dictionary.md)**

Berkas ini (`erd/data-dictionary.md`) tetap memegang kamus data **baseline** — seluruh kolom tabel `Bil*`, empat master policy, skema DDL, dan kolom milik modul lain yang dibaca dokumen Invoice Asuransi. Isinya **tidak** disalin ke lokasi baru, supaya tidak ada dua sumber kebenaran yang dapat saling menyimpang.

Penyatuan keduanya ke satu lokasi adalah perubahan struktur yang menyentuh rujukan pada belasan berkas lain. Ia **MUST** dikerjakan sebagai revisi tersendiri oleh `/manage-module-blueprint`, bukan sebagai efek samping pass desain — lihat `BKC-OQ-089` pada [`../04-prd-to-mvp.md`](../04-prd-to-mvp.md).
