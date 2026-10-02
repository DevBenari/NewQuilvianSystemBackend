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
