using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministrationFeePolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstAdministrationFeePolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ServiceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OncePerPatientLocalDay = table.Column<bool>(type: "boolean", nullable: false),
                    ReplacementPriority = table.Column<int>(type: "integer", nullable: false),
                    Coverable = table.Column<bool>(type: "boolean", nullable: false),
                    Discountable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_MstAdministrationFeePolicy", x => x.Id);
                    table.CheckConstraint("CK_MstAdministrationFeePolicy_Amount", "\"Amount\" >= 0");
                    table.CheckConstraint("CK_MstAdministrationFeePolicy_EffectivePeriod", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
                    table.CheckConstraint("CK_MstAdministrationFeePolicy_NotDiscountable", "\"Discountable\" = false");
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "MstAdministrationFeePolicy",
                columns: new[] { "Id", "Amount", "CancelBy", "CancelDateTime", "Code", "Coverable", "CreateBy", "CreateDateTime", "DeleteBy", "DeleteDateTime", "EffectiveFrom", "EffectiveTo", "Name", "OncePerPatientLocalDay", "ReplacementPriority", "ServiceType", "UpdateBy", "UpdateDateTime" },
                values: new object[,]
                {
                    { new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b801"), 0m, new Guid("00000000-0000-0000-0000-000000000000"), null, "ADM-RAJAL-DRAFT", false, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 8, 20, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, new DateTimeOffset(new DateTime(2026, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 8, 20, 0, 0, 1, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Draft biaya administrasi rawat jalan", true, 10, "RAJAL", new Guid("00000000-0000-0000-0000-000000000000"), null },
                    { new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b802"), 0m, new Guid("00000000-0000-0000-0000-000000000000"), null, "ADM-IGD-DRAFT", false, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 8, 20, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, new DateTimeOffset(new DateTime(2026, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 8, 20, 0, 0, 1, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Draft biaya administrasi IGD", true, 10, "IGD", new Guid("00000000-0000-0000-0000-000000000000"), null },
                    { new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b803"), 0m, new Guid("00000000-0000-0000-0000-000000000000"), null, "ADM-OTC-DRAFT", false, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 8, 20, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, new DateTimeOffset(new DateTime(2026, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 8, 20, 0, 0, 1, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Draft biaya administrasi OTC", true, 10, "OTC", new Guid("00000000-0000-0000-0000-000000000000"), null },
                    { new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b804"), 0m, new Guid("00000000-0000-0000-0000-000000000000"), null, "ADM-RANAP-DRAFT", false, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 8, 20, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, new DateTimeOffset(new DateTime(2026, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 8, 20, 0, 0, 1, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Draft biaya administrasi rawat inap", false, 100, "RANAP", new Guid("00000000-0000-0000-0000-000000000000"), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstAdministrationFeePolicy_Code",
                schema: "public",
                table: "MstAdministrationFeePolicy",
                column: "Code",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstAdministrationFeePolicy_ServiceType_EffectiveFrom_Effect~",
                schema: "public",
                table: "MstAdministrationFeePolicy",
                columns: new[] { "ServiceType", "EffectiveFrom", "EffectiveTo", "IsActive", "IsDelete" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstAdministrationFeePolicy",
                schema: "public");
        }
    }
}
