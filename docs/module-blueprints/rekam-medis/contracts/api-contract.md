# API Contract — Rekam Medis Existing Clinical Foundation

| Field | Nilai |
| --- | --- |
| `contract_version` | `rm-existing-clinical-api-evidence-v0.2-draft` |
| Status | `draft` |
| Owner | Clinical Management; Rekam Medis sebagai consumer |
| `approved_by` / `approved_at` | Belum tersedia — `RM-APR-002` |
| Input | Requirement gate revision `4`; domain architecture revision `1` |
| Snapshot | Backend `5103e68eec5529540d369673c8a4e2651be0344b` |
| Compatibility | Tidak mengubah endpoint; memisahkan kemampuan reuse dari repair constraint |
| Traceability | `RM-CAP-03`, `RM-CAP-05`, `RM-CAP-06`, `RM-SIG-*`, `RM-COR-*`, `RM-CLS-*` |

Semua endpoint di bawah **sudah tersedia** pada source. Dokumen ini merekam kontrak as-is, bukan
menyetujui endpoint sebagai kontrak final Rekam Medis. Permission generic belum membuktikan hubungan
pelayanan aktif. Status completion/verified/approved existing belum membuktikan reauth, profesi,
makna signature, dan hash isi.

### Health Services / Clinical Management / Patient Assessment

Base URL: `api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keputusan |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar assessment. | `PatientAssessment : Read` | Query filter/paging | `ApiResponse<ResponsePatientAssessmentPagedResult>` | Reuse baca |
| `GET` | `/{id}` | Detail assessment. | `PatientAssessment : Read` | Path `id` | `ApiResponse<PatientAssessmentDetailResponse>` | Reuse baca |
| `GET` | `/active-by-encounter/{encounterId}` | Assessment aktif per encounter. | `PatientAssessment : Read` | Path + `queueId?` | `ApiResponse<PatientAssessmentDetailResponse>` | Reuse baca |
| `GET` | `/active-by-queue/{queueId}` | Assessment aktif per antrean. | `PatientAssessment : Read` | Path | `ApiResponse<PatientAssessmentDetailResponse>` | Reuse baca |
| `POST` | `/` | Membuat assessment existing. | `PatientAssessment : Create` | `CreatePatientAssessmentRequest` | `ApiResponse<PatientAssessmentCreateResponse>` | Extend |
| `PUT` | `/{id}` | Mengubah assessment yang masih boleh diubah owner. | `PatientAssessment : Update` | `UpdatePatientAssessmentRequest` | `ApiResponse<object>` | Guard finality wajib |
| `PATCH` | `/{id}/complete` | Menyelesaikan assessment existing. | `PatientAssessment : Update` | `CompletePatientAssessmentRequest` | `ApiResponse<PatientAssessmentCompleteResponse>` | Bukan signature RM |
| `PATCH` | `/{id}/cancel` | Membatalkan assessment existing. | `PatientAssessment : Update` | `CancelPatientAssessmentRequest` | `ApiResponse<object>` | Bukan correction RM |

### Health Services / Clinical Management / Doctor Consultation

Base URL: `api/v1/health-services/clinical-management/doctor-consultations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keputusan |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter konsultasi. | `DoctorConsultation : Read` | — | `ApiResponse<DoctorConsultationFilterMetadataResponse>` | Reuse |
| `GET` | `/` | Daftar konsultasi. | `DoctorConsultation : Read` | Query filter/paging | `ApiResponse<ResponseDoctorConsultationPagedResult>` | Reuse baca |
| `GET` | `/{id}` | Detail konsultasi/SOAP. | `DoctorConsultation : Read` | Path | `ApiResponse<DoctorConsultationDetailResponse>` | Reuse baca |
| `GET` | `/active-by-queue/{queueId}` | Konsultasi aktif per antrean. | `DoctorConsultation : Read` | Path | `ApiResponse<DoctorConsultationDetailResponse>` | Reuse baca |
| `POST` | `/` | Membuat konsultasi. | `DoctorConsultation : Create` | `CreateDoctorConsultationRequest` | `ApiResponse<DoctorConsultationCreateResponse>` | Extend |
| `PUT` | `/{id}` | Mengubah konsultasi yang belum final. | `DoctorConsultation : Update` | `UpdateDoctorConsultationRequest` | `ApiResponse<DoctorConsultationUpdateResponse>` | Guard finality wajib |
| `PATCH` | `/{id}/soap` | Mengubah SOAP existing. | `DoctorConsultation : Update` | `UpdateDoctorConsultationSoapRequest` | `ApiResponse<DoctorConsultationSoapUpdateResponse>` | Guard finality wajib |
| `GET` | `/{id}/finalization-validation` | Memeriksa kesiapan finalisasi owner. | `DoctorConsultation : Read` | Path | `ApiResponse<ConsultationFinalizationValidationResponse>` | Reuse dengan repair checklist |
| `PATCH` | `/{id}/complete` | Menyelesaikan konsultasi dan finalisasi terkait dalam transaksi existing. | `DoctorConsultation : Update` | `FinalizeDoctorConsultationRequest` | `ApiResponse<ConsultationFinalizationResponse>` | Bukan closure/signature RM |
| `PATCH` | `/{id}/cancel` | Membatalkan konsultasi. | `DoctorConsultation : Update` | `CancelDoctorConsultationRequest` | `ApiResponse<object>` | Guard record signed wajib |

### Health Services / Clinical Management / Patient Diagnosis

Base URL: `api/v1/health-services/clinical-management/patient-diagnoses`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keputusan |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter. | `PatientDiagnosis : Read` | — | `ApiResponse<PatientDiagnosisFilterMetadataResponse>` | Reuse |
| `GET` | `/master-options` | Pilihan master diagnosis. | `PatientDiagnosis : Read` | Query | `ApiResponse<List<PatientDiagnosisMasterOptionResponse>>` | Reuse |
| `GET` | `/` | Daftar diagnosis pasien. | `PatientDiagnosis : Read` | Query filter/paging | `ApiResponse<ResponsePatientDiagnosisPagedResult>` | Reuse |
| `GET` | `/options` | Pilihan diagnosis pasien. | `PatientDiagnosis : Read` | Query | `ApiResponse<List<PatientDiagnosisOptionResponse>>` | Reuse |
| `GET` | `/{id}` | Detail diagnosis. | `PatientDiagnosis : Read` | Path | `ApiResponse<PatientDiagnosisDetailResponse>` | Reuse |
| `POST` | `/` | Membuat diagnosis. | `PatientDiagnosis : Create` | `CreatePatientDiagnosisRequest` | `ApiResponse<PatientDiagnosisCreateResponse>` | Reuse/adapter |
| `PUT` | `/{id}` | Mengubah diagnosis existing. | `PatientDiagnosis : Update` | `UpdatePatientDiagnosisRequest` | `ApiResponse<PatientDiagnosisUpdateResponse>` | Guard finality wajib |
| `PATCH` | `/{id}/set-primary` | Menetapkan diagnosis utama. | `PatientDiagnosis : Update` | `SetPrimaryPatientDiagnosisRequest` | `ApiResponse<object>` | Reuse; penting untuk completeness |
| `PATCH` | `/{id}/resolve` | Menyelesaikan diagnosis. | `PatientDiagnosis : Update` | `ResolvePatientDiagnosisRequest` | `ApiResponse<object>` | Reuse owner |
| `PATCH` | `/{id}/cancel` | Membatalkan diagnosis. | `PatientDiagnosis : Update` | `CancelPatientDiagnosisRequest` | `ApiResponse<object>` | Guard finality wajib |

### Health Services / Clinical Management / Patient Procedure

Base URL: `api/v1/health-services/clinical-management/patient-procedures`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keputusan |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter. | `PatientProcedure : Read` | — | `ApiResponse<PatientProcedureFilterMetadataResponse>` | Reuse |
| `GET` | `/master-options` | Pilihan master tindakan. | `PatientProcedure : Read` | Query | `ApiResponse<List<PatientProcedureMasterOptionResponse>>` | Reuse |
| `GET` | `/` | Daftar tindakan. | `PatientProcedure : Read` | Query filter/paging | `ApiResponse<ResponsePatientProcedurePagedResult>` | Reuse |
| `GET` | `/options` | Pilihan tindakan pasien. | `PatientProcedure : Read` | Query | `ApiResponse<List<PatientProcedureOptionResponse>>` | Reuse |
| `GET` | `/{id}` | Detail tindakan. | `PatientProcedure : Read` | Path | `ApiResponse<PatientProcedureDetailResponse>` | Reuse |
| `POST` | `/select` | Memilih tindakan master. | `PatientProcedure : Create` | `SelectPatientProcedureRequest` | `ApiResponse<PatientProcedureCreateResponse>` | Reuse |
| `POST` | `/` | Membuat tindakan. | `PatientProcedure : Create` | `CreatePatientProcedureRequest` | `ApiResponse<PatientProcedureCreateResponse>` | Reuse |
| `PUT` | `/{id}` | Mengubah tindakan existing. | `PatientProcedure : Update` | `UpdatePatientProcedureRequest` | `ApiResponse<PatientProcedureUpdateResponse>` | Guard finality wajib |
| `PATCH` | `/{id}/approve` | Menyetujui tindakan owner. | `PatientProcedure : Update` | `ApprovePatientProcedureRequest` | `ApiResponse<object>` | Bukan approval klinis RM |
| `PATCH` | `/{id}/execute` | Menandai pelaksanaan tindakan. | `PatientProcedure : Update` | `ExecutePatientProcedureRequest` | `ApiResponse<object>` | Pemicu conditional item |
| `PATCH` | `/{id}/remove-draft` | Mengeluarkan tindakan draft. | `PatientProcedure : Update` | Path | `ApiResponse<object>` | Hanya draft; jangan untuk signed |
| `PATCH` | `/{id}/cancel` | Membatalkan tindakan. | `PatientProcedure : Update` | `CancelPatientProcedureRequest` | `ApiResponse<object>` | Review bila memengaruhi checklist |

### Health Services / Clinical Management / Patient Allergy

Base URL: `api/v1/health-services/clinical-management/patient-allergies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keputusan |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter. | `PatientAllergy : Read` | — | `ApiResponse<PatientAllergyFilterMetadataResponse>` | Reuse |
| `GET` | `/active-alerts` | Alert alergi aktif pasien. | `PatientAllergy : Read` | Query `patientId` | `ApiResponse<List<PatientAllergyAlertResponse>>` | Reuse keselamatan |
| `GET` | `/` | Daftar alergi. | `PatientAllergy : Read` | Query filter/paging | `ApiResponse<ResponsePatientAllergyPagedResult>` | Reuse |
| `GET` | `/options` | Pilihan alergi pasien. | `PatientAllergy : Read` | Query | `ApiResponse<List<PatientAllergyOptionResponse>>` | Reuse |
| `GET` | `/{id}` | Detail alergi. | `PatientAllergy : Read` | Path | `ApiResponse<PatientAllergyDetailResponse>` | Reuse |
| `POST` | `/` | Membuat alergi. | `PatientAllergy : Create` | `CreatePatientAllergyRequest` | `ApiResponse<PatientAllergyCreateResponse>` | Reuse |
| `PUT` | `/{id}` | Mengubah alergi. | `PatientAllergy : Update` | `UpdatePatientAllergyRequest` | `ApiResponse<PatientAllergyUpdateResponse>` | Guard finality wajib |
| `PATCH` | `/{id}/verify` | Memverifikasi alergi. | `PatientAllergy : Update` | `VerifyPatientAllergyRequest` | `ApiResponse<object>` | Reuse owner |
| `PATCH` | `/{id}/resolve` | Menyelesaikan alergi. | `PatientAllergy : Update` | `ResolvePatientAllergyRequest` | `ApiResponse<object>` | Reuse owner |
| `PATCH` | `/{id}/cancel` | Membatalkan alergi. | `PatientAllergy : Update` | `CancelPatientAllergyRequest` | `ApiResponse<object>` | Guard safety review |
| `DELETE` | `/{id}` | Soft-delete alergi existing. | `PatientAllergy : Delete` | Path | `ApiResponse<object>` | Konflik untuk fakta resmi |

### Health Services / Clinical Management / Patient Vital Sign

Base URL: `api/v1/health-services/clinical-management/patient-vital-signs`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keputusan |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter. | `PatientVitalSign : Read` | — | `ApiResponse<PatientVitalSignFilterMetadataResponse>` | Reuse |
| `GET` | `/critical-alerts` | Alert vital kritis. | `PatientVitalSign : Read` | Query | `ApiResponse<List<PatientVitalSignAlertResponse>>` | Reuse keselamatan |
| `GET` | `/` | Daftar observasi vital. | `PatientVitalSign : Read` | Query filter/paging | `ApiResponse<ResponsePatientVitalSignPagedResult>` | Reuse |
| `GET` | `/options` | Pilihan observasi. | `PatientVitalSign : Read` | Query | `ApiResponse<List<PatientVitalSignOptionResponse>>` | Reuse |
| `GET` | `/{id}` | Detail vital. | `PatientVitalSign : Read` | Path | `ApiResponse<PatientVitalSignDetailResponse>` | Reuse |
| `GET` | `/active-by-encounter/{encounterId}` | Vital aktif per encounter. | `PatientVitalSign : Read` | Path + `queueId?` | `ApiResponse<PatientVitalSignDetailResponse>` | Reuse |
| `GET` | `/active-by-queue/{queueId}` | Vital aktif per antrean. | `PatientVitalSign : Read` | Path | `ApiResponse<PatientVitalSignDetailResponse>` | Reuse |
| `POST` | `/` | Mencatat vital. | `PatientVitalSign : Create` | `CreatePatientVitalSignRequest` | `ApiResponse<PatientVitalSignCreateResponse>` | Reuse |
| `PUT` | `/{id}` | Mengubah vital. | `PatientVitalSign : Update` | `UpdatePatientVitalSignRequest` | `ApiResponse<PatientVitalSignUpdateResponse>` | Guard finality wajib |
| `PATCH` | `/{id}/verify` | Memverifikasi vital. | `PatientVitalSign : Update` | `VerifyPatientVitalSignRequest` | `ApiResponse<object>` | Reuse owner |
| `PATCH` | `/{id}/notify-doctor` | Mencatat notifikasi dokter. | `PatientVitalSign : Update` | `NotifyDoctorPatientVitalSignRequest` | `ApiResponse<object>` | Reuse keselamatan |
| `PATCH` | `/{id}/cancel` | Membatalkan vital. | `PatientVitalSign : Update` | `CancelPatientVitalSignRequest` | `ApiResponse<object>` | Guard safety review |
| `DELETE` | `/{id}` | Soft-delete vital existing. | `PatientVitalSign : Delete` | Path | `ApiResponse<object>` | Konflik untuk fakta resmi |

### Health Services / Clinical Management / Patient Integrated Progress Note

Base URL: `api/v1/health-services/clinical-management/patient-integrated-progress-notes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keputusan |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter CPPT. | `PatientIntegratedProgressNote : Read` | — | `ApiResponse<PatientIntegratedProgressNoteFilterMetadataResponse>` | Reuse |
| `GET` | `/` | Daftar CPPT. | `PatientIntegratedProgressNote : Read` | Query filter/paging | `ApiResponse<ResponsePatientIntegratedProgressNotePagedResult>` | Reuse baca |
| `GET` | `/timeline` | Timeline CPPT. | `PatientIntegratedProgressNote : Read` | Query patient/encounter | `ApiResponse<List<PatientIntegratedProgressNoteTimelineResponse>>` | Reuse baca |
| `GET` | `/{id}` | Detail CPPT. | `PatientIntegratedProgressNote : Read` | Path | `ApiResponse<PatientIntegratedProgressNoteDetailResponse>` | Reuse baca |
| `POST` | `/` | Membuat CPPT. | `PatientIntegratedProgressNote : Create` | `CreatePatientIntegratedProgressNoteRequest` | `ApiResponse<PatientIntegratedProgressNoteCreateResponse>` | Extend |
| `POST` | `/from-consultation/{consultationId}` | Membuat CPPT dari konsultasi. | `PatientIntegratedProgressNote : Create` | Path + `CreatePatientIntegratedProgressNoteFromConsultationRequest` | `ApiResponse<PatientIntegratedProgressNoteCreateResponse>` | Extend |
| `GET` | `/draft-from-consultation/{consultationId}` | Membangun draft dari konsultasi. | `PatientIntegratedProgressNote : Read` | Path | `ApiResponse<CreatePatientIntegratedProgressNoteRequest>` | Reuse draft |
| `PUT` | `/{id}` | Mengubah CPPT existing. | `PatientIntegratedProgressNote : Update` | `UpdatePatientIntegratedProgressNoteRequest` | `ApiResponse<PatientIntegratedProgressNoteUpdateResponse>` | **Repair sebelum signed** |
| `PATCH` | `/{id}/cancel` | Membatalkan CPPT. | `PatientIntegratedProgressNote : Update` | `CancelPatientIntegratedProgressNoteRequest` | `ApiResponse<object>` | **Repair sebelum signed** |
| `DELETE` | `/{id}` | Soft-delete CPPT. | `PatientIntegratedProgressNote : Delete` | Path | `ApiResponse<object>` | **Tidak boleh untuk signed** |

### Health Services / Clinical Management / Patient Clinical Document

Base URL: `api/v1/health-services/clinical-management/patient-clinical-documents`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keputusan |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter. | `PatientClinicalDocument : Read` | — | `ApiResponse<PatientClinicalDocumentFilterMetadataResponse>` | Reuse |
| `GET` | `/` | Daftar dokumen klinis. | `PatientClinicalDocument : Read` | Query filter/paging | `ApiResponse<ResponsePatientClinicalDocumentPagedResult>` | Reuse |
| `GET` | `/options` | Pilihan dokumen. | `PatientClinicalDocument : Read` | Query | `ApiResponse<List<PatientClinicalDocumentOptionResponse>>` | Reuse |
| `GET` | `/{id}` | Detail dokumen. | `PatientClinicalDocument : Read` | Path | `ApiResponse<PatientClinicalDocumentDetailResponse>` | Reuse |
| `POST` | `/` | Membuat metadata dokumen. | `PatientClinicalDocument : Create` | `CreatePatientClinicalDocumentRequest` | `ApiResponse<PatientClinicalDocumentCreateResponse>` | Extend |
| `PUT` | `/{id}` | Mengubah dokumen existing. | `PatientClinicalDocument : Update` | `UpdatePatientClinicalDocumentRequest` | `ApiResponse<PatientClinicalDocumentUpdateResponse>` | Guard finality wajib |
| `PATCH` | `/{id}/review` | Review dokumen owner. | `PatientClinicalDocument : Update` | `ReviewPatientClinicalDocumentRequest` | `ApiResponse<object>` | Reuse owner |
| `PATCH` | `/{id}/verify` | Verifikasi dokumen owner. | `PatientClinicalDocument : Update` | `VerifyPatientClinicalDocumentRequest` | `ApiResponse<object>` | Permission masih generic |
| `PATCH` | `/{id}/approve` | Approval dokumen owner. | `PatientClinicalDocument : Update` | `ApprovePatientClinicalDocumentRequest` | `ApiResponse<object>` | Permission masih generic |
| `PATCH` | `/{id}/archive` | Arsipkan dokumen. | `PatientClinicalDocument : Update` | `ArchivePatientClinicalDocumentRequest` | `ApiResponse<object>` | Bukan retensi RM |
| `PATCH` | `/{id}/cancel` | Membatalkan dokumen. | `PatientClinicalDocument : Update` | `CancelPatientClinicalDocumentRequest` | `ApiResponse<object>` | Guard finality wajib |
| `DELETE` | `/{id}` | Soft-delete dokumen. | `PatientClinicalDocument : Delete` | Path | `ApiResponse<object>` | **Tidak boleh untuk signed** |

### Health Services / Clinical Management / Patient Consent

Base URL: `api/v1/health-services/clinical-management/patient-consents`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keputusan |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter consent. | `PatientConsent : Read` | — | `ApiResponse<PatientConsentFilterMetadataResponse>` | Reuse |
| `GET` | `/` | Daftar consent. | `PatientConsent : Read` | Query filter/paging | `ApiResponse<ResponsePatientConsentPagedResult>` | Reuse |
| `GET` | `/options` | Pilihan consent. | `PatientConsent : Read` | Query | `ApiResponse<List<PatientConsentOptionResponse>>` | Reuse |
| `GET` | `/{id}` | Detail consent. | `PatientConsent : Read` | Path | `ApiResponse<PatientConsentDetailResponse>` | Reuse |
| `POST` | `/` | Membuat consent. | `PatientConsent : Create` | `CreatePatientConsentRequest` | `ApiResponse<PatientConsentCreateResponse>` | Reuse/adapter |
| `PUT` | `/{id}` | Mengubah consent existing. | `PatientConsent : Update` | `UpdatePatientConsentRequest` | `ApiResponse<PatientConsentUpdateResponse>` | Hanya status editable |
| `PATCH` | `/{id}/sign` | Menandatangani consent. | `PatientConsent : Update` | `SignPatientConsentRequest` | `ApiResponse<object>` | Belum signature evidence RM |
| `PATCH` | `/{id}/verify` | Verifikasi consent. | `PatientConsent : Update` | `VerifyPatientConsentRequest` | `ApiResponse<object>` | Permission generic |
| `PATCH` | `/{id}/approve` | Menyetujui consent. | `PatientConsent : Update` | `ApprovePatientConsentRequest` | `ApiResponse<object>` | Permission generic |
| `PATCH` | `/{id}/reject` | Menolak consent. | `PatientConsent : Update` | `RejectPatientConsentRequest` | `ApiResponse<object>` | Reuse owner |
| `PATCH` | `/{id}/withdraw` | Menarik consent. | `PatientConsent : Update` | `WithdrawPatientConsentRequest` | `ApiResponse<object>` | Reuse owner |
| `PATCH` | `/{id}/cancel` | Membatalkan consent. | `PatientConsent : Update` | `CancelPatientConsentRequest` | `ApiResponse<object>` | Guard finality wajib |
| `DELETE` | `/{id}` | Soft-delete consent. | `PatientConsent : Delete` | Path | `ApiResponse<object>` | **Tidak boleh untuk signed** |

## Arti kode status

- `200`: operasi existing berhasil; bukan bukti bahwa invariant RM telah terpenuhi.
- `400`: isian atau status tidak valid, termasuk finalization validation gagal.
- `401`: pengguna belum terautentikasi.
- `403`: permission teknis ditolak; permission berhasil belum membuktikan assignment aktif.
- `404`: record owner tidak ditemukan.
- `409`: data berubah, duplicate/conflict, atau finalisasi bersaing; pengguna harus memuat ulang.

**Contoh:** dokter menyelesaikan konsultasi melalui `PATCH /{id}/complete`. Respons `200` menandai
konsultasi selesai. Respons itu tidak boleh langsung mengubah episode RM menjadi `Ditutup Final`
sebelum bukti diagnosis utama dan signature dokumen wajib tersedia.

## API yang sengaja belum dirancang

Tidak ada endpoint `Rencana (belum tersedia)` pada revision existing-first. API episode RM,
signature evidence, correction/addendum, break-glass, release, worklist Unit RM, dan retention
tetap fase desain berikutnya serta fail-closed selama policy belum disahkan.

## Bukti source

Repository `NewQuilvianSystemBackend`, commit
`5103e68eec5529540d369673c8a4e2651be0344b`:

| Controller/symbol | Path source |
| --- | --- |
| `PatientAssessmentController` | `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` |
| `DoctorConsultationController` | `Areas/HealthServices/ClinicalManagement/Controllers/DoctorConsultationController.cs` |
| `PatientDiagnosisController` | `Areas/HealthServices/ClinicalManagement/Controllers/PatientDiagnosisController.cs` |
| `PatientProcedureController` | `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` |
| `PatientAllergyController` | `Areas/HealthServices/ClinicalManagement/Controllers/PatientAllergyController.cs` |
| `PatientVitalSignController` | `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs` |
| `PatientIntegratedProgressNoteController` | `Areas/HealthServices/ClinicalManagement/Controllers/PatientIntegratedProgressNoteController.cs` |
| `PatientClinicalDocumentController` | `Areas/HealthServices/ClinicalManagement/Controllers/PatientClinicalDocumentController.cs` |
| `PatientConsentController` | `Areas/HealthServices/ClinicalManagement/Controllers/PatientConsentController.cs` |
