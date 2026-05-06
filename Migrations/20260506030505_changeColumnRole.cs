using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeColumnRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysControllerAccess_DisplayName",
                schema: "public",
                table: "SysControllerAccess");

            migrationBuilder.DropIndex(
                name: "IX_SysApplicationModule_ModuleName",
                schema: "public",
                table: "SysApplicationModule");

            migrationBuilder.DropIndex(
                name: "IX_SysActionAccess_DisplayName",
                schema: "public",
                table: "SysActionAccess");

            migrationBuilder.DropIndex(
                name: "IX_SysAccessPolicy_ControllerAccessId",
                schema: "public",
                table: "SysAccessPolicy");

            migrationBuilder.DropIndex(
                name: "IX_SysAccessPolicy_DepartmentId",
                schema: "public",
                table: "SysAccessPolicy");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "SysControllerAccess",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "SysControllerAccess",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "SysControllerAccess",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "SysControllerAccess",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemOnly",
                schema: "public",
                table: "SysControllerAccess",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VisibleInRoleAccess",
                schema: "public",
                table: "SysControllerAccess",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "SysApplicationModule",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "SysApplicationModule",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "SysApplicationModule",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "SysApplicationModule",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "SysActionAccess",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "SysActionAccess",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "SysActionAccess",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "SysActionAccess",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "AccessType",
                schema: "public",
                table: "SysActionAccess",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Read");

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemOnly",
                schema: "public",
                table: "SysActionAccess",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "VisibleInRoleAccess",
                schema: "public",
                table: "SysActionAccess",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "SysAccessPolicy",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "SysAccessPolicy",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "SysAccessPolicy",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerAccess_ControllerName",
                schema: "public",
                table: "SysControllerAccess",
                column: "ControllerName");

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerAccess_IsActive",
                schema: "public",
                table: "SysControllerAccess",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerAccess_IsSystemOnly",
                schema: "public",
                table: "SysControllerAccess",
                column: "IsSystemOnly");

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerAccess_VisibleInRoleAccess",
                schema: "public",
                table: "SysControllerAccess",
                column: "VisibleInRoleAccess");

            migrationBuilder.CreateIndex(
                name: "IX_SysApplicationModule_IsActive",
                schema: "public",
                table: "SysApplicationModule",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_AccessType",
                schema: "public",
                table: "SysActionAccess",
                column: "AccessType");

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_ActionName",
                schema: "public",
                table: "SysActionAccess",
                column: "ActionName");

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_IsActive",
                schema: "public",
                table: "SysActionAccess",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_IsSystemOnly",
                schema: "public",
                table: "SysActionAccess",
                column: "IsSystemOnly");

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_VisibleInRoleAccess",
                schema: "public",
                table: "SysActionAccess",
                column: "VisibleInRoleAccess");

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_ControllerAccessId_ActionAccessId_IsAllowed~",
                schema: "public",
                table: "SysAccessPolicy",
                columns: new[] { "ControllerAccessId", "ActionAccessId", "IsAllowed", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_DepartmentId_PositionId_IsAllowed_IsActive_~",
                schema: "public",
                table: "SysAccessPolicy",
                columns: new[] { "DepartmentId", "PositionId", "IsAllowed", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysControllerAccess_ControllerName",
                schema: "public",
                table: "SysControllerAccess");

            migrationBuilder.DropIndex(
                name: "IX_SysControllerAccess_IsActive",
                schema: "public",
                table: "SysControllerAccess");

            migrationBuilder.DropIndex(
                name: "IX_SysControllerAccess_IsSystemOnly",
                schema: "public",
                table: "SysControllerAccess");

            migrationBuilder.DropIndex(
                name: "IX_SysControllerAccess_VisibleInRoleAccess",
                schema: "public",
                table: "SysControllerAccess");

            migrationBuilder.DropIndex(
                name: "IX_SysApplicationModule_IsActive",
                schema: "public",
                table: "SysApplicationModule");

            migrationBuilder.DropIndex(
                name: "IX_SysActionAccess_AccessType",
                schema: "public",
                table: "SysActionAccess");

            migrationBuilder.DropIndex(
                name: "IX_SysActionAccess_ActionName",
                schema: "public",
                table: "SysActionAccess");

            migrationBuilder.DropIndex(
                name: "IX_SysActionAccess_IsActive",
                schema: "public",
                table: "SysActionAccess");

            migrationBuilder.DropIndex(
                name: "IX_SysActionAccess_IsSystemOnly",
                schema: "public",
                table: "SysActionAccess");

            migrationBuilder.DropIndex(
                name: "IX_SysActionAccess_VisibleInRoleAccess",
                schema: "public",
                table: "SysActionAccess");

            migrationBuilder.DropIndex(
                name: "IX_SysAccessPolicy_ControllerAccessId_ActionAccessId_IsAllowed~",
                schema: "public",
                table: "SysAccessPolicy");

            migrationBuilder.DropIndex(
                name: "IX_SysAccessPolicy_DepartmentId_PositionId_IsAllowed_IsActive_~",
                schema: "public",
                table: "SysAccessPolicy");

            migrationBuilder.DropColumn(
                name: "IsSystemOnly",
                schema: "public",
                table: "SysControllerAccess");

            migrationBuilder.DropColumn(
                name: "VisibleInRoleAccess",
                schema: "public",
                table: "SysControllerAccess");

            migrationBuilder.DropColumn(
                name: "AccessType",
                schema: "public",
                table: "SysActionAccess");

            migrationBuilder.DropColumn(
                name: "IsSystemOnly",
                schema: "public",
                table: "SysActionAccess");

            migrationBuilder.DropColumn(
                name: "VisibleInRoleAccess",
                schema: "public",
                table: "SysActionAccess");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "SysControllerAccess",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "SysControllerAccess",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "SysControllerAccess",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "SysControllerAccess",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "SysApplicationModule",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "SysApplicationModule",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "SysApplicationModule",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "SysApplicationModule",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "SysActionAccess",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "SysActionAccess",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "SysActionAccess",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "SysActionAccess",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "SysAccessPolicy",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "SysAccessPolicy",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "SysAccessPolicy",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_SysControllerAccess_DisplayName",
                schema: "public",
                table: "SysControllerAccess",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_SysApplicationModule_ModuleName",
                schema: "public",
                table: "SysApplicationModule",
                column: "ModuleName");

            migrationBuilder.CreateIndex(
                name: "IX_SysActionAccess_DisplayName",
                schema: "public",
                table: "SysActionAccess",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_ControllerAccessId",
                schema: "public",
                table: "SysAccessPolicy",
                column: "ControllerAccessId");

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_DepartmentId",
                schema: "public",
                table: "SysAccessPolicy",
                column: "DepartmentId");
        }
    }
}
