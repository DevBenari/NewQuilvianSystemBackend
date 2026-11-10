using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicTypeToMedicalRecordCompletenessProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstMedicalRecordCompletenessProfile_EncounterType_ServiceUn~",
                schema: "public",
                table: "MstMedicalRecordCompletenessProfile");

            migrationBuilder.AddColumn<int>(
                name: "ClinicType",
                schema: "public",
                table: "MstMedicalRecordCompletenessProfile",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalRecordCompletenessProfile_EncounterType_ServiceUn~",
                schema: "public",
                table: "MstMedicalRecordCompletenessProfile",
                columns: new[] { "EncounterType", "ServiceUnitType", "ClinicType", "Condition" },
                unique: true,
                filter: "\"IsDelete\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstMedicalRecordCompletenessProfile_ClinicTypeTanpaKondisi",
                schema: "public",
                table: "MstMedicalRecordCompletenessProfile",
                sql: "\"Condition\" = 0 OR \"ClinicType\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // BE-42 / A5.7: sebelum mundur, hapus profil berpoli (mis. RJ-REHAB) lebih dulu. Tanpa itu
            // index tiga kolom yang dipulihkan di bawah bentrok dengan profil RJ yang berkunci sama.
            migrationBuilder.DropIndex(
                name: "IX_MstMedicalRecordCompletenessProfile_EncounterType_ServiceUn~",
                schema: "public",
                table: "MstMedicalRecordCompletenessProfile");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstMedicalRecordCompletenessProfile_ClinicTypeTanpaKondisi",
                schema: "public",
                table: "MstMedicalRecordCompletenessProfile");

            migrationBuilder.DropColumn(
                name: "ClinicType",
                schema: "public",
                table: "MstMedicalRecordCompletenessProfile");

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalRecordCompletenessProfile_EncounterType_ServiceUn~",
                schema: "public",
                table: "MstMedicalRecordCompletenessProfile",
                columns: new[] { "EncounterType", "ServiceUnitType", "Condition" },
                unique: true,
                filter: "\"IsDelete\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }
    }
}
