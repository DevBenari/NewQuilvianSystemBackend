using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddMstPaymentMethodAndMstBillingItemCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstBillingItemCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingItemCategoryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BillingItemCategoryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    BillingGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItemSourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Manual"),
                    IsRegistrationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAdministrationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsConsultationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRoomCharge = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsProcedure = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsLaboratory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRadiology = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPharmacy = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDrug = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPackage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDiscount = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsTax = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDeposit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRefund = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCoveredByInsuranceDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNeedDoctor = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEditableInBilling = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSystemCategory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_MstBillingItemCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstPaymentMethod",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethodCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaymentMethodName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PaymentMethodType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Cash"),
                    PaymentGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsCash = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsBankTransfer = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCardPayment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsQris = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsInsurance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCompanyGuarantor = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsMembership = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedReferenceNumber = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedAttachment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAvailableForRegistration = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForBilling = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForRefund = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankAccountNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BankAccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MerchantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TerminalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalPaymentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IntegrationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AdminFeeAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    AdminFeePercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
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
                    table.PrimaryKey("PK_MstPaymentMethod", x => x.Id);
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

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethod_ExternalPaymentCode",
                schema: "public",
                table: "MstPaymentMethod",
                column: "ExternalPaymentCode",
                filter: "\"ExternalPaymentCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethod_IntegrationCode",
                schema: "public",
                table: "MstPaymentMethod",
                column: "IntegrationCode",
                filter: "\"IntegrationCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethod_IsAvailableForBilling_IsActive_IsDelete",
                schema: "public",
                table: "MstPaymentMethod",
                columns: new[] { "IsAvailableForBilling", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethod_IsAvailableForRegistration_IsActive_IsDele~",
                schema: "public",
                table: "MstPaymentMethod",
                columns: new[] { "IsAvailableForRegistration", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethod_IsCash_IsInsurance_IsCompanyGuarantor_IsMe~",
                schema: "public",
                table: "MstPaymentMethod",
                columns: new[] { "IsCash", "IsInsurance", "IsCompanyGuarantor", "IsMembership", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethod_PaymentGroupName",
                schema: "public",
                table: "MstPaymentMethod",
                column: "PaymentGroupName");

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethod_PaymentMethodCode",
                schema: "public",
                table: "MstPaymentMethod",
                column: "PaymentMethodCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethod_PaymentMethodName",
                schema: "public",
                table: "MstPaymentMethod",
                column: "PaymentMethodName");

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethod_PaymentMethodType",
                schema: "public",
                table: "MstPaymentMethod",
                column: "PaymentMethodType");

            migrationBuilder.CreateIndex(
                name: "IX_MstPaymentMethod_PaymentMethodType_IsActive_IsDelete",
                schema: "public",
                table: "MstPaymentMethod",
                columns: new[] { "PaymentMethodType", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstBillingItemCategory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPaymentMethod",
                schema: "public");
        }
    }
}
