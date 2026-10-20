using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorCertificatePatientDependencyLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PatientDependencyLevel",
                schema: "public",
                table: "CliDoctorCertificate",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientDependencyLevel",
                schema: "public",
                table: "CliDoctorCertificate");
        }
    }
}
