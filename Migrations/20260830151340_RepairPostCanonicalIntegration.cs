using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class RepairPostCanonicalIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilCalculationVersion_Amounts",
                schema: "public",
                table: "BilCalculationVersion");

            migrationBuilder.AddColumn<string>(
                name: "CashierReferenceNote",
                schema: "public",
                table: "BilTender",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KwitansiNumber",
                schema: "public",
                table: "BilTender",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "public",
                table: "BilSettlement",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RoomChargeAmount",
                schema: "public",
                table: "BilCalculationVersion",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

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
                name: "TrxClinicalDocumentIntegrity",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentKind = table.Column<int>(type: "integer", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    IntegrityStatus = table.Column<int>(type: "integer", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsAuthorKnown = table.Column<bool>(type: "boolean", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SignedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SignatureDeviceInfo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SignatureIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockTrigger = table.Column<int>(type: "integer", nullable: true),
                    LockedEncounterClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AddendumCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_TrxClinicalDocumentIntegrity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxClinicalDocumentIntegrity_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalDocumentIntegrity_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrxClinicalNoteAuthorDelegation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalAuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    GrantReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_TrxClinicalNoteAuthorDelegation", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "TrxClinicalNoteAddendum",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntegrityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsSubstituteAuthor = table.Column<bool>(type: "boolean", nullable: false),
                    DelegationId = table.Column<Guid>(type: "uuid", nullable: true),
                    AddendumText = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CorrectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SignatureDeviceInfo = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SignatureIpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
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
                    table.PrimaryKey("PK_TrxClinicalNoteAddendum", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAddendum_TrxClinicalDocumentIntegrity_Integr~",
                        column: x => x.IntegrityId,
                        principalSchema: "public",
                        principalTable: "TrxClinicalDocumentIntegrity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrxClinicalNoteAddendum_TrxClinicalNoteAuthorDelegation_Del~",
                        column: x => x.DelegationId,
                        principalSchema: "public",
                        principalTable: "TrxClinicalNoteAuthorDelegation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilTender_KwitansiNumber",
                schema: "public",
                table: "BilTender",
                column: "KwitansiNumber",
                unique: true,
                filter: "\"KwitansiNumber\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilCalculationVersion_Amounts",
                schema: "public",
                table: "BilCalculationVersion",
                sql: "\"GrossAmount\" >= 0 AND \"AdministrationFeeAmount\" >= 0 AND \"RoomChargeAmount\" >= 0 AND \"ItemDiscount\" >= 0 AND \"TotalDiscount\" >= 0 AND \"TaxAmount\" >= 0 AND \"PatientAmount\" >= 0 AND \"PrimaryAmount\" >= 0 AND \"ExcessAmount\" >= 0 AND \"UnresolvedCoverageAmount\" >= 0");

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
                name: "IX_TrxClinicalDocumentIntegrity_AuthorUserId_IntegrityStatus_I~",
                schema: "public",
                table: "TrxClinicalDocumentIntegrity",
                columns: new[] { "AuthorUserId", "IntegrityStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalDocumentIntegrity_DocumentKind_DocumentId",
                schema: "public",
                table: "TrxClinicalDocumentIntegrity",
                columns: new[] { "DocumentKind", "DocumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalDocumentIntegrity_EncounterId_IntegrityStatus_Is~",
                schema: "public",
                table: "TrxClinicalDocumentIntegrity",
                columns: new[] { "EncounterId", "IntegrityStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalDocumentIntegrity_PatientId_IntegrityStatus_IsDe~",
                schema: "public",
                table: "TrxClinicalDocumentIntegrity",
                columns: new[] { "PatientId", "IntegrityStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAddendum_AuthorUserId",
                schema: "public",
                table: "TrxClinicalNoteAddendum",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAddendum_DelegationId",
                schema: "public",
                table: "TrxClinicalNoteAddendum",
                column: "DelegationId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAddendum_IntegrityId_Sequence",
                schema: "public",
                table: "TrxClinicalNoteAddendum",
                columns: new[] { "IntegrityId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAuthorDelegation_GrantedByUserId",
                schema: "public",
                table: "TrxClinicalNoteAuthorDelegation",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAuthorDelegation_OriginalAuthorUserId_IsActi~",
                schema: "public",
                table: "TrxClinicalNoteAuthorDelegation",
                columns: new[] { "OriginalAuthorUserId", "IsActive", "ValidUntil" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxClinicalNoteAuthorDelegation_Trigger",
                schema: "public",
                table: "TrxClinicalNoteAuthorDelegation",
                column: "Trigger");

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
                name: "TrxClinicalNoteAddendum",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxMedicalRecordAccessLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxClinicalDocumentIntegrity",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxClinicalNoteAuthorDelegation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstMedicalRecordAccessPurpose",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_BilTender_KwitansiNumber",
                schema: "public",
                table: "BilTender");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilCalculationVersion_Amounts",
                schema: "public",
                table: "BilCalculationVersion");

            migrationBuilder.DropColumn(
                name: "CashierReferenceNote",
                schema: "public",
                table: "BilTender");

            migrationBuilder.DropColumn(
                name: "KwitansiNumber",
                schema: "public",
                table: "BilTender");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.DropColumn(
                name: "RoomChargeAmount",
                schema: "public",
                table: "BilCalculationVersion");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilCalculationVersion_Amounts",
                schema: "public",
                table: "BilCalculationVersion",
                sql: "\"GrossAmount\" >= 0 AND \"AdministrationFeeAmount\" >= 0 AND \"ItemDiscount\" >= 0 AND \"TotalDiscount\" >= 0 AND \"TaxAmount\" >= 0 AND \"PatientAmount\" >= 0 AND \"PrimaryAmount\" >= 0 AND \"ExcessAmount\" >= 0 AND \"UnresolvedCoverageAmount\" >= 0");
        }
    }
}
