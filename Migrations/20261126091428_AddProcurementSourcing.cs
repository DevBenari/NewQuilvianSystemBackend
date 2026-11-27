using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-PBJ-PS-004 (pemilihan-penyedia, urutan migration 10 di <c>02-module-map.md</c>): 14 tabel submodule
    /// <c>ProcurementManagement / Sourcing</c> (<c>Prc</c>, <c>PBJ-DEC-096</c>) — paket, item, sumber baris PR, checklist,
    /// evaluator, RFQ, undangan, penawaran, baris penawaran, kriteria teknis, hasil evaluasi, negosiasi, baris negosiasi,
    /// dan penetapan penyedia. Hanya menambah tabel kosong, sehingga aman tanpa henti layanan; <c>Down</c> menghapus
    /// ke-14 tabel dan hanya dijalankan sebelum ada data produksi.
    /// </summary>
    public partial class AddProcurementSourcing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrcPackage",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcurementTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FiscalYear = table.Column<int>(type: "integer", nullable: false),
                    PackagePlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    SplitFromPackageId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    MethodCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MethodName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    MethodMinHpsAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MethodMaxHpsAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MethodMinimumQuotationCount = table.Column<int>(type: "integer", nullable: true),
                    MethodRequiresTechnicalEvaluation = table.Column<bool>(type: "boolean", nullable: false),
                    IsMethodOutOfRange = table.Column<bool>(type: "boolean", nullable: false),
                    MethodDeviationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HpsTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ApprovedHpsTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PreparationWorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreparationSubmissionCount = table.Column<int>(type: "integer", nullable: false),
                    PreparationSubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PreparationApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PicUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseOrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PurchaseOrderRecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PurchaseOrderRecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PrcPackage", x => x.Id);
                    table.CheckConstraint("CK_PrcPackage_HpsTotal", "\"HpsTotal\" >= 0");
                    table.CheckConstraint("CK_PrcPackage_MethodDeviationReason", "NOT \"IsMethodOutOfRange\" OR length(btrim(coalesce(\"MethodDeviationReason\", ''))) > 0");
                    table.CheckConstraint("CK_PrcPackage_PreparationSubmissionCount", "\"PreparationSubmissionCount\" >= 0");
                    table.CheckConstraint("CK_PrcPackage_Status", "\"Status\" IN ('DRAFT', 'PREPARATION_PENDING_APPROVAL', 'PREPARATION_RETURNED', 'READY_FOR_RFQ', 'RFQ_OPEN', 'EVALUATION', 'AWARD_PENDING_APPROVAL', 'AWARDED', 'ORDERED', 'REJECTED', 'CANCELLED')");
                    table.ForeignKey(
                        name: "FK_PrcPackage_MstProcurementMethod_MethodId",
                        column: x => x.MethodId,
                        principalSchema: "public",
                        principalTable: "MstProcurementMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPackage_MstProcurementType_ProcurementTypeId",
                        column: x => x.ProcurementTypeId,
                        principalSchema: "public",
                        principalTable: "MstProcurementType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPackage_PrcPackagePlan_PackagePlanId",
                        column: x => x.PackagePlanId,
                        principalSchema: "public",
                        principalTable: "PrcPackagePlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPackage_PrcPackage_SplitFromPackageId",
                        column: x => x.SplitFromPackageId,
                        principalSchema: "public",
                        principalTable: "PrcPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcPackageChecklistItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentTypeCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DocumentTypeName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_PrcPackageChecklistItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrcPackageChecklistItem_MstProcurementPackageDocumentType_D~",
                        column: x => x.DocumentTypeId,
                        principalSchema: "public",
                        principalTable: "MstProcurementPackageDocumentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPackageChecklistItem_PrcPackage_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "public",
                        principalTable: "PrcPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcPackageEvaluator",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_PrcPackageEvaluator", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrcPackageEvaluator_PrcPackage_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "public",
                        principalTable: "PrcPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcPackageItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Specification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    HpsUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HpsAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_PrcPackageItem", x => x.Id);
                    table.CheckConstraint("CK_PrcPackageItem_HpsUnitPrice", "\"HpsUnitPrice\" >= 0");
                    table.CheckConstraint("CK_PrcPackageItem_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_PrcPackageItem_MstProcurementItem_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "public",
                        principalTable: "MstProcurementItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPackageItem_PrcPackage_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "public",
                        principalTable: "PrcPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcRfq",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Deadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExtensionCount = table.Column<int>(type: "integer", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InheritedFromRfqId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_PrcRfq", x => x.Id);
                    table.CheckConstraint("CK_PrcRfq_ExtensionCount", "\"ExtensionCount\" >= 0");
                    table.CheckConstraint("CK_PrcRfq_FailureReason", "\"Status\" <> 'FAILED' OR length(btrim(coalesce(\"FailureReason\", ''))) > 0");
                    table.CheckConstraint("CK_PrcRfq_Status", "\"Status\" IN ('DRAFT', 'SENT', 'CLOSED', 'FAILED', 'CANCELLED')");
                    table.ForeignKey(
                        name: "FK_PrcRfq_PrcPackage_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "public",
                        principalTable: "PrcPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcRfq_PrcRfq_InheritedFromRfqId",
                        column: x => x.InheritedFromRfqId,
                        principalSchema: "public",
                        principalTable: "PrcRfq",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcPackageItemSource",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseRequestLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    IsReleased = table.Column<bool>(type: "boolean", nullable: false),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PrcPackageItemSource", x => x.Id);
                    table.CheckConstraint("CK_PrcPackageItemSource_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_PrcPackageItemSource_PrcPackageItem_PackageItemId",
                        column: x => x.PackageItemId,
                        principalSchema: "public",
                        principalTable: "PrcPackageItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPackageItemSource_PrcPurchaseRequestLine_PurchaseRequest~",
                        column: x => x.PurchaseRequestLineId,
                        principalSchema: "public",
                        principalTable: "PrcPurchaseRequestLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPackageItemSource_PrcPurchaseRequest_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalSchema: "public",
                        principalTable: "PrcPurchaseRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcTechnicalCriterion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionNo = table.Column<int>(type: "integer", nullable: false),
                    CriterionText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_PrcTechnicalCriterion", x => x.Id);
                    table.CheckConstraint("CK_PrcTechnicalCriterion_CriterionNo", "\"CriterionNo\" >= 1");
                    table.ForeignKey(
                        name: "FK_PrcTechnicalCriterion_PrcPackageItem_PackageItemId",
                        column: x => x.PackageItemId,
                        principalSchema: "public",
                        principalTable: "PrcPackageItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcQuotation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsLate = table.Column<bool>(type: "boolean", nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false),
                    TaxPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    TotalBeforeTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VoidReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SourceQuotationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_PrcQuotation", x => x.Id);
                    table.CheckConstraint("CK_PrcQuotation_Status", "\"Status\" IN ('RECORDED', 'VOID')");
                    table.CheckConstraint("CK_PrcQuotation_TaxPercent", "\"TaxPercent\" >= 0 AND \"TaxPercent\" <= 100");
                    table.CheckConstraint("CK_PrcQuotation_VoidReason", "\"Status\" <> 'VOID' OR length(btrim(coalesce(\"VoidReason\", ''))) > 0");
                    table.ForeignKey(
                        name: "FK_PrcQuotation_MstSupplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcQuotation_PrcPackage_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "public",
                        principalTable: "PrcPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcQuotation_PrcQuotation_SourceQuotationId",
                        column: x => x.SourceQuotationId,
                        principalSchema: "public",
                        principalTable: "PrcQuotation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcQuotation_PrcRfq_RfqId",
                        column: x => x.RfqId,
                        principalSchema: "public",
                        principalTable: "PrcRfq",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcRfqInvitation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RfqId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    WasRestricted = table.Column<bool>(type: "boolean", nullable: false),
                    RestrictedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PrcRfqInvitation", x => x.Id);
                    table.CheckConstraint("CK_PrcRfqInvitation_RestrictedReason", "NOT \"WasRestricted\" OR length(btrim(coalesce(\"RestrictedReason\", ''))) > 0");
                    table.ForeignKey(
                        name: "FK_PrcRfqInvitation_MstSupplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcRfqInvitation_PrcRfq_RfqId",
                        column: x => x.RfqId,
                        principalSchema: "public",
                        principalTable: "PrcRfq",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcNegotiation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoundNo = table.Column<int>(type: "integer", nullable: false),
                    NegotiatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Participants = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
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
                    table.PrimaryKey("PK_PrcNegotiation", x => x.Id);
                    table.CheckConstraint("CK_PrcNegotiation_RoundNo", "\"RoundNo\" >= 1");
                    table.ForeignKey(
                        name: "FK_PrcNegotiation_PrcQuotation_QuotationId",
                        column: x => x.QuotationId,
                        principalSchema: "public",
                        principalTable: "PrcQuotation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcPackageAward",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AwardTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HpsTotalSnapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsOverHps = table.Column<bool>(type: "boolean", nullable: false),
                    OverHpsReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsLowest = table.Column<bool>(type: "boolean", nullable: false),
                    NonLowestReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsRestrictedVendor = table.Column<bool>(type: "boolean", nullable: false),
                    RestrictedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RankingSnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmissionCount = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RevokeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NotAppliedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_PrcPackageAward", x => x.Id);
                    table.CheckConstraint("CK_PrcPackageAward_NonLowestReason", "\"IsLowest\" OR length(btrim(coalesce(\"NonLowestReason\", ''))) > 0");
                    table.CheckConstraint("CK_PrcPackageAward_NotAppliedReason", "\"Status\" <> 'NOT_APPLIED' OR length(btrim(coalesce(\"NotAppliedReason\", ''))) > 0");
                    table.CheckConstraint("CK_PrcPackageAward_OverHpsReason", "NOT \"IsOverHps\" OR length(btrim(coalesce(\"OverHpsReason\", ''))) > 0");
                    table.CheckConstraint("CK_PrcPackageAward_RestrictedReason", "NOT \"IsRestrictedVendor\" OR length(btrim(coalesce(\"RestrictedReason\", ''))) > 0");
                    table.CheckConstraint("CK_PrcPackageAward_RevokeReason", "\"Status\" <> 'REVOKED' OR length(btrim(coalesce(\"RevokeReason\", ''))) > 0");
                    table.CheckConstraint("CK_PrcPackageAward_Status", "\"Status\" IN ('PENDING_APPROVAL', 'RETURNED', 'APPROVED', 'REJECTED', 'WITHDRAWN', 'REVOKED', 'NOT_APPLIED')");
                    table.CheckConstraint("CK_PrcPackageAward_SubmissionCount", "\"SubmissionCount\" >= 1");
                    table.ForeignKey(
                        name: "FK_PrcPackageAward_MstSupplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPackageAward_PrcPackage_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "public",
                        principalTable: "PrcPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPackageAward_PrcQuotation_QuotationId",
                        column: x => x.QuotationId,
                        principalSchema: "public",
                        principalTable: "PrcQuotation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcQuotationLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferedUnitPriceBeforeTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CurrentUnitPriceBeforeTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    FinalUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    LineAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SourceQuotationLineId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_PrcQuotationLine", x => x.Id);
                    table.CheckConstraint("CK_PrcQuotationLine_CurrentPrice", "\"CurrentUnitPriceBeforeTax\" > 0 AND \"CurrentUnitPriceBeforeTax\" <= \"OfferedUnitPriceBeforeTax\"");
                    table.CheckConstraint("CK_PrcQuotationLine_OfferedPrice", "\"OfferedUnitPriceBeforeTax\" > 0");
                    table.ForeignKey(
                        name: "FK_PrcQuotationLine_PrcPackageItem_PackageItemId",
                        column: x => x.PackageItemId,
                        principalSchema: "public",
                        principalTable: "PrcPackageItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcQuotationLine_PrcQuotationLine_SourceQuotationLineId",
                        column: x => x.SourceQuotationLineId,
                        principalSchema: "public",
                        principalTable: "PrcQuotationLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcQuotationLine_PrcQuotation_QuotationId",
                        column: x => x.QuotationId,
                        principalSchema: "public",
                        principalTable: "PrcQuotation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcNegotiationLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NegotiationId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousUnitPriceBeforeTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NewUnitPriceBeforeTax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_PrcNegotiationLine", x => x.Id);
                    table.CheckConstraint("CK_PrcNegotiationLine_NewPrice", "\"NewUnitPriceBeforeTax\" > 0 AND \"NewUnitPriceBeforeTax\" <= \"PreviousUnitPriceBeforeTax\"");
                    table.ForeignKey(
                        name: "FK_PrcNegotiationLine_PrcNegotiation_NegotiationId",
                        column: x => x.NegotiationId,
                        principalSchema: "public",
                        principalTable: "PrcNegotiation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcNegotiationLine_PrcQuotationLine_QuotationLineId",
                        column: x => x.QuotationLineId,
                        principalSchema: "public",
                        principalTable: "PrcQuotationLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcTechnicalEvaluationResult",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotationLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Result = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceResultId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_PrcTechnicalEvaluationResult", x => x.Id);
                    table.CheckConstraint("CK_PrcTechnicalEvaluationResult_FailReason", "\"Result\" <> 'FAIL' OR length(btrim(coalesce(\"Reason\", ''))) > 0");
                    table.CheckConstraint("CK_PrcTechnicalEvaluationResult_Result", "\"Result\" IN ('PASS', 'FAIL')");
                    table.ForeignKey(
                        name: "FK_PrcTechnicalEvaluationResult_PrcQuotationLine_QuotationLine~",
                        column: x => x.QuotationLineId,
                        principalSchema: "public",
                        principalTable: "PrcQuotationLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcTechnicalEvaluationResult_PrcTechnicalCriterion_Criterio~",
                        column: x => x.CriterionId,
                        principalSchema: "public",
                        principalTable: "PrcTechnicalCriterion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcTechnicalEvaluationResult_PrcTechnicalEvaluationResult_S~",
                        column: x => x.SourceResultId,
                        principalSchema: "public",
                        principalTable: "PrcTechnicalEvaluationResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrcNegotiation_QuotationId_RoundNo",
                schema: "public",
                table: "PrcNegotiation",
                columns: new[] { "QuotationId", "RoundNo" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNegotiationLine_NegotiationId_QuotationLineId",
                schema: "public",
                table: "PrcNegotiationLine",
                columns: new[] { "NegotiationId", "QuotationLineId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNegotiationLine_QuotationLineId",
                schema: "public",
                table: "PrcNegotiationLine",
                column: "QuotationLineId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackage_FiscalYear",
                schema: "public",
                table: "PrcPackage",
                column: "FiscalYear");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackage_MethodId",
                schema: "public",
                table: "PrcPackage",
                column: "MethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackage_PackageNumber",
                schema: "public",
                table: "PrcPackage",
                column: "PackageNumber",
                unique: true,
                filter: "\"PackageNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackage_PackagePlanId",
                schema: "public",
                table: "PrcPackage",
                column: "PackagePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackage_PicUserId",
                schema: "public",
                table: "PrcPackage",
                column: "PicUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackage_PreparationWorkflowInstanceId",
                schema: "public",
                table: "PrcPackage",
                column: "PreparationWorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackage_ProcurementTypeId",
                schema: "public",
                table: "PrcPackage",
                column: "ProcurementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackage_PurchaseOrderNumber",
                schema: "public",
                table: "PrcPackage",
                column: "PurchaseOrderNumber",
                unique: true,
                filter: "\"PurchaseOrderNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackage_SplitFromPackageId",
                schema: "public",
                table: "PrcPackage",
                column: "SplitFromPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackage_Status",
                schema: "public",
                table: "PrcPackage",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageAward_LivePerPackage",
                schema: "public",
                table: "PrcPackageAward",
                column: "PackageId",
                unique: true,
                filter: "\"Status\" IN ('PENDING_APPROVAL', 'RETURNED', 'APPROVED') AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageAward_QuotationId",
                schema: "public",
                table: "PrcPackageAward",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageAward_Status",
                schema: "public",
                table: "PrcPackageAward",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageAward_SupplierId",
                schema: "public",
                table: "PrcPackageAward",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageAward_WorkflowInstanceId",
                schema: "public",
                table: "PrcPackageAward",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageChecklistItem_DocumentTypeId",
                schema: "public",
                table: "PrcPackageChecklistItem",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageChecklistItem_PackageId_DocumentTypeId",
                schema: "public",
                table: "PrcPackageChecklistItem",
                columns: new[] { "PackageId", "DocumentTypeId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageEvaluator_ActivePackageEvaluator",
                schema: "public",
                table: "PrcPackageEvaluator",
                columns: new[] { "PackageId", "EvaluatorUserId" },
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageEvaluator_EvaluatorUserId",
                schema: "public",
                table: "PrcPackageEvaluator",
                column: "EvaluatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageItem_ItemCategoryId",
                schema: "public",
                table: "PrcPackageItem",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageItem_ItemId",
                schema: "public",
                table: "PrcPackageItem",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageItem_PackageId_ItemId",
                schema: "public",
                table: "PrcPackageItem",
                columns: new[] { "PackageId", "ItemId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageItemSource_DepartmentId",
                schema: "public",
                table: "PrcPackageItemSource",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageItemSource_LiveRequestLine",
                schema: "public",
                table: "PrcPackageItemSource",
                column: "PurchaseRequestLineId",
                unique: true,
                filter: "\"IsReleased\" = false AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageItemSource_PackageItemId",
                schema: "public",
                table: "PrcPackageItemSource",
                column: "PackageItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackageItemSource_PurchaseRequestId",
                schema: "public",
                table: "PrcPackageItemSource",
                column: "PurchaseRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcQuotation_PackageId",
                schema: "public",
                table: "PrcQuotation",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcQuotation_RecordedByUserId",
                schema: "public",
                table: "PrcQuotation",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcQuotation_RecordedPerRfqSupplier",
                schema: "public",
                table: "PrcQuotation",
                columns: new[] { "RfqId", "SupplierId" },
                unique: true,
                filter: "\"Status\" = 'RECORDED' AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcQuotation_SourceQuotationId",
                schema: "public",
                table: "PrcQuotation",
                column: "SourceQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcQuotation_SupplierId",
                schema: "public",
                table: "PrcQuotation",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcQuotationLine_PackageItemId",
                schema: "public",
                table: "PrcQuotationLine",
                column: "PackageItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcQuotationLine_QuotationId_PackageItemId",
                schema: "public",
                table: "PrcQuotationLine",
                columns: new[] { "QuotationId", "PackageItemId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcQuotationLine_SourceQuotationLineId",
                schema: "public",
                table: "PrcQuotationLine",
                column: "SourceQuotationLineId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcRfq_InheritedFromRfqId",
                schema: "public",
                table: "PrcRfq",
                column: "InheritedFromRfqId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcRfq_OpenPerPackage",
                schema: "public",
                table: "PrcRfq",
                column: "PackageId",
                unique: true,
                filter: "\"Status\" IN ('DRAFT', 'SENT') AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcRfq_RfqNumber",
                schema: "public",
                table: "PrcRfq",
                column: "RfqNumber",
                unique: true,
                filter: "\"RfqNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcRfq_Status",
                schema: "public",
                table: "PrcRfq",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PrcRfqInvitation_RfqId_SupplierId",
                schema: "public",
                table: "PrcRfqInvitation",
                columns: new[] { "RfqId", "SupplierId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcRfqInvitation_SupplierId",
                schema: "public",
                table: "PrcRfqInvitation",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcTechnicalCriterion_PackageItemId_CriterionNo",
                schema: "public",
                table: "PrcTechnicalCriterion",
                columns: new[] { "PackageItemId", "CriterionNo" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcTechnicalEvaluationResult_CriterionId",
                schema: "public",
                table: "PrcTechnicalEvaluationResult",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcTechnicalEvaluationResult_EvaluatorUserId",
                schema: "public",
                table: "PrcTechnicalEvaluationResult",
                column: "EvaluatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcTechnicalEvaluationResult_LineCriterionEvaluator",
                schema: "public",
                table: "PrcTechnicalEvaluationResult",
                columns: new[] { "QuotationLineId", "CriterionId", "EvaluatorUserId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcTechnicalEvaluationResult_SourceResultId",
                schema: "public",
                table: "PrcTechnicalEvaluationResult",
                column: "SourceResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrcNegotiationLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcPackageAward",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcPackageChecklistItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcPackageEvaluator",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcPackageItemSource",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcRfqInvitation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcTechnicalEvaluationResult",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcNegotiation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcQuotationLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcTechnicalCriterion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcQuotation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcPackageItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcRfq",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcPackage",
                schema: "public");
        }
    }
}
