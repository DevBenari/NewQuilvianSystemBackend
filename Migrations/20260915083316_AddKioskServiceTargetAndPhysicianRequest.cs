using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddKioskServiceTargetAndPhysicianRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasPhysicianRequest",
                schema: "public",
                table: "TrxKioskScanSession",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetService",
                schema: "public",
                table: "TrxKioskScanSession",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxKioskScanSession_TargetService",
                schema: "public",
                table: "TrxKioskScanSession",
                column: "TargetService");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxKioskScanSession_TargetService",
                schema: "public",
                table: "TrxKioskScanSession");

            migrationBuilder.DropColumn(
                name: "HasPhysicianRequest",
                schema: "public",
                table: "TrxKioskScanSession");

            migrationBuilder.DropColumn(
                name: "TargetService",
                schema: "public",
                table: "TrxKioskScanSession");
        }
    }
}
