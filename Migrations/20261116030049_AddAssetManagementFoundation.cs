using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetManagementFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AstAccountingOutbox",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceTransactionId = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    SourceVersion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AccountingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DocumentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    ComponentsJson = table.Column<string>(type: "text", nullable: true),
                    SubledgerJson = table.Column<string>(type: "text", nullable: true),
                    SourceDocumentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SourceDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AccountingEventId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstAccountingOutbox", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AstAttachment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StoredPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstAttachment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AstValuationParameter",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFromPeriod = table.Column<DateOnly>(type: "date", nullable: false),
                    CapitalizationThreshold = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DisposalDirectorThreshold = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UtilizationDirectorThreshold = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SetBy = table.Column<Guid>(type: "uuid", nullable: false),
                    SetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstValuationParameter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstValuationParameter_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstAssetCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsDepreciable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstAssetCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstAssetClassification",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassificationCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ClassificationName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsMedical = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstAssetClassification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstAssetDocumentType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DocumentTypeName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    HasExpiry = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstAssetDocumentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstAssetLocation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LocationName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildingName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FloorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoomName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstAssetLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstAssetLocation_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstAssetLocation_MstRoom_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "MstRoom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstAssetOwnershipType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnershipCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OwnershipName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsCapitalizable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstAssetOwnershipType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstAssetSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdleThresholdDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 90),
                    DocumentExpiryWarningDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    UtilizationMaxMonthsForAssetHead = table.Column<int>(type: "integer", nullable: false, defaultValue: 12),
                    CutoverDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstAssetSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstAssetSetting_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstAssetSetting_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstImportBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CutoverDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FileAttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RowCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ErrorCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TotalAcquisitionCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TotalAccumulatedDepreciation = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssetHeadApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AssetHeadApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AccountingApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AccountingApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstImportBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstImportBatch_AstAttachment_FileAttachmentId",
                        column: x => x.FileAttachmentId,
                        principalSchema: "public",
                        principalTable: "AstAttachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstUtilization",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilizationNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UtilizationType = table.Column<int>(type: "integer", nullable: false),
                    PartnerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AgreementNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AgreementValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequiredApprovalLevel = table.Column<int>(type: "integer", nullable: true),
                    ThresholdUsed = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AssetHeadDecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    AssetHeadDecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DirectorDecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DirectorDecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AgreementAttachmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstUtilization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstUtilization_AstAttachment_AgreementAttachmentId",
                        column: x => x.AgreementAttachmentId,
                        principalSchema: "public",
                        principalTable: "AstAttachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstAssetGroup",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryCode = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    GroupName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstAssetGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstAssetGroup_MstAssetCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "public",
                        principalTable: "MstAssetCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstAssetGroup_MstAssetClassification_ClassificationId",
                        column: x => x.ClassificationId,
                        principalSchema: "public",
                        principalTable: "MstAssetClassification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstAssetCategoryDocumentRequirement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_MstAssetCategoryDocumentRequirement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstAssetCategoryDocumentRequirement_MstAssetCategory_Catego~",
                        column: x => x.CategoryId,
                        principalSchema: "public",
                        principalTable: "MstAssetCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstAssetCategoryDocumentRequirement_MstAssetDocumentType_Do~",
                        column: x => x.DocumentTypeId,
                        principalSchema: "public",
                        principalTable: "MstAssetDocumentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstAssetHandover",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HandoverNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Purpose = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TargetOrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetCustodianEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    HandedOverBy = table.Column<Guid>(type: "uuid", nullable: false),
                    HandedOverAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstAssetHandover", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstAssetHandover_MstAssetLocation_TargetLocationId",
                        column: x => x.TargetLocationId,
                        principalSchema: "public",
                        principalTable: "MstAssetLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAssetHandover_MstEmployee_TargetCustodianEmployeeId",
                        column: x => x.TargetCustodianEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAssetHandover_MstOrganizationUnit_TargetOrganizationUnit~",
                        column: x => x.TargetOrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstAsset",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: false),
                    InventoryNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AssetGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnershipTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ModelType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DistributionPermitNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ManufactureYear = table.Column<int>(type: "integer", nullable: true),
                    DistributorName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    AcquisitionType = table.Column<int>(type: "integer", nullable: false),
                    AcquisitionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReadyForUseDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RegisteredAcquisitionCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    SupplierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoodsReceiptId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoodsReceiptNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DonationReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RecordStatus = table.Column<int>(type: "integer", nullable: false),
                    Condition = table.Column<int>(type: "integer", nullable: false),
                    ConditionChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PlacementState = table.Column<int>(type: "integer", nullable: false),
                    CurrentOrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentLocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentCustodianEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActiveProcessKind = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ActiveProcessId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SourceImportBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstAsset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstAsset_AstImportBatch_SourceImportBatchId",
                        column: x => x.SourceImportBatchId,
                        principalSchema: "public",
                        principalTable: "AstImportBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAsset_FinGoodsReceipt_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalSchema: "public",
                        principalTable: "FinGoodsReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAsset_FinPurchaseOrder_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "public",
                        principalTable: "FinPurchaseOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAsset_MstAssetGroup_AssetGroupId",
                        column: x => x.AssetGroupId,
                        principalSchema: "public",
                        principalTable: "MstAssetGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAsset_MstAssetLocation_CurrentLocationId",
                        column: x => x.CurrentLocationId,
                        principalSchema: "public",
                        principalTable: "MstAssetLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAsset_MstAssetOwnershipType_OwnershipTypeId",
                        column: x => x.OwnershipTypeId,
                        principalSchema: "public",
                        principalTable: "MstAssetOwnershipType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAsset_MstEmployee_CurrentCustodianEmployeeId",
                        column: x => x.CurrentCustodianEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAsset_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAsset_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAsset_MstOrganizationUnit_CurrentOrganizationUnitId",
                        column: x => x.CurrentOrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstDepreciationPolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFromPeriod = table.Column<DateOnly>(type: "date", nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "integer", nullable: false),
                    ResidualValuePercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SetBy = table.Column<Guid>(type: "uuid", nullable: false),
                    SetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstDepreciationPolicy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstDepreciationPolicy_MstAssetGroup_AssetGroupId",
                        column: x => x.AssetGroupId,
                        principalSchema: "public",
                        principalTable: "MstAssetGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstAssetDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssuedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    HolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstAssetDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstAssetDocument_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAssetDocument_AstAttachment_AttachmentId",
                        column: x => x.AttachmentId,
                        principalSchema: "public",
                        principalTable: "AstAttachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAssetDocument_MstAssetDocumentType_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalSchema: "public",
                        principalTable: "MstAssetDocumentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstAssetHandoverItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HandoverId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConditionAtHandover = table.Column<int>(type: "integer", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstAssetHandoverItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstAssetHandoverItem_AstAssetHandover_HandoverId",
                        column: x => x.HandoverId,
                        principalSchema: "public",
                        principalTable: "AstAssetHandover",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAssetHandoverItem_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstAssetHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    SubModule = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FromValue = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ToValue = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SourceDocumentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SourceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceDocumentNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstAssetHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstAssetHistory_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstAssetPlacement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustodianEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstAssetPlacement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstAssetPlacement_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAssetPlacement_MstAssetLocation_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "public",
                        principalTable: "MstAssetLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAssetPlacement_MstEmployee_CustodianEmployeeId",
                        column: x => x.CustodianEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAssetPlacement_MstOrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "public",
                        principalTable: "MstOrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstAssetValueEntry",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryType = table.Column<int>(type: "integer", nullable: false),
                    Period = table.Column<DateOnly>(type: "date", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CostDelta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AccumulatedDepreciationDelta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AccumulatedImpairmentDelta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceDocumentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SourceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceDocumentNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstAssetValueEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstAssetValueEntry_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAssetValueEntry_MstCostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "public",
                        principalTable: "MstCostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstConditionReport",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    SourceDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreviousCondition = table.Column<int>(type: "integer", nullable: false),
                    ReportedCondition = table.Column<int>(type: "integer", nullable: false),
                    IsFoundAgain = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PhotoAttachmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    LossReportAttachmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReportedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DecidedCondition = table.Column<int>(type: "integer", nullable: true),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstConditionReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstConditionReport_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstConditionReport_AstAttachment_LossReportAttachmentId",
                        column: x => x.LossReportAttachmentId,
                        principalSchema: "public",
                        principalTable: "AstAttachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstConditionReport_AstAttachment_PhotoAttachmentId",
                        column: x => x.PhotoAttachmentId,
                        principalSchema: "public",
                        principalTable: "AstAttachment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstImportBatchRow",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    RawJson = table.Column<string>(type: "text", nullable: false),
                    ErrorMessages = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAssetId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstImportBatchRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstImportBatchRow_AstAsset_CreatedAssetId",
                        column: x => x.CreatedAssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstImportBatchRow_AstImportBatch_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalSchema: "public",
                        principalTable: "AstImportBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstUtilizationItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstUtilizationItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AstUtilizationItem_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstUtilizationItem_AstUtilization_UtilizationId",
                        column: x => x.UtilizationId,
                        principalSchema: "public",
                        principalTable: "AstUtilization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AstAssetBook",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCapitalized = table.Column<bool>(type: "boolean", nullable: false),
                    IsDepreciable = table.Column<bool>(type: "boolean", nullable: false),
                    AcquisitionCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CapitalizedCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ResidualValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UsefulLifeMonths = table.Column<int>(type: "integer", nullable: false),
                    DepreciatedMonths = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DepreciationStartPeriod = table.Column<DateOnly>(type: "date", nullable: false),
                    LastDepreciatedPeriod = table.Column<DateOnly>(type: "date", nullable: true),
                    AccumulatedDepreciation = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    AccumulatedImpairment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    HasEstimateOverride = table.Column<bool>(type: "boolean", nullable: false),
                    DepreciationPolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookStatus = table.Column<int>(type: "integer", nullable: false),
                    StoppedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_AstAssetBook", x => x.Id);
                    table.CheckConstraint("CK_AstAssetBook_NotBelowResidual", "\"CapitalizedCost\" - \"AccumulatedDepreciation\" - \"AccumulatedImpairment\" >= \"ResidualValue\" OR \"IsCapitalized\" = false");
                    table.ForeignKey(
                        name: "FK_AstAssetBook_AstAsset_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "public",
                        principalTable: "AstAsset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AstAssetBook_AstDepreciationPolicy_DepreciationPolicyId",
                        column: x => x.DepreciationPolicyId,
                        principalSchema: "public",
                        principalTable: "AstDepreciationPolicy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AstAccountingOutbox_EventTypeCode",
                schema: "public",
                table: "AstAccountingOutbox",
                column: "EventTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_AstAccountingOutbox_Source",
                schema: "public",
                table: "AstAccountingOutbox",
                columns: new[] { "SourceTransactionId", "SourceVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AstAccountingOutbox_SourceDocumentType_SourceDocumentId",
                schema: "public",
                table: "AstAccountingOutbox",
                columns: new[] { "SourceDocumentType", "SourceDocumentId" });

            migrationBuilder.CreateIndex(
                name: "IX_AstAccountingOutbox_Status_Created",
                schema: "public",
                table: "AstAccountingOutbox",
                columns: new[] { "Status", "CreateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_AcquisitionType",
                schema: "public",
                table: "AstAsset",
                column: "AcquisitionType");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_ActiveProcessKind",
                schema: "public",
                table: "AstAsset",
                column: "ActiveProcessKind");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_AssetGroupId",
                schema: "public",
                table: "AstAsset",
                column: "AssetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_AssetName",
                schema: "public",
                table: "AstAsset",
                column: "AssetName");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_Brand_SerialNumber",
                schema: "public",
                table: "AstAsset",
                columns: new[] { "Brand", "SerialNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_Condition",
                schema: "public",
                table: "AstAsset",
                column: "Condition");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_CurrentCustodianEmployeeId",
                schema: "public",
                table: "AstAsset",
                column: "CurrentCustodianEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_CurrentLocationId",
                schema: "public",
                table: "AstAsset",
                column: "CurrentLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_GoodsReceiptId",
                schema: "public",
                table: "AstAsset",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_HospitalSiteId",
                schema: "public",
                table: "AstAsset",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_InventoryNumber",
                schema: "public",
                table: "AstAsset",
                column: "InventoryNumber",
                unique: true,
                filter: "\"InventoryNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_LegalEntityId",
                schema: "public",
                table: "AstAsset",
                column: "LegalEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_OwnershipTypeId",
                schema: "public",
                table: "AstAsset",
                column: "OwnershipTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_PlacementState",
                schema: "public",
                table: "AstAsset",
                column: "PlacementState");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_PurchaseOrderId",
                schema: "public",
                table: "AstAsset",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_RecordStatus",
                schema: "public",
                table: "AstAsset",
                column: "RecordStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_SourceImportBatchId",
                schema: "public",
                table: "AstAsset",
                column: "SourceImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAsset_Unit_Status",
                schema: "public",
                table: "AstAsset",
                columns: new[] { "CurrentOrganizationUnitId", "RecordStatus", "Condition" });

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetBook_AssetId",
                schema: "public",
                table: "AstAssetBook",
                column: "AssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetBook_BookStatus",
                schema: "public",
                table: "AstAssetBook",
                column: "BookStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetBook_DepreciationPolicyId",
                schema: "public",
                table: "AstAssetBook",
                column: "DepreciationPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetBook_DepreciationStartPeriod",
                schema: "public",
                table: "AstAssetBook",
                column: "DepreciationStartPeriod");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetBook_IsCapitalized",
                schema: "public",
                table: "AstAssetBook",
                column: "IsCapitalized");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetDocument_AssetId",
                schema: "public",
                table: "AstAssetDocument",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetDocument_AttachmentId",
                schema: "public",
                table: "AstAssetDocument",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetDocument_DocumentTypeId",
                schema: "public",
                table: "AstAssetDocument",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetDocument_ExpiryDate",
                schema: "public",
                table: "AstAssetDocument",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHandover_HandoverNumber",
                schema: "public",
                table: "AstAssetHandover",
                column: "HandoverNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHandover_Purpose",
                schema: "public",
                table: "AstAssetHandover",
                column: "Purpose");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHandover_Status",
                schema: "public",
                table: "AstAssetHandover",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHandover_TargetCustodianEmployeeId",
                schema: "public",
                table: "AstAssetHandover",
                column: "TargetCustodianEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHandover_TargetLocationId",
                schema: "public",
                table: "AstAssetHandover",
                column: "TargetLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHandover_TargetOrganizationUnitId",
                schema: "public",
                table: "AstAssetHandover",
                column: "TargetOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHandoverItem_AssetId",
                schema: "public",
                table: "AstAssetHandoverItem",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHandoverItem_Handover_Asset",
                schema: "public",
                table: "AstAssetHandoverItem",
                columns: new[] { "HandoverId", "AssetId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHistory_ActorUserId",
                schema: "public",
                table: "AstAssetHistory",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHistory_Asset_OccurredAt",
                schema: "public",
                table: "AstAssetHistory",
                columns: new[] { "AssetId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHistory_EventType",
                schema: "public",
                table: "AstAssetHistory",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHistory_SourceDocumentId",
                schema: "public",
                table: "AstAssetHistory",
                column: "SourceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetHistory_SubModule",
                schema: "public",
                table: "AstAssetHistory",
                column: "SubModule");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetPlacement_Current",
                schema: "public",
                table: "AstAssetPlacement",
                column: "AssetId",
                unique: true,
                filter: "\"EndAt\" IS NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetPlacement_CustodianEmployeeId",
                schema: "public",
                table: "AstAssetPlacement",
                column: "CustodianEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetPlacement_LocationId",
                schema: "public",
                table: "AstAssetPlacement",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetPlacement_OrganizationUnitId",
                schema: "public",
                table: "AstAssetPlacement",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetPlacement_SourceDocumentId",
                schema: "public",
                table: "AstAssetPlacement",
                column: "SourceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetPlacement_StartAt",
                schema: "public",
                table: "AstAssetPlacement",
                column: "StartAt");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetValueEntry_Asset_Period",
                schema: "public",
                table: "AstAssetValueEntry",
                columns: new[] { "AssetId", "Period" });

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetValueEntry_CostCenterId",
                schema: "public",
                table: "AstAssetValueEntry",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetValueEntry_EntryType",
                schema: "public",
                table: "AstAssetValueEntry",
                column: "EntryType");

            migrationBuilder.CreateIndex(
                name: "IX_AstAssetValueEntry_SourceDocumentId",
                schema: "public",
                table: "AstAssetValueEntry",
                column: "SourceDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_AstAttachment_Owner",
                schema: "public",
                table: "AstAttachment",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_AstAttachment_Sha256",
                schema: "public",
                table: "AstAttachment",
                column: "Sha256");

            migrationBuilder.CreateIndex(
                name: "IX_AstConditionReport_LossReportAttachmentId",
                schema: "public",
                table: "AstConditionReport",
                column: "LossReportAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AstConditionReport_Open",
                schema: "public",
                table: "AstConditionReport",
                column: "AssetId",
                unique: true,
                filter: "\"Status\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstConditionReport_PhotoAttachmentId",
                schema: "public",
                table: "AstConditionReport",
                column: "PhotoAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AstConditionReport_ReportNumber",
                schema: "public",
                table: "AstConditionReport",
                column: "ReportNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstConditionReport_Source",
                schema: "public",
                table: "AstConditionReport",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_AstConditionReport_Status",
                schema: "public",
                table: "AstConditionReport",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AstDepreciationPolicy_Group_Period",
                schema: "public",
                table: "AstDepreciationPolicy",
                columns: new[] { "AssetGroupId", "EffectiveFromPeriod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AstImportBatch_FileAttachmentId",
                schema: "public",
                table: "AstImportBatch",
                column: "FileAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AstImportBatch_ImportNumber",
                schema: "public",
                table: "AstImportBatch",
                column: "ImportNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstImportBatch_Status",
                schema: "public",
                table: "AstImportBatch",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AstImportBatchRow_Batch_RowNumber",
                schema: "public",
                table: "AstImportBatchRow",
                columns: new[] { "ImportBatchId", "RowNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstImportBatchRow_CreatedAssetId",
                schema: "public",
                table: "AstImportBatchRow",
                column: "CreatedAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AstUtilization_AgreementAttachmentId",
                schema: "public",
                table: "AstUtilization",
                column: "AgreementAttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AstUtilization_EndDate",
                schema: "public",
                table: "AstUtilization",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_AstUtilization_StartDate",
                schema: "public",
                table: "AstUtilization",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_AstUtilization_Status",
                schema: "public",
                table: "AstUtilization",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AstUtilization_UtilizationNumber",
                schema: "public",
                table: "AstUtilization",
                column: "UtilizationNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstUtilization_UtilizationType",
                schema: "public",
                table: "AstUtilization",
                column: "UtilizationType");

            migrationBuilder.CreateIndex(
                name: "IX_AstUtilizationItem_AssetId",
                schema: "public",
                table: "AstUtilizationItem",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AstUtilizationItem_Utilization_Asset",
                schema: "public",
                table: "AstUtilizationItem",
                columns: new[] { "UtilizationId", "AssetId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AstValuationParameter_Entity_Period",
                schema: "public",
                table: "AstValuationParameter",
                columns: new[] { "LegalEntityId", "EffectiveFromPeriod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetCategory_CategoryCode",
                schema: "public",
                table: "MstAssetCategory",
                column: "CategoryCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetCategoryDocumentRequirement_Category_DocumentType",
                schema: "public",
                table: "MstAssetCategoryDocumentRequirement",
                columns: new[] { "CategoryId", "DocumentTypeId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetCategoryDocumentRequirement_DocumentTypeId",
                schema: "public",
                table: "MstAssetCategoryDocumentRequirement",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetClassification_ClassificationCode",
                schema: "public",
                table: "MstAssetClassification",
                column: "ClassificationCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetDocumentType_DocumentTypeCode",
                schema: "public",
                table: "MstAssetDocumentType",
                column: "DocumentTypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetGroup_CategoryId",
                schema: "public",
                table: "MstAssetGroup",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetGroup_ClassificationId",
                schema: "public",
                table: "MstAssetGroup",
                column: "ClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetGroup_InventoryCode",
                schema: "public",
                table: "MstAssetGroup",
                column: "InventoryCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetLocation_LocationCode",
                schema: "public",
                table: "MstAssetLocation",
                column: "LocationCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetLocation_OrganizationUnitId",
                schema: "public",
                table: "MstAssetLocation",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetLocation_RoomId",
                schema: "public",
                table: "MstAssetLocation",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetOwnershipType_OwnershipCode",
                schema: "public",
                table: "MstAssetOwnershipType",
                column: "OwnershipCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetSetting_HospitalSiteId",
                schema: "public",
                table: "MstAssetSetting",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_MstAssetSetting_LegalEntityId",
                schema: "public",
                table: "MstAssetSetting",
                column: "LegalEntityId",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AstAccountingOutbox",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstAssetBook",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstAssetDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstAssetHandoverItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstAssetHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstAssetPlacement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstAssetValueEntry",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstConditionReport",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstImportBatchRow",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstUtilizationItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstValuationParameter",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAssetCategoryDocumentRequirement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAssetSetting",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstDepreciationPolicy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstAssetHandover",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstAsset",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstUtilization",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAssetDocumentType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstImportBatch",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAssetGroup",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAssetLocation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAssetOwnershipType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AstAttachment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAssetCategory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstAssetClassification",
                schema: "public");
        }
    }
}
