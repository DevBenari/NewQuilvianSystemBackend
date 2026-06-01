using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddChildDoctorConsultant1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrxPatientAllergy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllergyRecordNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllergyCategory = table.Column<int>(type: "integer", nullable: false),
                    AllergenCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AllergenName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    AllergenGroupName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ReactionType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReactionDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Certainty = table.Column<int>(type: "integer", nullable: false),
                    AllergyStatus = table.Column<int>(type: "integer", nullable: false),
                    FirstReactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReportedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceOfInformation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsHighRisk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsLifeThreatening = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAlertEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PatientSafetyNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicalNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPatientAllergy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientAllergy_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAllergy_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAllergy_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAllergy_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAllergy_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAllergy_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAllergy_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAllergy_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAllergy_TrxDoctorConsultation_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAllergy_TrxPatientAssessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientAllergy_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPatientFamilyHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FamilyHistoryRecordNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    FamilyMemberPatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosisId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelationshipType = table.Column<int>(type: "integer", nullable: false),
                    RelationshipSide = table.Column<int>(type: "integer", nullable: false),
                    FamilyMemberNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RelationshipDescription = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsFirstDegreeRelative = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSecondDegreeRelative = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSameHousehold = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ConditionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConditionName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ConditionGroupName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ConditionMasterType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Manual"),
                    IcdVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsFromMasterDiagnosis = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FamilyHistoryStatus = table.Column<int>(type: "integer", nullable: false),
                    Certainty = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<int>(type: "integer", nullable: false),
                    IsHereditaryDisease = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsGeneticRisk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsChronicDisease = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCancerRelated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCardiovascularRisk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsMetabolicRisk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsMentalHealthRelated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsInfectiousDisease = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsHighRisk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsScreeningRecommended = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAlertEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RecordedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DiagnosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AgeAtDiagnosisYear = table.Column<int>(type: "integer", nullable: true),
                    IsFamilyMemberDeceased = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeathDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AgeAtDeathYear = table.Column<int>(type: "integer", nullable: true),
                    CauseOfDeath = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SourceOfInformation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ClinicalNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RiskNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ScreeningRecommendation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPatientFamilyHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_MstDiagnosis_DiagnosisId",
                        column: x => x.DiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_MstPatient_FamilyMemberPatientId",
                        column: x => x.FamilyMemberPatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_TrxDoctorConsultation_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_TrxPatientAssessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientFamilyHistory_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxPatientMedicalHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicalHistoryRecordNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiagnosisId = table.Column<Guid>(type: "uuid", nullable: true),
                    HistoryType = table.Column<int>(type: "integer", nullable: false),
                    HistoryStatus = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    Certainty = table.Column<int>(type: "integer", nullable: false),
                    ConditionCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConditionName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ConditionGroupName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ConditionMasterType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Manual"),
                    IcdVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsFromMasterDiagnosis = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCurrentProblem = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsChronic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsComorbidity = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsUnderTreatment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsControlled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsInfectiousDisease = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsHereditaryRelated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsMentalHealthRelated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPregnancyRelated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSurgicalHistory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsHospitalizationHistory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsHighRisk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAlertEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RecordedDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OnsetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OnsetAgeYear = table.Column<int>(type: "integer", nullable: true),
                    DiagnosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastTreatmentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastControlDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SourceOfInformation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TreatmentHistory = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MedicationHistory = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SurgeryHistory = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HospitalizationHistory = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ComplicationNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ClinicalNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RiskNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxPatientMedicalHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxPatientMedicalHistory_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientMedicalHistory_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientMedicalHistory_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientMedicalHistory_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientMedicalHistory_MstDiagnosis_DiagnosisId",
                        column: x => x.DiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientMedicalHistory_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientMedicalHistory_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientMedicalHistory_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientMedicalHistory_TrxDoctorConsultation_Consultation~",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientMedicalHistory_TrxPatientAssessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxPatientMedicalHistory_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_AllergyRecordNumber",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "AllergyRecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_AssessmentId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_CancelledByUserId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_ClinicId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_ConsultationId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_DoctorId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_DrugId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_EncounterId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_PatientId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_PatientId_AllergyCategory_AllergenName",
                schema: "public",
                table: "TrxPatientAllergy",
                columns: new[] { "PatientId", "AllergyCategory", "AllergenName" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_PatientId_AllergyStatus_IsActive",
                schema: "public",
                table: "TrxPatientAllergy",
                columns: new[] { "PatientId", "AllergyStatus", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_PatientId_IsAlertEnabled_IsHighRisk_IsLif~",
                schema: "public",
                table: "TrxPatientAllergy",
                columns: new[] { "PatientId", "IsAlertEnabled", "IsHighRisk", "IsLifeThreatening" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_ResolvedByUserId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_ServiceUnitId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_AssessmentId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_CancelledByUserId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_ClinicId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_ConsultationId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_DiagnosisId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "DiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_DoctorId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_EncounterId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_FamilyHistoryRecordNumber",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "FamilyHistoryRecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_FamilyMemberPatientId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "FamilyMemberPatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_PatientId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_PatientId_FamilyHistoryStatus_IsAct~",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                columns: new[] { "PatientId", "FamilyHistoryStatus", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_PatientId_IsAlertEnabled_IsScreenin~",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                columns: new[] { "PatientId", "IsAlertEnabled", "IsScreeningRecommended" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_PatientId_IsHereditaryDisease_IsGen~",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                columns: new[] { "PatientId", "IsHereditaryDisease", "IsGeneticRisk", "IsHighRisk" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_PatientId_RecordedDateTime",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                columns: new[] { "PatientId", "RecordedDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_PatientId_RelationshipSide_Relation~",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                columns: new[] { "PatientId", "RelationshipSide", "RelationshipType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_PatientId_RelationshipType_Conditio~",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                columns: new[] { "PatientId", "RelationshipType", "ConditionName" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_ResolvedByUserId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_ServiceUnitId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientFamilyHistory_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientFamilyHistory",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_AssessmentId",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_CancelledByUserId",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_ClinicId",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_ConsultationId",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_DiagnosisId",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "DiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_DoctorId",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_EncounterId",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_MedicalHistoryRecordNumber",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "MedicalHistoryRecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_PatientId",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_PatientId_HistoryStatus_IsActive",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                columns: new[] { "PatientId", "HistoryStatus", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_PatientId_HistoryType_ConditionName",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                columns: new[] { "PatientId", "HistoryType", "ConditionName" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_PatientId_IsAlertEnabled_IsHighRisk",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                columns: new[] { "PatientId", "IsAlertEnabled", "IsHighRisk" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_PatientId_IsCurrentProblem_IsChron~",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                columns: new[] { "PatientId", "IsCurrentProblem", "IsChronic", "IsComorbidity" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_PatientId_RecordedDateTime",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                columns: new[] { "PatientId", "RecordedDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_ResolvedByUserId",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_ServiceUnitId",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientMedicalHistory_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientMedicalHistory",
                column: "VerifiedByUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxPatientAllergy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPatientFamilyHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxPatientMedicalHistory",
                schema: "public");
        }
    }
}
