using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdEmploymentLifecycleSchedulerService : IHrdEmploymentLifecycleSchedulerService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHrdOnboardingCaseService _caseService;
        private readonly IEmployeeEmploymentRecordService _employeeEmploymentRecordService;
        private readonly ILogger<HrdEmploymentLifecycleSchedulerService> _logger;
        private readonly QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services.IHrdNotificationService _notificationService;
        // Diambil saat dipakai: integrasi workflow dan penyelarasan lifecycle (B6) bergantung pada WorkflowService.
        private readonly IServiceProvider _serviceProvider;

        private const string DailyTickLockKey = "HRD_EMPLOYMENT_LIFECYCLE_DAILY_TICK";

        public HrdEmploymentLifecycleSchedulerService(
            ApplicationDbContext dbContext,
            IHrdOnboardingCaseService caseService,
            IEmployeeEmploymentRecordService employeeEmploymentRecordService,
            ILogger<HrdEmploymentLifecycleSchedulerService> logger,
            QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services.IHrdNotificationService notificationService,
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _caseService = caseService ?? throw new ArgumentNullException(nameof(caseService));
            _employeeEmploymentRecordService = employeeEmploymentRecordService ?? throw new ArgumentNullException(nameof(employeeEmploymentRecordService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> ExecuteDailyTickAsync(
            DateOnly? testDate = null,
            CancellationToken cancellationToken = default)
        {
            // BE-HRD-423: dua instance aplikasi tidak menjalankan langkah harian bersamaan. Kunci sesi PostgreSQL
            // dipegang selama koneksi yang dibuka di sini; instance lain melewati tick ini.
            var isNpgsql = _dbContext.Database.IsNpgsql();
            if (isNpgsql)
            {
                await _dbContext.Database.OpenConnectionAsync(cancellationToken);
                var acquired = await _dbContext.Database
                    .SqlQueryRaw<bool>("SELECT pg_try_advisory_lock(hashtext({0})) AS \"Value\"", DailyTickLockKey)
                    .SingleAsync(cancellationToken);
                if (!acquired)
                {
                    _logger.LogInformation("Langkah harian siklus kepegawaian sedang dijalankan instance lain; tick ini dilewati.");
                    await _dbContext.Database.CloseConnectionAsync();
                    return 0;
                }
            }

            try
            {
                return await ExecuteDailyTickCoreAsync(testDate, cancellationToken);
            }
            finally
            {
                if (isNpgsql)
                {
                    await _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(hashtext({0}))", [DailyTickLockKey], CancellationToken.None);
                    await _dbContext.Database.CloseConnectionAsync();
                }
            }
        }

        private async Task<int> ExecuteDailyTickCoreAsync(
            DateOnly? testDate,
            CancellationToken cancellationToken)
        {
            DateOnly today;
            if (testDate.HasValue)
            {
                today = testDate.Value;
            }
            else
            {
                var wibZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var wibNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, wibZone);
                today = DateOnly.FromDateTime(wibNow);
            }
            var todayDateTime = today.ToDateTime(TimeOnly.MinValue);
            var processedCount = 0;

            _logger.LogInformation("Menjalankan langkah harian siklus hidup kepegawaian untuk tanggal {Today}", today);

            // 1. Pengaktifan Kontrak Lanjutan Berjalan pada Tanggal Mulai (AUD-08-01)
            var contractsToActivate = await _dbContext.WfpContractHistories
                .Where(c => !c.IsDelete &&
                            c.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusSigned &&
                            !c.IsCurrent &&
                            c.StartDate <= todayDateTime)
                .ToListAsync(cancellationToken);

            foreach (var contract in contractsToActivate)
            {
                contract.ContractStatus = "Active";
                contract.IsCurrent = true;
                contract.UpdateDateTime = DateTime.UtcNow;

                // Nonaktifkan seluruh kontrak berjalan sebelumnya untuk profil yang sama
                var previousCurrentContracts = await _dbContext.WfpContractHistories
                    .Where(c => c.WorkforceProfileId == contract.WorkforceProfileId && c.Id != contract.Id && c.IsCurrent && !c.IsDelete)
                    .ToListAsync(cancellationToken);

                foreach (var prev in previousCurrentContracts)
                {
                    prev.IsCurrent = false;
                    if (prev.ContractStatus == "Active" || prev.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusSigned)
                    {
                        prev.ContractStatus = "Completed";
                    }
                    prev.UpdateDateTime = DateTime.UtcNow;
                }

                // FIN-HRD-08-BE-008 / REG-HRD-08-005: Sinkronisasi mirror MstWorkforceProfile saat tanggal penempatan efektif tiba
                var profile = await _dbContext.MstWorkforceProfiles
                    .FirstOrDefaultAsync(p => p.Id == contract.WorkforceProfileId && !p.IsDelete, cancellationToken);
                if (profile != null)
                {
                    var currentPos = await _dbContext.Set<QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models.WfpPositionAssignment>()
                        .Where(p => p.WorkforceProfileId == contract.WorkforceProfileId && p.IsPrimary && p.IsActive && !p.IsDelete && p.EffectiveStartDate <= todayDateTime)
                        .OrderByDescending(p => p.EffectiveStartDate)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (currentPos != null)
                    {
                        profile.PrimaryPositionId = currentPos.PositionId;
                        if (currentPos.DepartmentId.HasValue && currentPos.DepartmentId.Value != Guid.Empty)
                        {
                            profile.PrimaryDepartmentId = currentPos.DepartmentId.Value;
                        }
                        profile.UpdateDateTime = DateTime.UtcNow;
                    }
                }

                // AUD-08-01: Sinkronisasi catatan riwayat kepegawaian
                await _employeeEmploymentRecordService.SyncFromContractAsync(contract, Guid.Empty, cancellationToken);

                processedCount++;
            }

            // 2. Pengakhiran Kontrak yang Lewat Tanggal Berakhir (Sehari Sesudah EndDate) (AUD-08-01)
            var expiredContracts = await _dbContext.WfpContractHistories
                .Where(c => !c.IsDelete &&
                            (c.ContractStatus == "Active" || c.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusSigned) &&
                            c.EndDate.HasValue &&
                            c.EndDate.Value < todayDateTime)
                .ToListAsync(cancellationToken);

            foreach (var contract in expiredContracts)
            {
                // Cek apakah ada kontrak lanjutan yang sudah Signed
                var hasSignedRenewal = await _dbContext.WfpContractHistories
                    .AnyAsync(r => r.PreviousContractHistoryId == contract.Id &&
                                   r.ContractStatus == EmploymentLifecycleValueConstants.ContractStatusSigned &&
                                   !r.IsDelete, cancellationToken);

                if (!hasSignedRenewal)
                {
                    // Cek keputusan kontrak
                    var decision = await _dbContext.HrdContractDecisions
                        .FirstOrDefaultAsync(d => d.ContractHistoryId == contract.Id && !d.IsDelete, cancellationToken);

                    if (decision != null && decision.DecisionStatus == ContractDecisionStatus.Approved &&
                        decision.DecisionType == ContractDecisionType.NotRenew)
                    {
                        contract.ContractStatus = "Ended";
                        contract.IsCurrent = false;
                        contract.UpdateDateTime = DateTime.UtcNow;
                    }
                    else if (decision == null || decision.DecisionStatus != ContractDecisionStatus.Approved)
                    {
                        // Kontrak berakhir tanpa keputusan disetujui
                        contract.ContractStatus = "LapsedWithoutDecision";
                        contract.IsCurrent = false;
                        contract.UpdateDateTime = DateTime.UtcNow;
                    }

                    processedCount++;
                }
                else
                {
                    if (contract.IsCurrent)
                    {
                        contract.IsCurrent = false;
                        contract.ContractStatus = "Completed";
                        contract.UpdateDateTime = DateTime.UtcNow;
                        processedCount++;
                    }
                }
            }

            // 3. Evaluasi Otomatis Masa Percobaan yang Lewat Tanggal Berakhir (Lulus Otomatis & Penutupan Penilaian Terbuka)
            var expiredProbations = await _dbContext.HrdProbations
                .Include(p => p.Assessments)
                .Where(p => !p.IsDelete &&
                            (p.ProbationStatus == ProbationStatus.Running || p.ProbationStatus == ProbationStatus.AwaitingDecision) &&
                            p.EndDate < today)
                .ToListAsync(cancellationToken);

            var expiredAssessmentWorkflows = new System.Collections.Generic.List<(Guid InstanceId, Guid RequesterUserId)>();

            foreach (var probation in expiredProbations)
            {
                probation.ProbationStatus = ProbationStatus.PassedAutomatically;
                probation.ClosedAt = DateTime.UtcNow;
                probation.RowVersion = Guid.NewGuid();
                probation.UpdateDateTime = DateTime.UtcNow;

                // Tutup penilaian terbuka yang belum sempat diputus (ClosedOnExpiry)
                foreach (var assessment in probation.Assessments.Where(a => !a.IsDelete &&
                    (a.AssessmentStatus == ProbationAssessmentStatus.Submitted || a.AssessmentStatus == ProbationAssessmentStatus.Returned)))
                {
                    if (assessment.WorkflowInstanceId.HasValue)
                    {
                        expiredAssessmentWorkflows.Add((assessment.WorkflowInstanceId.Value, assessment.SubmittedByUserId));
                    }

                    assessment.AssessmentStatus = ProbationAssessmentStatus.ClosedOnExpiry;
                    assessment.DecisionNote = "Penilaian ditutup otomatis karena masa percobaan telah melewati tanggal akhir tanpa persetujuan (Lulus Otomatis).";
                    assessment.DecidedAt = DateTime.UtcNow;
                    assessment.RowVersion = Guid.NewGuid();
                    assessment.UpdateDateTime = DateTime.UtcNow;
                }

                processedCount++;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // 3b. B6 (HRD-DEC-085 butir 9): penilaian yang ditutup otomatis ikut ditarik dari Kotak Masuk penyetuju,
            // atas nama pengajunya. Penangan lifecycle tidak mengubah penilaian yang sudah ClosedOnExpiry.
            if (expiredAssessmentWorkflows.Count > 0)
            {
                var integration = _serviceProvider.GetRequiredService<IHrdEmploymentLifecycleWorkflowIntegrationService>();
                foreach (var (instanceId, requesterUserId) in expiredAssessmentWorkflows)
                {
                    try
                    {
                        var withdrawn = await integration.WithdrawAsync(
                            instanceId,
                            requesterUserId,
                            "Masa percobaan berakhir tanpa persetujuan; pegawai lulus otomatis (HRD-DEC-085 butir 9).",
                            cancellationToken);
                        if (!withdrawn.Success)
                        {
                            _logger.LogWarning("Instance penilaian {InstanceId} gagal ditarik: {Message}", instanceId, withdrawn.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Instance penilaian {InstanceId} gagal ditarik.", instanceId);
                    }
                }
            }

            // 3c. B6: penyelarasan dokumen T15-T18 yang tertinggal bila panggilan sesudah aksi workflow gagal.
            processedCount += await ReconcileLifecycleWorkflowsAsync(cancellationToken);

            // 4. Evaluasi Butir Otomatis Kasus Onboarding Berjalan
            var activeCases = await _dbContext.HrdOnboardingCases
                .Where(c => !c.IsDelete && c.CaseStatus == OnboardingCaseStatus.InProgress)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            foreach (var caseId in activeCases)
            {
                try
                {
                    await _caseService.CheckAutomaticItemsAsync(caseId, new System.Security.Claims.ClaimsPrincipal(), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gagal mengevaluasi butir otomatis untuk kasus {CaseId}", caseId);
                }
            }

            // BE-HRD-419: eskalasi keputusan kontrak yang lewat tenggat pemberitahuan tertulis H-7 tanpa artefak bukti.
            // Satu pemberitahuan per keputusan (DedupKey) ke pemegang hak mencatat pemberitahuan.
            var overdueNoticeHorizon = todayDateTime.AddDays(7);
            var overdueNoticeFloor = todayDateTime.AddDays(-30);
            var overdueNotices = await _dbContext.HrdContractDecisions
                .AsNoTracking()
                .Where(d => !d.IsDelete &&
                            d.DecisionStatus != ContractDecisionStatus.Rejected &&
                            d.DecisionStatus != ContractDecisionStatus.Withdrawn &&
                            d.NoticeEvidencePath == null &&
                            d.ContractHistory != null &&
                            d.ContractHistory.EndDate != null &&
                            d.ContractHistory.EndDate < overdueNoticeHorizon &&
                            d.ContractHistory.EndDate >= overdueNoticeFloor)
                .Select(d => new { d.Id, d.DecisionNumber, d.ContractHistoryId, EndDate = d.ContractHistory!.EndDate })
                .ToListAsync(cancellationToken);

            foreach (var notice in overdueNotices)
            {
                try
                {
                    await _notificationService.NotifyPermissionHoldersAsync(
                        "WorkforceEmploymentContract",
                        "RecordNotification",
                        new QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs.HrdNotificationRequest(
                            Title: $"Pemberitahuan tertulis H-7 keputusan {notice.DecisionNumber} terlambat",
                            Body: $"Kontrak berakhir {notice.EndDate:dd-MM-yyyy}. Tenggat pemberitahuan tertulis H-7 sudah lewat dan belum ada berkas bukti pemberitahuan. Segera unggah bukti atau tindak lanjuti sesuai UU 27/2026 Pasal 82.",
                            Category: QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs.HrdNotificationCategories.Separation,
                            Priority: QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs.HrdNotificationPriorities.High,
                            DeepLinkUrl: $"/hrd/employment-contracts/{notice.ContractHistoryId}",
                            SourceModule: "Hrd.Lifecycle.ContractNoticeH7",
                            ReferenceType: nameof(HrdContractDecision),
                            ReferenceId: notice.Id,
                            DedupKey: $"HRD_CONTRACT_NOTICE_H7_OVERDUE_{notice.Id}"),
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gagal mengirim eskalasi pemberitahuan H-7 untuk keputusan {DecisionId}", notice.Id);
                }
            }

            _logger.LogInformation("Selesai menjalankan scheduler siklus kepegawaian. Total {Count} entitas dimutakhirkan.", processedCount);
            return processedCount;
        }

        /// <summary>
        /// Menyelaraskan dokumen T15–T18 yang masih Diajukan/Dikembalikan padahal instance-nya sudah berpindah status
        /// (keputusan 16 planning B6). Penangan lifecycle idempoten, jadi pemanggilan berulang aman.
        /// </summary>
        private async Task<int> ReconcileLifecycleWorkflowsAsync(CancellationToken cancellationToken)
        {
            var instanceIds = new System.Collections.Generic.List<Guid>();
            instanceIds.AddRange(await _dbContext.HrdContractDecisions.AsNoTracking()
                .Where(d => !d.IsDelete && d.WorkflowInstanceId != null &&
                            (d.DecisionStatus == ContractDecisionStatus.Submitted || d.DecisionStatus == ContractDecisionStatus.Returned))
                .Select(d => d.WorkflowInstanceId!.Value)
                .ToListAsync(cancellationToken));
            instanceIds.AddRange(await _dbContext.HrdProbationAssessments.AsNoTracking()
                .Where(a => !a.IsDelete && a.WorkflowInstanceId != null &&
                            (a.AssessmentStatus == ProbationAssessmentStatus.Submitted || a.AssessmentStatus == ProbationAssessmentStatus.Returned))
                .Select(a => a.WorkflowInstanceId!.Value)
                .ToListAsync(cancellationToken));
            instanceIds.AddRange(await _dbContext.HrdContractCorrections.AsNoTracking()
                .Where(c => !c.IsDelete && c.WorkflowInstanceId != null &&
                            (c.CorrectionStatus == ContractCorrectionStatus.Submitted || c.CorrectionStatus == ContractCorrectionStatus.Returned))
                .Select(c => c.WorkflowInstanceId!.Value)
                .ToListAsync(cancellationToken));
            instanceIds.AddRange(await _dbContext.HrdInitialContractBatches.AsNoTracking()
                .Where(b => !b.IsDelete && b.WorkflowInstanceId != null &&
                            (b.BatchStatus == InitialContractBatchStatus.Submitted || b.BatchStatus == InitialContractBatchStatus.Returned))
                .Select(b => b.WorkflowInstanceId!.Value)
                .ToListAsync(cancellationToken));

            if (instanceIds.Count == 0)
            {
                return 0;
            }

            var candidates = await _dbContext.TrxWorkflowInstances.AsNoTracking()
                .Where(w => instanceIds.Contains(w.Id) && !w.IsDelete)
                .Select(w => w.Id)
                .ToListAsync(cancellationToken);

            if (candidates.Count == 0)
            {
                return 0;
            }

            var lifecycle = _serviceProvider.GetRequiredService<WorkflowReferenceLifecycleService>();
            var changed = 0;
            foreach (var instanceId in candidates)
            {
                try
                {
                    var result = await lifecycle.SynchronizeAsync(instanceId, Guid.Empty, allowAutoApply: true, cancellationToken: cancellationToken);
                    if (result.StatusChanged)
                    {
                        changed++;
                        _logger.LogInformation(
                            "Dokumen {ReferenceId} diselaraskan dari {Previous} ke {Current} mengikuti workflow {InstanceId}.",
                            result.ReferenceId, result.PreviousReferenceStatus, result.CurrentReferenceStatus, instanceId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Penyelarasan dokumen lifecycle untuk workflow {InstanceId} gagal.", instanceId);
                }
            }

            return changed;
        }
    }
}
