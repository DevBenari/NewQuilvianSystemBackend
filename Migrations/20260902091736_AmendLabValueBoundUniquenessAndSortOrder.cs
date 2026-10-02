using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AmendLabValueBoundUniquenessAndSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LabValueBound_Procedure_Gender_AgeCategory",
                schema: "public",
                table: "LabValueBound");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "LabValueBound");

            migrationBuilder.CreateIndex(
                name: "IX_LabValueBound_Procedure_Gender_AgeCategory",
                schema: "public",
                table: "LabValueBound",
                columns: new[] { "ProcedureId", "GenderScope", "AgeCategoryId" },
                unique: true,
                filter: "\"IsDelete\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LabValueBound_Procedure_Gender_AgeCategory",
                schema: "public",
                table: "LabValueBound");

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "LabValueBound",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LabValueBound_Procedure_Gender_AgeCategory",
                schema: "public",
                table: "LabValueBound",
                columns: new[] { "ProcedureId", "GenderScope", "AgeCategoryId" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }
    }
}
