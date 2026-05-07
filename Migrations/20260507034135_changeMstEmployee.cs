using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeMstEmployee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_AttendanceNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_Email",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_EmployeeNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_IdentityNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "AttendanceNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "District",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "Province",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "Village",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.AlterColumn<string>(
                name: "WhatsAppNumber",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Religion",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unknown",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaritalStatus",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unknown",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstEmployee",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "MstEmployee",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "IdentityNumber",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeNumber",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyContactPhone",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "BloodType",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Unknown",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                schema: "public",
                table: "MstEmployee",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CountryId",
                schema: "public",
                table: "MstEmployee",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DistrictId",
                schema: "public",
                table: "MstEmployee",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PostalCodeId",
                schema: "public",
                table: "MstEmployee",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinceId",
                schema: "public",
                table: "MstEmployee",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_BloodType",
                schema: "public",
                table: "MstEmployee",
                column: "BloodType");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_CityId",
                schema: "public",
                table: "MstEmployee",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_CountryId_ProvinceId_CityId_DistrictId_PostalCo~",
                schema: "public",
                table: "MstEmployee",
                columns: new[] { "CountryId", "ProvinceId", "CityId", "DistrictId", "PostalCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_DistrictId",
                schema: "public",
                table: "MstEmployee",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_Email",
                schema: "public",
                table: "MstEmployee",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeNumber",
                schema: "public",
                table: "MstEmployee",
                column: "EmployeeNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeStatus_ProfessionType_IsActive_IsDelete",
                schema: "public",
                table: "MstEmployee",
                columns: new[] { "EmployeeStatus", "ProfessionType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_FullName",
                schema: "public",
                table: "MstEmployee",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_IdentityNumber",
                schema: "public",
                table: "MstEmployee",
                column: "IdentityNumber",
                unique: true,
                filter: "\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_MaritalStatus",
                schema: "public",
                table: "MstEmployee",
                column: "MaritalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_PhoneNumber",
                schema: "public",
                table: "MstEmployee",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_PostalCodeId",
                schema: "public",
                table: "MstEmployee",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_ProvinceId",
                schema: "public",
                table: "MstEmployee",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_Religion",
                schema: "public",
                table: "MstEmployee",
                column: "Religion");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_WhatsAppNumber",
                schema: "public",
                table: "MstEmployee",
                column: "WhatsAppNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_MstEmployee_MstCity_CityId",
                schema: "public",
                table: "MstEmployee",
                column: "CityId",
                principalSchema: "public",
                principalTable: "MstCity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstEmployee_MstCountry_CountryId",
                schema: "public",
                table: "MstEmployee",
                column: "CountryId",
                principalSchema: "public",
                principalTable: "MstCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstEmployee_MstDistrict_DistrictId",
                schema: "public",
                table: "MstEmployee",
                column: "DistrictId",
                principalSchema: "public",
                principalTable: "MstDistrict",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstEmployee_MstPostalCode_PostalCodeId",
                schema: "public",
                table: "MstEmployee",
                column: "PostalCodeId",
                principalSchema: "public",
                principalTable: "MstPostalCode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MstEmployee_MstProvince_ProvinceId",
                schema: "public",
                table: "MstEmployee",
                column: "ProvinceId",
                principalSchema: "public",
                principalTable: "MstProvince",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstEmployee_MstCity_CityId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_MstEmployee_MstCountry_CountryId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_MstEmployee_MstDistrict_DistrictId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_MstEmployee_MstPostalCode_PostalCodeId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropForeignKey(
                name: "FK_MstEmployee_MstProvince_ProvinceId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_BloodType",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_CityId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_CountryId_ProvinceId_CityId_DistrictId_PostalCo~",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_DistrictId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_Email",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_EmployeeNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_EmployeeStatus_ProfessionType_IsActive_IsDelete",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_FullName",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_IdentityNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_MaritalStatus",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_PhoneNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_PostalCodeId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_ProvinceId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_Religion",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropIndex(
                name: "IX_MstEmployee_WhatsAppNumber",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "CityId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "CountryId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "PostalCodeId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                schema: "public",
                table: "MstEmployee");

            migrationBuilder.AlterColumn<string>(
                name: "WhatsAppNumber",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Religion",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Unknown");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MaritalStatus",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Unknown");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "MstEmployee",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "MstEmployee",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "IdentityNumber",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeNumber",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "EmergencyContactPhone",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(13)",
                oldMaxLength: 13,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDateTime",
                schema: "public",
                table: "MstEmployee",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<string>(
                name: "BloodType",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Unknown");

            migrationBuilder.AddColumn<string>(
                name: "AttendanceNumber",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Village",
                schema: "public",
                table: "MstEmployee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_AttendanceNumber",
                schema: "public",
                table: "MstEmployee",
                column: "AttendanceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_Email",
                schema: "public",
                table: "MstEmployee",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_EmployeeNumber",
                schema: "public",
                table: "MstEmployee",
                column: "EmployeeNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstEmployee_IdentityNumber",
                schema: "public",
                table: "MstEmployee",
                column: "IdentityNumber");
        }
    }
}
