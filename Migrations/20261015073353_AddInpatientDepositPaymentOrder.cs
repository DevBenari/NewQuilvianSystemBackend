using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddInpatientDepositPaymentOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BilDepositPaymentOrder",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PolicyId = table.Column<Guid>(type: "uuid", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_BilDepositPaymentOrder", x => x.Id);
                    table.CheckConstraint("CK_BilDepositPaymentOrder_OutstandingAmount", "\"OutstandingAmount\" >= 0");
                    table.CheckConstraint("CK_BilDepositPaymentOrder_PaidAmount", "\"PaidAmount\" >= 0");
                    table.CheckConstraint("CK_BilDepositPaymentOrder_RequestedAmount", "\"RequestedAmount\" > 0");
                    table.CheckConstraint("CK_BilDepositPaymentOrder_Status", "\"Status\" IN ('UNPAID','PARTIALLY_PAID','PAID','CANCELLED','EXPIRED')");
                });

            migrationBuilder.CreateTable(
                name: "BilDepositOrderPayment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepositOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    DepositMovementId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ReversalReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_BilDepositOrderPayment", x => x.Id);
                    table.CheckConstraint("CK_BilDepositOrderPayment_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilDepositOrderPayment_Status", "\"Status\" IN ('SUCCESS','REVERSED')");
                    table.ForeignKey(
                        name: "FK_BilDepositOrderPayment_BilDepositMovement_DepositMovementId",
                        column: x => x.DepositMovementId,
                        principalSchema: "public",
                        principalTable: "BilDepositMovement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilDepositOrderPayment_BilDepositPaymentOrder_DepositOrderId",
                        column: x => x.DepositOrderId,
                        principalSchema: "public",
                        principalTable: "BilDepositPaymentOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositOrderPayment_DepositMovementId",
                schema: "public",
                table: "BilDepositOrderPayment",
                column: "DepositMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositOrderPayment_DepositOrderId",
                schema: "public",
                table: "BilDepositOrderPayment",
                column: "DepositOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositPaymentOrder_CorrelationId",
                schema: "public",
                table: "BilDepositPaymentOrder",
                column: "CorrelationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositPaymentOrder_EncounterId",
                schema: "public",
                table: "BilDepositPaymentOrder",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositPaymentOrder_EpisodeId",
                schema: "public",
                table: "BilDepositPaymentOrder",
                column: "EpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositPaymentOrder_EpisodeId_OrderType",
                schema: "public",
                table: "BilDepositPaymentOrder",
                columns: new[] { "EpisodeId", "OrderType" });

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositPaymentOrder_IdempotencyKey",
                schema: "public",
                table: "BilDepositPaymentOrder",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositPaymentOrder_OrderNumber",
                schema: "public",
                table: "BilDepositPaymentOrder",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositPaymentOrder_PatientId",
                schema: "public",
                table: "BilDepositPaymentOrder",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_BilDepositPaymentOrder_Status",
                schema: "public",
                table: "BilDepositPaymentOrder",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilDepositOrderPayment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilDepositPaymentOrder",
                schema: "public");
        }
    }
}
