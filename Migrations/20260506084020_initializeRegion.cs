using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeRegion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstCountry",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CountryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PhoneCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstCountry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstProvince",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProvinceCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProvinceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstProvince", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstProvince_MstCountry_CountryId",
                        column: x => x.CountryId,
                        principalSchema: "public",
                        principalTable: "MstCountry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstCity",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProvinceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CityCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CityName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CityType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstCity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstCity_MstProvince_ProvinceId",
                        column: x => x.ProvinceId,
                        principalSchema: "public",
                        principalTable: "MstProvince",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstDistrict",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DistrictName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstDistrict", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDistrict_MstCity_CityId",
                        column: x => x.CityId,
                        principalSchema: "public",
                        principalTable: "MstCity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstPostalCode",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VillageName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstPostalCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPostalCode_MstDistrict_DistrictId",
                        column: x => x.DistrictId,
                        principalSchema: "public",
                        principalTable: "MstDistrict",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstCity_CityType",
                schema: "public",
                table: "MstCity",
                column: "CityType");

            migrationBuilder.CreateIndex(
                name: "IX_MstCity_IsActive_IsDelete",
                schema: "public",
                table: "MstCity",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstCity_ProvinceId_CityCode",
                schema: "public",
                table: "MstCity",
                columns: new[] { "ProvinceId", "CityCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstCity_ProvinceId_CityName",
                schema: "public",
                table: "MstCity",
                columns: new[] { "ProvinceId", "CityName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstCountry_CountryCode",
                schema: "public",
                table: "MstCountry",
                column: "CountryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstCountry_CountryName",
                schema: "public",
                table: "MstCountry",
                column: "CountryName");

            migrationBuilder.CreateIndex(
                name: "IX_MstCountry_IsActive_IsDelete",
                schema: "public",
                table: "MstCountry",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDistrict_CityId_DistrictCode",
                schema: "public",
                table: "MstDistrict",
                columns: new[] { "CityId", "DistrictCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDistrict_CityId_DistrictName",
                schema: "public",
                table: "MstDistrict",
                columns: new[] { "CityId", "DistrictName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDistrict_IsActive_IsDelete",
                schema: "public",
                table: "MstDistrict",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPostalCode_DistrictId_PostalCode",
                schema: "public",
                table: "MstPostalCode",
                columns: new[] { "DistrictId", "PostalCode" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPostalCode_DistrictId_VillageName",
                schema: "public",
                table: "MstPostalCode",
                columns: new[] { "DistrictId", "VillageName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPostalCode_IsActive_IsDelete",
                schema: "public",
                table: "MstPostalCode",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPostalCode_PostalCode",
                schema: "public",
                table: "MstPostalCode",
                column: "PostalCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstProvince_CountryId_ProvinceCode",
                schema: "public",
                table: "MstProvince",
                columns: new[] { "CountryId", "ProvinceCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstProvince_CountryId_ProvinceName",
                schema: "public",
                table: "MstProvince",
                columns: new[] { "CountryId", "ProvinceName" });

            migrationBuilder.CreateIndex(
                name: "IX_MstProvince_IsActive_IsDelete",
                schema: "public",
                table: "MstProvince",
                columns: new[] { "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPostalCode",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDistrict",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCity",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstProvince",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCountry",
                schema: "public");
        }
    }
}
