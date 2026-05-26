using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeMstServiceUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstServiceUnit",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ServiceUnitName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ServiceUnitType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LocationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FloorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsAvailableForRegistration = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForKiosk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAvailableForAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsQueueRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDoctorRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsScreeningRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_MstServiceUnit", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstServiceUnit_IsAvailableForRegistration_IsAvailableForKio~",
                schema: "public",
                table: "MstServiceUnit",
                columns: new[] { "IsAvailableForRegistration", "IsAvailableForKiosk", "IsAvailableForAppointment", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstServiceUnit_ServiceUnitCode",
                schema: "public",
                table: "MstServiceUnit",
                column: "ServiceUnitCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstServiceUnit_ServiceUnitName",
                schema: "public",
                table: "MstServiceUnit",
                column: "ServiceUnitName");

            migrationBuilder.CreateIndex(
                name: "IX_MstServiceUnit_ServiceUnitType",
                schema: "public",
                table: "MstServiceUnit",
                column: "ServiceUnitType");

            migrationBuilder.CreateIndex(
                name: "IX_MstServiceUnit_ServiceUnitType_IsActive_IsDelete",
                schema: "public",
                table: "MstServiceUnit",
                columns: new[] { "ServiceUnitType", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstServiceUnit",
                schema: "public");
        }
    }
}
