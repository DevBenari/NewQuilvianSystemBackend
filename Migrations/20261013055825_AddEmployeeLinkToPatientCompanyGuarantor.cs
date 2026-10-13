using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeLinkToPatientCompanyGuarantor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BenefitRelationshipSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeIdSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyMemberIdSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BenefitRelationship",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EmployeeId",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyMemberId",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegPatientEncounterGuarantor_EmployeeIdSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor",
                column: "EmployeeIdSnapshot",
                filter: "\"EmployeeIdSnapshot\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_EmployeeId",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                column: "EmployeeId",
                filter: "\"EmployeeId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_FamilyMemberId",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                column: "FamilyMemberId",
                filter: "\"FamilyMemberId\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstPatientCompanyGuarantor_BenefitLink",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                sql: "(\"EmployeeId\" IS NULL AND \"FamilyMemberId\" IS NULL AND \"BenefitRelationship\" IS NULL) OR (\"EmployeeId\" IS NOT NULL AND \"BenefitRelationship\" IS NOT NULL AND ((\"BenefitRelationship\" = 'SELF') = (\"FamilyMemberId\" IS NULL)))");

            migrationBuilder.AddForeignKey(
                name: "FK_MstPatientCompanyGuarantor_EmployeeId",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                column: "EmployeeId",
                principalSchema: "public",
                principalTable: "MstEmployee",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstPatientCompanyGuarantor_FamilyMemberId",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                column: "FamilyMemberId",
                principalSchema: "public",
                principalTable: "WfpFamilyMember",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstPatientCompanyGuarantor_EmployeeId",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropForeignKey(
                name: "FK_MstPatientCompanyGuarantor_FamilyMemberId",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_RegPatientEncounterGuarantor_EmployeeIdSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_MstPatientCompanyGuarantor_EmployeeId",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_MstPatientCompanyGuarantor_FamilyMemberId",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstPatientCompanyGuarantor_BenefitLink",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropColumn(
                name: "BenefitRelationshipSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "EmployeeIdSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "FamilyMemberIdSnapshot",
                schema: "public",
                table: "RegPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "BenefitRelationship",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                schema: "public",
                table: "MstPatientCompanyGuarantor");

            migrationBuilder.DropColumn(
                name: "FamilyMemberId",
                schema: "public",
                table: "MstPatientCompanyGuarantor");
        }
    }
}
