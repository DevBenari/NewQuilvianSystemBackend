using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class TableFinPurchasingIdempotencyRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinPurchasingIdempotencyRecord",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "integer", nullable: false),
                    ResponseBody = table.Column<string>(type: "text", nullable: false),
                    CreateDateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinPurchasingIdempotencyRecord", x => x.Id);
                    table.CheckConstraint("CK_FinPurchasingIdempotencyRecord_Action", "\"Action\" IN ('Create','Submit','Approve','Cancel','Confirm')");
                    table.CheckConstraint("CK_FinPurchasingIdempotencyRecord_EntityType", "\"EntityType\" IN ('PurchaseOrder','GoodsReceipt','InvoiceExchange','PurchasingInvoice','SupplierReturn')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingIdempotencyRecord_EntityTypeEntityId",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_FinPurchasingIdempotencyRecord_IdempotencyKey",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinPurchasingIdempotencyRecord",
                schema: "public");
        }
    }
}
