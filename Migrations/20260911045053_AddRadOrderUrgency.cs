using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRadOrderUrgency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUrgent",
                schema: "public",
                table: "RadOrder",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UrgentMarkedAt",
                schema: "public",
                table: "RadOrder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UrgentMarkedByUserId",
                schema: "public",
                table: "RadOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_ModalityId_IsUrgent_OrderStatus",
                schema: "public",
                table: "RadOrder",
                columns: new[] { "ModalityId", "IsUrgent", "OrderStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RadOrder_ModalityId_IsUrgent_OrderStatus",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "IsUrgent",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "UrgentMarkedAt",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "UrgentMarkedByUserId",
                schema: "public",
                table: "RadOrder");
        }
    }
}
