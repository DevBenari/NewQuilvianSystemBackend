using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRegPatientGeneralConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegPatientGeneralConsent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientEncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignerType = table.Column<int>(type: "integer", nullable: false),
                    SignerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SignerRelationshipText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SignerIdentityNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SignerPhoneNumber = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    SignerAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AgreedClauses = table.Column<int[]>(type: "integer[]", nullable: false),
                    SignatureImageBase64 = table.Column<string>(type: "text", nullable: false),
                    SignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_RegPatientGeneralConsent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegPatientGeneralConsent_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegPatientGeneralConsent_RegPatientEncounter_PatientEncount~",
                        column: x => x.PatientEncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegPatientGeneralConsent_PatientEncounterId",
                schema: "public",
                table: "RegPatientGeneralConsent",
                column: "PatientEncounterId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RegPatientGeneralConsent_PatientId",
                schema: "public",
                table: "RegPatientGeneralConsent",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegPatientGeneralConsent",
                schema: "public");
        }
    }
}
