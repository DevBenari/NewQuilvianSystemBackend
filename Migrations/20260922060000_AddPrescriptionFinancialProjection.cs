using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Salinan keadaan finansial resep menurut Billing (PHA-BE-004, PHA-DEC-071, PHA-DES-001).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Satu tabel baru, nol tabel lain disentuh. Tidak ada kolom yang ditambahkan pada
    /// <c>PhmPrescription</c>, dan tidak ada satu pun baris tabel Billing yang diubah.
    /// </para>
    /// <para>
    /// Atribut <c>[Migration]</c> ditulis inline supaya migration ini tetap terbaca EF ketika
    /// <c>SkipMigrationMetadata</c> menyala — flag yang memangkas berkas Designer demi waktu
    /// build, dan karenanya menyembunyikan migration yang metadata-nya hanya ada di sana.
    /// </para>
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260922060000_AddPrescriptionFinancialProjection")]
    public partial class AddPrescriptionFinancialProjection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhmPrescriptionFinancialProjection",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClearanceStatus = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, defaultValue: "UNKNOWN"),
                    FinancialOutcome = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FinancialVersion = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SourceHandoffId = table.Column<Guid>(type: "uuid", nullable: true),
                    SyncState = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "NEVER_SYNCED"),
                    SyncedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_PhmPrescriptionFinancialProjection", x => x.Id);
                    table.ForeignKey(
                        // Npgsql memotong nama di atas 63 karakter dan menandainya dengan `~`
                        // pada karakter terakhir. Nama penuhnya 68 karakter, sehingga yang
                        // dipakai adalah 62 karakter pertama ditambah `~` — persis nama yang
                        // dihitung EF sendiri. Menuliskannya kurang satu huruf membuat
                        // constraint di database bernama lain daripada yang dikenal model.
                        name: "FK_PhmPrescriptionFinancialProjection_PhmPrescription_Prescrip~",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "PhmPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Tepat satu baris per resep. Inilah yang membuat dua surat untuk resep yang sama,
            // yang diproses bersamaan, tetap berakhir pada satu baris.
            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionFinancialProjection_PrescriptionId",
                schema: "public",
                table: "PhmPrescriptionFinancialProjection",
                column: "PrescriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionFinancialProjection_ClearanceStatus",
                schema: "public",
                table: "PhmPrescriptionFinancialProjection",
                column: "ClearanceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionFinancialProjection_SyncState",
                schema: "public",
                table: "PhmPrescriptionFinancialProjection",
                column: "SyncState");

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionFinancialProjection_InvoiceId",
                schema: "public",
                table: "PhmPrescriptionFinancialProjection",
                column: "InvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhmPrescriptionFinancialProjection",
                schema: "public");
        }
    }
}
