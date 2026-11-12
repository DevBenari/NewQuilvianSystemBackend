using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCredentialingAndClinicalPrivilegeEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tambah kolom baru ke WfpCredentialLicense
            migrationBuilder.AddColumn<bool>(
                name: "IsLifetimeStr",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPsvVerified",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PsvVerifiedAt",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PsvVerificationUrl",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PsvVerificationSource",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PsvAuditLogJson",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SipFaskesPracticeNumber",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SipFaskesName",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpiryRadarStatus",
                schema: "public",
                table: "WfpCredentialLicense",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Normal");

            // 2. Buat tabel MstCredentialingCommittee
            migrationBuilder.CreateTable(
                name: "MstCredentialingCommittee",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommitteeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CommitteeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SubCommitteeCredentialName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChairpersonWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecretaryWorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    MinimumPeerReviewersCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstCredentialingCommittee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstCredentialingCommittee_MstWorkforceProfile_Chairperson",
                        column: x => x.ChairpersonWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstCredentialingCommittee_MstWorkforceProfile_Secretary",
                        column: x => x.SecretaryWorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 3. Buat tabel TrxSpkRkkIssuance
            migrationBuilder.CreateTable(
                name: "TrxSpkRkkIssuance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommitteeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CredentialingApplicationId = table.Column<Guid>(type: "uuid", nullable: true),
                    SpkNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    RkkNumber = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClinicalPrivilegeType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Regular"),
                    DirectorGeneralUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DirectorGeneralName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsSignedByDirectorGeneral = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    QrCodeImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SpkPdfFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RkkPdfFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SpkFileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    SpkFileChecksum = table.Column<string>(type: "text", nullable: true),
                    EDossierDocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssuanceStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Active"),
                    IsSuspended = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    SuspendedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SuspensionReason = table.Column<string>(type: "text", nullable: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevocationBapNumber = table.Column<string>(type: "text", nullable: true),
                    RevocationReason = table.Column<string>(type: "text", nullable: true),
                    ApprovedPrivilegesSnapshotJson = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxSpkRkkIssuance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxSpkRkkIssuance_MstWorkforceProfile",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // 4. Buat tabel TrxClinicalPrivilegeEmergencyOverrideLog
            migrationBuilder.CreateTable(
                name: "TrxClinicalPrivilegeEmergencyOverrideLog",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrivilegeItemCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProcedureName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    OverrideReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    OverrideAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WitnessUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    WitnessName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IsReviewedByKmkp = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    KmkpReviewNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    KmkpVerdict = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "PendingReview"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrxClinicalPrivilegeEmergencyOverrideLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxClinicalPrivilegeEmergencyOverrideLog_MstWorkforceProfile",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Indeks
            migrationBuilder.CreateIndex(
                name: "IX_WfpCredentialLicense_WorkforceProfileId_LicenseType",
                schema: "public",
                table: "WfpCredentialLicense",
                columns: new[] { "WorkforceProfileId", "LicenseType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxSpkRkkIssuance_WorkforceProfileId_ValidUntil",
                schema: "public",
                table: "TrxSpkRkkIssuance",
                columns: new[] { "WorkforceProfileId", "ValidUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxSpkRkkIssuance_VerificationHash",
                schema: "public",
                table: "TrxSpkRkkIssuance",
                column: "VerificationHash");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalPrivilegeEmergencyOverrideLog_WorkforceProfileId",
                schema: "public",
                table: "TrxClinicalPrivilegeEmergencyOverrideLog",
                column: "WorkforceProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxClinicalPrivilegeEmergencyOverrideLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxSpkRkkIssuance",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCredentialingCommittee",
                schema: "public");

            migrationBuilder.DropColumn(name: "IsLifetimeStr", schema: "public", table: "WfpCredentialLicense");
            migrationBuilder.DropColumn(name: "IsPsvVerified", schema: "public", table: "WfpCredentialLicense");
            migrationBuilder.DropColumn(name: "PsvVerifiedAt", schema: "public", table: "WfpCredentialLicense");
            migrationBuilder.DropColumn(name: "PsvVerificationUrl", schema: "public", table: "WfpCredentialLicense");
            migrationBuilder.DropColumn(name: "PsvVerificationSource", schema: "public", table: "WfpCredentialLicense");
            migrationBuilder.DropColumn(name: "PsvAuditLogJson", schema: "public", table: "WfpCredentialLicense");
            migrationBuilder.DropColumn(name: "SipFaskesPracticeNumber", schema: "public", table: "WfpCredentialLicense");
            migrationBuilder.DropColumn(name: "SipFaskesName", schema: "public", table: "WfpCredentialLicense");
            migrationBuilder.DropColumn(name: "ExpiryRadarStatus", schema: "public", table: "WfpCredentialLicense");
        }
    }
}
