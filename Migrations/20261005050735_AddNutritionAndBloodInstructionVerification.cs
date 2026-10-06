using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionAndBloodInstructionVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "BbkBloodOrder",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "BbkBloodOrder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "BbkBloodOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "GziNutritionOrder",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "GziNutritionOrder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "GziNutritionOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodOrder_InstructionVerificationStatus",
                schema: "public",
                table: "BbkBloodOrder",
                column: "InstructionVerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BbkBloodOrder_InstructionVerifiedByUserId",
                schema: "public",
                table: "BbkBloodOrder",
                column: "InstructionVerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionOrder_InstructionVerificationStatus",
                schema: "public",
                table: "GziNutritionOrder",
                column: "InstructionVerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_GziNutritionOrder_InstructionVerifiedByUserId",
                schema: "public",
                table: "GziNutritionOrder",
                column: "InstructionVerifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BbkBloodOrder_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "BbkBloodOrder",
                column: "InstructionVerifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GziNutritionOrder_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "GziNutritionOrder",
                column: "InstructionVerifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BbkBloodOrder_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "BbkBloodOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_GziNutritionOrder_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "GziNutritionOrder");

            migrationBuilder.DropIndex(
                name: "IX_BbkBloodOrder_InstructionVerificationStatus",
                schema: "public",
                table: "BbkBloodOrder");

            migrationBuilder.DropIndex(
                name: "IX_BbkBloodOrder_InstructionVerifiedByUserId",
                schema: "public",
                table: "BbkBloodOrder");

            migrationBuilder.DropIndex(
                name: "IX_GziNutritionOrder_InstructionVerificationStatus",
                schema: "public",
                table: "GziNutritionOrder");

            migrationBuilder.DropIndex(
                name: "IX_GziNutritionOrder_InstructionVerifiedByUserId",
                schema: "public",
                table: "GziNutritionOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "BbkBloodOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "BbkBloodOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "BbkBloodOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "GziNutritionOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "GziNutritionOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "GziNutritionOrder");
        }
    }
}
