# Kamus Data — Sub-modul Payroll Gaji

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1`, status `approved` (Yasmin, 2026-11-12) |
| `last_changed_in` | `PGJ-C1` |
| `input_revision` | `02-backend-architecture.md` r1 |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu tidak diulang pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris.

Kolom bertanda **Sensitif = Ya** tidak boleh masuk custom logger dan tidak boleh dipakai sebagai
contoh berisi data asli. Seluruh nominal gaji dan tunjangan sensitif karena membuka penghasilan
pegawai.

## 1. Status dan kepemilikan tabel

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `MstSalaryStructureAllowance` | **Baru** | Payroll (`payroll-gaji`) | Prefix `Mst` dari registry; berkas di folder master HR `PayrollAndBenefit` |
| `PrlPayrollVariableInput` | **Diperbarui** | Payroll | Tambah lima kolom keputusan |
| `MstSalaryStructure`, `MstSalaryGrade`, `MstAllowanceType`, `MstPayrollComponent` | Sudah ada | Payroll (berkas di folder HR) | Dibaca |
| `PrlPayrollRun`, `PrlPayrollRunEmployee`, `PrlPayrollEmployeeComponent`, `PrlPayrollOvertimeInput`, `PrlPayroll` | Sudah ada | Payroll | Dibaca dan ditulis mesin hitung seperti sekarang |
| `PrlTransportAllowancePolicy`, `PrlTransportAllowance` | Sudah ada | Payroll | Dibaca mesin hitung |
| `WfpSalaryAssignment` | Sudah ada | HRD | Direferensikan, **tidak** disalin |
| `MstCostCenter` | Sudah ada | Human Resource | Direferensikan lewat snapshot run |

## 2. Tabel baru dan diperbarui

### 2.1 `MstSalaryStructureAllowance` — Baru

Model: `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Models/MstSalaryStructureAllowance.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `SalaryStructureId` | `Guid` | Ya | — | Index; bagian index unik | FK ke `MstSalaryStructure` | `Restrict` | Tidak | Struktur pemilik baris |
| `AllowanceTypeId` | `Guid` | Ya | — | Index; bagian index unik | FK ke `MstAllowanceType` | `Restrict` | Tidak | Jenis tunjangan |
| `CalculationMethod` | `string(30)` | Ya | `FixedAmount` | — | — | — | Tidak | `FixedAmount` atau `PercentageOfBaseSalary` |
| `Amount` | `decimal(18,2)` | Ya | `0` | — | — | — | **Ya** | Nominal sebulan bila `FixedAmount`; wajib > 0 untuk metode itu |
| `Percentage` | `decimal(9,4)` | Ya | `0` | — | — | — | **Ya** | Persen gaji pokok bila `PercentageOfBaseSalary`; > 0 dan ≤ 100 |
| `Notes` | `string(500)` | Tidak | — | — | — | — | Tidak | Keterangan petugas |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Baris nonaktif tidak dihitung |

Index unik: (`SalaryStructureId`, `AllowanceTypeId`) dengan filter `"IsDelete" = false`.

**Contoh baris:** struktur "GR-05 Perawat Pelaksana", jenis "Tunjangan Keahlian", metode
`PercentageOfBaseSalary`, `Percentage` 5, `Amount` 0.

### 2.2 `PrlPayrollVariableInput` — Diperbarui

Model: `Areas/Corporate/PayrollManagement/Models/PrlPayrollVariableInput.cs`. Panjang kolom,
presisi desimal, dan perilaku hapus yang tidak disebut di bawah mengikuti
`PrlPayrollVariableInputConfiguration` yang sudah ada dan tidak berubah.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PayrollRunEmployeeId` | `Guid` | Ya | — | Index | FK ke `PrlPayrollRunEmployee` | Sudah ada | Tidak | Pegawai di dalam run |
| `PayrollComponentId` | `Guid` | Ya | — | Index | FK ke `MstPayrollComponent` | Sudah ada | Tidak | Jenis penghasilan |
| `InputNumber` | `string(50)` | Ya | — | — | — | — | Tidak | Nomor dari seri `PVI` untuk input manual |
| `InputDate` | `DateOnly` | Ya | — | — | — | — | Tidak | Tanggal input |
| `InputType` | `string(30)` | Ya | `Manual` | — | — | — | Tidak | Input manual baru: `Incentive`, `Bonus`, `AdditionalDutyHonorarium` |
| `InputStatus` | `string(30)` | Ya | `Draft` | — | — | — | Tidak | Lihat `contracts/state-transition-matrix.md` |
| `CurrencyCode` | `string(3)` | Ya | `IDR` | — | — | — | Tidak | — |
| `Quantity` | `decimal` | Ya | `1` | — | — | — | Tidak | Input manual memakai 1 |
| `Rate` | `decimal` | Ya | `0` | — | — | — | **Ya** | Input manual memakai nilai sama dengan `Amount` |
| `Amount` | `decimal` | Ya | `0` | — | — | — | **Ya** | Nominal |
| `SourceType` | `string(50)` | Tidak | — | Sudah ada | — | — | Tidak | Input manual: `PayrollManualInput` |
| `SourceId` | `Guid` | Tidak | — | Sudah ada | — | — | Tidak | Kosong untuk input manual |
| `AttachmentPath` | `string(500)` | Tidak | — | — | — | — | Tidak | Lampiran bukti, opsional |
| `SubmittedAt` | `DateTime` | Tidak | — | — | — | — | Tidak | Waktu diajukan |
| `SubmittedByUserId` | `Guid` | Tidak | — | — | FK user | Sudah ada | Tidak | Pengaju |
| `VerifiedAt` | `DateTime` | Tidak | — | — | — | — | Tidak | Tidak dipakai input manual |
| `VerifiedByUserId` | `Guid` | Tidak | — | — | FK user | Sudah ada | Tidak | Tidak dipakai input manual |
| `ApprovedAt` | `DateTime` | Tidak | — | — | — | — | Tidak | **Baru.** Waktu disetujui |
| `ApprovedByUserId` | `Guid` | Tidak | — | — | FK user | `SetNull` | Tidak | **Baru.** Penyetuju |
| `RejectedAt` | `DateTime` | Tidak | — | — | — | — | Tidak | **Baru.** Waktu ditolak |
| `RejectedByUserId` | `Guid` | Tidak | — | — | FK user | `SetNull` | Tidak | **Baru.** Penolak |
| `DecisionReason` | `string(500)` | Tidak | — | — | — | — | Tidak | **Baru.** Alasan penolakan atau pembatalan |
| `Notes` | `string` | Tidak | — | — | — | — | Tidak | Wajib untuk `AdditionalDutyHonorarium`: alasan penugasan |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

## 3. Tabel yang sudah ada — kolom kunci

| Tabel | Kolom yang dipakai sub-modul ini | Sumber lengkap |
|---|---|---|
| `MstSalaryStructure` | `Id`, `SalaryGradeId`, `DefaultBaseSalary`, `MinimumBaseSalary`, `MaximumBaseSalary`, `IsActive` | `.../PayrollAndBenefit/Models/MstSalaryStructure.cs` |
| `MstSalaryGrade` | `Id`, `MinimumSalary`, `MaximumSalary`, `IsActive` | `.../PayrollAndBenefit/Models/MstSalaryGrade.cs` |
| `MstAllowanceType` | `Id`, `AllowanceTypeName`, `PayrollComponentId`, `MaximumAmount`, `IsTaxable`, `IsProrated`, `IsActive` | `.../PayrollAndBenefit/Models/MstAllowanceType.cs` |
| `MstPayrollComponent` | `Id`, `PayrollComponentCode`, `ComponentType`, `IsTaxable`, `IsActive` | `.../PayrollAndBenefit/Models/MstPayrollComponent.cs` |
| `PrlPayrollRun` | `Id`, `PayrollPeriodId`, `RunType`, `RunStatus`, `CalculatedAt`, `SubmittedAt`, `ApprovedAt` | `Areas/Corporate/PayrollManagement/Models/PrlPayrollRun.cs` |
| `PrlPayrollRunEmployee` | `Id`, `PayrollRunId`, `WorkforceProfileId`, `SalaryStructureId`, `CostCenterId`, `CostCenterCodeSnapshot`, `CostCenterNameSnapshot` | `.../Models/PrlPayrollRunEmployee.cs` |
| `PrlPayrollEmployeeComponent` | `PayrollRunEmployeeId`, `PayrollComponentId`, `SourceType`, `SourceId`, `ComponentCodeSnapshot`, `ComponentTypeSnapshot`, `Amount`, `IsTaxable` | `.../Models/PrlPayrollEmployeeComponent.cs` |
| `PrlPayrollOvertimeInput` | `PayrollRunEmployeeId`, `OvertimeDate`, `VerifiedMinutes`, `RateMultiplier`, `HourlyRate`, `OvertimeAmount` | `.../Models/PrlPayrollOvertimeInput.cs` |
| `PrlTransportAllowancePolicy` | `Id`, `CalculationMethod`, `FixedMonthlyAmount`, `MonthlyLimitAmount`, `IsProrated`, `IsTaxable`, `IsIncludedInPayroll`, `PayrollComponentId` | `.../Models/PrlTransportAllowancePolicy.cs` |
| `PrlTransportAllowance` | `Id`, `WorkforceProfileId`, `TransportAllowancePolicyId`, `PayrollComponentId`, `AllowanceStatus`, `MonthlyAmount`, `EffectiveStartDate`, `EffectiveEndDate` | `.../Models/PrlTransportAllowance.cs` |
| `PrlPayroll` | `WorkforceProfileId`, `TotalAllowance`, `TaxAmount`, `InsuranceAmount`, `CostCenterId` | `.../Models/PrlPayroll.cs` |

Nilai baru pada kolom teks yang sudah ada (tanpa perubahan skema):

| Tabel.kolom | Nilai baru |
|---|---|
| `PrlPayrollEmployeeComponent.ComponentCodeSnapshot` | `PAY_STRUCTURE_ALLOWANCE`, `PAY_TRANSPORT_ALLOWANCE` |
| `PrlPayrollEmployeeComponent.SourceType` | `SalaryStructureAllowance`, `TransportAllowance` |
| `PrlPayrollVariableInput.SourceType` | `PayrollManualInput` |
| `PrlPayrollVariableInput.InputType` | `Incentive`, `Bonus`, `AdditionalDutyHonorarium` |
| `PrlPayrollRun.RunType` | `Simulation` — hanya di dalam transaksi simulasi yang selalu dibatalkan; tidak pernah tersimpan |

## 4. Skema DDL

> **Peringatan.** Basis data project ini dibentuk EF Core Migrations. DDL di bawah adalah
> **dokumentasi bentuk tabel**, bukan skrip untuk dijalankan. Menjalankannya akan berbenturan
> dengan migration. Kolom audit `IdentityModel` tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."MstSalaryStructureAllowance" (
    "Id"                 uuid           NOT NULL,
    "SalaryStructureId"  uuid           NOT NULL,
    "AllowanceTypeId"    uuid           NOT NULL,
    "CalculationMethod"  varchar(30)    NOT NULL,
    "Amount"             numeric(18,2)  NOT NULL DEFAULT 0,  -- SENSITIF
    "Percentage"         numeric(9,4)   NOT NULL DEFAULT 0,  -- SENSITIF
    "Notes"              varchar(500),
    "IsActive"           boolean        NOT NULL DEFAULT true,
    CONSTRAINT "PK_MstSalaryStructureAllowance" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MstSalaryStructureAllowance_MstSalaryStructure_SalaryStructureId"
        FOREIGN KEY ("SalaryStructureId") REFERENCES public."MstSalaryStructure" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MstSalaryStructureAllowance_MstAllowanceType_AllowanceTypeId"
        FOREIGN KEY ("AllowanceTypeId") REFERENCES public."MstAllowanceType" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_MstSalaryStructureAllowance_SalaryStructureId_AllowanceTypeId"
    ON public."MstSalaryStructureAllowance" ("SalaryStructureId", "AllowanceTypeId")
    WHERE "IsDelete" = false;

CREATE INDEX "IX_MstSalaryStructureAllowance_AllowanceTypeId"
    ON public."MstSalaryStructureAllowance" ("AllowanceTypeId");

-- PrlPayrollVariableInput: hanya kolom baru.
ALTER TABLE public."PrlPayrollVariableInput"
    ADD COLUMN "ApprovedAt"        timestamp with time zone,
    ADD COLUMN "ApprovedByUserId"  uuid,
    ADD COLUMN "RejectedAt"        timestamp with time zone,
    ADD COLUMN "RejectedByUserId"  uuid,
    ADD COLUMN "DecisionReason"    varchar(500);
-- FK ApprovedByUserId dan RejectedByUserId ke tabel user Identity, ON DELETE SET NULL.
```
