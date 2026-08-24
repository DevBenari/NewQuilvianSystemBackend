using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddOperatingRoomFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OprCase",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimarySurgeonId = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseType = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Outcome = table.Column<int>(type: "integer", nullable: true),
                    Indication = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Laterality = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "integer", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreferredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_OprCase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprCase_MstDoctor_PrimarySurgeonId",
                        column: x => x.PrimarySurgeonId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OprCase_MstDoctor_RequesterDoctorId",
                        column: x => x.RequesterDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OprCase_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OprCase_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprAnesthesiaRecord",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssessmentSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Technique = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    MedicationFluidSummary = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    AirwaySummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    MonitoringSummary = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    EventSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    FinalCondition = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    FinalizedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_OprAnesthesiaRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprAnesthesiaRecord_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprCaseProcedure",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_OprCaseProcedure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprCaseProcedure_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OprCaseProcedure_TrxPatientProcedure_PatientProcedureId",
                        column: x => x.PatientProcedureId,
                        principalSchema: "public",
                        principalTable: "TrxPatientProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprExecutionRecord",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PreDiagnosis = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    PostDiagnosis = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Findings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    Technique = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    Complications = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    BloodLossMl = table.Column<decimal>(type: "numeric", nullable: true),
                    SpecimenNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ImplantDrainNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PostPlan = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalizedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_OprExecutionRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprExecutionRecord_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprHandover",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ConditionSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    DeviceTherapySummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RiskSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    InstructionSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    SentBy = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Revision = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_OprHandover", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprHandover_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprIntegrationDelivery",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Destination = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MessageType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PayloadReference = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastErrorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AcceptedReference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
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
                    table.PrimaryKey("PK_OprIntegrationDelivery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprIntegrationDelivery_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprMaterialUsage",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemType = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    CorrectionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_OprMaterialUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprMaterialUsage_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprRecovery",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ScoreSystem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ScoreValue = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ObservationJson = table.Column<string>(type: "jsonb", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    DecisionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReleasedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_OprRecovery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprRecovery_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprSafetyChecklist",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phase = table.Column<int>(type: "integer", nullable: false),
                    TemplateVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ItemsJson = table.Column<string>(type: "jsonb", nullable: false),
                    SignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsEmergencyBypass = table.Column<bool>(type: "boolean", nullable: false),
                    BypassReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BypassResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAfterStableAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_OprSafetyChecklist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprSafetyChecklist_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprSchedule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BufferBeforeMinutes = table.Column<int>(type: "integer", nullable: false),
                    BufferAfterMinutes = table.Column<int>(type: "integer", nullable: false),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_OprSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprSchedule_MstRoom_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "MstRoom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OprSchedule_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprStatusHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_OprStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprStatusHistory_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprExecutionAddendum",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutionRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AuthoredBy = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_OprExecutionAddendum", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprExecutionAddendum_OprExecutionRecord_ExecutionRecordId",
                        column: x => x.ExecutionRecordId,
                        principalSchema: "public",
                        principalTable: "OprExecutionRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprTeamMember",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsLead = table.Column<bool>(type: "boolean", nullable: false),
                    CredentialCheckStatus = table.Column<int>(type: "integer", nullable: false),
                    CredentialCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_OprTeamMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprTeamMember_MstWorkforceProfile_WorkforceId",
                        column: x => x.WorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OprTeamMember_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OprTeamMember_OprSchedule_ScheduleId",
                        column: x => x.ScheduleId,
                        principalSchema: "public",
                        principalTable: "OprSchedule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OprAnesthesiaRecord_OprCaseId",
                schema: "public",
                table: "OprAnesthesiaRecord",
                column: "OprCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprCase_CaseNumber",
                schema: "public",
                table: "OprCase",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprCase_EncounterId_Status",
                schema: "public",
                table: "OprCase",
                columns: new[] { "EncounterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OprCase_PatientId_RequestedAt",
                schema: "public",
                table: "OprCase",
                columns: new[] { "PatientId", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OprCase_PrimarySurgeonId",
                schema: "public",
                table: "OprCase",
                column: "PrimarySurgeonId");

            migrationBuilder.CreateIndex(
                name: "IX_OprCase_RequesterDoctorId",
                schema: "public",
                table: "OprCase",
                column: "RequesterDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_OprCaseProcedure_OprCaseId",
                schema: "public",
                table: "OprCaseProcedure",
                column: "OprCaseId",
                unique: true,
                filter: "\"IsPrimary\" = TRUE AND \"IsDelete\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_OprCaseProcedure_OprCaseId_Sequence",
                schema: "public",
                table: "OprCaseProcedure",
                columns: new[] { "OprCaseId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprCaseProcedure_PatientProcedureId",
                schema: "public",
                table: "OprCaseProcedure",
                column: "PatientProcedureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprExecutionAddendum_ExecutionRecordId_AuthoredAt",
                schema: "public",
                table: "OprExecutionAddendum",
                columns: new[] { "ExecutionRecordId", "AuthoredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OprExecutionRecord_OprCaseId",
                schema: "public",
                table: "OprExecutionRecord",
                column: "OprCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprHandover_DestinationUnitId_Status",
                schema: "public",
                table: "OprHandover",
                columns: new[] { "DestinationUnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OprHandover_OprCaseId_Revision",
                schema: "public",
                table: "OprHandover",
                columns: new[] { "OprCaseId", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprIntegrationDelivery_Destination_IdempotencyKey",
                schema: "public",
                table: "OprIntegrationDelivery",
                columns: new[] { "Destination", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprIntegrationDelivery_OprCaseId",
                schema: "public",
                table: "OprIntegrationDelivery",
                column: "OprCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_OprIntegrationDelivery_Status_RetryCount",
                schema: "public",
                table: "OprIntegrationDelivery",
                columns: new[] { "Status", "RetryCount" });

            migrationBuilder.CreateIndex(
                name: "IX_OprMaterialUsage_BatchNumber_SerialNumber",
                schema: "public",
                table: "OprMaterialUsage",
                columns: new[] { "BatchNumber", "SerialNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_OprMaterialUsage_OprCaseId_ExternalItemId",
                schema: "public",
                table: "OprMaterialUsage",
                columns: new[] { "OprCaseId", "ExternalItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_OprMaterialUsage_OprCaseId_Id_Revision",
                schema: "public",
                table: "OprMaterialUsage",
                columns: new[] { "OprCaseId", "Id", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprRecovery_OprCaseId",
                schema: "public",
                table: "OprRecovery",
                column: "OprCaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprSafetyChecklist_OprCaseId_Phase_Revision",
                schema: "public",
                table: "OprSafetyChecklist",
                columns: new[] { "OprCaseId", "Phase", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprSchedule_OprCaseId",
                schema: "public",
                table: "OprSchedule",
                column: "OprCaseId",
                unique: true,
                filter: "\"IsCurrent\" = TRUE AND \"IsDelete\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_OprSchedule_OprCaseId_Revision",
                schema: "public",
                table: "OprSchedule",
                columns: new[] { "OprCaseId", "Revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprSchedule_RoomId_StartAt_EndAt_IsCurrent",
                schema: "public",
                table: "OprSchedule",
                columns: new[] { "RoomId", "StartAt", "EndAt", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_OprStatusHistory_OprCaseId_OccurredAt",
                schema: "public",
                table: "OprStatusHistory",
                columns: new[] { "OprCaseId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OprTeamMember_OprCaseId",
                schema: "public",
                table: "OprTeamMember",
                column: "OprCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_OprTeamMember_ScheduleId_WorkforceId_Role",
                schema: "public",
                table: "OprTeamMember",
                columns: new[] { "ScheduleId", "WorkforceId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprTeamMember_WorkforceId_IsCurrent",
                schema: "public",
                table: "OprTeamMember",
                columns: new[] { "WorkforceId", "IsCurrent" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OprAnesthesiaRecord",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprCaseProcedure",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprExecutionAddendum",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprHandover",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprIntegrationDelivery",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprMaterialUsage",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprRecovery",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprSafetyChecklist",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprStatusHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprTeamMember",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprExecutionRecord",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprSchedule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprCase",
                schema: "public");
        }
    }
}
