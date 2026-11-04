using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services
{
    public class HrdProbationService : IHrdProbationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEmployeeEmploymentRecordService _employmentRecordService;
        private readonly IHrdNumberSeriesService _numberSeriesService;
        private readonly IHrdLifecycleAttachmentService _attachmentService;
        private readonly IHrdEmploymentLifecycleWorkflowIntegrationService _workflowIntegrationService;
        private readonly ResignationLifecycleHandoffService _resignationHandoffService;

        public HrdProbationService(
            ApplicationDbContext dbContext,
            IEmployeeEmploymentRecordService employmentRecordService,
            IHrdNumberSeriesService numberSeriesService,
            IHrdLifecycleAttachmentService attachmentService,
            IHrdEmploymentLifecycleWorkflowIntegrationService workflowIntegrationService,
            ResignationLifecycleHandoffService resignationHandoffService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _employmentRecordService = employmentRecordService ?? throw new ArgumentNullException(nameof(employmentRecordService));
            _numberSeriesService = numberSeriesService ?? throw new ArgumentNullException(nameof(numberSeriesService));
            _attachmentService = attachmentService ?? throw new ArgumentNullException(nameof(attachmentService));
            _workflowIntegrationService = workflowIntegrationService ?? throw new ArgumentNullException(nameof(workflowIntegrationService));
            _resignationHandoffService = resignationHandoffService ?? throw new ArgumentNullException(nameof(resignationHandoffService));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        public async Task<HrdProbation?> CreateForContractAsync(
            WfpContractHistory contract,
            Guid hospitalSiteId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (contract == null) return null;

            if (contract.HistoryType != "Initial")
            {
                return null;
            }

            var contractType = await _dbContext.MstContractTypes
                .FirstOrDefaultAsync(c => c.Id == contract.ContractTypeId && !c.IsDelete, cancellationToken);

            if (contractType == null || !contractType.IsProbationApplicable)
            {
                return null;
            }

            var existing = await _dbContext.HrdProbations
                .FirstOrDefaultAsync(p => p.ContractHistoryId == contract.Id && !p.IsDelete, cancellationToken);

            if (existing != null)
            {
                return existing;
            }

            var setting = await _dbContext.HrdEmploymentLifecycleSettings
                .FirstOrDefaultAsync(s => s.HospitalSiteId == hospitalSiteId && !s.IsDelete, cancellationToken);
            var reminderDays = setting?.ProbationReminderDays ?? 14;

            var startDate = DateOnly.FromDateTime(contract.StartDate);
            var endDate = startDate.AddMonths(3).AddDays(-1);
            var reminderFromDate = endDate.AddDays(-reminderDays);

            var probation = new HrdProbation
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = contract.WorkforceProfileId,
                ContractHistoryId = contract.Id,
                StartDate = startDate,
                EndDate = endDate,
                ReminderFromDate = reminderFromDate,
                ProbationStatus = ProbationStatus.Running,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdProbations.AddAsync(probation, cancellationToken);

            contract.ProbationEndDate = endDate.ToDateTime(TimeOnly.MinValue);
            contract.UpdateDateTime = DateTime.UtcNow;
            contract.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _employmentRecordService.SyncFromContractAsync(contract, userId, cancellationToken);

            return probation;
        }

        public async Task<RecruitmentResult<PagedResult<HrdProbationResponse>>> GetPagedAsync(
            HrdProbationQuery query,
            CancellationToken cancellationToken = default)
        {
            query ??= new HrdProbationQuery();
            var dbQuery = _dbContext.HrdProbations
                .AsNoTracking()
                .Include(p => p.WorkforceProfile)
                .Include(p => p.ContractHistory)
                .Include(p => p.Assessments)
                .Where(p => !p.IsDelete);

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.ProbationStatus == query.Status.Value);
            }

            if (query.OrganizationUnitId.HasValue)
            {
                var targetEmployeeIds = _dbContext.Set<WfpOrganizationAssignment>()
                    .Where(o => o.OrganizationUnitId == query.OrganizationUnitId.Value && o.IsPrimary && !o.IsDelete)
                    .Select(o => o.WorkforceProfileId);

                dbQuery = dbQuery.Where(p => targetEmployeeIds.Contains(p.WorkforceProfileId));
            }

            if (query.DueOnly.HasValue && query.DueOnly.Value)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                dbQuery = dbQuery.Where(p => p.ProbationStatus == ProbationStatus.Running &&
                                            p.ReminderFromDate <= today &&
                                            p.EndDate >= today);
            }

            if (query.EndDateFrom.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.EndDate >= query.EndDateFrom.Value);
            }

            if (query.EndDateTo.HasValue)
            {
                dbQuery = dbQuery.Where(p => p.EndDate <= query.EndDateTo.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(p =>
                    (p.WorkforceProfile != null && (p.WorkforceProfile.ProfileCode.ToLower().Contains(term) || p.WorkforceProfile.DisplayName.ToLower().Contains(term))) ||
                    (p.ContractHistory != null && p.ContractHistory.ContractNumber.ToLower().Contains(term)));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderBy(p => p.EndDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new HrdProbationResponse
                {
                    Id = p.Id,
                    WorkforceProfileId = p.WorkforceProfileId,
                    WorkforceCode = p.WorkforceProfile != null ? p.WorkforceProfile.ProfileCode : string.Empty,
                    FullName = p.WorkforceProfile != null ? p.WorkforceProfile.DisplayName : string.Empty,
                    ContractHistoryId = p.ContractHistoryId,
                    ContractNumber = p.ContractHistory != null ? p.ContractHistory.ContractNumber : string.Empty,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    ReminderFromDate = p.ReminderFromDate,
                    ProbationStatus = p.ProbationStatus,
                    ClosedAt = p.ClosedAt,
                    ClosedByUserId = p.ClosedByUserId,
                    EmployeeSeparationId = p.EmployeeSeparationId,
                    RowVersion = p.RowVersion,
                    Assessments = p.Assessments
                        .Where(a => !a.IsDelete)
                        .OrderByDescending(a => a.SubmittedAt)
                        .Select(a => new ProbationAssessmentResponse
                        {
                            Id = a.Id,
                            AssessmentNumber = a.AssessmentNumber,
                            ProbationId = a.ProbationId,
                            Result = a.Result,
                            ReasonText = a.ReasonText,
                            EvidenceFileName = a.EvidenceFileName,
                            AssessmentStatus = a.AssessmentStatus,
                            SubmittedByUserId = a.SubmittedByUserId,
                            SubmittedByUserName = a.SubmittedByUser != null ? a.SubmittedByUser.DisplayName : string.Empty,
                            SubmittedAt = a.SubmittedAt,
                            WorkflowInstanceId = a.WorkflowInstanceId,
                            DecidedByUserId = a.DecidedByUserId,
                            DecidedByUserName = a.DecidedByUser != null ? a.DecidedByUser.DisplayName : null,
                            DecidedAt = a.DecidedAt,
                            DecisionNote = a.DecisionNote,
                            RowVersion = a.RowVersion
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            var paged = new PagedResult<HrdProbationResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<HrdProbationResponse>>.Ok(paged, "Daftar masa percobaan berhasil diambil.");
        }

        public async Task<RecruitmentResult<HrdProbationResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var p = await _dbContext.HrdProbations
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.ContractHistory)
                .Include(x => x.Assessments)
                    .ThenInclude(a => a.SubmittedByUser)
                .Include(x => x.Assessments)
                    .ThenInclude(a => a.DecidedByUser)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (p == null)
            {
                return RecruitmentResult<HrdProbationResponse>.NotFound("Data masa percobaan tidak ditemukan.");
            }

            var response = new HrdProbationResponse
            {
                Id = p.Id,
                WorkforceProfileId = p.WorkforceProfileId,
                WorkforceCode = p.WorkforceProfile?.ProfileCode ?? string.Empty,
                FullName = p.WorkforceProfile?.DisplayName ?? string.Empty,
                ContractHistoryId = p.ContractHistoryId,
                ContractNumber = p.ContractHistory?.ContractNumber ?? string.Empty,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                ReminderFromDate = p.ReminderFromDate,
                ProbationStatus = p.ProbationStatus,
                ClosedAt = p.ClosedAt,
                ClosedByUserId = p.ClosedByUserId,
                EmployeeSeparationId = p.EmployeeSeparationId,
                RowVersion = p.RowVersion,
                Assessments = p.Assessments
                    .Where(a => !a.IsDelete)
                    .OrderByDescending(a => a.SubmittedAt)
                    .Select(a => MapToAssessmentResponse(a, null))
                    .ToList()
            };

            return RecruitmentResult<HrdProbationResponse>.Ok(response, "Detail masa percobaan berhasil diambil.");
        }

        // --- BE-HRD-079: Penilaian Masa Percobaan T15 ---

        public async Task<RecruitmentResult<ProbationAssessmentResponse>> SubmitAssessmentAsync(
            Guid probationId,
            SubmitProbationAssessmentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var actorId = GetUserId(user);

            var probation = await _dbContext.HrdProbations
                .Include(p => p.Assessments)
                .FirstOrDefaultAsync(p => p.Id == probationId && !p.IsDelete, cancellationToken);

            if (probation == null)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.NotFound("Masa percobaan tidak ditemukan.");
            }

            // VR-ONB-55: Pengajuan hanya saat masa percobaan berjalan (Running) dan sebelum tanggal akhir
            if (probation.ProbationStatus != ProbationStatus.Running)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.BadRequest(
                    $"VR-ONB-55: Penilaian hanya dapat diajukan saat masa percobaan berstatus Running. Status saat ini: {probation.ProbationStatus}.");
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (today > probation.EndDate)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.BadRequest(
                    $"VR-ONB-55: Masa percobaan telah berakhir pada {probation.EndDate:dd-MM-yyyy}. Pengajuan penilaian setelah tanggal berakhir ditolak.");
            }

            // Konkurensi
            if (probation.RowVersion != request.ProbationRowVersion)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.Conflict(
                    "Data masa percobaan telah dimutakhirkan oleh proses lain. Silakan muat ulang halaman.");
            }

            // VR-ONB-57: Satu penilaian terbuka per masa percobaan
            var hasOpenAssessment = probation.Assessments.Any(a => !a.IsDelete &&
                (a.AssessmentStatus == ProbationAssessmentStatus.Submitted || a.AssessmentStatus == ProbationAssessmentStatus.Returned));

            if (hasOpenAssessment)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.Conflict(
                    "VR-ONB-57: Masa percobaan ini sudah memiliki penilaian yang masih terbuka atau menunggu keputusan.");
            }

            // VR-ONB-56: Penilaian Tidak Lulus (Fail) wajib bukti dokumen pendukung
            string? storagePath = null;
            string? fileName = null;
            string? contentType = null;

            if (request.Result == ProbationResult.Fail)
            {
                if (request.EvidenceFile == null || request.EvidenceFile.Length == 0)
                {
                    return RecruitmentResult<ProbationAssessmentResponse>.BadRequest(
                        "VR-ONB-56: Penilaian Tidak Lulus wajib melampirkan berkas bukti evaluasi kinerja atau lembar penilaian atasan.");
                }

                var saveResult = await _attachmentService.ValidateAndSaveAttachmentAsync(
                    request.EvidenceFile,
                    "probation-evidence",
                    cancellationToken);

                if (!saveResult.IsSuccess)
                {
                    return RecruitmentResult<ProbationAssessmentResponse>.BadRequest(
                        saveResult.ErrorMessage ?? "Gagal menyimpan berkas bukti penilaian.");
                }

                storagePath = saveResult.StoragePath;
                fileName = saveResult.FileName;
                contentType = saveResult.ContentType;
            }
            else if (request.EvidenceFile != null && request.EvidenceFile.Length > 0)
            {
                var saveResult = await _attachmentService.ValidateAndSaveAttachmentAsync(
                    request.EvidenceFile,
                    "probation-evidence",
                    cancellationToken);

                if (saveResult.IsSuccess)
                {
                    storagePath = saveResult.StoragePath;
                    fileName = saveResult.FileName;
                    contentType = saveResult.ContentType;
                }
            }

            var assessmentNumber = await _numberSeriesService.GenerateAssessmentNumberAsync(cancellationToken);

            var assessment = new HrdProbationAssessment
            {
                Id = Guid.NewGuid(),
                AssessmentNumber = assessmentNumber,
                ProbationId = probationId,
                Result = request.Result,
                ReasonText = request.ReasonText.Trim(),
                EvidenceStoragePath = storagePath,
                EvidenceFileName = fileName,
                EvidenceContentType = contentType,
                AssessmentStatus = ProbationAssessmentStatus.Submitted,
                SubmittedByUserId = actorId,
                SubmittedAt = DateTime.UtcNow,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorId
            };

            probation.ProbationStatus = ProbationStatus.AwaitingDecision;
            probation.RowVersion = Guid.NewGuid();
            probation.UpdateDateTime = DateTime.UtcNow;
            probation.UpdateBy = actorId;

            _dbContext.HrdProbationAssessments.Add(assessment);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Pemicu alur persetujuan T15
            var workflowInstanceId = await _workflowIntegrationService.StartProbationAssessmentWorkflowAsync(
                assessment.Id,
                actorId,
                cancellationToken);

            assessment.WorkflowInstanceId = workflowInstanceId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return RecruitmentResult<ProbationAssessmentResponse>.Created(MapToAssessmentResponse(assessment, user.Identity?.Name));
        }

        public async Task<RecruitmentResult<ProbationAssessmentResponse>> UpdateAssessmentAsync(
            Guid probationId,
            Guid assessmentId,
            UpdateProbationAssessmentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var actorId = GetUserId(user);

            var probation = await _dbContext.HrdProbations
                .FirstOrDefaultAsync(p => p.Id == probationId && !p.IsDelete, cancellationToken);

            if (probation == null)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.NotFound("Masa percobaan tidak ditemukan.");
            }

            var assessment = await _dbContext.HrdProbationAssessments
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.ProbationId == probationId && !a.IsDelete, cancellationToken);

            if (assessment == null)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.NotFound("Penilaian masa percobaan tidak ditemukan.");
            }

            if (assessment.AssessmentStatus != ProbationAssessmentStatus.Returned)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.BadRequest(
                    $"Hanya penilaian berstatus Returned (Minta Revisi) yang dapat diubah. Status saat ini: {assessment.AssessmentStatus}.");
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (today > probation.EndDate)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.BadRequest(
                    $"VR-ONB-55: Masa percobaan telah berakhir pada {probation.EndDate:dd-MM-yyyy}. Pengajuan ulang ditolak.");
            }

            if (assessment.RowVersion != request.RowVersion)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.Conflict(
                    "Data penilaian telah dimutakhirkan oleh proses lain.");
            }

            if (request.Result == ProbationResult.Fail)
            {
                if (request.EvidenceFile == null && string.IsNullOrWhiteSpace(assessment.EvidenceStoragePath))
                {
                    return RecruitmentResult<ProbationAssessmentResponse>.BadRequest(
                        "VR-ONB-56: Penilaian Tidak Lulus wajib melampirkan berkas bukti evaluasi kinerja.");
                }
            }

            if (request.EvidenceFile != null && request.EvidenceFile.Length > 0)
            {
                var saveResult = await _attachmentService.ValidateAndSaveAttachmentAsync(
                    request.EvidenceFile,
                    "probation-evidence",
                    cancellationToken);

                if (!saveResult.IsSuccess)
                {
                    return RecruitmentResult<ProbationAssessmentResponse>.BadRequest(
                        saveResult.ErrorMessage ?? "Gagal menyimpan berkas bukti penilaian baru.");
                }

                assessment.EvidenceStoragePath = saveResult.StoragePath;
                assessment.EvidenceFileName = saveResult.FileName;
                assessment.EvidenceContentType = saveResult.ContentType;
            }

            assessment.Result = request.Result;
            assessment.ReasonText = request.ReasonText.Trim();
            assessment.AssessmentStatus = ProbationAssessmentStatus.Submitted;
            assessment.SubmittedAt = DateTime.UtcNow;
            assessment.SubmittedByUserId = actorId;
            assessment.RowVersion = Guid.NewGuid();
            assessment.UpdateDateTime = DateTime.UtcNow;
            assessment.UpdateBy = actorId;

            probation.ProbationStatus = ProbationStatus.AwaitingDecision;
            probation.RowVersion = Guid.NewGuid();
            probation.UpdateDateTime = DateTime.UtcNow;
            probation.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var wfId = await _workflowIntegrationService.StartProbationAssessmentWorkflowAsync(
                assessment.Id,
                actorId,
                cancellationToken);

            assessment.WorkflowInstanceId = wfId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return RecruitmentResult<ProbationAssessmentResponse>.Ok(MapToAssessmentResponse(assessment, user.Identity?.Name));
        }

        public async Task<RecruitmentResult<ProbationAssessmentResponse>> WithdrawAssessmentAsync(
            Guid probationId,
            Guid assessmentId,
            WithdrawAssessmentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var actorId = GetUserId(user);

            var probation = await _dbContext.HrdProbations
                .FirstOrDefaultAsync(p => p.Id == probationId && !p.IsDelete, cancellationToken);

            if (probation == null)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.NotFound("Masa percobaan tidak ditemukan.");
            }

            var assessment = await _dbContext.HrdProbationAssessments
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.ProbationId == probationId && !a.IsDelete, cancellationToken);

            if (assessment == null)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.NotFound("Penilaian masa percobaan tidak ditemukan.");
            }

            if (assessment.AssessmentStatus != ProbationAssessmentStatus.Submitted &&
                assessment.AssessmentStatus != ProbationAssessmentStatus.Returned)
            {
                return RecruitmentResult<ProbationAssessmentResponse>.BadRequest(
                    $"Penilaian tidak dapat ditarik karena berstatus {assessment.AssessmentStatus}.");
            }

            assessment.AssessmentStatus = ProbationAssessmentStatus.Withdrawn;
            assessment.DecisionNote = request?.Reason;
            assessment.RowVersion = Guid.NewGuid();
            assessment.UpdateDateTime = DateTime.UtcNow;
            assessment.UpdateBy = actorId;

            probation.ProbationStatus = ProbationStatus.Running;
            probation.RowVersion = Guid.NewGuid();
            probation.UpdateDateTime = DateTime.UtcNow;
            probation.UpdateBy = actorId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<ProbationAssessmentResponse>.Ok(MapToAssessmentResponse(assessment, user.Identity?.Name));
        }

        public async Task<(bool Exists, Stream? Stream, string? ContentType, string? FileName)> DownloadEvidenceAsync(
            Guid probationId,
            Guid assessmentId,
            CancellationToken cancellationToken = default)
        {
            var assessment = await _dbContext.HrdProbationAssessments
                .FirstOrDefaultAsync(a => a.Id == assessmentId && a.ProbationId == probationId && !a.IsDelete, cancellationToken);

            if (assessment == null || string.IsNullOrWhiteSpace(assessment.EvidenceStoragePath))
            {
                return (false, null, null, null);
            }

            return await _attachmentService.GetAttachmentStreamAsync(assessment.EvidenceStoragePath, cancellationToken);
        }

        public async Task<RecruitmentResult<bool>> ApplyAssessmentDecisionAsync(
            Guid assessmentId,
            Guid deciderUserId,
            bool isApproved,
            string? decisionNote,
            CancellationToken cancellationToken = default)
        {
            var assessment = await _dbContext.HrdProbationAssessments
                .Include(a => a.Probation)
                .FirstOrDefaultAsync(a => a.Id == assessmentId && !a.IsDelete, cancellationToken);

            if (assessment == null)
            {
                return RecruitmentResult<bool>.NotFound("Penilaian masa percobaan tidak ditemukan.");
            }

            var probation = assessment.Probation;
            if (probation == null)
            {
                return RecruitmentResult<bool>.NotFound("Masa percobaan induk tidak ditemukan.");
            }

            if (isApproved)
            {
                // VR-ONB-58: Persetujuan sesudah tanggal akhir ditolak
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                if (today > probation.EndDate)
                {
                    return RecruitmentResult<bool>.BadRequest(
                        $"VR-ONB-58: Persetujuan penilaian masa percobaan dilarang karena tanggal akhir ({probation.EndDate:dd-MM-yyyy}) telah terlampaui.");
                }

                assessment.AssessmentStatus = ProbationAssessmentStatus.Approved;
                assessment.DecidedByUserId = deciderUserId;
                assessment.DecidedAt = DateTime.UtcNow;
                assessment.DecisionNote = decisionNote;
                assessment.UpdateDateTime = DateTime.UtcNow;
                assessment.UpdateBy = deciderUserId;

                if (assessment.Result == ProbationResult.Pass)
                {
                    probation.ProbationStatus = ProbationStatus.Passed;
                    probation.ClosedAt = DateTime.UtcNow;
                    probation.ClosedByUserId = deciderUserId;
                    probation.RowVersion = Guid.NewGuid();
                    probation.UpdateDateTime = DateTime.UtcNow;
                    probation.UpdateBy = deciderUserId;
                }
                else if (assessment.Result == ProbationResult.Fail)
                {
                    probation.ProbationStatus = ProbationStatus.Failed;
                    probation.ClosedAt = DateTime.UtcNow;
                    probation.ClosedByUserId = deciderUserId;
                    probation.RowVersion = Guid.NewGuid();
                    probation.UpdateDateTime = DateTime.UtcNow;
                    probation.UpdateBy = deciderUserId;

                    // Serah terima pemisahan ke Sub-Modul 20 (BE-HRD-078)
                    var handoff = await _resignationHandoffService.HandoffLifecycleSeparationAsync(
                        probation.WorkforceProfileId,
                        null,
                        "ProbationFailed",
                        probation.EndDate,
                        assessment.ReasonText,
                        decisionNote,
                        deciderUserId,
                        cancellationToken);

                    if (handoff.Success && handoff.Data != null)
                    {
                        probation.EmployeeSeparationId = handoff.Data.EmployeeSeparationId;
                    }
                }
            }
            else
            {
                assessment.AssessmentStatus = ProbationAssessmentStatus.Rejected;
                assessment.DecidedByUserId = deciderUserId;
                assessment.DecidedAt = DateTime.UtcNow;
                assessment.DecisionNote = decisionNote;
                assessment.UpdateDateTime = DateTime.UtcNow;
                assessment.UpdateBy = deciderUserId;

                probation.ProbationStatus = ProbationStatus.Running;
                probation.RowVersion = Guid.NewGuid();
                probation.UpdateDateTime = DateTime.UtcNow;
                probation.UpdateBy = deciderUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<bool>.Ok(true, "Penerapan keputusan penilaian masa percobaan berhasil disimpan.");
        }

        private static ProbationAssessmentResponse MapToAssessmentResponse(HrdProbationAssessment a)
        {
            return MapToAssessmentResponse(a, null);
        }

        private static ProbationAssessmentResponse MapToAssessmentResponse(
            HrdProbationAssessment a,
            string? submitterName)
        {
            return new ProbationAssessmentResponse
            {
                Id = a.Id,
                AssessmentNumber = a.AssessmentNumber,
                ProbationId = a.ProbationId,
                Result = a.Result,
                ReasonText = a.ReasonText,
                EvidenceFileName = a.EvidenceFileName,
                AssessmentStatus = a.AssessmentStatus,
                SubmittedByUserId = a.SubmittedByUserId,
                SubmittedByUserName = a.SubmittedByUser?.DisplayName ?? submitterName ?? string.Empty,
                SubmittedAt = a.SubmittedAt,
                WorkflowInstanceId = a.WorkflowInstanceId,
                DecidedByUserId = a.DecidedByUserId,
                DecidedByUserName = a.DecidedByUser?.DisplayName,
                DecidedAt = a.DecidedAt,
                DecisionNote = a.DecisionNote,
                RowVersion = a.RowVersion
            };
        }
    }
}
