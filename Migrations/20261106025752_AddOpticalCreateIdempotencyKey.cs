using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddOpticalCreateIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdempotencyKey",
                schema: "public",
                table: "OptWarrantyClaim",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdempotencyKey",
                schema: "public",
                table: "OptPrescription",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdempotencyKey",
                schema: "public",
                table: "OptOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_OptWarrantyClaim_IdempotencyKey",
                schema: "public",
                table: "OptWarrantyClaim",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_OptPrescription_IdempotencyKey",
                schema: "public",
                table: "OptPrescription",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_OptOrder_IdempotencyKey",
                schema: "public",
                table: "OptOrder",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_OptWarrantyClaim_IdempotencyKey",
                schema: "public",
                table: "OptWarrantyClaim");

            migrationBuilder.DropIndex(
                name: "UX_OptPrescription_IdempotencyKey",
                schema: "public",
                table: "OptPrescription");

            migrationBuilder.DropIndex(
                name: "UX_OptOrder_IdempotencyKey",
                schema: "public",
                table: "OptOrder");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "public",
                table: "OptWarrantyClaim");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "public",
                table: "OptPrescription");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "public",
                table: "OptOrder");
        }
    }
}
