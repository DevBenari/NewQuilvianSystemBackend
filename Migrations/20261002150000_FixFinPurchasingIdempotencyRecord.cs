using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixFinPurchasingIdempotencyRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CancelBy",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelDateTime",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleteBy",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDateTime",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCancel",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdateBy",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateDateTime",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelBy",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord");

            migrationBuilder.DropColumn(
                name: "CancelDateTime",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord");

            migrationBuilder.DropColumn(
                name: "DeleteBy",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord");

            migrationBuilder.DropColumn(
                name: "DeleteDateTime",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord");

            migrationBuilder.DropColumn(
                name: "IsCancel",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord");

            migrationBuilder.DropColumn(
                name: "UpdateBy",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord");

            migrationBuilder.DropColumn(
                name: "UpdateDateTime",
                schema: "public",
                table: "FinPurchasingIdempotencyRecord");
        }
    }
}
