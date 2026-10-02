# Medical Fee — Kamus Data

| Field | Nilai |
|---|---|
| Blueprint ID | `MF-BP-001` |
| Revision | `1` — `draft` |
| Cakupan | 9 tabel baru, seluruhnya milik modul ini |

Seluruh entity mewarisi `IdentityModel`: `Id`, `CreateDateTime`, `CreateBy`, `UpdateDateTime`,
`UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, `IsDelete`.
Kolom-kolom itu **tidak diulang** di tabel-tabel di bawah. Penghapusan selalu soft delete.

Seluruh kolom uang `decimal` memakai `HasPrecision(18, 2)`. Seluruh kolom persentase memakai
`HasPrecision(5, 2)`. Seluruh kolom status `varchar(30)`.

---

## 1. `MstMedicalFeeRole`

| Kolom | Tipe | Null | Bawaan | Keterangan |
|---|---|:---:|---|---|
| `RoleCode` | `varchar(30)` | Tidak | — | Kode peran, unik. Contoh `OPERATOR`, `ASSISTANT`, `ANESTHESIA` |
| `RoleName` | `varchar(100)` | Tidak | — | Nama tampil |
| `OprTeamRoleMapping` | `varchar(30)` | Ya | `null` | Nilai `OprTeamRole` yang dipetakan. Kosong = tanpa padanan di kamar operasi |
| `IsPrimaryRole` | `boolean` | Tidak | `false` | Penanda peran utama; dipakai saat sumber hanya menyimpan satu pelaksana |
| `Description` | `varchar(500)` | Ya | `null` | — |
| `IsActive` | `boolean` | Tidak | `true` | Peran dinonaktifkan, bukan dihapus |

**Index dan constraint**

| Jenis | Definisi |
|---|---|
| Unique (partial) | `(RoleCode) WHERE "IsDelete" = false` |
| Unique (partial) | `(OprTeamRoleMapping) WHERE "IsDelete" = false AND "OprTeamRoleMapping" IS NOT NULL` |
| Index | `(IsActive)` |

---

## 2. `MdfSharingAgreement`

| Kolom | Tipe | Null | Bawaan | Keterangan |
|---|---|:---:|---|---|
| `AgreementNumber` | `varchar(50)` | Tidak | — | Nomor kesepakatan tarif, unik |
| `SourceContractHistoryId` | `uuid` | Tidak | — | FK `WfpContractHistory.Id`, `Restrict` |
| `PayeeType` | `varchar(30)` | Tidak | — | `Doctor` atau `Workforce` |
| `PayeeReferenceId` | `uuid` | Tidak | — | Rujukan polimorfik menurut `PayeeType`. Tanpa FK |
| `EffectiveStart` | `date` | Tidak | — | — |
| `EffectiveEnd` | `date` | Ya | `null` | Kosong = mengikuti akhir kontrak |
| `Status` | `varchar(30)` | Tidak | `Draft` | `Draft`, `Active`, `Expired`, `Terminated` |
| `Notes` | `varchar(1000)` | Ya | `null` | — |
| `RowVersion` | `uuid` | Tidak | `gen_random_uuid()` | Optimistic concurrency (`MDF-DES-005`) |

**Index dan constraint**

| Jenis | Definisi |
|---|---|
| Unique (partial) | `(AgreementNumber) WHERE "IsDelete" = false` |
| Index | `(SourceContractHistoryId)` |
| Index | `(PayeeType, PayeeReferenceId, Status)` |
| Check `CK_MdfSharingAgreement_Status` | `Status IN ('Draft','Active','Expired','Terminated')` |
| Check `CK_MdfSharingAgreement_PayeeType` | `PayeeType IN ('Doctor','Workforce')` |
| Check `CK_MdfSharingAgreement_Period` | `EffectiveEnd IS NULL OR EffectiveEnd >= EffectiveStart` |

---

## 3. `MdfSharingRule`

| Kolom | Tipe | Null | Bawaan | Keterangan |
|---|---|:---:|---|---|
| `AgreementId` | `uuid` | Tidak | — | FK `MdfSharingAgreement.Id`, `Cascade` |
| `TariffId` | `uuid` | Ya | `null` | FK `MstTariff.Id`, `Restrict`. Kosong = seluruh layanan |
| `TariffCategoryId` | `uuid` | Ya | `null` | FK `MstTariffCategory.Id`, `Restrict`. Kosong = seluruh kategori |
| `RoleId` | `uuid` | Tidak | — | FK `MstMedicalFeeRole.Id`, `Restrict` |
| `SharingPercentage` | `decimal(5,2)` | Tidak | — | 0..100 |
| `EffectiveStart` | `date` | Tidak | — | — |
| `EffectiveEnd` | `date` | Ya | `null` | Diisi saat digantikan baris baru |
| `SupersededByRuleId` | `uuid` | Ya | `null` | FK ke dirinya sendiri, `Restrict` |
| `Notes` | `varchar(500)` | Ya | `null` | — |

**Index dan constraint**

| Jenis | Definisi |
|---|---|
| Index | `(AgreementId, EffectiveStart, EffectiveEnd)` |
| Index | `(TariffId)`, `(TariffCategoryId)`, `(RoleId)` |
| Check `CK_MdfSharingRule_Percentage` | `SharingPercentage >= 0 AND SharingPercentage <= 100` |
| Check `CK_MdfSharingRule_Period` | `EffectiveEnd IS NULL OR EffectiveEnd >= EffectiveStart` |

Tidak ada unique index pada (`AgreementId`, `TariffId`, `TariffCategoryId`, `RoleId`) — justru
dibutuhkan beberapa baris untuk kombinasi yang sama pada masa berlaku berbeda (`MDF-DES-009`).
Tumpang-tindih masa berlaku ditolak service, bukan database.

---

## 4. `MdfFeePeriod`

| Kolom | Tipe | Null | Bawaan | Keterangan |
|---|---|:---:|---|---|
| `PeriodCode` | `varchar(20)` | Tidak | — | Contoh `2026-09`, unik |
| `PeriodStart` | `date` | Tidak | — | — |
| `PeriodEnd` | `date` | Tidak | — | — |
| `Status` | `varchar(30)` | Tidak | `Open` | `Open`, `Calculated`, `Verified`, `Approved`, `Closed` |
| `CalculatedAt` | `timestamptz` | Ya | `null` | Kapan perhitungan terakhir dijalankan |
| `CalculatedBy` | `uuid` | Ya | `null` | — |
| `VerifiedBy` | `uuid` | Ya | `null` | — |
| `VerifiedAt` | `timestamptz` | Ya | `null` | — |
| `ApprovedBy` | `uuid` | Ya | `null` | — |
| `ApprovedAt` | `timestamptz` | Ya | `null` | — |
| `ClosedBy` | `uuid` | Ya | `null` | — |
| `ClosedAt` | `timestamptz` | Ya | `null` | — |
| `RowVersion` | `uuid` | Tidak | `gen_random_uuid()` | — |

**Index dan constraint**

| Jenis | Definisi |
|---|---|
| Unique (partial) | `(PeriodCode) WHERE "IsDelete" = false` |
| Index | `(Status, PeriodStart)` |
| Check `CK_MdfFeePeriod_Status` | `Status IN ('Open','Calculated','Verified','Approved','Closed')` |
| Check `CK_MdfFeePeriod_Range` | `PeriodEnd >= PeriodStart` |
| Check `CK_MdfFeePeriod_Maker` | `VerifiedBy IS NULL OR ApprovedBy IS NULL OR VerifiedBy <> ApprovedBy` |

---

## 5. `MdfServiceFee`

| Kolom | Tipe | Null | Bawaan | Keterangan |
|---|---|:---:|---|---|
| `FeeNumber` | `varchar(50)` | Tidak | — | Nomor hasil jasa, unik |
| `PeriodId` | `uuid` | Tidak | — | FK `MdfFeePeriod.Id`, `Restrict` |
| `PayeeType` | `varchar(30)` | Tidak | — | `Doctor` atau `Workforce` |
| `PayeeReferenceId` | `uuid` | Tidak | — | Tanpa FK — polimorfik |
| `GrossAmount` | `decimal(18,2)` | Tidak | `0` | `Σ CalculatedAmount` rincian |
| `AdjustmentAmount` | `decimal(18,2)` | Tidak | `0` | Boleh negatif |
| `FinalAmount` | `decimal(18,2)` | Tidak | `0` | `GrossAmount + AdjustmentAmount` |
| `Status` | `varchar(30)` | Tidak | `Calculated` | `Calculated`, `Verified`, `Approved`, `HandedOff` |
| `CalculatedAt` | `timestamptz` | Tidak | — | — |
| `VerifiedBy` | `uuid` | Ya | `null` | — |
| `VerifiedAt` | `timestamptz` | Ya | `null` | — |
| `ApprovedBy` | `uuid` | Ya | `null` | — |
| `ApprovedAt` | `timestamptz` | Ya | `null` | — |
| `RowVersion` | `uuid` | Tidak | `gen_random_uuid()` | — |

**Index dan constraint**

| Jenis | Definisi |
|---|---|
| Unique (partial) | `(FeeNumber) WHERE "IsDelete" = false` |
| Unique (partial) | `(PeriodId, PayeeType, PayeeReferenceId) WHERE "IsDelete" = false` |
| Index | `(PayeeType, PayeeReferenceId, Status)` |
| Index | `(PeriodId, Status)` |
| Check `CK_MdfServiceFee_Status` | `Status IN ('Calculated','Verified','Approved','HandedOff')` |
| Check `CK_MdfServiceFee_Total` | `FinalAmount = GrossAmount + AdjustmentAmount` |
| Check `CK_MdfServiceFee_Gross` | `GrossAmount >= 0` |
| Check `CK_MdfServiceFee_Maker` | `VerifiedBy IS NULL OR ApprovedBy IS NULL OR VerifiedBy <> ApprovedBy` |

---

## 6. `MdfServiceFeeDetail`

| Kolom | Tipe | Null | Bawaan | Keterangan |
|---|---|:---:|---|---|
| `ServiceFeeId` | `uuid` | Tidak | — | FK `MdfServiceFee.Id`, `Cascade` |
| `SourceDomain` | `varchar(30)` | Tidak | — | `OPERATING_ROOM`, `PROCEDURE`, `LABORATORY`, `ADHOC`, `ADHOC_CATALOG` |
| `SourceDetailId` | `varchar(100)` | Tidak | — | Rujukan baris layanan di modul sumber |
| `InvoiceItemId` | `uuid` | Ya | `null` | Baris tagihan asal. Tanpa FK |
| `TariffId` | `uuid` | Ya | `null` | Layanan yang dihitung |
| `RoleId` | `uuid` | Tidak | — | FK `MstMedicalFeeRole.Id`, `Restrict` |
| `BaseAmount` | `decimal(18,2)` | Tidak | — | Nilai kotor baris tagihan (`MDF-DES-012`) |
| `SharingPercentage` | `decimal(5,2)` | Tidak | — | **Snapshot** (`MDF-DES-010`) |
| `CalculatedAmount` | `decimal(18,2)` | Tidak | — | `BaseAmount × SharingPercentage ÷ 100` |
| `SharingRuleId` | `uuid` | Tidak | — | FK `MdfSharingRule.Id`, `Restrict`. **Wajib** |
| `ServiceDate` | `date` | Tidak | — | Tanggal layanan, penentu baris tarif mana yang berlaku |

**Index dan constraint**

| Jenis | Definisi |
|---|---|
| Index | `(ServiceFeeId)` |
| Index | `(SourceDomain, SourceDetailId)` |
| Index | `(InvoiceItemId)` |
| Check `CK_MdfServiceFeeDetail_Percentage` | `SharingPercentage >= 0 AND SharingPercentage <= 100` |
| Check `CK_MdfServiceFeeDetail_Base` | `BaseAmount >= 0` |
| Check `CK_MdfServiceFeeDetail_Source` | `SourceDomain IN ('OPERATING_ROOM','PROCEDURE','LABORATORY','ADHOC','ADHOC_CATALOG')` |

`RADIOLOGY` **sengaja tidak** masuk daftar nilai `SourceDomain`. Menambahkannya kelak adalah
perubahan check constraint yang disengaja, bukan kelalaian (`MF-DEC-015`).

---

## 7. `MdfServiceFeeAdjustment`

| Kolom | Tipe | Null | Bawaan | Keterangan |
|---|---|:---:|---|---|
| `AdjustmentNumber` | `varchar(50)` | Tidak | — | Unik |
| `ServiceFeeId` | `uuid` | Tidak | — | FK `MdfServiceFee.Id`, `Restrict` |
| `Direction` | `varchar(20)` | Tidak | — | `Addition` atau `Deduction` |
| `Amount` | `decimal(18,2)` | Tidak | — | Selalu positif; tandanya dari `Direction` |
| `Reason` | `varchar(500)` | Tidak | — | **Wajib** — koreksi tanpa alasan tidak dapat diaudit |
| `Status` | `varchar(30)` | Tidak | `Pending` | `Pending`, `Approved`, `Rejected` |
| `RequestedBy` | `uuid` | Tidak | — | — |
| `RequestedAt` | `timestamptz` | Tidak | — | — |
| `ApprovedBy` | `uuid` | Ya | `null` | — |
| `ApprovedAt` | `timestamptz` | Ya | `null` | — |
| `RejectionReason` | `varchar(500)` | Ya | `null` | Wajib bila `Status = 'Rejected'` — ditegakkan service |
| `RowVersion` | `uuid` | Tidak | `gen_random_uuid()` | — |

**Index dan constraint**

| Jenis | Definisi |
|---|---|
| Unique (partial) | `(AdjustmentNumber) WHERE "IsDelete" = false` |
| Index | `(ServiceFeeId, Status)` |
| Check `CK_MdfServiceFeeAdjustment_Status` | `Status IN ('Pending','Approved','Rejected')` |
| Check `CK_MdfServiceFeeAdjustment_Direction` | `Direction IN ('Addition','Deduction')` |
| Check `CK_MdfServiceFeeAdjustment_Amount` | `Amount > 0` |
| Check `CK_MdfServiceFeeAdjustment_Maker` | `ApprovedBy IS NULL OR ApprovedBy <> RequestedBy` |

Check terakhir adalah wujud database dari maker-checker (`MDF-DES-017`, `MF-DEC-009`).

---

## 8. `MdfUnresolvedService`

| Kolom | Tipe | Null | Bawaan | Keterangan |
|---|---|:---:|---|---|
| `PeriodId` | `uuid` | Tidak | — | FK `MdfFeePeriod.Id`, `Cascade` |
| `SourceDomain` | `varchar(30)` | Tidak | — | Sama dengan daftar pada rincian |
| `SourceDetailId` | `varchar(100)` | Tidak | — | — |
| `InvoiceItemId` | `uuid` | Ya | `null` | — |
| `TariffId` | `uuid` | Ya | `null` | — |
| `ServiceDate` | `date` | Tidak | — | — |
| `BaseAmount` | `decimal(18,2)` | Ya | `null` | Nilai yang seharusnya jadi basis, bila sudah diketahui |
| `Reason` | `varchar(30)` | Tidak | — | `PERFORMER_MISSING`, `AGREEMENT_MISSING`, `RULE_MISSING`, `CONTRACT_EXPIRED`, `ROLE_UNMAPPED`, `SOURCE_UNSUPPORTED` |
| `ReasonDetail` | `varchar(500)` | Ya | `null` | Penjelasan yang dapat dibaca petugas |
| `DetectedAt` | `timestamptz` | Tidak | — | — |
| `Status` | `varchar(30)` | Tidak | `Open` | `Open`, `Resolved`, `Waived` |
| `ResolvedBy` | `uuid` | Ya | `null` | — |
| `ResolvedAt` | `timestamptz` | Ya | `null` | — |
| `ResolutionNote` | `varchar(500)` | Ya | `null` | Wajib bila `Status = 'Waived'` — ditegakkan service |

**Index dan constraint**

| Jenis | Definisi |
|---|---|
| Unique (partial) | `(PeriodId, SourceDomain, SourceDetailId) WHERE "IsDelete" = false` |
| Index | `(PeriodId, Status)` |
| Index | `(Reason)` |
| Check `CK_MdfUnresolvedService_Status` | `Status IN ('Open','Resolved','Waived')` |
| Check `CK_MdfUnresolvedService_Reason` | `Reason IN ('PERFORMER_MISSING','AGREEMENT_MISSING','RULE_MISSING','CONTRACT_EXPIRED','ROLE_UNMAPPED','SOURCE_UNSUPPORTED')` |

Unique index-nya mencegah satu layanan muncul dua kali di daftar periode yang sama saat
perhitungan diulang.

---

## 9. `MdfFinanceHandoff`

| Kolom | Tipe | Null | Bawaan | Keterangan |
|---|---|:---:|---|---|
| `ServiceFeeId` | `uuid` | Tidak | — | FK `MdfServiceFee.Id`, `Restrict`. Unik |
| `HandoffKey` | `uuid` | Tidak | — | Kunci idempoten untuk Finance. Unik |
| `PayeeType` | `varchar(30)` | Tidak | — | Disalin dari hasil jasa |
| `PayeeReferenceId` | `uuid` | Tidak | — | Disalin |
| `PeriodCode` | `varchar(20)` | Tidak | — | Disalin — Finance tidak perlu join ke periode |
| `GrossAmount` | `decimal(18,2)` | Tidak | — | `MdfServiceFee.FinalAmount`, sebelum potongan (`MF-DEC-005`) |
| `Status` | `varchar(30)` | Tidak | `Created` | `Created`, `Acknowledged`, `Failed` |
| `CorrelationId` | `uuid` | Tidak | — | Penelusuran lintas modul |
| `CausationId` | `uuid` | Ya | `null` | — |
| `AcknowledgedAt` | `timestamptz` | Ya | `null` | Diisi Finance |
| `FailureReason` | `varchar(500)` | Ya | `null` | — |
| `RowVersion` | `uuid` | Tidak | `gen_random_uuid()` | — |

**Index dan constraint**

| Jenis | Definisi |
|---|---|
| Unique (partial) | `(ServiceFeeId) WHERE "IsDelete" = false` |
| Unique (partial) | `(HandoffKey) WHERE "IsDelete" = false` |
| Index | `(Status, CreateDateTime)` |
| Check `CK_MdfFinanceHandoff_Status` | `Status IN ('Created','Acknowledged','Failed')` |
| Check `CK_MdfFinanceHandoff_Amount` | `GrossAmount >= 0` |

Empat kolom disalin dari hasil jasa dengan sengaja: Finance harus dapat membaca satu baris
handoff tanpa menyentuh tabel Medical Fee mana pun.

---

## 10. Ringkasan penamaan

| Aturan | Penerapan |
|---|---|
| Prefix data induk | `Mst` — hanya `MstMedicalFeeRole` |
| Prefix entity transaksi | `Mdf` — delapan tabel lainnya |
| Nama check constraint | `CK_<NamaTabel>_<Aspek>` |
| Partial unique index | Selalu `WHERE "IsDelete" = false` |
| Kolom uang | `decimal(18,2)` lewat `HasPrecision(18, 2)` |
| Kolom persentase | `decimal(5,2)` lewat `HasPrecision(5, 2)` |
| Kolom status | `varchar(30)` + `static class ...Statuses` + check constraint (`MDF-DES-004`) |
| Concurrency | `Guid RowVersion` pada aggregate root saja (`MDF-DES-005`) |

`MdfSharingRule`, `MdfServiceFeeDetail`, dan `MdfUnresolvedService` sengaja **tanpa**
`RowVersion`: ketiganya baris anak yang tidak pernah diubah sendirian — selalu lewat aggregate
root-nya atau lewat perhitungan ulang seluruh periode.
