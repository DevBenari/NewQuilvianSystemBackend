using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollInstallmentDeductionAndCutover : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Kolom MaxNetSalaryPercentage pada MstDeductionType (FIN-DEC-217, BE-FIN-110)
            migrationBuilder.AddColumn<decimal>(
                name: "MaxNetSalaryPercentage",
                schema: "public",
                table: "MstDeductionType",
                type: "numeric(9,4)",
                precision: 9,
                scale: 4,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstDeductionType_MaxNetSalaryPercentage",
                schema: "public",
                table: "MstDeductionType",
                sql: "\"MaxNetSalaryPercentage\" IS NULL OR (\"MaxNetSalaryPercentage\" >= 0 AND \"MaxNetSalaryPercentage\" <= 100)");

            // 2. Tabel MstPayrollDeductionCutover (FIN-DEC-222, BE-FIN-110, BE-FIN-115)
            migrationBuilder.CreateTable(
                name: "MstPayrollDeductionCutover",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AutomaticFromPeriod = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    ChangeReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstPayrollDeductionCutover", x => x.Id);
                    table.CheckConstraint("CK_MstPayrollDeductionCutover_Period", "\"AutomaticFromPeriod\" ~ '^\\d{4}-\\d{2}$'");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstPayrollDeductionCutover_Active",
                schema: "public",
                table: "MstPayrollDeductionCutover",
                column: "IsActive",
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPayrollDeductionCutover",
                schema: "public");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstDeductionType_MaxNetSalaryPercentage",
                schema: "public",
                table: "MstDeductionType");

            migrationBuilder.DropColumn(
                name: "MaxNetSalaryPercentage",
                schema: "public",
                table: "MstDeductionType");
        }
    }
}

