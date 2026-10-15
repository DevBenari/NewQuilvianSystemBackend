using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddNewbornAdmissionContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AppVersionSeeder previously added these columns without recording this migration.
            // Preserve those columns and their data while adding any that are still missing.
            migrationBuilder.Sql("""
                ALTER TABLE public."InpEpisode" ADD COLUMN IF NOT EXISTS "FamilyBillingPreference" character varying(20) NULL;
                ALTER TABLE public."InpEpisode" ADD COLUMN IF NOT EXISTS "NewbornAdmissionSource" character varying(20) NULL;
                ALTER TABLE public."InpEpisode" ADD COLUMN IF NOT EXISTS "NewbornCareMode" character varying(20) NULL;
                ALTER TABLE public."InpEpisode" ADD COLUMN IF NOT EXISTS "NewbornCareSource" character varying(200) NULL;
                ALTER TABLE public."InpEpisode" ADD COLUMN IF NOT EXISTS "PlannedDepositAmount" numeric(18,2) NULL;
                ALTER TABLE public."InpEpisode" ADD COLUMN IF NOT EXISTS "RequiresNeonatalIntensiveCare" boolean NOT NULL DEFAULT false;
                """);
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
