using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddOnBehalfFieldsToAttendanceCorrectionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InitiatedByUserId",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnBehalf",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OnBehalfReason",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnBehalfNotifiedAt",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendanceCorrectionRequest_InitiatedByUserId",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest",
                column: "InitiatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_HrdAttendanceCorrectionRequest_AspNetUsers_InitiatedByUserId",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest",
                column: "InitiatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_HrdAttendanceCorrectionRequest_IsOnBehalf_RequestStatus",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest",
                columns: new[] { "IsOnBehalf", "RequestStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HrdAttendanceCorrectionRequest_AspNetUsers_InitiatedByUserId",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest");

            migrationBuilder.DropIndex(
                name: "IX_HrdAttendanceCorrectionRequest_InitiatedByUserId",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest");

            migrationBuilder.DropIndex(
                name: "IX_HrdAttendanceCorrectionRequest_IsOnBehalf_RequestStatus",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest");

            migrationBuilder.DropColumn(
                name: "InitiatedByUserId",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest");

            migrationBuilder.DropColumn(
                name: "IsOnBehalf",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest");

            migrationBuilder.DropColumn(
                name: "OnBehalfReason",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest");

            migrationBuilder.DropColumn(
                name: "OnBehalfNotifiedAt",
                schema: "public",
                table: "HrdAttendanceCorrectionRequest");
        }
    }
}
