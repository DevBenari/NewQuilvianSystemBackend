using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddFloorPlanLocationToMstRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FloorCode",
                schema: "public",
                table: "MstRoom",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FloorPlanRoomKey",
                schema: "public",
                table: "MstRoom",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WingDirection",
                schema: "public",
                table: "MstRoom",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstRoom_FloorCode",
                schema: "public",
                table: "MstRoom",
                column: "FloorCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstRoom_FloorPlanRoomKey",
                schema: "public",
                table: "MstRoom",
                column: "FloorPlanRoomKey",
                unique: true,
                filter: "\"FloorPlanRoomKey\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstRoom_FloorPlanRoomKey_Floor",
                schema: "public",
                table: "MstRoom",
                sql: "\"FloorPlanRoomKey\" IS NULL OR \"FloorCode\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstRoom_WingDirection",
                schema: "public",
                table: "MstRoom",
                sql: "\"WingDirection\" IS NULL OR \"WingDirection\" BETWEEN 1 AND 8");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstRoom_FloorCode",
                schema: "public",
                table: "MstRoom");

            migrationBuilder.DropIndex(
                name: "IX_MstRoom_FloorPlanRoomKey",
                schema: "public",
                table: "MstRoom");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstRoom_FloorPlanRoomKey_Floor",
                schema: "public",
                table: "MstRoom");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstRoom_WingDirection",
                schema: "public",
                table: "MstRoom");

            migrationBuilder.DropColumn(
                name: "FloorCode",
                schema: "public",
                table: "MstRoom");

            migrationBuilder.DropColumn(
                name: "FloorPlanRoomKey",
                schema: "public",
                table: "MstRoom");

            migrationBuilder.DropColumn(
                name: "WingDirection",
                schema: "public",
                table: "MstRoom");
        }
    }
}
