using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstDiscountPolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DiscountType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TargetComponent = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ValueType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Limit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    ApproverRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_MstDiscountPolicy", x => x.Id);
                    table.CheckConstraint("CK_MstDiscountPolicy_Approval", "(\"DiscountType\" IN ('PROMO_TOTAL','PROMO_ITEM') AND \"RequiresApproval\" = false AND \"ApproverRole\" IS NULL) OR (\"DiscountType\" = 'DOCTOR' AND \"RequiresApproval\" = true AND \"ApproverRole\" = 'DOCTOR')");
                    table.CheckConstraint("CK_MstDiscountPolicy_EffectivePeriod", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
                    table.CheckConstraint("CK_MstDiscountPolicy_Limit", "\"Limit\" IS NULL OR \"Limit\" > 0");
                    table.CheckConstraint("CK_MstDiscountPolicy_TypeTarget", "(\"DiscountType\" = 'PROMO_TOTAL' AND \"TargetComponent\" = 'PATIENT_PORTION') OR (\"DiscountType\" = 'PROMO_ITEM' AND \"TargetComponent\" = 'INVOICE_ITEM') OR (\"DiscountType\" = 'DOCTOR' AND \"TargetComponent\" = 'DOCTOR_SHARE')");
                    table.CheckConstraint("CK_MstDiscountPolicy_Value", "\"Value\" > 0 AND (\"ValueType\" <> 'PERCENTAGE' OR \"Value\" <= 100)");
                    table.CheckConstraint("CK_MstDiscountPolicy_ValueType", "\"ValueType\" IN ('PERCENTAGE','FIXED_AMOUNT')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiscountPolicy_Code",
                schema: "public",
                table: "MstDiscountPolicy",
                column: "Code",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiscountPolicy_DiscountType_TargetComponent_EffectiveFro~",
                schema: "public",
                table: "MstDiscountPolicy",
                columns: new[] { "DiscountType", "TargetComponent", "EffectiveFrom", "EffectiveTo", "IsActive", "IsDelete" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDiscountPolicy",
                schema: "public");
        }
    }
}
