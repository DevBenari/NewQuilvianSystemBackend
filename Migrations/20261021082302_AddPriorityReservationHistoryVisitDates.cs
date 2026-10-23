using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPriorityReservationHistoryVisitDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FromVisitDate",
                schema: "public",
                table: "RegPriorityReservationHistory",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ToVisitDate",
                schema: "public",
                table: "RegPriorityReservationHistory",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromVisitDate",
                schema: "public",
                table: "RegPriorityReservationHistory");

            migrationBuilder.DropColumn(
                name: "ToVisitDate",
                schema: "public",
                table: "RegPriorityReservationHistory");
        }
    }
}
