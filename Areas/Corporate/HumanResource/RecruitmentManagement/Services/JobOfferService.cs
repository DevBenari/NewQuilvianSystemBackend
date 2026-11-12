using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class JobOfferService : IJobOfferService
    {
        private readonly ApplicationDbContext _dbContext;

        public JobOfferService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RecruitmentResult<PagedResult<JobOfferListItemResponse>>> GetPagedAsync(
            JobOfferQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdJobOffers
                .AsNoTracking()
                .Include(o => o.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(o => o.ContractType)
                .Where(o => !o.IsDelete);

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(o => o.Status == query.Status.Value);
            }

            if (query.ApprovalTier.HasValue)
            {
                dbQuery = dbQuery.Where(o => o.ApprovalTier == query.ApprovalTier.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(o =>
                    o.OfferNumber.ToLower().Contains(term) ||
                    o.OfferedPositionTitle.ToLower().Contains(term) ||
                    (o.Application != null && o.Application.Candidate != null && o.Application.Candidate.FullName.ToLower().Contains(term)));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);
            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderByDescending(o => o.OfferDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new JobOfferListItemResponse
                {
                    Id = o.Id,
                    OfferNumber = o.OfferNumber,
                    ApplicationNumber = o.Application != null ? o.Application.ApplicationNumber : string.Empty,
                    CandidateName = o.Application != null && o.Application.Candidate != null ? o.Application.Candidate.FullName : string.Empty,
                    OfferedPositionTitle = o.OfferedPositionTitle,
                    ContractTypeId = o.ContractTypeId,
                    ContractTypeName = o.ContractType != null ? o.ContractType.ContractTypeName : string.Empty,
                    ContractDurationMonths = o.ContractDurationMonths,
                    OfferedBasicSalary = o.OfferedBasicSalary,
                    ApprovalTier = o.ApprovalTier,
                    Status = o.Status,
                    OfferDate = o.OfferDate,
                    ExpiryDate = o.ExpiryDate
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<JobOfferListItemResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<JobOfferListItemResponse>>.Ok(result, "Daftar penawaran kerja berhasil diambil.");
        }

        public async Task<RecruitmentResult<JobOfferResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var o = await _dbContext.HrdJobOffers
                .AsNoTracking()
                .Include(x => x.Application)
                    .ThenInclude(a => a!.Candidate)
                .Include(x => x.SalaryGrade)
                .Include(x => x.ContractType)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (o == null)
            {
                return RecruitmentResult<JobOfferResponse>.NotFound("Surat penawaran kerja tidak ditemukan.");
            }

            var response = MapToResponse(o);
            return RecruitmentResult<JobOfferResponse>.Ok(response, "Detail surat penawaran kerja berhasil diambil.");
        }

        public async Task<RecruitmentResult<JobOfferResponse>> CreateAsync(
            CreateJobOfferRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var application = await _dbContext.HrdCandidateApplications
                .Include(a => a.Candidate)
                .Include(a => a.Vacancy)
                .FirstOrDefaultAsync(a => a.Id == request.ApplicationId && !a.IsDelete, cancellationToken);

            if (application == null)
            {
                return RecruitmentResult<JobOfferResponse>.NotFound("Lamaran tidak ditemukan.");
            }

            // Aturan Bisnis VR-REC-13: Hanya kandidat yang lolos evaluasi (tanpa veto klinis) yang dapat menerima penawaran
            if (application.StageStatus == "ASSESSMENT_FAILED" || application.StageStatus == "REJECTED_ADMINISTRATION")
            {
                return RecruitmentResult<JobOfferResponse>.Unprocessable(
                    "Pembuatan penawaran ditolak: Kandidat telah dinyatakan gugur seleksi atau terkena veto klinis (VR-REC-13).");
            }

            var hasClinicalVeto = await _dbContext.HrdCandidateEvaluations
                .AnyAsync(e => e.ApplicationId == request.ApplicationId && e.IsClinicalVetoApplied && !e.IsDelete, cancellationToken);

            if (hasClinicalVeto)
            {
                return RecruitmentResult<JobOfferResponse>.Unprocessable(
                    "Pembuatan penawaran ditolak secara permanen karena adanya Hak Veto Klinis Mutlak dari Asesor Medis (VR-REC-11, HRD-DEC-077).");
            }

            // Cek apakah sudah ada offer aktif untuk lamaran ini
            var existingOffer = await _dbContext.HrdJobOffers
                .FirstOrDefaultAsync(o => o.ApplicationId == request.ApplicationId &&
                    o.Status != JobOfferStatus.RejectedByApprover &&
                    o.Status != JobOfferStatus.ConditionalRejected &&
                    o.Status != JobOfferStatus.FinalRejected &&
                    o.Status != JobOfferStatus.Revoked &&
                    o.Status != JobOfferStatus.Expired &&
                    !o.IsDelete, cancellationToken);

            if (existingOffer != null)
            {
                return RecruitmentResult<JobOfferResponse>.Conflict("Sudah ada penawaran kerja aktif untuk lamaran ini.");
            }

            // Validasi Kontrak VR-ONB-50 & VR-ONB-51
            var contractType = await _dbContext.MstContractTypes
                .FirstOrDefaultAsync(c => c.Id == request.ContractTypeId && !c.IsDelete, cancellationToken);

            if (contractType == null)
            {
                return RecruitmentResult<JobOfferResponse>.NotFound("Master Jenis Kontrak (Contract Type) tidak ditemukan (VR-ONB-50).");
            }

            if (contractType.RequiresEndDate)
            {
                if (!request.ContractDurationMonths.HasValue || request.ContractDurationMonths.Value < 1 || request.ContractDurationMonths.Value > 60)
                {
                    return RecruitmentResult<JobOfferResponse>.Unprocessable("Tawaran kerja dengan jenis kontrak berkala (PKWT) wajib mengisi lama kontrak antara 1 sampai 60 bulan (VR-ONB-51).");
                }
            }
            else
            {
                if (request.ContractDurationMonths.HasValue && request.ContractDurationMonths.Value > 0)
                {
                    return RecruitmentResult<JobOfferResponse>.Unprocessable("Tawaran kerja dengan jenis kontrak waktu tidak tertentu (PKWTT) tidak boleh mengisi lama kontrak (VR-ONB-51).");
                }
            }

            // Ambil master skala upah untuk evaluasi plafon gaji
            var salaryGrade = await _dbContext.MstSalaryGrades
                .FirstOrDefaultAsync(g => g.Id == request.SalaryGradeId && !g.IsDelete, cancellationToken);

            if (salaryGrade == null)
            {
                return RecruitmentResult<JobOfferResponse>.NotFound("Master Skala Upah (Salary Grade) tidak ditemukan.");
            }

            // Aturan Bisnis VR-REC-14, HRD-DEC-079: Evaluasi Plafon Upah & Penentuan Tier Persetujuan
            var maximumSalary = salaryGrade.MaximumSalary;
            var approvalTier = request.OfferedBasicSalary > maximumSalary
                ? JobOfferApprovalTier.Tier14Exceptional
                : JobOfferApprovalTier.Tier13Standard;

            // Auto-numbering OfferNumber
            var todayPrefix = $"OFR-{DateTime.UtcNow:yyyyMMdd}";
            var countToday = await _dbContext.HrdJobOffers
                .CountAsync(o => o.OfferNumber.StartsWith(todayPrefix), cancellationToken);
            var offerNumber = $"{todayPrefix}-{(countToday + 1):D4}";

            var offer = new HrdJobOffer
            {
                Id = Guid.NewGuid(),
                OfferNumber = offerNumber,
                ApplicationId = request.ApplicationId,
                SalaryGradeId = request.SalaryGradeId,
                ContractTypeId = request.ContractTypeId,
                ContractDurationMonths = request.ContractDurationMonths,
                OfferedBasicSalary = request.OfferedBasicSalary,
                MaximumGradeSalary = maximumSalary,
                ApprovalTier = approvalTier,
                OfferedPositionTitle = request.OfferedPositionTitle.Trim(),
                ProposedJoinDate = request.ProposedJoinDate,
                OfferDate = DateTime.UtcNow,
                ExpiryDate = request.ExpiryDate > DateTime.UtcNow ? request.ExpiryDate : DateTime.UtcNow.AddDays(7),
                Status = JobOfferStatus.Draft,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdJobOffers.AddAsync(offer, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(offer.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<JobOfferResponse>> UpdateAsync(
            Guid id,
            UpdateJobOfferRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var offer = await _dbContext.HrdJobOffers
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDelete, cancellationToken);

            if (offer == null)
            {
                return RecruitmentResult<JobOfferResponse>.NotFound("Surat penawaran kerja tidak ditemukan.");
            }

            if (offer.Status != JobOfferStatus.Draft)
            {
                return RecruitmentResult<JobOfferResponse>.Unprocessable("Hanya penawaran berstatus Draft yang dapat diperbarui.");
            }

            if (offer.RowVersion != request.RowVersion)
            {
                return RecruitmentResult<JobOfferResponse>.Conflict("Data penawaran kerja telah diperbarui oleh pengguna lain.");
            }

            // Validasi Kontrak VR-ONB-50 & VR-ONB-51
            var contractType = await _dbContext.MstContractTypes
                .FirstOrDefaultAsync(c => c.Id == request.ContractTypeId && !c.IsDelete, cancellationToken);

            if (contractType == null)
            {
                return RecruitmentResult<JobOfferResponse>.NotFound("Master Jenis Kontrak (Contract Type) tidak ditemukan (VR-ONB-50).");
            }

            if (contractType.RequiresEndDate)
            {
                if (!request.ContractDurationMonths.HasValue || request.ContractDurationMonths.Value < 1 || request.ContractDurationMonths.Value > 60)
                {
                    return RecruitmentResult<JobOfferResponse>.Unprocessable("Tawaran kerja dengan jenis kontrak berkala (PKWT) wajib mengisi lama kontrak antara 1 sampai 60 bulan (VR-ONB-51).");
                }
            }
            else
            {
                if (request.ContractDurationMonths.HasValue && request.ContractDurationMonths.Value > 0)
                {
                    return RecruitmentResult<JobOfferResponse>.Unprocessable("Tawaran kerja dengan jenis kontrak waktu tidak tertentu (PKWTT) tidak boleh mengisi lama kontrak (VR-ONB-51).");
                }
            }

            var salaryGrade = await _dbContext.MstSalaryGrades
                .FirstOrDefaultAsync(g => g.Id == request.SalaryGradeId && !g.IsDelete, cancellationToken);

            if (salaryGrade == null)
            {
                return RecruitmentResult<JobOfferResponse>.NotFound("Master Skala Upah (Salary Grade) tidak ditemukan.");
            }

            var maximumSalary = salaryGrade.MaximumSalary;
            var approvalTier = request.OfferedBasicSalary > maximumSalary
                ? JobOfferApprovalTier.Tier14Exceptional
                : JobOfferApprovalTier.Tier13Standard;

            offer.SalaryGradeId = request.SalaryGradeId;
            offer.ContractTypeId = request.ContractTypeId;
            offer.ContractDurationMonths = request.ContractDurationMonths;
            offer.OfferedBasicSalary = request.OfferedBasicSalary;
            offer.MaximumGradeSalary = maximumSalary;
            offer.ApprovalTier = approvalTier;
            offer.OfferedPositionTitle = request.OfferedPositionTitle.Trim();
            offer.ProposedJoinDate = request.ProposedJoinDate;
            offer.ExpiryDate = request.ExpiryDate;
            offer.RowVersion = Guid.NewGuid();
            offer.UpdateDateTime = DateTime.UtcNow;
            offer.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(offer.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<JobOfferResponse>> SendConditionalOfferAsync(
            Guid id,
            SendConditionalOfferRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.Approvals)
                .Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDelete, cancellationToken);

            if (offer == null)
            {
                return RecruitmentResult<JobOfferResponse>.NotFound("Surat penawaran kerja tidak ditemukan.");
            }

            // Aturan Bisnis VR-REC-15, HRD-DEC-079: Penawaran hanya dapat dikirim jika telah disetujui penuh oleh seluruh penyetuju
            if (offer.Status != JobOfferStatus.Approved)
            {
                return RecruitmentResult<JobOfferResponse>.Unprocessable(
                    $"Penawaran kerja belum dapat dikirimkan kepada kandidat karena status saat ini adalah '{offer.Status}'. Surat penawaran wajib disetujui penuh sebelum pengiriman (VR-REC-15).");
            }

            offer.Status = JobOfferStatus.SentConditional;
            if (request.ExpiryDate.HasValue && request.ExpiryDate.Value > DateTime.UtcNow)
            {
                offer.ExpiryDate = request.ExpiryDate.Value;
            }
            offer.RowVersion = Guid.NewGuid();
            offer.UpdateDateTime = DateTime.UtcNow;
            offer.UpdateBy = userId;

            // Perbarui stage pada lamaran
            if (offer.Application != null)
            {
                offer.Application.CurrentStage = ApplicationStage.Offering;
                offer.Application.StageStatus = "OFFER_SENT";
                offer.Application.UpdateDateTime = DateTime.UtcNow;
                offer.Application.UpdateBy = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(offer.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<JobOfferResponse>> CandidateRespondAsync(
            Guid id,
            CandidateOfferResponseRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDelete, cancellationToken);

            if (offer == null)
            {
                return RecruitmentResult<JobOfferResponse>.NotFound("Surat penawaran kerja tidak ditemukan.");
            }

            if (offer.Status != JobOfferStatus.SentConditional && offer.Status != JobOfferStatus.FinalOfferSent && offer.Status != JobOfferStatus.Approved)
            {
                return RecruitmentResult<JobOfferResponse>.Unprocessable(
                    $"Respons kandidat hanya dapat dicatat saat penawaran berstatus terkirim atau disetujui. Status saat ini: '{offer.Status}'.");
            }

            if (DateTime.UtcNow > offer.ExpiryDate)
            {
                offer.Status = JobOfferStatus.Expired;
                offer.RowVersion = Guid.NewGuid();
                offer.UpdateDateTime = DateTime.UtcNow;
                offer.UpdateBy = userId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                return RecruitmentResult<JobOfferResponse>.Unprocessable("Masa berlaku surat penawaran telah kedaluwarsa (Expired). Respons kandidat tidak dapat diterima.");
            }

            if (request.ResponseStatus == JobOfferStatus.ConditionalAccepted || request.ResponseStatus == JobOfferStatus.FinalAccepted)
            {
                offer.Status = request.ResponseStatus;
                if (offer.Application != null)
                {
                    offer.Application.CurrentStage = request.ResponseStatus == JobOfferStatus.ConditionalAccepted
                        ? ApplicationStage.MedicalCheckup
                        : ApplicationStage.Handoff;
                    offer.Application.StageStatus = "OFFER_ACCEPTED";
                    offer.Application.UpdateDateTime = DateTime.UtcNow;
                    offer.Application.UpdateBy = userId;
                }
            }
            else if (request.ResponseStatus == JobOfferStatus.ConditionalRejected || request.ResponseStatus == JobOfferStatus.FinalRejected)
            {
                offer.Status = request.ResponseStatus;
                if (offer.Application != null)
                {
                    offer.Application.StageStatus = "OFFER_DECLINED";
                    offer.Application.UpdateDateTime = DateTime.UtcNow;
                    offer.Application.UpdateBy = userId;
                }
            }
            else
            {
                return RecruitmentResult<JobOfferResponse>.Fail("Nilai status respons tidak valid. Gunakan ConditionalAccepted atau FinalAccepted / Rejected.", 400);
            }

            offer.CandidateResponseDate = DateTime.UtcNow;
            offer.CandidateResponseNotes = request.Notes;
            offer.RowVersion = Guid.NewGuid();
            offer.UpdateDateTime = DateTime.UtcNow;
            offer.UpdateBy = userId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(offer.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<JobOfferResponse>> RevokeAsync(
            Guid id,
            string? reason,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDelete, cancellationToken);

            if (offer == null)
            {
                return RecruitmentResult<JobOfferResponse>.NotFound("Surat penawaran kerja tidak ditemukan.");
            }

            if (offer.Status == JobOfferStatus.FinalAccepted || offer.Status == JobOfferStatus.Revoked)
            {
                return RecruitmentResult<JobOfferResponse>.Unprocessable($"Penawaran tidak dapat ditarik kembali pada status '{offer.Status}'.");
            }

            offer.Status = JobOfferStatus.Revoked;
            offer.CandidateResponseNotes = $"[PENARIKAN RESMI] {reason}";
            offer.RowVersion = Guid.NewGuid();
            offer.UpdateDateTime = DateTime.UtcNow;
            offer.UpdateBy = userId;

            if (offer.Application != null)
            {
                offer.Application.StageStatus = "OFFER_REVOKED";
                offer.Application.UpdateDateTime = DateTime.UtcNow;
                offer.Application.UpdateBy = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(offer.Id, cancellationToken);
        }

        private static JobOfferResponse MapToResponse(HrdJobOffer o)
        {
            return new JobOfferResponse
            {
                Id = o.Id,
                OfferNumber = o.OfferNumber,
                ApplicationId = o.ApplicationId,
                ApplicationNumber = o.Application?.ApplicationNumber ?? string.Empty,
                CandidateId = o.Application?.CandidateId ?? Guid.Empty,
                CandidateCode = o.Application?.Candidate?.CandidateCode ?? string.Empty,
                CandidateName = o.Application?.Candidate?.FullName ?? string.Empty,
                SalaryGradeId = o.SalaryGradeId,
                SalaryGradeCode = o.SalaryGrade?.SalaryGradeCode ?? string.Empty,
                SalaryGradeName = o.SalaryGrade?.SalaryGradeName ?? string.Empty,
                ContractTypeId = o.ContractTypeId,
                ContractTypeName = o.ContractType?.ContractTypeName ?? string.Empty,
                ContractDurationMonths = o.ContractDurationMonths,
                OfferedBasicSalary = o.OfferedBasicSalary,
                MaximumGradeSalary = o.MaximumGradeSalary,
                ApprovalTier = o.ApprovalTier,
                OfferedPositionTitle = o.OfferedPositionTitle,
                ProposedJoinDate = o.ProposedJoinDate,
                OfferDate = o.OfferDate,
                ExpiryDate = o.ExpiryDate,
                Status = o.Status,
                CandidateResponseDate = o.CandidateResponseDate,
                CandidateResponseNotes = o.CandidateResponseNotes,
                RowVersion = o.RowVersion,
                CreateDateTime = o.CreateDateTime,
                UpdateDateTime = o.UpdateDateTime
            };
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }
    }
}
