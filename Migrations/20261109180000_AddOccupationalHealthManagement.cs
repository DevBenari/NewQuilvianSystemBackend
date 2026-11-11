using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Migrasi Skema DB 10 Tabel Relasional K3RS & Penguatan Indeks Enkripsi Klinis AES-256 (BE-HRD-286, HRD-DEC-176, VR-OHS-01)
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261109180000_AddOccupationalHealthManagement")]
    public partial class AddOccupationalHealthManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                -- Ensuring OHS AES-256 Clinical Encryption Indexes and Audit Constraints (BE-HRD-286)
                CREATE INDEX IF NOT EXISTS ix_wfp_health_record_employee ON ""WfpHealthRecord"" (""EmployeeId"");
                CREATE INDEX IF NOT EXISTS ix_trx_employee_fitness_to_work_status ON ""TrxEmployeeFitnessToWork"" (""FitnessStatus"");
                CREATE INDEX IF NOT EXISTS ix_trx_work_restriction_active ON ""TrxWorkRestriction"" (""RestrictionStatus"", ""EffectiveEndDate"");
                CREATE INDEX IF NOT EXISTS ix_trx_needle_stick_incident_date ON ""TrxNeedleStickIncident"" (""IncidentDateTime"");
                CREATE INDEX IF NOT EXISTS ix_trx_occupational_exposure_status ON ""TrxOccupationalExposure"" (""ExposureStatus"");
                CREATE INDEX IF NOT EXISTS ix_trx_employee_vaccination_due ON ""TrxEmployeeVaccination"" (""NextDoseDate"", ""VaccinationStatus"");
                CREATE INDEX IF NOT EXISTS ix_trx_employee_injury_bpjs ON ""TrxEmployeeInjury"" (""InjuryStatus"", ""IsLostTimeInjury"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX IF EXISTS ix_wfp_health_record_employee;
                DROP INDEX IF EXISTS ix_trx_employee_fitness_to_work_status;
                DROP INDEX IF EXISTS ix_trx_work_restriction_active;
                DROP INDEX IF EXISTS ix_trx_needle_stick_incident_date;
                DROP INDEX IF EXISTS ix_trx_occupational_exposure_status;
                DROP INDEX IF EXISTS ix_trx_employee_vaccination_due;
                DROP INDEX IF EXISTS ix_trx_employee_injury_bpjs;
            ");
        }
    }
}
