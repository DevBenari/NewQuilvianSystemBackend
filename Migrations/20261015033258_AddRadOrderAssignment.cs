using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRadOrderAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExaminingDoctorId",
                schema: "public",
                table: "RadOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RadiographerUserId",
                schema: "public",
                table: "RadOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_ExaminingDoctorId",
                schema: "public",
                table: "RadOrder",
                column: "ExaminingDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_RadiographerUserId",
                schema: "public",
                table: "RadOrder",
                column: "RadiographerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RadOrder_AspNetUsers_RadiographerUserId",
                schema: "public",
                table: "RadOrder",
                column: "RadiographerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RadOrder_MstDoctor_ExaminingDoctorId",
                schema: "public",
                table: "RadOrder",
                column: "ExaminingDoctorId",
                principalSchema: "public",
                principalTable: "MstDoctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RadOrder_AspNetUsers_RadiographerUserId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RadOrder_MstDoctor_ExaminingDoctorId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropIndex(
                name: "IX_RadOrder_ExaminingDoctorId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropIndex(
                name: "IX_RadOrder_RadiographerUserId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "ExaminingDoctorId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "RadiographerUserId",
                schema: "public",
                table: "RadOrder");
        }
    }
}
