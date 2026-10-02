using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalAssessmentContentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhysicalExamination",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TherapyPlan",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingDiagnosis",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhysicalExamination",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "TherapyPlan",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "WorkingDiagnosis",
                schema: "public",
                table: "TrxPatientAssessment");
        }
    }
}
