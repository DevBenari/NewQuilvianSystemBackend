using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class addColumnLingkarBayi : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HeadCircumference",
                schema: "public",
                table: "TrxPatientVitalSign",
                type: "numeric(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeadCircumference",
                schema: "public",
                table: "TrxPatientVitalSign");
        }
    }
}
