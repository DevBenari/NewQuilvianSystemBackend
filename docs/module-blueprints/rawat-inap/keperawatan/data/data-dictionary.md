# Kamus Data — Sub-modul `keperawatan` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `keperawatan` |
| Revision | **`0.4`** — bagian 11 penyelarasan `PRD-RWI-V2-001`, blueprint revision `7` |
| Status | **`draft`** untuk `0.4` |
| Tanggal | 2 September 2026; amandemen `0.4` 15 September 2026 |
| Sumber | [`../02-backend-architecture.md`](../02-backend-architecture.md) bagian 4; untuk `0.4` bagian 11 |

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

---

## 11. Amandemen revision `0.4` — penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

| Field | Nilai |
| --- | --- |
| Sumber | [`../02-backend-architecture.md`](../02-backend-architecture.md) revision `0.4` bagian 11 |
| Status | **`draft`** — belum disetujui manusia |
| Keputusan | `RWI-DEC-100`, `116` s.d. `120`, `124`, `131`, `136`, `141`, `145` s.d. `149` |

Sepuluh kolom warisan `IdentityModel` pada bagian 0 berlaku juga untuk setiap tabel baru di bawah. Seluruh tabel
memakai `Id uuid` sebagai primary key; baris `Id` tidak diulang.

**Koreksi nama.** Bagian 1, 3 s.d. 6, dan 9 menulis `TrxNursingCarePlan`, `TrxNursingCarePlanItem`,
`TrxNursingCarePlanItemRevision`, `TrxNursingIntervention`. Nama tabel yang benar-benar dibangun `BE-RWI-054` s.d. `065`
adalah **`CliNursingCarePlan`**, **`CliNursingCarePlanItem`**, **`CliNursingCarePlanItemRevision`**,
**`CliNursingIntervention`**. Kolomnya tidak berubah.

### 11.0 Status dan kepemilikan tabel pada `0.4`

| Tabel | Status | Modul pemilik | Kemampuan | Rincian |
| --- | --- | --- | --- | --- |
| `TrxPatientAssessment` | **`Diperbarui`** — 3 nilai enum, 3 kolom | `ClinicalManagement` | `CAP-012` | 11.1 |
| `TrxPatientVitalSign` | **`Diperbarui`** — 1 kolom | `ClinicalManagement` | `CAP-012` | 11.2 |
| `TrxPatientAllergy` | **`Diperbarui`** — 2 kolom | `ClinicalManagement` | `CAP-023-MAR` | 11.3 |
| `CliClinicalInstrument` | **`Baru`** | `ClinicalManagement` | `CAP-012` | 11.4 |
| `CliClinicalInstrumentVersion` | **`Baru`** | `ClinicalManagement` | `CAP-012` | 11.5 |
| `CliAssessmentInstrumentResponse` | **`Baru`** | `ClinicalManagement` | `CAP-012` | 11.6 |
| `CliCaseManagementEvaluation` | **`Baru`** | `ClinicalManagement` | `CAP-012` Evaluasi Awal | 11.7 |
| `CliFluidBalanceEntry` | **`Baru`** | `ClinicalManagement` | `CAP-012` Pengawasan Harian | 11.8 |
| `CliFluidBalanceEntryRevision` | **`Baru`** | `ClinicalManagement` | `CAP-012` | 11.8 |
| `CliBloodGlucoseReading` | **`Baru`** | `ClinicalManagement` | `CAP-012`, dibaca `CAP-023-MAR` | 11.9 |
| `CliBloodGlucoseReadingRevision` | **`Baru`** | `ClinicalManagement` | `CAP-012` | 11.9 |
| `CliDailyObservation` | **`Baru`** | `ClinicalManagement` | `CAP-012` | 11.10 |
| `CliDailyObservationRevision` | **`Baru`** | `ClinicalManagement` | `CAP-012` | 11.10 |
| `CliNursingShift` | **`Baru`** | `ClinicalManagement` | `CAP-012` total per shift | 11.11 |
| `PhmMedicationAdministration` | **`Baru`** | `PharmacyManagement` | `CAP-023-MAR` | 11.12 |
| `PhmMedicationAdministrationRevision` | **`Baru`** | `PharmacyManagement` | `CAP-023-MAR` | 11.13 |
| `PhmMedicationScheduleTime` | **`Baru`** | `PharmacyManagement` | `CAP-023-MAR` | 11.14 |
| `PhmMedicationAdministrationSetting` | **`Baru`** | `PharmacyManagement` | `CAP-023-MAR` | 11.14 |
| `PhmSlidingScaleExecution` | **`Baru`** | `PharmacyManagement` | `CAP-023-MAR` | 11.15 |
| `PhmPrescriptionItem`, `PhmSlidingScaleOrder`, `PhmSlidingScaleOrderVersion`, `PhmSlidingScaleRange`, `PhmMedicationReconciliationItem`, `TrxPatientIntegratedProgressNote.NoteKind` | Dirancang **`dokter-rawat-inap`** | `PharmacyManagement`, `ClinicalManagement` | `CAP-023-RSP`, `CAP-021` | Dirujuk — `../../dokter-rawat-inap/data/data-dictionary.md` bagian 13 |
| `InpEpisode`, `MstServiceUnit`, `MstDrug`, `MstEmployee` | `Sudah ada` | Pemilik masing-masing | Konteks | 11.16 |

### 11.1 `TrxPatientAssessment` — `Diperbarui` — sumber lengkap `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `AssessmentType` | `integer` | Ya | `0` | Sudah ada | — | — | Tidak | **Nilai baru:** `6` `FallRisk`, `7` `PainMonitoring`, `8` `EducationAssessment`. `2` `DailyReassessment` tidak dipakai jalur V2 |
| `PainAssessmentState` | `integer` | Ya | `0` `NotAssessed` | — | — | — | Tidak | `1` `NoPain`, `2` `HasPain`, `3` `UnableToAssess`. Pada Monitoring Nyeri wajib selain `NotAssessed` sebelum selesai |
| `PainReassessmentDueAt` | `timestamptz` | Tidak | `null` | `IX_TrxPatientAssessment_Episode_PainDue` (`InpEpisodeId`, `PainReassessmentDueAt`) parsial `WHERE PainReassessmentDueAt IS NOT NULL` | — | — | Tidak | Waktu kajian ulang nyeri setelah intervensi; interval dari instrumen nyeri yang disahkan (gate `G-04`). Contoh: nyeri 7 pukul 08.00, kajian ulang 60 menit → `09.00` |
| `VitalSignId` | `uuid` | Tidak | `null` | `IX_TrxPatientAssessment_VitalSignId` | FK ke `TrxPatientVitalSign.Id` | `SetNull` | Tidak | Kondisi Umum Kajian Umum menunjuk satu baris tanda vital — **bukan** menyalin angkanya |

**Kolom lama yang dipakai typed, tidak dipindah ke jawaban JSON:**

| Dokumen V2 | Kolom lama yang dipakai | Kolom lama yang **tidak** diisi jalur rawat inap V2 |
| --- | --- | --- |
| Kajian Umum | `ChiefComplaint`, `CurrentIllnessHistory`, `MedicationHistory`, `ConsciousnessStatus`, `IsUsingOxygen`, `OxygenSupportType`, `OxygenFlowRate`, `HasAllergy`, `AllergyNote`, `AppetiteStatus`, `HasNausea`, `HasVomiting`, `NutritionRiskStatus`, `NutritionRiskScore`, `FunctionalStatus`, `FunctionalNote`, `PsychosocialNote`, `NurseNote` | `BloodPressureSystolic`, `BloodPressureDiastolic`, `PulseRate`, `RespiratoryRate`, `Temperature`, `OxygenSaturation`, `Weight`, `Height`, `BMI`, `MeanArterialPressure`, `EarlyWarningScore` — dibaca dari `VitalSignId` |
| Resiko Jatuh | `FallRiskStatus` (diisi dari pita instrumen), `FallRiskScore` (diisi dari skor instrumen), `FallRiskNote` | `HasFallRisk`, `HasAtaxia`, `HasPosturalInstability` — dua centang V2 lama tidak dipakai lagi pada jalur rawat inap (`RWI-FACT-036`) |
| Monitoring Nyeri | `PainScale`, `PainTrigger`, `PainQuality`, `PainLocation`, `PainFrequency`, `PainManagement`, `PainNote` | `HasPain` — digantikan `PainAssessmentState` |
| Assesment Edukasi | `EducationNote` | — |
| Perencanaan Pulang | `NurseNote` | — |

Pengkajian poliklinik dan IGD **tidak berubah**; aturan kolom di atas hanya berlaku bagi baris dengan `InpEpisodeId` terisi
dan jenis dokumen V2.

### 11.2 `TrxPatientVitalSign` — `Diperbarui` — sumber lengkap `Areas/HealthServices/ClinicalManagement/Models/TrxPatientVitalSign.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `IX_TrxPatientVitalSign_InpEpisodeId_ObservationDateTime` | FK ke `InpEpisode.Id` | `Restrict` | Tidak | Diisi server dari episode `Admitted` encounter; tidak dipercaya dari klien |

**Kolom lama yang dipakai:** `ObservationDateTime`, `VitalSignSource` = `5` `InpatientObservation`, `VitalSignStatus`,
`ObservedByUserId`, `ConsciousnessStatus` (AVPU dan GCS), `IsCritical`, `NeedDoctorNotification`.
**Kolom lama yang tidak diisi pada baris rawat inap V2:** `HasPain`, `PainScale`, `PainLocation`, `PainNote` — nyeri
tempatnya Monitoring Nyeri (`INV-KEP-04`). Koreksi memakai status `Corrected` dan `EnteredInError` yang sudah ada;
jalur hapus tetap tertutup (`RWI-DEC-098`).

### 11.3 `TrxPatientAllergy` — `Diperbarui` — sumber lengkap `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAllergy.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `InpEpisodeId` | `uuid` | Tidak | `null` | `IX_TrxPatientAllergy_InpEpisodeId` | FK ke `InpEpisode.Id` | `Restrict` | Tidak | Episode tempat reaksi terjadi |
| `SourceMedicationAdministrationId` | `uuid` | Tidak | `null` | `IX_TrxPatientAllergy_SourceMedicationAdministrationId` | FK ke `PhmMedicationAdministration.Id` | `Restrict` | Tidak | Dosis MAR yang diduga memicu reaksi |

**Nilai yang diisi jalur dugaan reaksi obat:** `AllergyCategory = 1` `Drug`, `Certainty = 1` `Suspected`,
`AllergyStatus` aktif, `DrugId` dari dosis, `ReactionDescription` wajib, `IsVerified = false`, `IsAlertEnabled = true`.
Contoh: Budi gatal dan kemerahan 30 menit setelah Ceftriaxone 08.00 → satu baris dugaan tertaut dosis 08.00.

### 11.4 `CliClinicalInstrument` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Code` | `varchar(50)` | Ya | — | `UX_CliClinicalInstrument_Code` parsial `IsDelete = false` | — | — | Tidak | Contoh `FALL_RISK_ADULT` |
| `Name` | `varchar(200)` | Ya | — | — | — | — | Tidak | "Risiko Jatuh Dewasa" |
| `InstrumentKind` | `integer` | Ya | — | `IX_CliClinicalInstrument_Kind` | — | — | Tidak | `1` `GeneralNursingAssessmentForm`, `2` `FallRiskScale`, `3` `PainScale`, `4` `EducationAssessmentForm`, `5` `DischargePlanningForm`, `6` `CaseManagementChecklist` |
| `TargetMinAgeMonths` | `integer` | Tidak | `null` | — | — | — | Tidak | Batas usia bawah inklusif. Dewasa 216 bulan (18 tahun) |
| `TargetMaxAgeMonths` | `integer` | Tidak | `null` | — | — | — | Tidak | Batas usia atas eksklusif; `null` = terbuka. Instrumen sejenis yang aktif tidak boleh bertumpuk usia |
| `Description` | `varchar(1000)` | Tidak | `null` | — | — | — | Tidak | — |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | — |

### 11.5 `CliClinicalInstrumentVersion` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `InstrumentId` | `uuid` | Ya | — | `UX_CliClinicalInstrumentVersion_Instrument_Version` (`InstrumentId`, `VersionNumber`) | FK ke `CliClinicalInstrument.Id` | `Restrict` | Tidak | — |
| `VersionNumber` | `integer` | Ya | — | Bagian unique di atas | — | — | Tidak | Mulai 1 |
| `VersionStatus` | `integer` | Ya | `1` `Draft` | `UX_CliClinicalInstrumentVersion_Approved` (`InstrumentId`) parsial `WHERE VersionStatus = 2 AND IsDelete = false` | — | — | Tidak | `2` `Approved`, `3` `Retired`. Satu versi sah per instrumen; mengesahkan versi baru memensiunkan versi lama dalam transaksi yang sama |
| `DefinitionJson` | `jsonb` | Ya | — | — | — | — | Tidak | Bentuk di bawah. Beku setelah `Approved` |
| `DefinitionHash` | `char(64)` | Ya | — | — | — | — | Tidak | SHA-256 dari `DefinitionJson` ternormalisasi |
| `LastModifiedByUserId` | `uuid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | Pengubah terakhir |
| `LastModifiedAt` | `timestamptz` | Ya | — | — | — | — | Tidak | — |
| `ApprovedByUserId` | `uuid` | Tidak | `null` | — | FK ke `ApplicationUser` | `Restrict` | Tidak | **Wajib berbeda** dari `LastModifiedByUserId` |
| `ApprovedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | — |
| `ApprovalNote` | `varchar(500)` | Tidak | `null` | — | — | — | Tidak | Contoh "Disahkan Komite Keperawatan rapat 12/2026" |
| `RetiredAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | — |

**Bentuk `DefinitionJson`:**

| Kunci | Tipe | Wajib | Arti | Contoh |
| --- | --- | :---: | --- | --- |
| `sections[]` | larik | Ya | Bagian formulir berurutan | `{"code":"KU","label":"Kondisi Umum"}` |
| `sections[].items[]` | larik | Ya | Isian | `{"code":"KU_CONSCIOUSNESS","label":"Kesadaran","type":"single"}` |
| `items[].type` | teks | Ya | `boolean`, `single`, `multi`, `text`, `number`, `date` | `single` |
| `items[].options[]` | larik | Untuk `single`/`multi` | Pilihan dan nilai skor | `{"code":"YES","label":"Ya","score":25}` |
| `items[].binding` | teks | Tidak | Nama kolom typed `TrxPatientAssessment` yang **menyimpan** isian ini; jawaban tidak masuk `ResponsesJson` | `"ConsciousnessStatus"` |
| `scoring.method` | teks | Untuk skala | `sum` — satu-satunya cara hitung revision ini | `sum` |
| `bands[]` | larik | Untuk skala | `code`, `label`, `minInclusive`, `maxExclusive` (`null` = terbuka), `isAlert`, `mappedFallRiskStatus` | Rendah `[0,25)`, Sedang `[25,45)`, Tinggi `[45,null)` |
| `requiredItemCodes[]` | larik | Ya, boleh kosong | Isian wajib sebelum dokumen selesai | `[]` sampai `RWI-OQ-057` |
| `reassessmentMinutes` | angka | Tidak | Interval kajian ulang nyeri setelah intervensi | `60` |

Validasi simpan: kode unik dalam versi; `binding` hanya ke daftar kolom yang diizinkan 11.1; pita tidak bertumpuk dan
tidak berlubang; skor minimum dan maksimum yang mungkin tercakup pita.

### 11.6 `CliAssessmentInstrumentResponse` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `AssessmentId` | `uuid` | Tidak | `null` | `UX_CliAssessmentInstrumentResponse_Assessment_Version` parsial | FK ke `TrxPatientAssessment.Id` | `Restrict` | Tidak | Tepat satu dari dua pemilik — `CHECK (num_nonnulls("AssessmentId","CaseManagementEvaluationId") = 1)` |
| `CaseManagementEvaluationId` | `uuid` | Tidak | `null` | `UX_CliAssessmentInstrumentResponse_Evaluation_Version` parsial | FK ke `CliCaseManagementEvaluation.Id` | `Restrict` | Tidak | — |
| `InstrumentVersionId` | `uuid` | Ya | — | Bagian unique di atas | FK ke `CliClinicalInstrumentVersion.Id` | `Restrict` | Tidak | Versi yang dipakai saat konsep **terakhir** disimpan |
| `DefinitionHashSnapshot` | `char(64)` | Ya | — | — | — | — | Tidak | Salinan hash versi |
| `ResponsesJson` | `jsonb` | Ya | `{}` | — | — | — | **Ya** | Jawaban isian tanpa `binding`. Contoh `{"ELIM_CATHETER":true,"SKIN_INTEGRITY":"PRESSURE_ULCER_G2"}` |
| `TotalScore` | `numeric(8,2)` | Tidak | `null` | — | — | — | Tidak | Hasil hitung server |
| `BandCode` | `varchar(50)` | Tidak | `null` | — | — | — | Tidak | `HIGH` |
| `BandLabelSnapshot` | `varchar(100)` | Tidak | `null` | — | — | — | Tidak | "Tinggi" |
| `IsAlertBand` | `boolean` | Ya | `false` | — | — | — | Tidak | Memicu alert klinis di kepala konteks |
| `ComputedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | — |

### 11.7 `CliCaseManagementEvaluation` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `EvaluationNumber` | `varchar(30)` | Ya | — | `UX_CliCaseManagementEvaluation_Number` | — | — | Tidak | Contoh `MPP-20260915-0001` |
| `EncounterId` | `uuid` | Ya | — | — | FK ke `RegPatientEncounter.Id` | `Restrict` | Tidak | — |
| `InpEpisodeId` | `uuid` | Ya | — | `UX_CliCaseManagementEvaluation_Episode_Active` (`InpEpisodeId`) parsial `WHERE EvaluationStatus IN (1,2) AND IsDelete = false` | FK ke `InpEpisode.Id` | `Restrict` | Tidak | Satu dokumen hidup per episode — usulan gate `G-09` |
| `PatientId` | `uuid` | Ya | — | — | FK ke `MstPatient.Id` | `Restrict` | Tidak | — |
| `ServiceUnitIdSnapshot` | `uuid` | Ya | — | — | — | — | Tidak | Unit episode saat konsep dibuat; kewenangan dibaca ulang dari unit **saat simpan** |
| `EvaluationStatus` | `integer` | Ya | `1` `Draft` | — | — | — | Tidak | `2` `Completed`, `3` `Cancelled` |
| `AuthorEmployeeId` | `uuid` | Ya | — | — | FK ke `MstEmployee.Id` | `Restrict` | Tidak | Dari akun login |
| `AuthorUserId` | `uuid` | Ya | — | — | FK ke `ApplicationUser` | `Restrict` | Tidak | — |
| `ClinicalDateTime` | `timestamptz` | Ya | — | — | — | — | Tidak | — |
| `CompletedAt`, `CompletedByUserId` | `timestamptz`, `uuid` | Tidak | `null` | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `CancelledAt`, `CancelledByUserId` | `timestamptz`, `uuid` | Tidak | `null` | — | FK `ApplicationUser` | `Restrict` | Tidak | Hanya dari `Draft` |
| `CancelReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Wajib bila `Cancelled` |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | Unique parsial | — | — | Tidak | — |

Isi delapan bagian disimpan sebagai `CliAssessmentInstrumentResponse` dengan instrumen `CaseManagementChecklist`.

### 11.8 `CliFluidBalanceEntry` dan `CliFluidBalanceEntryRevision` — `Baru`

**`CliFluidBalanceEntry`**

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `EncounterId`, `PatientId` | `uuid` | Ya | — | — | FK | `Restrict` | Tidak | — |
| `InpEpisodeId` | `uuid` | Ya | — | `IX_CliFluidBalanceEntry_Episode_EntryDateTime` (`InpEpisodeId`, `EntryDateTime`) | FK ke `InpEpisode.Id` | `Restrict` | Tidak | — |
| `Direction` | `integer` | Ya | — | — | — | — | Tidak | `1` `Intake`, `2` `Output` |
| `SourceCategory` | `integer` | Ya | — | — | — | — | Tidak | Masuk `1`–`5`, keluar `11`–`19`; harus sejalan dengan `Direction` |
| `SourceDetail` | `varchar(200)` | Tidak | `null` | — | — | — | Tidak | "RL 500 ml tetes 20" |
| `VolumeMl` | `numeric(9,2)` | Ya | — | — | — | — | Tidak | `> 0` dan `≤ 10000` per entri |
| `EntryDateTime` | `timestamptz` | Ya | — | Bagian index | — | — | Tidak | Tidak boleh di masa depan; tidak sebelum waktu masuk episode |
| `RecordedByEmployeeId`, `RecordedByUserId` | `uuid` | Ya | — | — | FK | `Restrict` | Tidak | Dari akun login |
| `MedicationAdministrationId` | `uuid` | Tidak | `null` | `UX_CliFluidBalanceEntry_MedicationAdministration_Active` parsial `WHERE MedicationAdministrationId IS NOT NULL AND EntryStatus = 1 AND IsDelete = false` | FK ke `PhmMedicationAdministration.Id` | `Restrict` | Tidak | **Wajib** bila `SourceCategory = 5`; dilarang untuk sumber lain |
| `EntryStatus` | `integer` | Ya | `1` `Active` | — | — | — | Tidak | `2` `Cancelled` |
| `RevisionNumber` | `integer` | Ya | `0` | — | — | — | Tidak | Naik tiap koreksi; dipakai sebagai pemeriksaan keserentakan |
| `CancelReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | — |
| `DoseCorrectionFlaggedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | Diisi saat dosis tertaut dikoreksi menjadi selain `Administered` — usulan `G-26` |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | Unique parsial | — | — | Tidak | — |

**`CliFluidBalanceEntryRevision`** — `EntryId` (FK `Restrict`), `RevisionNumber` (unique bersama `EntryId`),
`PreviousVolumeMl numeric(9,2)`, `PreviousEntryDateTime timestamptz`, `PreviousSourceCategory integer`,
`PreviousSourceDetail varchar(200)`, `CorrectionReason varchar(500)` wajib **Sensitif**, `CorrectedByUserId uuid` wajib,
`CorrectedAt timestamptz` wajib. Hanya ditambah, tidak pernah diubah.

**Total dihitung, tidak disimpan.** Contoh 24 jam Budi: masuk infus 1.500 + oral 600 + obat 200 + darah 200 = 2.500 ml;
keluar urin 1.800 + drain 150 = 1.950 ml; balance **+550 ml**.

### 11.9 `CliBloodGlucoseReading` dan `CliBloodGlucoseReadingRevision` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `EncounterId`, `PatientId` | `uuid` | Ya | — | — | FK | `Restrict` | Tidak | — |
| `InpEpisodeId` | `uuid` | Ya | — | `IX_CliBloodGlucoseReading_Episode_MeasuredAt` | FK ke `InpEpisode.Id` | `Restrict` | Tidak | — |
| `MeasuredAt` | `timestamptz` | Ya | — | Bagian index | — | — | Tidak | Tidak di masa depan |
| `GlucoseValue` | `numeric(7,2)` | Ya | — | — | — | — | Tidak | mg/dL `10`–`1000`; mmol/L `0,6`–`55,5` |
| `GlucoseUnit` | `integer` | Ya | **tanpa bawaan** | — | — | — | Tidak | `1` `MgPerDl`, `2` `MmolPerL` — gate `G-25` |
| `Method` | `integer` | Ya | `1` `WardGlucometer` | — | — | — | Tidak | Satu-satunya nilai; hasil laboratorium tidak pernah disimpan di sini |
| `RecordedByEmployeeId`, `RecordedByUserId` | `uuid` | Ya | — | — | FK | `Restrict` | Tidak | — |
| `ReadingStatus` | `integer` | Ya | `1` `Active` | — | — | — | Tidak | `2` `Cancelled` — pembatalan ditolak bila GDS sudah dipakai pelaksanaan sliding scale |
| `RevisionNumber` | `integer` | Ya | `0` | — | — | — | Tidak | — |
| `CancelReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | — |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | Unique parsial | — | — | Tidak | — |

**`CliBloodGlucoseReadingRevision`** — `ReadingId`, `RevisionNumber` (unique bersama), `PreviousValue numeric(7,2)`,
`PreviousUnit integer`, `PreviousMeasuredAt timestamptz`, `CorrectionReason varchar(500)` wajib **Sensitif**,
`CorrectedByUserId`, `CorrectedAt`.

### 11.10 `CliDailyObservation` dan `CliDailyObservationRevision` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `EncounterId`, `PatientId` | `uuid` | Ya | — | — | FK | `Restrict` | Tidak | — |
| `InpEpisodeId` | `uuid` | Ya | — | `IX_CliDailyObservation_Episode_ObservedAt` | FK ke `InpEpisode.Id` | `Restrict` | Tidak | — |
| `ObservedAt` | `timestamptz` | Ya | — | Bagian index | — | — | Tidak | — |
| `DietIntakePercent` | `integer` | Tidak | `null` | — | — | — | Tidak | `0`–`100`. "Makan habis ¾ porsi" → `75` |
| `DietNote` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | — |
| `MobilizationLevel` | `integer` | Ya | `0` `NotAssessed` | — | — | — | Tidak | `1` `Bedrest`, `2` `SitInBed`, `3` `AssistedWalking`, `4` `Independent` |
| `AbdominalCircumferenceCm` | `numeric(5,1)` | Tidak | `null` | — | — | — | Tidak | `20`–`250` |
| `IsAgitated` | `boolean` | Tidak | `null` | — | — | — | Tidak | `null` = belum dinilai, bukan "tidak agitasi" |
| `Note` | `varchar(1000)` | Tidak | `null` | — | — | — | **Ya** | — |
| `RecordedByEmployeeId`, `RecordedByUserId` | `uuid` | Ya | — | — | FK | `Restrict` | Tidak | — |
| `ObservationStatus` | `integer` | Ya | `1` `Active` | — | — | — | Tidak | `2` `Cancelled` |
| `RevisionNumber` | `integer` | Ya | `0` | — | — | — | Tidak | — |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | Unique parsial | — | — | Tidak | — |

**`CliDailyObservationRevision`** — `ObservationId`, `RevisionNumber` (unique bersama), `PreviousValuesJson jsonb` wajib
**Sensitif** (nilai seluruh isian sebelum koreksi), `CorrectionReason varchar(500)` wajib **Sensitif**,
`CorrectedByUserId`, `CorrectedAt`.

### 11.11 `CliNursingShift` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `ServiceUnitId` | `uuid` | Tidak | `null` | `UX_CliNursingShift_Unit_Code` (`ServiceUnitId`, `ShiftCode`) `NULLS NOT DISTINCT` parsial `IsDelete = false` | FK ke `MstServiceUnit.Id` | `Restrict` | Tidak | `null` = bawaan seluruh unit |
| `ShiftCode` | `varchar(20)` | Ya | — | Bagian unique | — | — | Tidak | `PAGI` |
| `ShiftName` | `varchar(50)` | Ya | — | — | — | — | Tidak | "Pagi" |
| `StartTime` | `time` | Ya | — | — | — | — | Tidak | `07:00` |
| `EndTime` | `time` | Ya | — | — | — | — | Tidak | `14:00`; lebih kecil dari `StartTime` berarti melewati tengah malam |
| `SortOrder` | `integer` | Ya | `0` | — | — | — | Tidak | — |
| `IsActive` | `boolean` | Ya | `true` | — | — | — | Tidak | Shift aktif satu unit wajib menutup 24 jam tanpa tumpang tindih |

### 11.12 `PhmMedicationAdministration` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `AdministrationNumber` | `varchar(30)` | Ya | — | Unique | — | — | Tidak | `MAR-20260915-000123` |
| `PrescriptionId` | `uuid` | Ya | — | — | FK ke `PhmPrescription.Id` | `Restrict` | Tidak | — |
| `PrescriptionItemId` | `uuid` | Ya | — | `UX_PhmMedicationAdministration_Item_ScheduledAt` (`PrescriptionItemId`, `ScheduledAt`) parsial `WHERE DoseSource = 1 AND IsDelete = false` | FK ke `PhmPrescriptionItem.Id` | `Restrict` | Tidak | Satu slot jadwal satu baris |
| `EncounterId`, `PatientId`, `DrugId` | `uuid` | Ya | — | — | FK | `Restrict` | Tidak | — |
| `InpEpisodeId` | `uuid` | Ya | — | `IX_PhmMedicationAdministration_Episode_ScheduledAt` (`InpEpisodeId`, `ScheduledAt`) | FK ke `InpEpisode.Id` | `Restrict` | Tidak | — |
| `DoseSource` | `integer` | Ya | — | — | — | — | Tidak | `1` `Scheduled`, `2` `AsNeeded`, `3` `SlidingScale` |
| `ScheduledAt` | `timestamptz` | Tidak | `null` | Bagian index | — | — | Tidak | Wajib untuk `Scheduled` |
| `DoseStatus` | `integer` | Ya | `1` `Due` | `IX_PhmMedicationAdministration_Episode_Status` | — | — | Tidak | `2` `Administered`, `3` `Held`, `4` `Refused`, `5` `Missed`, `6` `Cancelled` |
| `StatusReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Wajib untuk `Held`, `Refused`, `Missed`, `Cancelled` |
| `PlannedDose` | `numeric(12,4)` | Tidak | `null` | — | — | — | Tidak | Dari butir resep; untuk sliding scale dari dosis hitung |
| `PlannedDoseUnitSnapshot` | `varchar(50)` | Tidak | `null` | — | — | — | Tidak | "g", "unit" |
| `ActualDose` | `numeric(12,4)` | Tidak | `null` | — | — | — | Tidak | Wajib untuk `Administered` |
| `ActualDoseUnitSnapshot` | `varchar(50)` | Tidak | `null` | — | — | — | Tidak | — |
| `ActualRouteSnapshot` | `varchar(100)` | Tidak | `null` | — | — | — | Tidak | Wajib untuk `Administered` |
| `AdministeredAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | Waktu aktual; tidak di masa depan |
| `RecordedByEmployeeId`, `RecordedByUserId` | `uuid` | Tidak | `null` | — | FK | `Restrict` | Tidak | Pencatat pertama, dari akun login |
| `RecordedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | — |
| `DeviationNote` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Wajib bila `ActualDose` ≠ `PlannedDose` atau `AdministeredAt` di luar ±60 menit dari `ScheduledAt` |
| `IsHighAlertSnapshot` | `boolean` | Ya | — | — | — | — | Tidak | Salinan `PhmPrescriptionItem.IsHighAlertSnapshot` |
| `DoubleCheckStatus` | `integer` | Ya | `0` `NotRequired` | `IX_PhmMedicationAdministration_Episode_DoubleCheck` parsial `WHERE DoubleCheckStatus = 1` | — | — | Tidak | `1` `Pending`, `2` `Confirmed`, `3` `Rejected` |
| `DoubleCheckedByEmployeeId`, `DoubleCheckedByUserId` | `uuid` | Tidak | `null` | — | FK | `Restrict` | Tidak | Wajib berbeda dari `RecordedByUserId` |
| `DoubleCheckedAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | — |
| `DoubleCheckNote` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Wajib bila `Rejected` |
| `PrnIndication` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Wajib untuk `AsNeeded` dari butir `IsAsNeeded = true`. "Nyeri skala 6". Pemberian butir berfrekuensi tanpa jadwal terkonfigurasi memakai `DeviationNote` sebagai alasan |
| `PrnEvaluationDueAt` | `timestamptz` | Tidak | `null` | — | — | — | Tidak | `AdministeredAt` + interval bila dikonfigurasi |
| `PrnEvaluationNote` | `varchar(1000)` | Tidak | `null` | — | — | — | **Ya** | — |
| `PrnEvaluatedAt`, `PrnEvaluatedByUserId` | `timestamptz`, `uuid` | Tidak | `null` | — | FK | `Restrict` | Tidak | — |
| `RevisionNumber` | `integer` | Ya | `0` | — | — | — | Tidak | Pemeriksaan keserentakan lewat `ExpectedRevisionNumber` |
| `IdempotencyKey` | `varchar(100)` | Tidak | `null` | Unique parsial | — | — | Tidak | — |

**Kolom `PhmPrescriptionItem` yang dibaca:** `FrequencyCode`, `IsAsNeeded`, `IsHighAlertSnapshot`, `Dose`,
`DoseUnitSymbolSnapshot`, `RouteSnapshot`, `IsStopped`, `StoppedAt`, `DoseKind`. **`AdministrationTime` teks bebas tidak
dipakai membentuk dosis** — jadwal hanya dari 11.14.

### 11.13 `PhmMedicationAdministrationRevision` — `Baru`

| Kolom | Tipe | Wajib | Keterangan |
| --- | --- | :---: | --- |
| `AdministrationId` | `uuid` | Ya | FK `Restrict`; unique bersama `RevisionNumber` |
| `RevisionNumber` | `integer` | Ya | Nomor revisi yang **menghasilkan** baris berlaku setelah koreksi |
| `PreviousDoseStatus` | `integer` | Ya | — |
| `PreviousActualDose` | `numeric(12,4)` | Tidak | — |
| `PreviousActualRouteSnapshot` | `varchar(100)` | Tidak | — |
| `PreviousAdministeredAt` | `timestamptz` | Tidak | — |
| `PreviousStatusReason` | `varchar(500)` | Tidak | **Sensitif** |
| `PreviousRecordedByUserId` | `uuid` | Tidak | — |
| `CorrectionReason` | `varchar(500)` | Ya | **Sensitif**. "Salah pilih pasien bed 3, dosis dicatat ulang" |
| `CorrectedByUserId` | `uuid` | Ya | — |
| `CorrectedAt` | `timestamptz` | Ya | — |

### 11.14 `PhmMedicationScheduleTime` dan `PhmMedicationAdministrationSetting` — `Baru`

**`PhmMedicationScheduleTime`**

| Kolom | Tipe | Wajib | Bawaan | Index | Keterangan |
| --- | --- | :---: | --- | --- | --- |
| `FrequencyCode` | `varchar(30)` | Ya | — | `UX_PhmMedicationScheduleTime_Code_Unit_Slot` (`FrequencyCode`, `ServiceUnitId`, `SlotNumber`) `NULLS NOT DISTINCT` parsial | Sama dengan `PhmPrescriptionItem.FrequencyCode` |
| `ServiceUnitId` | `uuid` | Tidak | `null` | Bagian unique; FK `MstServiceUnit` `Restrict` | `null` = bawaan; baris unit mengalahkan bawaan untuk kode yang sama |
| `SlotNumber` | `integer` | Ya | — | Bagian unique | 1, 2, 3 |
| `TimeOfDay` | `time` | Ya | — | — | `08:00` |
| `IsActive` | `boolean` | Ya | `true` | — | Jumlah slot aktif per kode per unit wajib sama dengan kali per hari frekuensinya bila `FrequencyPerDay` terisi |

**`PhmMedicationAdministrationSetting`** — satu baris aktif.

| Kolom | Tipe | Wajib | Bawaan | Keterangan |
| --- | --- | :---: | --- | --- |
| `DoseGenerationHorizonHours` | `integer` | Ya | `24` | `1`–`72` |
| `MissedAfterMinutes` | `integer` | Tidak | `null` | Penanda tampilan "lewat waktu"; `null` = tanpa penanda — gate `G-12` |
| `PrnEvaluationMinutes` | `integer` | Tidak | `null` | Interval evaluasi PRN bawaan — gate `G-14` |
| `IsActive` | `boolean` | Ya | `true` | Unique parsial `WHERE IsActive = true AND IsDelete = false` pada konstanta — satu baris aktif |

### 11.15 `PhmSlidingScaleExecution` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `ExecutionNumber` | `varchar(30)` | Ya | — | Unique | — | — | Tidak | `SSX-20260915-0007` |
| `OrderId` | `uuid` | Ya | — | `IX_PhmSlidingScaleExecution_Order_ExecutedAt` | FK ke `PhmSlidingScaleOrder.Id` | `Restrict` | Tidak | — |
| `OrderVersionId` | `uuid` | Ya | — | — | FK ke `PhmSlidingScaleOrderVersion.Id` | `Restrict` | Tidak | Versi berlaku **saat** hitung |
| `InpEpisodeId` | `uuid` | Ya | — | — | FK ke `InpEpisode.Id` | `Restrict` | Tidak | — |
| `BloodGlucoseReadingId` | `uuid` | Ya | — | `UX_PhmSlidingScaleExecution_Reading_Recorded` parsial `WHERE ExecutionStatus = 1` | FK ke `CliBloodGlucoseReading.Id` | `Restrict` | Tidak | Satu GDS paling banyak satu pelaksanaan tercatat |
| `GlucoseValueSnapshot` | `numeric(7,2)` | Ya | — | — | — | — | Tidak | Salinan saat hitung, **bukan** sumber |
| `GlucoseUnitSnapshot` | `integer` | Ya | — | — | — | — | Tidak | — |
| `MatchedRangeId` | `uuid` | Ya | — | — | FK ke `PhmSlidingScaleRange.Id` | `Restrict` | Tidak | — |
| `ComputedDoseUnits` | `numeric(6,2)` | Ya | — | — | — | — | Tidak | `≥ 0` |
| `MedicationAdministrationId` | `uuid` | Ya | — | `UX_PhmSlidingScaleExecution_MedicationAdministration` | FK ke `PhmMedicationAdministration.Id` | `Restrict` | Tidak | — |
| `IsException` | `boolean` | Ya | `false` | — | — | — | Tidak | Dosis aktual berbeda dari dosis hitung |
| `ExceptionReason` | `varchar(500)` | Tidak | `null` | — | — | — | **Ya** | Wajib bila `IsException = true` |
| `ReadingCorrectedAfterExecution` | `boolean` | Ya | `false` | — | — | — | Tidak | Diisi saat GDS rujukan dikoreksi setelah pelaksanaan — `RWI-DEC-148` (b) |
| `ExecutedByEmployeeId`, `ExecutedByUserId` | `uuid` | Ya | — | — | FK | `Restrict` | Tidak | — |
| `ExecutedAt` | `timestamptz` | Ya | — | — | — | — | Tidak | — |
| `ExecutionStatus` | `integer` | Ya | `1` `Recorded` | — | — | — | Tidak | `2` `Cancelled` — hanya bersama koreksi dosis MAR menjadi `Cancelled` |
| `IdempotencyKey` | `varchar(100)` | Ya | — | Unique | — | — | Tidak | Wajib — tombol ganda tidak boleh memberi insulin dua kali |

### 11.16 Tabel `Sudah ada` yang dipakai isi baru — kolom kunci saja

| Tabel | Kolom kunci | Dipakai untuk | Rujukan model |
| --- | --- | --- | --- |
| `InpEpisode` | `Id`, `EncounterId`, `EpisodeStatus`, `ServiceUnitId`, `AdmittedAt` | Episode `Admitted`; unit penempatan perawat dan MPP | `Areas/HealthServices/InPatientManagement/Models/InpEpisode.cs` |
| `MstDrug` | `IsHighAlert` | Sumber `IsHighAlertSnapshot` lewat butir resep | `Areas/HealthServices/MasterData/Models/MstDrug.cs` |
| `PhmPrescriptionItem` | Lihat 11.12 | Pembentukan dosis | `Areas/HealthServices/PharmacyManagement/Models/PhmPrescriptionItem.cs` |
| `MrcClinicalDocumentIntegrity` | `DocumentKind`, `DocumentStatus` | Evaluasi Awal meminta `DocumentKind = 14` — `INT-KEP-12` | `Areas/HealthServices/MedicalRecordManagement/Models/` |

### 11.17 Skema DDL revision `0.4`

> **Peringatan.** Dokumentasi bentuk tabel, bukan skrip yang dijalankan. Skema sungguhan lahir dari EF Core migration
> milik modul pemiliknya. Kolom warisan `IdentityModel` tidak ditulis ulang.

```sql
-- ClinicalManagement: tabel lama
ALTER TABLE public."TrxPatientAssessment" ADD COLUMN "PainAssessmentState" integer NOT NULL DEFAULT 0;
ALTER TABLE public."TrxPatientAssessment" ADD COLUMN "PainReassessmentDueAt" timestamptz NULL;
ALTER TABLE public."TrxPatientAssessment" ADD COLUMN "VitalSignId" uuid NULL
    REFERENCES public."TrxPatientVitalSign" ("Id") ON DELETE SET NULL;
CREATE INDEX "IX_TrxPatientAssessment_VitalSignId" ON public."TrxPatientAssessment" ("VitalSignId");
CREATE INDEX "IX_TrxPatientAssessment_Episode_PainDue"
    ON public."TrxPatientAssessment" ("InpEpisodeId", "PainReassessmentDueAt")
    WHERE "PainReassessmentDueAt" IS NOT NULL AND "IsDelete" = false;

ALTER TABLE public."TrxPatientVitalSign" ADD COLUMN "InpEpisodeId" uuid NULL
    REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT;
CREATE INDEX "IX_TrxPatientVitalSign_InpEpisodeId_ObservationDateTime"
    ON public."TrxPatientVitalSign" ("InpEpisodeId", "ObservationDateTime");

-- Konfigurasi klinis berversi
CREATE TABLE public."CliClinicalInstrument" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "Code" varchar(50) NOT NULL,
    "Name" varchar(200) NOT NULL,
    "InstrumentKind" integer NOT NULL,
    "TargetMinAgeMonths" integer NULL,
    "TargetMaxAgeMonths" integer NULL,
    "Description" varchar(1000) NULL,
    "IsActive" boolean NOT NULL DEFAULT true
);
CREATE UNIQUE INDEX "UX_CliClinicalInstrument_Code" ON public."CliClinicalInstrument" ("Code") WHERE "IsDelete" = false;

CREATE TABLE public."CliClinicalInstrumentVersion" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "InstrumentId" uuid NOT NULL REFERENCES public."CliClinicalInstrument" ("Id") ON DELETE RESTRICT,
    "VersionNumber" integer NOT NULL,
    "VersionStatus" integer NOT NULL DEFAULT 1,
    "DefinitionJson" jsonb NOT NULL,
    "DefinitionHash" char(64) NOT NULL,
    "LastModifiedByUserId" uuid NOT NULL,
    "LastModifiedAt" timestamptz NOT NULL,
    "ApprovedByUserId" uuid NULL,
    "ApprovedAt" timestamptz NULL,
    "ApprovalNote" varchar(500) NULL,
    "RetiredAt" timestamptz NULL,
    CONSTRAINT "CK_CliClinicalInstrumentVersion_ApproverDiffers"
        CHECK ("ApprovedByUserId" IS NULL OR "ApprovedByUserId" <> "LastModifiedByUserId")
);
CREATE UNIQUE INDEX "UX_CliClinicalInstrumentVersion_Instrument_Version"
    ON public."CliClinicalInstrumentVersion" ("InstrumentId", "VersionNumber");
CREATE UNIQUE INDEX "UX_CliClinicalInstrumentVersion_Approved"
    ON public."CliClinicalInstrumentVersion" ("InstrumentId") WHERE "VersionStatus" = 2 AND "IsDelete" = false;

CREATE TABLE public."CliCaseManagementEvaluation" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "EvaluationNumber" varchar(30) NOT NULL UNIQUE,
    "EncounterId" uuid NOT NULL,
    "InpEpisodeId" uuid NOT NULL REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    "PatientId" uuid NOT NULL,
    "ServiceUnitIdSnapshot" uuid NOT NULL,
    "EvaluationStatus" integer NOT NULL DEFAULT 1,
    "AuthorEmployeeId" uuid NOT NULL,
    "AuthorUserId" uuid NOT NULL,
    "ClinicalDateTime" timestamptz NOT NULL,
    "CompletedAt" timestamptz NULL,
    "CompletedByUserId" uuid NULL,
    "CancelledAt" timestamptz NULL,
    "CancelledByUserId" uuid NULL,
    "CancelReason" varchar(500) NULL,                               -- SENSITIF
    "IdempotencyKey" varchar(100) NULL
);
CREATE UNIQUE INDEX "UX_CliCaseManagementEvaluation_Episode_Active"
    ON public."CliCaseManagementEvaluation" ("InpEpisodeId")
    WHERE "EvaluationStatus" IN (1, 2) AND "IsDelete" = false;

CREATE TABLE public."CliAssessmentInstrumentResponse" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "AssessmentId" uuid NULL REFERENCES public."TrxPatientAssessment" ("Id") ON DELETE RESTRICT,
    "CaseManagementEvaluationId" uuid NULL REFERENCES public."CliCaseManagementEvaluation" ("Id") ON DELETE RESTRICT,
    "InstrumentVersionId" uuid NOT NULL REFERENCES public."CliClinicalInstrumentVersion" ("Id") ON DELETE RESTRICT,
    "DefinitionHashSnapshot" char(64) NOT NULL,
    "ResponsesJson" jsonb NOT NULL DEFAULT '{}'::jsonb,              -- SENSITIF
    "TotalScore" numeric(8,2) NULL,
    "BandCode" varchar(50) NULL,
    "BandLabelSnapshot" varchar(100) NULL,
    "IsAlertBand" boolean NOT NULL DEFAULT false,
    "ComputedAt" timestamptz NULL,
    CONSTRAINT "CK_CliAssessmentInstrumentResponse_OneOwner"
        CHECK (num_nonnulls("AssessmentId", "CaseManagementEvaluationId") = 1)
);
CREATE UNIQUE INDEX "UX_CliAssessmentInstrumentResponse_Assessment_Version"
    ON public."CliAssessmentInstrumentResponse" ("AssessmentId", "InstrumentVersionId")
    WHERE "AssessmentId" IS NOT NULL AND "IsDelete" = false;
CREATE UNIQUE INDEX "UX_CliAssessmentInstrumentResponse_Evaluation_Version"
    ON public."CliAssessmentInstrumentResponse" ("CaseManagementEvaluationId", "InstrumentVersionId")
    WHERE "CaseManagementEvaluationId" IS NOT NULL AND "IsDelete" = false;

-- PharmacyManagement: MAR
CREATE TABLE public."PhmMedicationAdministration" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "AdministrationNumber" varchar(30) NOT NULL UNIQUE,
    "PrescriptionId" uuid NOT NULL REFERENCES public."PhmPrescription" ("Id") ON DELETE RESTRICT,
    "PrescriptionItemId" uuid NOT NULL REFERENCES public."PhmPrescriptionItem" ("Id") ON DELETE RESTRICT,
    "EncounterId" uuid NOT NULL,
    "InpEpisodeId" uuid NOT NULL REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    "PatientId" uuid NOT NULL,
    "DrugId" uuid NOT NULL REFERENCES public."MstDrug" ("Id") ON DELETE RESTRICT,
    "DoseSource" integer NOT NULL,
    "ScheduledAt" timestamptz NULL,
    "DoseStatus" integer NOT NULL DEFAULT 1,
    "StatusReason" varchar(500) NULL,                                -- SENSITIF
    "PlannedDose" numeric(12,4) NULL,
    "PlannedDoseUnitSnapshot" varchar(50) NULL,
    "ActualDose" numeric(12,4) NULL,
    "ActualDoseUnitSnapshot" varchar(50) NULL,
    "ActualRouteSnapshot" varchar(100) NULL,
    "AdministeredAt" timestamptz NULL,
    "RecordedByEmployeeId" uuid NULL,
    "RecordedByUserId" uuid NULL,
    "RecordedAt" timestamptz NULL,
    "DeviationNote" varchar(500) NULL,                               -- SENSITIF
    "IsHighAlertSnapshot" boolean NOT NULL,
    "DoubleCheckStatus" integer NOT NULL DEFAULT 0,
    "DoubleCheckedByEmployeeId" uuid NULL,
    "DoubleCheckedByUserId" uuid NULL,
    "DoubleCheckedAt" timestamptz NULL,
    "DoubleCheckNote" varchar(500) NULL,                             -- SENSITIF
    "PrnIndication" varchar(500) NULL,                               -- SENSITIF
    "PrnEvaluationDueAt" timestamptz NULL,
    "PrnEvaluationNote" varchar(1000) NULL,                          -- SENSITIF
    "PrnEvaluatedAt" timestamptz NULL,
    "PrnEvaluatedByUserId" uuid NULL,
    "RevisionNumber" integer NOT NULL DEFAULT 0,
    "IdempotencyKey" varchar(100) NULL,
    CONSTRAINT "CK_PhmMedicationAdministration_ScheduledHasTime"
        CHECK ("DoseSource" <> 1 OR "ScheduledAt" IS NOT NULL),
    CONSTRAINT "CK_PhmMedicationAdministration_DoubleCheckerDiffers"
        CHECK ("DoubleCheckedByUserId" IS NULL OR "DoubleCheckedByUserId" <> "RecordedByUserId")
);
CREATE UNIQUE INDEX "UX_PhmMedicationAdministration_Item_ScheduledAt"
    ON public."PhmMedicationAdministration" ("PrescriptionItemId", "ScheduledAt")
    WHERE "DoseSource" = 1 AND "IsDelete" = false;
CREATE INDEX "IX_PhmMedicationAdministration_Episode_ScheduledAt"
    ON public."PhmMedicationAdministration" ("InpEpisodeId", "ScheduledAt");
CREATE INDEX "IX_PhmMedicationAdministration_Episode_DoubleCheck"
    ON public."PhmMedicationAdministration" ("InpEpisodeId") WHERE "DoubleCheckStatus" = 1;
CREATE UNIQUE INDEX "UX_PhmMedicationAdministration_IdempotencyKey"
    ON public."PhmMedicationAdministration" ("IdempotencyKey") WHERE "IdempotencyKey" IS NOT NULL;

-- ClinicalManagement: Pengawasan Harian (setelah MAR karena FK)
CREATE TABLE public."CliFluidBalanceEntry" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "EncounterId" uuid NOT NULL,
    "InpEpisodeId" uuid NOT NULL REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    "PatientId" uuid NOT NULL,
    "Direction" integer NOT NULL,
    "SourceCategory" integer NOT NULL,
    "SourceDetail" varchar(200) NULL,
    "VolumeMl" numeric(9,2) NOT NULL CHECK ("VolumeMl" > 0 AND "VolumeMl" <= 10000),
    "EntryDateTime" timestamptz NOT NULL,
    "RecordedByEmployeeId" uuid NOT NULL,
    "RecordedByUserId" uuid NOT NULL,
    "MedicationAdministrationId" uuid NULL
        REFERENCES public."PhmMedicationAdministration" ("Id") ON DELETE RESTRICT,
    "EntryStatus" integer NOT NULL DEFAULT 1,
    "RevisionNumber" integer NOT NULL DEFAULT 0,
    "CancelReason" varchar(500) NULL,                                -- SENSITIF
    "DoseCorrectionFlaggedAt" timestamptz NULL,
    "IdempotencyKey" varchar(100) NULL,
    CONSTRAINT "CK_CliFluidBalanceEntry_MedicationLink"
        CHECK (("SourceCategory" = 5) = ("MedicationAdministrationId" IS NOT NULL))
);
CREATE UNIQUE INDEX "UX_CliFluidBalanceEntry_MedicationAdministration_Active"
    ON public."CliFluidBalanceEntry" ("MedicationAdministrationId")
    WHERE "MedicationAdministrationId" IS NOT NULL AND "EntryStatus" = 1 AND "IsDelete" = false;
CREATE INDEX "IX_CliFluidBalanceEntry_Episode_EntryDateTime"
    ON public."CliFluidBalanceEntry" ("InpEpisodeId", "EntryDateTime");

CREATE TABLE public."CliBloodGlucoseReading" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "EncounterId" uuid NOT NULL,
    "InpEpisodeId" uuid NOT NULL REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    "PatientId" uuid NOT NULL,
    "MeasuredAt" timestamptz NOT NULL,
    "GlucoseValue" numeric(7,2) NOT NULL,
    "GlucoseUnit" integer NOT NULL,
    "Method" integer NOT NULL DEFAULT 1,
    "RecordedByEmployeeId" uuid NOT NULL,
    "RecordedByUserId" uuid NOT NULL,
    "ReadingStatus" integer NOT NULL DEFAULT 1,
    "RevisionNumber" integer NOT NULL DEFAULT 0,
    "CancelReason" varchar(500) NULL,                                -- SENSITIF
    "IdempotencyKey" varchar(100) NULL
);
CREATE INDEX "IX_CliBloodGlucoseReading_Episode_MeasuredAt"
    ON public."CliBloodGlucoseReading" ("InpEpisodeId", "MeasuredAt");

-- Tabel revisi Cli*Revision, CliDailyObservation, CliNursingShift, PhmMedicationAdministrationRevision,
-- PhmMedicationScheduleTime, PhmMedicationAdministrationSetting mengikuti kolom 11.8 s.d. 11.14 dengan pola yang sama:
-- FK ON DELETE RESTRICT, unique (induk, RevisionNumber), kolom alasan varchar(500) SENSITIF.

ALTER TABLE public."TrxPatientAllergy" ADD COLUMN "InpEpisodeId" uuid NULL
    REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT;
ALTER TABLE public."TrxPatientAllergy" ADD COLUMN "SourceMedicationAdministrationId" uuid NULL
    REFERENCES public."PhmMedicationAdministration" ("Id") ON DELETE RESTRICT;

-- PharmacyManagement: pelaksanaan sliding scale (setelah tabel order milik dokter-rawat-inap R6)
CREATE TABLE public."PhmSlidingScaleExecution" (
    "Id" uuid NOT NULL PRIMARY KEY,
    "ExecutionNumber" varchar(30) NOT NULL UNIQUE,
    "OrderId" uuid NOT NULL REFERENCES public."PhmSlidingScaleOrder" ("Id") ON DELETE RESTRICT,
    "OrderVersionId" uuid NOT NULL REFERENCES public."PhmSlidingScaleOrderVersion" ("Id") ON DELETE RESTRICT,
    "InpEpisodeId" uuid NOT NULL REFERENCES public."InpEpisode" ("Id") ON DELETE RESTRICT,
    "BloodGlucoseReadingId" uuid NOT NULL REFERENCES public."CliBloodGlucoseReading" ("Id") ON DELETE RESTRICT,
    "GlucoseValueSnapshot" numeric(7,2) NOT NULL,
    "GlucoseUnitSnapshot" integer NOT NULL,
    "MatchedRangeId" uuid NOT NULL REFERENCES public."PhmSlidingScaleRange" ("Id") ON DELETE RESTRICT,
    "ComputedDoseUnits" numeric(6,2) NOT NULL CHECK ("ComputedDoseUnits" >= 0),
    "MedicationAdministrationId" uuid NOT NULL UNIQUE
        REFERENCES public."PhmMedicationAdministration" ("Id") ON DELETE RESTRICT,
    "IsException" boolean NOT NULL DEFAULT false,
    "ExceptionReason" varchar(500) NULL,                             -- SENSITIF
    "ReadingCorrectedAfterExecution" boolean NOT NULL DEFAULT false,
    "ExecutedByEmployeeId" uuid NOT NULL,
    "ExecutedByUserId" uuid NOT NULL,
    "ExecutedAt" timestamptz NOT NULL,
    "ExecutionStatus" integer NOT NULL DEFAULT 1,
    "IdempotencyKey" varchar(100) NOT NULL UNIQUE,
    CONSTRAINT "CK_PhmSlidingScaleExecution_ExceptionReason"
        CHECK ("IsException" = false OR "ExceptionReason" IS NOT NULL)
);
CREATE UNIQUE INDEX "UX_PhmSlidingScaleExecution_Reading_Recorded"
    ON public."PhmSlidingScaleExecution" ("BloodGlucoseReadingId") WHERE "ExecutionStatus" = 1;
```

### 11.18 Tabel yang tidak dibuat pada `0.4`

| Yang tidak dibuat | Alasan |
| --- | --- |
| Tabel per formulir V1 (`InpGeneralAssessment`, `InpEducationAssessment`, dan sejenisnya) | `MVP-RWI-D-009` |
| `InpMedicationAdministration` atau MAR di `ClinicalManagement` | `RWI-DEC-117` |
| `CliSlidingScaleExecution`, `InpSlidingScale*` | `RWI-DEC-147`; `RWI-AC-225` |
| Kolom GDS pada `TrxPatientVitalSign` | `RWI-DEC-148`: satu tempat, tabel sendiri beserta satuan |
| Kolom volume pada `PhmMedicationAdministration` | `RWI-DEC-149` (4) |
| `CliNursingHandover`, tabel transfusi | `DEFERRED` — `RWI-DEC-145` |
| `CliNursingNote` terpisah dari CPPT | `RWI-DEC-115`, `RWI-DEC-140` |
| Salinan `EmgObservationDetail` | `RWI-DEC-081` |

---

## 12. Amandemen revision `0.5` / kontrak `0.6.0` — Finishing Rawat Inap ★ 1 Oktober 2026

### 12.1 Kepala

Seluruh tabel mewarisi `IdentityModel` (`CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, `IsDelete`); kolom itu tidak diulang. Penghapusan bersifat penandaan. Seluruh tabel ber-schema `public`, nama tabel tunggal PascalCase, dan relasi klinis `Restrict`.

### 12.2 Status dan kepemilikan tabel

| Entity | Status | Pemilik | Catatan |
|---|---|---|---|
| `MstMedicalEquipment` | **Baru** | `MasterData` | Prefix `Mst` |
| `MstTariff` | **Diperbarui** | `MasterData` | Satu kolom dari sub-modul ini; tiga kolom dari `episode-rawat-inap` `0.10.0`. Seluruh kolom ditulis di 12.14 sebagai satu-satunya daftar lengkap |
| `CliEquipmentUsage`, `CliEquipmentUsageRevision` | **Baru** | `ClinicalManagement` | — |
| `CliWsdDrain`, `CliWsdReading` | **Baru** | `ClinicalManagement` | — |
| `CliSurgicalSiteSurveillance`, `…Entry`, `…EntryRevision` | **Baru** | `ClinicalManagement` | — |
| `CliTransfusionMonitoring`, `…Point`, `…Reaction` | **Baru** | `ClinicalManagement` | — |
| `GziPatientDiet` | **Diperbarui** | `NutritionManagement` | Tiga kolom verifikasi |
| `BbkTransfusionReactionNotice` | **Baru** | `BloodBankManagement` | Disetujui `RWI-DEC-209` |
| `CliFluidBalanceEntry` | Sudah ada | `ClinicalManagement` | Kunci: `Id`, `InpEpisodeId`, `SourceCategory` (`DrainOrWsd = 14`), `VolumeMl`, `EntryStatus`, `RevisionNumber`. Model `ClinicalManagement/Models/CliFluidBalanceEntry.cs` |
| `TrxNosocomialInfection` | Sudah ada (legacy `Trx*`) | `ClinicalManagement` | Kunci: `Id`, `PatientId`, `EncounterId`, `InfectionType`, `Status`, `OnsetDateTime`. Model `ClinicalManagement/Models/TrxNosocomialInfection.cs`. Tidak diubah bentuknya |
| `TrxPatientVitalSign` | Sudah ada (legacy `Trx*`) | `ClinicalManagement` | Kunci: `EncounterId`, `ObservationDateTime`, `Temperature`, `VitalSignStatus` |
| `CliClinicalInstrument`, `CliClinicalInstrumentVersion` | Sudah ada | `ClinicalManagement` | Kunci: `InstrumentKind`, `VersionStatus`, `DefinitionJson` |
| `BbkBloodUnit` | Sudah ada | `BloodBankManagement` | Kunci: `Id`, `PmiBagNumber`, `BloodComponentId`, `IssuedToPatientId`, `IssuedAt` |
| `InpEpisode`, `MstDoctor` | Sudah ada | Rawat Inap; Human Resource | Dirujuk |

### 12.3 `CliEquipmentUsage` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `InpEpisodeId` | `Guid` | Ya | — | Index | FK `InpEpisode` | `Restrict` | Tidak | — |
| `EncounterId` | `Guid` | Ya | — | Index | FK `RegPatientEncounter` | `Restrict` | Tidak | Kunci tagihan |
| `PatientId` | `Guid` | Ya | — | Index | FK `MstPatient` | `Restrict` | Tidak | — |
| `MedicalEquipmentId` | `Guid` | Ya | — | Index | FK `MstMedicalEquipment` | `Restrict` | Tidak | — |
| `ResponsibleDoctorId` | `Guid` | Ya | — | Index | FK `MstDoctor` | `Restrict` | Tidak | Berpenugasan aktif saat mulai |
| `PerformedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | Dari akun login |
| `StartedAt` | `DateTime` | Ya | — | Index | — | — | Tidak | — |
| `EndedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `Quantity` | `decimal(10,2)?` | Tidak | — | — | — | — | Tidak | Wajib bila satuan `PerUse` |
| `ChargeUnitSnapshot` | `MstEquipmentChargeUnit` (`int`) | Ya | — | — | — | — | Tidak | Dibekukan saat mulai |
| `RoundingRuleSnapshot` | `MstEquipmentRoundingRule` (`int`) | Ya | — | — | — | — | Tidak | Dibekukan saat mulai |
| `BilledUnits` | `decimal(10,2)?` | Tidak | — | — | — | — | Tidak | Dihitung server saat selesai |
| `Status` | `CliEquipmentUsageStatus` (`int`) | Ya | `Running` | Index | — | — | Tidak | — |
| `RequiresNurseReview` | `bool` | Ya | `false` | Index | — | — | Tidak | `true` bila ditutup otomatis saat keluar ruangan |
| `AutoClosedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `CancelReason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | — |
| `Note` | `string(500)?` | Tidak | — | — | — | — | **Ya** | — |
| `RevisionNumber` | `int` | Ya | `1` | — | — | — | Tidak | Naik pada koreksi waktu |
| `Version` | `int` | Ya | `1` | — | — | — | Tidak | Konkurensi optimistis |

Index parsial: `IX_CliEquipmentUsage_Episode_Running` pada (`InpEpisodeId`) `WHERE "Status" = 1`, untuk penutupan saat keluar ruangan.

### 12.4 `CliEquipmentUsageRevision` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EquipmentUsageId` | `Guid` | Ya | — | Unique bersama `RevisionNumber` | FK `CliEquipmentUsage` | `Restrict` | Tidak | — |
| `RevisionNumber` | `int` | Ya | — | Bagian unique | — | — | Tidak | Nomor revisi yang digantikan |
| `PreviousStartedAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `PreviousEndedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `PreviousQuantity` | `decimal(10,2)?` | Tidak | — | — | — | — | Tidak | — |
| `PreviousBilledUnits` | `decimal(10,2)?` | Tidak | — | — | — | — | Tidak | — |
| `Reason` | `string(500)` | Ya | — | — | — | — | **Ya** | — |
| `RevisedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `RevisedAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |

### 12.5 `CliWsdDrain` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `InpEpisodeId` | `Guid` | Ya | — | Index | FK `InpEpisode` | `Restrict` | Tidak | — |
| `EncounterId` | `Guid` | Ya | — | — | FK `RegPatientEncounter` | `Restrict` | Tidak | — |
| `PatientId` | `Guid` | Ya | — | — | FK `MstPatient` | `Restrict` | Tidak | — |
| `DrainLabel` | `string(50)` | Ya | — | — | — | — | Tidak | Misalnya "WSD kanan" |
| `InsertionSite` | `string(100)?` | Tidak | — | — | — | — | **Ya** | Lokasi anatomi |
| `InsertedAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `InitialResidualMl` | `decimal(8,1)` | Ya | `0` | — | — | — | Tidak | Sisa awal; dipakai pembacaan pertama |
| `RemovedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `RemovedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `Status` | `CliWsdDrainStatus` (`int`) | Ya | `Active` | Index | — | — | Tidak | — |
| `RegisteredByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `CorrectionReason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Alasan koreksi terakhir |
| `Version` | `int` | Ya | `1` | — | — | — | Tidak | — |

Unique parsial `IX_CliWsdDrain_Episode_Label_Active` pada (`InpEpisodeId`, `DrainLabel`) `WHERE "Status" = 1`.

### 12.6 `CliWsdReading` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `WsdDrainId` | `Guid` | Ya | — | Index bersama `PeriodEndAt` | FK `CliWsdDrain` | `Restrict` | Tidak | — |
| `InpEpisodeId` | `Guid` | Ya | — | Index | FK `InpEpisode` | `Restrict` | Tidak | — |
| `ShiftId` | `Guid?` | Tidak | — | — | FK `CliNursingShift` | `Restrict` | Tidak | — |
| `PeriodStartAt` | `DateTime` | Ya | — | — | — | — | Tidak | Jam awal |
| `PeriodEndAt` | `DateTime` | Ya | — | Bagian index | — | — | Tidak | Jam akhir |
| `PreviousResidualMl` | `decimal(8,1)` | Ya | — | — | — | — | Tidak | Diisi server: sisa pembacaan aktif sebelumnya atau `InitialResidualMl` |
| `CurrentResidualMl` | `decimal(8,1)` | Ya | — | — | — | — | Tidak | — |
| `DiscardedVolumeMl` | `decimal(8,1)` | Ya | `0` | — | — | — | Tidak | — |
| `IncreaseMl` | `decimal(8,1)` | Ya | — | — | — | — | Tidak | Dihitung server, ≥ 0 |
| `FluidBalanceEntryId` | `Guid` | Ya | — | **Unique** | FK `CliFluidBalanceEntry` | `Restrict` | Tidak | Output `DrainOrWsd` |
| `Status` | `ClinicalMeasurementStatus` (`int`) | Ya | `Active` | — | — | — | Tidak | — |
| `RevisionNumber` | `int` | Ya | `1` | — | — | — | Tidak | — |
| `RecordedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `CancelReason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | — |

### 12.7 `CliSurgicalSiteSurveillance` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `OprCaseId` | `Guid` | Ya | — | **Unique** | FK `OprCase` | `Restrict` | Tidak | Satu formulir per kasus |
| `InpEpisodeId` | `Guid` | Ya | — | Index | FK `InpEpisode` | `Restrict` | Tidak | — |
| `EncounterId` | `Guid` | Ya | — | Index | FK `RegPatientEncounter` | `Restrict` | Tidak | — |
| `PatientId` | `Guid` | Ya | — | Index | FK `MstPatient` | `Restrict` | Tidak | — |
| `InstrumentVersionId` | `Guid` | Ya | — | — | FK `CliClinicalInstrumentVersion` | `Restrict` | Tidak | Versi `Approved` saat dibentuk |
| `SurgeryCompletedAt` | `DateTime` | Ya | — | — | — | — | Tidak | Dari riwayat status kasus OK |
| `DayOneDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `Status` | `CliSurveillanceStatus` (`int`) | Ya | `Active` | Index | — | — | Tidak | — |
| `StoppedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `StoppedOnDayNumber` | `int?` | Tidak | — | — | — | — | Tidak | — |
| `SummaryResponsesJson` | `text` | Ya | `"{}"` | — | — | — | **Ya** | Kultur, serologi |
| `NosocomialInfectionId` | `Guid?` | Tidak | — | Index | FK `TrxNosocomialInfection` | `Restrict` | Tidak | Diisi saat ditandai dicurigai |
| `SuspectedFlaggedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `SuspectedFlaggedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `ReviewNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | — |
| `CancelReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | — |
| `Version` | `int` | Ya | `1` | — | — | — | Tidak | — |

### 12.8 `CliSurgicalSiteSurveillanceEntry` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SurveillanceId` | `Guid` | Ya | — | Unique bersama `DayNumber` | FK `CliSurgicalSiteSurveillance` | `Restrict` | Tidak | — |
| `DayNumber` | `int` | Ya | — | Bagian unique | — | — | Tidak | 1–15 |
| `EntryDate` | `DateOnly` | Ya | — | — | — | — | Tidak | — |
| `ResponsesJson` | `text` | Ya | — | — | — | — | **Ya** | Indikator harian dan tanda per lokasi, divalidasi definisi versi |
| `TemperatureMaxCelsiusSnapshot` | `decimal(4,1)?` | Tidak | — | — | — | — | **Ya** | Dari tanda vital saat disimpan |
| `FeverIndicatorFromVitals` | `bool?` | Tidak | — | — | — | — | Tidak | `null` bila tidak ada pencatatan suhu |
| `RecordedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `RevisionNumber` | `int` | Ya | `1` | — | — | — | Tidak | — |

### 12.9 `CliSurgicalSiteSurveillanceEntryRevision` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EntryId` | `Guid` | Ya | — | Unique bersama `RevisionNumber` | FK `CliSurgicalSiteSurveillanceEntry` | `Restrict` | Tidak | — |
| `RevisionNumber` | `int` | Ya | — | Bagian unique | — | — | Tidak | — |
| `PreviousResponsesJson` | `text` | Ya | — | — | — | — | **Ya** | — |
| `Reason` | `string(500)` | Ya | — | — | — | — | **Ya** | — |
| `RevisedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `RevisedAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |

### 12.10 `CliTransfusionMonitoring` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `InpEpisodeId` | `Guid` | Ya | — | Index | FK `InpEpisode` | `Restrict` | Tidak | — |
| `EncounterId` | `Guid` | Ya | — | — | FK `RegPatientEncounter` | `Restrict` | Tidak | — |
| `PatientId` | `Guid` | Ya | — | Index | FK `MstPatient` | `Restrict` | Tidak | — |
| `BloodUnitId` | `Guid` | Ya | — | Unique parsial `WHERE "Status" <> 4` | FK `BbkBloodUnit` | `Restrict` | Tidak | Kantong dari Bank Darah |
| `ReceivedAtWardAt` | `DateTime` | Ya | — | — | — | — | Tidak | Jam darah diterima di bangsal |
| `TransfusionStartedAt` | `DateTime` | Ya | — | — | — | — | Tidak | Acuan jatuh tempo titik ukur (gate G-22) |
| `Status` | `CliTransfusionMonitoringStatus` (`int`) | Ya | `InProgress` | Index | — | — | Tidak | — |
| `StoppedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `StopReason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | — |
| `CompletedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `PerformedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `CancelReason` | `string(500)?` | Tidak | — | — | — | — | Tidak | — |
| `Version` | `int` | Ya | `1` | — | — | — | Tidak | — |

### 12.11 `CliTransfusionMonitoringPoint` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `MonitoringId` | `Guid` | Ya | — | Unique bersama `PointType` | FK `CliTransfusionMonitoring` | `Restrict` | Tidak | — |
| `PointType` | `CliTransfusionPointType` (`int`) | Ya | — | Bagian unique | — | — | Tidak | — |
| `DueAt` | `DateTime` | Ya | — | — | — | — | Tidak | Mulai + 0, 15, 60, 240 menit |
| `MeasuredAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `SystolicBp` | `int?` | Tidak | — | — | — | — | **Ya** | — |
| `DiastolicBp` | `int?` | Tidak | — | — | — | — | **Ya** | — |
| `TemperatureCelsius` | `decimal(4,1)?` | Tidak | — | — | — | — | **Ya** | — |
| `PulseRate` | `int?` | Tidak | — | — | — | — | **Ya** | — |
| `IsLate` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `IsStopped` | `bool` | Ya | `false` | — | — | — | Tidak | Titik setelah transfusi dihentikan |
| `LateNote` | `string(500)?` | Tidak | — | — | — | — | **Ya** | Wajib bila `IsLate` |
| `RecordedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `RevisionNumber` | `int` | Ya | `0` | — | — | — | Tidak | `1` saat pertama dicatat; naik pada koreksi |
| `CorrectionReason` | `string(500)?` | Tidak | — | — | — | — | **Ya** | — |

### 12.12 `CliTransfusionReaction` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `MonitoringId` | `Guid` | Ya | — | Index | FK `CliTransfusionMonitoring` | `Restrict` | Tidak | — |
| `PointType` | `CliTransfusionPointType?` (`int`) | Tidak | — | — | — | — | Tidak | Titik saat reaksi terlihat |
| `OccurredAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `ReactionSummary` | `string(250)` | Ya | — | — | — | — | **Ya** | Misalnya "menggigil, demam" |
| `ReactionDetail` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | — |
| `RecordedByUserId` | `Guid` | Ya | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `NoticeDelivery` | `CliReactionNoticeDelivery` (`int`) | Ya | `Pending` | Index | — | — | Tidak | — |
| `NoticeAttemptCount` | `int` | Ya | `0` | — | — | — | Tidak | — |
| `BloodBankNoticeId` | `Guid?` | Tidak | — | — | Rujukan logis ke `BbkTransfusionReactionNotice.Id` | — | Tidak | — |

### 12.13 `MstMedicalEquipment` — `Baru`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `EquipmentCode` | `string(30)` | Ya | — | **Unique** | — | — | Tidak | — |
| `EquipmentName` | `string(150)` | Ya | — | Index | — | — | Tidak | — |
| `CategoryName` | `string(100)?` | Tidak | — | — | — | — | Tidak | Teks bebas; taksonomi tidak ditetapkan |
| `ChargeUnit` | `MstEquipmentChargeUnit` (`int`) | Ya | — | — | — | — | Tidak | — |
| `RoundingRule` | `MstEquipmentRoundingRule` (`int`) | Ya | `CeilingWholeUnit` | — | — | — | Tidak | — |
| `Description` | `string(250)?` | Tidak | — | — | — | — | Tidak | — |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

### 12.14 `MstTariff` — `Diperbarui` (daftar lengkap satu-satunya)

`[Table("MstTariff", Schema = "public")]`. Model `Areas/HealthServices/MasterData/Models/MstTariff.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `TariffCode` | `string(50)` | Ya | — | Sesuai configuration yang ada | — | — | Tidak | — |
| `TariffName` | `string(250)` | Ya | — | — | — | — | Tidak | — |
| `TariffCategoryId` | `Guid` | Ya | — | Index | FK `MstTariffCategory` | `Restrict` | Tidak | — |
| `ServiceUnitId` | `Guid?` | Tidak | — | — | FK `MstServiceUnit` | `Restrict` | Tidak | — |
| `ClinicId` | `Guid?` | Tidak | — | — | FK `MstClinic` | `Restrict` | Tidak | — |
| `PatientClassId` | `Guid?` | Tidak | — | — | FK `MstPatientClass` | `Restrict` | Tidak | Kelas perawatan |
| `ProcedureId` | `Guid?` | Tidak | — | — | FK `MstProcedure` | `Restrict` | Tidak | — |
| `DrugId` | `Guid?` | Tidak | — | — | FK `MstDrug` | `Restrict` | Tidak | Juga dipakai bahan OK |
| **`MedicalEquipmentId`** | `Guid?` | Tidak | — | **Index** | FK `MstMedicalEquipment` | `Restrict` | Tidak | **Baru** (`keperawatan` `0.6.0`) |
| **`SurgeryComponentType`** | `MstSurgeryComponentType` (`int`) | Ya | `None` | **Index** | — | — | Tidak | **Baru** (`episode-rawat-inap` `0.10.0`): `None`, `AnesthesiaService`, `OperatingRoomRent` |
| **`ChargeBasis`** | `MstTariffChargeBasis` (`int`) | Ya | `PerService` | — | — | — | Tidak | **Baru** (`episode-rawat-inap` `0.10.0`): `PerService`, `PerHour` |
| **`ChargeRounding`** | `MstEquipmentRoundingRule` (`int`) | Ya | `CeilingWholeUnit` | — | — | — | Tidak | **Baru** (`episode-rawat-inap` `0.10.0`); berlaku bila `ChargeBasis = PerHour` |
| `ExternalServiceCode` | `string(50)?` | Tidak | — | — | — | — | Tidak | — |
| `ExternalClassCode` | `string(50)?` | Tidak | — | — | — | — | Tidak | — |
| `IsSurgeryRelated` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `IsRoomCharge` | `bool` | Ya | `false` | — | — | — | Tidak | Tarif kamar |
| `IsAdministrationFee` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `IsRegistrationFee` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `IsConsultationFee` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `IsPackageTariff` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `IsNeedDoctor` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `IsNeedApproval` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `NormalPrice` | `decimal` | Ya | — | — | — | — | Tidak | Satu-satunya angka tarif; tidak pernah dikirim layar |
| `EffectiveStartDate` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `EffectiveEndDate` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `IsTaxable` | `bool` | Ya | `false` | — | — | — | Tidak | — |
| `SortOrder` | `int` | Ya | `0` | — | — | — | Tidak | — |
| `Description` | `string(250)?` | Tidak | — | — | — | — | Tidak | — |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | — |

Aturan isian: satu baris tarif paling banyak punya satu rujukan objek (`ProcedureId`, `DrugId`, `MedicalEquipmentId`, atau `SurgeryComponentType ≠ None`). Divalidasi service master tarif.

### 12.15 `GziPatientDiet` — `Diperbarui` (milik Gizi)

`[Table("GziPatientDiet", Schema = "public")]`. Model `Areas/HealthServices/NutritionManagement/Models/GziPatientDiet.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `NutritionOrderId` | `Guid?` | Tidak | — | Sesuai configuration | FK `GziNutritionOrder` | `Restrict` | Tidak | — |
| `PatientId` | `Guid` | Ya | — | Sesuai configuration | FK `MstPatient` | `Restrict` | Tidak | — |
| `EncounterId` | `Guid` | Ya | — | Sesuai configuration | FK `RegPatientEncounter` | `Restrict` | Tidak | — |
| `DietTypeId` | `Guid` | Ya | — | — | FK `GziDietType` | `Restrict` | Tidak | — |
| `FoodFormId` | `Guid` | Ya | — | — | FK `GziFoodForm` | `Restrict` | Tidak | — |
| `NutritionRequirementId` | `Guid?` | Tidak | — | — | FK `GziNutritionRequirement` | `Restrict` | Tidak | — |
| `EnergyRequirementKcal` | `int?` | Tidak | — | — | — | — | Tidak | 1–10.000 |
| `Instruction` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | — |
| `Status` | `GziPatientDietStatus` (`int`) | Ya | `Active` | — | — | — | Tidak | — |
| `StartAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `EndAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `ChangeReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | — |
| `PrescribedByWorkforceId` | `Guid` | Ya | — | — | FK `MstWorkforceProfile` | `Restrict` | Tidak | Penetap = dokter pemberi instruksi |
| **`InstructionVerificationStatus`** | `GziInstructionVerificationStatus` (`int`) | Ya | `NotRequired` | **Index** | — | — | Tidak | **Baru** |
| **`InstructionVerifiedAt`** | `DateTime?` | Tidak | — | — | — | — | Tidak | **Baru** |
| **`InstructionVerifiedByUserId`** | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | **Baru** |
| `Version` | `int` | Ya | — | — | — | — | Tidak | Konkurensi |

### 12.16 `BbkTransfusionReactionNotice` — `Baru` (milik Bank Darah, disetujui `RWI-DEC-209`)

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `ClinicalReactionId` | `Guid` | Ya | — | **Unique** | Rujukan logis ke `CliTransfusionReaction` | — | Tidak | Kunci idempotensi |
| `BloodUnitId` | `Guid` | Ya | — | Index | FK `BbkBloodUnit` | `Restrict` | Tidak | — |
| `PatientId` | `Guid` | Ya | — | Index | FK `MstPatient` | `Restrict` | Tidak | — |
| `EncounterId` | `Guid` | Ya | — | — | FK `RegPatientEncounter` | `Restrict` | Tidak | — |
| `ServiceUnitId` | `Guid?` | Tidak | — | — | FK `MstServiceUnit` | `Restrict` | Tidak | Unit pelapor |
| `ReactionSummarySnapshot` | `string(250)` | Ya | — | — | — | — | **Ya** | Isi pesan saat dikirim |
| `OccurredAt` | `DateTime` | Ya | — | — | — | — | Tidak | — |
| `ReceivedAt` | `DateTime` | Ya | `DateTime.UtcNow` | Index | — | — | Tidak | — |
| `Status` | `BbkReactionNoticeStatus` (`int`) | Ya | `New` | Index | — | — | Tidak | — |
| `AcknowledgedByUserId` | `Guid?` | Tidak | — | — | FK `ApplicationUser` | `Restrict` | Tidak | — |
| `AcknowledgedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | — |
| `AcknowledgeNote` | `string(500)?` | Tidak | — | — | — | — | **Ya** | — |

### 12.17 Skema DDL

> **Peringatan.** Basis data dibentuk EF Core Migrations. DDL ini **dokumentasi bentuk**, bukan skrip yang dijalankan. Kolom audit `IdentityModel` tidak ditulis ulang. Untuk tabel `Diperbarui`, hanya kolom baru yang ditulis.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."MstMedicalEquipment" (
    "Id" uuid NOT NULL, "EquipmentCode" varchar(30) NOT NULL, "EquipmentName" varchar(150) NOT NULL,
    "CategoryName" varchar(100), "ChargeUnit" integer NOT NULL, "RoundingRule" integer NOT NULL,
    "Description" varchar(250), "IsActive" boolean NOT NULL, "RowVersion" uuid NOT NULL,
    CONSTRAINT "PK_MstMedicalEquipment" PRIMARY KEY ("Id"));
CREATE UNIQUE INDEX "IX_MstMedicalEquipment_EquipmentCode" ON public."MstMedicalEquipment" ("EquipmentCode");

ALTER TABLE public."MstTariff"
    ADD "MedicalEquipmentId" uuid,
    ADD "SurgeryComponentType" integer NOT NULL DEFAULT 0,   -- episode-rawat-inap 0.10.0
    ADD "ChargeBasis" integer NOT NULL DEFAULT 0,            -- episode-rawat-inap 0.10.0
    ADD "ChargeRounding" integer NOT NULL DEFAULT 1,         -- episode-rawat-inap 0.10.0
    ADD CONSTRAINT "FK_MstTariff_MstMedicalEquipment_MedicalEquipmentId"
        FOREIGN KEY ("MedicalEquipmentId") REFERENCES public."MstMedicalEquipment" ("Id") ON DELETE RESTRICT;
CREATE INDEX "IX_MstTariff_MedicalEquipmentId" ON public."MstTariff" ("MedicalEquipmentId");
CREATE INDEX "IX_MstTariff_SurgeryComponentType" ON public."MstTariff" ("SurgeryComponentType");

CREATE TABLE public."CliEquipmentUsage" (
    "Id" uuid NOT NULL, "InpEpisodeId" uuid NOT NULL, "EncounterId" uuid NOT NULL, "PatientId" uuid NOT NULL,
    "MedicalEquipmentId" uuid NOT NULL, "ResponsibleDoctorId" uuid NOT NULL, "PerformedByUserId" uuid NOT NULL,
    "StartedAt" timestamp NOT NULL, "EndedAt" timestamp, "Quantity" numeric(10,2),
    "ChargeUnitSnapshot" integer NOT NULL, "RoundingRuleSnapshot" integer NOT NULL, "BilledUnits" numeric(10,2),
    "Status" integer NOT NULL, "RequiresNurseReview" boolean NOT NULL, "AutoClosedAt" timestamp,
    "CancelReason" varchar(500),   -- SENSITIF
    "Note" varchar(500),           -- SENSITIF
    "RevisionNumber" integer NOT NULL, "Version" integer NOT NULL,
    CONSTRAINT "PK_CliEquipmentUsage" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CliEquipmentUsage_MstMedicalEquipment_MedicalEquipmentId"
        FOREIGN KEY ("MedicalEquipmentId") REFERENCES public."MstMedicalEquipment" ("Id") ON DELETE RESTRICT);
CREATE INDEX "IX_CliEquipmentUsage_Episode_Running" ON public."CliEquipmentUsage" ("InpEpisodeId") WHERE "Status" = 1;

CREATE TABLE public."CliEquipmentUsageRevision" (
    "Id" uuid NOT NULL, "EquipmentUsageId" uuid NOT NULL, "RevisionNumber" integer NOT NULL,
    "PreviousStartedAt" timestamp NOT NULL, "PreviousEndedAt" timestamp, "PreviousQuantity" numeric(10,2),
    "PreviousBilledUnits" numeric(10,2), "Reason" varchar(500) NOT NULL,  -- SENSITIF
    "RevisedByUserId" uuid NOT NULL, "RevisedAt" timestamp NOT NULL,
    CONSTRAINT "PK_CliEquipmentUsageRevision" PRIMARY KEY ("Id"));
CREATE UNIQUE INDEX "IX_CliEquipmentUsageRevision_Usage_Revision" ON public."CliEquipmentUsageRevision" ("EquipmentUsageId", "RevisionNumber");

CREATE TABLE public."CliWsdDrain" (
    "Id" uuid NOT NULL, "InpEpisodeId" uuid NOT NULL, "EncounterId" uuid NOT NULL, "PatientId" uuid NOT NULL,
    "DrainLabel" varchar(50) NOT NULL, "InsertionSite" varchar(100),  -- SENSITIF
    "InsertedAt" timestamp NOT NULL, "InitialResidualMl" numeric(8,1) NOT NULL DEFAULT 0,
    "RemovedAt" timestamp, "RemovedByUserId" uuid, "Status" integer NOT NULL, "RegisteredByUserId" uuid NOT NULL,
    "CorrectionReason" varchar(500),  -- SENSITIF
    "Version" integer NOT NULL,
    CONSTRAINT "PK_CliWsdDrain" PRIMARY KEY ("Id"));
CREATE UNIQUE INDEX "IX_CliWsdDrain_Episode_Label_Active" ON public."CliWsdDrain" ("InpEpisodeId", "DrainLabel") WHERE "Status" = 1;

CREATE TABLE public."CliWsdReading" (
    "Id" uuid NOT NULL, "WsdDrainId" uuid NOT NULL, "InpEpisodeId" uuid NOT NULL, "ShiftId" uuid,
    "PeriodStartAt" timestamp NOT NULL, "PeriodEndAt" timestamp NOT NULL,
    "PreviousResidualMl" numeric(8,1) NOT NULL, "CurrentResidualMl" numeric(8,1) NOT NULL,
    "DiscardedVolumeMl" numeric(8,1) NOT NULL DEFAULT 0, "IncreaseMl" numeric(8,1) NOT NULL,
    "FluidBalanceEntryId" uuid NOT NULL, "Status" integer NOT NULL, "RevisionNumber" integer NOT NULL,
    "RecordedByUserId" uuid NOT NULL, "CancelReason" varchar(500),  -- SENSITIF
    CONSTRAINT "PK_CliWsdReading" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CliWsdReading_CliWsdDrain_WsdDrainId" FOREIGN KEY ("WsdDrainId") REFERENCES public."CliWsdDrain" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_CliWsdReading_CliFluidBalanceEntry_FluidBalanceEntryId" FOREIGN KEY ("FluidBalanceEntryId") REFERENCES public."CliFluidBalanceEntry" ("Id") ON DELETE RESTRICT);
CREATE UNIQUE INDEX "IX_CliWsdReading_FluidBalanceEntryId" ON public."CliWsdReading" ("FluidBalanceEntryId");
CREATE INDEX "IX_CliWsdReading_Drain_PeriodEnd" ON public."CliWsdReading" ("WsdDrainId", "PeriodEndAt");

CREATE TABLE public."CliSurgicalSiteSurveillance" (
    "Id" uuid NOT NULL, "OprCaseId" uuid NOT NULL, "InpEpisodeId" uuid NOT NULL, "EncounterId" uuid NOT NULL,
    "PatientId" uuid NOT NULL, "InstrumentVersionId" uuid NOT NULL, "SurgeryCompletedAt" timestamp NOT NULL,
    "DayOneDate" date NOT NULL, "Status" integer NOT NULL, "StoppedAt" timestamp, "StoppedOnDayNumber" integer,
    "SummaryResponsesJson" text NOT NULL,  -- SENSITIF
    "NosocomialInfectionId" uuid, "SuspectedFlaggedAt" timestamp, "SuspectedFlaggedByUserId" uuid,
    "ReviewNote" varchar(1000),  -- SENSITIF
    "CancelReason" varchar(500), "Version" integer NOT NULL,
    CONSTRAINT "PK_CliSurgicalSiteSurveillance" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CliSurgicalSiteSurveillance_TrxNosocomialInfection" FOREIGN KEY ("NosocomialInfectionId") REFERENCES public."TrxNosocomialInfection" ("Id") ON DELETE RESTRICT);
CREATE UNIQUE INDEX "IX_CliSurgicalSiteSurveillance_OprCaseId" ON public."CliSurgicalSiteSurveillance" ("OprCaseId");

CREATE TABLE public."CliSurgicalSiteSurveillanceEntry" (
    "Id" uuid NOT NULL, "SurveillanceId" uuid NOT NULL, "DayNumber" integer NOT NULL, "EntryDate" date NOT NULL,
    "ResponsesJson" text NOT NULL,                    -- SENSITIF
    "TemperatureMaxCelsiusSnapshot" numeric(4,1),     -- SENSITIF
    "FeverIndicatorFromVitals" boolean, "RecordedByUserId" uuid NOT NULL, "RevisionNumber" integer NOT NULL,
    CONSTRAINT "PK_CliSurgicalSiteSurveillanceEntry" PRIMARY KEY ("Id"));
CREATE UNIQUE INDEX "IX_CliSurgicalSiteSurveillanceEntry_Surveillance_Day" ON public."CliSurgicalSiteSurveillanceEntry" ("SurveillanceId", "DayNumber");

CREATE TABLE public."CliSurgicalSiteSurveillanceEntryRevision" (
    "Id" uuid NOT NULL, "EntryId" uuid NOT NULL, "RevisionNumber" integer NOT NULL,
    "PreviousResponsesJson" text NOT NULL,  -- SENSITIF
    "Reason" varchar(500) NOT NULL,         -- SENSITIF
    "RevisedByUserId" uuid NOT NULL, "RevisedAt" timestamp NOT NULL,
    CONSTRAINT "PK_CliSurgicalSiteSurveillanceEntryRevision" PRIMARY KEY ("Id"));

CREATE TABLE public."CliTransfusionMonitoring" (
    "Id" uuid NOT NULL, "InpEpisodeId" uuid NOT NULL, "EncounterId" uuid NOT NULL, "PatientId" uuid NOT NULL,
    "BloodUnitId" uuid NOT NULL, "ReceivedAtWardAt" timestamp NOT NULL, "TransfusionStartedAt" timestamp NOT NULL,
    "Status" integer NOT NULL, "StoppedAt" timestamp, "StopReason" varchar(500),  -- SENSITIF
    "CompletedAt" timestamp, "PerformedByUserId" uuid NOT NULL, "CancelReason" varchar(500), "Version" integer NOT NULL,
    CONSTRAINT "PK_CliTransfusionMonitoring" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CliTransfusionMonitoring_BbkBloodUnit_BloodUnitId" FOREIGN KEY ("BloodUnitId") REFERENCES public."BbkBloodUnit" ("Id") ON DELETE RESTRICT);
CREATE UNIQUE INDEX "IX_CliTransfusionMonitoring_BloodUnit_NotCancelled" ON public."CliTransfusionMonitoring" ("BloodUnitId") WHERE "Status" <> 4;

CREATE TABLE public."CliTransfusionMonitoringPoint" (
    "Id" uuid NOT NULL, "MonitoringId" uuid NOT NULL, "PointType" integer NOT NULL, "DueAt" timestamp NOT NULL,
    "MeasuredAt" timestamp, "SystolicBp" integer, "DiastolicBp" integer, "TemperatureCelsius" numeric(4,1),
    "PulseRate" integer,  -- nilai tanda vital: SENSITIF
    "IsLate" boolean NOT NULL, "IsStopped" boolean NOT NULL, "LateNote" varchar(500),  -- SENSITIF
    "RecordedByUserId" uuid, "RevisionNumber" integer NOT NULL, "CorrectionReason" varchar(500),  -- SENSITIF
    CONSTRAINT "PK_CliTransfusionMonitoringPoint" PRIMARY KEY ("Id"));
CREATE UNIQUE INDEX "IX_CliTransfusionMonitoringPoint_Monitoring_Point" ON public."CliTransfusionMonitoringPoint" ("MonitoringId", "PointType");

CREATE TABLE public."CliTransfusionReaction" (
    "Id" uuid NOT NULL, "MonitoringId" uuid NOT NULL, "PointType" integer, "OccurredAt" timestamp NOT NULL,
    "ReactionSummary" varchar(250) NOT NULL,  -- SENSITIF
    "ReactionDetail" varchar(1000),           -- SENSITIF
    "RecordedByUserId" uuid NOT NULL, "NoticeDelivery" integer NOT NULL, "NoticeAttemptCount" integer NOT NULL,
    "BloodBankNoticeId" uuid,
    CONSTRAINT "PK_CliTransfusionReaction" PRIMARY KEY ("Id"));
CREATE INDEX "IX_CliTransfusionReaction_NoticeDelivery" ON public."CliTransfusionReaction" ("NoticeDelivery");

ALTER TABLE public."GziPatientDiet"
    ADD "InstructionVerificationStatus" integer NOT NULL DEFAULT 0,
    ADD "InstructionVerifiedAt" timestamp,
    ADD "InstructionVerifiedByUserId" uuid;
CREATE INDEX "IX_GziPatientDiet_InstructionVerificationStatus" ON public."GziPatientDiet" ("InstructionVerificationStatus");

CREATE TABLE public."BbkTransfusionReactionNotice" (   -- disetujui RWI-DEC-209
    "Id" uuid NOT NULL, "ClinicalReactionId" uuid NOT NULL, "BloodUnitId" uuid NOT NULL, "PatientId" uuid NOT NULL,
    "EncounterId" uuid NOT NULL, "ServiceUnitId" uuid,
    "ReactionSummarySnapshot" varchar(250) NOT NULL,  -- SENSITIF
    "OccurredAt" timestamp NOT NULL, "ReceivedAt" timestamp NOT NULL, "Status" integer NOT NULL,
    "AcknowledgedByUserId" uuid, "AcknowledgedAt" timestamp, "AcknowledgeNote" varchar(500),  -- SENSITIF
    CONSTRAINT "PK_BbkTransfusionReactionNotice" PRIMARY KEY ("Id"));
CREATE UNIQUE INDEX "IX_BbkTransfusionReactionNotice_ClinicalReactionId" ON public."BbkTransfusionReactionNotice" ("ClinicalReactionId");
```

Foreign key ke `InpEpisode`, `RegPatientEncounter`, `MstPatient`, `MstDoctor`, dan `ApplicationUser` pada tabel di atas mengikuti pola `Restrict` dan tidak ditulis satu per satu agar DDL tetap terbaca; daftar lengkapnya pada kolom "Relasi" setiap tabel.
