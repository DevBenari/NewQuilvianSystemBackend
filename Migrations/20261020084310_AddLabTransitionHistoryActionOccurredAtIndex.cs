using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabTransitionHistoryActionOccurredAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LabTransitionHistory_Action_OccurredAt",
                schema: "public",
                table: "LabTransitionHistory",
                columns: new[] { "Action", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LabTransitionHistory_Action_OccurredAt",
                schema: "public",
                table: "LabTransitionHistory");
        }
    }
}
