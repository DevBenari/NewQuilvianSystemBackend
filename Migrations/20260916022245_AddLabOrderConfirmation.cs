using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabOrderConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAt",
                schema: "public",
                table: "LabOrder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfirmedByUserId",
                schema: "public",
                table: "LabOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExaminerDoctorId",
                schema: "public",
                table: "LabOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_ExaminerDoctorId",
                schema: "public",
                table: "LabOrder",
                column: "ExaminerDoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrder_MstDoctor_ExaminerDoctorId",
                schema: "public",
                table: "LabOrder",
                column: "ExaminerDoctorId",
                principalSchema: "public",
                principalTable: "MstDoctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LabOrder_MstDoctor_ExaminerDoctorId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_ExaminerDoctorId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "ConfirmedAt",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "ConfirmedByUserId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "ExaminerDoctorId",
                schema: "public",
                table: "LabOrder");
        }
    }
}
