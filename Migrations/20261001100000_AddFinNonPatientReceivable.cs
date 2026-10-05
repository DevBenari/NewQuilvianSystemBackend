using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-056 (FIN-DEC-099..104, 02-backend-architecture.md AMENDMENT REVISI 13 §K). Membuat
    /// skema piutang sewa non-pasien — parkir dan tenant. DUA TABEL BARU, nol tabel lama disentuh.
    ///
    /// FinNonPatientReceivable adalah aggregate BERDIRI SENDIRI, TERPISAH PENUH dari FinReceivable
    /// (FIN-DEC-101) — nol kolom rujukan ke BilInvoice/FinReceivable/FinReceipt. Invariant "piutang
    /// pasien wajib dari serah terima Billing" pada FinReceivable TIDAK disentuh migration ini.
    ///
    /// FinNonPatientReceivableSettlement adalah anak langsung (FK Restrict) — pelunasan dicatat di
    /// sini, BELUM tersambung ke FinReceipt/kas harian/kejadian akuntansi (FIN-OQ-044, di luar
    /// cakupan migration ini).
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinNonPatientReceivable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinNonPatientReceivable",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivableNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CounterpartyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RentedObject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PeriodStart = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEnd = table.Column<DateOnly>(type: "date", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BilledAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LateFeeAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OutstandingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "OUTSTANDING"),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_FinNonPatientReceivable", x => x.Id);
                    table.CheckConstraint("CK_FinNonPatientReceivable_Category", "\"Category\" IN ('PARKING','TENANT')");
                    table.CheckConstraint("CK_FinNonPatientReceivable_Status", "\"Status\" IN ('OUTSTANDING','PARTIALLY_SETTLED','SETTLED','WRITTEN_OFF','CANCELLED')");
                });

            migrationBuilder.CreateTable(
                name: "FinNonPatientReceivableSettlement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NonPatientReceivableId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
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
                    table.PrimaryKey("PK_FinNonPatientReceivableSettlement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinNonPatientReceivableSettlement_FinNonPatientReceivable_NonPatientReceivableId",
                        column: x => x.NonPatientReceivableId,
                        principalSchema: "public",
                        principalTable: "FinNonPatientReceivable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinNonPatientReceivable_ReceivableNumber",
                schema: "public",
                table: "FinNonPatientReceivable",
                column: "ReceivableNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinNonPatientReceivable_Category",
                schema: "public",
                table: "FinNonPatientReceivable",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_FinNonPatientReceivable_Status",
                schema: "public",
                table: "FinNonPatientReceivable",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FinNonPatientReceivable_DueDate",
                schema: "public",
                table: "FinNonPatientReceivable",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinNonPatientReceivableSettlement_NonPatientReceivableId",
                schema: "public",
                table: "FinNonPatientReceivableSettlement",
                column: "NonPatientReceivableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Urutan kebalikan: anak (FK) lebih dulu, baru induk.
            migrationBuilder.DropTable(
                name: "FinNonPatientReceivableSettlement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinNonPatientReceivable",
                schema: "public");
        }
    }
}
