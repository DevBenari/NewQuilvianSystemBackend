using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBbkBloodGroupExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BbkBloodGroupExam",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    AboRhesusResult = table.Column<int>(type: "integer", nullable: true),
                    ExamStatus = table.Column<int>(type: "integer", nullable: false),
                    ExaminedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExaminedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsValidResult = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsConflictHeld = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Version = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_BbkBloodGroupExam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkBloodGroupExam_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BbkBloodGroupConflictResolution",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResolvingExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_BbkBloodGroupConflictResolution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkBloodGroupConflictResolution_BbkBloodGroupExam_Resolving~",
                        column: x => x.ResolvingExamId,
                        principalSchema: "public",
                        principalTable: "BbkBloodGroupExam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkBloodGroupConflictResolution_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BbkBloodGroupSample",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodGroupExamId = table.Column<Guid>(type: "uuid", nullable: false),
                    SampleIdentifier = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TakenByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TakenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_BbkBloodGroupSample", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkBloodGroupSample_BbkBloodGroupExam_BloodGroupExamId",
                        column: x => x.BloodGroupExamId,
                        principalSchema: "public",
                        principalTable: "BbkBloodGroupExam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodGroupConflictResolution_PatientId",
                schema: "public",
                table: "BbkBloodGroupConflictResolution",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodGroupConflictResolution_ResolvingExamId",
                schema: "public",
                table: "BbkBloodGroupConflictResolution",
                column: "ResolvingExamId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodGroupExam_ExamStatus",
                schema: "public",
                table: "BbkBloodGroupExam",
                column: "ExamStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodGroupExam_IsConflictHeld",
                schema: "public",
                table: "BbkBloodGroupExam",
                column: "IsConflictHeld");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodGroupExam_IsValidResult",
                schema: "public",
                table: "BbkBloodGroupExam",
                column: "IsValidResult");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodGroupExam_PatientId",
                schema: "public",
                table: "BbkBloodGroupExam",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodGroupSample_BloodGroupExamId",
                schema: "public",
                table: "BbkBloodGroupSample",
                column: "BloodGroupExamId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodGroupSample_SampleIdentifier",
                schema: "public",
                table: "BbkBloodGroupSample",
                column: "SampleIdentifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BbkBloodGroupConflictResolution",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BbkBloodGroupSample",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BbkBloodGroupExam",
                schema: "public");
        }
    }
}
