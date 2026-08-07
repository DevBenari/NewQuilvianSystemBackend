using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomaticApplicationVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysAppVersion_BackendVersion",
                schema: "public",
                table: "SysAppVersion");

            migrationBuilder.AddColumn<string>(
                name: "MergeCommitSha",
                schema: "public",
                table: "SysAppVersion",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PullRequestNumber",
                schema: "public",
                table: "SysAppVersion",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceBranch",
                schema: "public",
                table: "SysAppVersion",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetBranch",
                schema: "public",
                table: "SysAppVersion",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SysAppVersionBuild",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BuildNumber = table.Column<long>(type: "bigint", nullable: false),
                    CommitSha = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CommitMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BranchName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BuildDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_SysAppVersionBuild", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysAppVersionBuild_SysAppVersion_AppVersionId",
                        column: x => x.AppVersionId,
                        principalSchema: "public",
                        principalTable: "SysAppVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_AppName_BackendVersion",
                schema: "public",
                table: "SysAppVersion",
                columns: new[] { "AppName", "BackendVersion" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersionBuild_AppVersionId_BuildVersion",
                schema: "public",
                table: "SysAppVersionBuild",
                columns: new[] { "AppVersionId", "BuildVersion" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersionBuild_AppVersionId_CommitSha",
                schema: "public",
                table: "SysAppVersionBuild",
                columns: new[] { "AppVersionId", "CommitSha" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersionBuild_BuildDateTime",
                schema: "public",
                table: "SysAppVersionBuild",
                column: "BuildDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersionBuild_BuildNumber",
                schema: "public",
                table: "SysAppVersionBuild",
                column: "BuildNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SysAppVersionBuild",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_SysAppVersion_AppName_BackendVersion",
                schema: "public",
                table: "SysAppVersion");

            migrationBuilder.DropColumn(
                name: "MergeCommitSha",
                schema: "public",
                table: "SysAppVersion");

            migrationBuilder.DropColumn(
                name: "PullRequestNumber",
                schema: "public",
                table: "SysAppVersion");

            migrationBuilder.DropColumn(
                name: "SourceBranch",
                schema: "public",
                table: "SysAppVersion");

            migrationBuilder.DropColumn(
                name: "TargetBranch",
                schema: "public",
                table: "SysAppVersion");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_BackendVersion",
                schema: "public",
                table: "SysAppVersion",
                column: "BackendVersion");
        }
    }
}
