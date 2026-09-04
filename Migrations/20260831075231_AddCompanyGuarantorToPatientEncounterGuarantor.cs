using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyGuarantorToPatientEncounterGuarantor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyGuarantorCodeSnapshot",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeNameSnapshot",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeNumberSnapshot",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "CompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "PatientCompanyGuarantorId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_MstCompanyGuarantor_CompanyGua~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "CompanyGuarantorId",
                principalSchema: "public",
                principalTable: "MstCompanyGuarantor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_MstPatientCompanyGuarantor_Pat~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                column: "PatientCompanyGuarantorId",
                principalSchema: "public",
                principalTable: "MstPatientCompanyGuarantor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_MstCompanyGuarantor_CompanyGua~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounterGuarantor_MstPatientCompanyGuarantor_Pat~",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "CompanyGuarantorCodeSnapshot",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "CompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "EmployeeNameSnapshot",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "EmployeeNumberSnapshot",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "PatientCompanyGuarantorId",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");
        }
    }
}
