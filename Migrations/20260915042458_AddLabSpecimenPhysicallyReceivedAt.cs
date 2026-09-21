using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabSpecimenPhysicallyReceivedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PhysicallyReceivedAt",
                schema: "public",
                table: "LabSpecimen",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimen_PhysicallyReceivedAt",
                schema: "public",
                table: "LabSpecimen",
                column: "PhysicallyReceivedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LabSpecimen_PhysicallyReceivedAt",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropColumn(
                name: "PhysicallyReceivedAt",
                schema: "public",
                table: "LabSpecimen");
        }
    }
}
