# Kamus Data — CRM Membership

| Field | Nilai |
|---|---|
| Revision | `1`, status `draft` |
| Backend SHA | `1aecf642` |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu tidak diulang pada tabel di bawah.
Penghapusan bersifat penandaan melalui `IsDelete`. Pada membership, pembatalan bisnis juga
mengisi `IsCancel`, `CancelDateTime`, dan `CancelBy`.

Semua tanggal bertipe `date` adalah **tanggal kalender WIB** (`CRM-GAP-008`). Kolom
`timestamptz` menyimpan waktu absolut.

## `MstPatientMembership` — Diperbarui

Sumber model: `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientMembership.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PatientId` | `Guid` | Ya | — | Index + 3 unik bersyarat | FK `MstPatient` | `Restrict` | Tidak | Pemilik membership |
| `MembershipTierId` | `Guid` | Ya | — | Index | FK `MstMembershipTier` | `Restrict` | Tidak | Tingkat |
| `MemberNumber` | `string(50)` | Ya | — | Unik | — | — | Tidak | Dibuat `NumberSeriesAllocator`, tidak diketik petugas (`CRM-DEC-023`) |
| `MembershipStatus` | `MembershipStatus` (int) | Ya | **`PendingPayment` (7)** | Index gabungan | — | — | Tidak | Lihat `contracts/state-transition-matrix.md` |
| `RegistrationType` | `MembershipRegistrationType` (int) | Ya | `New` (1) | — | — | — | Tidak | **Baru.** Baru, Perpanjangan, Upgrade, Impor lama |
| `JoinDate` | `date` | Ya | tanggal WIB saat simpan | — | — | — | Tidak | Makna dipertegas: tanggal pendaftaran disimpan |
| `PeriodStartDate` | `date?` | Tidak | — | Index `(MembershipStatus, PeriodStartDate)` | — | — | Tidak | **Baru.** Tanggal mulai periode. Terisi saat lunas atau saat verifikasi data lama |
| `ExpiredDate` | `date?` | Tidak | — | Index `(MembershipStatus, ExpiredDate)` | — | — | Tidak | Makna dipertegas: tanggal akhir periode, inklusif. Contoh: mulai 13 Okt 2026, 12 bulan, berakhir 12 Okt 2027 |
| `PaymentDueAt` | `DateTimeOffset?` | Tidak | — | Index `(MembershipStatus, PaymentDueAt)` | — | — | Tidak | **Baru.** Waktu simpan + `PaymentDueDays` (7). Contoh: 13 Okt 2026 09.00 WIB menjadi 20 Okt 2026 09.00 WIB |
| `ActivatedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | **Baru.** Waktu status pertama kali Aktif |
| `FeeAmountSnapshot` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | **Baru.** Salinan `AnnualFeeAmount` saat simpan |
| `PreviousPatientMembershipId` | `Guid?` | Tidak | — | Index | FK `MstPatientMembership` (self) | `Restrict` | Tidak | **Baru.** Membership yang diperpanjang atau di-upgrade |
| `CancellationCategory` | `MembershipCancellationCategory?` (int) | Tidak | — | — | — | — | Tidak | **Baru.** Terisi saat Cancelled |
| `CancellationReason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | **Baru.** Teks bebas petugas, bisa memuat cerita pasien |
| `FeeChargeReferenceId` | `Guid?` | Tidak | — | Unik bersyarat (bukan null) | Belum ada FK | — | Tidak | **Baru.** Rujukan tagihan iuran di billing. Bentuk FK menunggu `DEC-CRM-001` |
| `IsPrimary` | `bool` | Ya | `true` | — | — | — | Tidak | Proyeksi kompatibilitas: `true` hanya bila Aktif |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Proyeksi kompatibilitas: `true` hanya bila Aktif |
| `IsAutoCreated`, `IsCreatedFromKiosk`, `IsCreatedFromAdmission` | `bool` | Ya | `false` | — | — | — | Tidak | Warisan. Selalu `false` untuk data baru |
| `IsCreatedByMarketing` | `bool` | Ya | `false` | — | — | — | Tidak | Warisan. Diisi `true` oleh service untuk pendaftaran CRM |
| `PointBalance` | `int` | Ya | `0` | — | — | — | Tidak | Warisan, tidak dipakai aturan |
| `TotalSpendAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Warisan, tidak dipakai aturan |
| `LastUpgradeDate`, `LastDowngradeDate` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Warisan, tidak dipakai (riwayat ada di `PatMembershipStatusHistory`) |
| `UpgradeDowngradeReason` | `string(250)?` | Tidak | — | — | — | — | Tidak | Warisan, tidak dipakai |
| `Notes` | `string(250)?` | Tidak | — | — | — | — | **Ya** | Catatan petugas. Satu-satunya kolom yang boleh diubah setelah Aktif |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | **Baru.** Pencegah tabrakan dua petugas. Diganti setiap perubahan |

## `PatMembershipStatusHistory` — Baru

Sumber model (rencana): `Areas/HealthServices/PatientManagement/Membership/Models/PatMembershipStatusHistory.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PatientMembershipId` | `Guid` | Ya | — | Index `(PatientMembershipId, OccurredAt)` | FK `MstPatientMembership` | `Restrict` | Tidak | Membership yang berubah |
| `FromStatus` | `MembershipStatus?` (int) | Tidak | — | — | — | — | Tidak | Kosong pada baris pertama |
| `ToStatus` | `MembershipStatus` (int) | Ya | — | — | — | — | Tidak | Sama dengan `FromStatus` untuk perubahan catatan |
| `ActionCode` | `string(50)` | Ya | — | — | — | — | Tidak | Contoh: `REGISTERED`, `FEE_PAID`, `EXPIRED`, `SCHEDULE_STARTED`, `UNPAID_CANCELLED`, `CANCELLED`, `UPGRADED`, `LEGACY_FLAGGED`, `LEGACY_ACCEPTED`, `LEGACY_REJECTED`, `NOTES_UPDATED` |
| `Source` | `MembershipChangeSource` (int) | Ya | — | — | — | — | Tidak | `Staff`, `System`, `Billing` |
| `ActorUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Kosong bila sumbernya sistem |
| `OccurredAt` | `DateTimeOffset` | Ya | waktu server | (lihat atas) | — | — | Tidak | Waktu kejadian |
| `Reason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Alasan petugas |
| `ReferenceId` | `Guid?` | Tidak | — | — | — | — | Tidak | Rujukan tagihan atau refund |
| `OldNotes` | `string(250)?` | Tidak | — | — | — | — | **Ya** | Untuk `NOTES_UPDATED` |
| `NewNotes` | `string(250)?` | Tidak | — | — | — | — | **Ya** | Untuk `NOTES_UPDATED` |

## `MstMembershipTier` — Diperbarui

Sumber model: `Areas/Administrator/MasterData/Models/MstMembershipTier.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `TierCode` | `string` | Ya | — | Unik | — | — | Tidak | Kode tingkat |
| `TierName` | `string` | Ya | — | — | — | — | Tidak | Nama tampil |
| `TierType` | `MembershipTierType` (int) | Ya | `Regular` | — | — | — | Tidak | Hanya Silver(2)/Gold(3)/Platinum(4)/Executive(5) yang boleh didaftarkan |
| `CardTitle`, `CardColor`, `CardImagePath` | `string?` | Tidak | — | — | — | — | Tidak | Kartu, ditunda (`CRM-OQ-004`) |
| `PriorityLevel` | `int` | Ya | `0` | — | — | — | Tidak | Urutan untuk upgrade (`AG-04`) |
| `IsDefault`, `IsSelectableInKiosk`, `IsSelectableInAdmission`, `IsManagedByMarketingOnly` | `bool` | Ya | — | — | — | — | Tidak | Warisan, tidak dipakai aturan baru |
| `RegistrationDiscountPercent` … `PharmacyDiscountPercent` (6 kolom) | `decimal` | Ya | `0` | — | — | — | Tidak | Persen diskon per kelompok (`CRM-DEC-027`). Contoh: konsultasi 10 berarti 10% |
| `PriorityQueue` | `bool` | Ya | `false` | — | — | — | Tidak | Diisi `true` untuk empat tingkat (`CRM-DEC-015`) |
| `QueueAudience`, `PublicDisplayMode` | int enum | Ya | `Member`, `Default` | — | — | — | Tidak | Milik keputusan rawat jalan `RJ-DOC-DEC-055`/`057` |
| `FreeAnnualCheckup`, `FreeParking` | `bool` | Ya | `false` | — | — | — | Tidak | Warisan, tidak dipakai |
| `ValidityMonths` | `int` | Ya | `12` | — | — | — | Tidak | Panjang periode |
| `MinimumSpendAmount` | `decimal` | Ya | `0` | — | — | — | Tidak | Warisan, tidak dipakai |
| `AnnualFeeAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | **Baru.** Nominal iuran per periode. Harus > 0 agar tingkat bisa didaftarkan |
| `SortOrder`, `BenefitDescription`, `Description`, `IsActive` | — | — | — | — | — | — | Tidak | Warisan |

## Tabel Sudah Ada (kolom kunci saja)

| Tabel | Kolom yang dipakai modul ini | Sumber |
|---|---|---|
| `MstPatient` | `Id` (PK), `MedicalRecordNumber`, `IdentityNumber` (**Sensitif**), `PhoneNumber` (**Sensitif**), `IsMember`, `DefaultMembershipTierId`, `ActivePatientMembershipId`, `IsActive` | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs` |
| `RegQueue` | `PatientMembershipIdSnapshot`, `MembershipTierIdSnapshot`, `MembershipTierCodeSnapshot` | `Areas/HealthServices/RegistrationManagement/Models/RegQueue.cs:114-119` |
| `RegPatientEncounter` | `Id` (PK). Usulan `AG-01`: `PatientMembershipIdSnapshot uuid null`, `MembershipTierIdSnapshot uuid null` | `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounter.cs` |

## Skema DDL

> **Peringatan:** basis data project ini dibentuk EF Core Migrations. DDL di bawah adalah
> **dokumentasi bentuk tabel**, bukan skrip untuk dijalankan. Menjalankannya akan berbenturan
> dengan migration. Kolom audit `IdentityModel` tidak ditulis ulang.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
-- MstPatientMembership: hanya kolom baru dan index yang berubah ditampilkan lengkap.
ALTER TABLE public."MstPatientMembership"
    ADD "RegistrationType"            integer       NOT NULL DEFAULT 1,  -- enum, HasConversion<int>
    ADD "PeriodStartDate"             date,
    ADD "PaymentDueAt"                timestamp with time zone,
    ADD "ActivatedAt"                 timestamp with time zone,
    ADD "FeeAmountSnapshot"           numeric(18,2) NOT NULL DEFAULT 0,
    ADD "PreviousPatientMembershipId" uuid,
    ADD "CancellationCategory"        integer,                           -- enum
    ADD "CancellationReason"          varchar(500),                      -- SENSITIF
    ADD "FeeChargeReferenceId"        uuid,
    ADD "RowVersion"                  uuid          NOT NULL;
ALTER TABLE public."MstPatientMembership"
    ALTER COLUMN "MembershipStatus" SET DEFAULT 7;                       -- PendingPayment
ALTER TABLE public."MstPatientMembership"
    ADD CONSTRAINT "FK_MstPatientMembership_MstPatientMembership_PreviousPatientMembershipId"
        FOREIGN KEY ("PreviousPatientMembershipId")
        REFERENCES public."MstPatientMembership" ("Id") ON DELETE RESTRICT;

DROP INDEX "IX_MstPatientMembership_PatientId_IsPrimary";
CREATE UNIQUE INDEX "UX_MstPatientMembership_Patient_Active"
    ON public."MstPatientMembership" ("PatientId") WHERE "MembershipStatus" = 1 AND "IsDelete" = false;
CREATE UNIQUE INDEX "UX_MstPatientMembership_Patient_Pending"
    ON public."MstPatientMembership" ("PatientId") WHERE "MembershipStatus" = 7 AND "IsDelete" = false;
CREATE UNIQUE INDEX "UX_MstPatientMembership_Patient_Scheduled"
    ON public."MstPatientMembership" ("PatientId") WHERE "MembershipStatus" = 8 AND "IsDelete" = false;
CREATE UNIQUE INDEX "UX_MstPatientMembership_FeeChargeReferenceId"
    ON public."MstPatientMembership" ("FeeChargeReferenceId") WHERE "FeeChargeReferenceId" IS NOT NULL;
CREATE INDEX "IX_MstPatientMembership_Status_ExpiredDate"
    ON public."MstPatientMembership" ("MembershipStatus", "ExpiredDate");
CREATE INDEX "IX_MstPatientMembership_Status_PeriodStartDate"
    ON public."MstPatientMembership" ("MembershipStatus", "PeriodStartDate");
CREATE INDEX "IX_MstPatientMembership_Status_PaymentDueAt"
    ON public."MstPatientMembership" ("MembershipStatus", "PaymentDueAt");
CREATE INDEX "IX_MstPatientMembership_PreviousPatientMembershipId"
    ON public."MstPatientMembership" ("PreviousPatientMembershipId");

CREATE TABLE public."PatMembershipStatusHistory" (
    "Id"                   uuid         NOT NULL,
    "PatientMembershipId"  uuid         NOT NULL,
    "FromStatus"           integer,                       -- enum
    "ToStatus"             integer      NOT NULL,         -- enum
    "ActionCode"           varchar(50)  NOT NULL,
    "Source"               integer      NOT NULL,         -- enum
    "ActorUserId"          uuid,
    "OccurredAt"           timestamp with time zone NOT NULL,
    "Reason"               varchar(500),                  -- SENSITIF
    "ReferenceId"          uuid,
    "OldNotes"             varchar(250),                  -- SENSITIF
    "NewNotes"             varchar(250),                  -- SENSITIF
    -- kolom audit IdentityModel tidak ditulis ulang di sini
    CONSTRAINT "PK_PatMembershipStatusHistory" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PatMembershipStatusHistory_MstPatientMembership_PatientMembershipId"
        FOREIGN KEY ("PatientMembershipId")
        REFERENCES public."MstPatientMembership" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_PatMembershipStatusHistory_PatientMembershipId_OccurredAt"
    ON public."PatMembershipStatusHistory" ("PatientMembershipId", "OccurredAt");

ALTER TABLE public."MstMembershipTier"
    ADD "AnnualFeeAmount" numeric(18,2) NOT NULL DEFAULT 0;
```
