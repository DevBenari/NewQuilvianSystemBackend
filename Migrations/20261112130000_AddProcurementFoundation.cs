using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-PBJ-002 (PBJ-BP-001, prefix Prc — PBJ-DEC-035): tabel pertama modul Pengadaan Barang &amp;
    /// Jasa, yaitu riwayat dokumen bersama <c>PrcDocumentHistory</c>. Tabel baru tanpa FK; tidak
    /// menyentuh tabel lain dan tidak ada pengisian data, sehingga aman dijalankan tanpa downtime.
    /// Tabel master dan katalog Pengadaan menyusul pada migration BE-PBJ-003.
    /// </summary>
    public partial class AddProcurementFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrcDocumentHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ActionCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_PrcDocumentHistory", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrcDocumentHistory_ActorUserId",
                schema: "public",
                table: "PrcDocumentHistory",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrcDocumentHistory_DocumentNumber",
                schema: "public",
                table: "PrcDocumentHistory",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PrcDocumentHistory_DocumentType_DocumentId_OccurredAt",
                schema: "public",
                table: "PrcDocumentHistory",
                columns: new[] { "DocumentType", "DocumentId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrcDocumentHistory",
                schema: "public");
        }
    }
}
