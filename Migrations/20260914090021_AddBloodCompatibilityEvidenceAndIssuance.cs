using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBloodCompatibilityEvidenceAndIssuance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompatibilityEvidenceIdUsed",
                schema: "public",
                table: "BbkBloodUnit",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BbkCompatibilityEvidence",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvidenceResult = table.Column<int>(type: "integer", nullable: false),
                    ValidatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsSuperseded = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_BbkCompatibilityEvidence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BbkCompatibilityEvidence_BbkBloodUnit_BloodUnitId",
                        column: x => x.BloodUnitId,
                        principalSchema: "public",
                        principalTable: "BbkBloodUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BbkCompatibilityEvidence_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodUnit_CompatibilityEvidenceIdUsed",
                schema: "public",
                table: "BbkBloodUnit",
                column: "CompatibilityEvidenceIdUsed");

            migrationBuilder.CreateIndex(
                name: "IX_BbkCompatibilityEvidence_BloodUnitId",
                schema: "public",
                table: "BbkCompatibilityEvidence",
                column: "BloodUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BbkCompatibilityEvidence_BloodUnitId_PatientId",
                schema: "public",
                table: "BbkCompatibilityEvidence",
                columns: new[] { "BloodUnitId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_BbkCompatibilityEvidence_IssuanceLookup",
                schema: "public",
                table: "BbkCompatibilityEvidence",
                columns: new[] { "BloodUnitId", "PatientId", "IsSuperseded", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BbkCompatibilityEvidence_PatientId",
                schema: "public",
                table: "BbkCompatibilityEvidence",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_BbkBloodUnit_BbkCompatibilityEvidence_CompatibilityEvidence~",
                schema: "public",
                table: "BbkBloodUnit",
                column: "CompatibilityEvidenceIdUsed",
                principalSchema: "public",
                principalTable: "BbkCompatibilityEvidence",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BbkBloodUnit_BbkCompatibilityEvidence_CompatibilityEvidence~",
                schema: "public",
                table: "BbkBloodUnit");

            migrationBuilder.DropTable(
                name: "BbkCompatibilityEvidence",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_BbkBloodUnit_CompatibilityEvidenceIdUsed",
                schema: "public",
                table: "BbkBloodUnit");

            migrationBuilder.DropColumn(
                name: "CompatibilityEvidenceIdUsed",
                schema: "public",
                table: "BbkBloodUnit");
        }
    }
}
