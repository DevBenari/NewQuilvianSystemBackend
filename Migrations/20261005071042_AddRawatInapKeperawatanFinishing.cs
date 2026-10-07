using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRawatInapKeperawatanFinishing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CliFluidBalanceEntry_Volume",
                schema: "public",
                table: "CliFluidBalanceEntry");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                schema: "public",
                table: "InpEpisode",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "GziPatientDiet",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "GziPatientDiet",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "GziPatientDiet",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BbkTransfusionReactionNotice",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicalReactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReactionSummarySnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AcknowledgedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgeNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_BbkTransfusionReactionNotice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkTransfusionReactionNotice_AspNetUsers_AcknowledgedByUser~",
                        column: x => x.AcknowledgedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkTransfusionReactionNotice_BbkBloodUnit_BloodUnitId",
                        column: x => x.BloodUnitId,
                        principalSchema: "public",
                        principalTable: "BbkBloodUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkTransfusionReactionNotice_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkTransfusionReactionNotice_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkTransfusionReactionNotice_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliEquipmentUsage",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicalEquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsibleDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    ChargeUnitSnapshot = table.Column<int>(type: "integer", nullable: false),
                    RoundingRuleSnapshot = table.Column<int>(type: "integer", nullable: false),
                    BilledUnits = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequiresNurseReview = table.Column<bool>(type: "boolean", nullable: false),
                    AutoClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CliEquipmentUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliEquipmentUsage_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliEquipmentUsage_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliEquipmentUsage_MstDoctor_ResponsibleDoctorId",
                        column: x => x.ResponsibleDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliEquipmentUsage_MstMedicalEquipment_MedicalEquipmentId",
                        column: x => x.MedicalEquipmentId,
                        principalSchema: "public",
                        principalTable: "MstMedicalEquipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliEquipmentUsage_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliEquipmentUsage_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliSurgicalSiteSurveillance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SurgeryCompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DayOneDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StoppedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StoppedOnDayNumber = table.Column<int>(type: "integer", nullable: true),
                    SummaryResponsesJson = table.Column<string>(type: "text", nullable: false),
                    NosocomialInfectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SuspectedFlaggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspectedFlaggedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CliSurgicalSiteSurveillance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliSurgicalSiteSurveillance_AspNetUsers_SuspectedFlaggedByU~",
                        column: x => x.SuspectedFlaggedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliSurgicalSiteSurveillance_CliClinicalInstrumentVersion_In~",
                        column: x => x.InstrumentVersionId,
                        principalSchema: "public",
                        principalTable: "CliClinicalInstrumentVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliSurgicalSiteSurveillance_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliSurgicalSiteSurveillance_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliSurgicalSiteSurveillance_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliSurgicalSiteSurveillance_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliSurgicalSiteSurveillance_TrxNosocomialInfection_Nosocomi~",
                        column: x => x.NosocomialInfectionId,
                        principalSchema: "public",
                        principalTable: "TrxNosocomialInfection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliTransfusionMonitoring",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedAtWardAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TransfusionStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    StoppedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StopReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CliTransfusionMonitoring", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliTransfusionMonitoring_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliTransfusionMonitoring_BbkBloodUnit_BloodUnitId",
                        column: x => x.BloodUnitId,
                        principalSchema: "public",
                        principalTable: "BbkBloodUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliTransfusionMonitoring_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliTransfusionMonitoring_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliTransfusionMonitoring_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliWsdDrain",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrainLabel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InsertionSite = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InsertedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InitialResidualMl = table.Column<decimal>(type: "numeric(8,1)", precision: 8, scale: 1, nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RemovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RegisteredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CliWsdDrain", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliWsdDrain_AspNetUsers_RegisteredByUserId",
                        column: x => x.RegisteredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliWsdDrain_AspNetUsers_RemovedByUserId",
                        column: x => x.RemovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliWsdDrain_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliWsdDrain_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliWsdDrain_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliEquipmentUsageRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentUsageId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    PreviousStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousEndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreviousQuantity = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    PreviousBilledUnits = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RevisedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_CliEquipmentUsageRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliEquipmentUsageRevision_AspNetUsers_RevisedByUserId",
                        column: x => x.RevisedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliEquipmentUsageRevision_CliEquipmentUsage_EquipmentUsageId",
                        column: x => x.EquipmentUsageId,
                        principalSchema: "public",
                        principalTable: "CliEquipmentUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliSurgicalSiteSurveillanceEntry",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SurveillanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayNumber = table.Column<int>(type: "integer", nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ResponsesJson = table.Column<string>(type: "text", nullable: false),
                    TemperatureMaxCelsiusSnapshot = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    FeverIndicatorFromVitals = table.Column<bool>(type: "boolean", nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_CliSurgicalSiteSurveillanceEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliSurgicalSiteSurveillanceEntry_AspNetUsers_RecordedByUser~",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliSurgicalSiteSurveillanceEntry_CliSurgicalSiteSurveillanc~",
                        column: x => x.SurveillanceId,
                        principalSchema: "public",
                        principalTable: "CliSurgicalSiteSurveillance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliTransfusionMonitoringPoint",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonitoringId = table.Column<Guid>(type: "uuid", nullable: false),
                    PointType = table.Column<int>(type: "integer", nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MeasuredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SystolicBp = table.Column<int>(type: "integer", nullable: true),
                    DiastolicBp = table.Column<int>(type: "integer", nullable: true),
                    TemperatureCelsius = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    PulseRate = table.Column<int>(type: "integer", nullable: true),
                    IsLate = table.Column<bool>(type: "boolean", nullable: false),
                    IsStopped = table.Column<bool>(type: "boolean", nullable: false),
                    LateNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    CorrectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_CliTransfusionMonitoringPoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliTransfusionMonitoringPoint_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliTransfusionMonitoringPoint_CliTransfusionMonitoring_Moni~",
                        column: x => x.MonitoringId,
                        principalSchema: "public",
                        principalTable: "CliTransfusionMonitoring",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliTransfusionReaction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonitoringId = table.Column<Guid>(type: "uuid", nullable: false),
                    PointType = table.Column<int>(type: "integer", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReactionSummary = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ReactionDetail = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NoticeDelivery = table.Column<int>(type: "integer", nullable: false),
                    NoticeAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    BloodBankNoticeId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_CliTransfusionReaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliTransfusionReaction_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliTransfusionReaction_CliTransfusionMonitoring_MonitoringId",
                        column: x => x.MonitoringId,
                        principalSchema: "public",
                        principalTable: "CliTransfusionMonitoring",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliWsdReading",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WsdDrainId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousResidualMl = table.Column<decimal>(type: "numeric(8,1)", precision: 8, scale: 1, nullable: false),
                    CurrentResidualMl = table.Column<decimal>(type: "numeric(8,1)", precision: 8, scale: 1, nullable: false),
                    DiscardedVolumeMl = table.Column<decimal>(type: "numeric(8,1)", precision: 8, scale: 1, nullable: false),
                    IncreaseMl = table.Column<decimal>(type: "numeric(8,1)", precision: 8, scale: 1, nullable: false),
                    FluidBalanceEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_CliWsdReading", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliWsdReading_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliWsdReading_CliFluidBalanceEntry_FluidBalanceEntryId",
                        column: x => x.FluidBalanceEntryId,
                        principalSchema: "public",
                        principalTable: "CliFluidBalanceEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliWsdReading_CliNursingShift_ShiftId",
                        column: x => x.ShiftId,
                        principalSchema: "public",
                        principalTable: "CliNursingShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliWsdReading_CliWsdDrain_WsdDrainId",
                        column: x => x.WsdDrainId,
                        principalSchema: "public",
                        principalTable: "CliWsdDrain",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliWsdReading_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliSurgicalSiteSurveillanceEntryRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    PreviousResponsesJson = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RevisedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_CliSurgicalSiteSurveillanceEntryRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliSurgicalSiteSurveillanceEntryRevision_AspNetUsers_Revise~",
                        column: x => x.RevisedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliSurgicalSiteSurveillanceEntryRevision_CliSurgicalSiteSur~",
                        column: x => x.EntryId,
                        principalSchema: "public",
                        principalTable: "CliSurgicalSiteSurveillanceEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GziPatientDiet_InstructionVerificationStatus",
                schema: "public",
                table: "GziPatientDiet",
                column: "InstructionVerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_GziPatientDiet_InstructionVerifiedByUserId",
                schema: "public",
                table: "GziPatientDiet",
                column: "InstructionVerifiedByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CliFluidBalanceEntry_Volume",
                schema: "public",
                table: "CliFluidBalanceEntry",
                sql: "(\"SourceCategory\" = 14 AND \"VolumeMl\" >= 0) OR (\"VolumeMl\" > 0 AND \"VolumeMl\" <= 10000)");

            migrationBuilder.CreateIndex(
                name: "IX_BbkTransfusionReactionNotice_AcknowledgedByUserId",
                schema: "public",
                table: "BbkTransfusionReactionNotice",
                column: "AcknowledgedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkTransfusionReactionNotice_BloodUnitId",
                schema: "public",
                table: "BbkTransfusionReactionNotice",
                column: "BloodUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkTransfusionReactionNotice_ClinicalReactionId",
                schema: "public",
                table: "BbkTransfusionReactionNotice",
                column: "ClinicalReactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BbkTransfusionReactionNotice_EncounterId",
                schema: "public",
                table: "BbkTransfusionReactionNotice",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkTransfusionReactionNotice_PatientId",
                schema: "public",
                table: "BbkTransfusionReactionNotice",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkTransfusionReactionNotice_ServiceUnitId",
                schema: "public",
                table: "BbkTransfusionReactionNotice",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkTransfusionReactionNotice_Status_ReceivedAt",
                schema: "public",
                table: "BbkTransfusionReactionNotice",
                columns: new[] { "Status", "ReceivedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CliEquipmentUsage_EncounterId",
                schema: "public",
                table: "CliEquipmentUsage",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliEquipmentUsage_InpEpisodeId",
                schema: "public",
                table: "CliEquipmentUsage",
                column: "InpEpisodeId",
                filter: "\"Status\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_CliEquipmentUsage_MedicalEquipmentId",
                schema: "public",
                table: "CliEquipmentUsage",
                column: "MedicalEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CliEquipmentUsage_PatientId",
                schema: "public",
                table: "CliEquipmentUsage",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CliEquipmentUsage_PerformedByUserId",
                schema: "public",
                table: "CliEquipmentUsage",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliEquipmentUsage_ResponsibleDoctorId",
                schema: "public",
                table: "CliEquipmentUsage",
                column: "ResponsibleDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_CliEquipmentUsageRevision_EquipmentUsageId_RevisionNumber",
                schema: "public",
                table: "CliEquipmentUsageRevision",
                columns: new[] { "EquipmentUsageId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliEquipmentUsageRevision_RevisedByUserId",
                schema: "public",
                table: "CliEquipmentUsageRevision",
                column: "RevisedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliSurgicalSiteSurveillance_EncounterId",
                schema: "public",
                table: "CliSurgicalSiteSurveillance",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliSurgicalSiteSurveillance_InpEpisodeId_Status",
                schema: "public",
                table: "CliSurgicalSiteSurveillance",
                columns: new[] { "InpEpisodeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CliSurgicalSiteSurveillance_InstrumentVersionId",
                schema: "public",
                table: "CliSurgicalSiteSurveillance",
                column: "InstrumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CliSurgicalSiteSurveillance_NosocomialInfectionId",
                schema: "public",
                table: "CliSurgicalSiteSurveillance",
                column: "NosocomialInfectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CliSurgicalSiteSurveillance_OprCaseId",
                schema: "public",
                table: "CliSurgicalSiteSurveillance",
                column: "OprCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliSurgicalSiteSurveillance_PatientId",
                schema: "public",
                table: "CliSurgicalSiteSurveillance",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CliSurgicalSiteSurveillance_SuspectedFlaggedByUserId",
                schema: "public",
                table: "CliSurgicalSiteSurveillance",
                column: "SuspectedFlaggedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliSurgicalSiteSurveillanceEntry_RecordedByUserId",
                schema: "public",
                table: "CliSurgicalSiteSurveillanceEntry",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliSurgicalSiteSurveillanceEntry_SurveillanceId_DayNumber",
                schema: "public",
                table: "CliSurgicalSiteSurveillanceEntry",
                columns: new[] { "SurveillanceId", "DayNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliSurgicalSiteSurveillanceEntryRevision_EntryId_RevisionNu~",
                schema: "public",
                table: "CliSurgicalSiteSurveillanceEntryRevision",
                columns: new[] { "EntryId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliSurgicalSiteSurveillanceEntryRevision_RevisedByUserId",
                schema: "public",
                table: "CliSurgicalSiteSurveillanceEntryRevision",
                column: "RevisedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliTransfusionMonitoring_BloodUnitId",
                schema: "public",
                table: "CliTransfusionMonitoring",
                column: "BloodUnitId",
                unique: true,
                filter: "\"Status\" <> 4");

            migrationBuilder.CreateIndex(
                name: "IX_CliTransfusionMonitoring_EncounterId",
                schema: "public",
                table: "CliTransfusionMonitoring",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliTransfusionMonitoring_InpEpisodeId_Status",
                schema: "public",
                table: "CliTransfusionMonitoring",
                columns: new[] { "InpEpisodeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CliTransfusionMonitoring_PatientId",
                schema: "public",
                table: "CliTransfusionMonitoring",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CliTransfusionMonitoring_PerformedByUserId",
                schema: "public",
                table: "CliTransfusionMonitoring",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliTransfusionMonitoringPoint_MonitoringId_PointType",
                schema: "public",
                table: "CliTransfusionMonitoringPoint",
                columns: new[] { "MonitoringId", "PointType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliTransfusionMonitoringPoint_RecordedByUserId",
                schema: "public",
                table: "CliTransfusionMonitoringPoint",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliTransfusionReaction_MonitoringId",
                schema: "public",
                table: "CliTransfusionReaction",
                column: "MonitoringId");

            migrationBuilder.CreateIndex(
                name: "IX_CliTransfusionReaction_NoticeDelivery",
                schema: "public",
                table: "CliTransfusionReaction",
                column: "NoticeDelivery");

            migrationBuilder.CreateIndex(
                name: "IX_CliTransfusionReaction_RecordedByUserId",
                schema: "public",
                table: "CliTransfusionReaction",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliWsdDrain_EncounterId",
                schema: "public",
                table: "CliWsdDrain",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliWsdDrain_InpEpisodeId_DrainLabel",
                schema: "public",
                table: "CliWsdDrain",
                columns: new[] { "InpEpisodeId", "DrainLabel" },
                unique: true,
                filter: "\"Status\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CliWsdDrain_PatientId",
                schema: "public",
                table: "CliWsdDrain",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CliWsdDrain_RegisteredByUserId",
                schema: "public",
                table: "CliWsdDrain",
                column: "RegisteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliWsdDrain_RemovedByUserId",
                schema: "public",
                table: "CliWsdDrain",
                column: "RemovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliWsdDrain_Status",
                schema: "public",
                table: "CliWsdDrain",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CliWsdReading_FluidBalanceEntryId",
                schema: "public",
                table: "CliWsdReading",
                column: "FluidBalanceEntryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliWsdReading_InpEpisodeId",
                schema: "public",
                table: "CliWsdReading",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CliWsdReading_RecordedByUserId",
                schema: "public",
                table: "CliWsdReading",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliWsdReading_ShiftId",
                schema: "public",
                table: "CliWsdReading",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_CliWsdReading_WsdDrainId_PeriodEndAt",
                schema: "public",
                table: "CliWsdReading",
                columns: new[] { "WsdDrainId", "PeriodEndAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_GziPatientDiet_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "GziPatientDiet",
                column: "InstructionVerifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GziPatientDiet_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "GziPatientDiet");

            migrationBuilder.DropTable(
                name: "BbkTransfusionReactionNotice",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliEquipmentUsageRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliSurgicalSiteSurveillanceEntryRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliTransfusionMonitoringPoint",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliTransfusionReaction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliWsdReading",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliEquipmentUsage",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliSurgicalSiteSurveillanceEntry",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliTransfusionMonitoring",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliWsdDrain",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliSurgicalSiteSurveillance",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_GziPatientDiet_InstructionVerificationStatus",
                schema: "public",
                table: "GziPatientDiet");

            migrationBuilder.DropIndex(
                name: "IX_GziPatientDiet_InstructionVerifiedByUserId",
                schema: "public",
                table: "GziPatientDiet");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CliFluidBalanceEntry_Volume",
                schema: "public",
                table: "CliFluidBalanceEntry");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "GziPatientDiet");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "GziPatientDiet");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "GziPatientDiet");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CliFluidBalanceEntry_Volume",
                schema: "public",
                table: "CliFluidBalanceEntry",
                sql: "\"VolumeMl\" > 0 AND \"VolumeMl\" <= 10000");
        }
    }
}
