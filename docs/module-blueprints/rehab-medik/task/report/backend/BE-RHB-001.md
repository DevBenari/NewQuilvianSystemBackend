# Laporan Perubahan Backend — `BE-RHB-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-001` |
| Judul | Fondasi model dan konstanta |
| Slice | S-A — Fondasi (`MVP-0`) |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` r2 — kartu `BE-RHB-001` |
| Trace | RHB-DEC-001..021; RHB-INV-01..10; `data/data-dictionary.md` v1; `02-backend-architecture.md` §4–5; `permission-audit-matrix.md` §2 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 |
| Dependency | `BE-RHB-000` ✅ |
| Klasifikasi | `HEAVY` — 45 berkas, 20 entity baru, tanpa endpoint |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/RehabilitationManagement/{Models,Enums,Constants}/`, `Repositories/Configurations/HealthServices/RehabilitationManagement/`, `Repositories/ApplicationDbContext.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7c0dab893647b87018a73c55359c19d08` (`sukmagp`) + perubahan working tree task sebelumnya |
| Tanggal | 2026-11-05 |
| Status | **Selesai** |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` |
| Registry | `Rhb` / `ACTIVE` (RHB-DEC-014), dibuktikan checker di `BE-RHB-000` |
| Applicability | `NEW CODE` (seluruh `Rhb*`); `TOUCHED LEGACY` hanya `ApplicationDbContext.cs` (tambah region + `using`) |
| QBE yang berlaku | QBE-MOD-002/003, QBE-NAM-001/002/004, QBE-ENT-001, QBE-CFG-001, QBE-CODE-002/003 |

## 1. Masalah yang diselesaikan

Semua task Rehab berikutnya (permintaan, asesmen, program, sesi, billing) membutuhkan tempat
menyimpan data. Sebelum task ini belum ada satu pun tabel, enum, maupun nama butir hak akses
Rehab. Task ini membangun kerangka data itu persis seperti kamus data yang sudah disetujui,
tanpa logika bisnis dan tanpa endpoint.

## 2. Proses bisnis yang disiapkan

Task ini belum menjalankan proses bisnis. Ia menyiapkan wadah untuk alur
`FLOW-RHB-MVP-001`. Contohnya, satu permintaan (`RhbRequest`) yang diterima membuka tepat satu
episode (`RhbEpisode`). Batas itu dijaga index unik `RequestId`, sehingga dua klik "Terima" yang
hampir bersamaan tidak pernah menghasilkan dua episode. Aturan lain yang kini dijaga basis data
(bukan hanya service nanti):

| Aturan | Penjaga di basis data | Contoh |
|---|---|---|
| Rujukan eksternal tidak punya kunjungan asal; sumber internal wajib punya | `CK_RhbRequest_Source` | Permintaan IGD tanpa kunjungan ditolak database |
| Satu asesmen medis awal final per episode | `UX_RhbAssessment_MedicalFinal` | Asesmen medis final kedua untuk episode yang sama gagal tersimpan |
| Satu program aktif per episode per disiplin | `UX_RhbProgram_ActivePerDiscipline` | Fisioterapi v1 dan v2 tidak bisa sama-sama `Active` |
| Hasil ukur milik tepat satu dokumen | `CK_RhbMeasurement_Owner` | Pengukuran tidak bisa menempel ke asesmen **dan** reasesmen sekaligus |
| Satu tindakan bertarif per intervensi | Unik `RhbSessionIntervention.PatientProcedureId` | Pencegah tagihan ganda di `BE-RHB-013` |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HmdEpisode.cs`, `HmdEpisodeConfiguration.cs`, `HemodialysisEnums.cs`,
`HemodialysisPermissions.cs`, `ApplicationDbContext.cs` (`ApplyConfigurationsFromAssembly`,
region HMD), `InpBedPlacementConfiguration.cs` (pola check constraint),
`ApplicationUserOrganizationConfiguration.cs` (`AreNullsDistinct`), `Models/IdentityModel.cs`,
dan namespace `MstPatient`, `RegPatientEncounter`, `InpEpisode`, `MstDoctor`, `MstProfession`,
`MstProcedure`, `CliClinicalInstrumentVersion`, `TrxPatientProcedure`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RehabilitationManagement/Models/*.cs` (20) | Baru: `RhbRequest`, `RhbRequestDiscipline`, `RhbEpisode`, `RhbEpisodeDiscipline`, `RhbAssessment`, `RhbMeasurement`, `RhbGoal`, `RhbProgram`, `RhbProgramIntervention`, `RhbProgramGoal`, `RhbEducationRecord`, `RhbSession`, `RhbSessionIntervention`, `RhbProgressReview`, `RhbGoalReview`, `RhbDischargeSummary`, `RhbStatusHistory`, `RhbDiscipline`, `RhbDisciplineProcedure`, `RhbDisciplineInstrument`. Semua turunan `IdentityModel`, `[Table(..., Schema = "public")]`, kolom sensitif ditandai komentar `SENSITIF`. |
| `Areas/HealthServices/RehabilitationManagement/Enums/RehabilitationEnums.cs` | Baru: 18 enum sesuai kamus data §3 |
| `Areas/HealthServices/RehabilitationManagement/Constants/RehabilitationPermissions.cs` | Baru: `ModuleCode`, 13 Resource beserta action bernama (`Return`, `Disposition`, `Cancel`, `Sign`, `Activate`, `Stop`, `Retry`) |
| `Areas/HealthServices/RehabilitationManagement/Constants/RhbNumberSeries.cs` | Baru: kunci deret `RHB_REQUEST`, `RHB_EPISODE`, `RHB_SESSION` |
| `Areas/HealthServices/RehabilitationManagement/Constants/RhbErrorCodes.cs` | Baru: `RHB-VAL-001`..`071` sesuai validation matrix |
| `Repositories/Configurations/HealthServices/RehabilitationManagement/*.cs` (20) | Baru: satu configuration per entity. Enum `HasConversion<int>`, `Version` `IsConcurrencyToken`, index, unique (parsial `IsDelete = false`), check constraint, `jsonb` untuk `AnswersJson`, dan seluruh FK `DeleteBehavior.Restrict`. FK lintas modul tanpa navigation property. |
| `Repositories/ApplicationDbContext.cs` | `using ...RehabilitationManagement.Models` + region `REHABILITATION MANAGEMENT` berisi 20 `DbSet` |

### 3.3 Delta terhadap kamus data v1

| Delta | Alasan |
|---|---|
| Tambah unique `UX_RhbProgram_Version (EpisodeId, DisciplineId, ProgramVersion)` | Mencegah dua versi bernomor sama pada satu disiplin bila advisory lock gagal. Tidak mengubah kolom. |
| Tambah check `CK_RhbSessionIntervention_Quantity` (1–99) dan `CK_RhbSessionIntervention_Duration` (1–600 atau kosong) | Batas yang sama sudah tertulis di kolom validasi kamus data. Kini juga dijaga database. |
| Kolom "wajib saat tanda tangan/aktivasi" (misalnya `RhbAssessment.HistoryText`, `RhbProgram.FrequencyText`) dibuat **nullable** di database | Draft boleh belum lengkap. Kewajibannya ditegakkan service saat tanda tangan/aktivasi, sesuai kolom "W" kamus data. |
| `RhbMeasurement.InstrumentKind` bertipe `ClinicalInstrumentKind?`, disimpan `int` | Konsisten dengan `RhbDisciplineInstrument`. Bentuk kolom sama. |
| Nama kelas konstanta `RehabilitationPermissions.TherapyProgram` (Resource tetap `RehabProgram`) | Menghindari tabrakan nama dengan kelas entry point `Program` |

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — belum ada endpoint |
| Database | 20 entity baru di model EF. **Belum ada migration** (`BE-RHB-002`), sehingga database belum berubah. |
| Keamanan/Auth | Konstanta Resource/Action disiapkan. Belum ada endpoint yang memakainya. |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `Invoke-QbeConformanceCheck.ps1 -Path <45 berkas> -Mode Strict` | `Files evaluated: 45`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` | Keluaran checker 2026-11-05 |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` (perintah owner) | `Build succeeded.` **0 Error(s)**, 426 Warning(s), `Time Elapsed 00:13:31.71`, exit 0 | `PASS` | Log build 11:37:41–11:51:17 |
| Warning dari berkas Rehab | Tidak ada. Pencarian `RehabilitationManagement`/`Rhb` di log tidak menemukan baris. Seluruh 426 warning berasal dari modul lain yang sudah ada. | `PASS` | grep log |
| Kesesuaian kolom/tipe/panjang terhadap kamus data | Diperiksa per tabel saat penulisan; delta dicatat di §3.3 | `PASS` | Review diff |
| Nama Resource/Action vs `permission-audit-matrix` §2 | 13 Resource identik | `PASS` | Review |
| Tanpa `Trx*`, `SortOrder` tersimpan, controller, maupun akses `DbContext` di controller | Terpenuhi | `PASS` | Review diff; QBE |
| Kecocokan model dengan migration | — | `NOT RUN` | Bagian `BE-RHB-002` (`has-pending-model-changes`) |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT APPLICABLE` — belum ada endpoint maupun tabel di database.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Setiap kolom, tipe, panjang, wajib, dan bawaan sesuai kamus data | Terpenuhi (delta tercatat §3.3) | §3.2, §3.3 |
| (2) Unique parsial `UX_RhbProgram_ActivePerDiscipline`, `UX_RhbAssessment_MedicalFinal`, check `CK_RhbRequest_Source`, `CK_RhbMeasurement_Owner` ada di configuration | Terpenuhi | `RhbProgramConfiguration`, `RhbAssessmentConfiguration`, `RhbRequestConfiguration`, `RhbMeasurementConfiguration` |
| (3) Seluruh FK ke modul lain `Restrict` | Terpenuhi | 20 configuration |
| (4) Konstanta resource/action sama persis dengan `permission-audit-matrix` §2 | Terpenuhi | `RehabilitationPermissions.cs` |
| (5) Tidak ada `Trx*`, `SortOrder` tersimpan, maupun controller | Terpenuhi | QBE `PASS`, review |
| DoD: build sukses | Terpenuhi | §5 |
| DoD: laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 426 warning legacy di modul lain. Tidak ada dari Rehab. |
| Masalah yang diketahui | — |
| Risiko tersisa | Model belum dicocokkan dengan migration. Bila `BE-RHB-002` menemukan selisih, model diperbaiki di task itu. |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Satu percobaan menulis model lewat heredoc shell gagal sebelum ada berkas tertulis, lalu dilanjutkan dengan tool Write. Tidak ada sisa berkas. |
| Status Git | `M Areas/HealthServices/ClinicalManagement/Enums/ClinicalInstrumentKind.cs`, `M Repositories/ApplicationDbContext.cs`, `M docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `?? Areas/HealthServices/RehabilitationManagement/`, `?? Repositories/Configurations/HealthServices/RehabilitationManagement/`, `?? docs/module-blueprints/rehab-medik/` |
| Langkah berikutnya | `BE-RHB-002` (migration), `BE-RHB-004` (jenis dokumen RM + gateway), `BE-RHB-005` (konfigurasi layanan). Ketiganya boleh paralel. |
