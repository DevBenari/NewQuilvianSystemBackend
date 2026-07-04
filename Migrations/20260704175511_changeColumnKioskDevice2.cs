using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeColumnKioskDevice2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstKioskDevice_SessionExpireMinutes",
                schema: "public",
                table: "MstKioskDevice");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_MstKioskDevice_SessionExpireMinutes",
                schema: "public",
                table: "MstKioskDevice",
                column: "SessionExpireMinutes");
        }
    }
}
