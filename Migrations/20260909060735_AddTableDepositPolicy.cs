using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTableDepositPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "MstDepositPolicy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    GuarantorId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MinimumAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FollowUpIntervalDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EffectiveTo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_MstDepositPolicy", x => x.Id);
                    table.CheckConstraint("CK_MstDepositPolicy_EffectivePeriod", "\"EffectiveTo\" IS NULL OR \"EffectiveTo\" > \"EffectiveFrom\"");
                    table.CheckConstraint("CK_MstDepositPolicy_FollowUpInterval", "\"FollowUpIntervalDays\" >= 0");
                    table.CheckConstraint("CK_MstDepositPolicy_MinimumAmount", "\"MinimumAmount\" >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId_IsPrimary",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "EncounterId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId_Priority",
                schema: "public",
                table: "TrxPatientEncounterGuarantor",
                columns: new[] { "EncounterId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDepositPolicy_Code",
                schema: "public",
                table: "MstDepositPolicy",
                column: "Code",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstDepositPolicy_GuarantorId_PatientClassId_EffectiveFrom_E~",
                schema: "public",
                table: "MstDepositPolicy",
                columns: new[] { "GuarantorId", "PatientClassId", "EffectiveFrom", "EffectiveTo", "IsActive", "IsDelete" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDepositPolicy",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId_IsPrimary",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientEncounterGuarantor_EncounterId_Priority",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "public",
                table: "TrxPatientEncounterGuarantor");
        }
    }
}
