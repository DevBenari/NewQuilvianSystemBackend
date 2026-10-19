using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// BE-RWI-240 / M-DNH-4 (RWI-DEC-315): penanda jenis kelamin dicabut dari kamar dan bed. Dirilis
    /// bersama FE-RWI-253 (DNH-MIS-08). Mundur menambah kembali kolom dengan bawaan true; isi lama
    /// tidak dipulihkan karena maknanya sudah dicabut.
    /// </remarks>
    public partial class DropGenderFlagsFromRoomAndBed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsForFemale",
                schema: "public",
                table: "MstRoom");

            migrationBuilder.DropColumn(
                name: "IsForMale",
                schema: "public",
                table: "MstRoom");

            migrationBuilder.DropColumn(
                name: "IsForFemale",
                schema: "public",
                table: "MstBed");

            migrationBuilder.DropColumn(
                name: "IsForMale",
                schema: "public",
                table: "MstBed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsForFemale",
                schema: "public",
                table: "MstRoom",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForMale",
                schema: "public",
                table: "MstRoom",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForFemale",
                schema: "public",
                table: "MstBed",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForMale",
                schema: "public",
                table: "MstBed",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }
    }
}
