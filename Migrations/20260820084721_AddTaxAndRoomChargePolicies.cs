using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxAndRoomChargePolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstRoomChargePolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MinimumMinutes = table.Column<int>(type: "integer", nullable: false),
                    PeriodMinutes = table.Column<int>(type: "integer", nullable: false),
                    RemainderRounding = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TariffMoment = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LeaveRule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_MstRoomChargePolicy", x => x.Id);
                    table.CheckConstraint("CK_MstRoomChargePolicy_EffectivePeriod", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
                    table.CheckConstraint("CK_MstRoomChargePolicy_LeaveRule", "\"LeaveRule\" IN ('INCLUDE_LEAVE','EXCLUDE_LEAVE')");
                    table.CheckConstraint("CK_MstRoomChargePolicy_MinimumMinutes", "\"MinimumMinutes\" >= \"PeriodMinutes\"");
                    table.CheckConstraint("CK_MstRoomChargePolicy_PeriodMinutes", "\"PeriodMinutes\" > 0");
                    table.CheckConstraint("CK_MstRoomChargePolicy_RemainderRounding", "\"RemainderRounding\" IN ('CEILING_PERIOD','PROPORTIONAL','WHOLE_PERIODS')");
                    table.CheckConstraint("CK_MstRoomChargePolicy_TariffMoment", "\"TariffMoment\" IN ('PERIOD_START','OCCUPANCY_START')");
                });

            migrationBuilder.CreateTable(
                name: "MstTaxRule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TaxableCategory = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    RoundingMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AllocationRule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_MstTaxRule", x => x.Id);
                    table.CheckConstraint("CK_MstTaxRule_AllocationRule", "\"AllocationRule\" IN ('PROPORTIONAL','PATIENT','GUARANTOR')");
                    table.CheckConstraint("CK_MstTaxRule_EffectivePeriod", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
                    table.CheckConstraint("CK_MstTaxRule_Rate", "\"Rate\" > 0 AND \"Rate\" <= 100");
                    table.CheckConstraint("CK_MstTaxRule_RoundingMode", "\"RoundingMode\" IN ('HALF_UP','HALF_EVEN','UP','DOWN')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstRoomChargePolicy_Code",
                schema: "public",
                table: "MstRoomChargePolicy",
                column: "Code",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstRoomChargePolicy_EffectiveFrom_EffectiveTo_IsActive_IsDe~",
                schema: "public",
                table: "MstRoomChargePolicy",
                columns: new[] { "EffectiveFrom", "EffectiveTo", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstTaxRule_Code",
                schema: "public",
                table: "MstTaxRule",
                column: "Code",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstTaxRule_TaxableCategory_EffectiveFrom_EffectiveTo_IsActi~",
                schema: "public",
                table: "MstTaxRule",
                columns: new[] { "TaxableCategory", "EffectiveFrom", "EffectiveTo", "IsActive", "IsDelete" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstRoomChargePolicy",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstTaxRule",
                schema: "public");
        }
    }
}
