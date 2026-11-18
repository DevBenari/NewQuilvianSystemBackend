using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-PBJ-009 — rencana kebutuhan tahunan berversi (PBJ-BP-001, kamus data bagian 3): tabel
    /// <c>PrcNeedPlan</c> dan <c>PrcNeedPlanLine</c>. Hanya membuat tabel dan index baru; tidak
    /// mengubah tabel yang sudah ada.
    /// </summary>
    /// <remarks>
    /// Scaffold EF semula ikut memuat operasi modul lain karena model snapshot pada HEAD rusak oleh merge
    /// <c>27fac19c</c> (entity Pengadaan hilang). Snapshot dipulihkan dan operasi di luar rencana kebutuhan
    /// dibuang dari migration ini.
    /// </remarks>
    public partial class AddProcurementNeedPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrcNeedPlan",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FiscalYear = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNo = table.Column<int>(type: "integer", nullable: false),
                    PreviousVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCurrentVersion = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RevisionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EstimatedTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SubmissionCount = table.Column<int>(type: "integer", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PrcNeedPlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrcNeedPlan_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcNeedPlan_PrcNeedPlan_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalSchema: "public",
                        principalTable: "PrcNeedPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcNeedPlanLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NeedPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineKey = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    EstimatedUnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EstimatedTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NeededMonth = table.Column<int>(type: "integer", nullable: true),
                    FundSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Justification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Specification = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PrcNeedPlanLine", x => x.Id);
                    table.CheckConstraint("CK_PrcNeedPlanLine_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_PrcNeedPlanLine_MstProcurementFundSource_FundSourceId",
                        column: x => x.FundSourceId,
                        principalSchema: "public",
                        principalTable: "MstProcurementFundSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcNeedPlanLine_MstProcurementItem_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "public",
                        principalTable: "MstProcurementItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcNeedPlanLine_PrcNeedPlan_NeedPlanId",
                        column: x => x.NeedPlanId,
                        principalSchema: "public",
                        principalTable: "PrcNeedPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrcNeedPlan_CurrentVersion",
                schema: "public",
                table: "PrcNeedPlan",
                columns: new[] { "DepartmentId", "FiscalYear" },
                unique: true,
                filter: "\"IsCurrentVersion\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNeedPlan_FiscalYear",
                schema: "public",
                table: "PrcNeedPlan",
                column: "FiscalYear");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNeedPlan_PlanNumber_VersionNo",
                schema: "public",
                table: "PrcNeedPlan",
                columns: new[] { "PlanNumber", "VersionNo" },
                unique: true,
                filter: "\"PlanNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNeedPlan_PreviousVersionId",
                schema: "public",
                table: "PrcNeedPlan",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNeedPlan_Status",
                schema: "public",
                table: "PrcNeedPlan",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNeedPlan_WorkflowInstanceId",
                schema: "public",
                table: "PrcNeedPlan",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNeedPlanLine_FundSourceId",
                schema: "public",
                table: "PrcNeedPlanLine",
                column: "FundSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNeedPlanLine_ItemId",
                schema: "public",
                table: "PrcNeedPlanLine",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNeedPlanLine_LineKey",
                schema: "public",
                table: "PrcNeedPlanLine",
                column: "LineKey");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNeedPlanLine_NeedPlanId_ItemId",
                schema: "public",
                table: "PrcNeedPlanLine",
                columns: new[] { "NeedPlanId", "ItemId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcNeedPlanLine_NeedPlanId_LineKey",
                schema: "public",
                table: "PrcNeedPlanLine",
                columns: new[] { "NeedPlanId", "LineKey" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrcNeedPlanLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcNeedPlan",
                schema: "public");
        }
    }
}
