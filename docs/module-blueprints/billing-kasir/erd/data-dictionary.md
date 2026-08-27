# Billing dan Kasir — Kamus Data

> Revision `0.4`, status **approved**. Semua tabel `Bil*` dan empat master policy berstatus **Baru/Rencana**. Kolom audit bawaan `IdentityModel` (`CreatedAt`, `CreatedBy`, perubahan/soft-delete sesuai model project) wajib tetapi tidak diulang. `decimal` uang memakai precision konsisten yang diputuskan migration task (rekomendasi `decimal(18,2)`); quantity dapat `decimal(18,4)`.

## Kolom baru

| Tabel | Kolom selain audit | Kunci/aturan | Sensitif |
| --- | --- | --- | :---: |
| `BilInvoice` | `Id`, `EncounterId`, `InvoiceNumber`, `ServiceType`, `Status`, `CurrentCalculationVersion`, `InvoiceDate`, `ClosedAt`, `RowVersion` | UK EncounterId, UK InvoiceNumber | EncounterId Ya |
| `BilInvoiceItem` | `Id`, `InvoiceId`, `SourceDomain`, `SourceDetailId`, `CategoryId`, `DescriptionSnapshot`, `Quantity`, `UnitPrice`, `DoctorShare`, `Status`, `VoidReason` | FK invoice/category; UK active source tuple | Description/Source Ya |
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
