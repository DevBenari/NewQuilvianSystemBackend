using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientCitizenship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Citizenship",
                schema: "public",
                table: "MstPatient",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NationalityCountryId",
                schema: "public",
                table: "MstPatient",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstPatient_NationalityCountryId",
                schema: "public",
                table: "MstPatient",
                column: "NationalityCountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MstPatient_MstCountry_NationalityCountryId",
                schema: "public",
                table: "MstPatient",
                column: "NationalityCountryId",
                principalSchema: "public",
                principalTable: "MstCountry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstPatient_MstCountry_NationalityCountryId",
                schema: "public",
                table: "MstPatient");

            migrationBuilder.DropIndex(
                name: "IX_MstPatient_NationalityCountryId",
                schema: "public",
                table: "MstPatient");

            migrationBuilder.DropColumn(
                name: "Citizenship",
                schema: "public",
                table: "MstPatient");

            migrationBuilder.DropColumn(
                name: "NationalityCountryId",
                schema: "public",
                table: "MstPatient");
        }
    }
}
