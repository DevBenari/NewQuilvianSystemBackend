using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeSysAppVersion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "SysAppVersion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BackendVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ApiVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FrontendMinimumVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FrontendRecommendedVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ReleaseName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsLatest = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReleaseDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysAppVersion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_ApiVersion",
                schema: "public",
                table: "SysAppVersion",
                column: "ApiVersion");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_BackendVersion",
                schema: "public",
                table: "SysAppVersion",
                column: "BackendVersion");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_IsActive",
                schema: "public",
                table: "SysAppVersion",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_IsLatest",
                schema: "public",
                table: "SysAppVersion",
                column: "IsLatest");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SysAppVersion",
                schema: "public");
        }
    }
}
