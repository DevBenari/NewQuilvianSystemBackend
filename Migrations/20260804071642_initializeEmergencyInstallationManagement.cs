using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeEmergencyInstallationManagement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstEmergencyArrivalMode",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsAmbulance = table.Column<bool>(type: "boolean", nullable: false),
                    IsReferral = table.Column<bool>(type: "boolean", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstEmergencyArrivalMode", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstEmergencyCaseType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstEmergencyCaseType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstEmergencyDispositionType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RequiresDestinationServiceUnit = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresReferralFacility = table.Column<bool>(type: "boolean", nullable: false),
                    ClosesEmergencyVisit = table.Column<bool>(type: "boolean", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstEmergencyDispositionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstEmergencySetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DefaultEmergencyServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriageSystem = table.Column<int>(type: "integer", nullable: false),
                    AllowProvisionalRegistration = table.Column<bool>(type: "boolean", nullable: false),
                    AllowUnknownPatient = table.Column<bool>(type: "boolean", nullable: false),
                    AutoCreateProvisionalEncounter = table.Column<bool>(type: "boolean", nullable: false),
                    ImmediateCareLevelThreshold = table.Column<int>(type: "integer", nullable: false),
                    RequireRegistrationBeforeTreatmentFromLevel = table.Column<int>(type: "integer", nullable: false),
                    RequireTriageBeforeStandardRegistration = table.Column<bool>(type: "boolean", nullable: false),
                    RequireRegistrationCompletionBeforeDisposition = table.Column<bool>(type: "boolean", nullable: false),
                    TemporaryPatientNumberPrefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EmergencyVisitNumberPrefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstEmergencySetting", x => x.Id);
                    table.CheckConstraint("CK_MstEmergencySetting_ImmediateCareLevelThreshold", "\"ImmediateCareLevelThreshold\" >= 1 AND \"ImmediateCareLevelThreshold\" <= 5");
                    table.CheckConstraint("CK_MstEmergencySetting_RequireRegistrationLevel", "\"RequireRegistrationBeforeTreatmentFromLevel\" >= 1 AND \"RequireRegistrationBeforeTreatmentFromLevel\" <= 5");
                    table.ForeignKey(
                        name: "FK_MstEmergencySetting_MstServiceUnit_DefaultEmergencyServiceU~",
                        column: x => x.DefaultEmergencyServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstEmergencyTriageLevel",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TriageSystem = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ColorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ColorHex = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MaxWaitingMinutes = table.Column<int>(type: "integer", nullable: false),
                    AllowsTreatmentBeforeRegistration = table.Column<bool>(type: "boolean", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstEmergencyTriageLevel", x => x.Id);
                    table.CheckConstraint("CK_MstEmergencyTriageLevel_Level", "\"Level\" >= 1 AND \"Level\" <= 5");
                    table.CheckConstraint("CK_MstEmergencyTriageLevel_MaxWaitingMinutes", "\"MaxWaitingMinutes\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "TrxEmergencyVisit",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyVisitNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArrivalModeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CaseTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArrivalDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ArrivalLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FoundLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TraumaLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TraumaDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsUnknownPatient = table.Column<bool>(type: "boolean", nullable: false),
                    TemporaryPatientAlias = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsImmediateCareAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    RegistrationStatus = table.Column<int>(type: "integer", nullable: false),
                    VisitStatus = table.Column<int>(type: "integer", nullable: false),
                    RegistrationCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegistrationCompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TreatmentStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VisitCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmergencyVisit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyVisit_AspNetUsers_RegistrationCompletedByUserId",
                        column: x => x.RegistrationCompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyVisit_MstEmergencyArrivalMode_ArrivalModeId",
                        column: x => x.ArrivalModeId,
                        principalSchema: "public",
                        principalTable: "MstEmergencyArrivalMode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyVisit_MstEmergencyCaseType_CaseTypeId",
                        column: x => x.CaseTypeId,
                        principalSchema: "public",
                        principalTable: "MstEmergencyCaseType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyVisit_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyVisit_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyVisit_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstEmergencyTriageIndicator",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TriageLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IndicatorGroup = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstEmergencyTriageIndicator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstEmergencyTriageIndicator_MstEmergencyTriageLevel_TriageL~",
                        column: x => x.TriageLevelId,
                        principalSchema: "public",
                        principalTable: "MstEmergencyTriageLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxEmergencyDisposition",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyVisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispositionTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DispositionStatus = table.Column<int>(type: "integer", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DestinationServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DestinationFacilityName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ReferralNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DispositionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PatientConditionAtDisposition = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FollowUpInstruction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RefusalReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsPatientDeceased = table.Column<bool>(type: "boolean", nullable: false),
                    DeathDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeathLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SuspectedCauseOfDeath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsVisumRequested = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmergencyDisposition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyDisposition_AspNetUsers_ConfirmedByUserId",
                        column: x => x.ConfirmedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyDisposition_MstDoctor_DecidedByDoctorId",
                        column: x => x.DecidedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyDisposition_MstEmergencyDispositionType_Disposi~",
                        column: x => x.DispositionTypeId,
                        principalSchema: "public",
                        principalTable: "MstEmergencyDispositionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyDisposition_MstServiceUnit_DestinationServiceUn~",
                        column: x => x.DestinationServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyDisposition_TrxEmergencyVisit_EmergencyVisitId",
                        column: x => x.EmergencyVisitId,
                        principalSchema: "public",
                        principalTable: "TrxEmergencyVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxEmergencyObservation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyVisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ObservationStatus = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ObservationLocation = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Indication = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ObservationPlan = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ResponsibleDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponsibleNurseUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletionSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EscalationReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmergencyObservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyObservation_AspNetUsers_ResponsibleNurseUserId",
                        column: x => x.ResponsibleNurseUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyObservation_MstDoctor_ResponsibleDoctorId",
                        column: x => x.ResponsibleDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyObservation_TrxEmergencyVisit_EmergencyVisitId",
                        column: x => x.EmergencyVisitId,
                        principalSchema: "public",
                        principalTable: "TrxEmergencyVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxEmergencyResuscitation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyVisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResuscitationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResuscitationStatus = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Location = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TriggerCondition = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TeamLeaderDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WasCardiopulmonaryResuscitationPerformed = table.Column<bool>(type: "boolean", nullable: false),
                    CardiopulmonaryResuscitationStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReturnOfSpontaneousCirculationAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DefibrillationCount = table.Column<int>(type: "integer", nullable: false),
                    AirwayManagementSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BreathingManagementSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CirculationManagementSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NeurologicalManagementSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OutcomeSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmergencyResuscitation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyResuscitation_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyResuscitation_MstDoctor_TeamLeaderDoctorId",
                        column: x => x.TeamLeaderDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyResuscitation_TrxEmergencyVisit_EmergencyVisitId",
                        column: x => x.EmergencyVisitId,
                        principalSchema: "public",
                        principalTable: "TrxEmergencyVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxEmergencyTransfer",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyVisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FromServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    FromBedId = table.Column<Guid>(type: "uuid", nullable: true),
                    ToBedId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransferStatus = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcceptedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ArrivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SendingNurseUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivingNurseUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransferReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HandoverSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmergencyTransfer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTransfer_AspNetUsers_AcceptedByUserId",
                        column: x => x.AcceptedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTransfer_AspNetUsers_ReceivingNurseUserId",
                        column: x => x.ReceivingNurseUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTransfer_AspNetUsers_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTransfer_AspNetUsers_SendingNurseUserId",
                        column: x => x.SendingNurseUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTransfer_MstServiceUnit_FromServiceUnitId",
                        column: x => x.FromServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTransfer_MstServiceUnit_ToServiceUnitId",
                        column: x => x.ToServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTransfer_TrxEmergencyVisit_EmergencyVisitId",
                        column: x => x.EmergencyVisitId,
                        principalSchema: "public",
                        principalTable: "TrxEmergencyVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxEmergencyTriage",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyVisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriageLevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientVitalSignId = table.Column<Guid>(type: "uuid", nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    IsRetriage = table.Column<bool>(type: "boolean", nullable: false),
                    PreviousTriageId = table.Column<Guid>(type: "uuid", nullable: true),
                    TriageSystem = table.Column<int>(type: "integer", nullable: false),
                    TriageStatus = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxWaitingMinutesSnapshot = table.Column<int>(type: "integer", nullable: false),
                    ResponseDueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImmediateCareAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    TriageReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AirwaySummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BreathingSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CirculationSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DisabilitySummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExposureSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RedFlagSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmergencyTriage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTriage_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTriage_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTriage_MstEmergencyTriageLevel_TriageLevelId",
                        column: x => x.TriageLevelId,
                        principalSchema: "public",
                        principalTable: "MstEmergencyTriageLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTriage_TrxEmergencyTriage_PreviousTriageId",
                        column: x => x.PreviousTriageId,
                        principalSchema: "public",
                        principalTable: "TrxEmergencyTriage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTriage_TrxEmergencyVisit_EmergencyVisitId",
                        column: x => x.EmergencyVisitId,
                        principalSchema: "public",
                        principalTable: "TrxEmergencyVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTriage_TrxPatientVitalSign_PatientVitalSignId",
                        column: x => x.PatientVitalSignId,
                        principalSchema: "public",
                        principalTable: "TrxPatientVitalSign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxEmergencyObservationDetail",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyObservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientVitalSignId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProgressNoteId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicalConditionSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InterventionSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PatientResponseSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FluidIntakeMl = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    UrineOutputMl = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OtherOutputMl = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BleedingEstimatedMl = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    VomitEstimatedMl = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmergencyObservationDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyObservationDetail_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyObservationDetail_TrxEmergencyObservation_Emerg~",
                        column: x => x.EmergencyObservationId,
                        principalSchema: "public",
                        principalTable: "TrxEmergencyObservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyObservationDetail_TrxPatientIntegratedProgressN~",
                        column: x => x.ProgressNoteId,
                        principalSchema: "public",
                        principalTable: "TrxPatientIntegratedProgressNote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyObservationDetail_TrxPatientVitalSign_PatientVi~",
                        column: x => x.PatientVitalSignId,
                        principalSchema: "public",
                        principalTable: "TrxPatientVitalSign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxEmergencyProcedureDetail",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyVisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyResuscitationId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmergencyObservationId = table.Column<Guid>(type: "uuid", nullable: true),
                    DetailType = table.Column<int>(type: "integer", nullable: false),
                    SkinTestResult = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    TetanusToxoidResult = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AntiTetanusSerumAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    AntiTetanusSerumUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MedicationRoute = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MedicationDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmergencySpecificResult = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmergencyProcedureDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyProcedureDetail_TrxEmergencyObservation_Emergen~",
                        column: x => x.EmergencyObservationId,
                        principalSchema: "public",
                        principalTable: "TrxEmergencyObservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyProcedureDetail_TrxEmergencyResuscitation_Emerg~",
                        column: x => x.EmergencyResuscitationId,
                        principalSchema: "public",
                        principalTable: "TrxEmergencyResuscitation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyProcedureDetail_TrxEmergencyVisit_EmergencyVisi~",
                        column: x => x.EmergencyVisitId,
                        principalSchema: "public",
                        principalTable: "TrxEmergencyVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyProcedureDetail_TrxPatientProcedure_PatientProc~",
                        column: x => x.PatientProcedureId,
                        principalSchema: "public",
                        principalTable: "TrxPatientProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxEmergencyTriageDetail",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyTriageId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriageIndicatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    IndicatorCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IndicatorNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IndicatorGroupSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ObservedValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Score = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    IsMatched = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxEmergencyTriageDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTriageDetail_MstEmergencyTriageIndicator_Triage~",
                        column: x => x.TriageIndicatorId,
                        principalSchema: "public",
                        principalTable: "MstEmergencyTriageIndicator",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxEmergencyTriageDetail_TrxEmergencyTriage_EmergencyTriage~",
                        column: x => x.EmergencyTriageId,
                        principalSchema: "public",
                        principalTable: "TrxEmergencyTriage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyArrivalMode_Code",
                schema: "public",
                table: "MstEmergencyArrivalMode",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyArrivalMode_IsActive_Sequence",
                schema: "public",
                table: "MstEmergencyArrivalMode",
                columns: new[] { "IsActive", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyArrivalMode_IsAmbulance_IsReferral",
                schema: "public",
                table: "MstEmergencyArrivalMode",
                columns: new[] { "IsAmbulance", "IsReferral" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyCaseType_Code",
                schema: "public",
                table: "MstEmergencyCaseType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyCaseType_IsActive_Sequence",
                schema: "public",
                table: "MstEmergencyCaseType",
                columns: new[] { "IsActive", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyDispositionType_Code",
                schema: "public",
                table: "MstEmergencyDispositionType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyDispositionType_IsActive_Sequence",
                schema: "public",
                table: "MstEmergencyDispositionType",
                columns: new[] { "IsActive", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyDispositionType_RequiresDestinationServiceUnit_~",
                schema: "public",
                table: "MstEmergencyDispositionType",
                columns: new[] { "RequiresDestinationServiceUnit", "RequiresReferralFacility", "ClosesEmergencyVisit" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencySetting_Code",
                schema: "public",
                table: "MstEmergencySetting",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencySetting_DefaultEmergencyServiceUnitId",
                schema: "public",
                table: "MstEmergencySetting",
                column: "DefaultEmergencyServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencySetting_IsActive_IsDefault",
                schema: "public",
                table: "MstEmergencySetting",
                columns: new[] { "IsActive", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyTriageIndicator_Code",
                schema: "public",
                table: "MstEmergencyTriageIndicator",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyTriageIndicator_TriageLevelId_IsActive",
                schema: "public",
                table: "MstEmergencyTriageIndicator",
                columns: new[] { "TriageLevelId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyTriageIndicator_TriageLevelId_Sequence",
                schema: "public",
                table: "MstEmergencyTriageIndicator",
                columns: new[] { "TriageLevelId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyTriageLevel_Code",
                schema: "public",
                table: "MstEmergencyTriageLevel",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyTriageLevel_IsActive_Sequence",
                schema: "public",
                table: "MstEmergencyTriageLevel",
                columns: new[] { "IsActive", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmergencyTriageLevel_TriageSystem_Level",
                schema: "public",
                table: "MstEmergencyTriageLevel",
                columns: new[] { "TriageSystem", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyDisposition_ConfirmedByUserId",
                schema: "public",
                table: "TrxEmergencyDisposition",
                column: "ConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyDisposition_DecidedByDoctorId",
                schema: "public",
                table: "TrxEmergencyDisposition",
                column: "DecidedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyDisposition_DestinationServiceUnitId",
                schema: "public",
                table: "TrxEmergencyDisposition",
                column: "DestinationServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyDisposition_DispositionTypeId",
                schema: "public",
                table: "TrxEmergencyDisposition",
                column: "DispositionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyDisposition_EmergencyVisitId_DispositionStatus_~",
                schema: "public",
                table: "TrxEmergencyDisposition",
                columns: new[] { "EmergencyVisitId", "DispositionStatus", "DecidedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyObservation_EmergencyVisitId_ObservationStatus_~",
                schema: "public",
                table: "TrxEmergencyObservation",
                columns: new[] { "EmergencyVisitId", "ObservationStatus", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyObservation_ObservationNumber",
                schema: "public",
                table: "TrxEmergencyObservation",
                column: "ObservationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyObservation_ResponsibleDoctorId",
                schema: "public",
                table: "TrxEmergencyObservation",
                column: "ResponsibleDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyObservation_ResponsibleNurseUserId",
                schema: "public",
                table: "TrxEmergencyObservation",
                column: "ResponsibleNurseUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyObservationDetail_EmergencyObservationId_Record~",
                schema: "public",
                table: "TrxEmergencyObservationDetail",
                columns: new[] { "EmergencyObservationId", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyObservationDetail_PatientVitalSignId",
                schema: "public",
                table: "TrxEmergencyObservationDetail",
                column: "PatientVitalSignId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyObservationDetail_ProgressNoteId",
                schema: "public",
                table: "TrxEmergencyObservationDetail",
                column: "ProgressNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyObservationDetail_RecordedByUserId",
                schema: "public",
                table: "TrxEmergencyObservationDetail",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyProcedureDetail_EmergencyObservationId",
                schema: "public",
                table: "TrxEmergencyProcedureDetail",
                column: "EmergencyObservationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyProcedureDetail_EmergencyResuscitationId",
                schema: "public",
                table: "TrxEmergencyProcedureDetail",
                column: "EmergencyResuscitationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyProcedureDetail_EmergencyVisitId_DetailType",
                schema: "public",
                table: "TrxEmergencyProcedureDetail",
                columns: new[] { "EmergencyVisitId", "DetailType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyProcedureDetail_PatientProcedureId",
                schema: "public",
                table: "TrxEmergencyProcedureDetail",
                column: "PatientProcedureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyResuscitation_EmergencyVisitId_ResuscitationSta~",
                schema: "public",
                table: "TrxEmergencyResuscitation",
                columns: new[] { "EmergencyVisitId", "ResuscitationStatus", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyResuscitation_RecordedByUserId",
                schema: "public",
                table: "TrxEmergencyResuscitation",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyResuscitation_ResuscitationNumber",
                schema: "public",
                table: "TrxEmergencyResuscitation",
                column: "ResuscitationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyResuscitation_TeamLeaderDoctorId",
                schema: "public",
                table: "TrxEmergencyResuscitation",
                column: "TeamLeaderDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_AcceptedByUserId",
                schema: "public",
                table: "TrxEmergencyTransfer",
                column: "AcceptedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_EmergencyVisitId_TransferStatus_Reques~",
                schema: "public",
                table: "TrxEmergencyTransfer",
                columns: new[] { "EmergencyVisitId", "TransferStatus", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_FromBedId",
                schema: "public",
                table: "TrxEmergencyTransfer",
                column: "FromBedId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_FromRoomId",
                schema: "public",
                table: "TrxEmergencyTransfer",
                column: "FromRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_FromServiceUnitId",
                schema: "public",
                table: "TrxEmergencyTransfer",
                column: "FromServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_ReceivingNurseUserId",
                schema: "public",
                table: "TrxEmergencyTransfer",
                column: "ReceivingNurseUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_RequestedByUserId",
                schema: "public",
                table: "TrxEmergencyTransfer",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_SendingNurseUserId",
                schema: "public",
                table: "TrxEmergencyTransfer",
                column: "SendingNurseUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_ToBedId",
                schema: "public",
                table: "TrxEmergencyTransfer",
                column: "ToBedId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_ToRoomId",
                schema: "public",
                table: "TrxEmergencyTransfer",
                column: "ToRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_ToServiceUnitId_TransferStatus",
                schema: "public",
                table: "TrxEmergencyTransfer",
                columns: new[] { "ToServiceUnitId", "TransferStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTransfer_TransferNumber",
                schema: "public",
                table: "TrxEmergencyTransfer",
                column: "TransferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriage_EmergencyVisitId_Sequence",
                schema: "public",
                table: "TrxEmergencyTriage",
                columns: new[] { "EmergencyVisitId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriage_EmergencyVisitId_TriageStatus_StartedAt",
                schema: "public",
                table: "TrxEmergencyTriage",
                columns: new[] { "EmergencyVisitId", "TriageStatus", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriage_PatientVitalSignId",
                schema: "public",
                table: "TrxEmergencyTriage",
                column: "PatientVitalSignId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriage_PerformedByUserId",
                schema: "public",
                table: "TrxEmergencyTriage",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriage_PreviousTriageId",
                schema: "public",
                table: "TrxEmergencyTriage",
                column: "PreviousTriageId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriage_ResponseDueAt",
                schema: "public",
                table: "TrxEmergencyTriage",
                column: "ResponseDueAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriage_ReviewedByUserId",
                schema: "public",
                table: "TrxEmergencyTriage",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriage_TriageLevelId",
                schema: "public",
                table: "TrxEmergencyTriage",
                column: "TriageLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriageDetail_EmergencyTriageId_Sequence",
                schema: "public",
                table: "TrxEmergencyTriageDetail",
                columns: new[] { "EmergencyTriageId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriageDetail_EmergencyTriageId_TriageIndicatorId",
                schema: "public",
                table: "TrxEmergencyTriageDetail",
                columns: new[] { "EmergencyTriageId", "TriageIndicatorId" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriageDetail_IsMatched",
                schema: "public",
                table: "TrxEmergencyTriageDetail",
                column: "IsMatched");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriageDetail_TriageIndicatorId",
                schema: "public",
                table: "TrxEmergencyTriageDetail",
                column: "TriageIndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyVisit_ArrivalModeId",
                schema: "public",
                table: "TrxEmergencyVisit",
                column: "ArrivalModeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyVisit_CaseTypeId",
                schema: "public",
                table: "TrxEmergencyVisit",
                column: "CaseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyVisit_EmergencyVisitNumber",
                schema: "public",
                table: "TrxEmergencyVisit",
                column: "EmergencyVisitNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyVisit_EncounterId",
                schema: "public",
                table: "TrxEmergencyVisit",
                column: "EncounterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyVisit_PatientId_ArrivalDateTime",
                schema: "public",
                table: "TrxEmergencyVisit",
                columns: new[] { "PatientId", "ArrivalDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyVisit_RegistrationCompletedByUserId",
                schema: "public",
                table: "TrxEmergencyVisit",
                column: "RegistrationCompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyVisit_RegistrationStatus_ArrivalDateTime",
                schema: "public",
                table: "TrxEmergencyVisit",
                columns: new[] { "RegistrationStatus", "ArrivalDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyVisit_ServiceUnitId_VisitStatus_ArrivalDateTime",
                schema: "public",
                table: "TrxEmergencyVisit",
                columns: new[] { "ServiceUnitId", "VisitStatus", "ArrivalDateTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstEmergencySetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmergencyDisposition",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmergencyObservationDetail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmergencyProcedureDetail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmergencyTransfer",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmergencyTriageDetail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstEmergencyDispositionType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmergencyObservation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmergencyResuscitation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstEmergencyTriageIndicator",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmergencyTriage",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstEmergencyTriageLevel",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxEmergencyVisit",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstEmergencyArrivalMode",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstEmergencyCaseType",
                schema: "public");
        }
    }
}
