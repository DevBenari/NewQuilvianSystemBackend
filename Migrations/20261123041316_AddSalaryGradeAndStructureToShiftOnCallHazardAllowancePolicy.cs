using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryGradeAndStructureToShiftOnCallHazardAllowancePolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeGradeId",
                schema: "public",
                table: "MstShiftAllowancePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalaryGradeId",
                schema: "public",
                table: "MstShiftAllowancePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalaryStructureId",
                schema: "public",
                table: "MstShiftAllowancePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeGradeId",
                schema: "public",
                table: "MstOnCallAllowancePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalaryGradeId",
                schema: "public",
                table: "MstOnCallAllowancePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalaryStructureId",
                schema: "public",
                table: "MstOnCallAllowancePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeGradeId",
                schema: "public",
                table: "MstHazardAllowancePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalaryGradeId",
                schema: "public",
                table: "MstHazardAllowancePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalaryStructureId",
                schema: "public",
                table: "MstHazardAllowancePolicy",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstShiftAllowancePolicy_EmployeeGradeId",
                schema: "public",
                table: "MstShiftAllowancePolicy",
                column: "EmployeeGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstShiftAllowancePolicy_SalaryGrade_SalaryStructure",
                schema: "public",
                table: "MstShiftAllowancePolicy",
                columns: new[] { "SalaryGradeId", "SalaryStructureId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstShiftAllowancePolicy_SalaryStructureId",
                schema: "public",
                table: "MstShiftAllowancePolicy",
                column: "SalaryStructureId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstShiftAllowancePolicy_StructureRequiresGrade",
                schema: "public",
                table: "MstShiftAllowancePolicy",
                sql: "\"SalaryStructureId\" IS NULL OR \"SalaryGradeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnCallAllowancePolicy_EmployeeGradeId",
                schema: "public",
                table: "MstOnCallAllowancePolicy",
                column: "EmployeeGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstOnCallAllowancePolicy_SalaryGrade_SalaryStructure",
                schema: "public",
                table: "MstOnCallAllowancePolicy",
                columns: new[] { "SalaryGradeId", "SalaryStructureId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstOnCallAllowancePolicy_SalaryStructureId",
                schema: "public",
                table: "MstOnCallAllowancePolicy",
                column: "SalaryStructureId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstOnCallAllowancePolicy_StructureRequiresGrade",
                schema: "public",
                table: "MstOnCallAllowancePolicy",
                sql: "\"SalaryStructureId\" IS NULL OR \"SalaryGradeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstHazardAllowancePolicy_EmployeeGradeId",
                schema: "public",
                table: "MstHazardAllowancePolicy",
                column: "EmployeeGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstHazardAllowancePolicy_SalaryGrade_SalaryStructure",
                schema: "public",
                table: "MstHazardAllowancePolicy",
                columns: new[] { "SalaryGradeId", "SalaryStructureId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstHazardAllowancePolicy_SalaryStructureId",
                schema: "public",
                table: "MstHazardAllowancePolicy",
                column: "SalaryStructureId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstHazardAllowancePolicy_StructureRequiresGrade",
                schema: "public",
                table: "MstHazardAllowancePolicy",
                sql: "\"SalaryStructureId\" IS NULL OR \"SalaryGradeId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_MstHazardAllowancePolicy_EmployeeGrade",
                schema: "public",
                table: "MstHazardAllowancePolicy",
                column: "EmployeeGradeId",
                principalSchema: "public",
                principalTable: "MstEmployeeGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstHazardAllowancePolicy_SalaryGrade",
                schema: "public",
                table: "MstHazardAllowancePolicy",
                column: "SalaryGradeId",
                principalSchema: "public",
                principalTable: "MstSalaryGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstHazardAllowancePolicy_SalaryStructure",
                schema: "public",
                table: "MstHazardAllowancePolicy",
                column: "SalaryStructureId",
                principalSchema: "public",
                principalTable: "MstSalaryStructure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnCallAllowancePolicy_EmployeeGrade",
                schema: "public",
                table: "MstOnCallAllowancePolicy",
                column: "EmployeeGradeId",
                principalSchema: "public",
                principalTable: "MstEmployeeGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnCallAllowancePolicy_SalaryGrade",
                schema: "public",
                table: "MstOnCallAllowancePolicy",
                column: "SalaryGradeId",
                principalSchema: "public",
                principalTable: "MstSalaryGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstOnCallAllowancePolicy_SalaryStructure",
                schema: "public",
                table: "MstOnCallAllowancePolicy",
                column: "SalaryStructureId",
                principalSchema: "public",
                principalTable: "MstSalaryStructure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstShiftAllowancePolicy_EmployeeGrade",
                schema: "public",
                table: "MstShiftAllowancePolicy",
                column: "EmployeeGradeId",
                principalSchema: "public",
                principalTable: "MstEmployeeGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstShiftAllowancePolicy_SalaryGrade",
                schema: "public",
                table: "MstShiftAllowancePolicy",
                column: "SalaryGradeId",
                principalSchema: "public",
                principalTable: "MstSalaryGrade",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstShiftAllowancePolicy_SalaryStructure",
                schema: "public",
                table: "MstShiftAllowancePolicy",
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
                name: "FK_MstHazardAllowancePolicy_EmployeeGrade",
                schema: "public",
                table: "MstHazardAllowancePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstHazardAllowancePolicy_SalaryGrade",
                schema: "public",
                table: "MstHazardAllowancePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstHazardAllowancePolicy_SalaryStructure",
                schema: "public",
                table: "MstHazardAllowancePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnCallAllowancePolicy_EmployeeGrade",
                schema: "public",
                table: "MstOnCallAllowancePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnCallAllowancePolicy_SalaryGrade",
                schema: "public",
                table: "MstOnCallAllowancePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstOnCallAllowancePolicy_SalaryStructure",
                schema: "public",
                table: "MstOnCallAllowancePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstShiftAllowancePolicy_EmployeeGrade",
                schema: "public",
                table: "MstShiftAllowancePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstShiftAllowancePolicy_SalaryGrade",
                schema: "public",
                table: "MstShiftAllowancePolicy");

            migrationBuilder.DropForeignKey(
                name: "FK_MstShiftAllowancePolicy_SalaryStructure",
                schema: "public",
                table: "MstShiftAllowancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstShiftAllowancePolicy_EmployeeGradeId",
                schema: "public",
                table: "MstShiftAllowancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstShiftAllowancePolicy_SalaryGrade_SalaryStructure",
                schema: "public",
                table: "MstShiftAllowancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstShiftAllowancePolicy_SalaryStructureId",
                schema: "public",
                table: "MstShiftAllowancePolicy");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstShiftAllowancePolicy_StructureRequiresGrade",
                schema: "public",
                table: "MstShiftAllowancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstOnCallAllowancePolicy_EmployeeGradeId",
                schema: "public",
                table: "MstOnCallAllowancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstOnCallAllowancePolicy_SalaryGrade_SalaryStructure",
                schema: "public",
                table: "MstOnCallAllowancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstOnCallAllowancePolicy_SalaryStructureId",
                schema: "public",
                table: "MstOnCallAllowancePolicy");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstOnCallAllowancePolicy_StructureRequiresGrade",
                schema: "public",
                table: "MstOnCallAllowancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstHazardAllowancePolicy_EmployeeGradeId",
                schema: "public",
                table: "MstHazardAllowancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstHazardAllowancePolicy_SalaryGrade_SalaryStructure",
                schema: "public",
                table: "MstHazardAllowancePolicy");

            migrationBuilder.DropIndex(
                name: "IX_MstHazardAllowancePolicy_SalaryStructureId",
                schema: "public",
                table: "MstHazardAllowancePolicy");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstHazardAllowancePolicy_StructureRequiresGrade",
                schema: "public",
                table: "MstHazardAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "EmployeeGradeId",
                schema: "public",
                table: "MstShiftAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "SalaryGradeId",
                schema: "public",
                table: "MstShiftAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "SalaryStructureId",
                schema: "public",
                table: "MstShiftAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "EmployeeGradeId",
                schema: "public",
                table: "MstOnCallAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "SalaryGradeId",
                schema: "public",
                table: "MstOnCallAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "SalaryStructureId",
                schema: "public",
                table: "MstOnCallAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "EmployeeGradeId",
                schema: "public",
                table: "MstHazardAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "SalaryGradeId",
                schema: "public",
                table: "MstHazardAllowancePolicy");

            migrationBuilder.DropColumn(
                name: "SalaryStructureId",
                schema: "public",
                table: "MstHazardAllowancePolicy");
        }
    }
}
