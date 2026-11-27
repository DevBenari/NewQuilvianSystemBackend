using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    /// <summary>
    /// Menyambungkan dokumen T15–T18 ke mesin persetujuan bersama (planning
    /// <c>PLANNING_PERSETUJUAN_T15_T18_KONTRAK_DAN_MASA_KERJA.md</c>, perbaikan B6).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Pembuatan instance.</b> <see cref="WorkflowService.CreateAndSubmitOnBehalfAsync"/> dengan pemohon = pengaju
    /// dan kunci idempoten per dokumen. Mesin membuka transaksinya sendiri, jadi pemanggil tidak boleh membungkus
    /// panggilan ini dalam transaksi lain; bila mesin menolak, pemanggil membatalkan dokumennya (gagal tertutup).
    /// </para>
    /// <para>
    /// <b>Tanpa ketergantungan melingkar.</b> <see cref="WorkflowService"/> dan service domain diambil dari
    /// <see cref="IServiceProvider"/> saat dipakai: WorkflowService → penangan lifecycle → service ini → WorkflowService.
    /// </para>
    /// <para>
    /// <b>Ringkasan untuk penyetuju.</b> <c>RequestContext.summary</c> berisi daftar label–nilai yang ditampilkan
    /// Kotak Masuk; <c>RequestContext.source</c> berisi Id dokumen untuk tautan "Buka dokumen". Gaji tidak pernah ikut.
    /// </para>
    /// </remarks>
    public class HrdEmploymentLifecycleWorkflowIntegrationService : IHrdEmploymentLifecycleWorkflowIntegrationService
    {
        private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");

        private readonly ApplicationDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HrdEmploymentLifecycleWorkflowIntegrationService> _logger;

        public HrdEmploymentLifecycleWorkflowIntegrationService(
            ApplicationDbContext dbContext,
            IServiceProvider serviceProvider,
            ILogger<HrdEmploymentLifecycleWorkflowIntegrationService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<LifecycleWorkflowResult> StartProbationAssessmentWorkflowAsync(
            Guid assessmentId,
            Guid submitterUserId,
            CancellationToken cancellationToken = default)
        {
            var assessment = await _dbContext.HrdProbationAssessments
                .AsNoTracking()
                .Include(a => a.Probation)
                .FirstOrDefaultAsync(a => a.Id == assessmentId && !a.IsDelete, cancellationToken);

            if (assessment?.Probation == null)
            {
                return LifecycleWorkflowResult.Fail(StatusCodes.Status404NotFound, "Penilaian masa percobaan tidak ditemukan.");
            }

            var probation = assessment.Probation;
            var summary = new List<object>
            {
                Line("Pegawai", await DescribeEmployeeAsync(probation.WorkforceProfileId, cancellationToken)),
                Line("Masa percobaan", $"{FormatDate(probation.StartDate)} – {FormatDate(probation.EndDate)}"),
                Line("Hasil usulan", assessment.Result == ProbationResult.Fail ? "Tidak Lulus" : "Lulus"),
                Line("Alasan", assessment.ReasonText),
                Line("Lampiran", string.IsNullOrWhiteSpace(assessment.EvidenceFileName) ? "Tidak ada" : assessment.EvidenceFileName!),
                Line("Batas persetujuan", $"{FormatDate(probation.EndDate)} (sesudah tanggal ini persetujuan ditolak)")
            };

            return await StartAsync(
                EmploymentLifecycleWorkflowCodes.ProbationAssessmentDefinitionCode,
                EmploymentLifecycleWorkflowCodes.ProbationAssessmentReferenceType,
                assessment.Id,
                assessment.AssessmentNumber,
                submitterUserId,
                summary,
                new { probationId = probation.Id, assessmentId = assessment.Id, workforceProfileId = probation.WorkforceProfileId },
                cancellationToken);
        }

        public async Task<LifecycleWorkflowResult> StartContractDecisionWorkflowAsync(
            Guid decisionId,
            Guid submitterUserId,
            CancellationToken cancellationToken = default)
        {
            var decision = await _dbContext.HrdContractDecisions
                .AsNoTracking()
                .Include(d => d.ContractHistory)
                .FirstOrDefaultAsync(d => d.Id == decisionId && !d.IsDelete, cancellationToken);

            if (decision == null)
            {
                return LifecycleWorkflowResult.Fail(StatusCodes.Status404NotFound, "Keputusan kontrak tidak ditemukan.");
            }

            var contract = decision.ContractHistory;
            var summary = new List<object>
            {
                Line("Pegawai", await DescribeEmployeeAsync(decision.WorkforceProfileId, cancellationToken)),
                Line("Kontrak berjalan", contract == null
                    ? "-"
                    : contract.EndDate.HasValue
                        ? $"{contract.ContractNumber} · berakhir {FormatDate(contract.EndDate.Value)}"
                        : $"{contract.ContractNumber} · tanpa tanggal berakhir"),
                Line("Usulan", decision.DecisionType switch
                {
                    ContractDecisionType.Extend => decision.ExtensionMonths.HasValue
                        ? $"Perpanjang {decision.ExtensionMonths} bulan"
                        : "Perpanjang",
                    ContractDecisionType.ConvertToPermanent => "Angkat jadi Pegawai Tetap",
                    ContractDecisionType.NotRenew => "Tidak Diperpanjang",
                    _ => decision.DecisionType.ToString()
                }),
                Line(decision.DecisionType == ContractDecisionType.NotRenew ? "Hari terakhir bekerja" : "Periode baru",
                    decision.DecisionType == ContractDecisionType.NotRenew
                        ? (contract?.EndDate.HasValue == true ? FormatDate(contract.EndDate!.Value) : "-")
                        : decision.ProposedEndDate.HasValue
                            ? $"{FormatDate(decision.ProposedStartDate)} – {FormatDate(decision.ProposedEndDate.Value)}"
                            : $"Mulai {FormatDate(decision.ProposedStartDate)}"),
                Line("Alasan", string.IsNullOrWhiteSpace(decision.ReasonText) ? "-" : decision.ReasonText!)
            };

            return await StartAsync(
                EmploymentLifecycleWorkflowCodes.ContractDecisionDefinitionCode,
                EmploymentLifecycleWorkflowCodes.ContractDecisionReferenceType,
                decision.Id,
                decision.DecisionNumber,
                submitterUserId,
                summary,
                new { contractHistoryId = decision.ContractHistoryId, decisionId = decision.Id, workforceProfileId = decision.WorkforceProfileId },
                cancellationToken);
        }

        public async Task<LifecycleWorkflowResult> StartContractCorrectionWorkflowAsync(
            Guid correctionId,
            Guid submitterUserId,
            CancellationToken cancellationToken = default)
        {
            // Tanpa Include jenis kontrak: relasi wajib menjadi INNER JOIN, sehingga koreksi dengan jenis kontrak yang
            // sudah dihapus tidak akan ditemukan. Nama jenis kontrak dibaca terpisah.
            var correction = await _dbContext.HrdContractCorrections
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == correctionId && !c.IsDelete, cancellationToken);

            if (correction == null)
            {
                return LifecycleWorkflowResult.Fail(StatusCodes.Status404NotFound, "Pengajuan koreksi kontrak tidak ditemukan.");
            }

            var correctedWorkforceProfileId = await _dbContext.WfpContractHistories
                .AsNoTracking()
                .Where(x => x.Id == correction.ContractHistoryId)
                .Select(x => (Guid?)x.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);
            var typeNames = await _dbContext.MstContractTypes
                .AsNoTracking()
                .Where(x => x.Id == correction.BeforeContractTypeId || x.Id == correction.AfterContractTypeId)
                .ToDictionaryAsync(x => x.Id, x => x.ContractTypeName, cancellationToken);

            var summary = new List<object>
            {
                Line("Pegawai", correctedWorkforceProfileId.HasValue
                    ? await DescribeEmployeeAsync(correctedWorkforceProfileId.Value, cancellationToken)
                    : "-"),
                Line("Kontrak", correction.BeforeContractNumber)
            };

            if (!string.Equals(correction.BeforeContractNumber, correction.AfterContractNumber, StringComparison.Ordinal))
            {
                summary.Add(Line("Nomor kontrak", $"{correction.BeforeContractNumber} → {correction.AfterContractNumber}"));
            }

            if (correction.BeforeContractTypeId != correction.AfterContractTypeId)
            {
                summary.Add(Line("Jenis kontrak",
                    $"{typeNames.GetValueOrDefault(correction.BeforeContractTypeId, "-")} → {typeNames.GetValueOrDefault(correction.AfterContractTypeId, "-")}"));
            }

            if (correction.BeforeStartDate != correction.AfterStartDate)
            {
                summary.Add(Line("Tanggal mulai", $"{FormatDate(correction.BeforeStartDate)} → {FormatDate(correction.AfterStartDate)}"));
            }

            if (correction.BeforeEndDate != correction.AfterEndDate)
            {
                summary.Add(Line("Tanggal berakhir", $"{FormatDate(correction.BeforeEndDate)} → {FormatDate(correction.AfterEndDate)}"));
            }

            summary.Add(Line("Alasan koreksi", correction.ReasonText));
            summary.Add(Line("Scan perbaikan", correction.EvidenceFileName));

            return await StartAsync(
                EmploymentLifecycleWorkflowCodes.ContractCorrectionDefinitionCode,
                EmploymentLifecycleWorkflowCodes.ContractCorrectionReferenceType,
                correction.Id,
                correction.CorrectionNumber,
                submitterUserId,
                summary,
                new { contractHistoryId = correction.ContractHistoryId, correctionId = correction.Id },
                cancellationToken);
        }

        public async Task<LifecycleWorkflowResult> StartInitialContractBatchWorkflowAsync(
            Guid batchId,
            Guid submitterUserId,
            CancellationToken cancellationToken = default)
        {
            var batch = await _dbContext.HrdInitialContractBatches
                .AsNoTracking()
                .Include(b => b.HospitalSite)
                .FirstOrDefaultAsync(b => b.Id == batchId && !b.IsDelete, cancellationToken);

            if (batch == null)
            {
                return LifecycleWorkflowResult.Fail(StatusCodes.Status404NotFound, "Batch data awal kontrak tidak ditemukan.");
            }

            var summary = new List<object>
            {
                Line("Batch", batch.BatchNumber),
                Line("Rumah sakit", batch.HospitalSite?.SiteName ?? "-"),
                Line("Jumlah baris", $"{batch.TotalRows} baris ({batch.ValidRows} valid)"),
                Line("Efek bila disetujui", "Setiap baris valid menjadi riwayat kontrak berstatus ditandatangani")
            };

            return await StartAsync(
                EmploymentLifecycleWorkflowCodes.InitialContractBatchDefinitionCode,
                EmploymentLifecycleWorkflowCodes.InitialContractBatchReferenceType,
                batch.Id,
                batch.BatchNumber,
                submitterUserId,
                summary,
                new { batchId = batch.Id },
                cancellationToken);
        }

        public async Task<LifecycleWorkflowResult> ResubmitAsync(
            Guid workflowInstanceId,
            string? comment,
            CancellationToken cancellationToken = default)
        {
            var workflowService = _serviceProvider.GetRequiredService<WorkflowService>();
            var result = await workflowService.SubmitAsync(
                workflowInstanceId,
                new WorkflowSubmitRequest { Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim() },
                cancellationToken);

            if (!result.Success || result.Data == null)
            {
                var message = result.StatusCode == StatusCodes.Status403Forbidden
                    ? "Hanya pengaju yang dapat memperbaiki dan mengajukan ulang dokumen ini."
                    : $"Pengajuan ulang ke alur persetujuan gagal: {result.Message}";
                return LifecycleWorkflowResult.Fail(result.StatusCode, message);
            }

            return LifecycleWorkflowResult.Ok(result.Data.Id, "Dokumen diajukan ulang ke alur persetujuan.");
        }

        public async Task<LifecycleWorkflowResult> WithdrawAsync(
            Guid? workflowInstanceId,
            Guid requesterUserId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            if (!workflowInstanceId.HasValue || workflowInstanceId.Value == Guid.Empty)
            {
                return LifecycleWorkflowResult.Ok(null, "Dokumen belum memiliki alur persetujuan.");
            }

            var instance = await _dbContext.TrxWorkflowInstances
                .AsNoTracking()
                .Where(x => x.Id == workflowInstanceId.Value && !x.IsDelete)
                .Select(x => new { x.Id, x.WorkflowStatus, x.RequestedByUserId })
                .FirstOrDefaultAsync(cancellationToken);

            if (instance == null)
            {
                // Dokumen lama yang dibuat sebelum integrasi hanya menyimpan GUID tanpa instance.
                return LifecycleWorkflowResult.Ok(null, "Alur persetujuan dokumen tidak ditemukan; hanya dokumen yang ditarik.");
            }

            var outcome = EmploymentLifecycleWorkflowCodes.MapOutcome(instance.WorkflowStatus);
            if (outcome is EmploymentLifecycleWorkflowCodes.Outcome.Withdrawn
                or EmploymentLifecycleWorkflowCodes.Outcome.Rejected
                or EmploymentLifecycleWorkflowCodes.Outcome.Approved)
            {
                return LifecycleWorkflowResult.Ok(instance.Id, "Alur persetujuan sudah selesai.");
            }

            if (instance.RequestedByUserId != requesterUserId)
            {
                return LifecycleWorkflowResult.Fail(
                    StatusCodes.Status403Forbidden,
                    "Hanya pengaju yang dapat menarik dokumen ini dari alur persetujuan.");
            }

            var workflowService = _serviceProvider.GetRequiredService<WorkflowService>();
            var result = await workflowService.WithdrawOnBehalfAsync(
                instance.Id,
                requesterUserId,
                new WorkflowWithdrawRequest
                {
                    Reason = string.IsNullOrWhiteSpace(reason) ? "Ditarik oleh pengaju." : reason.Trim()
                },
                cancellationToken);

            if (!result.Success)
            {
                return LifecycleWorkflowResult.Fail(result.StatusCode, $"Penarikan dari alur persetujuan gagal: {result.Message}");
            }

            return LifecycleWorkflowResult.Ok(instance.Id, "Dokumen ditarik dari alur persetujuan.");
        }

        public async Task<bool> InstanceExistsAsync(Guid? workflowInstanceId, CancellationToken cancellationToken = default)
        {
            if (!workflowInstanceId.HasValue || workflowInstanceId.Value == Guid.Empty)
            {
                return false;
            }

            return await _dbContext.TrxWorkflowInstances
                .AsNoTracking()
                .AnyAsync(x => x.Id == workflowInstanceId.Value && !x.IsDelete, cancellationToken);
        }

        public async Task<bool> HandleWorkflowCompletedAsync(
            Guid workflowInstanceId,
            bool isApproved,
            Guid deciderUserId,
            string? decisionNote,
            CancellationToken cancellationToken = default)
        {
            // T15 — penilaian masa percobaan (maker-checker, VR-ONB-58, dan idempoten di HrdProbationService)
            var assessmentId = await _dbContext.HrdProbationAssessments
                .AsNoTracking()
                .Where(a => a.WorkflowInstanceId == workflowInstanceId && !a.IsDelete)
                .Select(a => (Guid?)a.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (assessmentId.HasValue)
            {
                var result = await _serviceProvider.GetRequiredService<IHrdProbationService>()
                    .ApplyAssessmentDecisionAsync(assessmentId.Value, deciderUserId, isApproved, decisionNote, cancellationToken);
                return LogApplyResult("T15", workflowInstanceId, result.Success, result.Message);
            }

            // T16 — keputusan kontrak (AUD-08-02)
            var decisionId = await _dbContext.HrdContractDecisions
                .AsNoTracking()
                .Where(d => d.WorkflowInstanceId == workflowInstanceId && !d.IsDelete)
                .Select(d => (Guid?)d.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (decisionId.HasValue)
            {
                var result = await _serviceProvider.GetRequiredService<IHrdEmploymentContractService>()
                    .ApplyDecisionAsync(decisionId.Value, deciderUserId, isApproved, decisionNote, cancellationToken);
                return LogApplyResult("T16", workflowInstanceId, result.Success, result.Message);
            }

            // T17 — koreksi kontrak: isi kontrak diganti versi koreksi
            var correctionId = await _dbContext.HrdContractCorrections
                .AsNoTracking()
                .Where(c => c.WorkflowInstanceId == workflowInstanceId && !c.IsDelete)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (correctionId.HasValue)
            {
                var result = await _serviceProvider.GetRequiredService<IHrdEmploymentContractService>()
                    .ApplyCorrectionAsync(correctionId.Value, deciderUserId, isApproved, decisionNote, cancellationToken);
                return LogApplyResult("T17", workflowInstanceId, result.Success, result.Message);
            }

            // T18 — batch data awal: baris valid menjadi riwayat kontrak
            var batchId = await _dbContext.HrdInitialContractBatches
                .AsNoTracking()
                .Where(b => b.WorkflowInstanceId == workflowInstanceId && !b.IsDelete)
                .Select(b => (Guid?)b.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (batchId.HasValue)
            {
                var result = await _serviceProvider.GetRequiredService<IHrdInitialContractBatchService>()
                    .ApplyInitialBatchAsync(batchId.Value, deciderUserId, isApproved, decisionNote, cancellationToken);
                return LogApplyResult("T18", workflowInstanceId, result.Success, result.Message);
            }

            _logger.LogWarning("Instance workflow {WorkflowInstanceId} tidak terhubung ke dokumen T15–T18 mana pun.", workflowInstanceId);
            return false;
        }

        private async Task<LifecycleWorkflowResult> StartAsync(
            string definitionCode,
            string referenceType,
            Guid referenceId,
            string documentNumber,
            Guid submitterUserId,
            List<object> summary,
            object source,
            CancellationToken cancellationToken)
        {
            var workflowService = _serviceProvider.GetRequiredService<WorkflowService>();
            var result = await workflowService.CreateAndSubmitOnBehalfAsync(
                new CreateWorkflowInstanceRequest
                {
                    WorkflowDefinitionCode = definitionCode,
                    ReferenceType = referenceType,
                    ReferenceId = referenceId,
                    ExternalReferenceNumber = documentNumber,
                    SourceChannel = "Web",
                    IdempotencyKey = EmploymentLifecycleWorkflowCodes.IdempotencyKey(definitionCode, referenceId),
                    RequestContext = JsonSerializer.SerializeToElement(new { summary, source }),
                    SelectedApproverUserIds = new List<Guid>()
                },
                submitterUserId,
                cancellationToken);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning(
                    "Alur persetujuan {DefinitionCode} untuk {ReferenceType} {ReferenceId} gagal dimulai: {Message}",
                    definitionCode, referenceType, referenceId, result.Message);

                var statusCode = result.StatusCode == StatusCodes.Status404NotFound || result.StatusCode == StatusCodes.Status400BadRequest
                    ? StatusCodes.Status400BadRequest
                    : result.StatusCode;
                return LifecycleWorkflowResult.Fail(
                    statusCode,
                    $"Alur persetujuan {definitionCode} belum dapat dimulai: {result.Message} Hubungi admin untuk memeriksa Langkah Workflow {definitionCode} (penyetuju aktif wajib ada).");
            }

            _logger.LogInformation(
                "Alur persetujuan {DefinitionCode} dimulai untuk {ReferenceType} {ReferenceId}, instance {WorkflowInstanceId}",
                definitionCode, referenceType, referenceId, result.Data.Id);
            return LifecycleWorkflowResult.Ok(result.Data.Id, "Dokumen diajukan ke alur persetujuan.");
        }

        private bool LogApplyResult(string code, Guid workflowInstanceId, bool success, string? message)
        {
            if (!success)
            {
                _logger.LogWarning(
                    "Penerapan keputusan {Code} dari instance {WorkflowInstanceId} ditolak service domain: {Message}",
                    code, workflowInstanceId, message);
            }

            return success;
        }

        private async Task<string> DescribeEmployeeAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var profile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId)
                .Select(x => new { x.DisplayName, x.ProfileCode })
                .FirstOrDefaultAsync(cancellationToken);

            var now = DateTime.UtcNow;
            var positionName = await (
                    from assignment in _dbContext.WfpOrganizationAssignments.AsNoTracking()
                    join position in _dbContext.MstPositions.AsNoTracking() on assignment.PositionId equals position.Id
                    where assignment.WorkforceProfileId == workforceProfileId &&
                          assignment.IsActive &&
                          !assignment.IsDelete &&
                          !assignment.IsCancel &&
                          assignment.EffectiveStartDate <= now &&
                          (!assignment.EffectiveEndDate.HasValue || assignment.EffectiveEndDate.Value >= now)
                    orderby assignment.IsPrimary descending, assignment.EffectiveStartDate descending
                    select position.PositionName)
                .FirstOrDefaultAsync(cancellationToken);

            var name = profile?.DisplayName ?? "-";
            var parts = new[] { name, positionName, profile?.ProfileCode }
                .Where(x => !string.IsNullOrWhiteSpace(x));
            return string.Join(" · ", parts);
        }

        private static object Line(string label, string value) => new { label, value };

        private static string FormatDate(DateOnly? date) =>
            date.HasValue ? date.Value.ToString("dd MMM yyyy", Indonesian) : "-";

        private static string FormatDate(DateTime date) =>
            date.ToString("dd MMM yyyy", Indonesian);
    }
}
