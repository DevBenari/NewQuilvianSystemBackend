# Kamus Data — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` |
| Revision | `0.1` |
| Status | `draft` |
| Tanggal | 2 September 2026 |
| Sumber | [`../02-backend-architecture.md`](../02-backend-architecture.md) bagian 4 |

---

## 0. Dua hal yang wajib dibaca lebih dulu

**Pertama: tidak satu tabel pun di dokumen ini dimiliki modul Rawat Inap.** Kolom **Pemilik** pada
setiap tabel menyebut modul yang berwenang mengubahnya. `RWI-DEC-081` dan `PRD-RWI-FINAL-001`
bagian 23.1 menaruh seluruhnya pada `ClinicalManagement`.

**Kedua: sepuluh kolom warisan `IdentityModel` tidak diulang per tabel.** Setiap tabel di bawah
mewarisinya, dan tidak satu pun ditulis ulang pada daftar kolom maupun pada DDL:

`CreateBy`, `CreateDate`, `UpdateBy`, `UpdateDate`, `DeleteBy`, `DeleteDate`, `IsDelete`,
`Flag`, `Reserved1`, `Reserved2`.

**Penanda Sensitif.** Kolom bertanda **Ya** tidak boleh masuk custom logger dan tidak boleh
dipakai sebagai contoh berisi data asli.

---

## 1. Status dan kepemilikan tabel

| Tabel | Status | Modul pemilik | Kemampuan |
| --- | --- | --- | --- |
| `TrxPatientAssessment` | **`Diperbarui`** | `ClinicalManagement` | `CAP-012` |
| `TrxNursingCarePlan` | **`Baru`** | `ClinicalManagement` | `CAP-013` |
| `TrxNursingCarePlanItem` | **`Baru`** | `ClinicalManagement` | `CAP-013` |
| `TrxNursingCarePlanItemRevision` | **`Baru`** | `ClinicalManagement` | `CAP-013` |
| `TrxNursingIntervention` | **`Baru`** | `ClinicalManagement` | `CAP-014` |
| `MstClinicalAssessmentPolicy` | **`Baru`** | `ClinicalManagement` | `CAP-012` aturan 11 |
| `TrxPatientIntegratedProgressNote` | `Sudah ada` | `ClinicalManagement` | `CAP-014` aturan 4 |
| `InpEpisode` | `Sudah ada` | `InPatientManagement` | Konteks |
| `InpNurseAssignment` | `Sudah ada` | `InPatientManagement` | Kewenangan |
| *Pemakaian alat* | **`DEFERRED`** — `RWI-DEC-089` | **Sengaja ditunda**; masuk kembali setelah modul persediaan/aset ada | `CAP-016` |

---

## 2. `TrxPatientAssessment` — `Diperbarui`

Tabel berstatus `Diperbarui` didokumentasikan **seluruh kolom yang berubah**. Ke-85 kolom yang
sudah ada tidak diulang di sini; rujukannya
`Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif |
| --- | --- | :---: | --- | --- | --- | --- | :---: |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `IX_TrxPatientAssessment_InpEpisodeId` | → `InpEpisode.Id` | `Restrict` | Tidak |
| `AssessmentType` | `int` (enum) | **Ya** | `Initial` | bagian dari index parsial | — | — | Tidak |
| `DueAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak |
| `PolicyId` | `uuid` | Tidak | `null` | — | → `MstClinicalAssessmentPolicy.Id` | `Restrict` | Tidak |
| `AmendedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak |
| `AmendedByUserId` | `uuid` | Tidak | `null` | — | → `ApplicationUser.Id` | `Restrict` | Tidak |

### 2.1 Kolom lama yang **berubah artinya**

| Kolom | Yang berubah |
| --- | --- |
| `QueueId` | Bentuknya **tidak berubah** — sudah `uuid?` sejak awal. Yang berubah adalah **kapan ia boleh kosong**: kini juga saat encounter punya episode rawat inap `Admitted`, bukan hanya saat pasien IGD |
| `AssessmentStatus` | **Nol perubahan** sejak revision `0.3`. Nilai `Amended` sempat direncanakan lalu dicabut `RWI-DEC-091`: koreksi disimpan mesin addendum `MedicalRecordManagement`, bukan sebagai status dokumen |

### 2.2 Kolom lama yang **sensitif** dan sudah ada

`NurseNote`, `PsychosocialNote`, `EducationNote`, `PainNote`, `NutritionNote`, `FallRiskNote`,
`FunctionalNote`, `AllergyNote`, `ImmunizationNote`, `HereditaryDiseaseNote`, `CancelReason`.

---

## 3. `TrxNursingCarePlan` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif |
| --- | --- | :---: | --- | --- | --- | --- | :---: |
| `Id` | `uuid` | Ya | `newid` | PK | — | — | Tidak |
| `EncounterId` | `uuid` | Ya | — | Index | → `TrxPatientEncounter.Id` | `Restrict` | Tidak |
| `InpEpisodeId` | `uuid` | Ya | — | Unique parsial `WHERE IsDelete=false` | → `InpEpisode.Id` | `Restrict` | Tidak |
| `PatientId` | `uuid` | Ya | — | Index | → `MstPatient.Id` | `Restrict` | Tidak |
| `OpenedAt` | `timestamptz` | Ya | `now` | — | — | — | Tidak |
| `OpenedByEmployeeId` | `uuid` | Ya | — | — | → `MstEmployee.Id` | `Restrict` | Tidak |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak |

> **Unique parsial pada `InpEpisodeId`**: satu episode tepat satu rencana asuhan. Butir-butirnya
> yang banyak, bukan rencananya.

---

## 4. `TrxNursingCarePlanItem` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif |
| --- | --- | :---: | --- | --- | --- | --- | :---: |
| `Id` | `uuid` | Ya | `newid` | PK | — | — | Tidak |
| `CarePlanId` | `uuid` | Ya | — | Index | → `TrxNursingCarePlan.Id` | `Cascade` | Tidak |
| `NursingDiagnosisId` | `uuid` | Tidak | `null` | — | → katalog terminologi, **`OPEN DECISION`** | `Restrict` | Tidak |
| `ProblemStatement` | `varchar(500)` | Ya | — | — | — | — | **Ya** |
| `GoalStatement` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** |
| `PlannedIntervention` | `text` | Tidak | `null` | — | — | — | **Ya** |
| `ItemStatus` | `int` (enum) | Ya | `Active` | Index | — | — | Tidak |
| `ResolvedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak |
| `CloseReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** |
| `VersionNumber` | `int` | Ya | `1` | — | — | — | Tidak |

`NursingCarePlanItemStatus`: `Active`, `Resolved`, `Discontinued`. Bawaan `Active`.

> `NursingDiagnosisId` **nullable dan tanpa tabel tujuan yang pasti**, karena katalog SDKI/SLKI/SIKI
> baru wajib bila rumah sakit memakainya — PRD 17 `CAP-013` aturan 3. Selama belum diputuskan,
> `ProblemStatement` menampung teksnya.

---

## 5. `TrxNursingCarePlanItemRevision` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif |
| --- | --- | :---: | --- | --- | --- | --- | :---: |
| `Id` | `uuid` | Ya | `newid` | PK | — | — | Tidak |
| `CarePlanItemId` | `uuid` | Ya | — | Index | → `TrxNursingCarePlanItem.Id` | `Cascade` | Tidak |
| `VersionNumber` | `int` | Ya | — | Unique bersama `CarePlanItemId` | — | — | Tidak |
| `ProblemStatement` | `varchar(500)` | Ya | — | — | — | — | **Ya** |
| `GoalStatement` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** |
| `PlannedIntervention` | `text` | Tidak | `null` | — | — | — | **Ya** |
| `EvaluationNote` | `text` | Tidak | `null` | — | — | — | **Ya** |
| `RevisedAt` | `timestamptz` | Ya | `now` | — | — | — | Tidak |
| `OriginalAuthorEmployeeId` | `uuid` | Ya | — | — | → `MstEmployee.Id` | `Restrict` | Tidak |
| `OriginalAuthoredAt` | `timestamptz` | Ya | — | — | — | — | Tidak |

> Dua kolom terakhir yang membuat `AC-CAP013-02` dapat dibuktikan: versi lama **mempertahankan
> penulis dan waktunya sendiri**, bukan penulis yang merevisi.

---

## 6. `TrxNursingIntervention` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif |
| --- | --- | :---: | --- | --- | --- | --- | :---: |
| `Id` | `uuid` | Ya | `newid` | PK | — | — | Tidak |
| `EncounterId` | `uuid` | Ya | — | Index | → `TrxPatientEncounter.Id` | `Restrict` | Tidak |
| `InpEpisodeId` | `uuid` | Tidak | `null` | Index | → `InpEpisode.Id` | `Restrict` | Tidak |
| `CarePlanItemId` | `uuid` | Tidak | `null` | Index | → `TrxNursingCarePlanItem.Id` | `SetNull` | Tidak |
| `InterventionName` | `varchar(300)` | Ya | — | — | — | — | Tidak |
| `PerformedAt` | `timestamptz` | Ya | — | Index bersama `InpEpisodeId` | — | — | Tidak |
| `PerformedByEmployeeId` | `uuid` | Ya | — | Index | → `MstEmployee.Id` | `Restrict` | Tidak |
| `ResultNote` | `text` | Tidak | `null` | — | — | — | **Ya** |
| `RecordStatus` | `int` (enum) | Ya | `Recorded` | Index | — | — | Tidak |
| `FinalizedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak |
| `AmendReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | **Unique parsial** `WHERE IdempotencyKey IS NOT NULL AND IsDelete=false` | — | — | Tidak |
| `IsBillable` | `boolean` | Ya | `false` | — | — | — | Tidak |
| `BillingDispatchStatus` | `int` (enum) | Ya | `NotApplicable` | Index | — | — | Tidak |

`NursingBillingDispatchStatus`: `NotApplicable`, `Pending`, `Dispatched`, `Failed`. Bawaan
`NotApplicable`.

> **`CarePlanItemId` memakai `SetNull`, bukan `Restrict`.** Sebabnya `CAP-013` aturan 6: menutup
> butir rencana **tidak boleh** menghapus tindakan yang sudah dilakukan. Tindakannya tetap hidup
> walaupun rujukan rencananya lepas.

---

## 7. `MstClinicalAssessmentPolicy` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Sensitif |
| --- | --- | :---: | --- | --- | :---: |
| `Id` | `uuid` | Ya | `newid` | PK | Tidak |
| `PolicyCode` | `varchar(50)` | Ya | — | **Unique** | Tidak |
| `AssessmentType` | `int` (enum) | Ya | — | Index bersama `ServiceUnitTypeId` | Tidak |
| `ServiceUnitTypeId` | `uuid?` | Tidak | `null` | — | Tidak |
| `DueWithinMinutes` | `int` | Ya | — | — | Tidak |
| `EffectiveFrom` | `timestamptz` | Ya | — | Index | Tidak |
| `EffectiveTo` | `timestamptz?` | Tidak | `null` | — | Tidak |
| `IsActive` | `boolean` | Ya | `true` | — | Tidak |

> **Berversi lewat `EffectiveFrom`/`EffectiveTo`, bukan ditimpa.** Mengubah kebijakan tidak boleh
> membuat pengkajian yang dulu tepat waktu berubah menjadi terlambat.

---

## 8. Tabel `Sudah ada` — kolom kunci saja

### `InpEpisode` — `InPatientManagement`

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `Id` | Konteks seluruh dokumentasi |
| `EncounterId` | Menjembatani ke mesin klinis |
| `PatientId` | Identitas pasien |
| `EpisodeStatus` | **`INV-KEP-01`** — hanya `Admitted` yang menerima dokumentasi baru |
| `ServiceUnitId` | Kewenangan berbasis unit bila penugasan perawat kosong |

Rujukan model: `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs`

### `InpNurseAssignment` — `InPatientManagement`

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `EpisodeId`, `EmployeeId` | Menjawab siapa perawat penanggung jawab |
| `StartDateTime`, `EndDateTime` | Bentuk berperiode; penanggung jawab pada tanggal tertentu |
| `IsActive` | Penanggung jawab yang sedang berlaku |

### `TrxPatientIntegratedProgressNote` — `ClinicalManagement`

| Kolom kunci | Dipakai untuk |
| --- | --- |
| `ProfessionType` | Menandai catatan sebagai catatan keperawatan |
| `EncounterId`, `AssessmentId` | Menghubungkan catatan ke konteks dan pengkajiannya |
| `ProviderUserId` | Penulisnya |

**Nol kolom diminta berubah.** Seluruh kolom penghubungnya sudah nullable.

---

## 9. Skema DDL

> **Peringatan.** Bagian ini adalah **dokumentasi bentuk**, bukan skrip yang dijalankan. Skema
> sungguhan lahir dari EF Core migration milik `ClinicalManagement`. Kolom warisan `IdentityModel`
> **tidak** ditulis di sini.

```sql
-- TrxPatientAssessment: enam kolom tambahan
ALTER TABLE "TrxPatientAssessment" ADD COLUMN "InpEpisodeId" uuid NULL;
ALTER TABLE "TrxPatientAssessment" ADD COLUMN "AssessmentType" integer NOT NULL DEFAULT 0;
ALTER TABLE "TrxPatientAssessment" ADD COLUMN "DueAt" timestamptz NULL;
ALTER TABLE "TrxPatientAssessment" ADD COLUMN "PolicyId" uuid NULL;
ALTER TABLE "TrxPatientAssessment" ADD COLUMN "AmendedAt" timestamptz NULL;
ALTER TABLE "TrxPatientAssessment" ADD COLUMN "AmendedByUserId" uuid NULL;

CREATE INDEX "IX_TrxPatientAssessment_InpEpisodeId"
    ON "TrxPatientAssessment" ("InpEpisodeId");

CREATE INDEX "IX_TrxPatientAssessment_Episode_Type_Active"
    ON "TrxPatientAssessment" ("InpEpisodeId", "AssessmentType")
    WHERE "AssessmentType" = 0 AND "IsDelete" = false;

-- Satu episode tepat satu rencana asuhan yang hidup
CREATE UNIQUE INDEX "UX_TrxNursingCarePlan_Episode_Active"
    ON "TrxNursingCarePlan" ("InpEpisodeId")
    WHERE "IsDelete" = false;

-- Satu tindakan tersimpan sekali walaupun permintaannya diulang
CREATE UNIQUE INDEX "UX_TrxNursingIntervention_IdempotencyKey"
    ON "TrxNursingIntervention" ("IdempotencyKey")
    WHERE "IdempotencyKey" IS NOT NULL AND "IsDelete" = false;

-- Satu nomor versi tepat satu baris per butir
CREATE UNIQUE INDEX "UX_TrxNursingCarePlanItemRevision_Item_Version"
    ON "TrxNursingCarePlanItemRevision" ("CarePlanItemId", "VersionNumber");
```

---

## 10. Tabel yang **tidak** dibuat

| Yang tidak dibuat | Alasan |
| --- | --- |
| `InpNursingAssessment` atau `Inp*` apa pun untuk dokumentasi klinis | `RWI-DEC-081`, PRD 23.1 |
| Salinan master alat | PRD 20 aturan 2 melarangnya |
| Tabel asuhan gizi | PRD 23.1 menaruhnya pada modul Gizi |
| Tabel pemakaian alat | **`DEFERRED`** — `RWI-DEC-089` mengeluarkannya dari scope rilis pertama; pemiliknya sengaja tidak diputuskan |
