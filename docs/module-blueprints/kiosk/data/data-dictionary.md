# Kamus Data — Modul Kiosk

| Field | Nilai |
| --- | --- |
| Blueprint ID | `KSK-BP-001` r1 |
| Status | `approved` — Sukma Giri Pratama, 30 Sep 2026 |
| `input_revision` | `02-backend-architecture.md` r1 |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu tidak diulang di bawah. Penghapusan bersifat penandaan melalui `IsDelete`, bukan penghapusan baris.

> **Revisi Kiosk tidak menambah atau mengubah tabel apa pun.** Semua tabel di bawah berstatus `Sudah ada`, sehingga hanya kolom yang dipakai aturan bisnis Kiosk yang dicatat, ditambah rujukan ke file model. Karena itu **tidak ada bagian DDL**: DDL hanya ditulis untuk tabel `Baru` atau `Diperbarui`.

## 1. Status dan kepemilikan tabel

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `MstPatient` | Sudah ada | PatientManagement | Dibaca Cek No. RM; **MUST NOT** disalin atau diubah |
| `MstPatientIdentityDocument` | Sudah ada | PatientManagement | Dibaca untuk pencocokan KTP cadangan |
| `MstPatientInsurance` | Sudah ada | PatientManagement | Dibaca: kartu asuransi (Step 1), kandidat penjamin |
| `MstPatientMembership` | Sudah ada | PatientManagement | Dibaca: nomor member (Step 1) |
| `MstPatientCompanyGuarantor` | Sudah ada | PatientManagement | Dibaca: kandidat penjamin perusahaan |
| `RegPatientEncounter` | Sudah ada | RegistrationManagement | Dibuat lewat route kiosk existing |
| `RegPatientEncounterGuarantor` | Sudah ada | RegistrationManagement | Satu per kunjungan; kini boleh berisi perusahaan dari Kiosk |
| `TrxKioskScanSession` | Sudah ada (legacy `Trx*`) | RegistrationManagement | Dibuat sekali di Step 3 |

## 2. Kolom yang dipakai aturan Kiosk

### `MstPatient` — `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs`

| Kolom | Tipe | Wajib | Index | Sensitif | Dipakai untuk |
| --- | --- | :---: | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | Tidak | `patientId` di respons lookup |
| `MedicalRecordNumber` | `string(50)` | Ya | Unique | Tidak | Kartu Pasien |
| `PatientCode` | `string(50)` | Ya | Unique | Tidak | Kartu Pasien |
| `FullName` | `string(200)` | Ya | Index | **Ya** | Kartu Pasien (tidak masuk log) |
| `IdentityNumber` | `string(50)` | Tidak | Unique, filter `IdentityNumber IS NOT NULL AND IsDelete = false` | **Ya** | Pencocokan KTP |
| `PhoneNumber` | `string(30)` | Tidak | Index (kolom mentah) | **Ya** | Pencocokan HP setelah dinormalkan di query |
| `PatientStatus` | `PatientStatus` (int) | Ya | — | Tidak | Hanya `Active (1)` yang dianggap ditemukan |
| `IsActive` | `bool` | Ya | — | Tidak | Harus `true` |
| `IsDeceased` | `bool` | Ya | — | **Ya** | Harus `false`; **tidak pernah** disebut di layar |
| `MergedToPatientId` | `Guid?` | Tidak | FK ke `MstPatient`, `Restrict` | Tidak | Diikuti ke pasien tujuan (maks. 3 langkah) |
| `PatientType`, `Gender`, `BloodType` | enum | — | — | `BloodType`: **Ya** | Label Kartu Pasien |

### `MstPatientIdentityDocument` — `.../MasterData/Models/MstPatientIdentityDocument.cs`

| Kolom | Dipakai untuk |
| --- | --- |
| `PatientId` (FK), `IdentityNumber` (**Sensitif**), `IsActive` | Pencocokan KTP cadangan, sama dengan `FindPatientAsync` |

### `MstPatientInsurance` — `.../MasterData/Models/MstPatientInsurance.cs`

| Kolom | Dipakai untuk |
| --- | --- |
| `PatientId` (FK), `CardNumber` (**Sensitif**), `IsActive` | `searchType = 3` di Step 1; daftar kandidat penjamin |

### `MstPatientMembership` — `.../MasterData/Models/MstPatientMembership.cs`

| Kolom | Dipakai untuk |
| --- | --- |
| `PatientId` (FK), `MemberNumber` (**Sensitif**), `IsActive` | `searchType = 4` di Step 1 |

### `RegPatientEncounterGuarantor` — `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs`

| Kolom | Dipakai untuk |
| --- | --- |
| `EncounterId` (FK), `PaymentType`, `PatientInsuranceId`, `PatientCompanyGuarantorId`, `IsPrimary` | Satu penanggung per kunjungan (`KSK-FACT-004`). Diisi `CreateEncounterCoreAsync` existing |

### `TrxKioskScanSession` — `Areas/HealthServices/RegistrationManagement/Models/TrxKioskScanSession.cs`

| Kolom | Dipakai untuk |
| --- | --- |
| `Id`, `PatientId`, `TargetService` (null untuk poliklinik, `2` untuk Laboratorium), `HasPhysicianRequest`, `Status` | Dibentuk sekali di Step 3; ditempel ke kunjungan poliklinik lewat `KioskScanSessionId` |

## 3. Data yang tidak disimpan

| Data | Letak hidupnya | Dibuang kapan |
| --- | --- | --- |
| Nilai KTP/HP yang diketik di Cek No. RM | Memori layar + body request | Saat pindah halaman, `SESSION_CLEARED`, atau hasil baru |
| `patientId` untuk handoff Cek No. RM → Pasien Lama | Redux in-memory (tanpa persistence) | Begitu dibaca halaman Pasien Lama, atau saat `SESSION_CLEARED` |
| Hasil pindai kartu Step 1 | Memori layar | Setelah dikirim ke `scan-result` di Step 3, atau `SESSION_CLEARED` |


---

# Amendment PO — Pendaftaran Pasien Online (`KSK-CONTRACT-v2`, `approved` 2026-11-19)

Seluruh tabel mewarisi `IdentityModel` (kolom audit `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, `IsDelete`). Kolom-kolom itu tidak diulang di bawah. Penghapusan bersifat penandaan.

Kolom bertanda **Sensitif = Ya** tidak boleh masuk custom logger, tidak boleh dipakai sebagai contoh berisi data asli, dan tidak boleh keluar lewat endpoint publik kecuali dalam bentuk tersamar.

## PO-D.1 Status dan kepemilikan tabel

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `RegOnlineRegistration` | **Baru** | Registration | Aggregate root pendaftaran online (`KSK-DSN-PO-001`) |
| `RegOnlineRegistrationReferralDocument` | **Baru** | Registration | Berkas surat rujukan yang diunggah pasien |
| `RegOnlineRegistrationHistory` | **Baru** | Registration | Riwayat kejadian, hanya tambah |
| `RegPatientEncounter` | Sudah ada | Registration | Dirujuk lewat `EncounterId`. `RegistrationSource` menerima `7 = OnlineRegistration` |
| `RegPatientGeneralConsent` | Sudah ada | Registration | Ditulis saat check-in dari consent yang ditampung |
| `RegEncounterReferral`, `RegEncounterReferralDocument` | Sudah ada | Registration | Dibuat jalur kunjungan saat check-in. `CaptureSource` menerima `3 = OnlineRegistration` |
| `MstPatient`, `MstPatientInsurance`, `MstPatientCompanyGuarantor` | Sudah ada | Patient Management | Dirujuk, **tidak disalin** |
| `MstClinic`, `MstDoctorSchedule`, `MstReferralInstitution` | Sudah ada | Master Data | Dirujuk |
| `MstInsuranceProvider` | Sudah ada | Administrator Master Data | Dirujuk untuk kartu asuransi baru |

## PO-D.2 `RegOnlineRegistration` — Baru

`Areas/HealthServices/RegistrationManagement/Models/RegOnlineRegistration.cs`, tabel `public."RegOnlineRegistration"`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `RegistrationNumber` | `string(30)` | Ya | — | Unique | — | — | Tidak | `PO-yyMMdd-nnnn`, dari `NumberSeriesAllocator` |
| `CodeHash` | `string(64)` | Ya | — | Unique bersyarat `Status = 1` | — | — | **Ya** | SHA-256 hex kode yang dinormalkan |
| `Status` | `int` (`OnlineRegistrationStatus`) | Ya | `1` | Index bersama `VisitDate` | — | — | Tidak | 1 Menunggu Check-in, 2 Menjadi Kunjungan, 3 Didaftarkan lewat Jalur Lain, 4 Tidak Datang, 5 Dibatalkan |
| `PatientMode` | `int` (`OnlineRegistrationPatientMode`) | Ya | — | — | — | — | Tidak | 1 pasien lama, 2 pasien baru |
| `PatientId` | `Guid?` | Pasien lama | — | Index bersama `VisitDate`, `Status` | FK `MstPatient` | `Restrict` | Tidak | Diisi saat daftar (pasien lama) atau saat check-in (pasien baru) |
| `IdentityNumber` | `string(16)?` | Pasien baru | — | Index bersama `VisitDate`, `Status` | — | — | **Ya** | NIK 16 digit. Pasien lama: NIK master bila ada, disalin saat daftar untuk aturan satu pendaftaran per NIK |
| `FullName` | `string(150)?` | Pasien baru | — | — | — | — | **Ya** | Nama yang diketik pasien baru. Pasien lama memakai nama master |
| `BirthDate` | `date?` | Pasien baru | — | — | — | — | **Ya** | Tanggal lahir pasien baru, untuk check-in dan Lupa Kode |
| `Gender` | `int?` | Pasien baru | — | — | — | — | **Ya** | Nilai enum jenis kelamin master pasien |
| `VisitDate` | `date` | Ya | — | Index | — | — | Tidak | Tanggal kunjungan, H+1 s.d. H+30 |
| `ClinicId` | `Guid` | Ya | — | Index | FK `MstClinic` | `Restrict` | Tidak | Poli |
| `DoctorScheduleId` | `Guid` | Ya | — | Index bersama `VisitDate`, `Status` | FK `MstDoctorSchedule` | `Restrict` | Tidak | Jadwal dokter. Dipakai hitungan kuota |
| `PatientType` | `int` (`OnlineRegistrationPatientType`) | Ya | — | — | — | — | Tidak | Pribadi, Asuransi, Rujukan |
| `PaymentType` | `int` (`EncounterPaymentType`) | Ya | — | — | — | — | Tidak | Tunai, Asuransi, Penjamin Perusahaan |
| `PatientInsuranceId` | `Guid?` | Kondisional | — | — | FK `MstPatientInsurance` | `Restrict` | Tidak | Kartu asuransi milik pasien lama |
| `PatientCompanyGuarantorId` | `Guid?` | Kondisional | — | — | FK `MstPatientCompanyGuarantor` | `Restrict` | Tidak | Penjamin perusahaan milik pasien lama |
| `NewInsuranceProviderId` | `Guid?` | Kondisional | — | — | FK `MstInsuranceProvider` | `Restrict` | Tidak | Kartu asuransi baru: penyedia |
| `NewInsurancePolicyNumber` | `string(50)?` | Kondisional | — | — | — | — | **Ya** | Kartu baru: nomor polis/kartu |
| `NewInsuranceHolderName` | `string(150)?` | Tidak | — | — | — | — | **Ya** | Kartu baru: nama pemegang |
| `NewInsuranceHolderRelationship` | `string(50)?` | Tidak | — | — | — | — | Tidak | Kartu baru: hubungan dengan pemegang |
| `IsReferral` | `bool` | Ya | `false` | — | — | — | Tidak | `true` bila jenis pasien Rujukan |
| `ReferralInstitutionId` | `Guid?` | Rujukan | — | — | FK `MstReferralInstitution` | `Restrict` | Tidak | Faskes perujuk |
| `ReferralNumber` | `string(50)?` | Tidak | — | — | — | — | **Ya** | Nomor surat rujukan |
| `ReferralDate` | `date?` | Tidak | — | — | — | — | Tidak | Tanggal surat rujukan, tidak boleh di masa depan |
| `ReferralDoctorName` | `string(150)?` | Tidak | — | — | — | — | Tidak | Nama dokter perujuk |
| `ReferralDiagnosisNote` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Catatan diagnosa dari surat |
| `ReferralReason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Alasan rujukan |
| `HasConsent` | `bool` | Ya | `false` | — | — | — | Tidak | `true` untuk pasien baru yang menandatangani consent |
| `ConsentSignerType` | `int?` (`GeneralConsentSignerType`) | Bila consent | — | — | — | — | Tidak | Diri sendiri, orang tua, wali, pasangan, keluarga |
| `ConsentSignerName` | `string(150)?` | Bila consent | — | — | — | — | **Ya** | Nama penanda tangan |
| `ConsentSignerRelationshipText` | `string(100)?` | Tidak | — | — | — | — | Tidak | Hubungan penanda tangan |
| `ConsentSignerIdentityNumber` | `string(50)?` | Tidak | — | — | — | — | **Ya** | Identitas penanda tangan bila bukan pasien |
| `ConsentSignerPhoneNumber` | `string(30)?` | Tidak | — | — | — | — | **Ya** | — |
| `ConsentSignerAddress` | `string(500)?` | Tidak | — | — | — | — | **Ya** | — |
| `ConsentAgreedClauses` | `int[]` | Bila consent | `{}` | — | — | — | Tidak | Klausul yang disetujui, sama dengan `RegPatientGeneralConsent.AgreedClauses` |
| `ConsentSignatureImageBase64` | `text?` | Bila consent | — | — | — | — | **Ya** | Gambar tanda tangan, maks. `MaxSignatureKb` |
| `ConsentSignedAt` | `timestamptz?` | Bila consent | — | — | — | — | Tidak | Waktu tanda tangan online; dipertahankan saat ditulis ke consent kunjungan |
| `ContactWhatsAppNumber` | `string(20)?` | Salah satu | — | — | — | — | **Ya** | Format `08…`, aturan panjang sama dengan No. HP kiosk (`KSK-DEC-022`, `043`) |
| `ContactEmail` | `string(150)?` | Salah satu | — | — | — | — | **Ya** | Alamat email sah |
| `EmailDeliveryStatus` | `int` (`OnlineRegistrationEmailStatus`) | Ya | `0` | — | — | — | Tidak | Belum diminta, terkirim, gagal |
| `EmailAttemptedAt` | `timestamptz?` | Tidak | — | — | — | — | Tidak | Waktu percobaan kirim |
| `SubmittedAt` | `timestamptz` | Ya | — | — | — | — | Tidak | Waktu pendaftaran disimpan |
| `CheckInTokenHash` | `string(64)?` | Tidak | — | — | — | — | **Ya** | Hash token check-in; dikosongkan setelah check-in |
| `CheckInTokenExpiresAt` | `timestamptz?` | Tidak | — | — | — | — | Tidak | Batas berlaku token |
| `EncounterId` | `Guid?` | Tidak | — | Unique bersyarat bukan null | FK `RegPatientEncounter` | `Restrict` | Tidak | Kunjungan hasil check-in atau kunjungan jalur lain |
| `CheckedInAt` | `timestamptz?` | Tidak | — | — | — | — | Tidak | Waktu check-in berhasil |
| `ClosedAt` | `timestamptz?` | Tidak | — | — | — | — | Tidak | Waktu menjadi status akhir selain Menjadi Kunjungan |
| `CancelReason` | `string(500)?` | Bila Dibatalkan | — | — | — | — | Tidak | Alasan petugas; jangan diisi data medis |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Token konkurensi, diganti setiap perubahan |

## PO-D.3 `RegOnlineRegistrationReferralDocument` — Baru

`.../Models/RegOnlineRegistrationReferralDocument.cs`, tabel `public."RegOnlineRegistrationReferralDocument"`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `OnlineRegistrationId` | `Guid` | Ya | — | Index | FK `RegOnlineRegistration` | `Restrict` | Tidak | Induk |
| `OriginalFileName` | `string(255)` | Ya | — | — | — | — | **Ya** | Nama berkas dari perangkat pasien |
| `ContentType` | `string(100)` | Ya | — | — | — | — | Tidak | `application/pdf`, `image/jpeg`, `image/png` |
| `SizeBytes` | `long` | Ya | — | — | — | — | Tidak | Maks. 5 MB |
| `StoragePath` | `string(500)` | Ya | — | — | — | — | Tidak | `online-registration-referrals/{OnlineRegistrationId:N}/{nama-acak}.{ext}`, relatif ke root privat |
| `PageOrder` | `int` | Ya | — | — | — | — | Tidak | 1–3 |
| `EncounterReferralDocumentId` | `Guid?` | Tidak | — | — | FK `RegEncounterReferralDocument` | `Restrict` | Tidak | Diisi saat check-in |

## PO-D.4 `RegOnlineRegistrationHistory` — Baru

`.../Models/RegOnlineRegistrationHistory.cs`, tabel `public."RegOnlineRegistrationHistory"`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `OnlineRegistrationId` | `Guid` | Ya | — | Index bersama `OccurredAt` | FK `RegOnlineRegistration` | `Restrict` | Tidak | Induk |
| `EventCode` | `string(50)` | Ya | — | — | — | — | Tidak | `SUBMITTED`, `CHECKED_IN`, `REGISTERED_ELSEWHERE`, `NO_SHOW`, `CANCELLED`, `SCHEDULE_CHANGED`, `CONTACT_CHANGED` |
| `FromStatus` | `int?` | Tidak | — | — | — | — | Tidak | Kosong untuk `SUBMITTED` |
| `ToStatus` | `int` | Ya | — | — | — | — | Tidak | Sama dengan `FromStatus` untuk perubahan jadwal/kontak |
| `Source` | `int` (`OnlineRegistrationEventSource`) | Ya | — | — | — | — | Tidak | Halaman publik, kiosk, petugas, sistem |
| `ActorUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Petugas atau akun kiosk; kosong untuk halaman publik dan sistem |
| `Reason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Alasan batal |
| `EncounterId` | `Guid?` | Tidak | — | — | — | — | Tidak | Kunjungan terkait (`CHECKED_IN`, `REGISTERED_ELSEWHERE`) |
| `FromVisitDate`, `ToVisitDate` | `date?` | Tidak | — | — | — | — | Tidak | `SCHEDULE_CHANGED` |
| `FromDoctorScheduleId`, `ToDoctorScheduleId` | `Guid?` | Tidak | — | — | — | — | Tidak | `SCHEDULE_CHANGED` |
| `FromContact`, `ToContact` | `string(150)?` | Tidak | — | — | — | — | **Ya** | `CONTACT_CHANGED`, nilai lama dan baru (`KSK-DEC-051`) |
| `OccurredAt` | `timestamptz` | Ya | — | — | — | — | Tidak | Waktu kejadian |

## PO-D.5 Kolom tabel lama yang dipakai aturan Amendment PO

| Tabel | Kolom | Dipakai untuk |
| --- | --- | --- |
| `MstPatient` | `Id`, `MedicalRecordNumber`, `IdentityNumber`, `BirthDate`, `FullName`, status aktif/gabung | Pencocokan pasien lama, Lupa Kode, pengikatan NIK pasien baru saat check-in, penutupan jalur lain lewat NIK |
| `MstDoctorSchedule` | `Id`, `ClinicId`, `DoctorId`, `ServiceUnitId`, `IsAllowAppointment`, `IsAllowKioskRegistration`, `MaxAppointmentQuota`, `MaxPatientQuota`, hari praktik dan masa berlaku | Opsi jadwal publik, validasi daftar/ubah, kuota |
| `RegPatientEncounter` | `PatientId`, `EncounterDate`, `DoctorScheduleId`, `IsAppointment`, `RegistrationSource` | Hitungan kuota dan penutupan jalur lain |
| `RegPatientGeneralConsent` | `PatientId`, `PatientEncounterId`, `SignerType`, `AgreedClauses`, `SignatureImageBase64`, `SignedAt` | Tujuan penulisan consent saat check-in |
| `RegEncounterReferralDocument` | `EncounterReferralId`, `StoragePath`, `PageOrder` | Tujuan penautan berkas rujukan |

## PO-D.6 Skema DDL

> **Peringatan.** Basis data project ini dibentuk EF Core Migrations. DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip yang dijalankan. Menjalankannya akan berbenturan dengan migration `AddRegOnlineRegistration`.

```sql
-- Bentuk tabel sebagaimana akan dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."RegOnlineRegistration" (
    "Id"                              uuid          NOT NULL,
    "RegistrationNumber"              varchar(30)   NOT NULL,
    "CodeHash"                        varchar(64)   NOT NULL,  -- SENSITIF
    "Status"                          integer       NOT NULL DEFAULT 1,
    "PatientMode"                     integer       NOT NULL,
    "PatientId"                       uuid,
    "IdentityNumber"                  varchar(16),               -- SENSITIF
    "FullName"                        varchar(150),              -- SENSITIF
    "BirthDate"                       date,                      -- SENSITIF
    "Gender"                          integer,                   -- SENSITIF
    "VisitDate"                       date          NOT NULL,
    "ClinicId"                        uuid          NOT NULL,
    "DoctorScheduleId"                uuid          NOT NULL,
    "PatientType"                     integer       NOT NULL,
    "PaymentType"                     integer       NOT NULL,
    "PatientInsuranceId"              uuid,
    "PatientCompanyGuarantorId"       uuid,
    "NewInsuranceProviderId"          uuid,
    "NewInsurancePolicyNumber"        varchar(50),               -- SENSITIF
    "NewInsuranceHolderName"          varchar(150),              -- SENSITIF
    "NewInsuranceHolderRelationship"  varchar(50),
    "IsReferral"                      boolean       NOT NULL DEFAULT false,
    "ReferralInstitutionId"           uuid,
    "ReferralNumber"                  varchar(50),               -- SENSITIF
    "ReferralDate"                    date,
    "ReferralDoctorName"              varchar(150),
    "ReferralDiagnosisNote"           varchar(500),              -- SENSITIF
    "ReferralReason"                  varchar(500),              -- SENSITIF
    "HasConsent"                      boolean       NOT NULL DEFAULT false,
    "ConsentSignerType"               integer,
    "ConsentSignerName"               varchar(150),              -- SENSITIF
    "ConsentSignerRelationshipText"   varchar(100),
    "ConsentSignerIdentityNumber"     varchar(50),               -- SENSITIF
    "ConsentSignerPhoneNumber"        varchar(30),               -- SENSITIF
    "ConsentSignerAddress"            varchar(500),              -- SENSITIF
    "ConsentAgreedClauses"            integer[]     NOT NULL DEFAULT '{}',
    "ConsentSignatureImageBase64"     text,                      -- SENSITIF
    "ConsentSignedAt"                 timestamptz,
    "ContactWhatsAppNumber"           varchar(20),               -- SENSITIF
    "ContactEmail"                    varchar(150),              -- SENSITIF
    "EmailDeliveryStatus"             integer       NOT NULL DEFAULT 0,
    "EmailAttemptedAt"                timestamptz,
    "SubmittedAt"                     timestamptz   NOT NULL,
    "CheckInTokenHash"                varchar(64),               -- SENSITIF
    "CheckInTokenExpiresAt"           timestamptz,
    "EncounterId"                     uuid,
    "CheckedInAt"                     timestamptz,
    "ClosedAt"                        timestamptz,
    "CancelReason"                    varchar(500),
    "RowVersion"                      uuid          NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini
    CONSTRAINT "PK_RegOnlineRegistration" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_RegOnlineRegistration_Contact"
        CHECK ("ContactWhatsAppNumber" IS NOT NULL OR "ContactEmail" IS NOT NULL),
    CONSTRAINT "FK_RegOnlineRegistration_MstPatient_PatientId"
        FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RegOnlineRegistration_MstClinic_ClinicId"
        FOREIGN KEY ("ClinicId") REFERENCES public."MstClinic" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RegOnlineRegistration_MstDoctorSchedule_DoctorScheduleId"
        FOREIGN KEY ("DoctorScheduleId") REFERENCES public."MstDoctorSchedule" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RegOnlineRegistration_MstPatientInsurance_PatientInsuranceId"
        FOREIGN KEY ("PatientInsuranceId") REFERENCES public."MstPatientInsurance" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RegOnlineRegistration_MstPatientCompanyGuarantor_PatientCompanyGuarantorId"
        FOREIGN KEY ("PatientCompanyGuarantorId") REFERENCES public."MstPatientCompanyGuarantor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RegOnlineRegistration_MstInsuranceProvider_NewInsuranceProviderId"
        FOREIGN KEY ("NewInsuranceProviderId") REFERENCES public."MstInsuranceProvider" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RegOnlineRegistration_MstReferralInstitution_ReferralInstitutionId"
        FOREIGN KEY ("ReferralInstitutionId") REFERENCES public."MstReferralInstitution" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RegOnlineRegistration_RegPatientEncounter_EncounterId"
        FOREIGN KEY ("EncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_RegOnlineRegistration_RegistrationNumber" ON public."RegOnlineRegistration" ("RegistrationNumber");
CREATE UNIQUE INDEX "IX_RegOnlineRegistration_CodeHash_Awaiting" ON public."RegOnlineRegistration" ("CodeHash") WHERE "Status" = 1;
CREATE UNIQUE INDEX "IX_RegOnlineRegistration_EncounterId" ON public."RegOnlineRegistration" ("EncounterId") WHERE "EncounterId" IS NOT NULL;
CREATE INDEX "IX_RegOnlineRegistration_DoctorScheduleId_VisitDate_Status" ON public."RegOnlineRegistration" ("DoctorScheduleId", "VisitDate", "Status");
CREATE INDEX "IX_RegOnlineRegistration_PatientId_VisitDate_Status" ON public."RegOnlineRegistration" ("PatientId", "VisitDate", "Status");
CREATE INDEX "IX_RegOnlineRegistration_IdentityNumber_VisitDate_Status" ON public."RegOnlineRegistration" ("IdentityNumber", "VisitDate", "Status");
CREATE INDEX "IX_RegOnlineRegistration_VisitDate_Status" ON public."RegOnlineRegistration" ("VisitDate", "Status");

CREATE TABLE public."RegOnlineRegistrationReferralDocument" (
    "Id"                           uuid          NOT NULL,
    "OnlineRegistrationId"         uuid          NOT NULL,
    "OriginalFileName"             varchar(255)  NOT NULL,  -- SENSITIF
    "ContentType"                  varchar(100)  NOT NULL,
    "SizeBytes"                    bigint        NOT NULL,
    "StoragePath"                  varchar(500)  NOT NULL,
    "PageOrder"                    integer       NOT NULL,
    "EncounterReferralDocumentId"  uuid,
    CONSTRAINT "PK_RegOnlineRegistrationReferralDocument" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RegOnlineRegistrationReferralDocument_RegOnlineRegistration_OnlineRegistrationId"
        FOREIGN KEY ("OnlineRegistrationId") REFERENCES public."RegOnlineRegistration" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RegOnlineRegistrationReferralDocument_RegEncounterReferralDocument_EncounterReferralDocumentId"
        FOREIGN KEY ("EncounterReferralDocumentId") REFERENCES public."RegEncounterReferralDocument" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_RegOnlineRegistrationReferralDocument_OnlineRegistrationId" ON public."RegOnlineRegistrationReferralDocument" ("OnlineRegistrationId");

CREATE TABLE public."RegOnlineRegistrationHistory" (
    "Id"                    uuid          NOT NULL,
    "OnlineRegistrationId"  uuid          NOT NULL,
    "EventCode"             varchar(50)   NOT NULL,
    "FromStatus"            integer,
    "ToStatus"              integer       NOT NULL,
    "Source"                integer       NOT NULL,
    "ActorUserId"           uuid,
    "Reason"                varchar(500),
    "EncounterId"           uuid,
    "FromVisitDate"         date,
    "ToVisitDate"           date,
    "FromDoctorScheduleId"  uuid,
    "ToDoctorScheduleId"    uuid,
    "FromContact"           varchar(150),  -- SENSITIF
    "ToContact"             varchar(150),  -- SENSITIF
    "OccurredAt"            timestamptz   NOT NULL,
    CONSTRAINT "PK_RegOnlineRegistrationHistory" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RegOnlineRegistrationHistory_RegOnlineRegistration_OnlineRegistrationId"
        FOREIGN KEY ("OnlineRegistrationId") REFERENCES public."RegOnlineRegistration" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_RegOnlineRegistrationHistory_OnlineRegistrationId_OccurredAt" ON public."RegOnlineRegistrationHistory" ("OnlineRegistrationId", "OccurredAt");
```

## PO-D.7 Data yang sengaja tidak disimpan

| Data | Alasan |
| --- | --- |
| Kode pendaftaran asli dan token check-in asli | Hanya hash (`INV-PO-05`) |
| Alamat IP pengunjung | Tidak diperlukan setelah batas laju bekerja (`GATE-PO-09`) |
| Alamat, agama, pekerjaan, dan data diri lain pasien baru | Master dibuat dari hasil pindai (`KSK-DSN-PO-004`) |
| Token captcha | Sekali pakai, diverifikasi lalu dibuang |
| Percobaan kode atau pencocokan yang gagal | Hanya jumlah dan waktu di log, tanpa data pribadi |
