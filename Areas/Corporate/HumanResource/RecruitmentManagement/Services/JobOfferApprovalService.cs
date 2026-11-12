using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class JobOfferApprovalService : IJobOfferApprovalService
    {
        private readonly ApplicationDbContext _dbContext;

        public JobOfferApprovalService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        public async Task<RecruitmentResult<JobOfferApprovalHistoryResponse>> GetApprovalsByOfferIdAsync(
            Guid offerId,
            CancellationToken cancellationToken = default)
        {
            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(o => o.Application)
                    .ThenInclude(a => a!.Vacancy)
                .Include(o => o.Approvals)
                .FirstOrDefaultAsync(o => o.Id == offerId && !o.IsDelete, cancellationToken);

            if (offer == null)
            {
                return RecruitmentResult<JobOfferApprovalHistoryResponse>.NotFound("Surat penawaran kerja tidak ditemukan.");
            }

            // Pastikan template langkah persetujuan ada jika belum terbentuk
            if (!offer.Approvals.Any())
            {
                await InitializeApprovalStepsAsync(offer, cancellationToken);
            }

            var response = new JobOfferApprovalHistoryResponse
            {
                OfferId = offer.Id,
                OfferNumber = offer.OfferNumber,
                CandidateName = offer.Application?.Candidate?.FullName ?? "-",
                VacancyTitle = offer.Application?.Vacancy?.Title ?? "-",
                ApprovalTier = offer.ApprovalTier,
                CurrentOfferStatus = offer.Status,
                Approvals = offer.Approvals
                    .Where(a => !a.IsDelete)
                    .OrderBy(a => a.StepNumber)
                    .Select(MapToResponse)
                    .ToList()
            };

            return RecruitmentResult<JobOfferApprovalHistoryResponse>.Ok(response, "Riwayat langkah persetujuan berhasil diambil.");
        }

        public async Task<RecruitmentResult<JobOfferApprovalResponse>> ApproveOfferStepAsync(
            Guid offerId,
            ApproveJobOfferRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);
            var userName = user.Identity?.Name ?? "Approver";

            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.Approvals)
                .Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.Id == offerId && !o.IsDelete, cancellationToken);

            if (offer == null)
            {
                return RecruitmentResult<JobOfferApprovalResponse>.NotFound("Surat penawaran kerja tidak ditemukan.");
            }

            // Aturan Bisnis VR-REC-16: Self-Approval Forbidden
            if (offer.CreateBy == userId && userId != Guid.Empty)
            {
                return RecruitmentResult<JobOfferApprovalResponse>.Fail(
                    "Pembuat draf surat penawaran kerja dilarang menyetujui surat penawarannya sendiri (VR-REC-16: Self-Approval Forbidden).", 403);
            }

            if (!offer.Approvals.Any())
            {
                await InitializeApprovalStepsAsync(offer, cancellationToken);
            }

            // Cari langkah persetujuan aktif berikutnya yang berstatus Pending
            var currentStep = offer.Approvals
                .Where(a => !a.IsDelete && a.Status == ApprovalStatus.Pending)
                .OrderBy(a => a.StepNumber)
                .FirstOrDefault();

            if (currentStep == null)
            {
                return RecruitmentResult<JobOfferApprovalResponse>.Unprocessable(
                    "Tidak ada langkah persetujuan aktif yang berstatus Pending untuk penawaran ini.");
            }

            currentStep.Status = ApprovalStatus.Approved;
            currentStep.ApproverUserId = userId;
            currentStep.ApproverName = userName;
            currentStep.DecisionDate = DateTime.UtcNow;
            currentStep.Notes = request.Notes?.Trim();
            currentStep.UpdateDateTime = DateTime.UtcNow;
            currentStep.UpdateBy = userId;

            // Cek apakah masih ada langkah persetujuan berikutnya
            var hasMorePendingSteps = offer.Approvals
                .Any(a => !a.IsDelete && a.Status == ApprovalStatus.Pending && a.StepNumber > currentStep.StepNumber);

            if (!hasMorePendingSteps)
            {
                // Seluruh rantai persetujuan (baik T13 maupun T14) telah selesai dan disahkan
                offer.Status = JobOfferStatus.Approved;
                offer.RowVersion = Guid.NewGuid();
                offer.UpdateDateTime = DateTime.UtcNow;
                offer.UpdateBy = userId;

                if (offer.Application != null)
                {
                    offer.Application.StageStatus = "OfferApprovedReadyToSend";
                    offer.Application.UpdateDateTime = DateTime.UtcNow;
                    offer.Application.UpdateBy = userId;
                }
            }
            else
            {
                offer.Status = JobOfferStatus.PendingApproval;
                offer.RowVersion = Guid.NewGuid();
                offer.UpdateDateTime = DateTime.UtcNow;
                offer.UpdateBy = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<JobOfferApprovalResponse>.Ok(MapToResponse(currentStep), "Langkah persetujuan penawaran berhasil disetujui.");
        }

        public async Task<RecruitmentResult<JobOfferApprovalResponse>> RejectOfferStepAsync(
            Guid offerId,
            RejectJobOfferRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);
            var userName = user.Identity?.Name ?? "Approver";

            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.Approvals)
                .Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.Id == offerId && !o.IsDelete, cancellationToken);

            if (offer == null)
            {
                return RecruitmentResult<JobOfferApprovalResponse>.NotFound("Surat penawaran kerja tidak ditemukan.");
            }

            // Aturan Bisnis VR-REC-16: Self-Approval Forbidden
            if (offer.CreateBy == userId && userId != Guid.Empty)
            {
                return RecruitmentResult<JobOfferApprovalResponse>.Fail(
                    "Pembuat draf surat penawaran kerja dilarang mengambil keputusan persetujuan pada surat penawarannya sendiri (VR-REC-16).", 403);
            }

            if (!offer.Approvals.Any())
            {
                await InitializeApprovalStepsAsync(offer, cancellationToken);
            }

            var currentStep = offer.Approvals
                .Where(a => !a.IsDelete && a.Status == ApprovalStatus.Pending)
                .OrderBy(a => a.StepNumber)
                .FirstOrDefault();

            if (currentStep == null)
            {
                return RecruitmentResult<JobOfferApprovalResponse>.Unprocessable(
                    "Tidak ada langkah persetujuan aktif yang berstatus Pending untuk ditolak.");
            }

            currentStep.Status = ApprovalStatus.Rejected;
            currentStep.ApproverUserId = userId;
            currentStep.ApproverName = userName;
            currentStep.DecisionDate = DateTime.UtcNow;
            currentStep.Notes = request.Reason.Trim();
            currentStep.UpdateDateTime = DateTime.UtcNow;
            currentStep.UpdateBy = userId;

            // Penolakan satu langkah persetujuan mengembalikan status offer ke RejectedByApprover
            offer.Status = JobOfferStatus.RejectedByApprover;
            offer.RowVersion = Guid.NewGuid();
            offer.UpdateDateTime = DateTime.UtcNow;
            offer.UpdateBy = userId;

            if (offer.Application != null)
            {
                offer.Application.StageStatus = "OfferApprovalRejected";
                offer.Application.Notes = $"Penawaran ditolak oleh {currentStep.ApproverRole}: {request.Reason.Trim()}";
                offer.Application.UpdateDateTime = DateTime.UtcNow;
                offer.Application.UpdateBy = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RecruitmentResult<JobOfferApprovalResponse>.Ok(MapToResponse(currentStep), "Langkah persetujuan penawaran ditolak.");
        }

        private async Task InitializeApprovalStepsAsync(HrdJobOffer offer, CancellationToken cancellationToken)
        {
            var steps = new List<HrdJobOfferApproval>();

            if (offer.ApprovalTier == JobOfferApprovalTier.Tier14Exceptional)
            {
                // Jalur Eskalasi T14: Unit Head, HR Manager, Finance Director, Direktur Utama (VR-REC-15)
                steps.Add(CreateStep(offer.Id, 1, "Kepala Unit Kerja (Unit Head)"));
                steps.Add(CreateStep(offer.Id, 2, "Manajer SDM / HR Manager"));
                steps.Add(CreateStep(offer.Id, 3, "Direktur Keuangan (Finance Director)"));
                steps.Add(CreateStep(offer.Id, 4, "Direktur Utama (President Director)"));
            }
            else
            {
                // Jalur Standar T13: Unit Head, HR Manager
                steps.Add(CreateStep(offer.Id, 1, "Kepala Unit Kerja (Unit Head)"));
                steps.Add(CreateStep(offer.Id, 2, "Manajer SDM / HR Manager"));
            }

            await _dbContext.HrdJobOfferApprovals.AddRangeAsync(steps, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var s in steps)
            {
                offer.Approvals.Add(s);
            }
        }

        private static HrdJobOfferApproval CreateStep(Guid offerId, int stepNumber, string role)
        {
            return new HrdJobOfferApproval
            {
                Id = Guid.NewGuid(),
                OfferId = offerId,
                StepNumber = stepNumber,
                ApproverRole = role,
                Status = ApprovalStatus.Pending,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Guid.Empty
            };
        }

        private static JobOfferApprovalResponse MapToResponse(HrdJobOfferApproval model)
        {
            return new JobOfferApprovalResponse
            {
                Id = model.Id,
                OfferId = model.OfferId,
                StepNumber = model.StepNumber,
                ApproverRole = model.ApproverRole,
                ApproverUserId = model.ApproverUserId,
                ApproverName = model.ApproverName,
                Status = model.Status,
                DecisionDate = model.DecisionDate,
                Notes = model.Notes,
                CreateDateTime = model.CreateDateTime
            };
        }
    }
}
