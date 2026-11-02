using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Ekstensi CliTransferHandover untuk mendukung serah terima transfer pasien masuk dari IGD/Rajal (BE-RWI-241):
    /// Kolom FromPlacementId dan ToPlacementId dijadikan nullable, dan index unik UX_CliTransferHandover_ToPlacement
    /// diberi filter partial WHERE "ToPlacementId" IS NOT NULL.
    /// </summary>
    /// <inheritdoc />
    public partial class MakeTransferHandoverPlacementsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_CliTransferHandover_ToPlacement",
                schema: "public",
                table: "CliTransferHandover");

            migrationBuilder.AlterColumn<Guid>(
                name: "FromPlacementId",
                schema: "public",
                table: "CliTransferHandover",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ToPlacementId",
                schema: "public",
                table: "CliTransferHandover",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "UX_CliTransferHandover_ToPlacement",
                schema: "public",
                table: "CliTransferHandover",
                column: "ToPlacementId",
                unique: true,
                filter: "\"ToPlacementId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_CliTransferHandover_ToPlacement",
                schema: "public",
                table: "CliTransferHandover");

            migrationBuilder.AlterColumn<Guid>(
                name: "ToPlacementId",
                schema: "public",
                table: "CliTransferHandover",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FromPlacementId",
                schema: "public",
                table: "CliTransferHandover",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_CliTransferHandover_ToPlacement",
                schema: "public",
                table: "CliTransferHandover",
                column: "ToPlacementId",
                unique: true);
        }
    }
}
