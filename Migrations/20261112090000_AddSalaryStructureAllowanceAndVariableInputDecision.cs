using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// PAY-BE-14 (blueprint payroll-gaji PGJ-C1). Hanya penambahan:
    /// 1. tabel MstSalaryStructureAllowance (tunjangan tetap per Struktur Gaji, PRL-DEC-02);
    /// 2. lima kolom keputusan input manual di PrlPayrollVariableInput (PRL-DEC-07).
    /// Tidak mengubah, mengganti nama, atau menghapus tabel/kolom yang sudah ada; baris lama tidak perlu diisi.
    /// Ditulis manual (dotnet ef tidak dijalankan agen); prasyarat: 20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix.
    /// </summary>
    public partial class AddSalaryStructureAllowanceAndVariableInputDecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                schema: "public",
                table: "PrlPayrollVariableInput",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionReason",
                schema: "public",
                table: "PrlPayrollVariableInput",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                schema: "public",
                table: "PrlPayrollVariableInput",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstSalaryStructureAllowance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SalaryStructureId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowanceTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CalculationMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "FixedAmount"),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Percentage = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstSalaryStructureAllowance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstSalaryStructureAllowance_MstAllowanceType_AllowanceTypeId",
                        column: x => x.AllowanceTypeId,
                        principalSchema: "public",
                        principalTable: "MstAllowanceType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstSalaryStructureAllowance_MstSalaryStructure_SalaryStructureId",
                        column: x => x.SalaryStructureId,
                        principalSchema: "public",
                        principalTable: "MstSalaryStructure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrlPayrollVariableInput_ApprovedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrlPayrollVariableInput_RejectedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput",
                column: "RejectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MstSalaryStructureAllowance_AllowanceTypeId",
                schema: "public",
                table: "MstSalaryStructureAllowance",
                column: "AllowanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstSalaryStructureAllowance_SalaryStructureId_AllowanceTypeId",
                schema: "public",
                table: "MstSalaryStructureAllowance",
                columns: new[] { "SalaryStructureId", "AllowanceTypeId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_PrlPayrollVariableInput_AspNetUsers_ApprovedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput",
                column: "ApprovedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PrlPayrollVariableInput_AspNetUsers_RejectedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput",
                column: "RejectedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrlPayrollVariableInput_AspNetUsers_ApprovedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput");

            migrationBuilder.DropForeignKey(
                name: "FK_PrlPayrollVariableInput_AspNetUsers_RejectedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput");

            migrationBuilder.DropTable(
                name: "MstSalaryStructureAllowance",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_PrlPayrollVariableInput_ApprovedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput");

            migrationBuilder.DropIndex(
                name: "IX_PrlPayrollVariableInput_RejectedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                schema: "public",
                table: "PrlPayrollVariableInput");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput");

            migrationBuilder.DropColumn(
                name: "DecisionReason",
                schema: "public",
                table: "PrlPayrollVariableInput");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                schema: "public",
                table: "PrlPayrollVariableInput");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                schema: "public",
                table: "PrlPayrollVariableInput");
        }
    }
}
