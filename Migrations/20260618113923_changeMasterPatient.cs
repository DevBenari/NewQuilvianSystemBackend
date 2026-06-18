using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeMasterPatient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RawScanText",
                schema: "public",
                table: "TrxKioskScanSession",
                type: "character varying(12000)",
                maxLength: 12000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ParsedJson",
                schema: "public",
                table: "TrxKioskScanSession",
                type: "character varying(12000)",
                maxLength: 12000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScanImageContentType",
                schema: "public",
                table: "TrxKioskScanSession",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScanImagePath",
                schema: "public",
                table: "TrxKioskScanSession",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScanImageContentType",
                schema: "public",
                table: "TrxKioskScanSession");

            migrationBuilder.DropColumn(
                name: "ScanImagePath",
                schema: "public",
                table: "TrxKioskScanSession");

            migrationBuilder.AlterColumn<string>(
                name: "RawScanText",
                schema: "public",
                table: "TrxKioskScanSession",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(12000)",
                oldMaxLength: 12000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ParsedJson",
                schema: "public",
                table: "TrxKioskScanSession",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(12000)",
                oldMaxLength: 12000,
                oldNullable: true);
        }
    }
}
