using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddSupplierAndDrugSupplier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstSupplier",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SupplierName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    LegalName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    SupplierType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    SupplierGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TaxNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BusinessLicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactPersonName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WhatsAppNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Website = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProvinceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CountryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankAccountName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PaymentTermDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MinimumPurchaseAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    CreditLimitAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TaxPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    IsPrincipal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDistributor = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsManufacturer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPharmacySupplier = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsMedicalDeviceSupplier = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsLaboratorySupplier = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsConsumableSupplier = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPreferredSupplier = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsBlacklisted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    BlacklistReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstSupplier", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstDrugSupplier",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugSupplierCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SupplierDrugCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SupplierDrugName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PurchaseUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    MinimumOrderQuantity = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    OrderMultipleQuantity = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    MaximumOrderQuantity = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    MinimumPurchaseAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DefaultPurchasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    LastPurchasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ContractPurchasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    TaxPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsPreferredSupplier = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsContractSupplier = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDefaultForPurchase = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAllowPurchase = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsRequireQuotation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_MstDrugSupplier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDrugSupplier_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugSupplier_MstMeasurement_PurchaseUnitMeasurementId",
                        column: x => x.PurchaseUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrugSupplier_MstSupplier_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "public",
                        principalTable: "MstSupplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugSupplier_Drug_Supplier_NullPurchaseUnit",
                schema: "public",
                table: "MstDrugSupplier",
                columns: new[] { "DrugId", "SupplierId" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"PurchaseUnitMeasurementId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugSupplier_Drug_Supplier_PurchaseUnit",
                schema: "public",
                table: "MstDrugSupplier",
                columns: new[] { "DrugId", "SupplierId", "PurchaseUnitMeasurementId" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"PurchaseUnitMeasurementId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugSupplier_DrugId",
                schema: "public",
                table: "MstDrugSupplier",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugSupplier_DrugId_IsPreferredSupplier_IsDefaultForPurc~",
                schema: "public",
                table: "MstDrugSupplier",
                columns: new[] { "DrugId", "IsPreferredSupplier", "IsDefaultForPurchase", "IsAllowPurchase", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugSupplier_DrugSupplierCode",
                schema: "public",
                table: "MstDrugSupplier",
                column: "DrugSupplierCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugSupplier_EffectiveStartDate_EffectiveEndDate_IsActiv~",
                schema: "public",
                table: "MstDrugSupplier",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugSupplier_PurchaseUnitMeasurementId",
                schema: "public",
                table: "MstDrugSupplier",
                column: "PurchaseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugSupplier_SupplierDrugCode",
                schema: "public",
                table: "MstDrugSupplier",
                column: "SupplierDrugCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugSupplier_SupplierId",
                schema: "public",
                table: "MstDrugSupplier",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugSupplier_SupplierId_IsContractSupplier_IsAllowPurcha~",
                schema: "public",
                table: "MstDrugSupplier",
                columns: new[] { "SupplierId", "IsContractSupplier", "IsAllowPurchase", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugSupplier_SupplierId_SupplierDrugCode",
                schema: "public",
                table: "MstDrugSupplier",
                columns: new[] { "SupplierId", "SupplierDrugCode" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"SupplierDrugCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstSupplier_Email",
                schema: "public",
                table: "MstSupplier",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_MstSupplier_IsPharmacySupplier_IsMedicalDeviceSupplier_IsLa~",
                schema: "public",
                table: "MstSupplier",
                columns: new[] { "IsPharmacySupplier", "IsMedicalDeviceSupplier", "IsLaboratorySupplier", "IsConsumableSupplier", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstSupplier_IsPreferredSupplier_IsBlacklisted_IsActive_IsDe~",
                schema: "public",
                table: "MstSupplier",
                columns: new[] { "IsPreferredSupplier", "IsBlacklisted", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstSupplier_PhoneNumber",
                schema: "public",
                table: "MstSupplier",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_MstSupplier_SupplierCode",
                schema: "public",
                table: "MstSupplier",
                column: "SupplierCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstSupplier_SupplierGroupName",
                schema: "public",
                table: "MstSupplier",
                column: "SupplierGroupName");

            migrationBuilder.CreateIndex(
                name: "IX_MstSupplier_SupplierName",
                schema: "public",
                table: "MstSupplier",
                column: "SupplierName");

            migrationBuilder.CreateIndex(
                name: "IX_MstSupplier_SupplierType",
                schema: "public",
                table: "MstSupplier",
                column: "SupplierType");

            migrationBuilder.CreateIndex(
                name: "IX_MstSupplier_SupplierType_SupplierGroupName_IsActive_IsDelete",
                schema: "public",
                table: "MstSupplier",
                columns: new[] { "SupplierType", "SupplierGroupName", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstSupplier_TaxNumber",
                schema: "public",
                table: "MstSupplier",
                column: "TaxNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDrugSupplier",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstSupplier",
                schema: "public");
        }
    }
}
