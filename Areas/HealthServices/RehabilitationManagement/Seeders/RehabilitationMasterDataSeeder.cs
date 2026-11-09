using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Seeders
{
    /// <summary>
    /// Data awal konfigurasi Rehab Medik (<c>BE-RHB-005</c>, <c>02-backend-architecture.md</c>
    /// bagian 8): empat disiplin dan peta jenis instrumen asesmen (RHB-DEC-004/017).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Seluruh disiplin diisi tidak aktif dan tanpa profesi.</b> Layanan hanya boleh aktif bila
    /// punya profesi klinis (<c>RHB-VAL-070</c>), dan profesi adalah data HR yang dipetakan admin
    /// (RHB-DEC-020). Admin memilih profesi lalu mengaktifkan Fisioterapi, Terapi Okupasi, dan
    /// Terapi Wicara; Ortotik-Prostetik tetap tidak aktif sampai fase perangkat (RHB-DEC-004).
    /// </para>
    /// <para>
    /// Tindakan per disiplin <b>tidak</b> diisi: tindakan dan tarifnya milik master tindakan dan
    /// dipilih admin (RHB-OQ-016).
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Disiplin dicari menurut kodenya, termasuk yang sudah terhapus; instrumen
    /// menurut pasangan disiplin dan jenisnya. Hanya yang belum ada yang ditambahkan, dan nilai yang
    /// sudah diubah admin tidak pernah ditimpa.
    /// </para>
    /// </remarks>
    public static class RehabilitationMasterDataSeeder
    {
        private sealed record DisiplinAwal(Guid Id, string Kode, string Nama, ClinicalInstrumentKind[] Instrumen);

        private static readonly DisiplinAwal[] DisciplineBaseline =
        {
            new(Guid.Parse("9a3c6e40-0001-4c30-ad03-7a4f1e8b9c01"), "PHYSIOTHERAPY", "Fisioterapi",
                [ClinicalInstrumentKind.PainScale, ClinicalInstrumentKind.RehabBergBalanceScale, ClinicalInstrumentKind.RehabTimedUpAndGo]),
            new(Guid.Parse("9a3c6e40-0002-4c30-ad03-7a4f1e8b9c02"), "OCCUPATIONAL_THERAPY", "Terapi Okupasi",
                [ClinicalInstrumentKind.RehabBarthelIndex, ClinicalInstrumentKind.RehabLawtonIadl]),
            new(Guid.Parse("9a3c6e40-0003-4c30-ad03-7a4f1e8b9c03"), "SPEECH_THERAPY", "Terapi Wicara",
                [ClinicalInstrumentKind.RehabGussSwallowing, ClinicalInstrumentKind.RehabFoisOralIntake]),
            new(Guid.Parse("9a3c6e40-0004-4c30-ad03-7a4f1e8b9c04"), "ORTHOTIC_PROSTHETIC", "Ortotik-Prostetik",
                [])
        };

        /// <summary>Instrumen asesmen medis Sp.KFR (disiplin kosong).</summary>
        private static readonly ClinicalInstrumentKind[] MedicalInstrumentBaseline =
            [ClinicalInstrumentKind.PainScale, ClinicalInstrumentKind.RehabBarthelIndex];

        public sealed record SeedResult(int Discipline, int Instrument);

        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(RehabilitationMasterDataSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;
            if (!seedEnabled)
            {
                logger.LogInformation("Seeder konfigurasi Rehab Medik dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            await SeedAsync(dbContext, logger, cancellationToken);
        }

        public static async Task<SeedResult> SeedAsync(ApplicationDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var disciplineAdded = 0;
            var instrumentAdded = 0;

            var codes = DisciplineBaseline.Select(x => x.Kode).ToList();
            var existing = await dbContext.RhbDisciplines
                .Where(x => codes.Contains(x.DisciplineCode))
                .Select(x => new { x.Id, x.DisciplineCode, x.IsDelete })
                .ToListAsync(cancellationToken);

            foreach (var baseline in DisciplineBaseline)
            {
                var found = existing.FirstOrDefault(x => x.DisciplineCode == baseline.Kode);
                var disciplineId = found?.Id ?? baseline.Id;

                if (found == null)
                {
                    dbContext.RhbDisciplines.Add(new RhbDiscipline
                    {
                        Id = baseline.Id,
                        DisciplineCode = baseline.Kode,
                        DisciplineName = baseline.Nama,
                        ProfessionId = null,
                        IsActive = false,
                        CreateDateTime = now
                    });
                    disciplineAdded++;
                }
                else if (found.IsDelete)
                {
                    // Disiplin yang sengaja dihapus admin tidak dihidupkan lagi beserta instrumennya.
                    continue;
                }

                instrumentAdded += await EnsureInstrumentsAsync(dbContext, disciplineId, baseline.Instrumen, now, cancellationToken);
            }

            instrumentAdded += await EnsureInstrumentsAsync(dbContext, null, MedicalInstrumentBaseline, now, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Seeder konfigurasi Rehab Medik menambahkan {Disiplin} disiplin dan {Instrumen} peta instrumen.",
                disciplineAdded, instrumentAdded);

            return new SeedResult(disciplineAdded, instrumentAdded);
        }

        private static async Task<int> EnsureInstrumentsAsync(
            ApplicationDbContext dbContext,
            Guid? disciplineId,
            IEnumerable<ClinicalInstrumentKind> kinds,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var present = await dbContext.RhbDisciplineInstruments
                .Where(x => x.DisciplineId == disciplineId)
                .Select(x => x.InstrumentKind)
                .ToListAsync(cancellationToken);

            var added = 0;
            foreach (var kind in kinds.Where(k => !present.Contains(k)))
            {
                dbContext.RhbDisciplineInstruments.Add(new RhbDisciplineInstrument
                {
                    DisciplineId = disciplineId,
                    InstrumentKind = kind,
                    IsActive = true,
                    CreateDateTime = now
                });
                added++;
            }

            return added;
        }
    }
}
