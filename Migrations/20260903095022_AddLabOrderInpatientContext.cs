using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabOrderInpatientContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InpEpisodeId",
                schema: "public",
                table: "LabOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_InpEpisodeId_CreateDateTime",
                schema: "public",
                table: "LabOrder",
                columns: new[] { "InpEpisodeId", "CreateDateTime" });

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrder_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "LabOrder",
                column: "InpEpisodeId",
                principalSchema: "public",
                principalTable: "InpEpisode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabOrder_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_InpEpisodeId_CreateDateTime",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "InpEpisodeId",
                schema: "public",
                table: "LabOrder");
        }
    }
}
