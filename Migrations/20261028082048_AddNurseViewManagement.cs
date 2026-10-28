using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddNurseViewManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NvwDisplayDevice",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FloorCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    LocationName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeviceStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    LoginUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LoginIssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoginIssuedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LastOnlineAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeactivatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeactivationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_NvwDisplayDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NvwDisplayDevice_AspNetUsers_LoginUserId",
                        column: x => x.LoginUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NvwVitalSignInterval",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EwsRiskLevel = table.Column<int>(type: "integer", nullable: false),
                    IntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    RatificationReference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RatifiedOn = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_NvwVitalSignInterval", x => x.Id);
                    table.CheckConstraint("CK_NvwVitalSignInterval_Level", "\"EwsRiskLevel\" BETWEEN 1 AND 4");
                    table.CheckConstraint("CK_NvwVitalSignInterval_Minutes", "\"IntervalMinutes\" BETWEEN 1 AND 1440");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NvwDisplayDevice_DeviceCode",
                schema: "public",
                table: "NvwDisplayDevice",
                column: "DeviceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NvwDisplayDevice_FloorCode_DeviceStatus",
                schema: "public",
                table: "NvwDisplayDevice",
                columns: new[] { "FloorCode", "DeviceStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_NvwDisplayDevice_LoginUserId",
                schema: "public",
                table: "NvwDisplayDevice",
                column: "LoginUserId",
                unique: true,
                filter: "\"LoginUserId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_NvwVitalSignInterval_EwsRiskLevel",
                schema: "public",
                table: "NvwVitalSignInterval",
                column: "EwsRiskLevel",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NvwDisplayDevice",
                schema: "public");

            migrationBuilder.DropTable(
                name: "NvwVitalSignInterval",
                schema: "public");
        }
    }
}
