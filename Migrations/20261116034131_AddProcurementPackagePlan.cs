using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementPackagePlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrcPackagePlan",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackagePlanNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FiscalYear = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcurementTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicativeMethod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TargetMonth = table.Column<int>(type: "integer", nullable: true),
                    EstimatedValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PicUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CancelReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PrcPackagePlan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrcPackagePlan_MstProcurementType_ProcurementTypeId",
                        column: x => x.ProcurementTypeId,
                        principalSchema: "public",
                        principalTable: "MstProcurementType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcPackagePlanLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PackagePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    NeedPlanLineKey = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocatedQuantity = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
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
                    table.PrimaryKey("PK_PrcPackagePlanLine", x => x.Id);
                    table.CheckConstraint("CK_PrcPackagePlanLine_AllocatedQuantity", "\"AllocatedQuantity\" > 0");
                    table.ForeignKey(
                        name: "FK_PrcPackagePlanLine_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPackagePlanLine_MstProcurementItem_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "public",
                        principalTable: "MstProcurementItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcPackagePlanLine_PrcPackagePlan_PackagePlanId",
                        column: x => x.PackagePlanId,
                        principalSchema: "public",
                        principalTable: "PrcPackagePlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackagePlan_FiscalYear",
                schema: "public",
                table: "PrcPackagePlan",
                column: "FiscalYear");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackagePlan_PackagePlanNumber",
                schema: "public",
                table: "PrcPackagePlan",
                column: "PackagePlanNumber",
                unique: true,
                filter: "\"PackagePlanNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackagePlan_PicUserId",
                schema: "public",
                table: "PrcPackagePlan",
                column: "PicUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackagePlan_ProcurementTypeId",
                schema: "public",
                table: "PrcPackagePlan",
                column: "ProcurementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackagePlan_Status",
                schema: "public",
                table: "PrcPackagePlan",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackagePlan_WorkflowInstanceId",
                schema: "public",
                table: "PrcPackagePlan",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackagePlanLine_DepartmentId",
                schema: "public",
                table: "PrcPackagePlanLine",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackagePlanLine_ItemId",
                schema: "public",
                table: "PrcPackagePlanLine",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackagePlanLine_NeedPlanLineKey",
                schema: "public",
                table: "PrcPackagePlanLine",
                column: "NeedPlanLineKey");

            migrationBuilder.CreateIndex(
                name: "IX_PrcPackagePlanLine_PackagePlanId_NeedPlanLineKey",
                schema: "public",
                table: "PrcPackagePlanLine",
                columns: new[] { "PackagePlanId", "NeedPlanLineKey" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrcPackagePlanLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcPackagePlan",
                schema: "public");
        }
    }
}
