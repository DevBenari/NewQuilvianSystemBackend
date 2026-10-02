using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyDuplicateEpisodeOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmgDuplicateEpisodeOverride",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    OverriddenEncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    OverriddenVisitId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OverriddenByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OverriddenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_EmgDuplicateEpisodeOverride", x => x.Id);
                    table.CheckConstraint("CK_EmgDuplicateEpisodeOverride_EpisodeYangDilangkahi", "\"OverriddenEncounterId\" IS NOT NULL OR \"OverriddenVisitId\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_EmgDuplicateEpisodeOverride_AspNetUsers_OverriddenByUserId",
                        column: x => x.OverriddenByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgDuplicateEpisodeOverride_EmgVisit_OverriddenVisitId",
                        column: x => x.OverriddenVisitId,
                        principalSchema: "public",
                        principalTable: "EmgVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgDuplicateEpisodeOverride_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgDuplicateEpisodeOverride_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgDuplicateEpisodeOverride_RegPatientEncounter_OverriddenE~",
                        column: x => x.OverriddenEncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmgDuplicateEpisodeOverride_EncounterId",
                schema: "public",
                table: "EmgDuplicateEpisodeOverride",
                column: "EncounterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmgDuplicateEpisodeOverride_OverriddenByUserId",
                schema: "public",
                table: "EmgDuplicateEpisodeOverride",
                column: "OverriddenByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgDuplicateEpisodeOverride_OverriddenEncounterId",
                schema: "public",
                table: "EmgDuplicateEpisodeOverride",
                column: "OverriddenEncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgDuplicateEpisodeOverride_OverriddenVisitId",
                schema: "public",
                table: "EmgDuplicateEpisodeOverride",
                column: "OverriddenVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgDuplicateEpisodeOverride_PatientId",
                schema: "public",
                table: "EmgDuplicateEpisodeOverride",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM public."EmgDuplicateEpisodeOverride") THEN
                        RAISE EXCEPTION 'Down BE-IGD-053 dihentikan: tabel EmgDuplicateEpisodeOverride berisi catatan pendaftaran ganda. Menghapus tabel ini menghilangkan jejak audit tanpa bekas.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropTable(
                name: "EmgDuplicateEpisodeOverride",
                schema: "public");
        }
    }
}
