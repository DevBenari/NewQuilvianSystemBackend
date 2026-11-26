using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Seeders
{
    /// <summary>
    /// Definisi workflow "Persetujuan Perjalanan Dinas" pada mesin workflow bersama (BE-HRD-374;
    /// <c>02-backend-architecture.md</c> bagian 36.7, kontrak <c>v28</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dibuat aktif.</b> Satu-satunya tahap bawaan adalah atasan langsung pemohon
    /// (<c>RequesterManager</c>), yang tidak memerlukan isian jabatan dari admin. Karena itu
    /// definisi langsung dapat dipakai saat SPPD diajukan (<c>HRD-DEC-224</c>; usulan desain yang
    /// disetujui bersama kontrak, pertanyaan <c>HRD-Q-222</c> masih terbuka). Admin HR dapat
    /// menambah tahap atau menonaktifkannya di Master Workflow tanpa rilis baru.
    /// </para>
    /// <para>
    /// <b>Idempoten.</b> Definisi dicari menurut kode dan versi 1, tahap menurut kode tahapnya,
    /// termasuk yang sudah dihapus. Yang sudah ada tidak pernah ditimpa, sehingga perubahan admin
    /// (tahap tambahan, penonaktifan, penghapusan) aman saat aplikasi start ulang.
    /// </para>
    /// </remarks>
    public static class BusinessTravelWorkflowDefinitionSeeder
    {
        private static readonly Guid DefinitionId = Guid.Parse("5b1d7e30-0001-4c2a-9f10-7a3e2d4c6b01");

        public sealed record SeedResult(int Definition, int Step);

        public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(BusinessTravelWorkflowDefinitionSeeder));

            var seedEnabled = configuration.GetValue<bool?>("SeedDefaultData:Enabled") ?? true;
            if (!seedEnabled)
            {
                logger.LogInformation("Seeder definisi workflow Perjalanan Dinas dilewati karena SeedDefaultData dimatikan.");
                return;
            }

            var result = await SeedAsync(dbContext, cancellationToken);
            logger.LogInformation(
                "Seeder definisi workflow Perjalanan Dinas: {Definition} definisi, {Step} tahap ditambahkan.",
                result.Definition, result.Step);
        }

        public static async Task<SeedResult> SeedAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var definitionAdded = 0;
            var stepAdded = 0;

            var found = await dbContext.Set<MstWorkflowDefinition>()
                .Where(x => x.WorkflowCode == BusinessTravelWorkflowCodes.DefinitionCode && x.Version == 1)
                .Select(x => new { x.Id, x.IsDelete })
                .FirstOrDefaultAsync(cancellationToken);

            if (found is { IsDelete: true })
            {
                // Definisi yang sengaja dihapus admin tidak dihidupkan lagi.
                return new SeedResult(0, 0);
            }

            var definitionId = found?.Id ?? DefinitionId;
            if (found == null)
            {
                dbContext.Set<MstWorkflowDefinition>().Add(new MstWorkflowDefinition
                {
                    Id = DefinitionId,
                    WorkflowCode = BusinessTravelWorkflowCodes.DefinitionCode,
                    WorkflowName = "Persetujuan Perjalanan Dinas",
                    RequestType = BusinessTravelWorkflowCodes.RequestType,
                    WorkflowCategory = BusinessTravelWorkflowCodes.WorkflowCategory,
                    Version = 1,
                    WorkflowStatus = BusinessTravelWorkflowCodes.DefinitionActiveStatus,
                    IsActive = true,
                    AllowRequesterCancel = true,
                    AllowRequesterWithdraw = true,
                    AllowParallelApproval = false,
                    AllowStepSkip = false,
                    StopOnRejection = true,
                    IsDefault = true,
                    Description = "Persetujuan SPPD sebelum uang muka, presensi Dinas Luar, dan gembok roster (HRD-DEC-224, HRD-DEC-226). Tahap dapat ditambah admin.",
                    CreateDateTime = now
                });
                definitionAdded++;
            }

            var stepExists = await dbContext.Set<MstWorkflowStep>()
                .AnyAsync(x =>
                    x.WorkflowDefinitionId == definitionId &&
                    x.StepCode == BusinessTravelWorkflowCodes.ManagerStepCode,
                    cancellationToken);

            if (!stepExists)
            {
                dbContext.Set<MstWorkflowStep>().Add(new MstWorkflowStep
                {
                    WorkflowDefinitionId = definitionId,
                    StepCode = BusinessTravelWorkflowCodes.ManagerStepCode,
                    StepName = "Atasan langsung pemohon",
                    StepOrder = 1,
                    StepType = WorkflowValueConstants.StepType.Approval,
                    ApprovalMode = WorkflowValueConstants.ApprovalMode.Any,
                    RequiredApprovalCount = 1,
                    ApproverSourceType = WorkflowValueConstants.ApproverSource.RequesterManager,
                    IsRequired = true,
                    IsParallel = false,
                    AllowDelegation = true,
                    AllowSelfApproval = false,
                    EscalationAfterHours = BusinessTravelWorkflowCodes.ManagerStepSlaHours,
                    IsActive = true,
                    CreateDateTime = now
                });
                stepAdded++;
            }

            if (definitionAdded + stepAdded > 0)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return new SeedResult(definitionAdded, stepAdded);
        }
    }
}
