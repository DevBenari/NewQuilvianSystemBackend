using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class A0AuthorizationIntegrityProjection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUserOrganization_UserId_DepartmentId_PositionId",
                schema: "public",
                table: "AspNetUserOrganization");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserOrganization_UserId_DepartmentId_PositionId_Effec~",
                schema: "public",
                table: "AspNetUserOrganization");

            migrationBuilder.AddColumn<Guid>(
                name: "SourceAssignmentId",
                schema: "public",
                table: "AspNetUserOrganization",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserOrganization_SourceAssignmentId",
                schema: "public",
                table: "AspNetUserOrganization",
                column: "SourceAssignmentId",
                unique: true,
                filter: "\"SourceAssignmentId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserOrganization_UserId_DepartmentId_PositionId_Effec~",
                schema: "public",
                table: "AspNetUserOrganization",
                columns: new[] { "UserId", "DepartmentId", "PositionId", "EffectiveStartDate" },
                unique: true,
                filter: "\"IsDelete\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUserOrganization_SourceAssignmentId",
                schema: "public",
                table: "AspNetUserOrganization");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserOrganization_UserId_DepartmentId_PositionId_Effec~",
                schema: "public",
                table: "AspNetUserOrganization");

            migrationBuilder.DropColumn(
                name: "SourceAssignmentId",
                schema: "public",
                table: "AspNetUserOrganization");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserOrganization_UserId_DepartmentId_PositionId",
                schema: "public",
                table: "AspNetUserOrganization",
                columns: new[] { "UserId", "DepartmentId", "PositionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserOrganization_UserId_DepartmentId_PositionId_Effec~",
                schema: "public",
                table: "AspNetUserOrganization",
                columns: new[] { "UserId", "DepartmentId", "PositionId", "EffectiveStartDate" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }
    }
}
