using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabOrderDiscipline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Discipline",
                schema: "public",
                table: "LabOrder",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_Discipline",
                schema: "public",
                table: "LabOrder",
                column: "Discipline");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LabOrder_Discipline",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "Discipline",
                schema: "public",
                table: "LabOrder");
        }
    }
}
