using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCoveringMcuOrderToLabRadOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CoveringMcuOrderId",
                schema: "public",
                table: "RadOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CoveringMcuOrderId",
                schema: "public",
                table: "LabOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_CoveringMcuOrderId",
                schema: "public",
                table: "RadOrder",
                column: "CoveringMcuOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_CoveringMcuOrderId",
                schema: "public",
                table: "LabOrder",
                column: "CoveringMcuOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrder_McuOrder_CoveringMcuOrderId",
                schema: "public",
                table: "LabOrder",
                column: "CoveringMcuOrderId",
                principalSchema: "public",
                principalTable: "McuOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RadOrder_McuOrder_CoveringMcuOrderId",
                schema: "public",
                table: "RadOrder",
                column: "CoveringMcuOrderId",
                principalSchema: "public",
                principalTable: "McuOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabOrder_McuOrder_CoveringMcuOrderId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RadOrder_McuOrder_CoveringMcuOrderId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropIndex(
                name: "IX_RadOrder_CoveringMcuOrderId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_CoveringMcuOrderId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "CoveringMcuOrderId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "CoveringMcuOrderId",
                schema: "public",
                table: "LabOrder");
        }
    }
}
