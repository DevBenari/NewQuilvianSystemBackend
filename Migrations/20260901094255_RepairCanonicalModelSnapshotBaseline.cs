using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Repairs the canonical EF Core migration model snapshot.
    ///
    /// The physical database already contains schema changes that were
    /// previously applied through migrations whose model snapshot metadata
    /// did not fully advance, including Radiology and Patient Encounter
    /// Company Guarantor integration.
    ///
    /// This migration intentionally performs no database DDL. Its target
    /// model establishes the canonical EF Core metadata baseline for schema
    /// that already exists physically in the database.
    /// </summary>
    public partial class RepairCanonicalModelSnapshotBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
            // Required physical schema already exists.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally empty.
            // Metadata baseline rollback must not alter existing schema/data.
        }
    }
}
