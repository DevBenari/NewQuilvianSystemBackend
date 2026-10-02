using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Nama migration mengikuti roadmap (BE-FIN-007), tetapi cakupannya hanya 5 tabel piutang.
    /// 2 tabel penerimaan (FinReceipt, FinReceiptAllocation) yang semula direncanakan ikut migration
    /// ini BELUM dibuat — belum ada satu pun task roadmap yang secara eksplisit memiliki entity
    /// Collection tersebut (temuan yang dilaporkan pada laporan task BE-FIN-007). Migration ini
    /// disesuaikan dengan ApplicationDbContext yang sebenarnya, bukan dengan rencana 7 tabel.
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinanceReceivableAndCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinReceivable",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivableNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceHandoffKey = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceHandoffId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DebtorType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DebtorReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    BenefitOwnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    BenefitRelationship = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    AdjustedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    WrittenOffAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "OUTSTANDING"),
                    ClaimStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "NOT_REQUIRED"),
                    RecognizedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FinReceivable", x => x.Id);
                    table.CheckConstraint("CK_FinReceivable_DebtorType", "\"DebtorType\" IN ('PAYER','PATIENT_GUARANTOR','EMPLOYEE_BENEFIT')");
                    table.CheckConstraint("CK_FinReceivable_Status", "\"Status\" IN ('OUTSTANDING','PARTIAL','SETTLED','WRITTEN_OFF','CANCELLED')");
                    table.CheckConstraint("CK_FinReceivable_Outstanding", "\"OutstandingAmount\" >= 0");
                    table.CheckConstraint("CK_FinReceivable_Balance", "\"OriginalAmount\" = \"OutstandingAmount\" + \"AllocatedAmount\" + \"AdjustedAmount\" + \"WrittenOffAmount\"");
                    table.CheckConstraint("CK_FinReceivable_BenefitOwner", "(\"DebtorType\" = 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NOT NULL) OR (\"DebtorType\" <> 'EMPLOYEE_BENEFIT' AND \"BenefitOwnerId\" IS NULL)");
                });

            migrationBuilder.CreateTable(
                name: "FinReceivableAdjustment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdjustmentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReceivableId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceHandoffAdjustmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "REQUESTED"),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FinReceivableAdjustment", x => x.Id);
                    table.CheckConstraint("CK_FinReceivableAdjustment_Direction", "\"Direction\" IN ('DEBIT','CREDIT')");
                    table.CheckConstraint("CK_FinReceivableAdjustment_Status", "\"Status\" IN ('REQUESTED','APPROVED','REJECTED')");
                    table.CheckConstraint("CK_FinReceivableAdjustment_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_FinReceivableAdjustment_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
                    table.ForeignKey(
                        name: "FK_FinReceivableAdjustment_FinReceivable_ReceivableId",
                        column: x => x.ReceivableId,
                        principalSchema: "public",
                        principalTable: "FinReceivable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinReceivableDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivableId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsReceived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FinReceivableDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinReceivableDocument_FinReceivable_ReceivableId",
                        column: x => x.ReceivableId,
                        principalSchema: "public",
                        principalTable: "FinReceivable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinReceivableItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivableId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_FinReceivableItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinReceivableItem_FinReceivable_ReceivableId",
                        column: x => x.ReceivableId,
                        principalSchema: "public",
                        principalTable: "FinReceivable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinReceivableWriteOff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WriteOffNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReceivableId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "REQUESTED"),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FinReceivableWriteOff", x => x.Id);
                    table.CheckConstraint("CK_FinReceivableWriteOff_Status", "\"Status\" IN ('REQUESTED','APPROVED','REJECTED')");
                    table.CheckConstraint("CK_FinReceivableWriteOff_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_FinReceivableWriteOff_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
                    table.ForeignKey(
                        name: "FK_FinReceivableWriteOff_FinReceivable_ReceivableId",
                        column: x => x.ReceivableId,
                        principalSchema: "public",
                        principalTable: "FinReceivable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivable_ReceivableNumber",
                schema: "public",
                table: "FinReceivable",
                column: "ReceivableNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivable_SourceHandoffKey",
                schema: "public",
                table: "FinReceivable",
                column: "SourceHandoffKey",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivable_Status_DueDate",
                schema: "public",
                table: "FinReceivable",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableAdjustment_ReceivableId",
                schema: "public",
                table: "FinReceivableAdjustment",
                column: "ReceivableId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableAdjustment_Number",
                schema: "public",
                table: "FinReceivableAdjustment",
                column: "AdjustmentNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableDocument_ReceivableId",
                schema: "public",
                table: "FinReceivableDocument",
                column: "ReceivableId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableItem_ReceivableId",
                schema: "public",
                table: "FinReceivableItem",
                column: "ReceivableId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableWriteOff_ReceivableId",
                schema: "public",
                table: "FinReceivableWriteOff",
                column: "ReceivableId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableWriteOff_Number",
                schema: "public",
                table: "FinReceivableWriteOff",
                column: "WriteOffNumber",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinReceivableAdjustment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinReceivableDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinReceivableItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinReceivableWriteOff",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinReceivable",
                schema: "public");
        }
    }
}
