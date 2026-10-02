using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-021: Utang jasa tenaga medis (FinMedicalServicePayable, FinMedicalServicePayableItem)
    /// menggantikan rencana FinDoctorPayable/FinDoctorPayableItem (FIN-DES-025, FIN-CAP-021,
    /// 02-backend-architecture.md §A.5, data-dictionary.md §A.1..A.2).
    /// Menghubungkan relasi foreign key dari FinPaymentAllocation dan FinPayableAdjustment ke
    /// FinMedicalServicePayable.
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinanceMedicalServicePayable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinMedicalServicePayable",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayableNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PayeeType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "DOCTOR"),
                    PayeeReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceMedicalServiceFeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceApHandoffId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    AdjustedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "OUTSTANDING"),
                    RecognizedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_FinMedicalServicePayable", x => x.Id);
                    table.CheckConstraint("CK_FinMedicalServicePayable_PayeeType", "\"PayeeType\" IN ('DOCTOR','NURSE','OTHER_PRACTITIONER')");
                    table.CheckConstraint("CK_FinMedicalServicePayable_Status", "\"Status\" IN ('OUTSTANDING','PARTIAL','PAID','CANCELLED')");
                    table.CheckConstraint("CK_FinMedicalServicePayable_Outstanding", "\"OutstandingAmount\" >= 0");
                    table.CheckConstraint("CK_FinMedicalServicePayable_Balance", "\"OriginalAmount\" = \"OutstandingAmount\" + \"PaidAmount\" + \"AdjustedAmount\"");
                });

            migrationBuilder.CreateTable(
                name: "FinMedicalServicePayableItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayableId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceServiceFeeDetailId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_FinMedicalServicePayableItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinMedicalServicePayableItem_Payable",
                        column: x => x.PayableId,
                        principalSchema: "public",
                        principalTable: "FinMedicalServicePayable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinMedicalServicePayable_PayableNumber",
                schema: "public",
                table: "FinMedicalServicePayable",
                column: "PayableNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinMedicalServicePayable_SourceFee",
                schema: "public",
                table: "FinMedicalServicePayable",
                column: "SourceMedicalServiceFeeId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinMedicalServicePayable_Payee_Period",
                schema: "public",
                table: "FinMedicalServicePayable",
                columns: new[] { "PayeeType", "PayeeReferenceId", "PeriodCode" });

            migrationBuilder.CreateIndex(
                name: "IX_FinMedicalServicePayable_SourceApHandoffId",
                schema: "public",
                table: "FinMedicalServicePayable",
                column: "SourceApHandoffId");

            migrationBuilder.CreateIndex(
                name: "IX_FinMedicalServicePayable_Status",
                schema: "public",
                table: "FinMedicalServicePayable",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FinMedicalServicePayable_Outstanding",
                schema: "public",
                table: "FinMedicalServicePayable",
                column: "OutstandingAmount");

            migrationBuilder.CreateIndex(
                name: "IX_FinMedicalServicePayableItem_PayableId",
                schema: "public",
                table: "FinMedicalServicePayableItem",
                column: "PayableId");

            migrationBuilder.CreateIndex(
                name: "IX_FinMedicalServicePayableItem_SourceServiceFeeDetailId",
                schema: "public",
                table: "FinMedicalServicePayableItem",
                column: "SourceServiceFeeDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_FinPaymentAllocation_FinMedicalServicePayable_MedicalServicePayableId",
                schema: "public",
                table: "FinPaymentAllocation",
                column: "MedicalServicePayableId",
                principalSchema: "public",
                principalTable: "FinMedicalServicePayable",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinPayableAdjustment_FinMedicalServicePayable_MedicalServicePayableId",
                schema: "public",
                table: "FinPayableAdjustment",
                column: "MedicalServicePayableId",
                principalSchema: "public",
                principalTable: "FinMedicalServicePayable",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinPayableAdjustment_FinMedicalServicePayable_MedicalServicePayableId",
                schema: "public",
                table: "FinPayableAdjustment");

            migrationBuilder.DropForeignKey(
                name: "FK_FinPaymentAllocation_FinMedicalServicePayable_MedicalServicePayableId",
                schema: "public",
                table: "FinPaymentAllocation");

            migrationBuilder.DropTable(
                name: "FinMedicalServicePayableItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinMedicalServicePayable",
                schema: "public");
        }
    }
}
