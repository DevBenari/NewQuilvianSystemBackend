using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRehabilitationManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RhbDiscipline",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisciplineCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DisciplineName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProfessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_RhbDiscipline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbDiscipline_MstProfession_ProfessionId",
                        column: x => x.ProfessionId,
                        principalSchema: "public",
                        principalTable: "MstProfession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceEncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    InternalReferralLetterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalInstitutionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalReferralNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RequestingDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestingPractitionerName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IndicationText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false),
                    DuplicateOverrideReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReturnReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DispositionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RedirectTargetText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DispositionByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DispositionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_RhbRequest", x => x.Id);
                    table.CheckConstraint("CK_RhbRequest_Source", "(\"SourceType\" = 4) = (\"SourceEncounterId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_RhbRequest_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbRequest_MstDoctor_RequestingDoctorId",
                        column: x => x.RequestingDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbRequest_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbRequest_RegPatientEncounter_SourceEncounterId",
                        column: x => x.SourceEncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbStatusHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Action = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ReasonText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_RhbStatusHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RhbDisciplineInstrument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstrumentKind = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_RhbDisciplineInstrument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbDisciplineInstrument_RhbDiscipline_DisciplineId",
                        column: x => x.DisciplineId,
                        principalSchema: "public",
                        principalTable: "RhbDiscipline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbDisciplineProcedure",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_RhbDisciplineProcedure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbDisciplineProcedure_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbDisciplineProcedure_RhbDiscipline_DisciplineId",
                        column: x => x.DisciplineId,
                        principalSchema: "public",
                        principalTable: "RhbDiscipline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbEpisode",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponsibleDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeStatus = table.Column<int>(type: "integer", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosureReason = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_RhbEpisode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbEpisode_MstDoctor_ResponsibleDoctorId",
                        column: x => x.ResponsibleDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbEpisode_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbEpisode_RhbRequest_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "public",
                        principalTable: "RhbRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbRequestDiscipline",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_RhbRequestDiscipline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbRequestDiscipline_RhbDiscipline_DisciplineId",
                        column: x => x.DisciplineId,
                        principalSchema: "public",
                        principalTable: "RhbDiscipline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbRequestDiscipline_RhbRequest_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "public",
                        principalTable: "RhbRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbAssessment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentType = table.Column<int>(type: "integer", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HistoryText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    PhysicalFunctionalFindingsText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RiskAndContraindicationText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ProblemOrDiagnosisText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PrognosisText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ConclusionText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PatientGoalText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RecordStatus = table.Column<int>(type: "integer", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_RhbAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbAssessment_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbAssessment_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbAssessment_RhbDiscipline_DisciplineId",
                        column: x => x.DisciplineId,
                        principalSchema: "public",
                        principalTable: "RhbDiscipline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbAssessment_RhbEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "RhbEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbDischargeSummary",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClosureReason = table.Column<int>(type: "integer", nullable: true),
                    MainServicesText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    GoalAchievementText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FinalConditionText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FollowUpType = table.Column<int>(type: "integer", nullable: false),
                    FollowUpInstructionText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FollowUpTargetText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FollowUpDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordStatus = table.Column<int>(type: "integer", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_RhbDischargeSummary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbDischargeSummary_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbDischargeSummary_RhbEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "RhbEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbGoal",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uuid", nullable: true),
                    Term = table.Column<int>(type: "integer", nullable: false),
                    DescriptionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    BaselineText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TargetText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TargetDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GoalStatus = table.Column<int>(type: "integer", nullable: false),
                    SupersedesGoalId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgreedWithPatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_RhbGoal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbGoal_RhbDiscipline_DisciplineId",
                        column: x => x.DisciplineId,
                        principalSchema: "public",
                        principalTable: "RhbDiscipline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbGoal_RhbEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "RhbEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbGoal_RhbGoal_SupersedesGoalId",
                        column: x => x.SupersedesGoalId,
                        principalSchema: "public",
                        principalTable: "RhbGoal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbProgram",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramVersion = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SupersedesProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    AuthoredEncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    FrequencyText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PlannedSessionCount = table.Column<int>(type: "integer", nullable: true),
                    PrecautionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EvaluationDueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ProgramStatus = table.Column<int>(type: "integer", nullable: false),
                    NeedsReview = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ModificationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StoppedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StopReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RhbProgram", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbProgram_RegPatientEncounter_AuthoredEncounterId",
                        column: x => x.AuthoredEncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbProgram_RhbDiscipline_DisciplineId",
                        column: x => x.DisciplineId,
                        principalSchema: "public",
                        principalTable: "RhbDiscipline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbProgram_RhbEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "RhbEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbProgram_RhbProgram_SupersedesProgramId",
                        column: x => x.SupersedesProgramId,
                        principalSchema: "public",
                        principalTable: "RhbProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbEpisodeDiscipline",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescribedByAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicalPrecautionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_RhbEpisodeDiscipline", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbEpisodeDiscipline_RhbAssessment_PrescribedByAssessmentId",
                        column: x => x.PrescribedByAssessmentId,
                        principalSchema: "public",
                        principalTable: "RhbAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbEpisodeDiscipline_RhbDiscipline_DisciplineId",
                        column: x => x.DisciplineId,
                        principalSchema: "public",
                        principalTable: "RhbDiscipline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbEpisodeDiscipline_RhbEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "RhbEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbEducationRecord",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    RecipientName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RecipientRelation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MethodText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UnderstandingLevel = table.Column<int>(type: "integer", nullable: false),
                    BarrierText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ConsentStatus = table.Column<int>(type: "integer", nullable: false),
                    ConsentNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EducatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordStatus = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_RhbEducationRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbEducationRecord_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbEducationRecord_RhbEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "RhbEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbEducationRecord_RhbProgram_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "RhbProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbProgramGoal",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_RhbProgramGoal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbProgramGoal_RhbGoal_GoalId",
                        column: x => x.GoalId,
                        principalSchema: "public",
                        principalTable: "RhbGoal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbProgramGoal_RhbProgram_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "RhbProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbProgramIntervention",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParametersText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
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
                    table.PrimaryKey("PK_RhbProgramIntervention", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbProgramIntervention_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbProgramIntervention_RhbProgram_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "RhbProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbProgressReview",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DisciplineId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProgressSummaryText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    BarrierText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    InstrumentChangeNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Decision = table.Column<int>(type: "integer", nullable: true),
                    DecisionReasonText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReferralTargetText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RecordStatus = table.Column<int>(type: "integer", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_RhbProgressReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbProgressReview_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbProgressReview_RhbDiscipline_DisciplineId",
                        column: x => x.DisciplineId,
                        principalSchema: "public",
                        principalTable: "RhbDiscipline",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbProgressReview_RhbEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "RhbEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbProgressReview_RhbProgram_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "RhbProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbSession",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    TherapistUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionStatus = table.Column<int>(type: "integer", nullable: false),
                    NotDoneReason = table.Column<int>(type: "integer", nullable: true),
                    IdentityVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProgramVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CurrentConditionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ContraindicationFound = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ContraindicationText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResponseText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ImmediateOutcomeText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DeviationType = table.Column<int>(type: "integer", nullable: false),
                    DeviationReasonText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImmediateActionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ContactedPartyText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RecordStatus = table.Column<int>(type: "integer", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BillingHandoffStatus = table.Column<int>(type: "integer", nullable: false),
                    BillingHandoffAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BillingHandoffError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RhbSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbSession_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbSession_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbSession_RhbEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "RhbEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbSession_RhbProgram_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "RhbProgram",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbGoalReview",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgressReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementStatus = table.Column<int>(type: "integer", nullable: false),
                    NoteText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_RhbGoalReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RhbGoalReview_RhbGoal_GoalId",
                        column: x => x.GoalId,
                        principalSchema: "public",
                        principalTable: "RhbGoal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbGoalReview_RhbProgressReview_ProgressReviewId",
                        column: x => x.ProgressReviewId,
                        principalSchema: "public",
                        principalTable: "RhbProgressReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbMeasurement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProgressReviewId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    InstrumentVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstrumentKind = table.Column<int>(type: "integer", nullable: true),
                    InstrumentName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BodyPartOrDomain = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    AnswersJson = table.Column<string>(type: "jsonb", nullable: true),
                    NumericValue = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    ValueText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Unit = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ScoreBandCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MeasuredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_RhbMeasurement", x => x.Id);
                    table.CheckConstraint("CK_RhbMeasurement_Owner", "(\"AssessmentId\" IS NULL) <> (\"ProgressReviewId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_RhbMeasurement_CliClinicalInstrumentVersion_InstrumentVersi~",
                        column: x => x.InstrumentVersionId,
                        principalSchema: "public",
                        principalTable: "CliClinicalInstrumentVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbMeasurement_RhbAssessment_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "RhbAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbMeasurement_RhbProgressReview_ProgressReviewId",
                        column: x => x.ProgressReviewId,
                        principalSchema: "public",
                        principalTable: "RhbProgressReview",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RhbSessionIntervention",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramInterventionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActualParametersText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PatientProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_RhbSessionIntervention", x => x.Id);
                    table.CheckConstraint("CK_RhbSessionIntervention_Duration", "\"DurationMinutes\" IS NULL OR \"DurationMinutes\" BETWEEN 1 AND 600");
                    table.CheckConstraint("CK_RhbSessionIntervention_Quantity", "\"Quantity\" BETWEEN 1 AND 99");
                    table.ForeignKey(
                        name: "FK_RhbSessionIntervention_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbSessionIntervention_RhbProgramIntervention_ProgramInterv~",
                        column: x => x.ProgramInterventionId,
                        principalSchema: "public",
                        principalTable: "RhbProgramIntervention",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbSessionIntervention_RhbSession_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "RhbSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RhbSessionIntervention_TrxPatientProcedure_PatientProcedure~",
                        column: x => x.PatientProcedureId,
                        principalSchema: "public",
                        principalTable: "TrxPatientProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RhbAssessment_AuthorUserId",
                schema: "public",
                table: "RhbAssessment",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbAssessment_DisciplineId",
                schema: "public",
                table: "RhbAssessment",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbAssessment_EncounterId",
                schema: "public",
                table: "RhbAssessment",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbAssessment_EpisodeId_AssessmentType_DisciplineId",
                schema: "public",
                table: "RhbAssessment",
                columns: new[] { "EpisodeId", "AssessmentType", "DisciplineId" });

            migrationBuilder.CreateIndex(
                name: "IX_RhbAssessment_InpEpisodeId",
                schema: "public",
                table: "RhbAssessment",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbAssessment_RecordStatus",
                schema: "public",
                table: "RhbAssessment",
                column: "RecordStatus");

            migrationBuilder.CreateIndex(
                name: "UX_RhbAssessment_MedicalFinal",
                schema: "public",
                table: "RhbAssessment",
                column: "EpisodeId",
                unique: true,
                filter: "\"AssessmentType\" = 1 AND \"RecordStatus\" = 2 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RhbDischargeSummary_EncounterId",
                schema: "public",
                table: "RhbDischargeSummary",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbDischargeSummary_EpisodeId",
                schema: "public",
                table: "RhbDischargeSummary",
                column: "EpisodeId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RhbDischargeSummary_RecordStatus",
                schema: "public",
                table: "RhbDischargeSummary",
                column: "RecordStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RhbDiscipline_DisciplineCode",
                schema: "public",
                table: "RhbDiscipline",
                column: "DisciplineCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RhbDiscipline_ProfessionId",
                schema: "public",
                table: "RhbDiscipline",
                column: "ProfessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbDisciplineInstrument_DisciplineId_InstrumentKind",
                schema: "public",
                table: "RhbDisciplineInstrument",
                columns: new[] { "DisciplineId", "InstrumentKind" },
                unique: true,
                filter: "\"IsDelete\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_RhbDisciplineProcedure_DisciplineId_ProcedureId",
                schema: "public",
                table: "RhbDisciplineProcedure",
                columns: new[] { "DisciplineId", "ProcedureId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RhbDisciplineProcedure_ProcedureId",
                schema: "public",
                table: "RhbDisciplineProcedure",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbEducationRecord_ConsentStatus",
                schema: "public",
                table: "RhbEducationRecord",
                column: "ConsentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RhbEducationRecord_EncounterId",
                schema: "public",
                table: "RhbEducationRecord",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbEducationRecord_EpisodeId",
                schema: "public",
                table: "RhbEducationRecord",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbEducationRecord_ProgramId_EducatedAt",
                schema: "public",
                table: "RhbEducationRecord",
                columns: new[] { "ProgramId", "EducatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RhbEpisode_EpisodeNumber",
                schema: "public",
                table: "RhbEpisode",
                column: "EpisodeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RhbEpisode_EpisodeStatus",
                schema: "public",
                table: "RhbEpisode",
                column: "EpisodeStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RhbEpisode_OpenedAt",
                schema: "public",
                table: "RhbEpisode",
                column: "OpenedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RhbEpisode_PatientId",
                schema: "public",
                table: "RhbEpisode",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbEpisode_RequestId",
                schema: "public",
                table: "RhbEpisode",
                column: "RequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RhbEpisode_ResponsibleDoctorId",
                schema: "public",
                table: "RhbEpisode",
                column: "ResponsibleDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbEpisodeDiscipline_DisciplineId",
                schema: "public",
                table: "RhbEpisodeDiscipline",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbEpisodeDiscipline_EpisodeId_DisciplineId",
                schema: "public",
                table: "RhbEpisodeDiscipline",
                columns: new[] { "EpisodeId", "DisciplineId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RhbEpisodeDiscipline_PrescribedByAssessmentId",
                schema: "public",
                table: "RhbEpisodeDiscipline",
                column: "PrescribedByAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbGoal_DisciplineId",
                schema: "public",
                table: "RhbGoal",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbGoal_EpisodeId_DisciplineId_GoalStatus",
                schema: "public",
                table: "RhbGoal",
                columns: new[] { "EpisodeId", "DisciplineId", "GoalStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_RhbGoal_SupersedesGoalId",
                schema: "public",
                table: "RhbGoal",
                column: "SupersedesGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbGoalReview_GoalId",
                schema: "public",
                table: "RhbGoalReview",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbGoalReview_ProgressReviewId_GoalId",
                schema: "public",
                table: "RhbGoalReview",
                columns: new[] { "ProgressReviewId", "GoalId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RhbMeasurement_AssessmentId",
                schema: "public",
                table: "RhbMeasurement",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbMeasurement_InstrumentVersionId",
                schema: "public",
                table: "RhbMeasurement",
                column: "InstrumentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbMeasurement_ProgressReviewId",
                schema: "public",
                table: "RhbMeasurement",
                column: "ProgressReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgram_AuthoredEncounterId",
                schema: "public",
                table: "RhbProgram",
                column: "AuthoredEncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgram_DisciplineId",
                schema: "public",
                table: "RhbProgram",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgram_EvaluationDueDate",
                schema: "public",
                table: "RhbProgram",
                column: "EvaluationDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgram_ProgramStatus",
                schema: "public",
                table: "RhbProgram",
                column: "ProgramStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgram_SupersedesProgramId",
                schema: "public",
                table: "RhbProgram",
                column: "SupersedesProgramId");

            migrationBuilder.CreateIndex(
                name: "UX_RhbProgram_ActivePerDiscipline",
                schema: "public",
                table: "RhbProgram",
                columns: new[] { "EpisodeId", "DisciplineId" },
                unique: true,
                filter: "\"ProgramStatus\" = 2 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_RhbProgram_Version",
                schema: "public",
                table: "RhbProgram",
                columns: new[] { "EpisodeId", "DisciplineId", "ProgramVersion" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgramGoal_GoalId",
                schema: "public",
                table: "RhbProgramGoal",
                column: "GoalId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgramGoal_ProgramId_GoalId",
                schema: "public",
                table: "RhbProgramGoal",
                columns: new[] { "ProgramId", "GoalId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgramIntervention_ProcedureId",
                schema: "public",
                table: "RhbProgramIntervention",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgramIntervention_ProgramId",
                schema: "public",
                table: "RhbProgramIntervention",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgressReview_DisciplineId",
                schema: "public",
                table: "RhbProgressReview",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgressReview_EncounterId",
                schema: "public",
                table: "RhbProgressReview",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgressReview_EpisodeId_DisciplineId",
                schema: "public",
                table: "RhbProgressReview",
                columns: new[] { "EpisodeId", "DisciplineId" });

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgressReview_ProgramId",
                schema: "public",
                table: "RhbProgressReview",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbProgressReview_RecordStatus",
                schema: "public",
                table: "RhbProgressReview",
                column: "RecordStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RhbRequest_InpEpisodeId",
                schema: "public",
                table: "RhbRequest",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbRequest_PatientId",
                schema: "public",
                table: "RhbRequest",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbRequest_RequestedAt",
                schema: "public",
                table: "RhbRequest",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RhbRequest_RequestingDoctorId",
                schema: "public",
                table: "RhbRequest",
                column: "RequestingDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbRequest_RequestNumber",
                schema: "public",
                table: "RhbRequest",
                column: "RequestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RhbRequest_RequestStatus",
                schema: "public",
                table: "RhbRequest",
                column: "RequestStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RhbRequest_SourceEncounterId",
                schema: "public",
                table: "RhbRequest",
                column: "SourceEncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbRequest_SourceType",
                schema: "public",
                table: "RhbRequest",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_RhbRequestDiscipline_DisciplineId",
                schema: "public",
                table: "RhbRequestDiscipline",
                column: "DisciplineId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbRequestDiscipline_RequestId_DisciplineId",
                schema: "public",
                table: "RhbRequestDiscipline",
                columns: new[] { "RequestId", "DisciplineId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSession_BillingHandoffStatus",
                schema: "public",
                table: "RhbSession",
                column: "BillingHandoffStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSession_EncounterId",
                schema: "public",
                table: "RhbSession",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSession_EpisodeId",
                schema: "public",
                table: "RhbSession",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSession_InpEpisodeId",
                schema: "public",
                table: "RhbSession",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSession_ProgramId",
                schema: "public",
                table: "RhbSession",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSession_RecordStatus",
                schema: "public",
                table: "RhbSession",
                column: "RecordStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSession_SessionNumber",
                schema: "public",
                table: "RhbSession",
                column: "SessionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RhbSession_SessionStatus",
                schema: "public",
                table: "RhbSession",
                column: "SessionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSession_StartedAt",
                schema: "public",
                table: "RhbSession",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSession_TherapistUserId",
                schema: "public",
                table: "RhbSession",
                column: "TherapistUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSessionIntervention_PatientProcedureId",
                schema: "public",
                table: "RhbSessionIntervention",
                column: "PatientProcedureId",
                unique: true,
                filter: "\"PatientProcedureId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSessionIntervention_ProcedureId",
                schema: "public",
                table: "RhbSessionIntervention",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSessionIntervention_ProgramInterventionId",
                schema: "public",
                table: "RhbSessionIntervention",
                column: "ProgramInterventionId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbSessionIntervention_SessionId",
                schema: "public",
                table: "RhbSessionIntervention",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_RhbStatusHistory_AggregateType_AggregateId_OccurredAt",
                schema: "public",
                table: "RhbStatusHistory",
                columns: new[] { "AggregateType", "AggregateId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RhbDischargeSummary",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbDisciplineInstrument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbDisciplineProcedure",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbEducationRecord",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbEpisodeDiscipline",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbGoalReview",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbMeasurement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbProgramGoal",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbRequestDiscipline",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbSessionIntervention",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbStatusHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbAssessment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbProgressReview",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbGoal",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbProgramIntervention",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbSession",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbProgram",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbDiscipline",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbEpisode",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RhbRequest",
                schema: "public");
        }
    }
}
