using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class addColumnAssessmentPatient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAssessment_EncounterId",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAssessment_QueueId",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.AddColumn<bool>(
                name: "HasBcgImmunization",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasDptImmunization",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasHepatitisBImmunization",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasMeaslesImmunization",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasPolioImmunization",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ImmunizationNote",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_EncounterId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "EncounterId",
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_QueueId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "QueueId",
                filter: "\"IsDelete\" = false");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAssessment_EncounterId",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAssessment_QueueId",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "HasBcgImmunization",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "HasDptImmunization",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "HasHepatitisBImmunization",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "HasMeaslesImmunization",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "HasPolioImmunization",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "ImmunizationNote",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_EncounterId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "EncounterId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_QueueId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "QueueId",
                unique: true,
                filter: "\"IsDelete\" = false");
        }
    }
}
