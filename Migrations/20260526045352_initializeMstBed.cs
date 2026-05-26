using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeMstBed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstBed",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    BedCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BedNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BedStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsForMale = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForFemale = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForNewborn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsIsolationBed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsIntensiveCareBed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsOdcBed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsReservable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstBed", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstBed_MstRoom_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "public",
                        principalTable: "MstRoom",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstBed_BedCode",
                schema: "public",
                table: "MstBed",
                column: "BedCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstBed_BedStatus_IsReservable_IsActive_IsDelete",
                schema: "public",
                table: "MstBed",
                columns: new[] { "BedStatus", "IsReservable", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstBed_RoomId",
                schema: "public",
                table: "MstBed",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_MstBed_RoomId_BedNumber",
                schema: "public",
                table: "MstBed",
                columns: new[] { "RoomId", "BedNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstBed_RoomId_BedStatus_IsActive_IsDelete",
                schema: "public",
                table: "MstBed",
                columns: new[] { "RoomId", "BedStatus", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstBed",
                schema: "public");
        }
    }
}
