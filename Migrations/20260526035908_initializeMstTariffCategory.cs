using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeMstTariffCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstTariffCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TariffCategoryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TariffCategoryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    TariffGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsRegistrationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAdministrationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsConsultationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRoomCharge = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsProcedure = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsLaboratory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRadiology = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPharmacy = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSurgery = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPackage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCoveredByInsuranceDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstTariffCategory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariffCategory_IsProcedure_IsLaboratory_IsRadiology_IsPh~",
                schema: "public",
                table: "MstTariffCategory",
                columns: new[] { "IsProcedure", "IsLaboratory", "IsRadiology", "IsPharmacy", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariffCategory_IsRegistrationFee_IsAdministrationFee_IsC~",
                schema: "public",
                table: "MstTariffCategory",
                columns: new[] { "IsRegistrationFee", "IsAdministrationFee", "IsConsultationFee", "IsRoomCharge", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariffCategory_TariffCategoryCode",
                schema: "public",
                table: "MstTariffCategory",
                column: "TariffCategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstTariffCategory_TariffCategoryName",
                schema: "public",
                table: "MstTariffCategory",
                column: "TariffCategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariffCategory_TariffGroupName",
                schema: "public",
                table: "MstTariffCategory",
                column: "TariffGroupName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstTariffCategory",
                schema: "public");
        }
    }
}
