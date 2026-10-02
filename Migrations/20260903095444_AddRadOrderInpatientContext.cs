using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRadOrderInpatientContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InpEpisodeId",
                schema: "public",
                table: "RadOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_InpEpisodeId_CreateDateTime",
                schema: "public",
                table: "RadOrder",
                columns: new[] { "InpEpisodeId", "CreateDateTime" });

            migrationBuilder.AddForeignKey(
                name: "FK_RadOrder_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "RadOrder",
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
                name: "FK_RadOrder_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropIndex(
                name: "IX_RadOrder_InpEpisodeId_CreateDateTime",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "InpEpisodeId",
                schema: "public",
                table: "RadOrder");
        }
    }
}
