using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryGradeAndStructureToTransportAllowancePolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SalaryGradeId",
                schema: "public",
                table: "PrlTransportAllowancePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalaryStructureId",
                schema: "public",
                table: "PrlTransportAllowancePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrlTransportAllowancePolicy_SalaryGrade_SalaryStructure",
                schema: "public",
                table: "PrlTransportAllowancePolicy",
                columns: new[] { "SalaryGradeId", "SalaryStructureId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_PrlTransportAllowancePolicy_SalaryStructureId",
                schema: "public",
                table: "PrlTransportAllowancePolicy",
                column: "SalaryStructureId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PrlTransportAllowancePolicy_StructureRequiresGrade",
                schema: "public",
                table: "PrlTransportAllowancePolicy",
                sql: "\"SalaryStructureId\" IS NULL OR \"SalaryGradeId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PrlTransportAllowancePolicy_SalaryGrade",
                schema: "public",
                table: "PrlTransportAllowancePolicy",
                column: "SalaryGradeId",
                principalSchema: "public",
                principalTable: "MstSalaryGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrlTransportAllowancePolicy_SalaryStructure",
                schema: "public",
                table: "PrlTransportAllowancePolicy",
                column: "SalaryStructureId",
                principalSchema: "public",
                principalTable: "MstSalaryStructure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrlTransportAllowancePolicy_SalaryGrade",
                schema: "public",
                table: "PrlTransportAllowancePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_PrlTransportAllowancePolicy_SalaryStructure",
                schema: "public",
                table: "PrlTransportAllowancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_PrlTransportAllowancePolicy_SalaryGrade_SalaryStructure",
                schema: "public",
                table: "PrlTransportAllowancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_PrlTransportAllowancePolicy_SalaryStructureId",
                schema: "public",
                table: "PrlTransportAllowancePolicy");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PrlTransportAllowancePolicy_StructureRequiresGrade",
                schema: "public",
                table: "PrlTransportAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "SalaryGradeId",
                schema: "public",
                table: "PrlTransportAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "SalaryStructureId",
                schema: "public",
                table: "PrlTransportAllowancePolicy");
        }
    }
}
