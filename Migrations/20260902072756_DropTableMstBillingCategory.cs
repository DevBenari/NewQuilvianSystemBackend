using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class DropTableMstBillingCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BilInvoiceItem_MstBillingItemCategory_CategoryId",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropTable(
                name: "MstBillingItemCategory",
                schema: "public");

            migrationBuilder.AddColumn<bool>(
                name: "IsCoveredByInsuranceDefault",
                schema: "public",
                table: "MstTariffCategory",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_BilInvoiceItem_MstTariffCategory_CategoryId",
                schema: "public",
                table: "BilInvoiceItem",
                column: "CategoryId",
                principalSchema: "public",
                principalTable: "MstTariffCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BilInvoiceItem_MstTariffCategory_CategoryId",
                schema: "public",
                table: "BilInvoiceItem");

            migrationBuilder.DropColumn(
                name: "IsCoveredByInsuranceDefault",
                schema: "public",
                table: "MstTariffCategory");

            migrationBuilder.CreateTable(
                name: "MstBillingItemCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BillingItemCategoryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BillingItemCategoryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAdministrationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsConsultationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCoveredByInsuranceDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDeposit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDiscount = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDrug = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEditableInBilling = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsLaboratory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedDoctor = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPackage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPharmacy = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsProcedure = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRadiology = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRefund = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRegistrationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRoomCharge = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSystemCategory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsTax = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ItemSourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Manual"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstBillingItemCategory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingItemCategory_BillingGroupName",
                schema: "public",
                table: "MstBillingItemCategory",
                column: "BillingGroupName");

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingItemCategory_BillingItemCategoryCode",
                schema: "public",
                table: "MstBillingItemCategory",
                column: "BillingItemCategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingItemCategory_BillingItemCategoryName",
                schema: "public",
                table: "MstBillingItemCategory",
                column: "BillingItemCategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingItemCategory_IsActive_IsDelete",
                schema: "public",
                table: "MstBillingItemCategory",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingItemCategory_IsCoveredByInsuranceDefault_IsNeedAp~",
                schema: "public",
                table: "MstBillingItemCategory",
                columns: new[] { "IsCoveredByInsuranceDefault", "IsNeedApproval", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingItemCategory_IsRegistrationFee_IsAdministrationFe~",
                schema: "public",
                table: "MstBillingItemCategory",
                columns: new[] { "IsRegistrationFee", "IsAdministrationFee", "IsConsultationFee", "IsRoomCharge", "IsProcedure", "IsLaboratory", "IsRadiology", "IsPharmacy", "IsDrug", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingItemCategory_ItemSourceType",
                schema: "public",
                table: "MstBillingItemCategory",
                column: "ItemSourceType");

            migrationBuilder.CreateIndex(
                name: "IX_MstBillingItemCategory_ItemSourceType_IsActive_IsDelete",
                schema: "public",
                table: "MstBillingItemCategory",
                columns: new[] { "ItemSourceType", "IsActive", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_BilInvoiceItem_MstBillingItemCategory_CategoryId",
                schema: "public",
                table: "BilInvoiceItem",
                column: "CategoryId",
                principalSchema: "public",
                principalTable: "MstBillingItemCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
