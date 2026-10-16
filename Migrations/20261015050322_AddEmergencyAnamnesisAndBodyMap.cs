using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyAnamnesisAndBodyMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BodyMapData",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOralIntakeDateTime",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastOralIntakeNote",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastOralIntakeType",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraumaMechanism",
                schema: "public",
                table: "EmgVisit",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TraumaSafetyEquipment",
                schema: "public",
                table: "EmgVisit",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyMapData",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "LastOralIntakeDateTime",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "LastOralIntakeNote",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "LastOralIntakeType",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "TraumaMechanism",
                schema: "public",
                table: "EmgVisit");

            migrationBuilder.DropColumn(
                name: "TraumaSafetyEquipment",
                schema: "public",
                table: "EmgVisit");
        }
    }
}
