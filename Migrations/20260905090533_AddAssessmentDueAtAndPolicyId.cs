using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAssessmentDueAtAndPolicyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueAt",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PolicyId",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueAt",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "PolicyId",
                schema: "public",
                table: "TrxPatientAssessment");
        }
    }
}
