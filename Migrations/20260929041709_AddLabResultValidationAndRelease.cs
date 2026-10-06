using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabResultValidationAndRelease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReleaseExceptionReasonId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseExceptionReasonNameSnapshot",
                schema: "public",
                table: "LabExamination",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleasedAt",
                schema: "public",
                table: "LabExamination",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReleasedByPositionId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleasedByPositionNameSnapshot",
                schema: "public",
                table: "LabExamination",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReleasedByPrivilegeId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReleasedByUserId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidatedAt",
                schema: "public",
                table: "LabExamination",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ValidatedByPositionId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidatedByPositionNameSnapshot",
                schema: "public",
                table: "LabExamination",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ValidatedByPrivilegeId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ValidatedByUserId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ValidationExceptionReasonId",
                schema: "public",
                table: "LabExamination",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValidationExceptionReasonNameSnapshot",
                schema: "public",
                table: "LabExamination",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabFourEyesExceptionReason",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReasonName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RequiresNote = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_LabFourEyesExceptionReason", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LabResultCorrectionReason",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReasonName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RequiresNote = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_LabResultCorrectionReason", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_FinalizedAt_ValidatedAt",
                schema: "public",
                table: "LabExamination",
                columns: new[] { "FinalizedAt", "ValidatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_ReleasedAt",
                schema: "public",
                table: "LabExamination",
                column: "ReleasedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_ReleaseExceptionReasonId",
                schema: "public",
                table: "LabExamination",
                column: "ReleaseExceptionReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_ValidationExceptionReasonId",
                schema: "public",
                table: "LabExamination",
                column: "ValidationExceptionReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_LabFourEyesExceptionReason_IsActive_SortOrder",
                schema: "public",
                table: "LabFourEyesExceptionReason",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_LabFourEyesExceptionReason_ReasonCode",
                schema: "public",
                table: "LabFourEyesExceptionReason",
                column: "ReasonCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LabResultCorrectionReason_IsActive_SortOrder",
                schema: "public",
                table: "LabResultCorrectionReason",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_LabResultCorrectionReason_ReasonCode",
                schema: "public",
                table: "LabResultCorrectionReason",
                column: "ReasonCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_LabExamination_LabFourEyesExceptionReason_ReleaseExceptionR~",
                schema: "public",
                table: "LabExamination",
                column: "ReleaseExceptionReasonId",
                principalSchema: "public",
                principalTable: "LabFourEyesExceptionReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabExamination_LabFourEyesExceptionReason_ValidationExcepti~",
                schema: "public",
                table: "LabExamination",
                column: "ValidationExceptionReasonId",
                principalSchema: "public",
                principalTable: "LabFourEyesExceptionReason",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabExamination_LabFourEyesExceptionReason_ReleaseExceptionR~",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropForeignKey(
                name: "FK_LabExamination_LabFourEyesExceptionReason_ValidationExcepti~",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropTable(
                name: "LabFourEyesExceptionReason",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LabResultCorrectionReason",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_LabExamination_FinalizedAt_ValidatedAt",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropIndex(
                name: "IX_LabExamination_ReleasedAt",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropIndex(
                name: "IX_LabExamination_ReleaseExceptionReasonId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropIndex(
                name: "IX_LabExamination_ValidationExceptionReasonId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ReleaseExceptionReasonId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ReleaseExceptionReasonNameSnapshot",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ReleasedAt",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ReleasedByPositionId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ReleasedByPositionNameSnapshot",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ReleasedByPrivilegeId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ReleasedByUserId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ValidatedAt",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ValidatedByPositionId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ValidatedByPositionNameSnapshot",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ValidatedByPrivilegeId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ValidatedByUserId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ValidationExceptionReasonId",
                schema: "public",
                table: "LabExamination");

            migrationBuilder.DropColumn(
                name: "ValidationExceptionReasonNameSnapshot",
                schema: "public",
                table: "LabExamination");
        }
    }
}
