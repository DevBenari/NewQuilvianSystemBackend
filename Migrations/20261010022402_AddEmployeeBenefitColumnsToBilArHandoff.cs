using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeBenefitColumnsToBilArHandoff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilArHandoff_DebtorType",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.AddColumn<Guid>(
                name: "BenefitOwnerId",
                schema: "public",
                table: "BilArHandoff",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BenefitRelationship",
                schema: "public",
                table: "BilArHandoff",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilArHandoff_BenefitOwner",
                schema: "public",
                table: "BilArHandoff",
                sql: "(\"DebtorType\" = 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NOT NULL) OR (\"DebtorType\" <> 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilArHandoff_DebtorType",
                schema: "public",
                table: "BilArHandoff",
                sql: "\"DebtorType\" IN ('PATIENT_GUARANTOR','PAYER','EMPLOYEE_BENEFIT')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilArHandoff_BenefitOwner",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilArHandoff_DebtorType",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropColumn(
                name: "BenefitOwnerId",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropColumn(
                name: "BenefitRelationship",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilArHandoff_DebtorType",
                schema: "public",
                table: "BilArHandoff",
                sql: "\"DebtorType\" IN ('PATIENT_GUARANTOR','PAYER')");
        }
    }
}
