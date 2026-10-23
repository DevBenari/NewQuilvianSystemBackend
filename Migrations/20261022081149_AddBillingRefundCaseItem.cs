using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingRefundCaseItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BilRefundCaseItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RefundCaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceItemId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_BilRefundCaseItem", x => x.Id);
                    table.CheckConstraint("CK_BilRefundCaseItem_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_BilRefundCaseItem_BilInvoiceItem_InvoiceItemId",
                        column: x => x.InvoiceItemId,
                        principalSchema: "public",
                        principalTable: "BilInvoiceItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilRefundCaseItem_BilRefundCase_RefundCaseId",
                        column: x => x.RefundCaseId,
                        principalSchema: "public",
                        principalTable: "BilRefundCase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilRefundCaseItem_InvoiceItemId",
                schema: "public",
                table: "BilRefundCaseItem",
                column: "InvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BilRefundCaseItem_RefundCaseId_InvoiceItemId",
                schema: "public",
                table: "BilRefundCaseItem",
                columns: new[] { "RefundCaseId", "InvoiceItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilRefundCaseItem",
                schema: "public");
        }
    }
}
