using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AllowBilTenderCancelledStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilTender_Status",
                schema: "public",
                table: "BilTender");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilTender_Status",
                schema: "public",
                table: "BilTender",
                sql: "\"Status\" IN ('CREATED','PENDING','SUCCEEDED','FAILED','EXPIRED','REVERSED','CANCELLED')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilTender_Status",
                schema: "public",
                table: "BilTender");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilTender_Status",
                schema: "public",
                table: "BilTender",
                sql: "\"Status\" IN ('CREATED','PENDING','SUCCEEDED','FAILED','EXPIRED','REVERSED')");
        }
    }
}
