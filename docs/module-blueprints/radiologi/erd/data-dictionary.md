# Kamus Data — Modul Radiologi

| Field | Value |
|---|---|
| Contract version | `RAD-ERD-DICT-001` |
| Revision | `2` |
| Status | `draft` |
| Backend SHA | `64da911` |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu **tidak diulang** pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris. Desain apa pun
**tidak boleh** mengandalkan baris benar-benar hilang dari tabel.

**Kedalaman dokumentasi mengikuti status tabel.** Tabel `Baru` dan `Diperbarui` ditulis seluruh
kolomnya. Tabel `Sudah ada` cukup kolom kuncinya, ditambah rujukan ke berkas model sebagai
sumber lengkap.

---

## 1. `RadReport` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `RadStudyId` | `Guid` | Ya | — | **Unik**, difilter | FK ke `RadStudy` | `Restrict` | Tidak | Study yang dibaca. Satu study paling banyak satu bacaan |
| `RadOrderId` | `Guid` | Ya | — | Index | FK ke `RadOrder` | `Restrict` | Tidak | Disalin dari study, untuk pencarian tanpa penggabungan tabel |
| `EncounterId` | `Guid` | Ya | — | Index | — | — | Tidak | Disalin dari study. Pemiliknya tetap Registration Management |
| `ReportNumber` | `string(64)` | Ya | — | **Unik**, difilter | — | — | Tidak | Nomor bacaan yang terbaca manusia |
| `ReportStatus` | `RadReportStatus` | Ya | `Pending` | Index | — | — | Tidak | Disimpan sebagai `int` |
| `CurrentVersionNumber` | `int` | Ya | `0` | — | — | — | Tidak | Nomor versi yang sedang berlaku. `0` berarti belum ada draf |
| `FirstReleasedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Kapan bacaan pertama kali sampai ke dokter pengirim |
| `LastReleasedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Kapan versi terakhir dirilis |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Token konkurensi |

## 2. `RadReportVersion` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `RadReportId` | `Guid` | Ya | — | **Unik** bersama `VersionNumber` | FK ke `RadReport` | `Restrict` | Tidak | Bacaan induknya |
| `VersionNumber` | `int` | Ya | — | **Unik** bersama `RadReportId` | — | — | Tidak | Mulai dari `1` |
| `PreviousVersionId` | `Guid?` | Tidak | — | Index | FK ke `RadReportVersion` | `Restrict` | Tidak | Versi yang digantikan. Kosong berarti versi pertama |
| `VersionStatus` | `RadReportVersionStatus` | Ya | `Drafted` | — | — | — | Tidak | Disimpan sebagai `int` |
| `IsAmendment` | `bool` | Ya | `false` | — | — | — | Tidak | `true` untuk versi kedua dan seterusnya |
| `Findings` | `string(8000)` | Tidak | — | — | — | — | **Ya** | Uraian temuan pada citra |
| `Impression` | `string(4000)` | Ya | — | — | — | — | **Ya** | Kesimpulan bacaan. Inilah yang dibaca dokter pengirim |
| `Recommendation` | `string(2000)` | Tidak | — | — | — | — | **Ya** | Saran tindak lanjut |
| `AuthorUserId` | `Guid` | Ya | — | Index | — | — | Tidak | Penulis draf |
| `AuthorRoleSnapshot` | `RadReportAuthorRole` | Ya | — | — | — | — | Tidak | **Peran penulis dibekukan pada saat draf dibuat.** Menentukan boleh atau tidaknya pengesahan sendiri |
| `DraftedAt` | `DateTime` | Ya | — | — | — | — | Tidak | Kapan draf ditulis |
| `ValidatorUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pengesah. Kosong selama masih draf |
| `ValidatedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `ReleasedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Setelah terisi, isi versi ini tidak boleh berubah |
| `AmendmentReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | **Wajib** bila `IsAmendment` bernilai `true` |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Token konkurensi |

## 3. `MstRadModalitySafetyRule` — status `Diperbarui`

Seluruh kolom ditulis karena statusnya `Diperbarui`. Enam kolom terakhir adalah tambahan.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ModalityId` | `Guid` | Ya | — | Unik gabungan; Index | FK ke `MstRadModality` | `Restrict` | Tidak | Alat yang diatur |
| `ProcedureId` | `Guid?` | Tidak | — | Unik gabungan | FK ke `MstProcedure` | `Restrict` | Tidak | Kosong berarti berlaku untuk semua pemeriksaan alat itu |
| `SafetyRequirementId` | `Guid` | Ya | — | Unik gabungan | FK ke `MstRadSafetyRequirement` | `Restrict` | Tidak | Butir yang diatur |
| `IsMandatory` | `bool` | Ya | `true` | — | — | — | Tidak | Butir wajib memblokir pemeriksaan; butir tidak wajib tidak |
| `EffectiveFrom` | `DateTime` | Ya | — | — | — | — | Tidak | Mulai berlaku |
| `EffectiveTo` | `DateTime?` | Tidak | — | — | — | — | Tidak | Batas berlaku |
| `RuleVersion` | `int` | Ya | `1` | — | — | — | Tidak | Naik satu setiap pengesahan. Dibekukan pada study yang lolos |
| `Note` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Keterangan bebas |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | **Dipertahankan untuk kompatibilitas.** Sumber kebenaran baru adalah `RuleStatus` |
| `ApprovedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pengesah |
| `ApprovedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `RuleStatus` | `RadSafetyRuleStatus` | Ya | `Active` saat migration | Unik gabungan | — | — | Tidak | **BARU.** Hanya `Active` yang dinilai gerbang keselamatan |
| `SubmittedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | **BARU.** Pengaju pengesahan |
| `SubmittedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | **BARU** |
| `RejectedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | **BARU** |
| `RejectedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | **BARU** |
| `RejectionReason` | `string(1000)?` | Tidak | — | — | — | — | Tidak | **BARU.** Wajib diisi saat menolak |

## 4. `RadOrder` — status `Diperbarui`

Seluruh kolom ditulis karena statusnya berubah menjadi `Diperbarui`. Tiga kolom terakhir adalah
tambahan dari `RAD-DEC-013`.

Berkas: `Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EncounterId` | `Guid` | Ya | — | Index | FK ke `TrxPatientEncounter` | `Restrict` | Tidak | Kunjungan pasien. Pemiliknya Registration Management |
| `ProcedureId` | `Guid` | Ya | — | — | FK ke `MstProcedure` | `Restrict` | Tidak | Pemeriksaan yang dipesan |
| `InpEpisodeId` | `Guid?` | Tidak | — | — | FK ke perawatan rawat inap | `Restrict` | Tidak | Kosong untuk pasien rawat jalan |
| `ModalityId` | `Guid` | Ya | — | Index gabungan | FK ke `MstRadModality` | `Restrict` | Tidak | Alat yang diminta. Menentukan aturan keselamatan mana yang berlaku |
| `OrderStatus` | `RadOrderStatus` | Ya | `Requested` | Index; index gabungan | — | — | Tidak | Disimpan sebagai `int` |
| `StatusBeforeHold` | `RadOrderStatus?` | Tidak | — | — | — | — | Tidak | Status sebelum ditahan, supaya dapat dilanjutkan tanpa menebak |
| `ClinicalIndication` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Alasan klinis pemeriksaan |
| `RequestedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Kapan dokter memesan |
| `RequestedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Dokter pemesan |
| `ScheduledAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Jadwal pemeriksaan |
| `CompletedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Kapan pesanan ditutup selesai |
| `ClosureReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Alasan penolakan atau pembatalan |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Token konkurensi |
| `IsUrgent` | `bool` | Ya | `false` | Index gabungan | — | — | Tidak | **BARU.** Penanda cito, diisi dokter pengirim |
| `UrgentMarkedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | **BARU.** Siapa yang menandai cito |
| `UrgentMarkedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | **BARU.** Kapan ditandai cito |

Index gabungan yang ditambahkan: `ModalityId` + `IsUrgent` + `OrderStatus`, menopang daftar
kerja per alat yang mendahulukan pesanan cito.

---

## 5. Tabel berstatus `Sudah ada` — kolom kunci saja

Sumber lengkapnya ada pada berkas model masing-masing.

### `RadStudy`
Berkas: `.../Models/RadStudy.cs`

| Kolom | Tipe | Peran | Sensitif |
|---|---|---|:---:|
| `Id` | `Guid` | PK | Tidak |
| `RadOrderId` | `Guid` | FK pesanan | Tidak |
| `EncounterId` | `Guid` | FK kunjungan, disalin | Tidak |
| `StudyNumber` | `string(64)` | UK, difilter `IsDelete = false` | Tidak |
| `StudySequence` | `int` | Nomor urut dalam satu pesanan | Tidak |
| `StudyStatus` | `RadStudyStatus` | Status | Tidak |
| `IsUsable` | `bool?` | **Penentu kelayakan tagih.** `null` berarti belum dinilai | Tidak |
| `SafetyRuleVersionAtClearance` | `int?` | Versi aturan dibekukan saat lolos | Tidak |
| `RepeatOfStudyId` | `Guid?` | FK ke study yang diulang | Tidak |
| `RepeatCause` | `RadRepeatCause?` | Sebab pengulangan | Tidak |
| `AbortCause` | `RadAbortCause?` | Sebab penghentian | Tidak |
| `QualityNote` | `string(1000)?` | Catatan mutu | **Ya** |
| `AbortReason` | `string(1000)?` | Alasan penghentian | **Ya** |
| `PerformedPortionNote` | `string(1000)?` | Bagian yang sempat dikerjakan | **Ya** |
| `BillingFactSubmitted` | `bool` | Penjaga pengiriman ganda ke Billing | Tidak |
| `ExternalStudyUid` | `string(128)?` | Cadangan RIS/PACS, tidak dipakai | Tidak |
| `Version` | `int` | Token konkurensi | Tidak |

### `RadStudySafetyCheck`
Berkas: `.../Models/RadStudySafetyCheck.cs`

| Kolom | Tipe | Peran | Sensitif |
|---|---|---|:---:|
| `Id` | `Guid` | PK | Tidak |
| `RadStudyId` | `Guid` | FK study | Tidak |
| `SafetyRequirementId` | `Guid` | FK butir keselamatan | Tidak |
| `RequirementCodeSnapshot` | `string` | Salinan kode butir saat dijawab | Tidak |
| `IsMandatorySnapshot` | `bool` | Salinan sifat wajib saat dijawab | Tidak |
| `RuleVersionSnapshot` | `int` | Salinan versi aturan saat dijawab | Tidak |
| `CheckState` | `RadSafetyCheckState` | Jawaban | Tidak |
| `Note` | `string?` | Keterangan jawaban | **Ya** |

### `RadAcquisitionConsumption`
Berkas: `.../Models/RadAcquisitionConsumption.cs`

| Kolom | Tipe | Peran | Sensitif |
|---|---|---|:---:|
| `Id` | `Guid` | PK | Tidak |
| `RadStudyId` | `Guid` | FK study | Tidak |
| `ItemType` | `RadConsumptionItemType` | Jenis bahan | Tidak |
| `ItemCode`, `ItemName` | `string` | Identitas bahan | Tidak |
| `Quantity` | `decimal` | **Jumlah, bukan rupiah** | Tidak |
| `Unit` | `string` | Satuan | Tidak |
| `ConsumedDespiteFailure` | `bool` | Terpakai walau pemeriksaan gagal | Tidak |

### `RadTransitionHistory`
Berkas: `.../Models/RadTransitionHistory.cs`

| Kolom | Tipe | Peran | Sensitif |
|---|---|---|:---:|
| `Id` | `Guid` | PK | Tidak |
| `RadOrderId` | `Guid` | FK pesanan | Tidak |
| `RadStudyId` | `Guid?` | FK study, kosong bila lingkupnya pesanan | Tidak |
| `Scope` | `RadTransitionScope` | Objek yang berpindah status | Tidak |
| `Action`, `FromStatus`, `ToStatus` | `string` | Perpindahannya | Tidak |
| `ReasonCode` | `string?` | Kode alasan | Tidak |
| `ReasonNote` | `string?` | Uraian alasan | **Ya** |
| `ActorUserId` | `Guid` | Pelaku | Tidak |
| `OccurredAt` | `DateTime` | Waktu | Tidak |

### `MstRadModality` dan `MstRadSafetyRequirement`
Berkas: `.../Models/MstRadModality.cs`, `.../Models/MstRadSafetyRequirement.cs`

| Tabel | Kolom kunci | Sensitif |
|---|---|:---:|
| `MstRadModality` | `Id` PK, `ModalityCode` UK, `ModalityName`, `UsesIonisingRadiation`, `SupportsContrast`, `IsActive` | Tidak |
| `MstRadSafetyRequirement` | `Id` PK, `RequirementCode` UK, `RequirementName`, `Category`, `RequiresNote`, `IsActive` | Tidak |

---

## 6. Kolom Sensitif

Kolom bertanda **Ya** pada kolom Sensitif:

- **MUST NOT** masuk ke custom logger;
- **MUST NOT** dipakai sebagai contoh berisi data asli di dokumentasi mana pun;
- **SHOULD** ditinjau kebutuhan penyamarannya pada response DTO.

Daftar lengkapnya: `ClinicalIndication`, `ClosureReason`, `QualityNote`, `AbortReason`,
`PerformedPortionNote`, `Note` pada `RadStudySafetyCheck`, `ReasonNote` pada
`RadTransitionHistory`, serta `Findings`, `Impression`, `Recommendation`, dan
`AmendmentReason` pada `RadReportVersion`.

Empat kolom terakhir adalah yang paling sensitif di seluruh modul: isinya kesimpulan klinis
atas seorang pasien.

---

## 7. Skema DDL

> **PERINGATAN.** Basis data project ini dibentuk EF Core Migrations, **bukan** skrip SQL
> manual. DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip untuk dijalankan.
> Menjalankannya akan berbenturan dengan migration.

Hanya tabel `Baru` dan `Diperbarui` yang ditulis DDL-nya. Kolom audit `IdentityModel` tidak
ditulis ulang.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.

CREATE TABLE public."RadReport" (
    "Id"                    uuid          NOT NULL,
    "RadStudyId"            uuid          NOT NULL,
    "RadOrderId"            uuid          NOT NULL,
    "EncounterId"           uuid          NOT NULL,
    "ReportNumber"          varchar(64)   NOT NULL,
    "ReportStatus"          integer       NOT NULL,  -- enum, HasConversion<int>
    "CurrentVersionNumber"  integer       NOT NULL,
    "FirstReleasedAt"       timestamp,
    "LastReleasedAt"        timestamp,
    "Version"               integer       NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_RadReport" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RadReport_RadStudy_RadStudyId"
        FOREIGN KEY ("RadStudyId") REFERENCES public."RadStudy" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RadReport_RadOrder_RadOrderId"
        FOREIGN KEY ("RadOrderId") REFERENCES public."RadOrder" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_RadReport_RadStudyId"
    ON public."RadReport" ("RadStudyId") WHERE "IsDelete" = false;

CREATE UNIQUE INDEX "IX_RadReport_ReportNumber"
    ON public."RadReport" ("ReportNumber") WHERE "IsDelete" = false;

CREATE INDEX "IX_RadReport_EncounterId" ON public."RadReport" ("EncounterId");
CREATE INDEX "IX_RadReport_ReportStatus" ON public."RadReport" ("ReportStatus");


CREATE TABLE public."RadReportVersion" (
    "Id"                  uuid           NOT NULL,
    "RadReportId"         uuid           NOT NULL,
    "VersionNumber"       integer        NOT NULL,
    "PreviousVersionId"   uuid,
    "VersionStatus"       integer        NOT NULL,  -- enum, HasConversion<int>
    "IsAmendment"         boolean        NOT NULL,
    "Findings"            varchar(8000),            -- SENSITIF
    "Impression"          varchar(4000)  NOT NULL,  -- SENSITIF
    "Recommendation"      varchar(2000),            -- SENSITIF
    "AuthorUserId"        uuid           NOT NULL,
    "AuthorRoleSnapshot"  integer        NOT NULL,  -- enum, peran dibekukan
    "DraftedAt"           timestamp      NOT NULL,
    "ValidatorUserId"     uuid,
    "ValidatedAt"         timestamp,
    "ReleasedAt"          timestamp,
    "AmendmentReason"     varchar(1000),            -- SENSITIF
    "Version"             integer        NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_RadReportVersion" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RadReportVersion_RadReport_RadReportId"
        FOREIGN KEY ("RadReportId") REFERENCES public."RadReport" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RadReportVersion_RadReportVersion_PreviousVersionId"
        FOREIGN KEY ("PreviousVersionId") REFERENCES public."RadReportVersion" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_RadReportVersion_RadReportId_VersionNumber"
    ON public."RadReportVersion" ("RadReportId", "VersionNumber");

CREATE INDEX "IX_RadReportVersion_PreviousVersionId"
    ON public."RadReportVersion" ("PreviousVersionId");

CREATE INDEX "IX_RadReportVersion_AuthorUserId"
    ON public."RadReportVersion" ("AuthorUserId");


-- MstRadModalitySafetyRule — DIPERBARUI, enam kolom ditambahkan
ALTER TABLE public."MstRadModalitySafetyRule"
    ADD COLUMN "RuleStatus"         integer       NOT NULL DEFAULT 3,  -- 3 = Active
    ADD COLUMN "SubmittedByUserId"  uuid,
    ADD COLUMN "SubmittedAt"        timestamp,
    ADD COLUMN "RejectedByUserId"   uuid,
    ADD COLUMN "RejectedAt"         timestamp,
    ADD COLUMN "RejectionReason"    varchar(1000);

-- Filter index unik diubah dari IsActive menjadi RuleStatus.
-- URUTAN PENTING: isi RuleStatus lebih dulu, baru ubah index.
DROP INDEX "IX_MstRadModalitySafetyRule_ModalityId_ProcedureId_SafetyRequirementId";

CREATE UNIQUE INDEX "IX_MstRadModalitySafetyRule_ModalityId_ProcedureId_SafetyRequirementId"
    ON public."MstRadModalitySafetyRule" ("ModalityId", "ProcedureId", "SafetyRequirementId")
    WHERE "IsDelete" = false AND "RuleStatus" = 3;


-- RadOrder — DIPERBARUI, tiga kolom penanda cito ditambahkan
ALTER TABLE public."RadOrder"
    ADD COLUMN "IsUrgent"              boolean    NOT NULL DEFAULT false,
    ADD COLUMN "UrgentMarkedByUserId"  uuid,
    ADD COLUMN "UrgentMarkedAt"        timestamp;

-- Menopang daftar kerja per alat yang mendahulukan pesanan cito.
CREATE INDEX "IX_RadOrder_ModalityId_IsUrgent_OrderStatus"
    ON public."RadOrder" ("ModalityId", "IsUrgent", "OrderStatus");
```
