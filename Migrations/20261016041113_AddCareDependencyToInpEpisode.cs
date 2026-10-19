using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCareDependencyToInpEpisode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CareDependencyLevel",
                schema: "public",
                table: "InpEpisode",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CareDependencyNote",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CareDependencySetAt",
                schema: "public",
                table: "InpEpisode",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CareDependencySetByUserId",
                schema: "public",
                table: "InpEpisode",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CareDependencySource",
                schema: "public",
                table: "InpEpisode",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InpCareDependencyHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousLevel = table.Column<int>(type: "integer", nullable: true),
                    NewLevel = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InpCareDependencyHistory", x => x.Id);
                    table.CheckConstraint("CK_InpCareDependencyHistory_NewLevel", "\"NewLevel\" BETWEEN 1 AND 2");
                    table.CheckConstraint("CK_InpCareDependencyHistory_PreviousLevel", "\"PreviousLevel\" IS NULL OR \"PreviousLevel\" BETWEEN 1 AND 2");
                    table.CheckConstraint("CK_InpCareDependencyHistory_Source", "\"Source\" BETWEEN 1 AND 3");
                    table.ForeignKey(
                        name: "FK_InpCareDependencyHistory_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InpCareDependencyHistory_InpEpisode_EpisodeId",
                        column: x => x.EpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InpEpisode_CareDependencySetByUserId",
                schema: "public",
                table: "InpEpisode",
                column: "CareDependencySetByUserId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InpEpisode_CareDependencyLevel",
                schema: "public",
                table: "InpEpisode",
                sql: "\"CareDependencyLevel\" IS NULL OR \"CareDependencyLevel\" BETWEEN 1 AND 2");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InpEpisode_CareDependencySource",
                schema: "public",
                table: "InpEpisode",
                sql: "\"CareDependencySource\" IS NULL OR \"CareDependencySource\" BETWEEN 1 AND 3");

            migrationBuilder.CreateIndex(
                name: "IX_InpCareDependencyHistory_ActorUserId",
                schema: "public",
                table: "InpCareDependencyHistory",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InpCareDependencyHistory_EpisodeId_RecordedAtUtc_Id",
                schema: "public",
                table: "InpCareDependencyHistory",
                columns: new[] { "EpisodeId", "RecordedAtUtc", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_InpEpisode_AspNetUsers_CareDependencySetByUserId",
                schema: "public",
                table: "InpEpisode",
                column: "CareDependencySetByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InpEpisode_AspNetUsers_CareDependencySetByUserId",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropTable(
                name: "InpCareDependencyHistory",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_InpEpisode_CareDependencySetByUserId",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InpEpisode_CareDependencyLevel",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InpEpisode_CareDependencySource",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "CareDependencyLevel",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "CareDependencyNote",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "CareDependencySetAt",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "CareDependencySetByUserId",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "CareDependencySource",
                schema: "public",
                table: "InpEpisode");
        }
    }
}
