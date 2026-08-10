using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class FinalApplicationVersioningV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysAppVersion_AppName_BackendVersion",
                schema: "public",
                table: "SysAppVersion");

            migrationBuilder.RenameColumn(
                name: "FrontendMinimumVersion",
                schema: "public",
                table: "SysAppVersion",
                newName: "MinimumSupportedFrontendVersion");

            migrationBuilder.AddColumn<int>(
                name: "VersioningGeneration",
                schema: "public",
                table: "SysAppVersion",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Explicitly classify every pre-V2 record without changing its version or history.
            migrationBuilder.Sql(
                """
                UPDATE public."SysAppVersion"
                SET "VersioningGeneration" = 1;
                """);

            // Select one deterministic active/non-deleted latest row per application before
            // enforcing the partial unique index. No row is deleted or deactivated.
            migrationBuilder.Sql(
                """
                WITH ranked AS
                (
                    SELECT
                        "Id",
                        ROW_NUMBER() OVER
                        (
                            PARTITION BY "AppName"
                            ORDER BY
                                "IsLatest" DESC,
                                "ReleaseDateTime" DESC,
                                "CreateDateTime" DESC,
                                "Id" DESC
                        ) AS row_number
                    FROM public."SysAppVersion"
                    WHERE "IsActive" = true
                      AND "IsDelete" = false
                )
                UPDATE public."SysAppVersion" AS app_version
                SET "IsLatest" = (ranked.row_number = 1)
                FROM ranked
                WHERE app_version."Id" = ranked."Id";

                UPDATE public."SysAppVersion"
                SET "IsLatest" = false
                WHERE ("IsActive" = false OR "IsDelete" = true)
                  AND "IsLatest" = true;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_AppName",
                schema: "public",
                table: "SysAppVersion",
                column: "AppName",
                unique: true,
                filter: "\"IsLatest\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_AppName_VersioningGeneration_BackendVersion",
                schema: "public",
                table: "SysAppVersion",
                columns: new[] { "AppName", "VersioningGeneration", "BackendVersion" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysAppVersion_AppName",
                schema: "public",
                table: "SysAppVersion");

            migrationBuilder.DropIndex(
                name: "IX_SysAppVersion_AppName_VersioningGeneration_BackendVersion",
                schema: "public",
                table: "SysAppVersion");

            migrationBuilder.DropColumn(
                name: "VersioningGeneration",
                schema: "public",
                table: "SysAppVersion");

            migrationBuilder.RenameColumn(
                name: "MinimumSupportedFrontendVersion",
                schema: "public",
                table: "SysAppVersion",
                newName: "FrontendMinimumVersion");

            migrationBuilder.CreateIndex(
                name: "IX_SysAppVersion_AppName_BackendVersion",
                schema: "public",
                table: "SysAppVersion",
                columns: new[] { "AppName", "BackendVersion" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }
    }
}
