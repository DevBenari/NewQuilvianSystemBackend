using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddNewbornAdmissionContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "FamilyBillingPreference", schema: "public", table: "InpEpisode",
                type: "character varying(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<string>(name: "NewbornAdmissionSource", schema: "public", table: "InpEpisode",
                type: "character varying(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<string>(name: "NewbornCareMode", schema: "public", table: "InpEpisode",
                type: "character varying(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<string>(name: "NewbornCareSource", schema: "public", table: "InpEpisode",
                type: "character varying(200)", maxLength: 200, nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "PlannedDepositAmount", schema: "public", table: "InpEpisode",
                type: "numeric(18,2)", precision: 18, scale: 2, nullable: true);
            migrationBuilder.AddColumn<bool>(name: "RequiresNeonatalIntensiveCare", schema: "public", table: "InpEpisode",
                type: "boolean", nullable: false, defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "FamilyBillingPreference", schema: "public", table: "InpEpisode");
            migrationBuilder.DropColumn(name: "NewbornAdmissionSource", schema: "public", table: "InpEpisode");
            migrationBuilder.DropColumn(name: "NewbornCareMode", schema: "public", table: "InpEpisode");
            migrationBuilder.DropColumn(name: "NewbornCareSource", schema: "public", table: "InpEpisode");
            migrationBuilder.DropColumn(name: "PlannedDepositAmount", schema: "public", table: "InpEpisode");
            migrationBuilder.DropColumn(name: "RequiresNeonatalIntensiveCare", schema: "public", table: "InpEpisode");
        }
    }
}
