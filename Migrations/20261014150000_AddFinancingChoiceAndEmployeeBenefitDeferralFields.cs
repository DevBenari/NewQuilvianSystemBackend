using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancingChoiceAndEmployeeBenefitDeferralFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinancingChoice",
                schema: "public",
                table: "RegPatientEncounterGuarantor",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinancingChoice",
                schema: "public",
                table: "BilArHandoff",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeferredConsentNotes",
                schema: "public",
                table: "BilArHandoff",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinancingChoice",
                schema: "public",
                table: "RegPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "FinancingChoice",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropColumn(
                name: "DeferredConsentNotes",
                schema: "public",
                table: "BilArHandoff");
        }
    }
}

