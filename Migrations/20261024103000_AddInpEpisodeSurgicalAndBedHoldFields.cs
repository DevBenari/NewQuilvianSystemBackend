using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-182 / BE-RWI-183 (RWI-DEC-355..358, OPS-DEC-040) — Lima kolom integrasi bedah dan admisi rawat inap
    /// pada tabel InpEpisode (SurgicalBookingNumber, ClinicalSubCategory, BedHoldStatus, IsRoomingInBabyPlanned, MaternityPackageId).
    /// Seluruh operasi murni aditif.
    /// </summary>
    public partial class AddInpEpisodeSurgicalAndBedHoldFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SurgicalBookingNumber",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClinicalSubCategory",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BedHoldStatus",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRoomingInBabyPlanned",
                schema: "public",
                table: "InpEpisode",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MaternityPackageId",
                schema: "public",
                table: "InpEpisode",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SurgicalBookingNumber",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "ClinicalSubCategory",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "BedHoldStatus",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "IsRoomingInBabyPlanned",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "MaternityPackageId",
                schema: "public",
                table: "InpEpisode");
        }
    }
}
