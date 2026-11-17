using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddProcurementBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrcBudget",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FiscalYear = table.Column<int>(type: "integer", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    FundSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VersionNo = table.Column<int>(type: "integer", nullable: false),
                    PreviousVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    LineageKey = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCurrentVersion = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    RevisionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_PrcBudget", x => x.Id);
                    table.CheckConstraint("CK_PrcBudget_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_PrcBudget_MstDepartment_DepartmentId",
                        column: x => x.DepartmentId,
                        principalSchema: "public",
                        principalTable: "MstDepartment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcBudget_MstProcurementFundSource_FundSourceId",
                        column: x => x.FundSourceId,
                        principalSchema: "public",
                        principalTable: "MstProcurementFundSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcBudget_MstProcurementItemCategory_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalSchema: "public",
                        principalTable: "MstProcurementItemCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrcBudget_PrcBudget_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalSchema: "public",
                        principalTable: "PrcBudget",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrcBudgetCommitment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetLineageKey = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDocumentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SourceDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_PrcBudgetCommitment", x => x.Id);
                    table.CheckConstraint("CK_PrcBudgetCommitment_Amount", "\"Amount\" >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudget_CurrentVersion",
                schema: "public",
                table: "PrcBudget",
                columns: new[] { "FiscalYear", "DepartmentId", "ItemCategoryId", "FundSourceId" },
                unique: true,
                filter: "\"IsCurrentVersion\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudget_DepartmentId",
                schema: "public",
                table: "PrcBudget",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudget_FiscalYear",
                schema: "public",
                table: "PrcBudget",
                column: "FiscalYear");

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudget_FundSourceId",
                schema: "public",
                table: "PrcBudget",
                column: "FundSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudget_ItemCategoryId",
                schema: "public",
                table: "PrcBudget",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudget_LineageKey_VersionNo",
                schema: "public",
                table: "PrcBudget",
                columns: new[] { "LineageKey", "VersionNo" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudget_PreviousVersionId",
                schema: "public",
                table: "PrcBudget",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudget_Status",
                schema: "public",
                table: "PrcBudget",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudget_WorkflowInstanceId",
                schema: "public",
                table: "PrcBudget",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudgetCommitment_ActiveSource",
                schema: "public",
                table: "PrcBudgetCommitment",
                columns: new[] { "BudgetLineageKey", "SourceDocumentType", "SourceDocumentId" },
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudgetCommitment_BudgetLineageKey",
                schema: "public",
                table: "PrcBudgetCommitment",
                column: "BudgetLineageKey");

            migrationBuilder.CreateIndex(
                name: "IX_PrcBudgetCommitment_SourceDocumentType_SourceDocumentId",
                schema: "public",
                table: "PrcBudgetCommitment",
                columns: new[] { "SourceDocumentType", "SourceDocumentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrcBudget",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PrcBudgetCommitment",
                schema: "public");
        }
    }
}
