using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddHemodialysisManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HmdChecklistItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    IsOverridable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    OverridableDecisionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OverridableDecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OverridableDecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckSequence = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_HmdChecklistItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HmdEpisode",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    DpjpDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EpisodeStatus = table.Column<int>(type: "integer", nullable: false),
                    SuspendReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ClosureReason = table.Column<int>(type: "integer", nullable: true),
                    ClosureNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_HmdEpisode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdEpisode_MstDoctor_DpjpDoctorId",
                        column: x => x.DpjpDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdEpisode_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdEpisode_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdMachine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MachineName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MachineStatus = table.Column<int>(type: "integer", nullable: false),
                    DedicatedFor = table.Column<int>(type: "integer", nullable: false),
                    IsSchedulable = table.Column<bool>(type: "boolean", nullable: false),
                    LastStatusChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_HmdMachine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdMachine_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdReadinessItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresResultDate = table.Column<bool>(type: "boolean", nullable: false),
                    CheckSequence = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_HmdReadinessItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HmdSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxPatientsPerNurse = table.Column<int>(type: "integer", nullable: false),
                    EnforceNurseRatio = table.Column<bool>(type: "boolean", nullable: false),
                    WaterResultValidityHours = table.Column<int>(type: "integer", nullable: false),
                    EnforceCompetencyCheck = table.Column<bool>(type: "boolean", nullable: false),
                    AllowMultipleActiveEpisodePerPatient = table.Column<bool>(type: "boolean", nullable: false),
                    SessionStartGraceMinutes = table.Column<int>(type: "integer", nullable: false),
                    RequireDifferentSigner = table.Column<bool>(type: "boolean", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_HmdSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdSetting_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSetting_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdStation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StationCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    StationName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    StationStatus = table.Column<int>(type: "integer", nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LastStatusChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsIsolationStation = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_HmdStation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdStation_MstRoom_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "MstRoom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdStation_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdUnitReadiness",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Shift = table.Column<int>(type: "integer", nullable: false),
                    ReadinessStatus = table.Column<int>(type: "integer", nullable: false),
                    DeclaredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeclaredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NotReadyReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_HmdUnitReadiness", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdUnitReadiness_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdEligibilityAssessment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessedByDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    IndicationSummary = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DecisionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FollowUpInstruction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_HmdEligibilityAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdEligibilityAssessment_HmdEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "HmdEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdEligibilityAssessment_MstDoctor_AssessedByDoctorId",
                        column: x => x.AssessedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdOrder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestingDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ClinicalReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RequestedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OrderStatus = table.Column<int>(type: "integer", nullable: false),
                    StatusBeforeHold = table.Column<int>(type: "integer", nullable: true),
                    DecisionByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecisionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_HmdOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdOrder_HmdEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "HmdEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdOrder_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdOrder_MstDoctor_RequestingDoctorId",
                        column: x => x.RequestingDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdOrder_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdOrder_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdSerologyReview",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TestType = table.Column<int>(type: "integer", nullable: false),
                    LabExaminationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResultDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ResultFlag = table.Column<int>(type: "integer", nullable: false),
                    ResultSummary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReviewStatus = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_HmdSerologyReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdSerologyReview_HmdEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "HmdEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdVascularAccess",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessType = table.Column<int>(type: "integer", nullable: false),
                    AccessSite = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccessStatus = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    EstablishedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LastAssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConditionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_HmdVascularAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdVascularAccess_HmdEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "HmdEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdMachineStatusHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_HmdMachineStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdMachineStatusHistory_HmdMachine_MachineId",
                        column: x => x.MachineId,
                        principalSchema: "public",
                        principalTable: "HmdMachine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdUnitReadinessDetail",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UnitReadinessId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadinessItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    ResultDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_HmdUnitReadinessDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdUnitReadinessDetail_HmdReadinessItem_ReadinessItemId",
                        column: x => x.ReadinessItemId,
                        principalSchema: "public",
                        principalTable: "HmdReadinessItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdUnitReadinessDetail_HmdUnitReadiness_UnitReadinessId",
                        column: x => x.UnitReadinessId,
                        principalSchema: "public",
                        principalTable: "HmdUnitReadiness",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdIsolationDecision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Requirement = table.Column<int>(type: "integer", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SerologyReviewId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_HmdIsolationDecision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdIsolationDecision_HmdEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "HmdEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdIsolationDecision_HmdSerologyReview_SerologyReviewId",
                        column: x => x.SerologyReviewId,
                        principalSchema: "public",
                        principalTable: "HmdSerologyReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdPrescription",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescribingDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FrequencyPerWeek = table.Column<int>(type: "integer", nullable: false),
                    TargetDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    TargetUltrafiltrationMl = table.Column<int>(type: "integer", nullable: true),
                    BloodFlowRate = table.Column<int>(type: "integer", nullable: true),
                    DialysateFlowRate = table.Column<int>(type: "integer", nullable: true),
                    DialyzerType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DialysateComposition = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SodiumBicarbonateProfile = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DialysateTemperatureC = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    AnticoagulantPlan = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VascularAccessId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicalNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PrescriptionStatus = table.Column<int>(type: "integer", nullable: false),
                    SupersededByPrescriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActivatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_HmdPrescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdPrescription_HmdEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "HmdEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdPrescription_HmdVascularAccess_VascularAccessId",
                        column: x => x.VascularAccessId,
                        principalSchema: "public",
                        principalTable: "HmdVascularAccess",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdPrescription_MstDoctor_PrescribingDoctorId",
                        column: x => x.PrescribingDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdPrescription_SupersededByPrescription",
                        column: x => x.SupersededByPrescriptionId,
                        principalSchema: "public",
                        principalTable: "HmdPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdSession",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsibleDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Shift = table.Column<int>(type: "integer", nullable: false),
                    ScheduledStartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledEndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CheckedInByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReadyAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadyByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    ActualUltrafiltrationMl = table.Column<int>(type: "integer", nullable: true),
                    SessionStatus = table.Column<int>(type: "integer", nullable: false),
                    HoldReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StopReason = table.Column<int>(type: "integer", nullable: true),
                    StopNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DeviationNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Disposition = table.Column<int>(type: "integer", nullable: true),
                    DispositionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReturnReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DocumentedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecordHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PatientProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BillingHandoffStatus = table.Column<int>(type: "integer", nullable: false),
                    BillingHandoffAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BillingHandoffError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_HmdSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdSession_HmdEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "HmdEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSession_HmdMachine_MachineId",
                        column: x => x.MachineId,
                        principalSchema: "public",
                        principalTable: "HmdMachine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSession_HmdOrder_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "public",
                        principalTable: "HmdOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSession_HmdPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "HmdPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSession_HmdStation_StationId",
                        column: x => x.StationId,
                        principalSchema: "public",
                        principalTable: "HmdStation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSession_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSession_MstDoctor_ResponsibleDoctorId",
                        column: x => x.ResponsibleDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSession_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSession_TrxPatientProcedure_PatientProcedureId",
                        column: x => x.PatientProcedureId,
                        principalSchema: "public",
                        principalTable: "TrxPatientProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdSessionAssessment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Phase = table.Column<int>(type: "integer", nullable: false),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AssessedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BodyWeightKg = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    PatientVitalSignId = table.Column<Guid>(type: "uuid", nullable: true),
                    Complaint = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AccessConditionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PatientCondition = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TargetAchieved = table.Column<bool>(type: "boolean", nullable: true),
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
                    table.PrimaryKey("PK_HmdSessionAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdSessionAssessment_HmdSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "HmdSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdSessionChecklist",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChecklistItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsOverridden = table.Column<bool>(type: "boolean", nullable: false),
                    OverrideReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    OverriddenByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    OverriddenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_HmdSessionChecklist", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdSessionChecklist_HmdChecklistItem_ChecklistItemId",
                        column: x => x.ChecklistItemId,
                        principalSchema: "public",
                        principalTable: "HmdChecklistItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSessionChecklist_HmdSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "HmdSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdSessionComplication",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplicationType = table.Column<int>(type: "integer", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DetectedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SignsAndSymptoms = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Intervention = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ClinicianInstruction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Outcome = table.Column<int>(type: "integer", nullable: false),
                    SessionImpact = table.Column<int>(type: "integer", nullable: false),
                    TransferDestination = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_HmdSessionComplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdSessionComplication_HmdSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "HmdSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdSessionMedication",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    Dose = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    DoseUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Route = table.Column<int>(type: "integer", nullable: false),
                    InstructedByDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdministeredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdministeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DrugUsageId = table.Column<Guid>(type: "uuid", nullable: true),
                    HandoffStatus = table.Column<int>(type: "integer", nullable: false),
                    PharmacyStorageLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PharmacyMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    PharmacyQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    HandoffAttemptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HandoffError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_HmdSessionMedication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdSessionMedication_HmdSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "HmdSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSessionMedication_MstDoctor_InstructedByDoctorId",
                        column: x => x.InstructedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSessionMedication_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdSessionObservation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    ObservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SystolicBp = table.Column<int>(type: "integer", nullable: true),
                    DiastolicBp = table.Column<int>(type: "integer", nullable: true),
                    PulseRate = table.Column<int>(type: "integer", nullable: true),
                    RespiratoryRate = table.Column<int>(type: "integer", nullable: true),
                    TemperatureC = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    OxygenSaturation = table.Column<int>(type: "integer", nullable: true),
                    BloodFlowRate = table.Column<int>(type: "integer", nullable: true),
                    DialysateFlowRate = table.Column<int>(type: "integer", nullable: true),
                    TransmembranePressure = table.Column<int>(type: "integer", nullable: true),
                    VenousPressure = table.Column<int>(type: "integer", nullable: true),
                    ArterialPressure = table.Column<int>(type: "integer", nullable: true),
                    UltrafiltrationVolumeMl = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_HmdSessionObservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdSessionObservation_HmdSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "HmdSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HmdSessionStaffAssignment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffRole = table.Column<int>(type: "integer", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompetencyVerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    CompetencyCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompetencySourceReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_HmdSessionStaffAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HmdSessionStaffAssignment_HmdSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "HmdSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HmdSessionStaffAssignment_MstWorkforceProfile_WorkforceProf~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HmdChecklistItem_Category",
                schema: "public",
                table: "HmdChecklistItem",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_HmdChecklistItem_IsActive",
                schema: "public",
                table: "HmdChecklistItem",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HmdChecklistItem_IsMandatory",
                schema: "public",
                table: "HmdChecklistItem",
                column: "IsMandatory");

            migrationBuilder.CreateIndex(
                name: "IX_HmdChecklistItem_IsOverridable",
                schema: "public",
                table: "HmdChecklistItem",
                column: "IsOverridable");

            migrationBuilder.CreateIndex(
                name: "IX_HmdChecklistItem_ItemCode",
                schema: "public",
                table: "HmdChecklistItem",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdEligibilityAssessment_AssessedAt",
                schema: "public",
                table: "HmdEligibilityAssessment",
                column: "AssessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HmdEligibilityAssessment_AssessedByDoctorId",
                schema: "public",
                table: "HmdEligibilityAssessment",
                column: "AssessedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdEligibilityAssessment_EpisodeId",
                schema: "public",
                table: "HmdEligibilityAssessment",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdEligibilityAssessment_Outcome",
                schema: "public",
                table: "HmdEligibilityAssessment",
                column: "Outcome");

            migrationBuilder.CreateIndex(
                name: "IX_HmdEpisode_DpjpDoctorId",
                schema: "public",
                table: "HmdEpisode",
                column: "DpjpDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdEpisode_EpisodeNumber",
                schema: "public",
                table: "HmdEpisode",
                column: "EpisodeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdEpisode_EpisodeStatus",
                schema: "public",
                table: "HmdEpisode",
                column: "EpisodeStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdEpisode_PatientId_Active",
                schema: "public",
                table: "HmdEpisode",
                column: "PatientId",
                unique: true,
                filter: "\"EpisodeStatus\" = 2 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HmdEpisode_PatientId_EpisodeStatus",
                schema: "public",
                table: "HmdEpisode",
                columns: new[] { "PatientId", "EpisodeStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_HmdEpisode_ServiceUnitId",
                schema: "public",
                table: "HmdEpisode",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdEpisode_StartDate",
                schema: "public",
                table: "HmdEpisode",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_HmdIsolationDecision_DecidedByUserId",
                schema: "public",
                table: "HmdIsolationDecision",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdIsolationDecision_EffectiveFrom",
                schema: "public",
                table: "HmdIsolationDecision",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_HmdIsolationDecision_EpisodeId",
                schema: "public",
                table: "HmdIsolationDecision",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdIsolationDecision_IsActive",
                schema: "public",
                table: "HmdIsolationDecision",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HmdIsolationDecision_Requirement",
                schema: "public",
                table: "HmdIsolationDecision",
                column: "Requirement");

            migrationBuilder.CreateIndex(
                name: "IX_HmdIsolationDecision_SerologyReviewId",
                schema: "public",
                table: "HmdIsolationDecision",
                column: "SerologyReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdMachine_DedicatedFor",
                schema: "public",
                table: "HmdMachine",
                column: "DedicatedFor");

            migrationBuilder.CreateIndex(
                name: "IX_HmdMachine_IsActive",
                schema: "public",
                table: "HmdMachine",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HmdMachine_IsSchedulable",
                schema: "public",
                table: "HmdMachine",
                column: "IsSchedulable");

            migrationBuilder.CreateIndex(
                name: "IX_HmdMachine_MachineStatus",
                schema: "public",
                table: "HmdMachine",
                column: "MachineStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdMachine_ServiceUnitId_MachineCode",
                schema: "public",
                table: "HmdMachine",
                columns: new[] { "ServiceUnitId", "MachineCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdMachineStatusHistory_ChangedByUserId",
                schema: "public",
                table: "HmdMachineStatusHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdMachineStatusHistory_MachineId_ChangedAt",
                schema: "public",
                table: "HmdMachineStatusHistory",
                columns: new[] { "MachineId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HmdMachineStatusHistory_ToStatus",
                schema: "public",
                table: "HmdMachineStatusHistory",
                column: "ToStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdOrder_EncounterId",
                schema: "public",
                table: "HmdOrder",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdOrder_EpisodeId",
                schema: "public",
                table: "HmdOrder",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdOrder_InpEpisodeId",
                schema: "public",
                table: "HmdOrder",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdOrder_OrderNumber",
                schema: "public",
                table: "HmdOrder",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdOrder_OrderStatus",
                schema: "public",
                table: "HmdOrder",
                column: "OrderStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdOrder_PatientId_OrderStatus",
                schema: "public",
                table: "HmdOrder",
                columns: new[] { "PatientId", "OrderStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_HmdOrder_Priority",
                schema: "public",
                table: "HmdOrder",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_HmdOrder_RequestedAt",
                schema: "public",
                table: "HmdOrder",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HmdOrder_RequestedByUserId",
                schema: "public",
                table: "HmdOrder",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdOrder_RequestedDate_Priority",
                schema: "public",
                table: "HmdOrder",
                columns: new[] { "RequestedDate", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_HmdOrder_RequestingDoctorId",
                schema: "public",
                table: "HmdOrder",
                column: "RequestingDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdPrescription_EffectiveDate",
                schema: "public",
                table: "HmdPrescription",
                column: "EffectiveDate");

            migrationBuilder.CreateIndex(
                name: "IX_HmdPrescription_EpisodeId_Active",
                schema: "public",
                table: "HmdPrescription",
                column: "EpisodeId",
                unique: true,
                filter: "\"PrescriptionStatus\" = 2 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HmdPrescription_EpisodeId_PrescriptionStatus",
                schema: "public",
                table: "HmdPrescription",
                columns: new[] { "EpisodeId", "PrescriptionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_HmdPrescription_PrescribingDoctorId",
                schema: "public",
                table: "HmdPrescription",
                column: "PrescribingDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdPrescription_SupersededByPrescriptionId",
                schema: "public",
                table: "HmdPrescription",
                column: "SupersededByPrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdPrescription_VascularAccessId",
                schema: "public",
                table: "HmdPrescription",
                column: "VascularAccessId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdReadinessItem_Category",
                schema: "public",
                table: "HmdReadinessItem",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_HmdReadinessItem_IsActive",
                schema: "public",
                table: "HmdReadinessItem",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HmdReadinessItem_IsMandatory",
                schema: "public",
                table: "HmdReadinessItem",
                column: "IsMandatory");

            migrationBuilder.CreateIndex(
                name: "IX_HmdReadinessItem_ItemCode",
                schema: "public",
                table: "HmdReadinessItem",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdSerologyReview_EpisodeId",
                schema: "public",
                table: "HmdSerologyReview",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSerologyReview_LabExaminationId",
                schema: "public",
                table: "HmdSerologyReview",
                column: "LabExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSerologyReview_ResultDate",
                schema: "public",
                table: "HmdSerologyReview",
                column: "ResultDate");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSerologyReview_ResultFlag",
                schema: "public",
                table: "HmdSerologyReview",
                column: "ResultFlag");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSerologyReview_ReviewStatus",
                schema: "public",
                table: "HmdSerologyReview",
                column: "ReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSerologyReview_TestType",
                schema: "public",
                table: "HmdSerologyReview",
                column: "TestType");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_BillingHandoffStatus",
                schema: "public",
                table: "HmdSession",
                column: "BillingHandoffStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_EncounterId",
                schema: "public",
                table: "HmdSession",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_Episode_Window",
                schema: "public",
                table: "HmdSession",
                columns: new[] { "EpisodeId", "ScheduledStartAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_IdempotencyKey",
                schema: "public",
                table: "HmdSession",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_InpEpisodeId",
                schema: "public",
                table: "HmdSession",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_Machine_Window",
                schema: "public",
                table: "HmdSession",
                columns: new[] { "MachineId", "ScheduledStartAt", "ScheduledEndAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_OrderId",
                schema: "public",
                table: "HmdSession",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_PatientProcedureId",
                schema: "public",
                table: "HmdSession",
                column: "PatientProcedureId",
                unique: true,
                filter: "\"PatientProcedureId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_PrescriptionId",
                schema: "public",
                table: "HmdSession",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_ResponsibleDoctorId",
                schema: "public",
                table: "HmdSession",
                column: "ResponsibleDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_ScheduledDate_Shift",
                schema: "public",
                table: "HmdSession",
                columns: new[] { "ScheduledDate", "Shift" });

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_SessionNumber",
                schema: "public",
                table: "HmdSession",
                column: "SessionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_SessionStatus",
                schema: "public",
                table: "HmdSession",
                column: "SessionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_StartedAt",
                schema: "public",
                table: "HmdSession",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_Station_Window",
                schema: "public",
                table: "HmdSession",
                columns: new[] { "StationId", "ScheduledStartAt", "ScheduledEndAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HmdSession_StopReason",
                schema: "public",
                table: "HmdSession",
                column: "StopReason");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionAssessment_AssessedByUserId",
                schema: "public",
                table: "HmdSessionAssessment",
                column: "AssessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionAssessment_PatientVitalSignId",
                schema: "public",
                table: "HmdSessionAssessment",
                column: "PatientVitalSignId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionAssessment_SessionId_Phase",
                schema: "public",
                table: "HmdSessionAssessment",
                columns: new[] { "SessionId", "Phase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionChecklist_ChecklistItemId",
                schema: "public",
                table: "HmdSessionChecklist",
                column: "ChecklistItemId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionChecklist_IsOverridden",
                schema: "public",
                table: "HmdSessionChecklist",
                column: "IsOverridden");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionChecklist_Result",
                schema: "public",
                table: "HmdSessionChecklist",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionChecklist_SessionId_ChecklistItemId",
                schema: "public",
                table: "HmdSessionChecklist",
                columns: new[] { "SessionId", "ChecklistItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionComplication_ComplicationType",
                schema: "public",
                table: "HmdSessionComplication",
                column: "ComplicationType");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionComplication_DetectedAt",
                schema: "public",
                table: "HmdSessionComplication",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionComplication_DetectedByUserId",
                schema: "public",
                table: "HmdSessionComplication",
                column: "DetectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionComplication_Outcome",
                schema: "public",
                table: "HmdSessionComplication",
                column: "Outcome");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionComplication_SessionId",
                schema: "public",
                table: "HmdSessionComplication",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionMedication_AdministeredAt",
                schema: "public",
                table: "HmdSessionMedication",
                column: "AdministeredAt");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionMedication_AdministeredByUserId",
                schema: "public",
                table: "HmdSessionMedication",
                column: "AdministeredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionMedication_DrugId",
                schema: "public",
                table: "HmdSessionMedication",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionMedication_DrugUsageId",
                schema: "public",
                table: "HmdSessionMedication",
                column: "DrugUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionMedication_HandoffStatus",
                schema: "public",
                table: "HmdSessionMedication",
                column: "HandoffStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionMedication_InstructedByDoctorId",
                schema: "public",
                table: "HmdSessionMedication",
                column: "InstructedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionMedication_SessionId",
                schema: "public",
                table: "HmdSessionMedication",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionObservation_RecordedByUserId",
                schema: "public",
                table: "HmdSessionObservation",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionObservation_SessionId_ObservedAt",
                schema: "public",
                table: "HmdSessionObservation",
                columns: new[] { "SessionId", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionObservation_SessionId_SequenceNumber",
                schema: "public",
                table: "HmdSessionObservation",
                columns: new[] { "SessionId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionStaffAssignment_CompetencyVerificationStatus",
                schema: "public",
                table: "HmdSessionStaffAssignment",
                column: "CompetencyVerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionStaffAssignment_SessionId_WorkforceProfileId",
                schema: "public",
                table: "HmdSessionStaffAssignment",
                columns: new[] { "SessionId", "WorkforceProfileId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionStaffAssignment_StaffRole",
                schema: "public",
                table: "HmdSessionStaffAssignment",
                column: "StaffRole");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSessionStaffAssignment_WorkforceProfileId",
                schema: "public",
                table: "HmdSessionStaffAssignment",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSetting_ProcedureId",
                schema: "public",
                table: "HmdSetting",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdSetting_ServiceUnitId",
                schema: "public",
                table: "HmdSetting",
                column: "ServiceUnitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdStation_IsActive",
                schema: "public",
                table: "HmdStation",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_HmdStation_IsIsolationStation",
                schema: "public",
                table: "HmdStation",
                column: "IsIsolationStation");

            migrationBuilder.CreateIndex(
                name: "IX_HmdStation_RoomId",
                schema: "public",
                table: "HmdStation",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdStation_ServiceUnitId_StationCode",
                schema: "public",
                table: "HmdStation",
                columns: new[] { "ServiceUnitId", "StationCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdStation_StationStatus",
                schema: "public",
                table: "HmdStation",
                column: "StationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdUnitReadiness_DeclaredByUserId",
                schema: "public",
                table: "HmdUnitReadiness",
                column: "DeclaredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdUnitReadiness_ReadinessStatus",
                schema: "public",
                table: "HmdUnitReadiness",
                column: "ReadinessStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdUnitReadiness_ServiceUnitId_ReadinessDate_Shift",
                schema: "public",
                table: "HmdUnitReadiness",
                columns: new[] { "ServiceUnitId", "ReadinessDate", "Shift" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_HmdUnitReadinessDetail_ReadinessItemId",
                schema: "public",
                table: "HmdUnitReadinessDetail",
                column: "ReadinessItemId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdUnitReadinessDetail_Result",
                schema: "public",
                table: "HmdUnitReadinessDetail",
                column: "Result");

            migrationBuilder.CreateIndex(
                name: "IX_HmdUnitReadinessDetail_UnitReadinessId_ReadinessItemId",
                schema: "public",
                table: "HmdUnitReadinessDetail",
                columns: new[] { "UnitReadinessId", "ReadinessItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HmdVascularAccess_AccessStatus",
                schema: "public",
                table: "HmdVascularAccess",
                column: "AccessStatus");

            migrationBuilder.CreateIndex(
                name: "IX_HmdVascularAccess_AccessType",
                schema: "public",
                table: "HmdVascularAccess",
                column: "AccessType");

            migrationBuilder.CreateIndex(
                name: "IX_HmdVascularAccess_EpisodeId",
                schema: "public",
                table: "HmdVascularAccess",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_HmdVascularAccess_IsPrimary",
                schema: "public",
                table: "HmdVascularAccess",
                column: "IsPrimary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HmdEligibilityAssessment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdIsolationDecision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdMachineStatusHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdSessionAssessment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdSessionChecklist",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdSessionComplication",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdSessionMedication",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdSessionObservation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdSessionStaffAssignment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdUnitReadinessDetail",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdSerologyReview",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdChecklistItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdSession",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdReadinessItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdUnitReadiness",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdMachine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdOrder",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdPrescription",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdStation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdVascularAccess",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HmdEpisode",
                schema: "public");
        }
    }
}
