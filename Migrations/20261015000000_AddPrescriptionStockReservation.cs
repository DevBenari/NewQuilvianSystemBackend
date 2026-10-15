using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Penahanan stok milik resep — <c>PHA-DEC-066</c>, <c>PHA-BE-003</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Seluruhnya additive: satu tabel baru dan indeksnya. Tidak ada kolom yang dihapus, tidak
    /// ada tipe yang berubah, dan tidak ada tabel lain yang tersentuh. Basis data yang sudah
    /// berjalan karena itu tidak kehilangan apa pun.
    /// </para>
    /// <para>
    /// <b>Mengapa tabel, bukan kolom tambahan.</b> Sebelum <c>PHA-DEC-066</c>, satu-satunya
    /// jejak penahanan adalah penghitung <c>PhmDrugStockBalance.QuantityReserved</c>. Penghitung
    /// itu tidak dapat menyatakan kepemilikan oleh resep, batas waktu, konsumsi sekali saja,
    /// maupun alasan pelepasan. Granularitasnya per batch karena FEFO menyebar satu permintaan
    /// ke beberapa batch, sehingga satu baris resep dapat memiliki beberapa baris di sini.
    /// Penghitung lamanya <b>tetap dipakai apa adanya</b> sebagai sumber kebenaran jumlah.
    /// </para>
    /// <para>
    /// Ketiga indeks unik bersyarat adalah penjaga terakhir terhadap penahanan ganda. Bila dua
    /// permintaan finalisasi lolos pemeriksaan idempotency di service pada saat yang sama,
    /// basis data yang menolak yang kedua. Saringan <c>"State" = 1</c> membuat penjaga itu
    /// hanya berlaku bagi penahanan yang masih aktif, sehingga resep yang penahanannya pernah
    /// lepas tetap boleh menahan ulang.
    /// </para>
    /// <para>
    /// Atribut <c>[Migration]</c> dan <c>[DbContext]</c> sengaja ditulis di berkas ini. EF
    /// menyaring migration dengan membandingkan <c>DbContextAttribute.ContextType</c> terhadap
    /// context yang aktif; migration yang hanya membawa <c>[Migration]</c> tidak terlihat sama
    /// sekali, tanpa satu pun peringatan build. Lihat
    /// <c>20260929000000_AddOperatingRoomOutboxEventContract</c>, tempat cacat itu pertama
    /// ditemukan.
    /// </para>
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261015000000_AddPrescriptionStockReservation")]
    public partial class AddPrescriptionStockReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhmPrescriptionStockReservation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrescriptionCompoundItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageLocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReleaseReasonCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceEvent = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: ""),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, defaultValue: ""),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhmPrescriptionStockReservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmPrescriptionStockReservation_PhmPrescription_Prescriptio~",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "PhmPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmPrescriptionStockReservation_PhmPrescriptionItem_Prescri~",
                        column: x => x.PrescriptionItemId,
                        principalSchema: "public",
                        principalTable: "PhmPrescriptionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmPrescriptionStockReservation_PhmDrugBatch_DrugBatchId",
                        column: x => x.DrugBatchId,
                        principalSchema: "public",
                        principalTable: "PhmDrugBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmPrescriptionStockReservation_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmPrescriptionStockReservation_MstDrugStorageLocation_Stora~",
                        column: x => x.StorageLocationId,
                        principalSchema: "public",
                        principalTable: "MstDrugStorageLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionStockReservation_PrescriptionId_State",
                schema: "public",
                table: "PhmPrescriptionStockReservation",
                columns: new[] { "PrescriptionId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionStockReservation_ExpiresAt",
                schema: "public",
                table: "PhmPrescriptionStockReservation",
                column: "ExpiresAt",
                filter: "\"State\" = 1 AND \"ExpiresAt\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionStockReservation_IdempotencyKey",
                schema: "public",
                table: "PhmPrescriptionStockReservation",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionStockReservation_Item_Batch_Active",
                schema: "public",
                table: "PhmPrescriptionStockReservation",
                columns: new[] { "PrescriptionId", "PrescriptionItemId", "DrugBatchId" },
                unique: true,
                filter: "\"State\" = 1 AND \"PrescriptionItemId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionStockReservation_Compound_Batch_Active",
                schema: "public",
                table: "PhmPrescriptionStockReservation",
                columns: new[] { "PrescriptionId", "PrescriptionCompoundItemId", "DrugBatchId" },
                unique: true,
                filter: "\"State\" = 1 AND \"PrescriptionCompoundItemId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionStockReservation_DrugId",
                schema: "public",
                table: "PhmPrescriptionStockReservation",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionStockReservation_StorageLocationId",
                schema: "public",
                table: "PhmPrescriptionStockReservation",
                column: "StorageLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhmPrescriptionStockReservation",
                schema: "public");
        }
    }
}
