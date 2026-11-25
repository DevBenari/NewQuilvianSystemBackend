# Laporan Perubahan Backend — `BE-HRD-439`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-439` |
| Judul | Gerbang kualitas backend Menu 09 |
| Slice | `S-FIN-M09` / Verifikasi (alias `FIN-HRD-09-BE-014`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.6, kartu `BE-HRD-439` |
| Trace | Seluruh task `BE-HRD-426` s.d. `BE-HRD-438` |
| Contract version | Kontrak Kompetensi & Kredensial existing (`BE-HRD-207` s.d. `BE-HRD-220`); tidak ada endpoint baru |
| Dependency | `BE-HRD-429` ✅, `434` 🟡, `435` ✅, `436` ✅, `438` ✅ |
| Klasifikasi | `LIGHT` — verifikasi |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | —; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | 🟡 Sebagian — seluruh endpoint Menu 09 tertelusur dan P0 lain tertutup; P0 `M09-P0-10` (`BE-HRD-434`) masih terbuka menunggu kontrak EMR; 56 UAT `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `CredentialingManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Gerbang sebelumnya terbuka karena P0 di `BE-HRD-427`, `428`, `431`, `432`, `434`. Empat yang pertama kini tertutup di source; `434` tertutup sebagian.

## 2. Proses bisnis

**Status P0 Menu 09.**

| P0 | Task | Status |
| --- | --- | --- |
| `M09-P0-01` | `BE-HRD-433` | Tertutup (19 Nov) |
| `M09-P0-02` | `BE-HRD-427` | Tertutup (20 Nov) |
| `M09-P0-03` | `BE-HRD-428` | Tertutup (20 Nov) |
| `M09-P0-04` | `BE-HRD-426` | Tertutup (19 Nov) |
| `M09-P0-05`, `M09-P0-06` | `BE-HRD-430` | Tertutup (20 Nov) |
| `M09-P0-07` | `BE-HRD-431` | Tertutup (20 Nov) |
| `M09-P0-08`, `M09-P0-09` | `BE-HRD-432` | Tertutup (20 Nov) |
| `M09-P0-10` | `BE-HRD-434` | **Terbuka sebagian** — blocker kontrak co-sign EMR, pemilik modul EMR/Health Services |
| `M09-P0-11` | `BE-HRD-437` | Tertutup (20 Nov) |

**Jejak endpoint Menu 09 (method + URI + permission + DTO), dari source 20 November 2026.**

| Controller | Method | URI | Permission | Request DTO | Response DTO |
| --- | --- | --- | --- | --- | --- |
| `ClinicalPrivilegeGateController` | `POST` | `/api/v1/corporate/hrd/clinical-privileges/validate-gate` | `ClinicalPrivilegeGate : Validate` | `ClinicalPrivilegeGateValidationRequestDto` | `ClinicalPrivilegeGateValidationResultDto` |
| `ClinicalPrivilegeGateController` | `POST` | `/api/v1/corporate/hrd/clinical-privileges/co-sign` | `ClinicalPrivilegeGate : CoSign` | `CoSignatureActionDto` | `CoSignatureResultDto` |
| `ClinicalPrivilegeGateController` | `POST` | `/api/v1/corporate/hrd/clinical-privileges/emergency-override` | `ClinicalPrivilegeGate : EmergencyOverride` | `ClinicalEmergencyOverrideRequestDto` | `EmergencyOverrideLogDto` |
| `ClinicalPrivilegeGateController` | `POST` | `/api/v1/corporate/hrd/clinical-privileges/emergency-overrides/review` | `ClinicalPrivilegeGate : DecideKmkpAudit` | `ReviewEmergencyOverrideDto` | `EmergencyOverrideLogDto` |
| `ClinicalPrivilegeGateController` | `GET` | `/api/v1/corporate/hrd/clinical-privileges/emergency-overrides` | `ClinicalPrivilegeGate : Read` | — | `List<EmergencyOverrideLogDto>` |
| `ClinicalPrivilegeGateController` | `POST` | `/api/v1/corporate/hrd/clinical-privileges/temporary-privilege` | `ClinicalPrivilegeGate : GrantTemporary` | `GrantTemporaryPrivilegeDto` | `TemporaryPrivilegeResultDto` |
| `ProfessionalLicenseController` | `POST` | `/api/v1/corporate/hrd/credentialing/licenses` | `ProfessionalLicense : Create` | `RegisterProfessionalLicenseDto` | `ProfessionalLicenseDto` |
| `ProfessionalLicenseController` | `GET` | `/api/v1/corporate/hrd/credentialing/licenses/{id:guid}` | `ProfessionalLicense : Read` | — | `ProfessionalLicenseDto` |
| `ProfessionalLicenseController` | `GET` | `/api/v1/corporate/hrd/credentialing/licenses/employee/{employeeId:guid}` | `ProfessionalLicense : Read` | — | `List<ProfessionalLicenseDto>` |
| `ProfessionalLicenseController` | `POST` | `/api/v1/corporate/hrd/credentialing/licenses/{id:guid}/verify-psv` | `ProfessionalLicense : VerifyPsv` | `PsvVerificationRequestDto` | `PsvVerificationResultDto` |
| `ProfessionalLicenseController` | `POST` | `/api/v1/corporate/hrd/credentialing/licenses/{id:guid}/renew` | `ProfessionalLicense : Renew` | `RenewProfessionalLicenseDto` | `ProfessionalLicenseDto` |
| `ProfessionalLicenseController` | `GET` | `/api/v1/corporate/hrd/credentialing/licenses/radar/expiring` | `ProfessionalLicense : Read` | — | `List<LicenseRadarAlertDto>` |
| `ProfessionalLicenseController` | `POST` | `/api/v1/corporate/hrd/credentialing/licenses/radar/execute-fail-closed` | `ProfessionalLicense : ExecuteFailClosed` | — | `object` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/committees` | `SpkRkkIssuance : Read` | — | `List<CredentialingCommitteeDto>` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/whitebook-catalog` | `SpkRkkIssuance : Read` | — | `List<ClinicalPrivilegeCatalogItemDto>` |
| `SpkRkkIssuanceController` | `POST` | `/api/v1/corporate/hrd/credentialing/applications` | `SpkRkkIssuance : CreateApplication` | `CreateCredentialingApplicationDto` | `CredentialingApplicationDto` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/applications/{id:guid}` | `SpkRkkIssuance : Read` | — | `CredentialingApplicationDto` |
| `SpkRkkIssuanceController` | `POST` | `/api/v1/corporate/hrd/credentialing/applications/{id:guid}/assign-reviewers` | `SpkRkkIssuance : AssignReviewer` | `AssignPeerReviewersDto` | `CredentialingApplicationDto` |
| `SpkRkkIssuanceController` | `POST` | `/api/v1/corporate/hrd/credentialing/applications/{id:guid}/assessments` | `SpkRkkIssuance : AssessPeer` | `SubmitPeerReviewAssessmentDto` | `object` |
| `SpkRkkIssuanceController` | `POST` | `/api/v1/corporate/hrd/credentialing/applications/{id:guid}/plenary-session` | `SpkRkkIssuance : ApproveCommittee` | `ConcludePlenarySessionDto` | `PlenarySessionResultDto` |
| `SpkRkkIssuanceController` | `POST` | `/api/v1/corporate/hrd/credentialing/spk-rkk/issue-draft` | `SpkRkkIssuance : ParafDirector` | `IssueSpkRkkRequestDto` | `SpkRkkDetailDto` |
| `SpkRkkIssuanceController` | `POST` | `/api/v1/corporate/hrd/credentialing/spk-rkk/{id:guid}/sign-dirut` | `SpkRkkIssuance : SignDirector` | `SignSpkRkkDto` | `SpkRkkDetailDto` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/spk-rkk/{id:guid}` | `SpkRkkIssuance : Read` | — | `SpkRkkDetailDto` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/spk-rkk/employee/{employeeId:guid}` | `SpkRkkIssuance : Read` | — | `List<SpkRkkDetailDto>` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/spk-rkk/{id:guid}/files` | `SpkRkkIssuance : Read` | — | `FileResult` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/applications` | `SpkRkkIssuance : Read` | — | `ApiResponse<PagedResult<CredentialingApplicationListItemDto>>` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/applications/peer-review-worklist` | `SpkRkkIssuance : AssessPeer` | — | `ApiResponse<PagedResult<CredentialingApplicationListItemDto>>` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/applications/{id:guid}/peer-review-sheet` | `SpkRkkIssuance : AssessPeer` | — | `ApiResponse<CredentialingPeerReviewSheetDto>` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/applications/plenary-worklist` | `SpkRkkIssuance : ApproveCommittee` | — | `ApiResponse<PagedResult<CredentialingApplicationListItemDto>>` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/applications/{id:guid}/plenary-matrix` | `SpkRkkIssuance : ApproveCommittee` | — | `ApiResponse<CredentialingPlenaryMatrixDto>` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/spk-rkk/draft-queue` | `SpkRkkIssuance : ParafDirector` | — | `ApiResponse<PagedResult<CredentialingApplicationListItemDto>>` |
| `SpkRkkIssuanceController` | `GET` | `/api/v1/corporate/hrd/credentialing/spk-rkk/pending-signature` | `SpkRkkIssuance : SignDirector` | — | `ApiResponse<PagedResult<SpkRkkPendingSignatureItemDto>>` |
| `SpkRkkPublicVerificationController` | `GET` | `/api/v1/public/clinical-privileges/verify/{hash}` | anonim (rate limit) | — | `ApiResponse<SpkRkkPublicVerificationDto>` |
| `WfpCertificationController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications/filters/metadata` | `WorkforceCertification : Read` | — | `ApiResponse<WfpCertificationFilterMetadataResponse>` |
| `WfpCertificationController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications/summary` | `WorkforceCertification : Read` | — | `ApiResponse<WfpCertificationSummaryResponse>` |
| `WfpCertificationController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications` | `WorkforceCertification : Read` | — | `ApiResponse<PagedResult<WfpCertificationResponse>>` |
| `WfpCertificationController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications/{id:guid}` | `WorkforceCertification : Read` | — | `ApiResponse<WfpCertificationDetailResponse>` |
| `WfpCertificationController` | `POST` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications` | `WorkforceCertification : Create` | `CreateWfpCertificationRequest` | `ApiResponse<WfpCertificationDetailResponse>` |
| `WfpCertificationController` | `PUT` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications/{id:guid}` | `WorkforceCertification : Update` | `UpdateWfpCertificationRequest` | `ApiResponse<WfpCertificationDetailResponse>` |
| `WfpCertificationController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications/{id:guid}/file` | `WorkforceCertification : Read` | — | — |
| `WfpCertificationController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications/{id:guid}/status` | `WorkforceCertification : Update` | `UpdateWfpCertificationStatusRequest` | `ApiResponse<WfpCertificationDetailResponse>` |
| `WfpCertificationController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications/{id:guid}/verify` | `WorkforceCertification : Update` | `VerifyWfpCertificationRequest` | `ApiResponse<WfpCertificationDetailResponse>` |
| `WfpCertificationController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications/portfolio` | `WorkforceCertification : Read` | — | `ApiResponse<List<WfpCertificationPortfolioItemResponse>>` |
| `WfpCertificationController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certification-portfolio` | `WorkforceCertification : Read` | — | `ApiResponse<List<WfpCertificationPortfolioItemResponse>>` |
| `WfpCertificationController` | `DELETE` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications/{id:guid}` | `WorkforceCertification : Delete` | — | `ApiResponse<object>` |
| `WfpClinicalPrivilegeController` | `GET` | `/api/v1/corporate/human-resource/clinical-privileges/registry` | `WorkforceClinicalPrivilege : Read` | — | `ApiResponse<PagedResult<ClinicalPrivilegeRegistryItemDto>>` |
| `WfpClinicalPrivilegeController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges/filters/metadata` | `WorkforceClinicalPrivilege : Read` | — | `ApiResponse<WfpClinicalPrivilegeFilterMetadataResponse>` |
| `WfpClinicalPrivilegeController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges/summary` | `WorkforceClinicalPrivilege : Read` | — | `ApiResponse<WfpClinicalPrivilegeSummaryResponse>` |
| `WfpClinicalPrivilegeController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges` | `WorkforceClinicalPrivilege : Read` | — | `ApiResponse<PagedResult<WfpClinicalPrivilegeResponse>>` |
| `WfpClinicalPrivilegeController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges/{id:guid}` | `WorkforceClinicalPrivilege : Read` | — | `ApiResponse<WfpClinicalPrivilegeDetailResponse>` |
| `WfpClinicalPrivilegeController` | `POST` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges` | `WorkforceClinicalPrivilege : Create` | `CreateWfpClinicalPrivilegeRequest` | `ApiResponse<WfpClinicalPrivilegeDetailResponse>` |
| `WfpClinicalPrivilegeController` | `PUT` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges/{id:guid}` | `WorkforceClinicalPrivilege : Update` | `UpdateWfpClinicalPrivilegeRequest` | `ApiResponse<WfpClinicalPrivilegeDetailResponse>` |
| `WfpClinicalPrivilegeController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges/{id:guid}/status` | `WorkforceClinicalPrivilege : Update` | `UpdateWfpClinicalPrivilegeStatusRequest` | `ApiResponse<WfpClinicalPrivilegeDetailResponse>` |
| `WfpClinicalPrivilegeController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges/{id:guid}/grant` | `WorkforceClinicalPrivilege : Update` | `GrantWfpClinicalPrivilegeRequest` | `ApiResponse<WfpClinicalPrivilegeDetailResponse>` |
| `WfpClinicalPrivilegeController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges/{id:guid}/reject` | `WorkforceClinicalPrivilege : Update` | `RejectWfpClinicalPrivilegeRequest` | `ApiResponse<WfpClinicalPrivilegeDetailResponse>` |
| `WfpClinicalPrivilegeController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges/{id:guid}/suspend` | `WorkforceClinicalPrivilege : Update` | `SuspendWfpClinicalPrivilegeRequest` | `ApiResponse<WfpClinicalPrivilegeDetailResponse>` |
| `WfpClinicalPrivilegeController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges/{id:guid}/revoke` | `WorkforceClinicalPrivilege : Update` | `RevokeWfpClinicalPrivilegeRequest` | `ApiResponse<WfpClinicalPrivilegeDetailResponse>` |
| `WfpClinicalPrivilegeController` | `DELETE` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/clinical-privileges/{id:guid}` | `WorkforceClinicalPrivilege : Delete` | — | `ApiResponse<object>` |
| `WfpComplianceAlertController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts/summary` | `WorkforceComplianceAlert : Read` | — | `ApiResponse<WfpComplianceAlertSummaryResponse>` |
| `WfpComplianceAlertController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts` | `WorkforceComplianceAlert : Read` | — | `ApiResponse<PagedResult<WfpComplianceAlertResponse>>` |
| `WfpComplianceAlertController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts/{id:guid}` | `WorkforceComplianceAlert : Read` | — | `ApiResponse<WfpComplianceAlertDetailResponse>` |
| `WfpComplianceAlertController` | `POST` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts` | `WorkforceComplianceAlert : Create` | `CreateWfpComplianceAlertRequest` | `ApiResponse<WfpComplianceAlertDetailResponse>` |
| `WfpComplianceAlertController` | `PUT` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts/{id:guid}` | `WorkforceComplianceAlert : Update` | `UpdateWfpComplianceAlertRequest` | `ApiResponse<WfpComplianceAlertDetailResponse>` |
| `WfpComplianceAlertController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts/{id:guid}/status` | `WorkforceComplianceAlert : Update` | `UpdateWfpComplianceAlertStatusRequest` | — |
| `WfpComplianceAlertController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts/{id:guid}/resolve` | `WorkforceComplianceAlert : Update` | `ResolveWfpComplianceAlertRequest` | — |
| `WfpComplianceAlertController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts/{id:guid}/reopen` | `WorkforceComplianceAlert : Update` | `ReopenWfpComplianceAlertRequest` | — |
| `WfpComplianceAlertController` | `DELETE` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts/{id:guid}` | `WorkforceComplianceAlert : Delete` | — | — |
| `WfpComplianceAlertLogController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts/{complianceAlertId:guid}/logs` | `WorkforceComplianceAlertLog : Read` | — | `ApiResponse<PagedResult<WfpComplianceAlertLogResponse>>` |
| `WfpComplianceAlertLogController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts/{complianceAlertId:guid}/logs/{id:guid}` | `WorkforceComplianceAlertLog : Read` | — | `ApiResponse<WfpComplianceAlertLogDetailResponse>` |
| `WfpComplianceAlertLogController` | `POST` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/compliance-alerts/{complianceAlertId:guid}/logs` | `WorkforceComplianceAlertLog : Create` | `CreateWfpComplianceAlertLogRequest` | `ApiResponse<WfpComplianceAlertLogDetailResponse>` |
| `WfpCredentialLicenseController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses/filters/metadata` | `WorkforceCredentialLicense : Read` | — | `ApiResponse<WfpCredentialLicenseFilterMetadataResponse>` |
| `WfpCredentialLicenseController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses/summary` | `WorkforceCredentialLicense : Read` | — | `ApiResponse<WfpCredentialLicenseSummaryResponse>` |
| `WfpCredentialLicenseController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses` | `WorkforceCredentialLicense : Read` | — | `ApiResponse<PagedResult<WfpCredentialLicenseResponse>>` |
| `WfpCredentialLicenseController` | `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses/{id:guid}` | `WorkforceCredentialLicense : Read` | — | `ApiResponse<WfpCredentialLicenseDetailResponse>` |
| `WfpCredentialLicenseController` | `POST` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses` | `WorkforceCredentialLicense : Create` | `CreateWfpCredentialLicenseRequest` | `ApiResponse<WfpCredentialLicenseDetailResponse>` |
| `WfpCredentialLicenseController` | `PUT` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses/{id:guid}` | `WorkforceCredentialLicense : Update` | `UpdateWfpCredentialLicenseRequest` | `ApiResponse<WfpCredentialLicenseDetailResponse>` |
| `WfpCredentialLicenseController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses/{id:guid}/status` | `WorkforceCredentialLicense : Update` | `UpdateWfpCredentialLicenseStatusRequest` | `ApiResponse<WfpCredentialLicenseDetailResponse>` |
| `WfpCredentialLicenseController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses/{id:guid}/primary` | `WorkforceCredentialLicense : Update` | `SetWfpCredentialLicensePrimaryRequest` | `ApiResponse<WfpCredentialLicenseDetailResponse>` |
| `WfpCredentialLicenseController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses/{id:guid}/verify` | `WorkforceCredentialLicense : Update` | `VerifyWfpCredentialLicenseRequest` | `ApiResponse<WfpCredentialLicenseDetailResponse>` |
| `WfpCredentialLicenseController` | `PATCH` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses/{id:guid}/revoke` | `WorkforceCredentialLicense : Update` | `RevokeWfpCredentialLicenseRequest` | `ApiResponse<WfpCredentialLicenseDetailResponse>` |
| `WfpCredentialLicenseController` | `DELETE` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses/{id:guid}` | `WorkforceCredentialLicense : Delete` | — | `ApiResponse<object>` |
| `CompetencyController` | `GET` | `/api/v1/corporate/human-resource/master-data/competencies/positions/{positionId:guid}/compliance` | `Competency : Read` | — | `ApiResponse<PositionCompetencyComplianceSummaryResponse>` |
| `RecredentialingQualityController` | `GET` | `/api/v1/corporate/hrd/credentialing/recredentialing/cycles` | `RecredentialingQuality : Read` | — | `RecredentialingCycleListItemDto` (paged) |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Seluruh controller `CredentialingManagement`, `CompetencyController` (matriks kepatuhan), `RecredentialingQualityController` (radar).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan source khusus task ini; verifikasi source |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Delta: lihat laporan `427`–`437` |
| Database | Tanpa migration |
| Keamanan/Auth | Penanda baru `Competency : ReadAllUnitsCompliance` |

## 4. Dokumentasi endpoint

Lihat tabel jejak endpoint pada bagian 2.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (14 berkas Menu 09) | 14 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| 56 UAT Menu 09 | Belum dijalankan | `NOT RUN` | Wewenang uji DB dev belum diberikan |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Semua endpoint ditrace method+URI+permission+DTO | Terpenuhi | tabel jejak 83 baris |
| (2) P1 sisa punya blocker dan pemilik | Terpenuhi | sisa: pencocokan faskes SIP bebas teks, pencocokan sertifikat-kompetensi berbasis nama, nama pejabat penandatangan — pemilik: tim HR backend |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Alasan 🟡 | P0 `M09-P0-10` (`BE-HRD-434`) belum tertutup penuh |
| Langkah berikutnya | `BE-HRD-381` dan slice `S-FIN-ADM` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
