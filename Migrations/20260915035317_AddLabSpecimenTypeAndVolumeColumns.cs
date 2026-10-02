using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabSpecimenTypeAndVolumeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SpecimenTypeId",
                schema: "public",
                table: "LabSpecimen",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecimenTypeOtherNote",
                schema: "public",
                table: "LabSpecimen",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VolumeAmount",
                schema: "public",
                table: "LabSpecimen",
                type: "numeric(12,3)",
                precision: 12,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VolumeUnitId",
                schema: "public",
                table: "LabSpecimen",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimen_SpecimenTypeId",
                schema: "public",
                table: "LabSpecimen",
                column: "SpecimenTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimen_VolumeUnitId",
                schema: "public",
                table: "LabSpecimen",
                column: "VolumeUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabSpecimen_LabSpecimenType_SpecimenTypeId",
                schema: "public",
                table: "LabSpecimen",
                column: "SpecimenTypeId",
                principalSchema: "public",
                principalTable: "LabSpecimenType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabSpecimen_MstMeasurement_VolumeUnitId",
                schema: "public",
                table: "LabSpecimen",
                column: "VolumeUnitId",
                principalSchema: "public",
                principalTable: "MstMeasurement",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabSpecimen_LabSpecimenType_SpecimenTypeId",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropForeignKey(
                name: "FK_LabSpecimen_MstMeasurement_VolumeUnitId",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropIndex(
                name: "IX_LabSpecimen_SpecimenTypeId",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropIndex(
                name: "IX_LabSpecimen_VolumeUnitId",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropColumn(
                name: "SpecimenTypeId",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropColumn(
                name: "SpecimenTypeOtherNote",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropColumn(
                name: "VolumeAmount",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropColumn(
                name: "VolumeUnitId",
                schema: "public",
                table: "LabSpecimen");
        }
    }
}
