using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRawatInapFinishing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlannedAnesthesiaType",
                schema: "public",
                table: "OprCase",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                schema: "public",
                table: "OprCase",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedByUserId",
                schema: "public",
                table: "OprCase",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "public",
                table: "OprCase",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SurgicalServiceType",
                schema: "public",
                table: "OprCase",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "ChargeBasis",
                schema: "public",
                table: "MstTariff",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChargeRounding",
                schema: "public",
                table: "MstTariff",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "MedicalEquipmentId",
                schema: "public",
                table: "MstTariff",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SurgeryComponentType",
                schema: "public",
                table: "MstTariff",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PendingAdmissionReferralAlertMinutes",
                schema: "public",
                table: "MstInpatientSetting",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<int>(
                name: "PendingSurgicalHandoverAlertMinutes",
                schema: "public",
                table: "MstInpatientSetting",
                type: "integer",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<Guid>(
                name: "AcknowledgedReceiptId",
                schema: "public",
                table: "InpIntegrationOutboxes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessingStartedAtUtc",
                schema: "public",
                table: "InpIntegrationOutboxes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplayBatchId",
                schema: "public",
                table: "InpIntegrationOutboxes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReplayedAtUtc",
                schema: "public",
                table: "InpIntegrationOutboxes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClosureClearanceObserved",
                schema: "public",
                table: "InpEpisode",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DepartureClearanceObserved",
                schema: "public",
                table: "InpEpisode",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepartureClearanceObservedAt",
                schema: "public",
                table: "InpEpisode",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DepartureClearanceWarningAcknowledged",
                schema: "public",
                table: "InpEpisode",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CorrectsPlacementId",
                schema: "public",
                table: "InpBedPlacement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupersededByCorrectionId",
                schema: "public",
                table: "InpBedPlacement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresReview",
                schema: "public",
                table: "BilInvoice",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewFlaggedAt",
                schema: "public",
                table: "BilInvoice",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewReasonCode",
                schema: "public",
                table: "BilInvoice",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewResolutionNote",
                schema: "public",
                table: "BilInvoice",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewResolvedAt",
                schema: "public",
                table: "BilInvoice",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewResolvedByUserId",
                schema: "public",
                table: "BilInvoice",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BilInpatientEventReceipt",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceVersion = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CalculationVersionNo = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_BilInpatientEventReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BilInpatientEventReceipt_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliTransferHandover",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromPlacementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToPlacementId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SoapSummary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    HandedItems = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SpecialInstructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    SentByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReceivedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_CliTransferHandover", x => x.Id);
                    table.CheckConstraint("CK_CliTransferHandover_TwoAccounts", "\"ReceivedByUserId\" IS NULL OR \"ReceivedByUserId\" <> \"SentByUserId\"");
                    table.ForeignKey(
                        name: "FK_CliTransferHandover_InpBedPlacement_FromPlacementId",
                        column: x => x.FromPlacementId,
                        principalSchema: "public",
                        principalTable: "InpBedPlacement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliTransferHandover_InpBedPlacement_ToPlacementId",
                        column: x => x.ToPlacementId,
                        principalSchema: "public",
                        principalTable: "InpBedPlacement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliTransferHandover_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliTransferHandover_MstServiceUnit_FromServiceUnitId",
                        column: x => x.FromServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliTransferHandover_MstServiceUnit_ToServiceUnitId",
                        column: x => x.ToServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpAdmissionReferral",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceEncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrimarySurgeonId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedCareLevel = table.Column<int>(type: "integer", nullable: false),
                    RecoveryDecisionNote = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CompletedEpisodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_InpAdmissionReferral", x => x.Id);
                    table.CheckConstraint("CK_InpAdmissionReferral_State", "(\"Status\" <> 2 OR \"CompletedEpisodeId\" IS NOT NULL) AND (\"Status\" <> 3 OR \"CancelledReason\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_InpAdmissionReferral_InpEpisode_CompletedEpisodeId",
                        column: x => x.CompletedEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpAdmissionReferral_MstDoctor_PrimarySurgeonId",
                        column: x => x.PrimarySurgeonId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpAdmissionReferral_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpAdmissionReferral_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpAdmissionReferral_RegPatientEncounter_SourceEncounterId",
                        column: x => x.SourceEncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstMedicalEquipment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EquipmentName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ChargeUnit = table.Column<int>(type: "integer", nullable: false),
                    RoundingRule = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MstMedicalEquipment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstSurgicalPreparationItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    GroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsMandatory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_MstSurgicalPreparationItem", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OprWardPreOpNote",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OprCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PreviousVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    VitalSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    PainSnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    MarkingLaterality = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MarkingLocationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SiteMarkingConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SentByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConfirmedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NeedsUpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_OprWardPreOpNote", x => x.Id);
                    table.CheckConstraint("CK_OprWardPreOpNote_TwoAccounts", "\"ConfirmedByUserId\" IS NULL OR \"ConfirmedByUserId\" <> \"SentByUserId\"");
                    table.ForeignKey(
                        name: "FK_OprWardPreOpNote_OprCase_OprCaseId",
                        column: x => x.OprCaseId,
                        principalSchema: "public",
                        principalTable: "OprCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OprWardPreOpNote_OprWardPreOpNote_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalSchema: "public",
                        principalTable: "OprWardPreOpNote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OprWardPreOpItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreparationItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsMandatorySnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    SenderConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReceiverConfirmed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReceiverConfirmedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReceiverConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_OprWardPreOpItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OprWardPreOpItem_MstSurgicalPreparationItem_PreparationItem~",
                        column: x => x.PreparationItemId,
                        principalSchema: "public",
                        principalTable: "MstSurgicalPreparationItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OprWardPreOpItem_OprWardPreOpNote_NoteId",
                        column: x => x.NoteId,
                        principalSchema: "public",
                        principalTable: "OprWardPreOpNote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OprWardPreOpSiteMark",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    BodyView = table.Column<int>(type: "integer", nullable: false),
                    X = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Y = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_OprWardPreOpSiteMark", x => x.Id);
                    table.CheckConstraint("CK_OprWardPreOpSiteMark_X", "\"X\" BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_OprWardPreOpSiteMark_Y", "\"Y\" BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_OprWardPreOpSiteMark_OprWardPreOpNote_NoteId",
                        column: x => x.NoteId,
                        principalSchema: "public",
                        principalTable: "OprWardPreOpNote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_OprCase_Rejected",
                schema: "public",
                table: "OprCase",
                sql: "\"Status\" <> 8 OR (\"RejectedAt\" IS NOT NULL AND \"RejectedByUserId\" IS NOT NULL AND \"RejectionReason\" IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_MedicalEquipmentId",
                schema: "public",
                table: "MstTariff",
                column: "MedicalEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_SurgeryComponentType",
                schema: "public",
                table: "MstTariff",
                column: "SurgeryComponentType");

            migrationBuilder.CreateIndex(
                name: "IX_InpIntegrationOutbox_ReplayBatchId",
                schema: "public",
                table: "InpIntegrationOutboxes",
                column: "ReplayBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InpIntegrationOutbox_Status_ProcessingStartedAtUtc",
                schema: "public",
                table: "InpIntegrationOutboxes",
                columns: new[] { "Status", "ProcessingStartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_DepartureClearanceObserved",
                schema: "public",
                table: "InpEpisode",
                column: "DepartureClearanceObserved");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_CorrectsPlacementId",
                schema: "public",
                table: "InpBedPlacement",
                column: "CorrectsPlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_InpBedPlacement_SupersededByCorrectionId",
                schema: "public",
                table: "InpBedPlacement",
                column: "SupersededByCorrectionId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoice_RequiresReview",
                schema: "public",
                table: "BilInvoice",
                column: "RequiresReview",
                filter: "\"RequiresReview\"");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoice_ReviewResolvedByUserId",
                schema: "public",
                table: "BilInvoice",
                column: "ReviewResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInpatientEventReceipt_EncounterId",
                schema: "public",
                table: "BilInpatientEventReceipt",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInpatientEventReceipt_EpisodeId",
                schema: "public",
                table: "BilInpatientEventReceipt",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInpatientEventReceipt_IdempotencyKey",
                schema: "public",
                table: "BilInpatientEventReceipt",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilInpatientEventReceipt_InvoiceId",
                schema: "public",
                table: "BilInpatientEventReceipt",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CliTransferHandover_Episode_Status",
                schema: "public",
                table: "CliTransferHandover",
                columns: new[] { "InpEpisodeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CliTransferHandover_FromPlacementId",
                schema: "public",
                table: "CliTransferHandover",
                column: "FromPlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_CliTransferHandover_FromServiceUnitId",
                schema: "public",
                table: "CliTransferHandover",
                column: "FromServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_CliTransferHandover_ToUnit_Status",
                schema: "public",
                table: "CliTransferHandover",
                columns: new[] { "ToServiceUnitId", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_CliTransferHandover_ToPlacement",
                schema: "public",
                table: "CliTransferHandover",
                column: "ToPlacementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionReferral_PrimarySurgeonId",
                schema: "public",
                table: "InpAdmissionReferral",
                column: "PrimarySurgeonId");

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionReferral_SourceEncounterId",
                schema: "public",
                table: "InpAdmissionReferral",
                column: "SourceEncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionReferral_Status_RequestedAt",
                schema: "public",
                table: "InpAdmissionReferral",
                columns: new[] { "Status", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_InpAdmissionReferral_Case_Pending",
                schema: "public",
                table: "InpAdmissionReferral",
                column: "OprCaseId",
                unique: true,
                filter: "\"Status\" = 1 AND NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "UX_InpAdmissionReferral_CompletedEpisode",
                schema: "public",
                table: "InpAdmissionReferral",
                column: "CompletedEpisodeId",
                unique: true,
                filter: "\"CompletedEpisodeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_InpAdmissionReferral_Patient_Pending",
                schema: "public",
                table: "InpAdmissionReferral",
                column: "PatientId",
                unique: true,
                filter: "\"Status\" = 1 AND NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalEquipment_EquipmentCode",
                schema: "public",
                table: "MstMedicalEquipment",
                column: "EquipmentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalEquipment_EquipmentName",
                schema: "public",
                table: "MstMedicalEquipment",
                column: "EquipmentName");

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalEquipment_IsActive",
                schema: "public",
                table: "MstMedicalEquipment",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MstSurgicalPreparationItem_GroupName",
                schema: "public",
                table: "MstSurgicalPreparationItem",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_MstSurgicalPreparationItem_IsActive",
                schema: "public",
                table: "MstSurgicalPreparationItem",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "UX_MstSurgicalPreparationItem_Code",
                schema: "public",
                table: "MstSurgicalPreparationItem",
                column: "Code",
                unique: true,
                filter: "NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "IX_OprWardPreOpItem_PreparationItemId",
                schema: "public",
                table: "OprWardPreOpItem",
                column: "PreparationItemId");

            migrationBuilder.CreateIndex(
                name: "UX_OprWardPreOpItem_Note_Item",
                schema: "public",
                table: "OprWardPreOpItem",
                columns: new[] { "NoteId", "PreparationItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OprWardPreOpNote_OprCaseId_Status",
                schema: "public",
                table: "OprWardPreOpNote",
                columns: new[] { "OprCaseId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OprWardPreOpNote_PreviousVersionId",
                schema: "public",
                table: "OprWardPreOpNote",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "UX_OprWardPreOpNote_Case_Version",
                schema: "public",
                table: "OprWardPreOpNote",
                columns: new[] { "OprCaseId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_OprWardPreOpNote_OneOpen",
                schema: "public",
                table: "OprWardPreOpNote",
                column: "OprCaseId",
                unique: true,
                filter: "\"Status\" IN (1, 2, 3) AND NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "IX_OprWardPreOpSiteMark_NoteId",
                schema: "public",
                table: "OprWardPreOpSiteMark",
                column: "NoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_BilInvoice_AspNetUsers_ReviewResolvedByUserId",
                schema: "public",
                table: "BilInvoice",
                column: "ReviewResolvedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InpBedPlacement_InpBedPlacement_CorrectsPlacementId",
                schema: "public",
                table: "InpBedPlacement",
                column: "CorrectsPlacementId",
                principalSchema: "public",
                principalTable: "InpBedPlacement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InpBedPlacement_InpBedPlacement_SupersededByCorrectionId",
                schema: "public",
                table: "InpBedPlacement",
                column: "SupersededByCorrectionId",
                principalSchema: "public",
                principalTable: "InpBedPlacement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstTariff_MstMedicalEquipment_MedicalEquipmentId",
                schema: "public",
                table: "MstTariff",
                column: "MedicalEquipmentId",
                principalSchema: "public",
                principalTable: "MstMedicalEquipment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BilInvoice_AspNetUsers_ReviewResolvedByUserId",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropForeignKey(
                name: "FK_InpBedPlacement_InpBedPlacement_CorrectsPlacementId",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropForeignKey(
                name: "FK_InpBedPlacement_InpBedPlacement_SupersededByCorrectionId",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropForeignKey(
                name: "FK_MstTariff_MstMedicalEquipment_MedicalEquipmentId",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropTable(
                name: "BilInpatientEventReceipt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliTransferHandover",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpAdmissionReferral",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstMedicalEquipment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprWardPreOpItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprWardPreOpSiteMark",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstSurgicalPreparationItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "OprWardPreOpNote",
                schema: "public");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OprCase_Rejected",
                schema: "public",
                table: "OprCase");

            migrationBuilder.DropIndex(
                name: "IX_MstTariff_MedicalEquipmentId",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropIndex(
                name: "IX_MstTariff_SurgeryComponentType",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropIndex(
                name: "IX_InpIntegrationOutbox_ReplayBatchId",
                schema: "public",
                table: "InpIntegrationOutboxes");

            migrationBuilder.DropIndex(
                name: "IX_InpIntegrationOutbox_Status_ProcessingStartedAtUtc",
                schema: "public",
                table: "InpIntegrationOutboxes");

            migrationBuilder.DropIndex(
                name: "IX_InpEpisode_DepartureClearanceObserved",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropIndex(
                name: "IX_InpBedPlacement_CorrectsPlacementId",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropIndex(
                name: "IX_InpBedPlacement_SupersededByCorrectionId",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropIndex(
                name: "IX_BilInvoice_RequiresReview",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropIndex(
                name: "IX_BilInvoice_ReviewResolvedByUserId",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropColumn(
                name: "PlannedAnesthesiaType",
                schema: "public",
                table: "OprCase");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                schema: "public",
                table: "OprCase");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                schema: "public",
                table: "OprCase");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "public",
                table: "OprCase");

            migrationBuilder.DropColumn(
                name: "SurgicalServiceType",
                schema: "public",
                table: "OprCase");

            migrationBuilder.DropColumn(
                name: "ChargeBasis",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropColumn(
                name: "ChargeRounding",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropColumn(
                name: "MedicalEquipmentId",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropColumn(
                name: "SurgeryComponentType",
                schema: "public",
                table: "MstTariff");

            migrationBuilder.DropColumn(
                name: "PendingAdmissionReferralAlertMinutes",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "PendingSurgicalHandoverAlertMinutes",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "AcknowledgedReceiptId",
                schema: "public",
                table: "InpIntegrationOutboxes");

            migrationBuilder.DropColumn(
                name: "ProcessingStartedAtUtc",
                schema: "public",
                table: "InpIntegrationOutboxes");

            migrationBuilder.DropColumn(
                name: "ReplayBatchId",
                schema: "public",
                table: "InpIntegrationOutboxes");

            migrationBuilder.DropColumn(
                name: "ReplayedAtUtc",
                schema: "public",
                table: "InpIntegrationOutboxes");

            migrationBuilder.DropColumn(
                name: "ClosureClearanceObserved",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "DepartureClearanceObserved",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "DepartureClearanceObservedAt",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "DepartureClearanceWarningAcknowledged",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "CorrectsPlacementId",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropColumn(
                name: "SupersededByCorrectionId",
                schema: "public",
                table: "InpBedPlacement");

            migrationBuilder.DropColumn(
                name: "RequiresReview",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropColumn(
                name: "ReviewFlaggedAt",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropColumn(
                name: "ReviewReasonCode",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropColumn(
                name: "ReviewResolutionNote",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropColumn(
                name: "ReviewResolvedAt",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropColumn(
                name: "ReviewResolvedByUserId",
                schema: "public",
                table: "BilInvoice");
        }
    }
}
