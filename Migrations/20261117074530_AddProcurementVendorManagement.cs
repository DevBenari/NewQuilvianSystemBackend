using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementVendorManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstProcurementEvaluationCriterion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    WeightPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstProcurementEvaluationCriterion", x => x.Id);
                    table.CheckConstraint("CK_MstProcurementEvaluationCriterion_WeightPercent", "\"WeightPercent\" > 0 AND \"WeightPercent\" <= 100");
                    table.ForeignKey(
                        name: "FK_MstProcurementEvaluationCriterion_MstProcurementItemCategor~",
                        column: x => x.ItemCategoryId,
                        principalSchema: "public",
                        principalTable: "MstProcurementItemCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstProcurementVendorDocumentType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DocumentTypeName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RequiresExpiryDate = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstProcurementVendorDocumentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrcVendorProfile",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRestricted = table.Column<bool>(type: "boolean", nullable: false),
                    RestrictedUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    RestrictionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RestrictedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RestrictedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_PrcVendorProfile", x => x.Id);
                    table.CheckConstraint("CK_PrcVendorProfile_RestrictionReason", "\"IsRestricted\" = false OR (\"RestrictionReason\" IS NOT NULL AND length(btrim(\"RestrictionReason\")) > 0)");
                    table.ForeignKey(
                        name: "FK_PrcVendorProfile_MstSupplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstProcurementCategoryDocumentRequirement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ActivatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ActivatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_MstProcurementCategoryDocumentRequirement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstProcurementCategoryDocumentRequirement_MstProcurementIte~",
                        column: x => x.ItemCategoryId,
                        principalSchema: "public",
                        principalTable: "MstProcurementItemCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstProcurementCategoryDocumentRequirement_MstProcurementVen~",
                        column: x => x.DocumentTypeId,
                        principalSchema: "public",
                        principalTable: "MstProcurementVendorDocumentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcVendorCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_PrcVendorCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrcVendorCategory_MstProcurementItemCategory_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalSchema: "public",
                        principalTable: "MstProcurementItemCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcVendorCategory_PrcVendorProfile_VendorProfileId",
                        column: x => x.VendorProfileId,
                        principalSchema: "public",
                        principalTable: "PrcVendorProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrcVendorDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IssuedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    VerificationStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PrcVendorDocument", x => x.Id);
                    table.CheckConstraint("CK_PrcVendorDocument_ExpiryDate", "\"ExpiryDate\" IS NULL OR \"IssuedDate\" IS NULL OR \"ExpiryDate\" >= \"IssuedDate\"");
                    table.ForeignKey(
                        name: "FK_PrcVendorDocument_MstProcurementVendorDocumentType_Document~",
                        column: x => x.DocumentTypeId,
                        principalSchema: "public",
                        principalTable: "MstProcurementVendorDocumentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcVendorDocument_PrcVendorProfile_VendorProfileId",
                        column: x => x.VendorProfileId,
                        principalSchema: "public",
                        principalTable: "PrcVendorProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcVendorEvaluation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PeriodNo = table.Column<int>(type: "integer", nullable: false),
                    SourceType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    FinalScore = table.Column<decimal>(type: "numeric(4,2)", nullable: true),
                    Recommendation = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PreviousEvaluationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevisionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FinalizedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PrcVendorEvaluation", x => x.Id);
                    table.CheckConstraint("CK_PrcVendorEvaluation_FinalScore", "\"FinalScore\" IS NULL OR (\"FinalScore\" >= 1 AND \"FinalScore\" <= 5)");
                    table.CheckConstraint("CK_PrcVendorEvaluation_PeriodNo", "\"PeriodNo\" >= 1 AND \"PeriodNo\" <= 4");
                    table.ForeignKey(
                        name: "FK_PrcVendorEvaluation_MstProcurementItemCategory_ItemCategory~",
                        column: x => x.ItemCategoryId,
                        principalSchema: "public",
                        principalTable: "MstProcurementItemCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcVendorEvaluation_PrcVendorEvaluation_PreviousEvaluationId",
                        column: x => x.PreviousEvaluationId,
                        principalSchema: "public",
                        principalTable: "PrcVendorEvaluation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcVendorEvaluation_PrcVendorProfile_VendorProfileId",
                        column: x => x.VendorProfileId,
                        principalSchema: "public",
                        principalTable: "PrcVendorProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcVendorEvaluationScore",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    WeightPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_PrcVendorEvaluationScore", x => x.Id);
                    table.CheckConstraint("CK_PrcVendorEvaluationScore_Score", "\"Score\" IS NULL OR (\"Score\" >= 1 AND \"Score\" <= 5)");
                    table.ForeignKey(
                        name: "FK_PrcVendorEvaluationScore_MstProcurementEvaluationCriterion_~",
                        column: x => x.CriterionId,
                        principalSchema: "public",
                        principalTable: "MstProcurementEvaluationCriterion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcVendorEvaluationScore_PrcVendorEvaluation_EvaluationId",
                        column: x => x.EvaluationId,
                        principalSchema: "public",
                        principalTable: "PrcVendorEvaluation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrcVendorStatusRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    VendorProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    EvidenceEvaluationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmissionCount = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PrcVendorStatusRequest", x => x.Id);
                    table.CheckConstraint("CK_PrcVendorStatusRequest_Reason", "length(btrim(\"Reason\")) > 0");
                    table.CheckConstraint("CK_PrcVendorStatusRequest_RequestType", "\"RequestType\" IN ('BLOCK', 'UNBLOCK')");
                    table.ForeignKey(
                        name: "FK_PrcVendorStatusRequest_PrcVendorEvaluation_EvidenceEvaluati~",
                        column: x => x.EvidenceEvaluationId,
                        principalSchema: "public",
                        principalTable: "PrcVendorEvaluation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcVendorStatusRequest_PrcVendorProfile_VendorProfileId",
                        column: x => x.VendorProfileId,
                        principalSchema: "public",
                        principalTable: "PrcVendorProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementCategoryDocumentRequirement_DocumentTypeId",
                schema: "public",
                table: "MstProcurementCategoryDocumentRequirement",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementCategoryDocumentRequirement_ItemCategoryId_Do~",
                schema: "public",
                table: "MstProcurementCategoryDocumentRequirement",
                columns: new[] { "ItemCategoryId", "DocumentTypeId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementEvaluationCriterion_ItemCategoryId_CriterionN~",
                schema: "public",
                table: "MstProcurementEvaluationCriterion",
                columns: new[] { "ItemCategoryId", "CriterionName" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcurementVendorDocumentType_DocumentTypeCode",
                schema: "public",
                table: "MstProcurementVendorDocumentType",
                column: "DocumentTypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorCategory_ItemCategoryId",
                schema: "public",
                table: "PrcVendorCategory",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorCategory_VendorProfileId_ItemCategoryId",
                schema: "public",
                table: "PrcVendorCategory",
                columns: new[] { "VendorProfileId", "ItemCategoryId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorDocument_DocumentTypeId",
                schema: "public",
                table: "PrcVendorDocument",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorDocument_ExpiryDate",
                schema: "public",
                table: "PrcVendorDocument",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorDocument_VendorProfileId_DocumentTypeId_Verificati~",
                schema: "public",
                table: "PrcVendorDocument",
                columns: new[] { "VendorProfileId", "DocumentTypeId", "VerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorEvaluation_ItemCategoryId",
                schema: "public",
                table: "PrcVendorEvaluation",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorEvaluation_OriginalPerPeriod",
                schema: "public",
                table: "PrcVendorEvaluation",
                columns: new[] { "VendorProfileId", "ItemCategoryId", "PeriodYear", "PeriodType", "PeriodNo" },
                unique: true,
                filter: "\"Status\" IN ('DRAFT', 'FINAL') AND \"PreviousEvaluationId\" IS NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorEvaluation_PreviousEvaluationId",
                schema: "public",
                table: "PrcVendorEvaluation",
                column: "PreviousEvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorEvaluation_Status",
                schema: "public",
                table: "PrcVendorEvaluation",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorEvaluationScore_CriterionId",
                schema: "public",
                table: "PrcVendorEvaluationScore",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorEvaluationScore_EvaluationId_CriterionId",
                schema: "public",
                table: "PrcVendorEvaluationScore",
                columns: new[] { "EvaluationId", "CriterionId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorProfile_IsRestricted",
                schema: "public",
                table: "PrcVendorProfile",
                column: "IsRestricted");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorProfile_SupplierId",
                schema: "public",
                table: "PrcVendorProfile",
                column: "SupplierId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorStatusRequest_EvidenceEvaluationId",
                schema: "public",
                table: "PrcVendorStatusRequest",
                column: "EvidenceEvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorStatusRequest_OpenPerVendor",
                schema: "public",
                table: "PrcVendorStatusRequest",
                column: "VendorProfileId",
                unique: true,
                filter: "\"Status\" IN ('DRAFT', 'PENDING_APPROVAL', 'RETURNED') AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorStatusRequest_RequestNumber",
                schema: "public",
                table: "PrcVendorStatusRequest",
                column: "RequestNumber",
                unique: true,
                filter: "\"RequestNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorStatusRequest_Status",
                schema: "public",
                table: "PrcVendorStatusRequest",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PrcVendorStatusRequest_WorkflowInstanceId",
                schema: "public",
                table: "PrcVendorStatusRequest",
                column: "WorkflowInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstProcurementCategoryDocumentRequirement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcVendorCategory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcVendorDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcVendorEvaluationScore",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcVendorStatusRequest",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstProcurementVendorDocumentType",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstProcurementEvaluationCriterion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcVendorEvaluation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcVendorProfile",
                schema: "public");
        }
    }
}
