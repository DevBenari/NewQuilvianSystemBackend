# Kamus Data — Modul Rekam Medis

| Field | Value |
|---|---|
| Blueprint ID | `RM-BP-001` |
| Revision | `1` |
| Status | `draft` |
| Backend SHA | `ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e` |

> **PERINGATAN DASAR DESAIN.** Disusun di atas keputusan berstatus `draft`. Lihat `RM-DEC-025`.

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu tidak diulang pada tabel di bawah.

Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris. Khusus
`TrxMedicalRecordAccessLog` berlaku aturan tambahan: penandaan hapus **tidak boleh** dipakai
sama sekali, karena jejak yang dapat dihapus bukan jejak.

Kolom bertanda **Sensitif = Ya** tidak boleh masuk `LoggerService`, tidak boleh dipakai sebagai
contoh berisi data asli, dan perlu ditinjau kebutuhan penyamarannya pada response.

---

## 1. `TrxClinicalDocumentIntegrity` — status `Baru`

Menyimpan keadaan keutuhan satu dokumen klinis. Tidak memuat isi klinis apa pun.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `DocumentKind` | `ClinicalDocumentKind` | Ya | — | Unik bersama `DocumentId` | — | — | Tidak | Jenis dokumen. Enum disimpan sebagai `int` |
| `DocumentId` | `Guid` | Ya | — | Unik bersama `DocumentKind` | Rujukan polimorfik, **bukan** FK | — | Tidak | `Id` pada tabel dokumen yang bersangkutan |
| `PatientId` | `Guid` | Ya | — | Index | FK ke `MstPatient` | `Restrict` | Tidak | Pemilik berkas |
| `EncounterId` | `Guid` | Ya | — | Index | FK ke `TrxPatientEncounter` | `Restrict` | Tidak | Kunjungan yang menaungi dokumen |
| `IntegrityStatus` | `ClinicalDocumentIntegrityStatus` | Ya | `Draft` | Index | — | — | Tidak | Status keutuhan. Enum sebagai `int` |
| `AuthorUserId` | `Guid` | Ya | — | Index | FK ke `ApplicationUser` | `Restrict` | Tidak | Penulis dokumen. **Tidak pernah boleh berubah setelah baris dibuat** |
| `IsAuthorKnown` | `bool` | Ya | `true` | — | — | — | Tidak | Bernilai salah untuk baris hasil pengisian data lama yang penulisnya tidak tercatat |
| `SignedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu penandatanganan. Kosong bila belum ditandatangani |
| `SignedByUserId` | `Guid?` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Selalu sama dengan `AuthorUserId`. Disimpan terpisah agar terbaca eksplisit |
| `SignatureDeviceInfo` | `string(250)` | Tidak | — | — | — | — | Tidak | Peramban dan perangkat saat menandatangani, sesuai `RM-DEC-021` |
| `SignatureIpAddress` | `string(64)` | Tidak | — | — | — | — | Tidak | Alamat IP saat menandatangani |
| `LockedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu terkunci |
| `LockTrigger` | `ClinicalDocumentLockTrigger?` | Tidak | — | — | — | — | Tidak | Sebab terkunci. Enum sebagai `int` |
| `LockedEncounterClosedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu kunjungan ditutup, bila penguncian dipicu penutupan |
| `CancelledReason` | `string(250)` | Tidak | — | — | — | — | **Ya** | Alasan pembatalan dokumen, dapat memuat keterangan klinis |
| `AddendumCount` | `int` | Ya | `0` | — | — | — | Tidak | Jumlah addendum. Disimpan agar daftar tidak perlu menghitung ulang |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Mengikuti konvensi project |

Index gabungan:

| Index | Kolom |
|---|---|
| Unik | `(DocumentKind, DocumentId)` |
| Pencarian per pasien | `(PatientId, IntegrityStatus, IsDelete)` |
| Penguncian saat kunjungan ditutup | `(EncounterId, IntegrityStatus, IsDelete)` |
| Daftar tugas penulis | `(AuthorUserId, IntegrityStatus, IsDelete)` |

---

## 2. `TrxClinicalNoteAddendum` — status `Baru`

Koreksi atau tambahan pada dokumen yang sudah terkunci. Tidak pernah menimpa isi lama.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `IntegrityId` | `Guid` | Ya | — | Unik bersama `Sequence` | FK ke `TrxClinicalDocumentIntegrity` | `Restrict` | Tidak | Dokumen yang dikoreksi |
| `Sequence` | `int` | Ya | — | Unik bersama `IntegrityId` | — | — | Tidak | Urutan koreksi, dimulai dari 1 |
| `AuthorUserId` | `Guid` | Ya | — | Index | FK ke `ApplicationUser` | `Restrict` | Tidak | Pembuat addendum. Bila pengganti, ini nama penggantinya, bukan penulis asli |
| `IsSubstituteAuthor` | `bool` | Ya | `false` | — | — | — | Tidak | Benar hanya bila `DelegationId` terisi |
| `DelegationId` | `Guid?` | Tidak | — | Index | FK ke `TrxClinicalNoteAuthorDelegation` | `Restrict` | Tidak | Dasar kewenangan pengganti |
| `AddendumText` | `string(4000)` | Ya | — | — | — | — | **Ya** | Isi koreksi. Data klinis |
| `CorrectionReason` | `string(500)` | Ya | — | — | — | — | **Ya** | Alasan koreksi. Wajib diisi tanpa kecuali |
| `SignedAt` | `DateTime` | Ya | `DateTime.UtcNow` | — | — | — | Tidak | Addendum selalu final saat dibuat; tidak ada tahap draft |
| `SignatureDeviceInfo` | `string(250)` | Tidak | — | — | — | — | Tidak | Perangkat saat membuat addendum |
| `SignatureIpAddress` | `string(64)` | Tidak | — | — | — | — | Tidak | Alamat IP saat membuat addendum |

Aturan yang tidak biasa: **addendum tidak dapat diubah maupun dibatalkan.** Koreksi atas
addendum dibuat sebagai addendum berikutnya dengan `Sequence` lebih tinggi. Karena itu tidak
ada kolom status pada tabel ini — addendum hanya punya satu keadaan, yaitu ada.

---

## 3. `TrxClinicalNoteAuthorDelegation` — status `Baru`

Mencatat bahwa seorang penulis dinyatakan berhalangan. Menjawab `RM-DEC-020`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `OriginalAuthorUserId` | `Guid` | Ya | — | Index | FK ke `ApplicationUser` | `Restrict` | Tidak | Penulis yang berhalangan |
| `Trigger` | `AuthorDelegationTrigger` | Ya | — | Index | — | — | Tidak | Sebab berhalangan. Enum sebagai `int` |
| `GrantedByUserId` | `Guid?` | Tidak | — | Index | FK ke `ApplicationUser` | `Restrict` | Tidak | Pemberi penetapan. Kosong bila sebabnya akun nonaktif |
| `GrantReason` | `string(500)` | Tidak | — | — | — | — | Tidak | **Wajib** bila `Trigger = UnitHeadGrant`; kosong bila akun nonaktif |
| `ValidFrom` | `DateTime` | Ya | `DateTime.UtcNow` | — | — | — | Tidak | Mulai berlaku |
| `ValidUntil` | `DateTime?` | Tidak | — | Index | — | — | Tidak | **Wajib** bila `Trigger = UnitHeadGrant`. Penetapan manual tanpa batas waktu dilarang |
| `RevokedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu penetapan dicabut lebih awal |
| `RevokedByUserId` | `Guid?` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Pencabut penetapan |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Mengikuti konvensi project |

Alasan `ValidUntil` diwajibkan untuk penetapan manual: penetapan tanpa batas waktu adalah pintu
belakang permanen. Bila kepala unit membuka jalur pengganti sekali lalu lupa menutupnya,
catatan penulis itu selamanya dapat dikoreksi orang lain — dan itu menghapus makna `RM-DEC-004`.

---

## 4. `TrxMedicalRecordAccessLog` — status `Baru`

Satu baris untuk setiap pembukaan berkas rekam medis.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PatientId` | `Guid` | Ya | — | Index | FK ke `MstPatient` | `Restrict` | Tidak | Pemilik berkas yang dibuka |
| `UserId` | `Guid` | Ya | — | Index | FK ke `ApplicationUser` | `Restrict` | Tidak | Pengguna yang membuka |
| `UserDisplayNameSnapshot` | `string(200)` | Ya | — | — | — | — | Tidak | Nama pengguna saat itu. Disimpan agar jejak tetap terbaca bila akun kelak dihapus |
| `UserRoleSnapshot` | `string(150)` | Tidak | — | — | — | — | Tidak | Peran pengguna saat itu |
| `AccessType` | `MedicalRecordAccessType` | Ya | — | Index | — | — | Tidak | Rawatan atau beralasan. Enum sebagai `int` |
| `AccessScope` | `MedicalRecordAccessScope` | Ya | — | Index | — | — | Tidak | Ringkasan, riwayat, atau catatan pribadi. Enum sebagai `int` |
| `AccessPurposeId` | `Guid?` | Tidak | — | Index | FK ke `MstMedicalRecordAccessPurpose` | `Restrict` | Tidak | Keperluan akses. **Wajib** bila `AccessType = ReasonedAccess` |
| `AccessReason` | `string(500)` | Tidak | — | — | — | — | **Ya** | Alasan bebas. **Wajib** bila keperluan yang dipilih menuntutnya |
| `HasActiveEncounter` | `bool` | Ya | — | — | — | — | Tidak | Hasil penilaian saat itu. Disimpan agar keputusan sistem dapat ditelusuri kemudian |
| `IsFlaggedForReview` | `bool` | Ya | `false` | Index | — | — | Tidak | Menandai baris yang perlu ditinjau unit rekam medis |
| `ReviewedAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Waktu ditinjau |
| `ReviewedByUserId` | `Guid?` | Tidak | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Petugas rekam medis yang meninjau |
| `ReviewNote` | `string(500)` | Tidak | — | — | — | — | Tidak | Catatan hasil tinjauan |
| `AccessedAt` | `DateTime` | Ya | `DateTime.UtcNow` | Index | — | — | Tidak | Waktu pembukaan |
| `IpAddress` | `string(64)` | Tidak | — | — | — | — | Tidak | Alamat IP pengakses |
| `ClientInfo` | `string(250)` | Tidak | — | — | — | — | Tidak | Peramban, sistem operasi, dan perangkat |
| `RequestPath` | `string(250)` | Tidak | — | — | — | — | Tidak | Path permintaan, untuk penelusuran teknis |

Index gabungan:

| Index | Kolom | Menjawab pertanyaan |
|---|---|---|
| Per pasien | `(PatientId, AccessedAt)` | Siapa saja membuka rekam medis pasien ini |
| Per pengguna | `(UserId, AccessedAt)` | Apa saja yang dibuka pengguna ini |
| Antrean tinjauan | `(IsFlaggedForReview, ReviewedAt, AccessedAt)` | Akses mana yang belum ditinjau |
| Laporan jenis akses | `(AccessType, AccessedAt)` | Perbandingan akses rawatan dan beralasan |

Kolom `UserDisplayNameSnapshot` sengaja menyimpan salinan nama. Ini satu-satunya tempat di
seluruh blueprint yang menyalin data milik modul lain, dan alasannya khusus: jejak akses harus
tetap terbaca puluhan tahun kemudian, sementara akun pengguna bisa berubah nama atau dihapus.
Menyimpan `UserId` saja membuat jejak lama menjadi tidak terbaca.

---

## 5. `MstMedicalRecordAccessPurpose` — status `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PurposeCode` | `string(50)` | Ya | — | Unik | — | — | Tidak | Kode keperluan, misalnya `CROSS_UNIT` |
| `PurposeName` | `string(150)` | Ya | — | — | — | — | Tidak | Nama yang dibaca pengguna |
| `IsFreeTextRequired` | `bool` | Ya | `false` | — | — | — | Tidak | Bila benar, pengguna wajib menuliskan alasan sendiri |
| `RequiresReview` | `bool` | Ya | `true` | — | — | — | Tidak | Bila benar, akses dengan keperluan ini masuk antrean tinjauan |
| `SortOrder` | `int` | Ya | `0` | — | — | — | Tidak | Urutan tampil |
| `Description` | `string(250)` | Tidak | — | — | — | — | Tidak | Keterangan tambahan |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Mengikuti konvensi project |

---

## 6. Enum

| Enum | Nilai | Bawaan |
|---|---|---|
| `ClinicalDocumentKind` | `ProgressNote = 1`, `Consultation = 2`, `Assessment = 3`, `Diagnosis = 4`, `Procedure = 5`, `VitalSign = 6`, `Allergy = 7`, `MedicalHistory = 8`, `FamilyHistory = 9`, `ClinicalDocument = 10`, `NoteAttachment = 11`, `MedicalCertificate = 12`, `Consent = 13` | — |
| `ClinicalDocumentIntegrityStatus` | `Draft = 1`, `Signed = 2`, `LockedUnsigned = 3`, `Cancelled = 4` | `Draft` |
| `ClinicalDocumentLockTrigger` | `AuthorSigned = 1`, `EncounterClosed = 2`, `BackfillEncounterClosed = 3`, `DocumentCancelled = 4` | — |
| `AuthorDelegationTrigger` | `InactiveAccount = 1`, `UnitHeadGrant = 2` | — |
| `MedicalRecordAccessType` | `RoutineCare = 1`, `ReasonedAccess = 2` | — |
| `MedicalRecordAccessScope` | `Summary = 1`, `Timeline = 2`, `DocumentDetail = 3`, `PrivateNote = 4` | — |

Catatan penting tentang `ClinicalDocumentKind`. Tiga belas nilai didaftarkan sekaligus supaya
nomornya stabil sejak awal dan tidak bergeser di kemudian hari. Namun **rilis pertama hanya
menegakkan aturan keutuhan untuk `ProgressNote`**, sesuai `RM-DEC-019`. Dua belas nilai lain
sudah punya tempat, tetapi belum dipakai. Ini disebut terbuka agar tidak ada yang mengira
seluruh jenis dokumen sudah terlindungi.

---

## 7. Tabel `Sudah ada` yang dipakai modul ini

Hanya kolom kunci yang ditulis. Sumber lengkapnya ada pada file model masing-masing.

### `MstPatient`

Sumber lengkap: `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs`

| Kolom | Dipakai untuk |
|---|---|
| `Id` | Kunci berkas rekam medis |
| `MedicalRecordNumber` | Nomor rekam medis yang ditampilkan. Sudah dijamin unik |
| `PatientStatus` | Menandai pasien tidak aktif pada tampilan |
| `MergedToPatientId` | **Perhatian.** Menandai pasien hasil penggabungan. Alur kerjanya belum ditemukan di controller mana pun — lihat `RM-CAP-007`, masih `Unknown` |

### `TrxPatientEncounter`

Sumber lengkap: `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs`

| Kolom | Dipakai untuk |
|---|---|
| `Id` | Pengelompokan dokumen per kunjungan |
| `PatientId` | Penghubung ke pasien |
| `EncounterStatus` | Menentukan kunjungan aktif, sesuai `RM-DEC-016` |
| `CompletedAt` | Pemicu penguncian, sesuai `RM-DEC-003` lapis kedua |
| `EncounterType` | Pengelompokan pada tampilan riwayat |

### `TrxPatientIntegratedProgressNote`

Sumber lengkap: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs`

| Kolom | Dipakai untuk |
|---|---|
| `Id` | Menjadi `DocumentId` pada baris keutuhan |
| `PatientId`, `EncounterId` | Disalin ke baris keutuhan saat pendaftaran |
| `ProviderUserId` | Menjadi `AuthorUserId` saat pendaftaran. **Setelah itu tidak pernah dibaca lagi sebagai penentu penulis** |
| `NoteDateTime` | Pengurutan pada riwayat |
| `PrivateNote` | Hanya dikembalikan lewat jalur akses beralasan, sesuai `RM-DEC-022` |
| `IsCancel` | Menentukan status keutuhan `Cancelled` |

Baris ketiga menjelaskan cara `RM-CAP-012` ditutup. `ProviderUserId` pada tabel klinis masih
dapat berubah sepanjang kode lama belum diperbaiki, tetapi setelah baris keutuhan dibuat,
**penentu penulis yang sah adalah `AuthorUserId` pada tabel keutuhan**, yang tidak dapat
disentuh permintaan ubah dokumen. Perbaikan pada controller tetap dilakukan, dan tabel keutuhan
menjadi lapis kedua yang menutup celah itu.

---

## 8. Skema tabel dalam bentuk DDL

> **PERINGATAN.** Basis data project ini dibentuk EF Core Migrations, bukan skrip SQL manual.
> DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip untuk dijalankan. Menjalankannya
> akan berbenturan dengan migration. Sumber kebenarannya adalah file configuration di
> `Repositories/Configurations/HealthService/MedicalRecordManagement/`.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
-- Kolom audit IdentityModel tidak ditulis ulang di sini.

CREATE TABLE public."TrxClinicalDocumentIntegrity" (
    "Id"                      uuid          NOT NULL,
    "DocumentKind"            integer       NOT NULL,   -- enum, HasConversion<int>
    "DocumentId"              uuid          NOT NULL,   -- rujukan polimorfik, bukan FK
    "PatientId"               uuid          NOT NULL,
    "EncounterId"             uuid          NOT NULL,
    "IntegrityStatus"         integer       NOT NULL,   -- enum, HasConversion<int>
    "AuthorUserId"            uuid          NOT NULL,
    "IsAuthorKnown"           boolean       NOT NULL DEFAULT true,
    "SignedAt"                timestamp,
    "SignedByUserId"          uuid,
    "SignatureDeviceInfo"     varchar(250),
    "SignatureIpAddress"      varchar(64),
    "LockedAt"                timestamp,
    "LockTrigger"             integer,                  -- enum, HasConversion<int>
    "LockedEncounterClosedAt" timestamp,
    "CancelledReason"         varchar(250),             -- SENSITIF
    "AddendumCount"           integer       NOT NULL DEFAULT 0,
    "IsActive"                boolean       NOT NULL DEFAULT true,

    CONSTRAINT "PK_TrxClinicalDocumentIntegrity" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TrxClinicalDocumentIntegrity_MstPatient_PatientId"
        FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_TrxClinicalDocumentIntegrity_TrxPatientEncounter_EncounterId"
        FOREIGN KEY ("EncounterId") REFERENCES public."TrxPatientEncounter" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_TrxClinicalDocumentIntegrity_DocumentKind_DocumentId"
    ON public."TrxClinicalDocumentIntegrity" ("DocumentKind", "DocumentId");

CREATE INDEX "IX_TrxClinicalDocumentIntegrity_PatientId_IntegrityStatus_IsDelete"
    ON public."TrxClinicalDocumentIntegrity" ("PatientId", "IntegrityStatus", "IsDelete");

CREATE INDEX "IX_TrxClinicalDocumentIntegrity_EncounterId_IntegrityStatus_IsDelete"
    ON public."TrxClinicalDocumentIntegrity" ("EncounterId", "IntegrityStatus", "IsDelete");

CREATE INDEX "IX_TrxClinicalDocumentIntegrity_AuthorUserId_IntegrityStatus_IsDelete"
    ON public."TrxClinicalDocumentIntegrity" ("AuthorUserId", "IntegrityStatus", "IsDelete");
```

```sql
CREATE TABLE public."TrxClinicalNoteAddendum" (
    "Id"                  uuid          NOT NULL,
    "IntegrityId"         uuid          NOT NULL,
    "Sequence"            integer       NOT NULL,
    "AuthorUserId"        uuid          NOT NULL,
    "IsSubstituteAuthor"  boolean       NOT NULL DEFAULT false,
    "DelegationId"        uuid,
    "AddendumText"        varchar(4000) NOT NULL,       -- SENSITIF
    "CorrectionReason"    varchar(500)  NOT NULL,       -- SENSITIF
    "SignedAt"            timestamp     NOT NULL,
    "SignatureDeviceInfo" varchar(250),
    "SignatureIpAddress"  varchar(64),

    CONSTRAINT "PK_TrxClinicalNoteAddendum" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TrxClinicalNoteAddendum_TrxClinicalDocumentIntegrity_IntegrityId"
        FOREIGN KEY ("IntegrityId")
        REFERENCES public."TrxClinicalDocumentIntegrity" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_TrxClinicalNoteAddendum_TrxClinicalNoteAuthorDelegation_DelegationId"
        FOREIGN KEY ("DelegationId")
        REFERENCES public."TrxClinicalNoteAuthorDelegation" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_TrxClinicalNoteAddendum_IntegrityId_Sequence"
    ON public."TrxClinicalNoteAddendum" ("IntegrityId", "Sequence");
```

```sql
CREATE TABLE public."TrxClinicalNoteAuthorDelegation" (
    "Id"                   uuid         NOT NULL,
    "OriginalAuthorUserId" uuid         NOT NULL,
    "Trigger"              integer      NOT NULL,       -- enum, HasConversion<int>
    "GrantedByUserId"      uuid,
    "GrantReason"          varchar(500),
    "ValidFrom"            timestamp    NOT NULL,
    "ValidUntil"           timestamp,
    "RevokedAt"            timestamp,
    "RevokedByUserId"      uuid,
    "IsActive"             boolean      NOT NULL DEFAULT true,

    CONSTRAINT "PK_TrxClinicalNoteAuthorDelegation" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_TrxClinicalNoteAuthorDelegation_OriginalAuthorUserId_IsActive_ValidUntil"
    ON public."TrxClinicalNoteAuthorDelegation" ("OriginalAuthorUserId", "IsActive", "ValidUntil");
```

```sql
CREATE TABLE public."TrxMedicalRecordAccessLog" (
    "Id"                      uuid         NOT NULL,
    "PatientId"               uuid         NOT NULL,
    "UserId"                  uuid         NOT NULL,
    "UserDisplayNameSnapshot" varchar(200) NOT NULL,
    "UserRoleSnapshot"        varchar(150),
    "AccessType"              integer      NOT NULL,    -- enum, HasConversion<int>
    "AccessScope"             integer      NOT NULL,    -- enum, HasConversion<int>
    "AccessPurposeId"         uuid,
    "AccessReason"            varchar(500),             -- SENSITIF
    "HasActiveEncounter"      boolean      NOT NULL,
    "IsFlaggedForReview"      boolean      NOT NULL DEFAULT false,
    "ReviewedAt"              timestamp,
    "ReviewedByUserId"        uuid,
    "ReviewNote"              varchar(500),
    "AccessedAt"              timestamp    NOT NULL,
    "IpAddress"               varchar(64),
    "ClientInfo"              varchar(250),
    "RequestPath"             varchar(250),

    CONSTRAINT "PK_TrxMedicalRecordAccessLog" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TrxMedicalRecordAccessLog_MstPatient_PatientId"
        FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_TrxMedicalRecordAccessLog_MstMedicalRecordAccessPurpose_AccessPurposeId"
        FOREIGN KEY ("AccessPurposeId")
        REFERENCES public."MstMedicalRecordAccessPurpose" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_TrxMedicalRecordAccessLog_PatientId_AccessedAt"
    ON public."TrxMedicalRecordAccessLog" ("PatientId", "AccessedAt");

CREATE INDEX "IX_TrxMedicalRecordAccessLog_UserId_AccessedAt"
    ON public."TrxMedicalRecordAccessLog" ("UserId", "AccessedAt");

CREATE INDEX "IX_TrxMedicalRecordAccessLog_IsFlaggedForReview_ReviewedAt_AccessedAt"
    ON public."TrxMedicalRecordAccessLog" ("IsFlaggedForReview", "ReviewedAt", "AccessedAt");

CREATE INDEX "IX_TrxMedicalRecordAccessLog_AccessType_AccessedAt"
    ON public."TrxMedicalRecordAccessLog" ("AccessType", "AccessedAt");
```

```sql
CREATE TABLE public."MstMedicalRecordAccessPurpose" (
    "Id"                 uuid         NOT NULL,
    "PurposeCode"        varchar(50)  NOT NULL,
    "PurposeName"        varchar(150) NOT NULL,
    "IsFreeTextRequired" boolean      NOT NULL DEFAULT false,
    "RequiresReview"     boolean      NOT NULL DEFAULT true,
    "SortOrder"          integer      NOT NULL DEFAULT 0,
    "Description"        varchar(250),
    "IsActive"           boolean      NOT NULL DEFAULT true,

    CONSTRAINT "PK_MstMedicalRecordAccessPurpose" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_MstMedicalRecordAccessPurpose_PurposeCode"
    ON public."MstMedicalRecordAccessPurpose" ("PurposeCode");
```

### Pembagian tabel `TrxMedicalRecordAccessLog`

Masa simpan ditetapkan **25 tahun** pada `RM-DEC-024`, sehingga tabel jejak akses dibuat
sebagai tabel terbagi per tahun berdasarkan `AccessedAt`.

```sql
-- Bentuk tabel terbagi. Bukan skrip untuk dijalankan; EF Core Migrations yang membentuknya.
-- Kolom sama persis dengan definisi TrxMedicalRecordAccessLog di atas.

CREATE TABLE public."TrxMedicalRecordAccessLog" (
    -- kolom sama seperti definisi di atas
    "AccessedAt" timestamp NOT NULL
) PARTITION BY RANGE ("AccessedAt");

CREATE TABLE public."TrxMedicalRecordAccessLog_2026"
    PARTITION OF public."TrxMedicalRecordAccessLog"
    FOR VALUES FROM ('2026-01-01') TO ('2027-01-01');

-- dan seterusnya, satu bagian per tahun
```

Tiga hal yang perlu diperhatikan implementer:

| Hal | Ketetapan |
|---|---|
| Primary key pada tabel terbagi | PostgreSQL mensyaratkan kolom pembagi ikut menjadi bagian primary key, sehingga kuncinya menjadi `("Id", "AccessedAt")`, bukan `"Id"` saja |
| Pembuatan bagian baru | Dijadwalkan otomatis menjelang pergantian tahun. Bagian yang belum dibuat menyebabkan penyisipan gagal, dan itu berarti rekam medis tidak dapat dibuka |
| Penghapusan bagian tertua | Lewat proses pengarsipan resmi, bukan lewat penandaan `IsDelete` |

Baris kedua adalah risiko operasional nyata: karena "gagal mencatat jejak berarti gagal
membaca", bagian tahun yang lupa dibuat akan **menghentikan pembacaan rekam medis** pada 1
Januari. Penjadwalannya wajib dibuat otomatis dan dipantau, bukan diserahkan pada ingatan
seseorang.

Perkiraan volume beserta dasar perhitungannya ada pada
[jejak-akses.md](jejak-akses.md) bagian 4.
