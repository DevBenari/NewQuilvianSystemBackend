using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyPrescriptionCopy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhmPrescriptionCopy",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CopyNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PharmacistWorkforceId = table.Column<Guid>(type: "uuid", nullable: true),
                    HospitalSiteId = table.Column<Guid>(type: "uuid", nullable: true),
                    SiteNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SiteAddressSnapshot = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SitePhoneSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PharmacistNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PharmacistLicenseNumberSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PharmacistLicenseTypeSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevokeReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_PhmPrescriptionCopy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmPrescriptionCopy_MstHospitalSite_HospitalSiteId",
                        column: x => x.HospitalSiteId,
                        principalSchema: "public",
                        principalTable: "MstHospitalSite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmPrescriptionCopy_MstWorkforceProfile_PharmacistWorkforce~",
                        column: x => x.PharmacistWorkforceId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmPrescriptionCopy_TrxPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "TrxPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhmPrescriptionCopyItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionCopyId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    DrugNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GenericNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StrengthSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DrugFormSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DispenseUnitSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SignaSnapshot = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AdministrationInstructionSnapshot = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsNarcoticSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    IsPsychotropicSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    QuantityPrescribed = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    QuantityDispensed = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    QuantityRemaining = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    Mark = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_PhmPrescriptionCopyItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmPrescriptionCopyItem_PhmPrescriptionCopy_PrescriptionCop~",
                        column: x => x.PrescriptionCopyId,
                        principalSchema: "public",
                        principalTable: "PhmPrescriptionCopy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhmPrescriptionCopyItem_TrxPrescriptionItem_PrescriptionIte~",
                        column: x => x.PrescriptionItemId,
                        principalSchema: "public",
                        principalTable: "TrxPrescriptionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionCopy_CopyNumber",
                schema: "public",
                table: "PhmPrescriptionCopy",
                column: "CopyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionCopy_HospitalSiteId",
                schema: "public",
                table: "PhmPrescriptionCopy",
                column: "HospitalSiteId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionCopy_PharmacistWorkforceId",
                schema: "public",
                table: "PhmPrescriptionCopy",
                column: "PharmacistWorkforceId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionCopy_PrescriptionId_IssuedAt",
                schema: "public",
                table: "PhmPrescriptionCopy",
                columns: new[] { "PrescriptionId", "IssuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionCopyItem_PrescriptionCopyId_PrescriptionItem~",
                schema: "public",
                table: "PhmPrescriptionCopyItem",
                columns: new[] { "PrescriptionCopyId", "PrescriptionItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionCopyItem_PrescriptionItemId",
                schema: "public",
                table: "PhmPrescriptionCopyItem",
                column: "PrescriptionItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhmPrescriptionCopyItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PhmPrescriptionCopy",
                schema: "public");
        }
    }
}
