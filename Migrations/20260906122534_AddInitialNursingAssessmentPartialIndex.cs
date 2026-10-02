using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialNursingAssessmentPartialIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_Episode_Type_Active",
                schema: "public",
                table: "TrxPatientAssessment",
                columns: new[] { "InpEpisodeId", "AssessmentType" },
                filter: "\"AssessmentType\" = 0 AND \"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAssessment_Episode_Type_Active",
                schema: "public",
                table: "TrxPatientAssessment");
        }
    }
}
