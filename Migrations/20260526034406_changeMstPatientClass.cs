using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeMstPatientClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstPatientClass_IsForOutpatient_IsForInpatient_IsForEmergen~",
                schema: "public",
                table: "MstPatientClass");

            migrationBuilder.AlterColumn<bool>(
                name: "IsForOutpatient",
                schema: "public",
                table: "MstPatientClass",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassAlias",
                schema: "public",
                table: "MstPatientClass",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalClassCode",
                schema: "public",
                table: "MstPatientClass",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForRoomCharge",
                schema: "public",
                table: "MstPatientClass",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsForTariffMapping",
                schema: "public",
                table: "MstPatientClass",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientClass_ExternalClassCode",
                schema: "public",
                table: "MstPatientClass",
                column: "ExternalClassCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientClass_ExternalClassCode_IsActive_IsDelete",
                schema: "public",
                table: "MstPatientClass",
                columns: new[] { "ExternalClassCode", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientClass_IsForRoomCharge_IsForTariffMapping_IsActive~",
                schema: "public",
                table: "MstPatientClass",
                columns: new[] { "IsForRoomCharge", "IsForTariffMapping", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstPatientClass_ExternalClassCode",
                schema: "public",
                table: "MstPatientClass");

            migrationBuilder.DropIndex(
                name: "IX_MstPatientClass_ExternalClassCode_IsActive_IsDelete",
                schema: "public",
                table: "MstPatientClass");

            migrationBuilder.DropIndex(
                name: "IX_MstPatientClass_IsForRoomCharge_IsForTariffMapping_IsActive~",
                schema: "public",
                table: "MstPatientClass");

            migrationBuilder.DropColumn(
                name: "ClassAlias",
                schema: "public",
                table: "MstPatientClass");

            migrationBuilder.DropColumn(
                name: "ExternalClassCode",
                schema: "public",
                table: "MstPatientClass");

            migrationBuilder.DropColumn(
                name: "IsForRoomCharge",
                schema: "public",
                table: "MstPatientClass");

            migrationBuilder.DropColumn(
                name: "IsForTariffMapping",
                schema: "public",
                table: "MstPatientClass");

            migrationBuilder.AlterColumn<bool>(
                name: "IsForOutpatient",
                schema: "public",
                table: "MstPatientClass",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientClass_IsForOutpatient_IsForInpatient_IsForEmergen~",
                schema: "public",
                table: "MstPatientClass",
                columns: new[] { "IsForOutpatient", "IsForInpatient", "IsForEmergency", "IsActive", "IsDelete" });
        }
    }
}
