using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyVisitClosureSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClosedByDispositionId",
                schema: "public",
                table: "EmgVisit",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmgVisit_ClosedByDispositionId",
                schema: "public",
                table: "EmgVisit",
                column: "ClosedByDispositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmgVisit_EmgDisposition_ClosedByDispositionId",
                schema: "public",
                table: "EmgVisit",
                column: "ClosedByDispositionId",
                principalSchema: "public",
                principalTable: "EmgDisposition",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM public."EmgVisit" WHERE "ClosedByDispositionId" IS NOT NULL) THEN
                        RAISE EXCEPTION 'Down BE-IGD-060 dihentikan: ada kunjungan IGD yang ditutup lewat disposisi (ClosedByDispositionId terisi). Menghapus kolom ini menghilangkan jejak asal penutupan tanpa bekas.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_EmgVisit_EmgDisposition_ClosedByDispositionId",
                schema: "public",
                table: "EmgVisit");

            migrationBuilder.DropIndex(
                name: "IX_EmgVisit_ClosedByDispositionId",
                schema: "public",
                table: "EmgVisit");

            migrationBuilder.DropColumn(
                name: "ClosedByDispositionId",
                schema: "public",
                table: "EmgVisit");
        }
    }
}
