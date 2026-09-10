# Kamus Data — Modul Accounting

| Field | Value |
|---|---|
| Blueprint ID | `ACC-BP-001` · Revision `3` · Status `draft` |
| Cakupan | MVP tulang punggung akuntansi (`ACC-DEC-009`) |
| Backend SHA | `aa837d784ff51cb2b889cf975ada3a204018f1f5` |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu **tidak diulang** pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris. Untuk jurnal
berstatus `Posted`, penandaan itu tetap dilarang oleh `ACC-DEC-006`.

Seluruh nama tabel berawalan `Acc` masih **sementara** sampai `ACC-DEP-002` selesai.

## Catatan tentang kolom sensitif

Modul Accounting MVP **tidak menyimpan satu pun data pribadi** — tidak ada kolom pasien maupun
pegawai. Yang perlu dijaga adalah **rahasia bisnis**: nilai uang dan keterangan jurnal.

Kolom bertanda **Ya** pada kolom Sensitif tidak boleh masuk ke custom logger, dan tidak boleh
dipakai sebagai contoh berisi angka asli di dokumentasi.

---

## 1. `AccChartOfAccount` — status `Baru`

Menyimpan satu akun pada daftar akun.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `LegalEntityId` | `Guid` | Ya | — | Unique bersama `AccountCode` | FK ke `MstLegalEntity` | `Restrict` | Tidak | Badan hukum pemilik buku (`ACC-DEC-037`) |
| `AccountCode` | `string(20)` | Ya | — | Unique bersama `LegalEntityId` | — | — | Tidak | Kode akun, contoh `1-1001`. Tidak boleh diubah setelah dipakai (`ACC-DEC-023`) |
| `AccountName` | `string(200)` | Ya | — | Index | — | — | Tidak | Nama akun, contoh `Kas Besar` |
| `ParentAccountId` | `Guid?` | Tidak | — | Index | FK ke `AccChartOfAccount` | `Restrict` | Tidak | Akun induk. Kosong bila akun tingkat pertama |
| `AccountLevel` | `int` | Ya | `1` | — | — | — | Tidak | Kedalaman susunan, 1 sampai 5 |
| `AccountType` | `int` | Ya | — | Index | — | — | Tidak | Enum `AccountType`, `HasConversion<int>` |
| `NormalBalance` | `int` | Ya | — | — | — | — | Tidak | Enum `NormalBalance`. Disimpan tersendiri agar akun kontra dapat ditangani |
| `IsPostable` | `bool` | Ya | `false` | — | — | — | Tidak | Menerima transaksi atau tidak. Wajib `false` bila punya anak (`ACC-DEC-022`) |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Tidak boleh dimatikan bila saldo belum nol (`ACC-DEC-024`) |
| `EffectiveStartDate` | `DateTime?` | Tidak | — | — | — | — | Tidak | Mulai berlaku |
| `Description` | `string(500)?` | Tidak | — | — | — | — | Tidak | Keterangan bebas |

## 2. `AccJournalType` — status `Baru`

Menyimpan jenis jurnal beserta aturan alurnya.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `JournalTypeCode` | `string(10)` | Ya | — | Unique | — | — | Tidak | Contoh `JU`, `JP`, `JB`, `SA` |
| `JournalTypeName` | `string(100)` | Ya | — | — | — | — | Tidak | Contoh `Jurnal Umum` |
| `NumberPrefix` | `string(10)` | Ya | — | — | — | — | Tidak | Awalan nomor jurnal. **Wajib dari master**, tidak boleh ditulis di kode |
| `RequiresApproval` | `bool` | Ya | `true` | — | — | — | Tidak | Mewujudkan `ACC-DEC-010` |
| `IsSystemType` | `bool` | Ya | `false` | — | — | — | Tidak | Jenis sistem tidak dapat dihapus pengguna |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Penanda aktif |

Tidak ada `LegalEntityId`: jenis jurnal berlaku sama untuk semua badan hukum.

## 3. `AccAccountingPeriod` — status `Baru`

Menyimpan satu periode akuntansi beserta statusnya.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `LegalEntityId` | `Guid` | Ya | — | Unique bersama `PeriodCode` | FK ke `MstLegalEntity` | `Restrict` | Tidak | Setiap badan hukum menutup bukunya sendiri |
| `PeriodCode` | `string(7)` | Ya | — | Unique bersama `LegalEntityId` | — | — | Tidak | Bentuk `2026-09` (`ACC-DEC-013`) |
| `FiscalYear` | `int` | Ya | — | Index | — | — | Tidak | Sama dengan tahun kalender |
| `PeriodMonth` | `int` | Ya | — | — | — | — | Tidak | 1 sampai 12 |
| `StartDate` | `DateTime` | Ya | — | — | — | — | Tidak | Tanggal 1 bulan itu |
| `EndDate` | `DateTime` | Ya | — | — | — | — | Tidak | Tanggal terakhir bulan itu |
| `PeriodStatus` | `int` | Ya | `1` (`Open`) | Index | — | — | Tidak | Enum `AccountingPeriodStatus`, `HasConversion<int>` |
| `ClosedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Pengguna yang menutup |
| `ClosedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu penutupan |
| `ReopenedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Pengguna yang membuka kembali |
| `ReopenedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pembukaan kembali |
| `LastReasonNote` | `string(500)?` | Tidak | — | — | — | — | Tidak | Alasan terakhir. Wajib diisi saat membuka kembali (`ACC-DEC-027`) |

## 4. `AccJournal` — status `Baru`

Kepala satu catatan transaksi akuntansi. Aggregate root.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `LegalEntityId` | `Guid` | Ya | — | Unique bersama `JournalNumber` | FK ke `MstLegalEntity` | `Restrict` | Tidak | Satu jurnal tidak boleh mencampur dua badan hukum |
| `JournalNumber` | `string(30)` | Ya | — | Unique bersama `LegalEntityId` | — | — | Tidak | Contoh `JU/2026/09/00001`. Boleh ada nomor terlewat (`ACC-DEC-014`) |
| `JournalTypeId` | `Guid` | Ya | — | Index | FK ke `AccJournalType` | `Restrict` | Tidak | Menentukan awalan nomor dan aturan alur |
| `AccountingPeriodId` | `Guid` | Ya | — | Index | FK ke `AccAccountingPeriod` | `Restrict` | Tidak | Ditentukan sistem dari `AccountingDate` |
| `DocumentNumber` | `string(50)?` | Tidak | — | — | — | — | Tidak | Nomor dokumen sumber, misalnya nomor faktur pemasok |
| `DocumentDate` | `DateTime?` | Tidak | — | — | — | Tidak | Tanggal dokumen sumber |
| `AccountingDate` | `DateTime` | Ya | — | Index | — | — | Tidak | Menentukan periode. Inilah tanggal yang dipakai laporan |
| `Description` | `string(500)` | Ya | — | — | — | — | **Ya** | Keterangan jurnal. Rahasia bisnis, tidak boleh masuk logger |
| `JournalStatus` | `int` | Ya | `1` (`Draft`) | Index | — | — | Tidak | Enum `JournalStatus`, `HasConversion<int>` |
| `TotalDebit` | `decimal(18,2)` | Ya | `0` | — | — | — | **Ya** | **Salinan** dari jumlah baris. Bukan sumber kebenaran |
| `TotalCredit` | `decimal(18,2)` | Ya | `0` | — | — | — | **Ya** | **Salinan** dari jumlah baris. Bukan sumber kebenaran |
| `SubmittedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Pengaju |
| `SubmittedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pengajuan |
| `ApprovedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Penyetuju. Tidak boleh sama dengan `CreateBy` (`ACC-DEC-016`) |
| `ApprovedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu persetujuan |
| `PostedBy` | `Guid?` | Tidak | — | — | — | — | Tidak | Pengesah |
| `PostedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pengesahan |
| `RejectionReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib diisi saat menolak |
| `ReversalOfJournalId` | `Guid?` | Tidak | — | Index | FK ke `AccJournal` | `Restrict` | Tidak | Jurnal yang dikoreksi. Kosong bila jurnal biasa |
| `CorrectionType` | `int?` | Tidak | — | — | — | — | Tidak | Enum `JournalCorrectionType`. Kosong bila bukan koreksi |

## 5. `AccJournalLine` — status `Baru`

Satu baris jurnal. Sumber tunggal buku besar.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `JournalId` | `Guid` | Ya | — | Unique bersama `LineNumber` | FK ke `AccJournal` | `Cascade` | Tidak | Induk jurnal |
| `LineNumber` | `int` | Ya | — | Unique bersama `JournalId` | — | — | Tidak | Urutan baris, mulai dari 1 |
| `AccountId` | `Guid` | Ya | — | Index | FK ke `AccChartOfAccount` | `Restrict` | Tidak | Wajib akun yang menerima transaksi dan aktif |
| `CostCenterId` | `Guid?` | Tidak | — | Index | FK ke `MstCostCenter` | `Restrict` | Tidak | **Wajib bila akun berjenis `Expense`** (`ACC-DEC-019`) |
| `Description` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Keterangan baris |
| `DebitAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | **Ya** | Nol bila baris ini kredit |
| `CreditAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | **Ya** | Nol bila baris ini debit |

`JournalId` memakai `Cascade`, berbeda dari relasi lain yang memakai `Restrict`. Ini disengaja:
baris jurnal tidak punya makna tanpa jurnalnya, dan penghapusan jurnal hanya mungkin saat masih
`Draft`. Untuk jurnal `Posted`, penghapusan sudah dilarang di lapis service sehingga cascade tidak
pernah tercapai.

## 6. `AccJournalApproval` — status `Baru`

Riwayat tindakan pada sebuah jurnal. Tidak pernah diubah maupun dihapus.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `JournalId` | `Guid` | Ya | — | Index | FK ke `AccJournal` | `Restrict` | Tidak | Jurnal yang ditindaklanjuti |
| `ApprovalAction` | `int` | Ya | — | — | — | — | Tidak | Enum `JournalApprovalAction`, `HasConversion<int>` |
| `ActionBy` | `Guid` | Ya | — | Index | — | — | Tidak | Pelaku tindakan |
| `ActionAt` | `DateTime` | Ya | `DateTime.UtcNow` | — | — | — | Tidak | Waktu tindakan |
| `Reason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib untuk `Rejected` dan `Reversed` |

`JournalId` memakai `Restrict`, bukan `Cascade`. Riwayat persetujuan adalah bukti audit dan tidak
boleh ikut terhapus.

## 7. `AccNumberSeries` — status `Baru`

Alokator nomor jurnal milik Accounting. Bentuknya meniru `BilNumberSeries` di
`Areas/HealthServices/BillingManagement/Billing/Models/BilNumberSeries.cs@aa837d7`, karena
`ACC-DEC-004` melarang Accounting menulis tabel Billing dan `QBE-CODE-006` menuntut alokator yang
atomik dan ber-scope.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `SequenceKey` | `string(50)` | Ya | — | Unique bersama `ScopeKey` | — | — | Tidak | Identitas deret, misalnya jenis jurnal per badan hukum |
| `ScopeKey` | `string(50)` | Ya | — | Unique bersama `SequenceKey` | — | — | Tidak | Cakupan reset. Untuk nomor jurnal berbentuk `yyyyMM` |
| `ResetPolicy` | `string(20)` | Ya | — | — | — | — | Tidak | `NEVER`, `YEARLY`, `MONTHLY`, atau `DAILY`. Nomor jurnal memakai `MONTHLY` |
| `CurrentValue` | `long` | Ya | `0` | — | — | — | Tidak | Nilai terakhir yang dialokasikan |
| `LastAllocatedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu alokasi terakhir |

Tabel ini **tidak** menyimpan nomor jurnal itu sendiri — nomornya tersimpan di
`AccJournal.JournalNumber`. Ia hanya menyimpan penghitungnya.

Alokasi wajib berada di dalam transaction dan didahului `pg_advisory_xact_lock`, sesuai pola
repository. Nomor terlewat tetap diizinkan (`ACC-DEC-014`); nomor kembar tidak. Rinciannya di
`roadmap/backend-roadmap.md` bagian `BE-ACC-010`.

## 8. `MstCostCenter` — status `Sudah ada`

Dimiliki Corporate / Human Resource / Master Data / Organization. **Tidak diubah** modul ini.
Sumber lengkap: `Areas/Corporate/HumanResource/MasterData/Organization/Models/MstCostCenter.cs@aa837d7`.

Kolom kunci yang dipakai Accounting:

| Kolom | Tipe | Keterangan bagi Accounting |
|---|---|---|
| `Id` | `Guid` | Tujuan FK dari `AccJournalLine.CostCenterId` |
| `LegalEntityId` | `Guid` | Wajib sama dengan badan hukum jurnalnya |
| `CostCenterCode` | `string(50)` | Ditampilkan pada daftar pilihan |
| `CostCenterName` | `string(200)` | Ditampilkan pada daftar pilihan |
| `IsActive` | `bool` | Hanya yang aktif boleh dipilih |
| `AccountingCode` | `string(100)?` | **Tidak dibaca Accounting.** Maknanya perlu diperjelas pemilik Human Resource setelah Accounting menjadi pemilik COA |

## 9. `MstLegalEntity` — status `Sudah ada`

Dimiliki Corporate / Master Data. **Tidak diubah** modul ini.
Sumber lengkap: `Repositories/ApplicationDbContext.cs#MstLegalEntities@aa837d7`.

| Kolom | Tipe | Keterangan bagi Accounting |
|---|---|---|
| `Id` | `Guid` | Tujuan FK dari `AccChartOfAccount`, `AccAccountingPeriod`, dan `AccJournal` |

---

## Bentuk DDL

> **Peringatan.** Basis data project ini dibentuk EF Core Migrations, **bukan** skrip SQL manual.
> DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip yang dijalankan. Menjalankannya
> akan berbenturan dengan migration. Sumber kebenarannya adalah berkas configuration di
> `Repositories/Configurations/Corporate/AccountingManagement/`.

Kolom audit `IdentityModel` tidak ditulis ulang pada DDL di bawah.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.

CREATE TABLE public."AccChartOfAccount" (
    "Id"                  uuid          NOT NULL,
    "LegalEntityId"       uuid          NOT NULL,
    "AccountCode"         varchar(20)   NOT NULL,
    "AccountName"         varchar(200)  NOT NULL,
    "ParentAccountId"     uuid,
    "AccountLevel"        integer       NOT NULL DEFAULT 1,
    "AccountType"         integer       NOT NULL,  -- enum, HasConversion<int>
    "NormalBalance"       integer       NOT NULL,  -- enum, HasConversion<int>
    "IsPostable"          boolean       NOT NULL DEFAULT false,
    "IsActive"            boolean       NOT NULL DEFAULT true,
    "EffectiveStartDate"  date,
    "Description"         varchar(500),

    CONSTRAINT "PK_AccChartOfAccount" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AccChartOfAccount_MstLegalEntity_LegalEntityId"
        FOREIGN KEY ("LegalEntityId") REFERENCES public."MstLegalEntity" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AccChartOfAccount_AccChartOfAccount_ParentAccountId"
        FOREIGN KEY ("ParentAccountId") REFERENCES public."AccChartOfAccount" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_AccChartOfAccount_LegalEntityId_AccountCode"
    ON public."AccChartOfAccount" ("LegalEntityId", "AccountCode");
CREATE INDEX "IX_AccChartOfAccount_ParentAccountId"
    ON public."AccChartOfAccount" ("ParentAccountId");


CREATE TABLE public."AccJournalType" (
    "Id"                uuid          NOT NULL,
    "JournalTypeCode"   varchar(10)   NOT NULL,
    "JournalTypeName"   varchar(100)  NOT NULL,
    "NumberPrefix"      varchar(10)   NOT NULL,
    "RequiresApproval"  boolean       NOT NULL DEFAULT true,
    "IsSystemType"      boolean       NOT NULL DEFAULT false,
    "IsActive"          boolean       NOT NULL DEFAULT true,

    CONSTRAINT "PK_AccJournalType" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_AccJournalType_JournalTypeCode"
    ON public."AccJournalType" ("JournalTypeCode");


CREATE TABLE public."AccAccountingPeriod" (
    "Id"              uuid         NOT NULL,
    "LegalEntityId"   uuid         NOT NULL,
    "PeriodCode"      varchar(7)   NOT NULL,
    "FiscalYear"      integer      NOT NULL,
    "PeriodMonth"     integer      NOT NULL,
    "StartDate"       date         NOT NULL,
    "EndDate"         date         NOT NULL,
    "PeriodStatus"    integer      NOT NULL DEFAULT 1,  -- enum, HasConversion<int>
    "ClosedBy"        uuid,
    "ClosedAt"        timestamptz,
    "ReopenedBy"      uuid,
    "ReopenedAt"      timestamptz,
    "LastReasonNote"  varchar(500),

    CONSTRAINT "PK_AccAccountingPeriod" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AccAccountingPeriod_MstLegalEntity_LegalEntityId"
        FOREIGN KEY ("LegalEntityId") REFERENCES public."MstLegalEntity" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_AccAccountingPeriod_LegalEntityId_PeriodCode"
    ON public."AccAccountingPeriod" ("LegalEntityId", "PeriodCode");


CREATE TABLE public."AccJournal" (
    "Id"                    uuid            NOT NULL,
    "LegalEntityId"         uuid            NOT NULL,
    "JournalNumber"         varchar(30)     NOT NULL,
    "JournalTypeId"         uuid            NOT NULL,
    "AccountingPeriodId"    uuid            NOT NULL,
    "DocumentNumber"        varchar(50),
    "DocumentDate"          date,
    "AccountingDate"        date            NOT NULL,
    "Description"           varchar(500)    NOT NULL,  -- SENSITIF
    "JournalStatus"         integer         NOT NULL DEFAULT 1,  -- enum, HasConversion<int>
    "TotalDebit"            numeric(18,2)   NOT NULL DEFAULT 0,  -- SENSITIF
    "TotalCredit"           numeric(18,2)   NOT NULL DEFAULT 0,  -- SENSITIF
    "SubmittedBy"           uuid,
    "SubmittedAt"           timestamptz,
    "ApprovedBy"            uuid,
    "ApprovedAt"            timestamptz,
    "PostedBy"              uuid,
    "PostedAt"              timestamptz,
    "RejectionReason"       varchar(500),
    "ReversalOfJournalId"   uuid,
    "CorrectionType"        integer,  -- enum, HasConversion<int>

    CONSTRAINT "PK_AccJournal" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AccJournal_MstLegalEntity_LegalEntityId"
        FOREIGN KEY ("LegalEntityId") REFERENCES public."MstLegalEntity" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AccJournal_AccJournalType_JournalTypeId"
        FOREIGN KEY ("JournalTypeId") REFERENCES public."AccJournalType" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AccJournal_AccAccountingPeriod_AccountingPeriodId"
        FOREIGN KEY ("AccountingPeriodId") REFERENCES public."AccAccountingPeriod" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AccJournal_AccJournal_ReversalOfJournalId"
        FOREIGN KEY ("ReversalOfJournalId") REFERENCES public."AccJournal" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_AccJournal_LegalEntityId_JournalNumber"
    ON public."AccJournal" ("LegalEntityId", "JournalNumber");
CREATE INDEX "IX_AccJournal_AccountingPeriodId"      ON public."AccJournal" ("AccountingPeriodId");
CREATE INDEX "IX_AccJournal_AccountingDate"          ON public."AccJournal" ("AccountingDate");
CREATE INDEX "IX_AccJournal_JournalStatus"           ON public."AccJournal" ("JournalStatus");


CREATE TABLE public."AccJournalLine" (
    "Id"            uuid            NOT NULL,
    "JournalId"     uuid            NOT NULL,
    "LineNumber"    integer         NOT NULL,
    "AccountId"     uuid            NOT NULL,
    "CostCenterId"  uuid,
    "Description"   varchar(500),                        -- SENSITIF
    "DebitAmount"   numeric(18,2)   NOT NULL DEFAULT 0,  -- SENSITIF
    "CreditAmount"  numeric(18,2)   NOT NULL DEFAULT 0,  -- SENSITIF

    CONSTRAINT "PK_AccJournalLine" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AccJournalLine_AccJournal_JournalId"
        FOREIGN KEY ("JournalId") REFERENCES public."AccJournal" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AccJournalLine_AccChartOfAccount_AccountId"
        FOREIGN KEY ("AccountId") REFERENCES public."AccChartOfAccount" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_AccJournalLine_MstCostCenter_CostCenterId"
        FOREIGN KEY ("CostCenterId") REFERENCES public."MstCostCenter" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_AccJournalLine_JournalId_LineNumber"
    ON public."AccJournalLine" ("JournalId", "LineNumber");
CREATE INDEX "IX_AccJournalLine_AccountId"     ON public."AccJournalLine" ("AccountId");
CREATE INDEX "IX_AccJournalLine_CostCenterId"  ON public."AccJournalLine" ("CostCenterId");


CREATE TABLE public."AccJournalApproval" (
    "Id"              uuid          NOT NULL,
    "JournalId"       uuid          NOT NULL,
    "ApprovalAction"  integer       NOT NULL,  -- enum, HasConversion<int>
    "ActionBy"        uuid          NOT NULL,
    "ActionAt"        timestamptz   NOT NULL,
    "Reason"          varchar(500),

    CONSTRAINT "PK_AccJournalApproval" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AccJournalApproval_AccJournal_JournalId"
        FOREIGN KEY ("JournalId") REFERENCES public."AccJournal" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_AccJournalApproval_JournalId"  ON public."AccJournalApproval" ("JournalId");

CREATE TABLE public."AccNumberSeries" (
    "Id"               uuid          NOT NULL,
    "SequenceKey"      varchar(50)   NOT NULL,
    "ScopeKey"         varchar(50)   NOT NULL,
    "ResetPolicy"      varchar(20)   NOT NULL,
    "CurrentValue"     bigint        NOT NULL,
    "LastAllocatedAt"  timestamptz   NOT NULL,

    CONSTRAINT "PK_AccNumberSeries" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_AccNumberSeries_SequenceKey_ScopeKey"
    ON public."AccNumberSeries" ("SequenceKey", "ScopeKey");
```

## Aturan yang tidak dapat dijaga database

Tiga invariant berikut **tidak** dapat ditegakkan lewat constraint tabel, sehingga wajib
ditegakkan di `AccJournalService`. Ini dicatat di sini agar implementer tidak menyangka database
sudah menjaganya.

| Invariant | Kenapa tidak bisa lewat constraint |
|---|---|
| Total debit sama dengan total kredit | Melibatkan seluruh baris satu jurnal, bukan satu baris |
| Seluruh baris menunjuk akun milik badan hukum yang sama dengan jurnalnya | Melibatkan tabel lain lewat dua tingkat relasi |
| Penyetuju tidak boleh sama dengan pembuat | Membandingkan dua kolom yang diisi pada waktu berbeda |

Satu invariant lagi, yaitu "tepat satu dari debit atau kredit lebih besar dari nol", **dapat**
ditegakkan lewat check constraint dan sebaiknya memang dipasang sebagai lapis kedua di samping
pemeriksaan service.


---

# PHASE 2 (`ACC-PH-006`) — Tabel Baru dan Diperbarui

| Field | Nilai |
|---|---|
| Cakupan | `ACC-P2-S1` sampai `ACC-P2-S4` |
| Status | `draft` — 8 September 2026 |
| Backend SHA | `02c3219` |
| Traceability | `ACC-DEC-044` sampai `ACC-DEC-057` |

Sepuluh kolom warisan `IdentityModel` tetap tidak diulang, sama seperti bagian MVP di atas.

## Catatan sensitif Phase 2 — bertambah satu hal penting

MVP tidak menyimpan data pribadi sama sekali. Phase 2 adalah fase pertama yang menerima data
**berasal** dari tagihan pasien, sehingga satu batas baru berlaku:

> `ACC-DEC-056` melarang seluruh tabel Accounting menyimpan nama pasien, nomor rekam medis, dan
> nomor kunjungan. Yang boleh disimpan hanya **modul asal** dan **nomor transaksi asal**.

Karena itu kolom `SourceTransactionId` ditandai **Sensitif = Ya**: ia bukan data pribadi, tetapi
ia adalah **penunjuk** ke kunjungan pasien. Ia tidak boleh masuk custom logger dan tidak boleh
dipakai sebagai contoh berisi nomor asli.

---

## 9. `AccAccountingEvent` — status `Baru`

Satu baris untuk setiap kejadian keuangan yang pernah diterima, berhasil maupun tidak.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `LegalEntityId` | `Guid` | Ya | — | Index bersama `EventStatus` | FK ke `MstLegalEntity` | `Restrict` | Tidak | Badan hukum yang bukunya disentuh |
| `EventNumber` | `string(50)` | Ya | — | **Unique** | — | — | Tidak | Nomor kejadian dari penerbit, contoh `EVT-100`. Kunci anti-ganda **pertama** (`ACC-DEC-035`) |
| `EventTypeId` | `Guid` | Ya | — | Unique bersama tiga kolom sumber | FK ke `AccEventType` | `Restrict` | Tidak | Jenis kejadian |
| `SourceModule` | `string(50)` | Ya | — | Unique bersama tiga kolom lain | — | — | Tidak | Modul penerbit, contoh `Finance` |
| `SourceTransactionId` | `string(100)` | Ya | — | Unique bersama tiga kolom lain | — | — | **Ya** | Nomor dokumen di modul asal. **Penunjuk ke kunjungan pasien** — lihat catatan sensitif |
| `SourceVersion` | `string(20)` | Ya | `"1"` | Unique bersama tiga kolom lain | — | — | Tidak | Versi kejadian. Kunci anti-ganda **kedua** bersama tiga kolom di atas |
| `EventOccurredAt` | `timestamptz` | Ya | — | — | — | — | Tidak | Waktu kejadian di modul asal (`ACC-DEC-040`) |
| `AccountingDate` | `date` | Ya | — | Index | — | — | Tidak | Tanggal akuntansi yang menentukan periode |
| `DocumentDate` | `date` | Ya | — | — | — | — | Tidak | Tanggal dokumen asli. **Berbeda dari `AccountingDate` bila kejadian datang terlambat** (`ACC-DEC-047`) |
| `Amount` | `numeric(18,2)` | Ya | — | — | — | — | **Ya** | Nilai kejadian. Wajib lebih besar dari nol |
| `CurrencyCode` | `string(3)` | Ya | `"IDR"` | — | — | — | Tidak | Hanya `IDR` diterima (`ACC-DEC-020`) |
| `EventStatus` | `int` | Ya | `1` | Index bersama `LegalEntityId` | — | — | Tidak | Enum `AccountingEventStatus`, `HasConversion<int>` |
| `JournalId` | `Guid?` | Tidak | — | Index | FK ke `AccJournal` | `Restrict` | Tidak | Jurnal yang dihasilkan. **Kosong** untuk kejadian Tertahan, Gagal, dan Diabaikan |
| `RawPayload` | `text` | Ya | — | — | — | — | **Ya** | Isi pesan asli apa adanya. Disimpan untuk menyelesaikan selisih angka di kemudian hari |
| `AttemptCount` | `int` | Ya | `0` | — | — | — | Tidak | Jumlah percobaan otomatis. Berhenti di 3 (`ACC-DEC-049`) |
| `IgnoreReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Alasan diabaikan. **Wajib** bila status `Diabaikan` |

**Dua unique index, bukan satu.** `EventNumber` sendirian tidak cukup: bila penerbit keliru
membuat nomor baru untuk kejadian yang sama, index pertama tidak menangkapnya. Gabungan
`SourceModule` + `SourceTransactionId` + `EventTypeId` + `SourceVersion` adalah jaring keduanya.

## 10. `AccAccountingEventAttempt` — status `Baru`

Riwayat setiap percobaan pemrosesan sebuah kejadian.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `AccountingEventId` | `Guid` | Ya | — | Unique bersama `AttemptNumber` | FK ke `AccAccountingEvent` | `Cascade` | Tidak | Kejadian induknya |
| `AttemptNumber` | `int` | Ya | — | Unique bersama `AccountingEventId` | — | — | Tidak | Nomor urut percobaan, 1 sampai 3 untuk otomatis; lebih dari 3 untuk coba ulang manual |
| `AttemptedAt` | `timestamptz` | Ya | — | — | — | — | Tidak | Waktu percobaan |
| `IsSuccess` | `bool` | Ya | `false` | — | — | — | Tidak | Berhasil atau tidak |
| `FailureMessage` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Pesan kegagalan teknis. **Bukan** pesan bagi pengguna |

`Cascade` dipakai di sini — satu-satunya di Phase 2 — karena percobaan tidak punya arti tanpa
kejadian induknya, persis seperti baris jurnal terhadap jurnalnya.

## 11. `AccEventType` — status `Baru`

Daftar jenis kejadian keuangan yang dikenal Accounting.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EventTypeCode` | `string(50)` | Ya | — | **Unique** | — | — | Tidak | Kode jenis, contoh `PENGAKUAN-PIUTANG` |
| `EventTypeName` | `string(200)` | Ya | — | Index | — | — | Tidak | Nama yang dibaca petugas |
| `SourceModule` | `string(50)` | Ya | — | — | — | — | Tidak | Modul yang diharapkan menerbitkannya |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Tidak boleh dimatikan bila masih ada aturan posting aktif |

**Isinya belum dapat ditetapkan** — `DEC-ACC-P2-002` masih `OPEN` dan menunggu owner Finance.
Bentuk tabelnya tidak menunggu.

## 12. `AccPostingRule` — status `Baru`

Kepala aturan posting: jenis kejadian, badan hukum, dan perlakuannya. Akunnya ada di `AccPostingRuleLine` (`ACC-DEC-058`).

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `LegalEntityId` | `Guid` | Ya | — | Unique bersama `EventTypeId`, **berfilter `IsActive = true`** | FK ke `MstLegalEntity` | `Restrict` | Tidak | Aturan berbeda per badan hukum (`ACC-DEC-037`) |
| `EventTypeId` | `Guid` | Ya | — | Unique bersama `LegalEntityId` | FK ke `AccEventType` | `Restrict` | Tidak | Jenis kejadian yang dipetakan |
| `Treatment` | `int` | Ya | `2` | — | — | — | Tidak | Enum `AccountingEventTreatment`. Bawaan `BuatDraft` — **sengaja yang lebih aman** |
| `IsActive` | `bool` | Ya | `true` | Bagian dari unique berfilter | — | — | Tidak | Aturan lama disimpan nonaktif, tidak dihapus |

**Kenapa unique index-nya berfilter.** Satu jenis kejadian hanya boleh punya **satu aturan aktif**
per badan hukum, tetapi aturan lama tetap disimpan sebagai riwayat. Tanpa filter `IsActive = true`,
menyimpan riwayat menjadi mustahil.

**Kenapa `Treatment` berbawaan `BuatDraft`.** Bila petugas lupa menetapkannya, akibat terburuknya
adalah jurnal menumpuk menunggu pemeriksaan — bukan angka salah yang langsung masuk buku besar.

**Kenapa akun debit dan kredit TIDAK ada di tabel ini** (`ACC-DEC-058`). Akun berpindah ke
`AccPostingRuleLine`, karena satu aturan dapat menghasilkan lebih dari dua baris. Bentuk lama
— sepasang `DebitAccountId` dan `CreditAccountId` — tidak dapat mengungkapkan pendapatan yang
disertai jasa medis dokter maupun potongan penjualan.

## 12b. `AccPostingRuleLine` — status `Baru`

Satu baris aturan posting. Meniru bentuk `AccJournalLine` yang sudah terbukti.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PostingRuleId` | `Guid` | Ya | — | Unique bersama `LineNumber` | FK ke `AccPostingRule` | `Cascade` | Tidak | Aturan induknya |
| `LineNumber` | `int` | Ya | — | Unique bersama `PostingRuleId` | — | — | Tidak | Nomor urut baris |
| `ComponentCode` | `string(50)` | Ya | `"TOTAL"` | Index | — | — | Tidak | Komponen nilai yang dipakai baris ini. `TOTAL` berarti memakai nilai total kejadian |
| `AccountId` | `Guid` | Ya | — | Index | FK ke `AccChartOfAccount` | `Restrict` | Tidak | Akun yang dicatat. Wajib menerima transaksi dan sebadan hukum |
| `CostCenterId` | `Guid?` | Tidak | — | Index | FK ke `MstCostCenter` | `Restrict` | Tidak | **Wajib** bila akunnya berjenis `Expense` (`ACC-DEC-019`) |
| `Side` | `int` | Ya | — | — | — | — | Tidak | Enum `PostingSide`: `Debit = 1`, `Kredit = 2` |
| `Description` | `string(500)?` | Tidak | — | — | — | — | Tidak | Keterangan yang disalin ke baris jurnal |

### Contoh isi — pendapatan rawat jalan dengan jasa medis dokter

Aturan untuk jenis kejadian `PENGAKUAN-PIUTANG`, badan hukum `LE-MMC-001`:

| Baris | Komponen | Akun | Sisi |
|---:|---|---|---|
| 1 | `TOTAL` | `1-1201 Piutang Penjamin` | Debit |
| 2 | `TOTAL` | `4-1001 Pendapatan Rawat Jalan` | Kredit |
| 3 | `JASA_MEDIS` | `5-3001 Beban Jasa Medis` | Debit |
| 4 | `JASA_MEDIS` | `2-1301 Utang Jasa Medis Dokter` | Kredit |

Kejadian bernilai total Rp 10.000.000 dengan komponen `JASA_MEDIS` Rp 3.000.000 menghasilkan
jurnal empat baris: debit Rp 13.000.000, kredit Rp 13.000.000, seimbang.

### Contoh isi — pendapatan dengan potongan

| Baris | Komponen | Akun | Sisi |
|---:|---|---|---|
| 1 | `NETTO` | `1-1201 Piutang Penjamin` | Debit |
| 2 | `POTONGAN` | `4-9001 Potongan Penjualan` | Debit |
| 3 | `TOTAL` | `4-1001 Pendapatan Rawat Jalan` | Kredit |

Kejadian bernilai total Rp 10.000.000 dengan komponen `NETTO` Rp 9.000.000 dan `POTONGAN`
Rp 1.000.000 menghasilkan debit Rp 10.000.000 lawan kredit Rp 10.000.000. **Inilah bentuk yang
tidak dapat diungkapkan rancangan lama sama sekali**, karena memecahnya menjadi dua kejadian
seimbang menuntut akun perantara yang dilarang `ACC-DEC-046`.

### Aturan yang mengikat bentuk ini

1. Minimal **dua** baris per aturan.
2. Setelah nilai komponen dimasukkan, **total debit wajib sama dengan total kredit**. Aturan yang
   tidak dapat seimbang untuk komponen mana pun ditolak saat disimpan.
3. Setiap `ComponentCode` yang dipakai baris aturan wajib ada pada kejadian, kecuali `TOTAL` yang
   selalu tersedia.
4. Kejadian yang membawa komponen **tanpa** baris aturan yang memakainya diperlakukan
   **Tertahan**, sama seperti kejadian tanpa pemetaan (`ACC-DEC-046`). Mengabaikan komponen yang
   tidak dikenal berarti membuang angka diam-diam.

## 12c. `AccAccountingEventComponent` — status `Baru`

Rincian nilai yang dibawa sebuah kejadian, di samping nilai totalnya.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `AccountingEventId` | `Guid` | Ya | — | Unique bersama `ComponentCode` | FK ke `AccAccountingEvent` | `Cascade` | Tidak | Kejadian induknya |
| `ComponentCode` | `string(50)` | Ya | — | Unique bersama `AccountingEventId` | — | — | Tidak | Kode komponen, contoh `JASA_MEDIS`, `POTONGAN`, `NETTO` |
| `Amount` | `numeric(18,2)` | Ya | — | — | — | — | **Ya** | Nilai komponen |

**Kenapa disimpan sebagai tabel, padahal `RawPayload` sudah memuat pesan aslinya.** `RawPayload`
berbentuk teks dan tidak dapat ditanya. Ketika enam bulan kemudian muncul pertanyaan "berapa total
jasa medis yang dibukukan September lalu", pertanyaan itu hanya terjawab bila komponennya berupa
kolom, bukan teks. Ini juga bahan bagi `DEC-ACC-P2-008`, deteksi aturan posting yang salah.

## 13. `AccRecurringJournalTemplate` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `LegalEntityId` | `Guid` | Ya | — | Unique bersama `TemplateCode` | FK ke `MstLegalEntity` | `Restrict` | Tidak | Badan hukum pemilik |
| `TemplateCode` | `string(50)` | Ya | — | Unique bersama `LegalEntityId` | — | — | Tidak | Kode template, contoh `SUSUT-ALKES` |
| `TemplateName` | `string(200)` | Ya | — | Index | — | — | Tidak | Nama yang dibaca petugas |
| `JournalTypeId` | `Guid` | Ya | — | — | FK ke `AccJournalType` | `Restrict` | Tidak | Jenis jurnal yang dihasilkan |
| `Frequency` | `int` | Ya | `1` | — | — | — | Tidak | Enum `RecurringFrequency`. Rilis pertama hanya `Bulanan` |
| `DayOfMonth` | `int` | Ya | `1` | — | — | — | Tidak | Tanggal terbit tiap bulan, 1 sampai 28. **Dibatasi 28** supaya Februari tidak pernah terlewat |
| `StartDate` | `date` | Ya | — | — | — | — | Tidak | Mulai berlaku |
| `EndDate` | `date?` | Tidak | — | — | — | — | Tidak | Berhenti berlaku. Kosong berarti tanpa batas |
| `IsActive` | `bool` | Ya | `false` | Index | — | — | Tidak | **Bawaan tidak aktif** — template baru wajib diperiksa dulu sebelum diaktifkan |

## 14. `AccRecurringJournalTemplateLine` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `TemplateId` | `Guid` | Ya | — | Unique bersama `LineNumber` | FK ke `AccRecurringJournalTemplate` | `Cascade` | Tidak | Template induknya |
| `LineNumber` | `int` | Ya | — | Unique bersama `TemplateId` | — | — | Tidak | Nomor urut baris |
| `AccountId` | `Guid` | Ya | — | Index | FK ke `AccChartOfAccount` | `Restrict` | Tidak | Akun yang dicatat |
| `CostCenterId` | `Guid?` | Tidak | — | Index | FK ke `MstCostCenter` | `Restrict` | Tidak | **Wajib** bila akunnya berjenis `Expense` (`ACC-DEC-019`) |
| `DebitAmount` | `numeric(18,2)` | Ya | `0` | — | — | — | **Ya** | Nilai debit |
| `CreditAmount` | `numeric(18,2)` | Ya | `0` | — | — | — | **Ya** | Nilai kredit. **Tepat satu sisi terisi**, sama seperti `AccJournalLine` |
| `Description` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Keterangan baris |

## 15. `AccRecurringJournalRun` — status `Baru`

Bukti bahwa satu template sudah terbit untuk satu periode. **Tabel terpenting bagi jurnal berulang.**

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `TemplateId` | `Guid` | Ya | — | **Unique bersama `AccountingPeriodId`** | FK ke `AccRecurringJournalTemplate` | `Restrict` | Tidak | Template yang terbit |
| `AccountingPeriodId` | `Guid` | Ya | — | **Unique bersama `TemplateId`** | FK ke `AccAccountingPeriod` | `Restrict` | Tidak | Periode tujuan |
| `JournalId` | `Guid` | Ya | — | Index | FK ke `AccJournal` | `Restrict` | Tidak | Jurnal draft yang dihasilkan |
| `GeneratedAt` | `timestamptz` | Ya | — | — | — | — | Tidak | Waktu penerbitan |

Unique `(TemplateId, AccountingPeriodId)` adalah **satu-satunya** hal yang mencegah penyusutan
September tercatat dua kali ketika penjadwal berjalan dua kali. Pemeriksaan di kode C# tidak
cukup: dua proses bersamaan dapat sama-sama lolos di kode, tetapi tidak dapat sama-sama lolos
dari database.

## 16. `AccPeriodClosingApproval` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `AccountingPeriodId` | `Guid` | Ya | — | Unique bersama `ActionSequence` | FK ke `AccAccountingPeriod` | `Cascade` | Tidak | Periode yang ditutup |
| `ActionSequence` | `int` | Ya | — | Unique bersama `AccountingPeriodId` | — | — | Tidak | Nomor urut tindakan |
| `Action` | `int` | Ya | — | — | — | — | Tidak | Enum `PeriodClosingAction`: Diajukan, Disetujui, Ditolak |
| `ActionBy` | `Guid` | Ya | — | Index | FK ke pengguna | `Restrict` | Tidak | Pelaku tindakan |
| `ActionAt` | `timestamptz` | Ya | — | — | — | — | Tidak | Waktu tindakan |
| `ActionNote` | `string(500)?` | Tidak | — | — | — | — | Tidak | **Wajib** bila tindakannya Ditolak |

Meniru bentuk `AccJournalApproval` yang sudah ada, dan alasannya sama: ini **data bisnis** yang
ditampilkan ke pengguna, bukan log teknis.

## 17. `AccAccountingConfiguration` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `LegalEntityId` | `Guid` | Ya | — | **Unique** | FK ke `MstLegalEntity` | `Restrict` | Tidak | Satu pengaturan per badan hukum |
| `RetainedEarningsAccountId` | `Guid` | Ya | — | Index | FK ke `AccChartOfAccount` | `Restrict` | Tidak | Akun laba ditahan (`ACC-DEC-054`). Wajib berjenis `Equity` dan menerima transaksi |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

**Kenapa tabel tersendiri, bukan kolom penanda pada `AccChartOfAccount`.** Akun laba ditahan tidak
dapat diturunkan dari data lain — `AccountType == Equity` saja tidak cukup karena akun ekuitas bisa
lebih dari satu. Menaruhnya sebagai penanda pada daftar akun akan menuntut penjagaan "hanya boleh
satu yang bertanda per badan hukum", yang justru lebih rumit daripada satu baris pengaturan.

## 18. `AccAccountingPeriod` — status `Diperbarui`

Hanya kolom yang **berubah** yang ditulis. Kolom lainnya tetap seperti bagian MVP di atas.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `PeriodStatus` | `int` | Ya | `1` | Index | — | — | Tidak | **Diperbarui.** Enum bertambah `PendingClosingApproval = 4`, ditambahkan **di belakang**. Kolomnya tidak berubah tipe |
| `ClosingSubmittedBy` | `Guid?` | Tidak | — | — | FK ke pengguna | `Restrict` | Tidak | **Baru.** Pengaju penutupan. Dipakai menegakkan "penyetuju bukan pengaju" |
| `ClosingSubmittedAt` | `timestamptz?` | Tidak | — | — | — | — | Tidak | **Baru.** Waktu pengajuan |

Kedua kolom baru **boleh kosong**, sehingga migration-nya dapat dijalankan tanpa mematikan
layanan dan tanpa mengisi data lama.

## 19. `AccJournalType` — status `Sudah ada`, hanya bertambah data

**Nol kolom berubah.** Yang bertambah adalah **satu baris data**:

| `JournalTypeCode` | `JournalTypeName` | `NumberPrefix` | `RequiresApproval` | `IsSystemType` |
|---|---|---|:---:|:---:|
| `JT` | Jurnal Tutup Tahun | `JT` | `true` | `true` |

Ditambahkan lewat seeder yang sudah ada, bukan lewat migration data.
