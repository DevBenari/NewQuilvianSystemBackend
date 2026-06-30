using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class adjustmentAgeCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AgeCalculatedAt",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgeCategoryCodeSnapshot",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AgeCategoryId",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgeCategoryNameSnapshot",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgeDayAtEncounter",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgeMonthAtEncounter",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AgeReferenceDate",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgeTextAtEncounter",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AgeYearAtEncounter",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalAgeDaysAtEncounter",
                schema: "public",
                table: "TrxPatientEncounter",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MstAgeCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgeCategoryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AgeCategoryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    AgeCategoryShortName = table.Column<string>(type: "character varying(75)", maxLength: 75, nullable: true),
                    MinAgeDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MaxAgeDays = table.Column<int>(type: "integer", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSelectableInKiosk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSelectableInRegistration = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsUsedForClinicalRule = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    StandardReference = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000000")),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstAgeCategory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_AgeCategoryId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "AgeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounter_AgeCategoryId_AgeCategoryCodeSnapshot_I~",
                schema: "public",
                table: "TrxPatientEncounter",
                columns: new[] { "AgeCategoryId", "AgeCategoryCodeSnapshot", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstAgeCategory_AgeCategoryCode",
                schema: "public",
                table: "MstAgeCategory",
                column: "AgeCategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstAgeCategory_AgeCategoryName",
                schema: "public",
                table: "MstAgeCategory",
                column: "AgeCategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_MstAgeCategory_IsDefault_IsActive_IsDelete",
                schema: "public",
                table: "MstAgeCategory",
                columns: new[] { "IsDefault", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstAgeCategory_IsSelectableInKiosk_IsSelectableInRegistrati~",
                schema: "public",
                table: "MstAgeCategory",
                columns: new[] { "IsSelectableInKiosk", "IsSelectableInRegistration", "IsUsedForClinicalRule", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstAgeCategory_MinAgeDays_MaxAgeDays_IsActive_IsDelete",
                schema: "public",
                table: "MstAgeCategory",
                columns: new[] { "MinAgeDays", "MaxAgeDays", "IsActive", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientEncounter_MstAgeCategory_AgeCategoryId",
                schema: "public",
                table: "TrxPatientEncounter",
                column: "AgeCategoryId",
                principalSchema: "public",
                principalTable: "MstAgeCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientEncounter_MstAgeCategory_AgeCategoryId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropTable(
                name: "MstAgeCategory",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_AgeCategoryId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounter_AgeCategoryId_AgeCategoryCodeSnapshot_I~",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "AgeCalculatedAt",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "AgeCategoryCodeSnapshot",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "AgeCategoryId",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "AgeCategoryNameSnapshot",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "AgeDayAtEncounter",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "AgeMonthAtEncounter",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "AgeReferenceDate",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "AgeTextAtEncounter",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "AgeYearAtEncounter",
                schema: "public",
                table: "TrxPatientEncounter");

            migrationBuilder.DropColumn(
                name: "TotalAgeDaysAtEncounter",
                schema: "public",
                table: "TrxPatientEncounter");
        }
    }
}
