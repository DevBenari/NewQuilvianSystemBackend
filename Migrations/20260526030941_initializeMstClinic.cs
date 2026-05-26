using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeMstClinic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstClinic",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClinicName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ClinicType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShortName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LocationName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FloorName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RoomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsAvailableForRegistration = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForKiosk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForAppointment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDoctorRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsScreeningRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsQueueRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DefaultEstimatedServiceMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
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
                    table.PrimaryKey("PK_MstClinic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstClinic_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstClinic_ClinicCode",
                schema: "public",
                table: "MstClinic",
                column: "ClinicCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstClinic_ClinicName",
                schema: "public",
                table: "MstClinic",
                column: "ClinicName");

            migrationBuilder.CreateIndex(
                name: "IX_MstClinic_IsAvailableForRegistration_IsAvailableForKiosk_Is~",
                schema: "public",
                table: "MstClinic",
                columns: new[] { "IsAvailableForRegistration", "IsAvailableForKiosk", "IsAvailableForAppointment", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstClinic_ServiceUnitId",
                schema: "public",
                table: "MstClinic",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstClinic_ServiceUnitId_ClinicName",
                schema: "public",
                table: "MstClinic",
                columns: new[] { "ServiceUnitId", "ClinicName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstClinic_ServiceUnitId_ClinicType_IsActive_IsDelete",
                schema: "public",
                table: "MstClinic",
                columns: new[] { "ServiceUnitId", "ClinicType", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstClinic",
                schema: "public");
        }
    }
}
