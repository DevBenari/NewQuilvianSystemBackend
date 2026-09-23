using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyArrivalTimeSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArrivalConfirmedAt",
                schema: "public",
                table: "EmgVisit",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArrivalConfirmedByUserId",
                schema: "public",
                table: "EmgVisit",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ArrivalTimeSource",
                schema: "public",
                table: "EmgVisit",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EmgVisit_ArrivalConfirmedByUserId",
                schema: "public",
                table: "EmgVisit",
                column: "ArrivalConfirmedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmgVisit_AspNetUsers_ArrivalConfirmedByUserId",
                schema: "public",
                table: "EmgVisit",
                column: "ArrivalConfirmedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmgVisit_AspNetUsers_ArrivalConfirmedByUserId",
                schema: "public",
                table: "EmgVisit");

            migrationBuilder.DropIndex(
                name: "IX_EmgVisit_ArrivalConfirmedByUserId",
                schema: "public",
                table: "EmgVisit");

            migrationBuilder.DropColumn(
                name: "ArrivalConfirmedAt",
                schema: "public",
                table: "EmgVisit");

            migrationBuilder.DropColumn(
                name: "ArrivalConfirmedByUserId",
                schema: "public",
                table: "EmgVisit");

            migrationBuilder.DropColumn(
                name: "ArrivalTimeSource",
                schema: "public",
                table: "EmgVisit");
        }
    }
}
