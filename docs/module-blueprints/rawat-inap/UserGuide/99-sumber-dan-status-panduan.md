# Sumber dan Status Panduan Rawat Inap

Versi 1.0 · 6 Oktober 2026 · Blueprint RWI-BP-001.

Lampiran ini ditujukan bagi administrator, pelatih, dan pemelihara dokumentasi. Petugas operasional dapat langsung mengikuti [daftar isi](README.md). Endpoint di bawah menjadi bukti hubungan langkah penggunaan dengan implementasi; petugas tidak perlu memanggil API secara manual.

## Snapshot yang diperiksa

| Repository | Commit HEAD saat penyusunan | Dasar penjelasan |
| --- | --- | --- |
| Backend | `00fc141f23a6cf04862235dfd3876be22e581b8a` | Controller, layanan domain, enum, dan kontrak blueprint. |
| Frontend | `b010ffb9722f236160477c95c931c22467550200` | Label, urutan langkah, tab, hook, dan batas panel yang tersedia. |

SHA-256 pada tabel sumber merekam isi file yang dibaca, termasuk bila berbeda dari commit HEAD. Nomor baris menjadi petunjuk pada snapshot ini dan dapat bergeser setelah perubahan source. Kontrak blueprint menjelaskan target; source yang diperiksa menentukan langkah yang dituliskan. Jika berbeda, batasnya dinyatakan secara eksplisit.

## Status verifikasi dan batas penggunaan

| Pemeriksaan | Status | Arti |
| --- | --- | --- |
| Penelusuran source dan kontrak | SOURCE_REVIEWED | Langkah, status, permission, serta batas fitur ditelusuri pada snapshot di atas. |
| Kelengkapan file dan tautan lokal | DOC_CHECKS_PASSED | 24 file, 210 tautan lokal, dan seluruh anchor sumber valid; semua bab dapat dijangkau dari README. |
| Kesesuaian sumber dan referensi API | DOC_CHECKS_PASSED | 71 baris hash sumber cocok; 96 baris API cocok dengan route, simbol, dan permission pada controller. |
| Pengujian antarmuka, akun peran, dan transaksi | NOT_RUN | Tidak dilakukan dalam pekerjaan pembuatan panduan ini. |
| Printer dan layanan eksternal lingkungan rumah sakit | NOT_RUN | Belum ada bukti operasi pada lingkungan target dari pekerjaan ini. |

| Area | Temuan source | Dampak pada panduan |
| --- | --- | --- |
| Admisi | Penyimpanan pada langkah Dokter membuka Draft; penempatan terpisah mengubahnya menjadi Admitted. | Konfirmasi/cetak tidak disebut sebagai aktivasi rawat inap. |
| Deposit | Hook deposit hanya menyimpan isian lokal; alur pembentukan episode tidak mengirim transaksi top-up deposit. | Nilai yang diketik bukan bukti saldo/kuitansi; tindak lanjut melalui proses Billing yang sah. |
| Bayi baru lahir | Pemilih episode ibu dan endpoint active-mothers sudah ada pada snapshot. | Jangan mengikuti laporan lama yang masih menyatakan pemilih ibu belum tersedia. |
| Resume ODC | Panel belum menyediakan formulir yang terhubung. | Tidak disediakan langkah simpan/tanda tangan ODC sebagai fitur aktif. |
| Rehab Medik | Layar perawat memasukkannya dalam daftar layanan belum tersedia; panel dokter terdapat pada source. | Ketersediaan pada dokter tidak diasumsikan sama pada perawat atau sudah lolos runtime. |
| Kepergian fisik | Bed dilepas, tetapi status masih DischargePending. | Setelah pasien keluar, checklist dan penutupan tetap ditindaklanjuti. |
| Izin kasir | Status dapat tidak terbaca; CloseOverride hanya melewati syarat finansial. | Angka tagihan/keberadaan tombol tidak menggantikan izin kasir. |

## Dokumen blueprint yang menjadi konteks

- [Manifest blueprint](../blueprint-manifest.md) dan [peta modul](../02-module-map.md).
- Episode: [API](../episode-rawat-inap/contracts/api-contract.md), [status](../episode-rawat-inap/contracts/state-transition-matrix.md), dan [validasi](../episode-rawat-inap/contracts/validation-matrix.md).
- Keperawatan: [API](../keperawatan/contracts/api-contract.md) dan [hak akses/audit](../keperawatan/contracts/permission-audit-matrix.md).
- Dokter: [API](../dokter-rawat-inap/contracts/api-contract.md).
- Integrasi Billing: [API](../integrasi-billing/contracts/api-contract.md).

## Membaca tabel API

Nama grup mengikuti atribut Tags pada controller. Gabungkan base path grup dengan path relatif pada tabel; `/` berarti base path itu sendiri. `{id:guid}` adalah parameter ID, bukan teks yang diketik pengguna. Permission ditulis sebagai resource:action sesuai AccessPermission.

Kolom respons memuat payload yang dideklarasikan controller beserta status sukses jika tersedia; wrapper `ApiResponse<T>` tidak diulang. Jika controller memakai pemetaan hasil service tanpa deklarasi DTO, nama service disebut dan tidak diasumsikan sebagai kontrak DTO baru. Daftar ini memilih endpoint yang menjelaskan panduan, bukan seluruh API modul.

Kesalahan umum dibaca dari respons server: 400 untuk permintaan tidak sah, 401 untuk autentikasi, 403 untuk kewenangan, 404 untuk data tidak ditemukan, dan 409 untuk konflik status/versi. Sebagian proses klinis memakai 422 untuk aturan yang belum terpenuhi. Kode yang tersedia pada setiap endpoint tetap mengikuti controller/service; lihat [penanganan kendala](bantuan/01-kendala-koreksi-dan-pertanyaan-umum.md).

## Bukti per proses

<a id="admisi"></a>

### Admisi

Urutan tiga jalur, titik simpan Draft, pilihan ibu aktif, dan kelanjutan admisi.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-admission-flow-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx) | `INPATIENT_ADMISSION_ENTRY_MODE` / 7 | `797a86ee8725d77975e314eadeeae944408a0fcd8c7e84f4082f09abab3ee082` |
| FE | [inpatient-admission-view.jsx](../../../../../QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx) | `InpatientAdmissionView` / 467 | `e84b766ac091987cec9a42ba9b9866c1397a6b5403976ac8b63f9e761d4a226e` |
| FE | [use-inpatient-admission-doctor.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx) | `useInpatientAdmissionDoctor` / 56 | `ddd42f2d27998a4f2f987d01fba91f6f9ddd1938a4c95e6961a875a2b460e424` |
| FE | [use-inpatient-admission-confirmation.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-confirmation.jsx) | `useInpatientAdmissionConfirmation` / 40 | `d34b0c763665a0c034826c44bd6b1a2cb08b427a21a6f2ca520166f1338120c5` |
| FE | [use-inpatient-admission-resume.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-resume.jsx) | `useInpatientAdmissionResume` / 33 | `ed241a281b7ee8f42d7431ed7a485acca4b30ee1fb8f7d140887fb2215437959` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Episode**

Base path: `/api/v1/health-services/inpatient-management/episodes`. Sumber: [InpatientEpisodeController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `active-mothers` | `GetActiveMothers` / 80 | `InpatientEpisode:Read` | `ActiveMotherListQuery` | `200: List<ActiveMotherItemResponse>` |
| POST | `/` | `OpenAdmission` / 199 | `InpatientEpisode:Create` | `OpenAdmissionRequest` | `200: InpatientEpisodeDetailResponse` |
| PUT | `{id:guid}` | `UpdateAdmission` / 241 | `InpatientEpisode:Update` | `UpdateAdmissionRequest` | `200: InpatientEpisodeDetailResponse` |
| PATCH | `{id:guid}/cancel` | `CancelAdmission` / 288 | `InpatientEpisode:Update` | `CancelAdmissionRequest` | `200: InpatientEpisodeDetailResponse` |

<a id="episode"></a>

### Episode, tempat tidur, dan penanggung jawab

Status episode, reservasi berbeda dari penempatan, transfer, serta penugasan dokter/perawat.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-episode-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-episode-constants.jsx) | `INPATIENT_EPISODE_ROUTE` / 3 | `a8fe2ef06b05249eb108bd27b0e89d08d8e17ea0af61fb90a78a87d35d62d6ed` |
| FE | [inpatient-bed-board-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-bed-board-constants.jsx) | `INPATIENT_BED_BOARD_ROUTE` / 3 | `fa011311bbf4f6c60a44ddccfcb8cd93d2ebda77267d188ecc06c82d2a6d109e` |
| BE | [InpBedOccupancyService.cs](../../../../Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs) | `InpBedOccupancyService` / 42 | `1092009fafa8fc67e28dc4fc2ba6d9932aea155cb84f36815147b8bb36458695` |
| BE | [InpatientEpisodeController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs) | `InpatientEpisodeController` / 53 | `74b882d03ec6a547b5d809b8acfd4215aa13777024394c438f724ca995f49aea` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Episode**

Base path: `/api/v1/health-services/inpatient-management/episodes`. Sumber: [InpatientEpisodeController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `{id:guid}` | `GetById` / 158 | `InpatientEpisode:Read` | `parameter path saja` | `200: InpatientEpisodeDetailResponse` |
| POST | `{id:guid}/doctor-assignments` | `HandoverDoctor` / 389 | `InpatientEpisode:Update` | `HandoverDoctorRequest` | `200: InpatientDoctorAssignmentResponse` |
| POST | `{id:guid}/doctor-assignments/supporting` | `AssignSupportingDoctor` / 470 | `InpatientEpisode:Update` | `AssignSupportingDoctorRequest` | `200: InpatientDoctorAssignmentResponse` |
| POST | `{id:guid}/nurse-assignments` | `AssignNurse` / 574 | `InpatientEpisode:Update` | `AssignNurseRequest` | `200: InpatientNurseAssignmentResponse` |

**Grup Swagger: Health Services / Inpatient Management / Bed Occupancy**

Base path: `/api/v1/health-services/inpatient-management/bed-occupancies`. Sumber: [InpatientBedOccupancyController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `available-beds` | `GetAvailableBeds` / 78 | `InpatientBedOccupancy:Read` | `AvailableBedQuery` | `200: AvailableBedPagedResult` |
| POST | `reservations` | `ReserveBed` / 120 | `InpatientBedOccupancy:Create` | `ReserveBedRequest` | `200: BedReservationResponse` |
| POST | `placements` | `PlacePatient` / 210 | `InpatientBedOccupancy:Create` | `PlacePatientRequest` | `200: BedPlacementResponse` |
| POST | `placements/transfer` | `TransferPatient` / 262 | `InpatientBedOccupancy:Transfer` | `TransferPatientRequest` | `200: BedPlacementResponse` |

<a id="akses"></a>

### Hak akses

Permission controller merupakan salah satu syarat; layanan juga memeriksa episode, profesi, dan penugasan. Nama role dalam panduan bukan pemetaan otomatis permission.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| BE | [InpatientEpisodeController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs) | `InpatientEpisodeController` / 53 | `74b882d03ec6a547b5d809b8acfd4215aa13777024394c438f724ca995f49aea` |
| BE | [InpatientDischargeController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs) | `InpatientDischargeController` / 56 | `716a77f354c50ce84f9b4b8dd7f9dcef223fe735b331ba10803a198d66327798` |
| BE | [PatientAssessmentController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs) | `PatientAssessmentController` / 49 | `9d137c4155604131e5ff760171b7819e487178d6c43fd13595ed3f3d36bbae49` |
| BE | [MedicationAdministrationController.cs](../../../../Areas/HealthServices/PharmacyManagement/Controllers/MedicationAdministrationController.cs) | `MedicationAdministrationController` / 38 | `7f471f227c4b4736412eab8039e168b1d1445629987765aef91132096ebd18cf` |

<a id="kamar-pulih"></a>

### Admisi dari kamar pulih

Rujukan harus mengikuti jalur kamar pulih dan menunjuk pasien/kunjungan yang benar.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-admission-recovery-referral-step.jsx](../../../../../QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/admission-referrals/inpatient-admission-recovery-referral-step.jsx) | `InpatientAdmissionRecoveryReferralStep` / 36 | `ffe632d72678299053e347f8d42bcb2a414ac5b7881c9b83582b52e900da71c3` |
| BE | [InpAdmissionReferralService.cs](../../../../Areas/HealthServices/InPatientManagement/Services/InpAdmissionReferralService.cs) | `InpAdmissionReferralService` / 29 | `aa8158120078f48a38533e28ae4667694dbe94a995e78cb0581f14976dff20ec` |
| BE | [InpatientAdmissionReferralController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientAdmissionReferralController.cs) | `InpatientAdmissionReferralController` / 32 | `36f3ed52df37285e8b2aecd1d472fee98041c8f30d5dc760d9e340333c757fac` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Admission Referral**

Base path: `/api/v1/health-services/inpatient-management/admission-referrals`. Sumber: [InpatientAdmissionReferralController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientAdmissionReferralController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `/` | `GetPaged` / 46 | `InpatientAdmissionReferral:Read` | `AdmissionReferralQuery` | `200: PagedResult<AdmissionReferralResponse>` |
| GET | `{id:guid}` | `GetById` / 60 | `InpatientAdmissionReferral:Read` | `parameter path saja` | `200: AdmissionReferralResponse` |

<a id="pengkajian"></a>

### Pengkajian keperawatan

Tab instrumen, pencatatan draf/final, due status, serta addendum.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-nursing-constants.js](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-nursing-constants.js) | `INPATIENT_NURSING_WORKSPACE_ENTRY_ROUTE` / 5 | `4c610d9689af169261af84852f6ab26f30af8fad6a36e86608a92940e82d8722` |
| FE | [assessment-section.jsx](../../../../../QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-section.jsx) | `AssessmentSection` / 55 | `42a51de0219d7b0c489da98ce930f5f03f20b6ae5c01ea1c0e447af83d9b5a00` |
| BE | [PatientAssessmentController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs) | `PatientAssessmentController` / 49 | `9d137c4155604131e5ff760171b7819e487178d6c43fd13595ed3f3d36bbae49` |

**Grup Swagger: Health Services / Clinical Management / Patient Assessment**

Base path: `/api/v1/health-services/clinical-management/patient-assessments`. Sumber: [PatientAssessmentController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `episodes/{episodeId:guid}` | `GetByEpisode` / 504 | `PatientAssessment:Read` | `query: assessmentType, assessmentStatus, pageNumber, pageSize` | `200: ResponsePatientAssessmentPagedResult` |
| POST | `/` | `CreateAssessment` / 623 | `PatientAssessment:Create` | `CreatePatientAssessmentRequest` | `201: PatientAssessmentCreateResponse` |
| PATCH | `{id:guid}/complete` | `CompleteAssessment` / 1295 | `PatientAssessment:Complete` | `CompletePatientAssessmentRequest` | `200: PatientAssessmentCompleteResponse` |
| POST | `{id:guid}/addendums` | `CreateAddendum` / 1633 | `PatientAssessment:Amend` | `CreateAssessmentAddendumRequest` | `201: ClinicalNoteAddendumResponse` |
| GET | `episodes/{episodeId:guid}/due-status` | `GetEpisodeDueStatus` / 1815 | `PatientAssessment:Read` | `parameter path saja` | `200: AssessmentDueStatusResponse` |

<a id="asuhan"></a>

### Asuhan dan catatan harian

Rencana asuhan, evaluasi, finalisasi intervensi, tanda vital, dan keterkaitan catatan.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [use-inpatient-nursing-care-plan.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-nursing-care-plan.jsx) | `useInpatientNursingCarePlan` / 26 | `38fa4b2e68d5ccc3a03c78c845193036fa43741e926c7350bbcfdc605391e702` |
| FE | [use-inpatient-nursing-intervention.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-nursing-intervention.jsx) | `useInpatientNursingIntervention` / 27 | `e5666df9e9d53dd8950b495ce632412edb8c2572bf3f4c5d6e661621b25ba8a4` |
| BE | [NursingCarePlanController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/NursingCarePlanController.cs) | `NursingCarePlanController` / 48 | `3736728c4c143a9320c9350b30ee793c03e6b753210d1d44e1b3f3e69fba3c5c` |
| BE | [NursingInterventionController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/NursingInterventionController.cs) | `NursingInterventionController` / 52 | `3f5200a9b2be46b0f7cfe7e1d9974d6ee44b79e867df1e2a7f10c64aeb9ffa58` |

**Grup Swagger: Health Services / Clinical Management / Nursing Care Plan**

Base path: `/api/v1/health-services/clinical-management/nursing-care-plans`. Sumber: [NursingCarePlanController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/NursingCarePlanController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `/` | `CreateCarePlan` / 81 | `NursingCarePlan:Create` | `CreateNursingCarePlanRequest` | `201: NursingCarePlanResponse` |
| PATCH | `items/{itemId:guid}/evaluate` | `EvaluateCarePlanItem` / 305 | `NursingCarePlan:Update` | `EvaluateCarePlanItemRequest` | `200: CarePlanItemResponse` |
| PATCH | `items/{itemId:guid}/close` | `CloseCarePlanItem` / 367 | `NursingCarePlan:Update` | `CloseCarePlanItemRequest` | `200: CarePlanItemResponse` |

**Grup Swagger: Health Services / Clinical Management / Nursing Intervention**

Base path: `/api/v1/health-services/clinical-management/nursing-interventions`. Sumber: [NursingInterventionController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/NursingInterventionController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `/` | `CreateNursingIntervention` / 95 | `NursingIntervention:Create` | `CreateNursingInterventionRequest` | `201: NursingInterventionResponse; 200: NursingInterventionResponse` |
| PATCH | `{id:guid}/finalize` | `FinalizeNursingIntervention` / 294 | `NursingIntervention:Update` | `parameter path saja` | `200: NursingInterventionResponse` |
| POST | `{id:guid}/addendums` | `CreateAddendum` / 362 | `NursingIntervention:Amend` | `CreateInterventionAddendumRequest` | `201: ClinicalNoteAddendumResponse` |

**Grup Swagger: Health Services / Clinical Management / Patient Vital Sign**

Base path: `/api/v1/health-services/clinical-management/patient-vital-signs`. Sumber: [PatientVitalSignController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `/` | `CreateVitalSign` / 457 | `PatientVitalSign:Create` | `CreatePatientVitalSignRequest` | `200: PatientVitalSignCreateResponse` |

<a id="obat-alat"></a>

### Pemberian obat dan pemakaian alat

Status dosis, double check, koreksi pemberian, awal/akhir penggunaan alat.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| BE | [MedicationDoseStatus.cs](../../../../Areas/HealthServices/PharmacyManagement/Enums/MedicationDoseStatus.cs) | `MedicationDoseStatus` / 1 | `8081f3e411a3a7884fbdd8ac5054887123d19f6b8d5e44c38ade49b08d825b50` |
| BE | [MedicationDoubleCheckStatus.cs](../../../../Areas/HealthServices/PharmacyManagement/Enums/MedicationDoubleCheckStatus.cs) | `MedicationDoubleCheckStatus` / 1 | `afc1782c2e184d2b50b1886a3e0ac3a760c9218c24a9828b5f882ee1c6760bca` |
| BE | [MedicationAdministrationController.cs](../../../../Areas/HealthServices/PharmacyManagement/Controllers/MedicationAdministrationController.cs) | `MedicationAdministrationController` / 38 | `7f471f227c4b4736412eab8039e168b1d1445629987765aef91132096ebd18cf` |
| BE | [EquipmentUsageController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/EquipmentUsageController.cs) | `EquipmentUsageController` / 12 | `06bb1db6315879b216104849d3632ffdba00b0d1af455a9105d71609081beb85` |

**Grup Swagger: Health Services / Pharmacy Management / Medication Administration**

Base path: `/api/v1/health-services/pharmacy-management/medication-administrations`. Sumber: [MedicationAdministrationController.cs](../../../../Areas/HealthServices/PharmacyManagement/Controllers/MedicationAdministrationController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `episodes/{episodeId:guid}` | `GetChart` / 59 | `MedicationAdministration:Read` | `query: date, includeStopped` | `200: MedicationAdministrationChartResponse` |
| PATCH | `{id:guid}/record` | `Record` / 106 | `MedicationAdministration:Create` | `RecordMedicationAdministrationRequest` | `200: MedicationAdministrationResponse` |
| PATCH | `{id:guid}/double-check` | `DoubleCheck` / 144 | `MedicationAdministration:DoubleCheck` | `DoubleCheckRequest` | `200: MedicationAdministrationResponse` |
| PUT | `{id:guid}/correct` | `Correct` / 157 | `MedicationAdministration:Update` | `CorrectMedicationAdministrationRequest` | `200: MedicationAdministrationResponse` |

**Grup Swagger: Health Services / Clinical Management / Equipment Usage**

Base path: `/api/v1/health-services/clinical-management/equipment-usages`. Sumber: [EquipmentUsageController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/EquipmentUsageController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `/` | `List` / 15 | `EquipmentUsage:Read` | `query: episodeId, status` | `hasil service ListAsync` |
| POST | `/` | `Start` / 17 | `EquipmentUsage:Create` | `StartEquipmentUsageRequest` | `hasil service StartAsync` |
| PATCH | `{id:guid}/finish` | `Finish` / 19 | `EquipmentUsage:Update` | `FinishEquipmentUsageRequest` | `hasil service FinishAsync` |
| POST | `{id:guid}/finish` | `Finish` / 19 | `EquipmentUsage:Update` | `FinishEquipmentUsageRequest` | `hasil service FinishAsync` |
| PUT | `{id:guid}/time-correction` | `Correct` / 23 | `EquipmentUsage:Correct` | `CorrectEquipmentUsageRequest` | `hasil service CorrectAsync` |

<a id="pemantauan-khusus"></a>

### Pemantauan khusus

Pencatatan cairan, WSD, surveilans luka operasi, dan transfusi adalah kejadian berbeda; isian mengikuti keadaan nyata.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| BE | [FluidBalanceController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/FluidBalanceController.cs) | `FluidBalanceController` / 32 | `958030424c51ebb9b2280e4744cc6d38ed8f84fedbb9f9518a712a1eae940690` |
| BE | [WsdObservationController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/WsdObservationController.cs) | `WsdObservationController` / 23 | `09ef4ab409b639bb1b0611513ba044439a30e54f9976696c9e2d4b8e8244382b` |
| BE | [SurgicalSiteSurveillanceController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/SurgicalSiteSurveillanceController.cs) | `SurgicalSiteSurveillanceController` / 13 | `fbbff3389d61bff9cde6f10f60b788211e39632813bce9f8e8555c4377625317` |
| BE | [TransfusionMonitoringController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/TransfusionMonitoringController.cs) | `TransfusionMonitoringController` / 13 | `5315b2351e1e5ae71bb1222496138f50c48325dfb7d25b45dd1836aead89ad86` |

**Grup Swagger: Health Services / Clinical Management / Fluid Balance**

Base path: `/api/v1/health-services/clinical-management/fluid-balance-entries`. Sumber: [FluidBalanceController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/FluidBalanceController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `/` | `Create` / 81 | `FluidBalance:Create` | `CreateFluidBalanceEntryRequest` | `201: FluidBalanceEntryResponse` |
| GET | `episodes/{episodeId:guid}/totals` | `GetTotals` / 62 | `FluidBalance:Read` | `query: date` | `200: FluidTotalsResponse` |

**Grup Swagger: Health Services / Clinical Management / WSD Observation**

Base path: `/api/v1/health-services/clinical-management/wsd-drains`. Sumber: [WsdObservationController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/WsdObservationController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `/` | `Register` / 30 | `FluidBalance:Create` | `RegisterWsdDrainRequest` | `hasil service RegisterDrainAsync` |
| POST | `{drainId:guid}/readings` | `Record` / 42 | `FluidBalance:Create` | `RecordWsdReadingRequest` | `hasil service RecordReadingAsync` |
| PATCH | `{drainId:guid}/remove` | `Remove` / 36 | `FluidBalance:Update` | `RemoveWsdDrainRequest` | `hasil service RemoveDrainAsync` |
| POST | `{drainId:guid}/remove` | `Remove` / 36 | `FluidBalance:Update` | `RemoveWsdDrainRequest` | `hasil service RemoveDrainAsync` |

**Grup Swagger: Health Services / Clinical Management / Surgical Site Surveillance**

Base path: `/api/v1/health-services/clinical-management/surgical-site-surveillances`. Sumber: [SurgicalSiteSurveillanceController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/SurgicalSiteSurveillanceController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| PUT | `{id:guid}/entries/{dayNumber:int}` | `Entry` / 26 | `SurgicalSiteSurveillance:Update` | `PutSurveillanceEntryRequest` | `hasil service PutEntryAsync` |
| PUT | `{id:guid}/summary` | `Summary` / 29 | `SurgicalSiteSurveillance:Update` | `PutSurveillanceSummaryRequest` | `hasil service PutSummaryAsync` |

**Grup Swagger: Health Services / Clinical Management / Transfusion Monitoring**

Base path: `/api/v1/health-services/clinical-management/transfusion-monitorings`. Sumber: [TransfusionMonitoringController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/TransfusionMonitoringController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `/` | `Start` / 23 | `TransfusionMonitoring:Create` | `StartTransfusionMonitoringRequest` | `hasil service StartAsync` |
| PUT | `{id:guid}/points/{pointType}` | `Point` / 26 | `TransfusionMonitoring:Update` | `PutTransfusionPointRequest` | `hasil service PutPointAsync` |
| POST | `{id:guid}/reactions` | `Reaction` / 29 | `TransfusionMonitoring:Update` | `RecordTransfusionReactionRequest` | `hasil service RecordReactionAsync` |
| PATCH | `{id:guid}/complete` | `Complete` / 35 | `TransfusionMonitoring:Update` | `CompleteTransfusionRequest` | `hasil service CompleteAsync` |
| POST | `{id:guid}/complete` | `Complete` / 35 | `TransfusionMonitoring:Update` | `CompleteTransfusionRequest` | `hasil service CompleteAsync` |

<a id="dokter"></a>

### Daftar pasien, kajian medis, dan visite

Tab dokter, syarat isi kajian, status dokumen, dan visite sebagai catatan tersendiri.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-physician-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-physician-constants.jsx) | `INPATIENT_PHYSICIAN_ENTRY_ROUTE` / 4 | `3aadb9b66b007602c30327943cb95b4767b9a15a409b8655d10353b884c48580` |
| FE | [inpatient-medical-assessment-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-medical-assessment-constants.jsx) | `PATIENT_ASSESSMENT_TYPE` / 9 | `a0501e3f01fc625eb9b54cf2f76e217b5a63243e5b5a464d12d0e35bc4b3e604` |
| FE | [inpatient-physician-visit-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-physician-visit-constants.jsx) | `PHYSICIAN_VISIT_ROLE` / 7 | `f1be1e8fb075983e329cda83942ed8b920c6b1e80ddb8779fff06202f2e2e01a` |
| BE | [PhysicianVisitController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PhysicianVisitController.cs) | `PhysicianVisitController` / 62 | `6a3d15f27a9dcaf390459fc68e2b46dda3bcced23c1a0f41d3030f99f5243b27` |

**Grup Swagger: Health Services / Clinical Management / Physician Visit**

Base path: `/api/v1/health-services/clinical-management/physician-visits`. Sumber: [PhysicianVisitController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PhysicianVisitController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `/` | `RecordVisit` / 259 | `PhysicianVisit:Create` | `CreatePhysicianVisitRequest` | `201: PhysicianVisitResponse; 200: PhysicianVisitResponse` |
| PATCH | `{id:guid}/cancel` | `CancelVisit` / 379 | `PhysicianVisit:Cancel` | `CancelPhysicianVisitRequest` | `200: PhysicianVisitResponse` |

<a id="catatan-dokter"></a>

### SOAP, CPPT, dan verifikasi

SOAP dapat draf/final; verifikasi CPPT tidak sama dengan membuat SOAP. Daftar instruksi merupakan alur tersendiri.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-progress-note-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-progress-note-constants.jsx) | `DOCTOR_CONSULTATION_STATUS` / 9 | `124a79aa4d4f00c5ab7c69cb89dc14b0ea5d8c4058dc8167610e9d6db391ed10` |
| FE | [inpatient-instruction-review-constants.js](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-instruction-review-constants.js) | `INSTRUCTION_REVIEW_SOURCES` / 1 | `999bb617e58af3a82245764db1b14d991167e2767916b5a637b80a453b03e727` |
| BE | [DoctorConsultationController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs) | `DoctorConsultationController` / 44 | `8ada6cdae0ec7f77dd98110dd30a742f90699c13f78462c9417f91bf976c9bf9` |
| BE | [PatientIntegratedProgressNoteController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs) | `PatientIntegratedProgressNoteController` / 45 | `9bd87173339c861dd6cb548d0b4ccf51d2c47c21d60b5edae9dbdfa3b746b7c6` |

**Grup Swagger: Health Services / Clinical Management / Doctor Consultation**

Base path: `/api/v1/health-services/clinical-management/doctor-consultations`. Sumber: [DoctorConsultationController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `episodes/{episodeId:guid}/soap-timeline` | `GetSoapTimelineByEpisode` / 339 | `DoctorConsultation:Read` | `query: from, to` | `200: SoapTimelineResponse` |
| POST | `/` | `CreateConsultation` / 427 | `DoctorConsultation:Create` | `CreateDoctorConsultationRequest` | `200: DoctorConsultationCreateResponse` |
| PATCH | `{id:guid}/soap` | `UpdateSoap` / 853 | `DoctorConsultation:WriteSoap` | `UpdateDoctorConsultationSoapRequest` | `200: DoctorConsultationSoapUpdateResponse` |
| PATCH | `{id:guid}/complete` | `CompleteConsultation` / 1014 | `DoctorConsultation:Complete` | `FinalizeDoctorConsultationRequest` | `200: ConsultationFinalizationResponse` |

**Grup Swagger: Health Services / Clinical Management / Patient Integrated Progress Note**

Base path: `/api/v1/health-services/clinical-management/patient-integrated-progress-notes`. Sumber: [PatientIntegratedProgressNoteController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `episodes/{episodeId:guid}` | `GetByEpisode` / 985 | `PatientIntegratedProgressNote:Read` | `query: professionType, noteKind, verificationStatus, from, to, pageNumber, pageSize` | `200: ResponsePatientIntegratedProgressNotePagedResult` |
| PATCH | `{id:guid}/verify` | `VerifyProgressNote` / 1076 | `PatientIntegratedProgressNote:Verify` | `parameter path saja` | `200: PatientIntegratedProgressNoteResponse` |

**Grup Swagger: Health Services / Clinical Management / Patient Procedure**

Base path: `/api/v1/health-services/clinical-management/patient-procedures`. Sumber: [PatientProcedureController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `instruction-verification-worklist` | `GetInstructionVerificationWorklist` / 618 | `PatientProcedure:Read` | `query: pageNumber, pageSize` | `200: PagedResult<InstructionVerificationItemResponse>` |
| PATCH | `{id:guid}/verify-instruction` | `VerifyInstruction` / 589 | `PatientProcedure:Verify` | `parameter path saja` | `200: PatientProcedureResponse` |

<a id="resep-tindakan"></a>

### Resep dan tindakan

Order, status pelaksanaan, dan penghentian item resep harus dibedakan.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-prescription-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-prescription-constants.jsx) | `INPATIENT_PRESCRIPTION_SEGMENT` / 28 | `1a06e058cb82960d9fd5c808815da5d6839f76e4a78c72af9bc52d41f9aa06d9` |
| FE | [inpatient-procedure-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-procedure-constants.jsx) | `INPATIENT_PROCEDURE_SEGMENT` / 6 | `775e3bdddac35f74b34d277c12d0e50f29e459bdf59eb730b3ea3515921fd0ec` |
| BE | [PrescriptionController.cs](../../../../Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs) | `PrescriptionController` / 41 | `8d58b0f812b954cf16aaea0c623b155d66029ec38513068fe4386fe1ea019f87` |
| BE | [PatientProcedureController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs) | `PatientProcedureController` / 42 | `a2b09f2ab7ddd501f6af31fae42f37f332cda1479ef06a3064497a39585a4975` |

**Grup Swagger: Health Services / Pharmacy Management / Prescription**

Base path: `/api/v1/health-services/pharmacy-management/prescriptions`. Sumber: [PrescriptionController.cs](../../../../Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `episodes/{episodeId:guid}` | `GetByEpisode` / 453 | `Prescription:Read` | `query: orderType, period, from, to, pageNumber, pageSize` | `200: PagedResult<InpatientPrescriptionListItem>` |
| POST | `/` | `CreatePrescription` / 295 | `Prescription:Create` | `CreatePrescriptionRequest` | `201: PrescriptionCreateResponse; 200: PrescriptionCreateResponse` |
| PATCH | `items/{itemId:guid}/stop` | `StopItem` / 605 | `Prescription:Stop` | `StopPrescriptionItemRequest` | `200: InpatientPrescriptionItemResponse` |

**Grup Swagger: Health Services / Clinical Management / Patient Procedure**

Base path: `/api/v1/health-services/clinical-management/patient-procedures`. Sumber: [PatientProcedureController.cs](../../../../Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `inpatient-orders` | `CreateInpatientOrder` / 546 | `PatientProcedure:Create` | `CreateInpatientProcedureOrderRequest` | `201: PatientProcedureResponse; 200: PatientProcedureResponse` |
| PATCH | `{id:guid}/execute` | `ExecuteProcedure` / 1282 | `PatientProcedure:Execute` | `ExecutePatientProcedureRequest` | `200: object` |

<a id="penunjang"></a>

### Layanan penunjang

Order mengikuti episode. Keberadaan panel di source tidak membuktikan layanan target telah aktif; Rehab Medik pada layar perawat masih dibatasi.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-supporting-service-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-supporting-service-constants.jsx) | `SUPPORTING_SERVICE_SECTION` / 7 | `2d67a87c2558f3f5934058fcea6d70cef9bea326e9cf775e092d954ccd5122b5` |
| FE | [nursing-ancillary-section.jsx](../../../../../QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/ancillary/nursing-ancillary-section.jsx) | `NursingAncillarySection` / 69 | `1090c6300f504543224119444771076e5a75f38561094ab4ac38e4bc87bbcac1` |
| BE | [InpatientAncillaryOrderController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientAncillaryOrderController.cs) | `InpatientAncillaryOrderController` / 17 | `f9ca9832e2f53a6ced7e5cc867bd63c950a0edf10cc9a7b848e3296964d2f034` |
| BE | [InpatientNutritionOrderController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientNutritionOrderController.cs) | `InpatientNutritionOrderController` / 17 | `ba55588bf43d7f8e264490e9ab1a196059ade391e4bf0d95cde01fbe3c83e396` |
| BE | [InpatientBloodOrderController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientBloodOrderController.cs) | `InpatientBloodOrderController` / 18 | `3425c4ce5b333192845c94ef5f608d873d050fee2107e46769b51f7a38449a3c` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Ancillary Order**

Base path: `/api/v1/health-services/inpatient-management/episodes/{episodeId:guid}/ancillary-orders`. Sumber: [InpatientAncillaryOrderController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientAncillaryOrderController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `coverage-status` | `CoverageStatus` / 26 | `InpatientEpisode:Read` | `AncillaryCoverageQuery` | `200: List<CoverageStatusItem>` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Ancillary Order**

Base path: `/api/v1/health-services/inpatient-management/episodes/{episodeId:guid}/ancillary-orders`. Sumber: [InpatientNutritionOrderController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientNutritionOrderController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `nutrition-consultations` | `Create` / 26 | `NutritionOrder:Create` | `CreateInpatientNutritionConsultationRequest` | `200: GziOrderDetailResponse` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Ancillary Order**

Base path: `/api/v1/health-services/inpatient-management/episodes/{episodeId:guid}/ancillary-orders`. Sumber: [InpatientBloodOrderController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientBloodOrderController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `blood-orders` | `Create` / 27 | `BloodOrder:Create` | `CreateInpatientBloodOrderRequest` | `200: BloodOrderDetailDto` |
| POST | `blood-orders/confirm-duplicate` | `ConfirmDuplicate` / 46 | `BloodOrder:Create` | `ConfirmInpatientDuplicateBloodOrderRequest` | `200: BloodOrderDetailDto` |

<a id="operasi"></a>

### Operasi dan serah terima

Pemesanan operasi, kasus bedah, ringkasan pascaoperasi, dan antrean serah terima harus diikuti secara terpisah.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [surgery-booking-section.jsx](../../../../../QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/surgery-booking/surgery-booking-section.jsx) | `SurgeryBookingSection` / 17 | `1419f037c85cd529c9dfdc9ddf727a6bc900e0957bd6e6a603c736ac599d38b0` |
| BE | [InpatientSurgeryBookingController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientSurgeryBookingController.cs) | `InpatientSurgeryBookingController` / 45 | `4a5a7ee25fae51b49a8085b9fb3768ed571275df0d79f0034104f5c59c1d1925` |
| BE | [OperatingRoomHandoverQueryController.cs](../../../../Areas/HealthServices/OperatingRoomManagement/Controllers/OperatingRoomHandoverQueryController.cs) | `OperatingRoomHandoverQueryController` / 27 | `65c11b685807f1b345c5a42bbc4b191d8861439e4d69ccc7c21e9f3a6747269b` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Surgery Booking**

Base path: `/api/v1/health-services/inpatient-management/episodes`. Sumber: [InpatientSurgeryBookingController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientSurgeryBookingController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `{episodeId:guid}/surgery-bookings` | `Book` / 72 | `OperatingRoomCase:Create` | `SurgeryBookingRequest; header: Idempotency-Key` | `201: OprCaseDetailResponse` |

**Grup Swagger: Health Services / Operating Room Management / Handovers**

Base path: `/api/v1/health-services/operating-room-management/handovers`. Sumber: [OperatingRoomHandoverQueryController.cs](../../../../Areas/HealthServices/OperatingRoomManagement/Controllers/OperatingRoomHandoverQueryController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `/` | `GetPaged` / 34 | `OperatingRoomHandover:Read` | `HandoverQueueQuery` | `200: PagedResult<HandoverQueueItemResponse>` |

<a id="billing"></a>

### Deposit, tagihan, dan izin kasir

Deposit admisi masih lokal. Kartu izin kasir hanya menyajikan status; bukan saldo, kuitansi, atau bukti lunas.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [use-inpatient-admission-deposit.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-deposit.jsx) | `useInpatientAdmissionDeposit` / 24 | `ff9643869df5bf11762da9d65427a37b4b6ab879098c604cf5774f3ceac62651` |
| FE | [use-inpatient-admission-doctor.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx) | `useInpatientAdmissionDoctor` / 56 | `ddd42f2d27998a4f2f987d01fba91f6f9ddd1938a4c95e6961a875a2b460e424` |
| FE | [inpatient-billing-status-constants.js](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-billing-status-constants.js) | `CASHIER_STATUS` / 9 | `6299c99153a522e04de40a478b36ca599a6904023b35eb6dea1176ddebd21fb5` |
| BE | [InpatientBillingOperationalController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientBillingOperationalController.cs) | `InpatientBillingOperationalController` / 30 | `09b98cb6c816c8f282793f094eb63daa39aa8c32f3a3719fd41d89d8e6acc050` |

**Grup Swagger: Inpatient Billing Operational**

Base path: `/api/v1/health-services/inpatient-management/episodes`. Sumber: [InpatientBillingOperationalController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientBillingOperationalController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `{episodeId:guid}/billing-status` | `GetBillingStatus` / 53 | `InpatientBillingOperational:Read` | `parameter path saja` | `200: InpatientBillingStatusResponseDto` |

<a id="pemulangan"></a>

### Resume, kepergian fisik, dan penutupan

Kepergian melepas bed tetapi mempertahankan DischargePending. Close memiliki lima syarat, dan override hanya berlaku pada keuangan.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-resume-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-resume-constants.jsx) | `INPATIENT_RESUME_SEGMENT` / 6 | `e719c6a0f7c60c5af3d14fecd05657c6030f6913c5b483bee1fdb53cba4136fb` |
| FE | [resume-odc-panel.jsx](../../../../../QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/resume/resume-odc-panel.jsx) | `ResumeOdcPanel` / 14 | `05fcefb0e2d0c3d24291e6415b649ef334d40a46e95821be43b4b19a4324f793` |
| FE | [use-inpatient-closure.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-closure.jsx) | `useInpatientClosure` / 39 | `1deb5be460b1aa57cb3eb33b1c963129ae1353a3b645e3bbdd952bbd47905ff6` |
| BE | [InpDischargeService.Departure.cs](../../../../Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs) | `InpDischargeService` / 8 | `8ae517873ed62be945cab380a73c06d8f078db6873b0a4a2af03e017d10ce4a5` |
| BE | [InpDischargeService.Closure.cs](../../../../Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs) | `InpDischargeService` / 30 | `bc174daa2ebcac1972395d1bd3638759468e569792ee8770bee9f39d5b2691f2` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Discharge**

Base path: `/api/v1/health-services/inpatient-management/discharges`. Sumber: [InpatientDischargeController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientDischargeController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `{episodeId:guid}/decide` | `DecideDischarge` / 96 | `InpatientDischarge:Update` | `DecideDischargeRequest` | `200: InpatientEpisodeDetailResponse` |
| GET | `{episodeId:guid}/summary-prefill` | `GetSummaryPrefill` / 293 | `InpatientDischarge:Read` | `parameter path saja` | `200: DischargeSummaryPrefillResponse` |
| PUT | `{episodeId:guid}/summary` | `UpsertSummary` / 188 | `InpatientDischarge:Update` | `UpsertDischargeSummaryRequest` | `200: DischargeSummaryResponse` |
| PATCH | `{episodeId:guid}/summary/sign` | `SignSummary` / 235 | `InpatientDischarge:Sign` | `parameter path saja` | `200: DischargeSummaryResponse` |
| GET | `{episodeId:guid}/clearance` | `GetClearanceChecklist` / 326 | `InpatientDischarge:Read` | `parameter path saja` | `200: ClearanceChecklistResponse` |
| POST | `{episodeId:guid}/clearance/{itemId:guid}/mark` | `MarkClearanceItem` / 354 | `InpatientDischarge:Update` | `parameter path saja` | `200: ClearanceChecklistResponse` |
| GET | `{episodeId:guid}/financial-clearance` | `GetFinancialClearance` / 414 | `InpatientDischarge:ReadFinancialClearance` | `parameter path saja` | `200: FinancialClearanceResponse` |
| GET | `{episodeId:guid}/closure-readiness` | `GetClosureReadiness` / 448 | `InpatientDischarge:Read` | `parameter path saja` | `200: ClosureReadinessResponse` |
| POST | `{episodeId:guid}/record-departure` | `RecordDeparture` / 598 | `InpatientDischarge:RecordDeparture` | `parameter path saja` | `200: InpatientDepartureResponse` |
| POST | `{episodeId:guid}/close` | `CloseEpisode` / 476 | `InpatientDischarge:Close` | `parameter path saja` | `200: InpatientEpisodeDetailResponse` |
| POST | `{episodeId:guid}/close-with-override` | `CloseEpisodeWithOverride` / 534 | `InpatientDischarge:CloseOverride` | `CloseEpisodeOverrideRequest` | `200: InpatientEpisodeDetailResponse` |

<a id="operasional"></a>

### Dashboard, sensus, dan daftar pantau

Sensus dan daftar tindak lanjut mempunyai tujuan berbeda. Gunakan waktu/filter yang sama saat membandingkan.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-dashboard-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-dashboard-constants.jsx) | `INPATIENT_DASHBOARD_CONFIG` / 11 | `19346305a76ef1595a708a638aaa5bfc0312eda34268464718b15b0e7e1cce2e` |
| FE | [inpatient-census-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-census-constants.jsx) | `INPATIENT_CENSUS_ROUTE` / 3 | `f40d765cc23efae6a7793481eba1c4ca91fdfce6bdb99f89c82fb4376ffe59d9` |
| FE | [inpatient-monitoring-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-monitoring-constants.jsx) | `INPATIENT_MONITORING_ROUTE` / 3 | `22e72dd2f70ba22096e7e8fbd4017bcbbb4ff972bffdfbb9e0dd90051caf9d92` |
| BE | [InpatientMonitoringController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs) | `InpatientMonitoringController` / 51 | `3b70fc3887d53b7707e2cebfa5e65cf55c546004d11f70e67b89088a38740c0e` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Census**

Base path: `/api/v1/health-services/inpatient-management/census`. Sumber: [InpatientCensusController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientCensusController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `/` | `GetAll` / 108 | `InpatientCensus:Read` | `CensusQuery` | `200: CensusPagedResult` |
| GET | `summary` | `GetSummary` / 72 | `InpatientCensus:Read` | `CensusQuery` | `200: CensusSummaryResponse` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Monitoring**

Base path: `/api/v1/health-services/inpatient-management/monitoring`. Sumber: [InpatientMonitoringController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientMonitoringController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `pending-closures` | `GetPendingClosures` / 113 | `InpatientMonitoring:Read` | `InpatientMonitoringQuery` | `200: PendingClosurePagedResult` |
| GET | `departures-before-clearance` | `GetDeparturesBeforeClearance` / 127 | `InpatientMonitoring:Read` | `DepartureBeforeClearanceQuery` | `DTO: PagedResult<DepartureBeforeClearanceItem>` |
| GET | `pending-surgical-handovers` | `GetPendingSurgicalHandovers` / 277 | `InpatientMonitoring:Read` | `PendingSurgicalHandoverQuery` | `200: PagedResult<HandoverQueueItemResponse>` |
| GET | `pending-admission-referrals` | `GetPendingAdmissionReferrals` / 303 | `InpatientMonitoring:Read` | `PendingAdmissionReferralQuery` | `200: PagedResult<AdmissionReferralResponse>` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Report**

Base path: `/api/v1/health-services/inpatient-management/reports`. Sumber: [InpatientReportController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientReportController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `room-transfers` | `GetRoomTransfers` / 58 | `InpatientReport:ReadRoomTransfer` | `RoomTransferReportQuery` | `200: PagedResult<RoomTransferReportRow>` |
| GET | `room-transfers/export` | `ExportRoomTransfers` / 77 | `InpatientReport:ExportRoomTransfer` | `RoomTransferReportQuery` | `200: FileContentResult` |

<a id="administrasi"></a>

### Master data dan pengaturan

Pengaturan efektif dan master checklist wajib memengaruhi validasi operasional.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-setting-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-setting-constants.jsx) | `INPATIENT_MANAGEMENT_ROUTE_BASE` / 1 | `1de4dafe70f2072ee182b98253de1988a8fc01e9d0bd764f1c96fdbf862796b6` |
| BE | [InpatientSettingController.cs](../../../../Areas/HealthServices/MasterData/Controllers/InpatientSettingController.cs) | `InpatientSettingController` / 37 | `20fabe7c236876a3af087208e5b85c3b7fc7bb35a19d5b570dcee2d39cc13372` |
| BE | [InpatientClearanceItemController.cs](../../../../Areas/HealthServices/MasterData/Controllers/InpatientClearanceItemController.cs) | `InpatientClearanceItemController` / 47 | `9241362ec580b38564258cc7eb2429748cdbceeb3c022b525706a8affa9965a8` |

**Grup Swagger: Health Services / Master Data / Inpatient Setting**

Base path: `/api/v1/health-services/master-data/inpatient-settings`. Sumber: [InpatientSettingController.cs](../../../../Areas/HealthServices/MasterData/Controllers/InpatientSettingController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `/` | `GetEffective` / 58 | `InpatientSetting:Read` | `tanpa body` | `200: InpatientSettingResponse` |
| PUT | `{id:guid}` | `Update` / 90 | `InpatientSetting:Update` | `UpdateInpatientSettingRequest` | `200: InpatientSettingResponse` |

**Grup Swagger: Health Services / Master Data / Inpatient Clearance Item**

Base path: `/api/v1/health-services/master-data/inpatient-clearance-items`. Sumber: [InpatientClearanceItemController.cs](../../../../Areas/HealthServices/MasterData/Controllers/InpatientClearanceItemController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| GET | `/` | `GetAll` / 95 | `InpatientClearanceItem:Read` | `query: startDate, endDate, customPeriod, search, isMandatory, isActive, sortBy, sortDirection, pageNumber, pageSize` | `200: InpatientClearanceItemPagedResult` |
| POST | `/` | `Create` / 192 | `InpatientClearanceItem:Create` | `CreateInpatientClearanceItemRequest` | `200: InpatientClearanceItemResponse` |

<a id="koreksi"></a>

### Koreksi dan penanganan kendala

Correction session untuk Closed tidak mengaktifkan kembali bed/sensus. Koreksi waktu penempatan memeriksa keadaan invoice.

| Repo | File sumber | Simbol / baris | SHA-256 isi file |
| --- | --- | --- | --- |
| FE | [inpatient-correction-constants.jsx](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-correction-constants.jsx) | `INPATIENT_CORRECTION_CONFIG` / 1 | `538f9731e5457ab95818c2b6d2289cbc2a59c6333f86f43c6016b90e70cc7bf2` |
| FE | [inpatient-placement-correction-constants.js](../../../../../QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-placement-correction-constants.js) | `PLACEMENT_CORRECTION_LIMITS` / 8 | `dc4d6f530b2cc07aa81af14e123695d320a20b9fb8d33b0569e0e1ad84914a00` |
| BE | [InpEpisodeService.Corrections.cs](../../../../Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Corrections.cs) | `InpEpisodeService` / 24 | `00ef7a2d9aca53fadae4ea14d42f9f9098b0b0d1b9f1a80d8f0b5e882927f216` |
| BE | [InpPlacementCorrectionService.cs](../../../../Areas/HealthServices/InPatientManagement/Services/InpPlacementCorrectionService.cs) | `InpPlacementCorrectionService` / 28 | `66c9e903a2f9d0657924c916dc17365810d561e864561c7c5090f4860dadef24` |

**Grup Swagger: Health Services / Inpatient Management / Inpatient Episode**

Base path: `/api/v1/health-services/inpatient-management/episodes`. Sumber: [InpatientEpisodeController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `{id:guid}/correction-sessions` | `OpenCorrectionSession` / 675 | `InpatientEpisode:Reopen` | `OpenCorrectionSessionRequest` | `200: InpatientCorrectionSessionResponse` |
| PATCH | `{id:guid}/correction-sessions/{sessionId:guid}/close` | `CloseCorrectionSession` / 721 | `InpatientEpisode:Reopen` | `CloseCorrectionSessionRequest` | `200: InpatientCorrectionSessionResponse` |

**Grup Swagger: Health Services / Inpatient Management / Bed Occupancy**

Base path: `/api/v1/health-services/inpatient-management/bed-occupancies`. Sumber: [InpatientBedOccupancyController.cs](../../../../Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs).

| Metode | Path relatif | Aksi / baris source | Hak akses | Request | Respons sukses |
| --- | --- | --- | --- | --- | --- |
| POST | `placements/{placementId:guid}/corrections` | `CorrectPlacement` / 315 | `InpatientBedOccupancy:Correct` | `CorrectPlacementRequest` | `200: BedPlacementResponse` |

## Skenario uji pengguna pada lingkungan target

Skenario berikut merupakan daftar validasi lanjutan, berstatus NOT_RUN pada pekerjaan ini. Gunakan data uji dan akun peran yang sesuai; hasil aktual dicatat oleh penguji, bukan diasumsikan dari keberadaan source.

| Skenario | Hasil yang harus diperiksa |
| --- | --- |
| Admisi pasien baru/lama | RM benar, kunjungan/episode satu kali, urutan langkah, Draft setelah konfirmasi, cetak sesuai pasien. |
| Ibu/bayi dan rujukan kamar pulih | Ibu aktif yang benar, rujukan mengikat pasien/kunjungan, jalur umum tidak melewati rujukan yang wajib. |
| Reservasi dan penempatan | Reservasi kedaluwarsa ditolak, konflik bed ditolak, penempatan mengaktifkan Admitted, transfer menampilkan riwayat. |
| Akses per peran | Akun tanpa permission ditolak; profesi/penugasan juga diperiksa; dokter pendukung tidak otomatis mendapat wewenang DPJP. |
| Catatan klinis | Draf/final/tanda tangan dibedakan; addendum menyimpan jejak; SOAP, CPPT, visite, dan instruksi mempunyai akibat masing-masing. |
| Obat dan alat | Dosis tidak dicatat ganda; pemeriksa kedua berbeda; status pending double check bukan Administered; waktu alat benar. |
| Penunjang dan operasi | Order terkait episode, hasil/keadaan layanan terbaca, konfirmasi operasi/serah terima tidak dinyatakan hanya dari pembuatan order. |
| Deposit dan kasir | Isian lokal tidak diasumsikan sebagai transaksi; status kasir dibaca dari server; kegagalan integrasi tampil jelas. |
| Pasien meninggalkan ruangan | Waktu sah, warning kasir diakui bila diperlukan, bed bebas, DischargePending tetap ada, pengulangan ditolak. |
| Penutupan dan koreksi | Lima syarat diperiksa; override hanya keuangan dan beralasan; Closed tidak membuka ulang bed melalui sesi koreksi. |

## Pemeliharaan panduan

1. Bila label, endpoint, status, atau aturan berubah, periksa ulang bab yang memakai anchor proses terkait.
2. Perbarui tanggal, versi, commit, simbol/baris, dan hash sumber yang berubah.
3. Periksa tautan relatif dari README, semua bab, serta lampiran ini.
4. Tambahkan bukti pengujian target beserta lingkungan, waktu, peran, skenario, dan hasil ketika tersedia; ubah NOT_RUN hanya berdasarkan hasil tersebut.
5. Tinjau contoh dan teks bersama pengguna operasional sebelum menjadikannya materi pelatihan rumah sakit.

Kembali ke [daftar isi panduan](README.md).
