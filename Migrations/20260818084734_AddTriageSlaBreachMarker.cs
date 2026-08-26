using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTriageSlaBreachMarker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSlaBreached",
                schema: "public",
                table: "TrxEmergencyTriage",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SlaBreachedAt",
                schema: "public",
                table: "TrxEmergencyTriage",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxEmergencyTriage_EmergencyVisitId_ResponseDueAt_IsSlaBrea~",
                schema: "public",
                table: "TrxEmergencyTriage",
                columns: new[] { "EmergencyVisitId", "ResponseDueAt", "IsSlaBreached" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxEmergencyTriage_EmergencyVisitId_ResponseDueAt_IsSlaBrea~",
                schema: "public",
                table: "TrxEmergencyTriage");

            migrationBuilder.DropColumn(
                name: "SlaBreachedAt",
                schema: "public",
                table: "TrxEmergencyTriage");

            migrationBuilder.DropColumn(
                name: "IsSlaBreached",
                schema: "public",
                table: "TrxEmergencyTriage");
        }
    }
}
