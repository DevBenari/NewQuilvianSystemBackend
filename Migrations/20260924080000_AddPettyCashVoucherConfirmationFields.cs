using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPettyCashVoucherConfirmationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConfirmedByUserId",
                schema: "public",
                table: "BilPettyCashVoucher",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConfirmedByName",
                schema: "public",
                table: "BilPettyCashVoucher",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ConfirmedAt",
                schema: "public",
                table: "BilPettyCashVoucher",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                schema: "public",
                table: "BilPettyCashVoucher");

            migrationBuilder.DropColumn(
                name: "ConfirmedByName",
                schema: "public",
                table: "BilPettyCashVoucher");

            migrationBuilder.DropColumn(
                name: "ConfirmedByUserId",
                schema: "public",
                table: "BilPettyCashVoucher");
        }
    }
}
