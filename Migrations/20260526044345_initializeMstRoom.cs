using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeMstRoom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstRoom",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoomCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RoomName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RoomType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RoomNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LocationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FloorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsForMale = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForFemale = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForNewborn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsIsolationRoom = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsIntensiveCare = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsOdcRoom = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAvailableForAdmission = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstRoom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstRoom_MstPatientClass_PatientClassId",
                        column: x => x.PatientClassId,
                        principalSchema: "public",
                        principalTable: "MstPatientClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstRoom_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstRoom_IsIsolationRoom_IsIntensiveCare_IsOdcRoom_IsActive_~",
                schema: "public",
                table: "MstRoom",
                columns: new[] { "IsIsolationRoom", "IsIntensiveCare", "IsOdcRoom", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstRoom_PatientClassId",
                schema: "public",
                table: "MstRoom",
                column: "PatientClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MstRoom_RoomCode",
                schema: "public",
                table: "MstRoom",
                column: "RoomCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstRoom_RoomName",
                schema: "public",
                table: "MstRoom",
                column: "RoomName");

            migrationBuilder.CreateIndex(
                name: "IX_MstRoom_ServiceUnitId",
                schema: "public",
                table: "MstRoom",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstRoom_ServiceUnitId_PatientClassId_RoomType_IsActive_IsDe~",
                schema: "public",
                table: "MstRoom",
                columns: new[] { "ServiceUnitId", "PatientClassId", "RoomType", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstRoom",
                schema: "public");
        }
    }
}
