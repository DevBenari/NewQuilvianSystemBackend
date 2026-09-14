using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBbkBloodUnitPlacement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentPlacementId",
                schema: "public",
                table: "BbkBloodUnit",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BbkBloodUnitPlacement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousPlacementId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlacedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlacedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_BbkBloodUnitPlacement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkBloodUnitPlacement_BbkBloodUnitPlacement_PreviousPlaceme~",
                        column: x => x.PreviousPlacementId,
                        principalSchema: "public",
                        principalTable: "BbkBloodUnitPlacement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodUnitPlacement_BbkBloodUnit_BloodUnitId",
                        column: x => x.BloodUnitId,
                        principalSchema: "public",
                        principalTable: "BbkBloodUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodUnitPlacement_MstBloodStorageLocation_StorageLocati~",
                        column: x => x.StorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstBloodStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnit_CurrentPlacementId",
                schema: "public",
                table: "BbkBloodUnit",
                column: "CurrentPlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitPlacement_BloodUnitId",
                schema: "public",
                table: "BbkBloodUnitPlacement",
                column: "BloodUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitPlacement_CurrentUnit",
                schema: "public",
                table: "BbkBloodUnitPlacement",
                column: "BloodUnitId",
                unique: true,
                filter: "\"IsCurrent\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitPlacement_PlacedAt",
                schema: "public",
                table: "BbkBloodUnitPlacement",
                column: "PlacedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitPlacement_PreviousPlacementId",
                schema: "public",
                table: "BbkBloodUnitPlacement",
                column: "PreviousPlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnitPlacement_StorageLocationId",
                schema: "public",
                table: "BbkBloodUnitPlacement",
                column: "StorageLocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_BbkBloodUnit_BbkBloodUnitPlacement_CurrentPlacementId",
                schema: "public",
                table: "BbkBloodUnit",
                column: "CurrentPlacementId",
                principalSchema: "public",
                principalTable: "BbkBloodUnitPlacement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BbkBloodUnit_BbkBloodUnitPlacement_CurrentPlacementId",
                schema: "public",
                table: "BbkBloodUnit");

            migrationBuilder.DropTable(
                name: "BbkBloodUnitPlacement",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_BbkBloodUnit_CurrentPlacementId",
                schema: "public",
                table: "BbkBloodUnit");

            migrationBuilder.DropColumn(
                name: "CurrentPlacementId",
                schema: "public",
                table: "BbkBloodUnit");
        }
    }
}
