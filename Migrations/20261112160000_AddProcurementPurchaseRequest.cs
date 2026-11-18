using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-PBJ-006: permintaan pengadaan (PR) dan barisnya. Dua tabel baru; FK ke MstDepartment,
    /// MstProcurementFundSource, dan MstProcurementItem dengan Restrict; baris ikut terhapus bila
    /// kepala PR dihapus fisik (Cascade). Tidak mengubah tabel lain, aman tanpa downtime.
    /// Mundur: Down menghapus kedua tabel (data PR ikut hilang).
    /// </summary>
    public partial class AddProcurementPurchaseRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrcPurchaseRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FiscalYear = table.Column<int>(type: "integer", nullable: false),
                    FundSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    NeededByDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UrgencyLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UrgencyReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UrgencyDeadline = table.Column<DateOnly>(type: "date", nullable: true),
                    Justification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IsOutOfPlan = table.Column<bool>(type: "boolean", nullable: false),
                    RequestedTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PurchaseTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsOverBudget = table.Column<bool>(type: "boolean", nullable: false),
                    OverBudgetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SubmissionCount = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConvertedPackageId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PrcPurchaseRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrcPurchaseRequest_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPurchaseRequest_MstProcurementFundSource_FundSourceId",
                        column: x => x.FundSourceId,
                        principalSchema: "public",
                        principalTable: "MstProcurementFundSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcPurchaseRequestLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchaseRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNo = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Specification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RequestedQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    EstimatedUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NeedPlanLineKey = table.Column<Guid>(type: "uuid", nullable: true),
                    InPlanQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OutOfPlanQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OutOfPlanReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    StockAvailableQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    StockFulfilledQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    StockSourceNote = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    StockCheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StockCheckedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PurchaseAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_PrcPurchaseRequestLine", x => x.Id);
                    table.CheckConstraint("CK_PrcPurchaseRequestLine_Quantity", "\"RequestedQuantity\" > 0");
                    table.ForeignKey(
                        name: "FK_PrcPurchaseRequestLine_MstProcurementItem_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "public",
                        principalTable: "MstProcurementItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPurchaseRequestLine_PrcPurchaseRequest_PurchaseRequestId",
                        column: x => x.PurchaseRequestId,
                        principalSchema: "public",
                        principalTable: "PrcPurchaseRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequest_DepartmentId",
                schema: "public",
                table: "PrcPurchaseRequest",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequest_FiscalYear",
                schema: "public",
                table: "PrcPurchaseRequest",
                column: "FiscalYear");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequest_FundSourceId",
                schema: "public",
                table: "PrcPurchaseRequest",
                column: "FundSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequest_RequesterUserId",
                schema: "public",
                table: "PrcPurchaseRequest",
                column: "RequesterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequest_RequestNumber",
                schema: "public",
                table: "PrcPurchaseRequest",
                column: "RequestNumber",
                unique: true,
                filter: "\"RequestNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequest_Status_DepartmentId",
                schema: "public",
                table: "PrcPurchaseRequest",
                columns: new[] { "Status", "DepartmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequest_UrgencyLevel",
                schema: "public",
                table: "PrcPurchaseRequest",
                column: "UrgencyLevel");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequest_WorkflowInstanceId",
                schema: "public",
                table: "PrcPurchaseRequest",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequestLine_ItemId",
                schema: "public",
                table: "PrcPurchaseRequestLine",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequestLine_NeedPlanLineKey",
                schema: "public",
                table: "PrcPurchaseRequestLine",
                column: "NeedPlanLineKey");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPurchaseRequestLine_PurchaseRequestId_LineNo",
                schema: "public",
                table: "PrcPurchaseRequestLine",
                columns: new[] { "PurchaseRequestId", "LineNo" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PrcPurchaseRequestLine", schema: "public");
            migrationBuilder.DropTable(name: "PrcPurchaseRequest", schema: "public");
        }
    }
}
