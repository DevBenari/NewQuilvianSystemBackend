using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class McuClearanceService : IMcuClearanceService
    {
        private readonly ApplicationDbContext _dbContext;

        public McuClearanceService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }

        private static bool IsAuthorizedMedicalStaff(ClaimsPrincipal user)
        {
            return user.IsInRole("Doctor")
                || user.IsInRole("DokterK3RS")
                || user.IsInRole("K3RS")
                || user.HasClaim("Permission", "RecruitmentMcu:DoctorWrite")
                || user.HasClaim("Permission", "RecruitmentMcu:DoctorRead");
        }

        public async Task<RecruitmentResult<PagedResult<McuClearanceListItemResponse>>> GetPagedAsync(
            McuClearanceQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdPreEmploymentMedicalChecks
                .Include(m => m.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Candidate)
                .Include(m => m.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Vacancy)
                .Where(m => !m.IsDelete)
                .AsQueryable();

            if (query.VacancyId.HasValue)
            {
                dbQuery = dbQuery.Where(m => m.Offer != null && m.Offer.Application != null && m.Offer.Application.VacancyId == query.VacancyId.Value);
            }

            if (query.ClearanceResult.HasValue)
            {
                dbQuery = dbQuery.Where(m => m.ClearanceResult == query.ClearanceResult.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim().ToLower();
                dbQuery = dbQuery.Where(m =>
                    m.ReferralNumber.ToLower().Contains(term) ||
                    m.ClinicOrFacility.ToLower().Contains(term) ||
                    (m.Offer != null && m.Offer.OfferNumber.ToLower().Contains(term)) ||
                    (m.Offer != null && m.Offer.Application != null && m.Offer.Application.Candidate != null &&
                     m.Offer.Application.Candidate.FullName.ToLower().Contains(term)));
            }

            var totalCount = await dbQuery.CountAsync(cancellationToken);
            var items = await dbQuery
                .OrderByDescending(m => m.ReferralDate)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(m => new McuClearanceListItemResponse
                {
                    Id = m.Id,
                    OfferId = m.OfferId,
                    OfferNumber = m.Offer != null ? m.Offer.OfferNumber : "-",
                    CandidateName = m.Offer != null && m.Offer.Application != null && m.Offer.Application.Candidate != null
                        ? m.Offer.Application.Candidate.FullName : "-",
                    VacancyTitle = m.Offer != null && m.Offer.Application != null && m.Offer.Application.Vacancy != null
                        ? m.Offer.Application.Vacancy.Title : "-",
                    ReferralNumber = m.ReferralNumber,
                    ReferralDate = m.ReferralDate,
                    CheckDate = m.CheckDate,
                    ClinicOrFacility = m.ClinicOrFacility,
                    ClearanceResult = m.ClearanceResult,
                    ExaminingDoctorName = m.ExaminingDoctorName
                })
                .ToListAsync(cancellationToken);

            var totalPages = query.PageSize > 0 ? (int)Math.Ceiling((double)totalCount / query.PageSize) : 0;

            var result = new PagedResult<McuClearanceListItemResponse>
            {
                Items = items,
                TotalData = totalCount,
                TotalPage = totalPages,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            return RecruitmentResult<PagedResult<McuClearanceListItemResponse>>.Ok(result, "Daftar rujukan MCU berhasil diambil.");
        }

        public async Task<RecruitmentResult<McuClearanceResponse>> GetByIdAsync(
            Guid id,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var mcu = await _dbContext.HrdPreEmploymentMedicalChecks
                .Include(m => m.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Candidate)
                .Include(m => m.Offer)
                    .ThenInclude(o => o!.Application)
                        .ThenInclude(a => a!.Vacancy)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDelete, cancellationToken);

            if (mcu == null)
            {
                return RecruitmentResult<McuClearanceResponse>.NotFound("Data rujukan pemeriksaan kesehatan (MCU) tidak ditemukan.");
            }

            var isMedical = IsAuthorizedMedicalStaff(user);

            var response = new McuClearanceResponse
            {
                Id = mcu.Id,
                OfferId = mcu.OfferId,
                OfferNumber = mcu.Offer?.OfferNumber ?? "-",
                CandidateId = mcu.Offer?.Application?.CandidateId ?? Guid.Empty,
                CandidateName = mcu.Offer?.Application?.Candidate?.FullName ?? "-",
                CandidateNik = mcu.Offer?.Application?.Candidate?.NationalIdNumber ?? "-",
                VacancyTitle = mcu.Offer?.Application?.Vacancy?.Title ?? "-",
                ReferralNumber = mcu.ReferralNumber,
                ReferralDate = mcu.ReferralDate,
                CheckDate = mcu.CheckDate,
                ClinicOrFacility = mcu.ClinicOrFacility,
                ClearanceResult = mcu.ClearanceResult,
                // Aturan Bisnis HRD-DEC-010 & AT-HRD-D2-09: Masking diagnosa medis klinis untuk non-medis
                DoctorNotes = isMedical
                    ? mcu.DoctorNotes
                    : "[DIAGNOSA MEDIS DIISOLASI - HANYA KESIMPULAN KELAIKAN KERJA DAPAT DIAKSES PIHAK HR/NON-MEDIS]",
                IsDoctorNotesMasked = !isMedical,
                CertificateFilePath = mcu.CertificateFilePath,
                ExaminingDoctorName = mcu.ExaminingDoctorName,
                CreateDateTime = mcu.CreateDateTime
            };

            return RecruitmentResult<McuClearanceResponse>.Ok(response, "Detail hasil MCU pra-kerja berhasil diambil.");
        }

        public async Task<RecruitmentResult<McuClearanceResponse>> RegisterMcuAsync(
            RegisterMcuRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var offer = await _dbContext.HrdJobOffers
                .Include(o => o.Application)
                .FirstOrDefaultAsync(o => o.Id == request.OfferId && !o.IsDelete, cancellationToken);

            if (offer == null)
            {
                return RecruitmentResult<McuClearanceResponse>.NotFound("Surat penawaran kerja tidak ditemukan.");
            }

            // Aturan Bisnis VR-REC-17: Rujukan MCU hanya untuk kandidat yang menerima conditional offer
            if (offer.Status != JobOfferStatus.ConditionalAccepted && offer.Status != JobOfferStatus.SentConditional)
            {
                return RecruitmentResult<McuClearanceResponse>.Unprocessable(
                    "Rujukan MCU pra-kerja hanya dapat diterbitkan untuk kandidat yang telah berstatus menyetujui penawaran bersyarat (VR-REC-17).");
            }

            var existingCheck = await _dbContext.HrdPreEmploymentMedicalChecks
                .FirstOrDefaultAsync(m => m.OfferId == request.OfferId && !m.IsDelete, cancellationToken);

            if (existingCheck != null)
            {
                return RecruitmentResult<McuClearanceResponse>.Conflict("Rujukan MCU pra-kerja sudah pernah diterbitkan untuk penawaran ini.");
            }

            var today = DateTime.UtcNow;
            var todayStr = today.ToString("yyyyMMdd");
            var countToday = await _dbContext.HrdPreEmploymentMedicalChecks
                .CountAsync(m => m.ReferralNumber.StartsWith($"MCU-{todayStr}-"), cancellationToken);

            var referralNumber = $"MCU-{todayStr}-{(countToday + 1):D4}";

            var mcu = new HrdPreEmploymentMedicalCheck
            {
                Id = Guid.NewGuid(),
                OfferId = request.OfferId,
                ReferralNumber = referralNumber,
                ReferralDate = DateTime.UtcNow,
                CheckDate = request.ScheduledCheckDate,
                ClinicOrFacility = request.ClinicOrFacility.Trim(),
                ClearanceResult = McuClearanceResult.Pending,
                DoctorNotes = request.Notes?.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdPreEmploymentMedicalChecks.AddAsync(mcu, cancellationToken);

            if (offer.Application != null)
            {
                offer.Application.CurrentStage = ApplicationStage.MedicalCheckup;
                offer.Application.StageStatus = "McuReferralIssued";
                offer.Application.UpdateDateTime = DateTime.UtcNow;
                offer.Application.UpdateBy = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(mcu.Id, user, cancellationToken);
        }

        public async Task<RecruitmentResult<McuClearanceResponse>> RecordResultAsync(
            Guid id,
            RecordMcuResultRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);

            var mcu = await _dbContext.HrdPreEmploymentMedicalChecks
                .Include(m => m.Offer)
                    .ThenInclude(o => o!.Application)
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDelete, cancellationToken);

            if (mcu == null)
            {
                return RecruitmentResult<McuClearanceResponse>.NotFound("Data rujukan MCU tidak ditemukan.");
            }

            mcu.ClearanceResult = request.ClearanceResult;
            mcu.CheckDate = request.CheckDate;
            mcu.ExaminingDoctorName = request.ExaminingDoctorName.Trim();
            mcu.DoctorNotes = request.DoctorNotes?.Trim();
            mcu.CertificateFilePath = request.CertificateFilePath?.Trim();
            mcu.UpdateDateTime = DateTime.UtcNow;
            mcu.UpdateBy = userId;

            var offer = mcu.Offer;
            var app = offer?.Application;

            // Aturan Bisnis Kriteria 3: Jika UNFIT -> Tawaran kerja otomatis Revoked (Batal Demi Hukum)
            if (request.ClearanceResult == McuClearanceResult.Unfit)
            {
                if (offer != null)
                {
                    offer.Status = JobOfferStatus.Revoked;
                    offer.RowVersion = Guid.NewGuid();
                    offer.UpdateDateTime = DateTime.UtcNow;
                    offer.UpdateBy = userId;
                }

                if (app != null)
                {
                    app.CurrentStage = ApplicationStage.Rejected;
                    app.StageStatus = "MCU_UNFIT_OFFER_REVOKED";
                    app.Notes = "Penawaran kerja dibatalkan otomatis oleh sistem karena hasil pemeriksaan kesehatan pra-kerja dinyatakan Tidak Laik Kerja (UNFIT).";
                    app.UpdateDateTime = DateTime.UtcNow;
                    app.UpdateBy = userId;
                }
            }
            else if (request.ClearanceResult == McuClearanceResult.Fit || request.ClearanceResult == McuClearanceResult.FitWithRestrictions)
            {
                if (offer != null)
                {
                    offer.Status = JobOfferStatus.FinalAccepted;
                    offer.RowVersion = Guid.NewGuid();
                    offer.UpdateDateTime = DateTime.UtcNow;
                    offer.UpdateBy = userId;
                }

                if (app != null)
                {
                    app.StageStatus = "McuFitReadyForFinalOfferOrHandoff";
                    app.Notes = request.ClearanceResult == McuClearanceResult.Fit
                        ? "Pemeriksaan kesehatan pra-kerja lulus (FIT)."
                        : "Pemeriksaan kesehatan pra-kerja lulus dengan catatan pembatasan kerja (FIT WITH RESTRICTIONS).";
                    app.UpdateDateTime = DateTime.UtcNow;
                    app.UpdateBy = userId;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(mcu.Id, user, cancellationToken);
        }
    }
}
