using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalRecordAccessAuditTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstMedicalRecordAccessPurpose",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PurposeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PurposeName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsFreeTextRequired = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresReview = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstMedicalRecordAccessPurpose", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrxMedicalRecordAccessLog",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserDisplayNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserRoleSnapshot = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    AccessType = table.Column<int>(type: "integer", nullable: false),
                    AccessScope = table.Column<int>(type: "integer", nullable: false),
                    AccessPurposeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccessReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    HasActiveEncounter = table.Column<bool>(type: "boolean", nullable: false),
                    IsFlaggedForReview = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ClientInfo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    RequestPath = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxMedicalRecordAccessLog", x => new { x.Id, x.AccessedAt });
                    table.ForeignKey(
                        name: "FK_TrxMedicalRecordAccessLog_MstMedicalRecordAccessPurpose_Acc~",
                        column: x => x.AccessPurposeId,
                        principalSchema: "public",
                        principalTable: "MstMedicalRecordAccessPurpose",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxMedicalRecordAccessLog_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalRecordAccessPurpose_IsActive_SortOrder",
                schema: "public",
                table: "MstMedicalRecordAccessPurpose",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MstMedicalRecordAccessPurpose_PurposeCode",
                schema: "public",
                table: "MstMedicalRecordAccessPurpose",
                column: "PurposeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalRecordAccessLog_AccessPurposeId",
                schema: "public",
                table: "TrxMedicalRecordAccessLog",
                column: "AccessPurposeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalRecordAccessLog_AccessType_AccessedAt",
                schema: "public",
                table: "TrxMedicalRecordAccessLog",
                columns: new[] { "AccessType", "AccessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalRecordAccessLog_IsFlaggedForReview_ReviewedAt_Acc~",
                schema: "public",
                table: "TrxMedicalRecordAccessLog",
                columns: new[] { "IsFlaggedForReview", "ReviewedAt", "AccessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalRecordAccessLog_PatientId_AccessedAt",
                schema: "public",
                table: "TrxMedicalRecordAccessLog",
                columns: new[] { "PatientId", "AccessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxMedicalRecordAccessLog_UserId_AccessedAt",
                schema: "public",
                table: "TrxMedicalRecordAccessLog",
                columns: new[] { "UserId", "AccessedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxMedicalRecordAccessLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstMedicalRecordAccessPurpose",
                schema: "public");
        }
    }
}
