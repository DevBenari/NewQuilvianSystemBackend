using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menambah FinReceivableInstallmentDeductionEvidence: penghubung bukti potong gaji (FinTransactionProof jenis
    /// LAPORAN-PENGGAJIAN) ke satu baris jadwal angsuran. Tabel baru yang aditif; tidak mengubah, memindahkan, atau
    /// mengisi data pada tabel mana pun, dan tidak menyentuh saldo piutang, angsuran, maupun buku mutasi.
    /// Unique index terfilter UX_..._Active mencegah dua bukti aktif dengan nomor laporan yang sama pada angsuran yang sama;
    /// bukti DITOLAK dilepas dari batasan itu. Nama constraint/index sengaja pendek (batas identifier PostgreSQL 63 karakter).
    /// Down hanya men-drop tabel ini, sehingga berkas bukti di FinTransactionProof tidak terhapus.
    /// </summary>
    /// <inheritdoc />
    public partial class AddInstallmentDeductionEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinReceivableInstallmentDeductionEvidence",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstallmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivableId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeductionPeriod = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    PayrollPeriodId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayrollRunId = table.Column<Guid>(type: "uuid", nullable: true),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProofId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultStatus = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DeductedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeductionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "BUKTI_DIUNGGAH"),
                    UploadedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VerifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerificationNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FinReceivableInstallmentDeductionEvidence", x => x.Id);
                    table.CheckConstraint("CK_FinReceivableInstallmentDeductionEvidence_Amount", "\"DeductedAmount\" >= 0");
                    table.CheckConstraint("CK_FinReceivableInstallmentDeductionEvidence_ResultStatus", "\"ResultStatus\" IN ('BERHASIL','SEBAGIAN','GAGAL')");
                    table.CheckConstraint("CK_FinReceivableInstallmentDeductionEvidence_Status", "\"Status\" IN ('BUKTI_DIUNGGAH','TERVERIFIKASI','DITOLAK')");
                    table.ForeignKey(
                        name: "FK_FinReceivableInstallmentDeductionEvidence_Installment",
                        column: x => x.InstallmentId,
                        principalSchema: "public",
                        principalTable: "FinReceivableInstallment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinReceivableInstallmentDeductionEvidence_Proof",
                        column: x => x.ProofId,
                        principalSchema: "public",
                        principalTable: "FinTransactionProof",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_FinReceivableInstallmentDeductionEvidence_Active",
                schema: "public",
                table: "FinReceivableInstallmentDeductionEvidence",
                columns: new[] { "InstallmentId", "DocumentNumber" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"Status\" <> 'DITOLAK'");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInstallmentDeductionEvidence_PlanId",
                schema: "public",
                table: "FinReceivableInstallmentDeductionEvidence",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInstallmentDeductionEvidence_ProofId",
                schema: "public",
                table: "FinReceivableInstallmentDeductionEvidence",
                column: "ProofId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInstallmentDeductionEvidence_ReceivableId",
                schema: "public",
                table: "FinReceivableInstallmentDeductionEvidence",
                column: "ReceivableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinReceivableInstallmentDeductionEvidence",
                schema: "public");
        }
    }
}
