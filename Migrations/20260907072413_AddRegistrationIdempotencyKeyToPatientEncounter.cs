using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationIdempotencyKeyToPatientEncounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistrationIdempotencyKey",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_RegistrationIdempotencyKey",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "RegistrationIdempotencyKey",
                unique: true,
                filter: "\"RegistrationIdempotencyKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_RegistrationIdempotencyKey",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "RegistrationIdempotencyKey",
                schema: "public",
                table: "TrxPatientEncounter");
        }
    }
}
