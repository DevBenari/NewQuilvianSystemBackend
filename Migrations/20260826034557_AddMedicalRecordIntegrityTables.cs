using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalRecordIntegrityTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrxClinicalNoteAddendum",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxClinicalDocumentIntegrity",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TrxClinicalNoteAuthorDelegation",
                schema: "public");
        }
    }
}
