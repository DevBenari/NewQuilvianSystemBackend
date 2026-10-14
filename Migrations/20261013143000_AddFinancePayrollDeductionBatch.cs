using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancePayrollDeductionBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinPayrollDeductionBatch",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeductionPeriod = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    PayrollPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "REGULER"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "DRAFT"),
                    ControlTotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDeductedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TotalRows = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    InvalidRows = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PayrollReportNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProofId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SourceFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinPayrollDeductionBatch", x => x.Id);
                    table.CheckConstraint("CK_FinPayrollDeductionBatch_Type", "\"BatchType\" IN ('REGULER','KOREKSI')");
                    table.CheckConstraint("CK_FinPayrollDeductionBatch_Status", "\"Status\" IN ('DRAFT','DIAJUKAN','DISAHKAN','DITOLAK','DIBATALKAN')");
                    table.CheckConstraint("CK_FinPayrollDeductionBatch_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"SubmittedBy\"");
                    table.CheckConstraint("CK_FinPayrollDeductionBatch_Correction", "\"BatchType\" <> 'KOREKSI' OR \"CorrectionReason\" IS NOT NULL");
                    table.CheckConstraint("CK_FinPayrollDeductionBatch_Period", "\"DeductionPeriod\" ~ '^[0-9]{4}-(0[1-9]|1[0-2])$'");
                    table.ForeignKey(
                        name: "FK_FinPayrollDeductionBatch_FinTransactionProof_ProofId",
                        column: x => x.ProofId,
                        principalSchema: "public",
                        principalTable: "FinTransactionProof",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinPayrollDeductionBatchItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    InstallmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstallmentCode = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    EmployeeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResultStatus = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DeductedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DueAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PreviousDeductedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    DeltaAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ValidationStatus = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "VALID"),
                    ValidationMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AppliedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinPayrollDeductionBatchItem", x => x.Id);
                    table.CheckConstraint("CK_FinPayrollDeductionBatchItem_Result", "\"ResultStatus\" IN ('BERHASIL','SEBAGIAN','GAGAL') AND \"DeductedAmount\" >= 0 AND (\"ResultStatus\" <> 'GAGAL' OR \"DeductedAmount\" = 0)");
                    table.CheckConstraint("CK_FinPayrollDeductionBatchItem_Validation", "\"ValidationStatus\" IN ('VALID','INVALID')");
                    table.ForeignKey(
                        name: "FK_FinPayrollDeductionBatchItem_FinPayrollDeductionBatch_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "public",
                        principalTable: "FinPayrollDeductionBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPayrollDeductionBatchItem_FinReceivableInstallment_InstallmentId",
                        column: x => x.InstallmentId,
                        principalSchema: "public",
                        principalTable: "FinReceivableInstallment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinPayrollDeductionBatch_BatchNumber",
                schema: "public",
                table: "FinPayrollDeductionBatch",
                column: "BatchNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinPayrollDeductionBatch_DeductionPeriod_Status",
                schema: "public",
                table: "FinPayrollDeductionBatch",
                columns: new[] { "DeductionPeriod", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FinPayrollDeductionBatch_ProofId",
                schema: "public",
                table: "FinPayrollDeductionBatch",
                column: "ProofId");

            migrationBuilder.CreateIndex(
                name: "UX_FinPayrollDeductionBatch_RegularPeriod",
                schema: "public",
                table: "FinPayrollDeductionBatch",
                column: "DeductionPeriod",
                unique: true,
                filter: "\"BatchType\" = 'REGULER' AND \"Status\" IN ('DRAFT','DIAJUKAN','DISAHKAN') AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinPayrollDeductionBatchItem_BatchId_RowNumber",
                schema: "public",
                table: "FinPayrollDeductionBatchItem",
                columns: new[] { "BatchId", "RowNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinPayrollDeductionBatchItem_InstallmentId",
                schema: "public",
                table: "FinPayrollDeductionBatchItem",
                column: "InstallmentId");

            migrationBuilder.CreateIndex(
                name: "UX_FinPayrollDeductionBatchItem_BatchInstallment",
                schema: "public",
                table: "FinPayrollDeductionBatchItem",
                columns: new[] { "BatchId", "InstallmentId" },
                unique: true,
                filter: "\"InstallmentId\" IS NOT NULL AND \"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinPayrollDeductionBatchItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinPayrollDeductionBatch",
                schema: "public");
        }
    }
}

