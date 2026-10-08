using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspacePpriAdmissionDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BeliefValuesFormCode",
                schema: "public",
                table: "MstInpatientSetting",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CostDifferenceFormCode",
                schema: "public",
                table: "MstInpatientSetting",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CostEstimateFormCode",
                schema: "public",
                table: "MstInpatientSetting",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepositSettlementFormCode",
                schema: "public",
                table: "MstInpatientSetting",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentSigningCity",
                schema: "public",
                table: "MstInpatientSetting",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneralConsentFormCode",
                schema: "public",
                table: "MstInpatientSetting",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InfantWristbandMaxAgeYears",
                schema: "public",
                table: "MstInpatientSetting",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<string>(
                name: "InpatientBaseDataFormCode",
                schema: "public",
                table: "MstInpatientSetting",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewPatientHandoverFormCode",
                schema: "public",
                table: "MstInpatientSetting",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientLabelHospitalCode",
                schema: "public",
                table: "MstInpatientSetting",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivacyRequestFormCode",
                schema: "public",
                table: "MstInpatientSetting",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChecklistType",
                schema: "public",
                table: "MstInpatientClearanceItem",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "HandoverSuggestionSource",
                schema: "public",
                table: "MstInpatientClearanceItem",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentItemId",
                schema: "public",
                table: "MstInpatientClearanceItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InpAdmissionDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    VersionNo = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PreviousVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SigningCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StatementDate = table.Column<DateTime>(type: "date", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SnapshotFormatVersion = table.Column<int>(type: "integer", nullable: true),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SupersededAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
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
                    table.PrimaryKey("PK_InpAdmissionDocument", x => x.Id);
                    table.CheckConstraint("CK_InpAdmissionDocument_State", "(\"Status\" <> 2 OR (\"LockedAt\" IS NOT NULL AND \"SnapshotJson\" IS NOT NULL)) AND (\"Status\" <> 3 OR (\"CompletedAt\" IS NOT NULL AND \"SnapshotJson\" IS NOT NULL)) AND (\"Status\" <> 4 OR \"SupersededAt\" IS NOT NULL) AND (\"Status\" <> 5 OR (\"CancelledAt\" IS NOT NULL AND \"CancelledReason\" IS NOT NULL)) AND (\"VersionNo\" = 1 OR (\"PreviousVersionId\" IS NOT NULL AND \"CorrectionReason\" IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_InpAdmissionDocument_InpAdmissionDocument_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalSchema: "public",
                        principalTable: "InpAdmissionDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpAdmissionDocument_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpAdmissionDocument_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpAdmissionBeliefItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemNo = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_InpAdmissionBeliefItem", x => x.Id);
                    table.CheckConstraint("CK_InpAdmissionBeliefItem_ItemNo", "\"ItemNo\" BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_InpAdmissionBeliefItem_InpAdmissionDocument_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "public",
                        principalTable: "InpAdmissionDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpAdmissionCostDifferenceStatement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject = table.Column<int>(type: "integer", nullable: false),
                    SubjectOtherText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_InpAdmissionCostDifferenceStatement", x => x.Id);
                    table.CheckConstraint("CK_InpAdmissionCostDifferenceStatement_Other", "\"Subject\" <> 5 OR \"SubjectOtherText\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_InpAdmissionCostDifferenceStatement_InpAdmissionDocument_Do~",
                        column: x => x.DocumentId,
                        principalSchema: "public",
                        principalTable: "InpAdmissionDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpAdmissionDepositStatement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PolicyFollowUpIntervalDays = table.Column<int>(type: "integer", nullable: true),
                    MinimumPolicyAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ReceivedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ShortfallAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    AmountsReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_InpAdmissionDepositStatement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpAdmissionDepositStatement_InpAdmissionDocument_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "public",
                        principalTable: "InpAdmissionDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpAdmissionDocumentParty",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    SourceRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    FullName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Relationship = table.Column<int>(type: "integer", nullable: true),
                    RelationshipText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: true),
                    Occupation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdentityType = table.Column<int>(type: "integer", nullable: true),
                    IdentityNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MobilePhone = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    OfficePhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
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
                    table.PrimaryKey("PK_InpAdmissionDocumentParty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpAdmissionDocumentParty_InpAdmissionDocument_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "public",
                        principalTable: "InpAdmissionDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpAdmissionDocumentSignature",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    SignerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SignerPositionName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    SignerRelationship = table.Column<int>(type: "integer", nullable: true),
                    SignerRelationshipText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
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
                    table.PrimaryKey("PK_InpAdmissionDocumentSignature", x => x.Id);
                    table.CheckConstraint("CK_InpAdmissionDocumentSignature_Method", "(\"Method\" <> 1 OR (\"Slot\" = 1 AND \"VerifiedByUserId\" IS NOT NULL AND \"SignedByUserId\" IS NULL)) AND (\"Method\" <> 2 OR (\"Slot\" <> 1 AND \"SignedByUserId\" IS NOT NULL))");
                    table.ForeignKey(
                        name: "FK_InpAdmissionDocumentSignature_InpAdmissionDocument_Document~",
                        column: x => x.DocumentId,
                        principalSchema: "public",
                        principalTable: "InpAdmissionDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpAdmissionHandoverItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClearanceItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemNumberSnapshot = table.Column<int>(type: "integer", nullable: true),
                    ParentItemNumberSnapshot = table.Column<int>(type: "integer", nullable: true),
                    ItemCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Choice = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_InpAdmissionHandoverItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpAdmissionHandoverItem_InpAdmissionDocument_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "public",
                        principalTable: "InpAdmissionDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpAdmissionHandoverItem_MstInpatientClearanceItem_Clearanc~",
                        column: x => x.ClearanceItemId,
                        principalSchema: "public",
                        principalTable: "MstInpatientClearanceItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpAdmissionPrintLog",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrintKind = table.Column<int>(type: "integer", nullable: false),
                    DocumentStatusAtPrint = table.Column<int>(type: "integer", nullable: true),
                    Copies = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsReprint = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReprintReason = table.Column<int>(type: "integer", nullable: true),
                    ReprintNote = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PrintedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrintedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
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
                    table.PrimaryKey("PK_InpAdmissionPrintLog", x => x.Id);
                    table.CheckConstraint("CK_InpAdmissionPrintLog_Reprint", "(\"IsReprint\" = false OR \"ReprintReason\" IS NOT NULL) AND (\"ReprintReason\" IS DISTINCT FROM 4 OR \"ReprintNote\" IS NOT NULL) AND (\"PrintKind\" <> 5 OR \"DocumentId\" IS NOT NULL) AND (\"Copies\" BETWEEN 1 AND 10)");
                    table.ForeignKey(
                        name: "FK_InpAdmissionPrintLog_InpAdmissionDocument_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "public",
                        principalTable: "InpAdmissionDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpAdmissionPrintLog_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpAdmissionPrivacyEntry",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryType = table.Column<int>(type: "integer", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_InpAdmissionPrivacyEntry", x => x.Id);
                    table.CheckConstraint("CK_InpAdmissionPrivacyEntry_LineNo", "\"LineNo\" BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_InpAdmissionPrivacyEntry_InpAdmissionDocument_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "public",
                        principalTable: "InpAdmissionDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InpAdmissionPrivacyRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsTransportPrivacyRequested = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_InpAdmissionPrivacyRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InpAdmissionPrivacyRequest_InpAdmissionDocument_DocumentId",
                        column: x => x.DocumentId,
                        principalSchema: "public",
                        principalTable: "InpAdmissionDocument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientClearanceItem_ChecklistType_IsActive",
                schema: "public",
                table: "MstInpatientClearanceItem",
                columns: new[] { "ChecklistType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInpatientClearanceItem_ParentItemId",
                schema: "public",
                table: "MstInpatientClearanceItem",
                column: "ParentItemId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstInpatientClearanceItem_Type",
                schema: "public",
                table: "MstInpatientClearanceItem",
                sql: "\"ChecklistType\" <> 1 OR (\"ParentItemId\" IS NULL AND \"HandoverSuggestionSource\" = 0)");

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionBeliefItem_DocumentId_ItemNo",
                schema: "public",
                table: "InpAdmissionBeliefItem",
                columns: new[] { "DocumentId", "ItemNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionCostDifferenceStatement_DocumentId",
                schema: "public",
                table: "InpAdmissionCostDifferenceStatement",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionDepositStatement_DocumentId",
                schema: "public",
                table: "InpAdmissionDepositStatement",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionDocument_EpisodeId_DocumentType_CreateDateTime",
                schema: "public",
                table: "InpAdmissionDocument",
                columns: new[] { "EpisodeId", "DocumentType", "CreateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionDocument_PatientId_DocumentType_Status",
                schema: "public",
                table: "InpAdmissionDocument",
                columns: new[] { "PatientId", "DocumentType", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_InpAdmissionDocument_Episode_Type_Active",
                schema: "public",
                table: "InpAdmissionDocument",
                columns: new[] { "EpisodeId", "DocumentType" },
                unique: true,
                filter: "\"Status\" IN (1, 2, 3) AND NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "UX_InpAdmissionDocument_IdempotencyKey",
                schema: "public",
                table: "InpAdmissionDocument",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL AND NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "UX_InpAdmissionDocument_PreviousVersionId",
                schema: "public",
                table: "InpAdmissionDocument",
                column: "PreviousVersionId",
                unique: true,
                filter: "\"PreviousVersionId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionDocumentParty_DocumentId",
                schema: "public",
                table: "InpAdmissionDocumentParty",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_InpAdmissionDocumentSignature_Document_Signer",
                schema: "public",
                table: "InpAdmissionDocumentSignature",
                columns: new[] { "DocumentId", "SignedByUserId" },
                unique: true,
                filter: "\"SignedByUserId\" IS NOT NULL AND NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "UX_InpAdmissionDocumentSignature_Document_Slot",
                schema: "public",
                table: "InpAdmissionDocumentSignature",
                columns: new[] { "DocumentId", "Slot" },
                unique: true,
                filter: "NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "UX_InpAdmissionDocumentSignature_IdempotencyKey",
                schema: "public",
                table: "InpAdmissionDocumentSignature",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL AND NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionHandoverItem_ClearanceItemId",
                schema: "public",
                table: "InpAdmissionHandoverItem",
                column: "ClearanceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionHandoverItem_DocumentId_ClearanceItemId",
                schema: "public",
                table: "InpAdmissionHandoverItem",
                columns: new[] { "DocumentId", "ClearanceItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionHandoverItem_DocumentId_LineNo",
                schema: "public",
                table: "InpAdmissionHandoverItem",
                columns: new[] { "DocumentId", "LineNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionPrintLog_DocumentId_PrintedAt",
                schema: "public",
                table: "InpAdmissionPrintLog",
                columns: new[] { "DocumentId", "PrintedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionPrintLog_EpisodeId_PrintKind_PrintedAt",
                schema: "public",
                table: "InpAdmissionPrintLog",
                columns: new[] { "EpisodeId", "PrintKind", "PrintedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_InpAdmissionPrintLog_IdempotencyKey",
                schema: "public",
                table: "InpAdmissionPrintLog",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL AND NOT \"IsDelete\"");

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionPrivacyEntry_DocumentId_EntryType_LineNo",
                schema: "public",
                table: "InpAdmissionPrivacyEntry",
                columns: new[] { "DocumentId", "EntryType", "LineNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpAdmissionPrivacyRequest_DocumentId",
                schema: "public",
                table: "InpAdmissionPrivacyRequest",
                column: "DocumentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MstInpatientClearanceItem_MstInpatientClearanceItem_ParentI~",
                schema: "public",
                table: "MstInpatientClearanceItem",
                column: "ParentItemId",
                principalSchema: "public",
                principalTable: "MstInpatientClearanceItem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstInpatientClearanceItem_MstInpatientClearanceItem_ParentI~",
                schema: "public",
                table: "MstInpatientClearanceItem");

            migrationBuilder.DropTable(
                name: "InpAdmissionBeliefItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpAdmissionCostDifferenceStatement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpAdmissionDepositStatement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpAdmissionDocumentParty",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpAdmissionDocumentSignature",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpAdmissionHandoverItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpAdmissionPrintLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpAdmissionPrivacyEntry",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpAdmissionPrivacyRequest",
                schema: "public");

            migrationBuilder.DropTable(
                name: "InpAdmissionDocument",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstInpatientClearanceItem_ChecklistType_IsActive",
                schema: "public",
                table: "MstInpatientClearanceItem");

            migrationBuilder.DropIndex(
                name: "IX_MstInpatientClearanceItem_ParentItemId",
                schema: "public",
                table: "MstInpatientClearanceItem");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstInpatientClearanceItem_Type",
                schema: "public",
                table: "MstInpatientClearanceItem");

            migrationBuilder.DropColumn(
                name: "BeliefValuesFormCode",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "CostDifferenceFormCode",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "CostEstimateFormCode",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "DepositSettlementFormCode",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "DocumentSigningCity",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "GeneralConsentFormCode",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "InfantWristbandMaxAgeYears",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "InpatientBaseDataFormCode",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "NewPatientHandoverFormCode",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "PatientLabelHospitalCode",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "PrivacyRequestFormCode",
                schema: "public",
                table: "MstInpatientSetting");

            migrationBuilder.DropColumn(
                name: "ChecklistType",
                schema: "public",
                table: "MstInpatientClearanceItem");

            migrationBuilder.DropColumn(
                name: "HandoverSuggestionSource",
                schema: "public",
                table: "MstInpatientClearanceItem");

            migrationBuilder.DropColumn(
                name: "ParentItemId",
                schema: "public",
                table: "MstInpatientClearanceItem");
        }
    }
}
