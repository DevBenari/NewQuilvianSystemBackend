using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddMstFloorPlanImportRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstFloorPlanImportRun",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManifestVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ManifestHash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    EnvironmentName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MarkedBedsReady = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReadyEvidenceReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SummaryJson = table.Column<string>(type: "jsonb", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_MstFloorPlanImportRun", x => x.Id);
                    table.CheckConstraint("CK_MstFloorPlanImportRun_ReadyEvidence", "NOT \"MarkedBedsReady\" OR \"ReadyEvidenceReference\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_MstFloorPlanImportRun_AspNetUsers_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstFloorPlanImportRun_ActorUserId",
                schema: "public",
                table: "MstFloorPlanImportRun",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MstFloorPlanImportRun_ManifestHash",
                schema: "public",
                table: "MstFloorPlanImportRun",
                column: "ManifestHash");

            migrationBuilder.CreateIndex(
                name: "IX_MstFloorPlanImportRun_StartedAtUtc",
                schema: "public",
                table: "MstFloorPlanImportRun",
                column: "StartedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstFloorPlanImportRun",
                schema: "public");
        }
    }
}
