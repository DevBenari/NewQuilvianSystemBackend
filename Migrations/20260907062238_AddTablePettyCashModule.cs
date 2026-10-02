using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTablePettyCashModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BilPettyCashBudget",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PoolCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PoolName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TotalTopUpAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TotalDisbursedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "ACTIVE"),
                    LastMovementAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_BilPettyCashBudget", x => x.Id);
                    table.CheckConstraint("CK_BilPettyCashBudget_CurrentBalance", "\"CurrentBalance\" >= 0");
                    table.CheckConstraint("CK_BilPettyCashBudget_Status", "\"Status\" IN ('ACTIVE','INACTIVE')");
                });

            migrationBuilder.CreateTable(
                name: "MstPettyCashCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_MstPettyCashCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BilPettyCashVoucher",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoucherNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RecipientName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "WAITING_APPROVAL"),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DecidedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisbursedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DisbursedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProofReferenceNumber = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ProofSubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ProofSubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_BilPettyCashVoucher", x => x.Id);
                    table.CheckConstraint("CK_BilPettyCashVoucher_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilPettyCashVoucher_RejectionReason", "(\"Status\" = 'REJECTED' AND \"RejectionReason\" IS NOT NULL) OR (\"Status\" <> 'REJECTED' AND \"RejectionReason\" IS NULL)");
                    table.CheckConstraint("CK_BilPettyCashVoucher_Status", "\"Status\" IN ('WAITING_APPROVAL','APPROVED','CASH_RECEIVED','COMPLETED','REJECTED')");
                    table.ForeignKey(
                        name: "FK_BilPettyCashVoucher_MstPettyCashCategory_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "public",
                        principalTable: "MstPettyCashCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilPettyCashBudgetMovement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BudgetId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    VoucherId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_BilPettyCashBudgetMovement", x => x.Id);
                    table.CheckConstraint("CK_BilPettyCashBudgetMovement_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilPettyCashBudgetMovement_BalanceAfter", "\"BalanceAfter\" >= 0");
                    table.CheckConstraint("CK_BilPettyCashBudgetMovement_MovementType", "\"MovementType\" IN ('TOP_UP','DISBURSEMENT','ADJUSTMENT')");
                    table.CheckConstraint("CK_BilPettyCashBudgetMovement_Reason", "\"MovementType\" NOT IN ('TOP_UP','ADJUSTMENT') OR \"Reason\" IS NOT NULL");
                    table.CheckConstraint("CK_BilPettyCashBudgetMovement_VoucherId", "(\"MovementType\" = 'DISBURSEMENT' AND \"VoucherId\" IS NOT NULL) OR (\"MovementType\" <> 'DISBURSEMENT' AND \"VoucherId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_BilPettyCashBudgetMovement_BilPettyCashBudget_BudgetId",
                        column: x => x.BudgetId,
                        principalSchema: "public",
                        principalTable: "BilPettyCashBudget",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilPettyCashBudgetMovement_BilPettyCashVoucher_VoucherId",
                        column: x => x.VoucherId,
                        principalSchema: "public",
                        principalTable: "BilPettyCashVoucher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilPettyCashVoucherCommand",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VoucherId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommandType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorRole = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    EntityVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusBefore = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    StatusAfter = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: true),
                    PayloadHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResponseJson = table.Column<string>(type: "text", nullable: false, defaultValue: "{}"),
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
                    table.PrimaryKey("PK_BilPettyCashVoucherCommand", x => x.Id);
                    table.CheckConstraint("CK_BilPettyCashVoucherCommand_CommandType", "\"CommandType\" IN ('SUBMIT','APPROVE','REJECT','CANCEL','DISBURSE','ATTACH_PROOF','PROOF_CORRECTED')");
                    table.CheckConstraint("CK_BilPettyCashVoucherCommand_Reason", "\"CommandType\" NOT IN ('REJECT','CANCEL') OR \"Reason\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_BilPettyCashVoucherCommand_BilPettyCashVoucher_VoucherId",
                        column: x => x.VoucherId,
                        principalSchema: "public",
                        principalTable: "BilPettyCashVoucher",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "BilPettyCashBudget",
                columns: new[] { "Id", "CancelBy", "CancelDateTime", "CreateBy", "CreateDateTime", "DeleteBy", "DeleteDateTime", "LastMovementAt", "PoolCode", "PoolName", "RowVersion", "Status", "UpdateBy", "UpdateDateTime" },
                values: new object[] { new Guid("b7c1f5a2-9d34-4e88-9a10-000000000001"), new Guid("00000000-0000-0000-0000-000000000000"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "HOSPITAL_MAIN", "Kas Kecil Rumah Sakit", new Guid("b7c1f5a2-9d34-4e88-9a10-0000000000f1"), "ACTIVE", new Guid("00000000-0000-0000-0000-000000000000"), null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "MstPettyCashCategory",
                columns: new[] { "Id", "CancelBy", "CancelDateTime", "CategoryCode", "CategoryName", "CreateBy", "CreateDateTime", "DeleteBy", "DeleteDateTime", "Description", "IsActive", "UpdateBy", "UpdateDateTime" },
                values: new object[,]
                {
                    { new Guid("c8d2e6b3-4a15-4f79-8b21-000000000001"), new Guid("00000000-0000-0000-0000-000000000000"), null, "TRANSPORT", "Transport", new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, "Ongkos transport kurir dan perjalanan dinas singkat", true, new Guid("00000000-0000-0000-0000-000000000000"), null },
                    { new Guid("c8d2e6b3-4a15-4f79-8b21-000000000002"), new Guid("00000000-0000-0000-0000-000000000000"), null, "OPERASIONAL", "Operasional", new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, "Keperluan operasional harian rumah sakit", true, new Guid("00000000-0000-0000-0000-000000000000"), null },
                    { new Guid("c8d2e6b3-4a15-4f79-8b21-000000000003"), new Guid("00000000-0000-0000-0000-000000000000"), null, "KONSUMSI", "Konsumsi", new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, "Konsumsi rapat dan kegiatan internal", true, new Guid("00000000-0000-0000-0000-000000000000"), null },
                    { new Guid("c8d2e6b3-4a15-4f79-8b21-000000000004"), new Guid("00000000-0000-0000-0000-000000000000"), null, "MAINTENANCE", "Maintenance", new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, "Perbaikan kecil sarana dan prasarana", true, new Guid("00000000-0000-0000-0000-000000000000"), null },
                    { new Guid("c8d2e6b3-4a15-4f79-8b21-000000000005"), new Guid("00000000-0000-0000-0000-000000000000"), null, "ATK", "Alat Tulis Kantor", new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 9, 7, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, "Pembelian alat tulis kantor", true, new Guid("00000000-0000-0000-0000-000000000000"), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudget_ActiveSingleton",
                schema: "public",
                table: "BilPettyCashBudget",
                column: "Status",
                unique: true,
                filter: "\"Status\" = 'ACTIVE' AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudget_PoolCode",
                schema: "public",
                table: "BilPettyCashBudget",
                column: "PoolCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudgetMovement_Budget_OccurredAt",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                columns: new[] { "BudgetId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudgetMovement_IdempotencyKey",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashBudgetMovement_Voucher_Disbursement",
                schema: "public",
                table: "BilPettyCashBudgetMovement",
                column: "VoucherId",
                unique: true,
                filter: "\"MovementType\" = 'DISBURSEMENT' AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashVoucher_CategoryId",
                schema: "public",
                table: "BilPettyCashVoucher",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashVoucher_IdempotencyKey",
                schema: "public",
                table: "BilPettyCashVoucher",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashVoucher_RecipientName",
                schema: "public",
                table: "BilPettyCashVoucher",
                column: "RecipientName");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashVoucher_Status_SubmittedAt",
                schema: "public",
                table: "BilPettyCashVoucher",
                columns: new[] { "Status", "SubmittedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashVoucher_VoucherNumber",
                schema: "public",
                table: "BilPettyCashVoucher",
                column: "VoucherNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashVoucherCommand_IdempotencyKey",
                schema: "public",
                table: "BilPettyCashVoucherCommand",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BilPettyCashVoucherCommand_VoucherId_OccurredAt",
                schema: "public",
                table: "BilPettyCashVoucherCommand",
                columns: new[] { "VoucherId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPettyCashCategory_CategoryCode",
                schema: "public",
                table: "MstPettyCashCategory",
                column: "CategoryCode",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilPettyCashBudgetMovement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilPettyCashVoucherCommand",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilPettyCashBudget",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilPettyCashVoucher",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPettyCashCategory",
                schema: "public");
        }
    }
}
