using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeAspUserFingerprint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUserFingerprintCredential",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    FingerPosition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TemplateFormat = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TemplateVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TemplateDataEncrypted = table.Column<byte[]>(type: "bytea", nullable: false),
                    TemplateHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SampleFormat = table.Column<int>(type: "integer", nullable: true),
                    QualityScore = table.Column<int>(type: "integer", nullable: true),
                    EnrollmentSampleCount = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RegisteredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisteredIpAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RegisteredUserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_ApplicationUserFingerprintCredential", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUserFingerprintCredential_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationUserFingerprintCredential_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationUserFingerprintCredential_MstEmployee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApplicationUserFingerprintCredential_MstWorkforceProfile_Wo~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFingerprintCredential_DoctorId",
                schema: "public",
                table: "ApplicationUserFingerprintCredential",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFingerprintCredential_EmployeeId",
                schema: "public",
                table: "ApplicationUserFingerprintCredential",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFingerprintCredential_TemplateHash",
                schema: "public",
                table: "ApplicationUserFingerprintCredential",
                column: "TemplateHash");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFingerprintCredential_UserId",
                schema: "public",
                table: "ApplicationUserFingerprintCredential",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFingerprintCredential_UserId_FingerPosition",
                schema: "public",
                table: "ApplicationUserFingerprintCredential",
                columns: new[] { "UserId", "FingerPosition" },
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFingerprintCredential_UserId_IsActive_IsDele~",
                schema: "public",
                table: "ApplicationUserFingerprintCredential",
                columns: new[] { "UserId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFingerprintCredential_UserId_IsPrimary",
                schema: "public",
                table: "ApplicationUserFingerprintCredential",
                columns: new[] { "UserId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserFingerprintCredential_WorkforceProfileId",
                schema: "public",
                table: "ApplicationUserFingerprintCredential",
                column: "WorkforceProfileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationUserFingerprintCredential",
                schema: "public");
        }
    }
}
