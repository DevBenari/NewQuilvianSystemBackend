using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyDoctorRosterOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRosterOverride",
                schema: "public",
                table: "EmgDoctorAssignment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RosterOverrideReason",
                schema: "public",
                table: "EmgDoctorAssignment",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM public."EmgDoctorAssignment" WHERE "IsRosterOverride") THEN
                        RAISE EXCEPTION 'Down BE-IGD-056 dihentikan: ada penugasan DPJP IGD di luar jadwal jaga (IsRosterOverride benar). Menghapus kolom ini menghilangkan jejak override tanpa bekas.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropColumn(
                name: "IsRosterOverride",
                schema: "public",
                table: "EmgDoctorAssignment");

            migrationBuilder.DropColumn(
                name: "RosterOverrideReason",
                schema: "public",
                table: "EmgDoctorAssignment");
        }
    }
}
