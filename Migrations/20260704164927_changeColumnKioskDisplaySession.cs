using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeColumnKioskDisplaySession : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionExpireMinutes",
                schema: "public",
                table: "MstQueueDisplayDevice",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionExpireMinutes",
                schema: "public",
                table: "MstKioskDevice",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstQueueDisplayDevice_SessionExpireMinutes",
                schema: "public",
                table: "MstQueueDisplayDevice",
                column: "SessionExpireMinutes");

            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_SessionExpireMinutes",
                schema: "public",
                table: "MstKioskDevice",
                column: "SessionExpireMinutes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstQueueDisplayDevice_SessionExpireMinutes",
                schema: "public",
                table: "MstQueueDisplayDevice");

            migrationBuilder.DropIndex(
                name: "IX_MstKioskDevice_SessionExpireMinutes",
                schema: "public",
                table: "MstKioskDevice");

            migrationBuilder.DropColumn(
                name: "SessionExpireMinutes",
                schema: "public",
                table: "MstQueueDisplayDevice");

            migrationBuilder.DropColumn(
                name: "SessionExpireMinutes",
                schema: "public",
                table: "MstKioskDevice");
        }
    }
}
