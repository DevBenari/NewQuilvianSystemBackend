using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class DeleteUserTypeRoleAccess : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysAccessPolicy_DepartmentId_PositionId_UserType_Controller~",
                schema: "public",
                table: "SysAccessPolicy");

            migrationBuilder.DropIndex(
                name: "IX_SysAccessPolicy_UserType",
                schema: "public",
                table: "SysAccessPolicy");

            migrationBuilder.DropColumn(
                name: "UserType",
                schema: "public",
                table: "SysAccessPolicy");

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_DepartmentId_PositionId_ControllerAccessId_~",
                schema: "public",
                table: "SysAccessPolicy",
                columns: new[] { "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SysAccessPolicy_DepartmentId_PositionId_ControllerAccessId_~",
                schema: "public",
                table: "SysAccessPolicy");

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                schema: "public",
                table: "SysAccessPolicy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_DepartmentId_PositionId_UserType_Controller~",
                schema: "public",
                table: "SysAccessPolicy",
                columns: new[] { "DepartmentId", "PositionId", "UserType", "ControllerAccessId", "ActionAccessId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SysAccessPolicy_UserType",
                schema: "public",
                table: "SysAccessPolicy",
                column: "UserType");
        }
    }
}
