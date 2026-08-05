using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeHardenOvertimePolicyAndRate4A2A : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstOvertimePolicy_IsDefault_RequirePreApproval_RequirePostV~",
                schema: "public",
                table: "MstOvertimePolicy");

            migrationBuilder.AddColumn<bool>(
                name: "IsFallback",
                schema: "public",
                table: "MstOvertimePolicy",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "public",
                table: "MstOvertimePolicy",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MstOvertimePolicy_Priority_IsFallback_IsDefault_RequirePreA~",
                schema: "public",
                table: "MstOvertimePolicy",
                columns: new[] { "Priority", "IsFallback", "IsDefault", "RequirePreApproval", "RequirePostVerification", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstOvertimePolicy_Priority_IsFallback_IsDefault_RequirePreA~",
                schema: "public",
                table: "MstOvertimePolicy");

            migrationBuilder.DropColumn(
                name: "IsFallback",
                schema: "public",
                table: "MstOvertimePolicy");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "public",
                table: "MstOvertimePolicy");

            migrationBuilder.CreateIndex(
                name: "IX_MstOvertimePolicy_IsDefault_RequirePreApproval_RequirePostV~",
                schema: "public",
                table: "MstOvertimePolicy",
                columns: new[] { "IsDefault", "RequirePreApproval", "RequirePostVerification", "IsActive", "IsDelete" });
        }
    }
}
