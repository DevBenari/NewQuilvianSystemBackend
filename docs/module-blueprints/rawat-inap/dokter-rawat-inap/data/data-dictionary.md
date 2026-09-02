# Kamus Data — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` |
| Revision | `0.1` |
| Status | `draft` |
| Tanggal | 2 September 2026 |
| Sumber | [`../02-backend-architecture.md`](../02-backend-architecture.md) bagian 4 |

---

## 0. Tiga hal yang wajib dibaca lebih dulu

**Pertama: tidak satu tabel pun di dokumen ini dimiliki modul Rawat Inap.** Kolom **Pemilik**
menyebut modul yang berwenang mengubahnya — `RWI-DEC-081` dan PRD 23.1.

**Kedua: sepuluh kolom warisan `IdentityModel` tidak diulang per tabel.**
`CreateBy`, `CreateDate`, `UpdateBy`, `UpdateDate`, `DeleteBy`, `DeleteDate`, `IsDelete`, `Flag`,
`Reserved1`, `Reserved2`.

**Ketiga: kolom bertanda Sensitif** tidak boleh masuk custom logger dan tidak boleh dipakai
sebagai contoh berisi data asli.

---

## 1. Status dan kepemilikan tabel

| Tabel | Status | Modul pemilik | Kemampuan |
| --- | --- | --- | --- |
| `TrxDoctorConsultation` | **`Diperbarui`** | `ClinicalManagement` | `CAP-020` |
| `TrxPatientAssessment` | **`Diperbarui`** — hanya nilai enum | `ClinicalManagement` | `CAP-022` |
| `TrxPatientIntegratedProgressNote` | **`Diperbarui`** | `ClinicalManagement` | `CAP-021` |
| `TrxPatientProcedure` | **`Diperbarui`** | `ClinicalManagement` | `CAP-024` |
| `TrxPhysicianVisit` | **`Baru`** | `ClinicalManagement` | `CAP-025` |
| `TrxPrescription` | **`Diperbarui`** | `PharmacyManagement` | `CAP-023` |
| `LabOrder` | **`Diperbarui`** | `LaboratoryManagement` | `CAP-015` |
| `TrxPatientDiagnosis` | `Sudah ada` | `ClinicalManagement` | `CAP-022` aturan 5 |
| `InpEpisode`, `InpDoctorAssignment` | `Sudah ada` | `InPatientManagement` | Konteks dan kewenangan |
| *Radiologi* | **Tidak ada modulnya** | — | `CAP-015` sebagian |

---

## 2. `TrxDoctorConsultation` — `Diperbarui`

Ke-71 kolom yang sudah ada tidak diulang; rujukannya
`Areas/HealthServices/ClinicalManagement/Models/TrxDoctorConsultation.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif |
| --- | --- | :---: | --- | --- | --- | --- | :---: |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `(InpEpisodeId, ClinicalDateTime)` | → `InpEpisode.Id` | `Restrict` | Tidak |
| `ClinicalDateTime` | `timestamptz` | Tidak | `null` | bagian index di atas | — | — | Tidak |
| `VisitId` | `uuid` | Tidak | `null` | — | → `TrxPhysicianVisit.Id` | `SetNull` | Tidak |

### 2.1 Kolom lama yang **berubah artinya**

| Kolom | Yang berubah |
| --- | --- |
| `QueueId` | Bentuknya **tidak berubah** — sudah `uuid?`. Yang berubah: kapan ia boleh kosong, kini juga saat encounter punya episode `Admitted` |

### 2.2 Kolom lama yang **sensitif**

`Subjective`, `Objective`, `Assessment`, `Plan`, `ProcedurePlan`, `PrescriptionPlan`,
`SupportingExamPlan`, `ReferralPlan`, `EducationPlan`.

> **`VisitId` memakai `SetNull`, bukan `Restrict`.** Visite yang dihapus karena salah catat tidak
> boleh menghapus catatan SOAP-nya. `INV-DOK-03`: keduanya memang tidak terikat mati.

---

## 3. `TrxPatientAssessment` — `Diperbarui`, hanya nilai enum

| Yang berubah | Isinya |
| --- | --- |
| Kolom baru dari sub-modul ini | **Nol** |
| Enum `PatientAssessmentType` | Bertambah `MedicalInitial` dan `MedicalReassessment` |

Keenam kolom `InpEpisodeId`, `AssessmentType`, `DueAt`, `PolicyId`, `AmendedAt`,
`AmendedByUserId` **sudah diminta** `keperawatan` dan dipakai apa adanya di sini. Rinciannya di
[`../keperawatan/data/data-dictionary.md`](../../keperawatan/data/data-dictionary.md) bagian 2.

> Berbagi satu tabel adalah keputusan struktur yang **menunggu persetujuan pemilik** —
> `../02-backend-architecture.md` bagian 4.2, beserta jalan yang ditolak dan syaratnya.

---

## 4. `TrxPatientIntegratedProgressNote` — `Diperbarui`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif |
| --- | --- | :---: | --- | --- | --- | --- | :---: |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `(InpEpisodeId, CreateDate)` | → `InpEpisode.Id` | `Restrict` | Tidak |
| `VerificationStatus` | `int` (enum) | **Ya** | `NotRequired` | Index | — | — | Tidak |
| `VerifiedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak |
| `VerifiedByUserId` | `uuid` | Tidak | `null` | — | → `ApplicationUser.Id` | `Restrict` | Tidak |
| `VerificationDueAt` | `timestamptz` | Tidak | `null` | Index parsial `WHERE VerificationStatus = 1` | — | — | Tidak |
| `AmendedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak |
| `AmendedByUserId` | `uuid` | Tidak | `null` | — | → `ApplicationUser.Id` | `Restrict` | Tidak |
| `AmendReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** |

`CpptVerificationStatus`: `NotRequired`, `Pending`, `Verified`, `Overdue`. Bawaan `NotRequired`.

> **`VerifiedByUserId` sengaja terpisah dari `ProviderUserId`.** Itulah yang membuat
> `AC-CAP021-03` dapat dibuktikan: verifikator **tidak pernah** menggantikan penulis asli.
>
> **Index parsial hanya pada yang `Pending`**, karena daftar pantau verifikasi hanya membaca baris
> yang benar-benar menunggu. Meng-index seluruh baris memboroskan tanpa dipakai.

---

## 5. `TrxPatientProcedure` — `Diperbarui`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif |
| --- | --- | :---: | --- | --- | --- | --- | :---: |
| `InpEpisodeId` | `uuid` | Tidak | `null` | Index | → `InpEpisode.Id` | `Restrict` | Tidak |
| `ProcedureRecordType` | `int` (enum) | **Ya** | `Performed` | Index | — | — | Tidak |
| `PerformedAt` | `timestamptz` | Tidak | `null` | `(InpEpisodeId, PerformedAt)` | — | — | Tidak |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | **Unique parsial** | — | — | Tidak |
| `BillingDispatchStatus` | `int` (enum) | **Ya** | `NotApplicable` | Index | — | — | Tidak |

`ProcedureRecordType`: `Ordered`, `Performed`, `Cancelled`, `Amended`. Bawaan `Performed`.

> Bawaan `Performed`, bukan `Ordered`: seluruh baris lama adalah tindakan yang **sudah**
> dilakukan, dan menandainya `Ordered` akan mengubah arti data lama.

---

## 6. `TrxPhysicianVisit` — `Baru`

Satu-satunya tabel yang benar-benar baru pada sub-modul ini.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif |
| --- | --- | :---: | --- | --- | --- | --- | :---: |
| `Id` | `uuid` | Ya | `newid` | PK | — | — | Tidak |
| `EncounterId` | `uuid` | Ya | — | Index | → `TrxPatientEncounter.Id` | `Restrict` | Tidak |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `(InpEpisodeId, VisitDateTime)` | → `InpEpisode.Id` | `Restrict` | Tidak |
| `PatientId` | `uuid` | Ya | — | Index | → `MstPatient.Id` | `Restrict` | Tidak |
| `DoctorId` | `uuid` | Ya | — | Index | → dokter | `Restrict` | Tidak |
| `VisitDateTime` | `timestamptz` | Ya | — | bagian index di atas | — | — | Tidak |
| `VisitRole` | `int` (enum) | Ya | `Dpjp` | — | — | — | Tidak |
| `ConsultationId` | `uuid` | Tidak | `null` | — | → `TrxDoctorConsultation.Id` | `SetNull` | Tidak |
| `ProgressNoteId` | `uuid` | Tidak | `null` | — | → `TrxPatientIntegratedProgressNote.Id` | `SetNull` | Tidak |
| `Note` | `varchar(1000)` | Tidak | `null` | — | — | — | **Ya** |
| `RecordedByUserId` | `uuid` | Ya | — | — | → `ApplicationUser.Id` | `Restrict` | Tidak |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | **Unique parsial** | — | — | Tidak |

`PhysicianVisitRole`: `Dpjp`, `Consultant`, `OnCall`. Bawaan `Dpjp`.

> **`ConsultationId` dan `ProgressNoteId` keduanya nullable dan `SetNull`.** `INV-DOK-03`: satu
> visite tidak wajib punya catatan, dan satu catatan tidak wajib punya visite. Membuat salah
> satunya wajib akan menghidupkan kembali kesalahan yang justru dilarang `AC-CAP025-02`.
>
> **Tidak ada unique `(EpisodeId, DoctorId, tanggal)`.** Dokter yang benar-benar datang dua kali
> sehari adalah kejadian nyata; duplikat dijaga kunci idempotency.

---

## 7. `TrxPrescription` — `Diperbarui` — milik `PharmacyManagement`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif |
| --- | --- | :---: | --- | --- | --- | --- | :---: |
| `InpEpisodeId` | `uuid` | Tidak | `null` | Index | → `InpEpisode.Id` | `Restrict` | Tidak |
| `PrescriptionOrderType` | `int` (enum) | **Ya** | `Routine` | Index | — | — | Tidak |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | **Unique parsial** | — | — | Tidak |

`PrescriptionOrderType`: `Routine`, `Daily`, `Discharge`. Bawaan `Routine`.

### 7.1 Kolom yang **hanya dibaca** sub-modul ini

| Kolom | Kenapa hanya dibaca |
| --- | --- |
| `PrescriptionStatus`, `PaymentStatus`, `FulfillmentStatus` | `INV-DOK-04`, PRD `CAP-023` aturan 6. Rawat Inap **tidak pernah** menandai obat sudah diserahkan |

---

## 8. `LabOrder` — `Diperbarui` — milik `LaboratoryManagement`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif |
| --- | --- | :---: | --- | --- | --- | --- | :---: |
| `InpEpisodeId` | `uuid` | Tidak | `null` | Index | → `InpEpisode.Id` | `Restrict` | Tidak |

Satu kolom. `LabOrder` sudah terikat `EncounterId` tanpa antrean maupun konsultasi, sehingga
pemesanan lab rawat inap sudah mungkin hari ini; kolom ini yang membuat `AC-CAP015-01` dapat
dibuktikan.

### 8.1 Yang **hanya dibaca**

`OrderStatus`, hasil, spesimen, dan riwayat transisi. `INV-DOK-05`, `AC-CAP015-02`.

---

## 9. Tabel `Sudah ada` — kolom kunci saja

### `TrxPatientDiagnosis` — `ClinicalManagement`

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `ConsultationId` | **Wajib** — diagnosis lahir dari konsultasi |
| `DiagnosisType`, `IsPrimary` | Daftar masalah terstruktur — PRD `CAP-022` aturan 5 |

Rujukan model: `Areas/HealthServices/ClinicalManagement/Models/TrxPatientDiagnosis.cs`

### `InpDoctorAssignment` — `InPatientManagement`

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `EpisodeId`, `DoctorId` | Menjawab siapa DPJP |
| `StartDateTime`, `EndDateTime` | **Berperiode** — kewenangan pada tanggal tertentu, bukan penugasan terkini |
| `IsActive` | DPJP yang sedang berlaku |

### `InpEpisode` — `InPatientManagement`

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `EncounterId`, `PatientId` | Jembatan ke mesin klinis |
| `EpisodeStatus` | `INV-DOK-01` dan `INV-DOK-02` |

---

## 10. Skema DDL

> **Peringatan.** Bagian ini **dokumentasi bentuk**, bukan skrip yang dijalankan. Skema sungguhan
> lahir dari EF Core migration milik modul pemiliknya. Kolom warisan `IdentityModel` tidak ditulis.

```sql
-- Konsultasi dan SOAP
ALTER TABLE "TrxDoctorConsultation" ADD COLUMN "InpEpisodeId" uuid NULL;
ALTER TABLE "TrxDoctorConsultation" ADD COLUMN "ClinicalDateTime" timestamptz NULL;
ALTER TABLE "TrxDoctorConsultation" ADD COLUMN "VisitId" uuid NULL;
CREATE INDEX "IX_TrxDoctorConsultation_Episode_ClinicalTime"
    ON "TrxDoctorConsultation" ("InpEpisodeId", "ClinicalDateTime");

-- CPPT: konteks episode dan verifikasi
ALTER TABLE "TrxPatientIntegratedProgressNote" ADD COLUMN "InpEpisodeId" uuid NULL;
ALTER TABLE "TrxPatientIntegratedProgressNote" ADD COLUMN "VerificationStatus" integer NOT NULL DEFAULT 0;
ALTER TABLE "TrxPatientIntegratedProgressNote" ADD COLUMN "VerifiedAt" timestamptz NULL;
ALTER TABLE "TrxPatientIntegratedProgressNote" ADD COLUMN "VerifiedByUserId" uuid NULL;
ALTER TABLE "TrxPatientIntegratedProgressNote" ADD COLUMN "VerificationDueAt" timestamptz NULL;
CREATE INDEX "IX_Cppt_PendingVerification"
    ON "TrxPatientIntegratedProgressNote" ("VerificationDueAt")
    WHERE "VerificationStatus" = 1 AND "IsDelete" = false;

-- Tindakan dokter
ALTER TABLE "TrxPatientProcedure" ADD COLUMN "InpEpisodeId" uuid NULL;
ALTER TABLE "TrxPatientProcedure" ADD COLUMN "ProcedureRecordType" integer NOT NULL DEFAULT 1;
ALTER TABLE "TrxPatientProcedure" ADD COLUMN "PerformedAt" timestamptz NULL;
CREATE UNIQUE INDEX "UX_TrxPatientProcedure_IdempotencyKey"
    ON "TrxPatientProcedure" ("IdempotencyKey")
    WHERE "IdempotencyKey" IS NOT NULL AND "IsDelete" = false;

-- Visite dokter: tabel baru
CREATE UNIQUE INDEX "UX_TrxPhysicianVisit_IdempotencyKey"
    ON "TrxPhysicianVisit" ("IdempotencyKey")
    WHERE "IdempotencyKey" IS NOT NULL AND "IsDelete" = false;
CREATE INDEX "IX_TrxPhysicianVisit_Episode_VisitTime"
    ON "TrxPhysicianVisit" ("InpEpisodeId", "VisitDateTime");

-- Resep
ALTER TABLE "TrxPrescription" ADD COLUMN "InpEpisodeId" uuid NULL;
ALTER TABLE "TrxPrescription" ADD COLUMN "PrescriptionOrderType" integer NOT NULL DEFAULT 0;
CREATE UNIQUE INDEX "UX_TrxPrescription_IdempotencyKey"
    ON "TrxPrescription" ("IdempotencyKey")
    WHERE "IdempotencyKey" IS NOT NULL AND "IsDelete" = false;

-- Pesanan laboratorium
ALTER TABLE "LabOrder" ADD COLUMN "InpEpisodeId" uuid NULL;
```

---

## 11. Tabel yang **tidak** dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| Tabel `Inp*` apa pun untuk dokumentasi dokter | `RWI-DEC-081`, PRD 23.1 |
| Tabel SOAP tersendiri | `TrxDoctorConsultation` sudah memuat S/O/A/P |
| `TrxMedicalAssessment` tersendiri | Menyalin ~40 kolom; lihat `../02-backend-architecture.md` bagian 4.2 |
| Salinan hasil laboratorium | `INV-DOK-05`, `AC-CAP015-02` |
| Kolom status penyerahan obat milik Rawat Inap | `INV-DOK-04` |
| Tabel radiologi | Modulnya belum ada |
