using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeMstPatientClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstPatientClass",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientClassCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientClassName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PatientClassType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ClassLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsForOutpatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsForInpatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsForEmergency = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsForIntensiveCare = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsForNewborn = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DefaultDailyRoomRate = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DefaultRegistrationFee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DefaultConsultationFee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
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
                    table.PrimaryKey("PK_MstPatientClass", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientClass_IsForOutpatient_IsForInpatient_IsForEmergen~",
                schema: "public",
                table: "MstPatientClass",
                columns: new[] { "IsForOutpatient", "IsForInpatient", "IsForEmergency", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientClass_PatientClassCode",
                schema: "public",
                table: "MstPatientClass",
                column: "PatientClassCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientClass_PatientClassName",
                schema: "public",
                table: "MstPatientClass",
                column: "PatientClassName");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientClass_PatientClassType",
                schema: "public",
                table: "MstPatientClass",
                column: "PatientClassType");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientClass_PatientClassType_IsActive_IsDelete",
                schema: "public",
                table: "MstPatientClass",
                columns: new[] { "PatientClassType", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPatientClass",
                schema: "public");
        }
    }
}
