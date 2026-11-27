using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Seeders
{
    /// <summary>
    /// Kerangka definisi approval Pengadaan pada mesin workflow bersama (BE-PBJ-005;
    /// <c>02-backend-architecture.md</c> bagian 8 dan 10). BE-PBJ-VD-001 menambah dua definisi usulan
    /// blokir/buka blokir vendor (<c>PBJ-VND-BLOKIR</c>, <c>PBJ-VND-BUKA</c>; PBJ-DEC-021, PBJ-ASM-003).
    /// BE-PBJ-PS-001 menambah persetujuan persiapan paket (<c>PBJ-PKT-PERSIAPAN</c>) dan penetapan penyedia
    /// (<c>PBJ-PKT-PENETAPAN</c>; PBJ-DEC-072), sehingga totalnya sembilan definisi.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dibuat tidak aktif (fail-closed).</b> Langkah jabatan — Manajer Pengadaan, Direktur
    /// Keuangan — membutuhkan jabatan pemegangnya, yang berasal dari SOP rumah sakit
    /// (<c>PBJ-OQ-019</c>). Seeder tidak menebak jabatan itu. Admin mengisi jabatan pada langkah
    /// (atau baris matriks) lalu mengaktifkan definisi. Sampai saat itu, pengiriman ke approval
    /// ditolak dengan pesan "alur persetujuan pengadaan belum dikonfigurasi" (<c>PBJ-VAL-091</c>),
    /// bukan diloloskan tanpa jenjang yang benar.
    /// </para>
    /// <para>
    /// Seluruh langkah <c>AllowSelfApproval = false</c>. Langkah bersyarat memakai
    /// <c>ActivationConditionCode</c>: contoh langkah "Direktur Keuangan (melebihi pagu)" hanya dibuat
    /// bila PR membawa kode <c>PBJ_OVER_BUDGET</c> (PBJ-DEC-030). Pada paket, langkah tambahan dibuat bila metode
    /// dipilih di luar rentang HPS (<c>PBJ_METHOD_OUT_OF_RANGE</c>) atau nilai penetapan melebihi HPS
    /// (<c>PBJ_OVER_HPS</c>).
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Definisi dicari menurut kode dan versi 1, langkah menurut kode langkahnya,
    /// termasuk yang sudah dihapus. Yang sudah ada tidak pernah ditimpa, sehingga konfigurasi admin
    /// (jabatan, aktivasi) aman saat aplikasi start ulang.
    /// </para>
    /// </remarks>
    public static class ProcurementWorkflowDefinitionSeeder
    {
        private sealed record StepSeed(string Code, string Name, int Order, string ApproverSource, string? Condition);

        private sealed record DefinitionSeed(Guid Id, string Code, string Name, string RequestType, string Description, StepSeed[] Steps);

        private const string RequesterManager = WorkflowValueConstants.ApproverSource.RequesterManager;
        private const string Position = WorkflowValueConstants.ApproverSource.Position;

        private static readonly DefinitionSeed[] Definitions =
        {
            new(Guid.Parse("7c2e4a10-0001-4b6d-8e21-3f9a0c5d7e01"), ProcurementWorkflowCodes.PurchaseRequestDefinition,
                "Persetujuan Permintaan Pengadaan", ProcurementDocumentTypes.PurchaseRequest,
                "PR normal; nominal = nilai kebutuhan beli setelah cek stok (PBJ-DEC-027)",
                [
                    new("PBJ-PR-01", "Atasan langsung pengusul", 1, RequesterManager, null),
                    new("PBJ-PR-02", "Manajer Pengadaan", 2, Position, null),
                    new("PBJ-PR-03", "Persetujuan kebutuhan di luar rencana", 3, Position, ProcurementWorkflowCodes.OutOfPlanCondition),
                    new("PBJ-PR-04", "Direktur Keuangan (melebihi pagu)", 4, Position, ProcurementWorkflowCodes.OverBudgetCondition)
                ]),
            new(Guid.Parse("7c2e4a10-0002-4b6d-8e21-3f9a0c5d7e02"), ProcurementWorkflowCodes.UrgentPurchaseRequestDefinition,
                "Persetujuan Permintaan Pengadaan Mendesak", ProcurementDocumentTypes.PurchaseRequest,
                "PR mendesak; jalur lebih pendek, tetap disetujui sebelum pembelian (PBJ-DEC-016)",
                [
                    new("PBJ-PRM-01", "Manajer Pengadaan", 1, Position, null),
                    new("PBJ-PRM-02", "Persetujuan kebutuhan di luar rencana", 2, Position, ProcurementWorkflowCodes.OutOfPlanCondition),
                    new("PBJ-PRM-03", "Direktur Keuangan (melebihi pagu)", 3, Position, ProcurementWorkflowCodes.OverBudgetCondition)
                ]),
            new(Guid.Parse("7c2e4a10-0003-4b6d-8e21-3f9a0c5d7e03"), ProcurementWorkflowCodes.NeedPlanDefinition,
                "Persetujuan Rencana Kebutuhan", ProcurementDocumentTypes.NeedPlan,
                "Rencana kebutuhan tahunan dan revisinya",
                [
                    new("PBJ-RKB-01", "Atasan langsung penyusun", 1, RequesterManager, null),
                    new("PBJ-RKB-02", "Manajer Pengadaan", 2, Position, null)
                ]),
            new(Guid.Parse("7c2e4a10-0004-4b6d-8e21-3f9a0c5d7e04"), ProcurementWorkflowCodes.PackagePlanDefinition,
                "Persetujuan Rencana Paket", ProcurementDocumentTypes.PackagePlan,
                "Rencana paket hasil konsolidasi",
                [
                    new("PBJ-RPK-01", "Manajer Pengadaan", 1, Position, null)
                ]),
            new(Guid.Parse("7c2e4a10-0005-4b6d-8e21-3f9a0c5d7e05"), ProcurementWorkflowCodes.BudgetDefinition,
                "Persetujuan Pagu Pengadaan", ProcurementDocumentTypes.Budget,
                "Pagu dan revisinya",
                [
                    new("PBJ-PAGU-01", "Direktur Keuangan", 1, Position, null)
                ]),
            new(Guid.Parse("7c2e4a10-0006-4b6d-8e21-3f9a0c5d7e06"), ProcurementWorkflowCodes.VendorBlockDefinition,
                "Persetujuan Blokir Vendor", ProcurementDocumentTypes.VendorStatusRequest,
                "Usulan blokir vendor; hanya usulan yang disetujui yang memblokir vendor (PBJ-DEC-060)",
                [
                    new("PBJ-VND-BLOKIR-01", "Penyetuju status vendor", 1, Position, null)
                ]),
            new(Guid.Parse("7c2e4a10-0007-4b6d-8e21-3f9a0c5d7e07"), ProcurementWorkflowCodes.VendorUnblockDefinition,
                "Persetujuan Buka Blokir Vendor", ProcurementDocumentTypes.VendorStatusRequest,
                "Usulan buka blokir vendor (PBJ-ASM-003)",
                [
                    new("PBJ-VND-BUKA-01", "Penyetuju status vendor", 1, Position, null)
                ]),
            new(Guid.Parse("7c2e4a10-0008-4b6d-8e21-3f9a0c5d7e08"), ProcurementWorkflowCodes.PackagePreparationDefinition,
                "Persetujuan Persiapan Paket Pengadaan", ProcurementDocumentTypes.Package,
                "Spesifikasi, HPS, metode, dan checklist paket; nominal = HPS paket termasuk PPN (PBJ-DEC-072, PBJ-DEC-073)",
                [
                    new("PBJ-PKT-PERSIAPAN-01", "Penyetuju persiapan paket", 1, Position, null),
                    new("PBJ-PKT-PERSIAPAN-02", "Persetujuan metode di luar rentang HPS", 2, Position, ProcurementWorkflowCodes.MethodOutOfRangeCondition)
                ]),
            new(Guid.Parse("7c2e4a10-0009-4b6d-8e21-3f9a0c5d7e09"), ProcurementWorkflowCodes.PackageAwardDefinition,
                "Persetujuan Penetapan Penyedia", ProcurementDocumentTypes.PackageAward,
                "Vendor terpilih dan nilai penetapan; nominal = nilai penetapan termasuk PPN (PBJ-DEC-072, PBJ-DEC-085)",
                [
                    new("PBJ-PKT-PENETAPAN-01", "Penyetuju penetapan penyedia", 1, Position, null),
                    new("PBJ-PKT-PENETAPAN-02", "Persetujuan nilai melebihi HPS", 2, Position, ProcurementWorkflowCodes.OverHpsCondition)
                ])
        };

        public sealed record SeedResult(int Definition, int Step);

        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(ProcurementWorkflowDefinitionSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;
            if (!seedEnabled)
            {
                logger.LogInformation("Seeder definisi workflow Pengadaan dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            var result = await SeedAsync(dbContext, cancellationToken);
            logger.LogInformation(
                "Seeder definisi workflow Pengadaan: {Definition} definisi, {Step} langkah ditambahkan (definisi baru tidak aktif sampai dikonfigurasi admin).",
                result.Definition, result.Step);
        }

        public static async Task<SeedResult> SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var definitionAdded = 0;
            var stepAdded = 0;

            var codes = Definitions.Select(x => x.Code).ToList();
            var existing = await dbContext.Set<MstWorkflowDefinition>()
                .Where(x => codes.Contains(x.WorkflowCode) && x.Version == 1)
                .Select(x => new { x.Id, x.WorkflowCode, x.IsDelete })
                .ToListAsync(cancellationToken);

            foreach (var seed in Definitions)
            {
                var found = existing.FirstOrDefault(x => x.WorkflowCode == seed.Code);
                if (found is { IsDelete: true })
                {
                    // Definisi yang sengaja dihapus admin tidak dihidupkan lagi.
                    continue;
                }

                var definitionId = found?.Id ?? seed.Id;
                if (found == null)
                {
                    dbContext.Set<MstWorkflowDefinition>().Add(new MstWorkflowDefinition
                    {
                        Id = seed.Id,
                        WorkflowCode = seed.Code,
                        WorkflowName = seed.Name,
                        RequestType = seed.RequestType,
                        WorkflowCategory = ProcurementWorkflowCodes.WorkflowCategory,
                        Version = 1,
                        WorkflowStatus = "Draft",
                        IsActive = false,
                        AllowRequesterCancel = true,
                        AllowRequesterWithdraw = true,
                        AllowParallelApproval = false,
                        AllowStepSkip = false,
                        StopOnRejection = true,
                        IsDefault = true,
                        Description = seed.Description,
                        CreateDateTime = now
                    });
                    definitionAdded++;
                }

                var stepCodes = seed.Steps.Select(x => x.Code).ToList();
                var existingSteps = await dbContext.Set<MstWorkflowStep>()
                    .Where(x => x.WorkflowDefinitionId == definitionId && stepCodes.Contains(x.StepCode))
                    .Select(x => x.StepCode)
                    .ToListAsync(cancellationToken);

                foreach (var step in seed.Steps.Where(x => !existingSteps.Contains(x.Code)))
                {
                    dbContext.Set<MstWorkflowStep>().Add(new MstWorkflowStep
                    {
                        WorkflowDefinitionId = definitionId,
                        StepCode = step.Code,
                        StepName = step.Name,
                        StepOrder = step.Order,
                        StepType = WorkflowValueConstants.StepType.Approval,
                        ApprovalMode = "Any",
                        RequiredApprovalCount = 1,
                        ApproverSourceType = step.ApproverSource,
                        IsRequired = true,
                        IsParallel = false,
                        AllowDelegation = true,
                        AllowSelfApproval = false,
                        ActivationConditionCode = step.Condition,
                        Instructions = step.ApproverSource == Position
                            ? "Isi jabatan penyetuju sesuai SOP pengadaan sebelum definisi diaktifkan."
                            : null,
                        IsActive = true,
                        CreateDateTime = now
                    });
                    stepAdded++;
                }
            }

            if (definitionAdded + stepAdded > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return new SeedResult(definitionAdded, stepAdded);
        }
    }
}
