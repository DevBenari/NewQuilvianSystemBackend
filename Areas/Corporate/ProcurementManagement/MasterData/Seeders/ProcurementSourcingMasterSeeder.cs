using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Seeders
{
    /// <summary>
    /// Data awal master pemilihan penyedia (BE-PBJ-PS-001; <c>pemilihan-penyedia/02-backend-architecture.md</c> bagian 10).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Isi: dua metode contoh bertanda <c>IsSample</c> (PBJ-DEC-069) — Penunjukan Langsung sampai Rp 50.000.000 dengan satu
    /// penawaran tanpa evaluasi teknis, dan Pembandingan Penawaran di atas Rp 50.000.000 dengan tiga penawaran dan evaluasi
    /// teknis; enam jenis dokumen paket (tiga di antaranya tertutup harga, PBJ-DEC-079); dan empat baris dokumen wajib:
    /// KAK/TOR untuk jenis Jasa, Spesifikasi Teknis untuk jenis Aset, serta Dasar HPS untuk kedua metode contoh
    /// (PBJ-ASM-007, PBJ-DEC-086). Semuanya nilai awal; nilai final mengikuti SOP (PBJ-OQ-005, PBJ-OQ-019).
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Metode dan jenis dokumen dicari menurut Id atau kodenya, dokumen wajib menurut Id atau pasangan
    /// jenis dokumen × metode/jenis pengadaan, termasuk yang sudah dihapus. Hanya yang belum ada yang ditambahkan; nilai yang
    /// sudah diubah admin tidak ditimpa, dan data yang sengaja dihapus admin tidak dihidupkan lagi. Dokumen wajib per jenis
    /// pengadaan dilewati bila jenis <c>SERVICE</c>/<c>ASSET</c> tidak ada (diisi <c>ProcurementMasterDataSeeder</c>), dan
    /// dokumen wajib per metode dilewati bila metode atau jenis dokumennya sudah dihapus admin.
    /// </para>
    /// </remarks>
    public static class ProcurementSourcingMasterSeeder
    {
        private sealed record MethodSeed(
            Guid Id, string Code, string Name, decimal? MinHps, decimal? MaxHps, int MinimumQuotationCount,
            bool RequiresTechnicalEvaluation, string Description);

        private sealed record DocumentTypeSeed(Guid Id, string Code, string Name, string Stage, bool IsPriceSensitive);

        /// <summary>Tepat satu dari <paramref name="MethodCode"/> dan <paramref name="ProcurementTypeCode"/> terisi.</summary>
        private sealed record RequirementSeed(Guid Id, string DocumentTypeCode, string? MethodCode, string? ProcurementTypeCode);

        private const string DirectAppointmentCode = "PENUNJUKAN_LANGSUNG_CONTOH";
        private const string QuotationComparisonCode = "PEMBANDINGAN_PENAWARAN_CONTOH";

        private static readonly MethodSeed[] Methods =
        {
            new(Guid.Parse("6b1f2c70-0501-4e0a-9c31-5a7d2e4f8b51"), DirectAppointmentCode,
                "Penunjukan Langsung (contoh)", null, 50_000_000m, 1, false,
                "Nilai contoh, bukan SOP: HPS sampai Rp 50.000.000, cukup satu penawaran, tanpa evaluasi teknis. Ganti dengan metode SOP sebelum produksi."),
            new(Guid.Parse("6b1f2c70-0502-4e0a-9c31-5a7d2e4f8b52"), QuotationComparisonCode,
                "Pembandingan Penawaran (contoh)", 50_000_000m, null, 3, true,
                "Nilai contoh, bukan SOP: HPS di atas Rp 50.000.000, minimal tiga penawaran, wajib evaluasi teknis. Ganti dengan metode SOP sebelum produksi.")
        };

        private static readonly DocumentTypeSeed[] DocumentTypes =
        {
            new(Guid.Parse("6b1f2c70-0601-4e0a-9c31-5a7d2e4f8b61"), "KAK_TOR", "Kerangka Acuan Kerja / TOR",
                ProcurementStatuses.PackageDocumentStage.Preparation, false),
            new(Guid.Parse("6b1f2c70-0602-4e0a-9c31-5a7d2e4f8b62"), "SPESIFIKASI_TEKNIS", "Spesifikasi Teknis",
                ProcurementStatuses.PackageDocumentStage.Preparation, false),
            new(Guid.Parse("6b1f2c70-0603-4e0a-9c31-5a7d2e4f8b63"), "DASAR_HPS", "Dasar Harga Perkiraan Sendiri",
                ProcurementStatuses.PackageDocumentStage.Preparation, true),
            new(Guid.Parse("6b1f2c70-0604-4e0a-9c31-5a7d2e4f8b64"), "SURAT_PENAWARAN_HARGA", "Surat Penawaran Harga",
                ProcurementStatuses.PackageDocumentStage.Quotation, true),
            new(Guid.Parse("6b1f2c70-0605-4e0a-9c31-5a7d2e4f8b65"), "LAMPIRAN_TEKNIS_PENAWARAN", "Lampiran Teknis Penawaran",
                ProcurementStatuses.PackageDocumentStage.Quotation, false),
            new(Guid.Parse("6b1f2c70-0606-4e0a-9c31-5a7d2e4f8b66"), "BERITA_ACARA_NEGOSIASI", "Berita Acara Negosiasi",
                ProcurementStatuses.PackageDocumentStage.Negotiation, true)
        };

        private static readonly RequirementSeed[] Requirements =
        {
            new(Guid.Parse("6b1f2c70-0701-4e0a-9c31-5a7d2e4f8b71"), "KAK_TOR", null, "SERVICE"),
            new(Guid.Parse("6b1f2c70-0702-4e0a-9c31-5a7d2e4f8b72"), "SPESIFIKASI_TEKNIS", null, "ASSET"),
            new(Guid.Parse("6b1f2c70-0703-4e0a-9c31-5a7d2e4f8b73"), "DASAR_HPS", DirectAppointmentCode, null),
            new(Guid.Parse("6b1f2c70-0704-4e0a-9c31-5a7d2e4f8b74"), "DASAR_HPS", QuotationComparisonCode, null)
        };

        public sealed record SeedResult(int Method, int DocumentType, int Requirement);

        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(ProcurementSourcingMasterSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;
            if (!seedEnabled)
            {
                logger.LogInformation("Seeder master pemilihan penyedia dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            var result = await SeedAsync(dbContext, cancellationToken);
            logger.LogInformation(
                "Seeder master pemilihan penyedia: {Method} metode contoh, {DocumentType} jenis dokumen paket, {Requirement} dokumen wajib ditambahkan.",
                result.Method, result.DocumentType, result.Requirement);
        }

        public static async Task<SeedResult> SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            // Metode contoh.
            var methodCodes = Methods.Select(x => x.Code).ToList();
            var methodIds = Methods.Select(x => x.Id).ToList();
            var existingMethods = await dbContext.MstProcurementMethods
                .Where(x => methodIds.Contains(x.Id) || methodCodes.Contains(x.MethodCode))
                .Select(x => new { x.Id, x.MethodCode, x.IsDelete })
                .ToListAsync(cancellationToken);
            var methodAdded = 0;
            foreach (var seed in Methods.Where(x =>
                         !existingMethods.Any(e => e.Id == x.Id || e.MethodCode == x.Code)))
            {
                dbContext.MstProcurementMethods.Add(new MstProcurementMethod
                {
                    Id = seed.Id, MethodCode = seed.Code, MethodName = seed.Name,
                    MinHpsAmount = seed.MinHps, MaxHpsAmount = seed.MaxHps,
                    MinimumQuotationCount = seed.MinimumQuotationCount,
                    RequiresTechnicalEvaluation = seed.RequiresTechnicalEvaluation,
                    IsSample = true, Description = seed.Description, IsActive = true, CreateDateTime = now
                });
                methodAdded++;
            }

            // Jenis dokumen paket.
            var documentTypeCodes = DocumentTypes.Select(x => x.Code).ToList();
            var documentTypeIds = DocumentTypes.Select(x => x.Id).ToList();
            var existingDocumentTypes = await dbContext.MstProcurementPackageDocumentTypes
                .Where(x => documentTypeIds.Contains(x.Id) || documentTypeCodes.Contains(x.DocumentTypeCode))
                .Select(x => new { x.Id, x.DocumentTypeCode, x.IsDelete })
                .ToListAsync(cancellationToken);
            var documentTypeAdded = 0;
            foreach (var seed in DocumentTypes.Where(x =>
                         !existingDocumentTypes.Any(e => e.Id == x.Id || e.DocumentTypeCode == x.Code)))
            {
                dbContext.MstProcurementPackageDocumentTypes.Add(new MstProcurementPackageDocumentType
                {
                    Id = seed.Id, DocumentTypeCode = seed.Code, DocumentTypeName = seed.Name,
                    Stage = seed.Stage, IsPriceSensitive = seed.IsPriceSensitive, IsActive = true, CreateDateTime = now
                });
                documentTypeAdded++;
            }

            // Id metode dan jenis dokumen yang hidup: baris baru di atas, atau baris lama yang belum dihapus admin.
            var methodIdByCode = Methods
                .Where(x => !existingMethods.Any(e => e.Id == x.Id || e.MethodCode == x.Code))
                .ToDictionary(x => x.Code, x => x.Id, StringComparer.Ordinal);
            foreach (var existing in existingMethods.Where(x => !x.IsDelete))
                methodIdByCode.TryAdd(existing.MethodCode, existing.Id);

            var documentTypeIdByCode = DocumentTypes
                .Where(x => !existingDocumentTypes.Any(e => e.Id == x.Id || e.DocumentTypeCode == x.Code))
                .ToDictionary(x => x.Code, x => x.Id, StringComparer.Ordinal);
            foreach (var existing in existingDocumentTypes.Where(x => !x.IsDelete))
                documentTypeIdByCode.TryAdd(existing.DocumentTypeCode, existing.Id);

            var typeCodes = Requirements.Where(x => x.ProcurementTypeCode != null).Select(x => x.ProcurementTypeCode!).Distinct().ToList();
            var procurementTypeIdByCode = await dbContext.MstProcurementTypes
                .Where(x => typeCodes.Contains(x.TypeCode) && !x.IsDelete)
                .ToDictionaryAsync(x => x.TypeCode, x => x.Id, cancellationToken);

            // Dokumen wajib.
            var requirementIds = Requirements.Select(x => x.Id).ToList();
            var candidateDocumentTypeIds = documentTypeIdByCode.Values.ToList();
            var existingRequirements = await dbContext.MstProcurementPackageDocumentRequirements
                .Where(x => requirementIds.Contains(x.Id) || candidateDocumentTypeIds.Contains(x.DocumentTypeId))
                .Select(x => new { x.Id, x.DocumentTypeId, x.MethodId, x.ProcurementTypeId })
                .ToListAsync(cancellationToken);
            var requirementAdded = 0;
            foreach (var seed in Requirements)
            {
                if (!documentTypeIdByCode.TryGetValue(seed.DocumentTypeCode, out var documentTypeId))
                    continue;

                Guid? methodId = null;
                Guid? procurementTypeId = null;
                if (seed.MethodCode != null)
                {
                    if (!methodIdByCode.TryGetValue(seed.MethodCode, out var foundMethodId)) continue;
                    methodId = foundMethodId;
                }
                else
                {
                    if (!procurementTypeIdByCode.TryGetValue(seed.ProcurementTypeCode!, out var foundTypeId)) continue;
                    procurementTypeId = foundTypeId;
                }

                var alreadyThere = existingRequirements.Any(e => e.Id == seed.Id
                    || (e.DocumentTypeId == documentTypeId && e.MethodId == methodId && e.ProcurementTypeId == procurementTypeId));
                if (alreadyThere) continue;

                dbContext.MstProcurementPackageDocumentRequirements.Add(new MstProcurementPackageDocumentRequirement
                {
                    Id = seed.Id, DocumentTypeId = documentTypeId, MethodId = methodId,
                    ProcurementTypeId = procurementTypeId, IsActive = true, CreateDateTime = now
                });
                requirementAdded++;
            }

            if (methodAdded + documentTypeAdded + requirementAdded > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return new SeedResult(methodAdded, documentTypeAdded, requirementAdded);
        }
    }
}
